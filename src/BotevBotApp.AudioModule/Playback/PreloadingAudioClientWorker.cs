using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Requests;
using Discord.Audio;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// An audio client worker that preloads the first N requests in its queue.
    /// </summary>
    internal sealed class PreloadingAudioClientWorker : IDisposable, IAudioClientWorker
    {
        private record PreloadedRequest
        {
            public AudioRequest Request { get; init; }
            public AudioPlayback Playback { get; init; }
        }

        /// <inheritdoc/>
        public event EventHandler<AudioEnqueuedEventArgs> AudioEnqueued;

        /// <inheritdoc/>
        public event EventHandler<AudioSkippedEventArgs> AudioSkipped;

        /// <inheritdoc/>
        public event EventHandler<AudioStartedPlayingEventArgs> AudioStartedPlaying;

        /// <inheritdoc/>
        public event EventHandler<AudioStoppedPlayingEventArgs> AudioStoppedPlaying;

        /// <inheritdoc/>
        public ulong WorkerId { get; init; }


        private AudioItemDTO _currentlyPlaying = null;
        private readonly object _currentlyPlayingLock = new object();

        /// <summary>
        /// Gets the currently playing item.
        /// </summary>
        public AudioItemDTO CurrentlyPlaying
        {
            get
            {
                lock (_currentlyPlayingLock)
                {
                    return _currentlyPlaying;
                }
            }
            private set
            {
                lock (_currentlyPlayingLock)
                {
                    _currentlyPlaying = value;
                }
            }
        }


        private readonly IAudioClient discordAudioClient;
        private readonly ILogger<PreloadingAudioClientWorker> logger;

        /// <summary>
        /// The collection used to store the requests.
        /// </summary>
        /// <remarks>
        /// The collection exists so snapshoots can be taken of the internal collection, via <see cref="ConcurrentQueue{T}.GetEnumerator()"/> and <see cref="ConcurrentQueue{T}.ToArray()"/>.<br/>
        /// Use <see cref="requestQueue"/> for adding and removing elements instead.
        /// </remarks>
        private readonly ConcurrentQueue<AudioRequest> _requestQueueInternal;

        /// <summary>
        /// Async wrapper over <see cref="_requestQueueInternal"/>.
        /// </summary>
        private readonly AsyncCollection<AudioRequest> requestQueue;

        /// <summary>
        /// Controls how many items can be kept preloaded.
        /// </summary>
        private readonly SemaphoreSlim preloadedSemaphore;
        private readonly int preloadedMaxItems;
        private readonly ConcurrentQueue<PreloadedRequest> _preloadedRequestQueueInternal;
        private readonly AsyncCollection<PreloadedRequest> preloadedRequestQueue;

        /// <summary>
        /// Checks if the audio client worker should wait.
        /// </summary>
        private readonly AsyncManualResetEvent runEvent = new(true);

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private int queueLength = 0;
        private bool disposedValue;

        private class SkipSongRequestEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the number of songs that are requested to be skipped.
            /// </summary>
            public int TotalSkips => PreloadedSkips + UnloadedSkips;

            /// <summary>
            /// Gets the number of skips that are requested to be performed in the preloaded queue.
            /// </summary>
            public int PreloadedSkips { get; set; }

            /// <summary>
            /// Get the number of skips that are requested to be performed in the regular queue.
            /// </summary>
            public int UnloadedSkips { get; set; }
        }
        private event EventHandler<SkipSongRequestEventArgs> SkipRequest;
        private event EventHandler<SkipSongRequestEventArgs> SkipCurrentRequest;

        public PreloadingAudioClientWorker(ulong workerId, IAudioClient discordAudioClient, ILogger<PreloadingAudioClientWorker> logger, int keepPreloaded = 2)
        {
            WorkerId = workerId;
            this.discordAudioClient = discordAudioClient;
            this.logger = logger;

            _requestQueueInternal = new();
            requestQueue = new(_requestQueueInternal);

            preloadedMaxItems = keepPreloaded;
            preloadedSemaphore = new(preloadedMaxItems);
            _preloadedRequestQueueInternal = new();
            preloadedRequestQueue = new(_preloadedRequestQueueInternal);

            Task DisconnectCancel(Exception ex) { cancellationTokenSource.Cancel(); return Task.CompletedTask; }
            this.discordAudioClient.Disconnected += DisconnectCancel;

            var token = cancellationTokenSource.Token;
            token.Register(() => { this.discordAudioClient.Disconnected -= DisconnectCancel; });

            SkipRequest += SkipSongsAsync;

            // TODO: Maybe wrap in longrunning task? Profiling needed.
            _ = PreloaderWorkAsync(token);
            _ = AudioWorkAsync(token);
        }

        /// <inheritdoc/>
        public async Task EnqueueAsync(AudioRequest request, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Enqueing request: {request}");
            Interlocked.Increment(ref queueLength);
            try
            {
                await requestQueue.AddAsync(request, cancellationToken).ConfigureAwait(false);
                AudioEnqueued?.Invoke(this, new AudioEnqueuedEventArgs { AudioRequest = request });
            }
            catch (Exception)
            {
                Interlocked.Decrement(ref queueLength);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task SkipAsync(int songsToSkip, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug($"Skipping {songsToSkip} requests.");

            // Decrements the counter to account for the current song being skipped.
            songsToSkip--;

            int currentPreloadedCount = preloadedMaxItems - preloadedSemaphore.CurrentCount;

            int preloadedSkips;
            int unloadedSkips;

            if(songsToSkip <= currentPreloadedCount)
            {
                preloadedSkips = songsToSkip;
                unloadedSkips = 0;
            }
            else
            {
                preloadedSkips = currentPreloadedCount;
                unloadedSkips = songsToSkip - currentPreloadedCount;
            }

            var eventArgs = new SkipSongRequestEventArgs { PreloadedSkips = preloadedSkips, UnloadedSkips = unloadedSkips };

            SkipRequest?.Invoke(this, eventArgs);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug($"Skipping all requests.");

            var eventArgs = new SkipSongRequestEventArgs { PreloadedSkips = int.MaxValue, UnloadedSkips = int.MaxValue };
            SkipRequest?.Invoke(this, eventArgs);

            return Task.CompletedTask;
        }

        private async void SkipSongsAsync(object sender, SkipSongRequestEventArgs eventArgs)
        {
            runEvent.Reset();

            SkipCurrentRequest?.Invoke(this, eventArgs);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                try
                {
                    var ct = cts.Token;
                    for (int i = 0; i < eventArgs.UnloadedSkips; i++)
                    {
                        var request = await requestQueue.TakeAsync(ct).ConfigureAwait(false);
                        AudioSkipped?.Invoke(this, new AudioSkippedEventArgs { AudioRequest = request });
                    }
                }
                catch (OperationCanceledException)
                { }
            }

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                try
                {
                    var ct = cts.Token;
                    for (int i = 0; i < eventArgs.PreloadedSkips; i++)
                    {
                        var preloadedItem = await preloadedRequestQueue.TakeAsync(ct).ConfigureAwait(false);
                        AudioSkipped?.Invoke(this, new AudioSkippedEventArgs { AudioRequest = preloadedItem.Request });
                    }
                }
                catch (OperationCanceledException)
                { }

                if (eventArgs.PreloadedSkips > 0)
                {
                    preloadedSemaphore.Release(eventArgs.PreloadedSkips);
                }
            }

            runEvent.Set();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AudioItemDTO>> GetQueueItemsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Fetching request queue.");
            var current = CurrentlyPlaying;

            var preloadeRequests = _preloadedRequestQueueInternal.ToArray();
            var queueRequests = _requestQueueInternal.ToArray();

            var preloadedItems = (await Task.WhenAll(preloadeRequests.Select(r => r.Request.ToAudioItemAsync(cancellationToken))).ConfigureAwait(false)).AsEnumerable();
            var unloadedItems = (await Task.WhenAll(queueRequests.Select(r => r.ToAudioItemAsync(cancellationToken))).ConfigureAwait(false)).AsEnumerable();

            if (current is not null)
            {
                preloadedItems = preloadedItems.Prepend(current);
            }
            return preloadedItems.Concat(unloadedItems);
        }

        /// <summary>
        /// The preloader worker task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation.</param>
        /// <returns>A task representing the work operation.</returns>
        private async Task PreloaderWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug($"Starting preloader worker...");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await runEvent.WaitAsync(cancellationToken).ConfigureAwait(false);

                    logger.LogTrace("Waiting for available slot in preloaded queue.");
                    await preloadedSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                
                    logger.LogTrace("Waiting for request to preload.");
                    var request = await requestQueue.TakeAsync(cancellationToken).ConfigureAwait(false);

                    logger.LogTrace($"Dequeued request to preload: {request}");
                    Interlocked.Decrement(ref queueLength);

                    logger.LogTrace($"Preloading request: {request}");
                    var preloadedPlayback = (await request.GetAudioPlaybackAsync(cancellationToken).ConfigureAwait(false)).WithPreloading();

                    logger.LogTrace($"Adding request to preloaded queue.");
                    await preloadedRequestQueue.AddAsync(new PreloadedRequest { Request = request, Playback = preloadedPlayback }, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogTrace($"Preloader worker cancellation requested.");
                throw;
            }
            catch (Exception ex) { logger.LogError(ex, $"Preloader worker stopping with exception!"); throw; }
            finally
            {
                logger.LogDebug($"Stopping preloader worker...");
            }
        }

        /// <summary>
        /// The audio worker task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation.</param>
        /// <returns>A task representing the work operation.</returns>
        private async Task AudioWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug($"Starting audio worker...");
            
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogTrace("Creating output stream.");
                using var discordAudioStream = discordAudioClient.CreatePCMStream(AudioApplication.Music);
                while (!cancellationToken.IsCancellationRequested)
                {
                    await runEvent.WaitAsync(cancellationToken).ConfigureAwait(false);

                    logger.LogTrace("Waiting for request.");
                    var preloadedItem = await preloadedRequestQueue.TakeAsync(cancellationToken).ConfigureAwait(false);
                    
                    // Frees a slot in the preloaded request queue.
                    preloadedSemaphore.Release();
                    logger.LogTrace($"Dequeued request: {preloadedItem.Request}");

                    using var cancelCurrentSong = new CancellationTokenSource();
                    using var mainAndCancelCurrentSongLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancelCurrentSong.Token);
                    var linkedToken = mainAndCancelCurrentSongLinkedCts.Token;

                    void skipCurrentSong(object sender, SkipSongRequestEventArgs eventArgs)
                    {
                        cancelCurrentSong.Cancel();
                    }

                    try
                    {
                        SkipCurrentRequest += skipCurrentSong;
                        linkedToken.ThrowIfCancellationRequested();

                        logger.LogTrace($"Updating currently playing.");
                        CurrentlyPlaying = await preloadedItem.Request.ToAudioItemAsync(linkedToken).ConfigureAwait(false);
                        logger.LogTrace($"Getting audio playback for request {preloadedItem.Request}");
                        await using var playback = preloadedItem.Playback;

                        AudioStartedPlaying?.Invoke(this, new AudioStartedPlayingEventArgs { AudioRequest = preloadedItem.Request });
                        logger.LogTrace($"Getting audio playback stream from playback: {playback}");
                        await using var audioStream = await playback.GetAudioStreamAsync(linkedToken).ConfigureAwait(false);

                        try
                        {
                            logger.LogTrace($"Copying to output stream...");
                            await audioStream.CopyToAsync(discordAudioStream, linkedToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            logger.LogTrace($"Waiting for output stream to flush...");
                            await discordAudioStream.FlushAsync(linkedToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogTrace($"Cancellation requested during playback.");
                        if (cancellationToken.IsCancellationRequested) throw;
                    }
                    finally
                    {
                        logger.LogTrace($"Resetting currently playing and performing cleanup.");
                        CurrentlyPlaying = null;
                        SkipCurrentRequest -= skipCurrentSong;
                        AudioStoppedPlaying?.Invoke(this, new AudioStoppedPlayingEventArgs { AudioRequest = preloadedItem.Request });
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) {
                logger.LogTrace($"Audio worker cancellation requested.");
                throw;
            }
            catch (Exception ex) { logger.LogError(ex, $"Audio worker stopping with exception!"); throw; }
            finally
            {
                logger.LogDebug($"Stopping audio worker...");
                await discordAudioClient.StopAsync();
                discordAudioClient.Dispose();
            }
        }

        /// <inheritdoc/>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    logger.LogTrace($"Disposing...");
                    SkipRequest -= SkipSongsAsync;
                    cancellationTokenSource.Cancel();
                    preloadedRequestQueue.CompleteAdding();
                    requestQueue.CompleteAdding();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PreloadingAudioClientWorker()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
