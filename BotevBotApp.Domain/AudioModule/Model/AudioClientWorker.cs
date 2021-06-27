using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Discord.Audio;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BotevBotApp.Domain.AudioModule.DTO;
using System.Linq;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal class AudioClientQueueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The audio request assosiated with the event.
        /// </summary>
        public AudioRequest AudioRequest { get; init; }
    }

    internal class AudioEnqueuedEventArgs : AudioClientQueueChangedEventArgs
    { }

    internal class AudioStartedPlayingEventArgs : AudioClientQueueChangedEventArgs
    { }

    internal class AudioStoppedPlayingEventArgs : AudioClientQueueChangedEventArgs
    { }

    internal class AudioSkippedEventArgs : AudioClientQueueChangedEventArgs
    { }

    internal sealed class AudioClientWorker : IDisposable
    {
        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> is enqueued.
        /// </summary>
        public event EventHandler<AudioEnqueuedEventArgs> AudioEnqueued;

        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> is skipped.
        /// </summary>
        public event EventHandler<AudioSkippedEventArgs> AudioSkipped;

        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> starts playing.
        /// </summary>
        public event EventHandler<AudioStartedPlayingEventArgs> AudioStartedPlaying;

        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> finishes playing.
        /// </summary>
        public event EventHandler<AudioStoppedPlayingEventArgs> AudioStoppedPlaying;

        /// <summary>
        /// Gets the worker id assosiated with the worker.
        /// </summary>
        public ulong WorkerId { get; init; }


        private AudioItemDTO _currentlyPlaying = null;

        /// <summary>
        /// Gets the currently playing item.
        /// </summary>
        public AudioItemDTO CurrentlyPlaying
        {
            get 
            {
                lock (_currentlyPlaying)
                {
                    return _currentlyPlaying; 
                }
            }
            private set 
            {
                lock (_currentlyPlaying)
                {
                    _currentlyPlaying = value; 
                }
            }
        }


        private readonly IAudioClient discordAudioClient;

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

        public AudioClientWorker(ulong workerId, IAudioClient discordAudioClient)
        {
            WorkerId = workerId;
            this.discordAudioClient = discordAudioClient;

            _queueInternal = new();
            queue = new(_queueInternal);

            Task DisconnectCancel(Exception ex) { cancellationTokenSource.Cancel(); return Task.CompletedTask; }
            this.discordAudioClient.Disconnected += DisconnectCancel;

            var token = cancellationTokenSource.Token;
            token.Register(() => { this.discordAudioClient.Disconnected -= DisconnectCancel; });

            _ = WorkAsync(token);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            queue.CompleteAdding();
            cancellationTokenSource.Cancel();
            // Should the cts be disposed?
        }

        /// <summary>
        /// Enqueues a new <see cref="AudioRequest"/> to the client queue.
        /// </summary>
        /// <param name="request">The audio request.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the enqueueing operation.</returns>
        public async Task EnqueueAsync(AudioRequest request, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref queueLength);
            try
            {
                await queue.AddAsync(request, cancellationToken);
                AudioEnqueued?.Invoke(this, new AudioEnqueuedEventArgs { AudioRequest = request });
            }
            catch (Exception)
            {
                Interlocked.Decrement(ref queueLength);
                throw;
            }
        }

        /// <summary>
        /// Skips the number of songs in the queue.
        /// </summary>
        /// <param name="songsToSkip">The number of songs to skip.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the skip operation.</returns>
        public Task SkipAsync(int songsToSkip, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Add(ref queueLength, -songsToSkip);
            SkipSongRequest?.Invoke(this, new SkipSongRequestEventArgs { SongsToSkip = songsToSkip });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears the audio queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the clear operation.</returns>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var toSkip = Interlocked.Exchange(ref queueLength, 0);
            SkipSongRequest?.Invoke(this, new SkipSongRequestEventArgs { SongsToSkip = toSkip });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the current queue information.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation.</param>
        /// <returns>A task returning an enumerable of <see cref="AudioItemDTO"/>.</returns>
        /// <remarks>
        /// The first item is the currently playing one.
        /// </remarks>
        public async Task<IEnumerable<AudioItemDTO>> GetQueueItemsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = CurrentlyPlaying;

            var queueRequests = _queueInternal.ToArray();
            var items = (await Task.WhenAll(queueRequests.Select(r => r.ToAudioItemAsync(cancellationToken)))).AsEnumerable();

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
            cancellationToken.ThrowIfCancellationRequested();
            using var discordAudioStream = discordAudioClient.CreatePCMStream(AudioApplication.Music);
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = await queue.TakeAsync(cancellationToken).ConfigureAwait(false);
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
                            var request = await queue.TakeAsync(mainAndmaxSkipExecutionCts.Token).ConfigureAwait(false);
                            AudioSkipped?.Invoke(this, new AudioSkippedEventArgs { AudioRequest = request });
                        }
                    }
                    finally
                    {
                        cancelCurrentSong.Cancel();
                    }
                }

                linkedToken.ThrowIfCancellationRequested();
                SkipSongRequest += skipSongs;
                try
                {
                    CurrentlyPlaying = await request.ToAudioItemAsync(linkedToken);
                    var playback = await request.GetAudioPlaybackAsync(linkedToken).ConfigureAwait(false);

                    _ = playback.StartAsync(linkedToken);

                    AudioStartedPlaying?.Invoke(this, new AudioStartedPlayingEventArgs { AudioRequest = request });
                    await playback.AudioOutputStream.CopyToAsync(discordAudioStream, linkedToken).ConfigureAwait(false);
                    await discordAudioStream.FlushAsync(linkedToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested) throw;
                }
                finally
                {
                    CurrentlyPlaying = null;
                    SkipSongRequest -= skipSongs;
                    AudioStoppedPlaying?.Invoke(this, new AudioStoppedPlayingEventArgs { AudioRequest = request });
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
