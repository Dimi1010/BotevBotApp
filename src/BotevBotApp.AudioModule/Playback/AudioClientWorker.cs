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
    internal sealed class AudioClientWorker : IDisposable, IAudioClientWorker
    {
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
        private readonly ILogger<AudioClientWorker> logger;

        /// <summary>
        /// The collection used to store the requests.
        /// </summary>
        /// <remarks>
        /// The collection exists so snapshoots can be taken of the internal collection, via <see cref="ConcurrentQueue{T}.GetEnumerator()"/> and <see cref="ConcurrentQueue{T}.ToArray()"/>.<br/>
        /// Use <see cref="queue"/> for adding and removing elements instead.
        /// </remarks>
        private readonly ConcurrentQueue<AudioRequest> _queueInternal;

        /// <summary>
        /// Async wrapper over <see cref="_queueInternal"/>.
        /// </summary>
        private readonly AsyncCollection<AudioRequest> queue;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private int queueLength = 0;

        private class SkipSongRequestEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the number of songs that are requested to be skipped.
            /// </summary>
            public int SongsToSkip { get; set; }
        }
        private event EventHandler<SkipSongRequestEventArgs> SkipSongRequest;

        public AudioClientWorker(ulong workerId, IAudioClient discordAudioClient, ILogger<AudioClientWorker> logger)
        {
            WorkerId = workerId;
            this.discordAudioClient = discordAudioClient;
            this.logger = logger;
            _queueInternal = new();
            queue = new(_queueInternal);

            Task DisconnectCancel(Exception ex) { cancellationTokenSource.Cancel(); return Task.CompletedTask; }
            this.discordAudioClient.Disconnected += DisconnectCancel;

            var token = cancellationTokenSource.Token;
            token.Register(() => { this.discordAudioClient.Disconnected -= DisconnectCancel; });

            // TODO: Maybe wrap in longrunning task? Profiling needed.
            _ = WorkAsync(token);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            logger.LogTrace($"Disposing...");
            // Should the cts be disposed?
            cancellationTokenSource.Cancel();
            queue.CompleteAdding();
        }

        /// <inheritdoc/>
        public async Task EnqueueAsync(AudioRequest request, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Enqueing request: {request}");
            Interlocked.Increment(ref queueLength);
            try
            {
                await queue.AddAsync(request, cancellationToken).ConfigureAwait(false);
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
            Interlocked.Add(ref queueLength, -songsToSkip);
            SkipSongRequest?.Invoke(this, new SkipSongRequestEventArgs { SongsToSkip = songsToSkip });
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug($"Skipping all requests.");
            var toSkip = Interlocked.Exchange(ref queueLength, 0);
            SkipSongRequest?.Invoke(this, new SkipSongRequestEventArgs { SongsToSkip = toSkip });
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AudioItemDTO>> GetQueueItemsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug($"Fetching request queue.");
            var current = CurrentlyPlaying;

            var queueRequests = _queueInternal.ToArray();
            var items = (await Task.WhenAll(queueRequests.Select(r => r.ToAudioItemAsync(cancellationToken))).ConfigureAwait(false)).AsEnumerable();

            if (current is not null)
            {
                items = items.Prepend(current);
            }
            return items;
        }

        /// <summary>
        /// The worker task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation.</param>
        /// <returns>A task representing the work operation.</returns>
        private async Task WorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug($"Starting worker...");
            
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogTrace("Creating output stream.");
                using var discordAudioStream = discordAudioClient.CreatePCMStream(AudioApplication.Music);
                while (!cancellationToken.IsCancellationRequested)
                {

                    logger.LogTrace("Waiting for request.");
                    var request = await queue.TakeAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogTrace($"Dequeued request: {request}");
                    Interlocked.Decrement(ref queueLength);

                    using var cancelCurrentSong = new CancellationTokenSource();
                    using var mainAndCancelCurrentSongLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancelCurrentSong.Token);
                    var linkedToken = mainAndCancelCurrentSongLinkedCts.Token;

                    async void skipSongs(object sender, SkipSongRequestEventArgs eventArgs)
                    {
                        using var maxSkipExecutionTimeCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                        using var mainAndmaxSkipExecutionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, maxSkipExecutionTimeCts.Token);
                        try
                        {
                            for (int skipped = 0; skipped < eventArgs.SongsToSkip - 1; skipped++)
                            {
                                logger.LogTrace($"Dequeing request to skip.");
                                var request = await queue.TakeAsync(mainAndmaxSkipExecutionCts.Token).ConfigureAwait(false);
                                logger.LogTrace($"Skipping request: {request}");
                                AudioSkipped?.Invoke(this, new AudioSkippedEventArgs { AudioRequest = request });
                            }
                        }
                        finally
                        {
                            cancelCurrentSong.Cancel();
                        }
                    }

                    try
                    {
                        SkipSongRequest += skipSongs;
                        linkedToken.ThrowIfCancellationRequested();

                        logger.LogTrace($"Updating currently playing.");
                        CurrentlyPlaying = await request.ToAudioItemAsync(linkedToken).ConfigureAwait(false);
                        logger.LogTrace($"Getting audio playback for request {request}");
                        await using var playback = await request.GetAudioPlaybackAsync(linkedToken).ConfigureAwait(false);

                        AudioStartedPlaying?.Invoke(this, new AudioStartedPlayingEventArgs { AudioRequest = request });
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
                        SkipSongRequest -= skipSongs;
                        AudioStoppedPlaying?.Invoke(this, new AudioStoppedPlayingEventArgs { AudioRequest = request });
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) {
                logger.LogTrace($"Worker cancellation requested.");
                throw;
            }
            catch (Exception ex) { logger.LogError(ex, $"Worker stopping with exception!"); throw; }
            finally
            {
                logger.LogDebug($"Stopping worker...");
                await discordAudioClient.StopAsync();
                discordAudioClient.Dispose();
            }
        }
    }
}
