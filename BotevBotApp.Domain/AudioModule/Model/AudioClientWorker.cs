using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Discord.Audio;
using BotevBotApp.Domain.Model;

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

    internal sealed class AudioClientWorker : DomainObject, IDisposable
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

        private readonly IAudioClient discordAudioClient;
        private readonly AsyncProducerConsumerQueue<AudioRequest> queue;
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

            Func<Exception, Task> disconnectCancel = (ex) => { cancellationTokenSource.Cancel(); return Task.CompletedTask; };
            this.discordAudioClient.Disconnected += disconnectCancel;

            var token = cancellationTokenSource.Token;
            token.Register(() => { this.discordAudioClient.Disconnected -= disconnectCancel; });

            _ = WorkAsync(token);
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
                await queue.EnqueueAsync(request, cancellationToken);
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
        
        /// <inheritdoc/>
        public void Dispose()
        {
            queue.CompleteAdding();
            cancellationTokenSource.Cancel();
            // Should the cts be disposed?
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
                var request = await queue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                Interlocked.Decrement(ref queueLength);

                using var cancelCurrentSong = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancelCurrentSong.Token);
                var linkedToken = linkedCts.Token;

                async void skipSongs(object sender, SkipSongRequestEventArgs eventArgs)
                {
                    try
                    {
                        for (int skipped = 0; skipped < eventArgs.SongsToSkip - 1; skipped++)
                        {
                            var request = await queue.DequeueAsync(cancellationToken).ConfigureAwait(false);
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
                    SkipSongRequest -= skipSongs;
                    AudioStoppedPlaying?.Invoke(this, new AudioStoppedPlayingEventArgs { AudioRequest = request });
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
