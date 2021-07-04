using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Requests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    public class AudioClientQueueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The audio request assosiated with the event.
        /// </summary>
        public AudioRequest AudioRequest { get; init; }
    }

    public class AudioEnqueuedEventArgs : AudioClientQueueChangedEventArgs
    { }

    public class AudioStartedPlayingEventArgs : AudioClientQueueChangedEventArgs
    { }

    public class AudioStoppedPlayingEventArgs : AudioClientQueueChangedEventArgs
    { }

    public class AudioSkippedEventArgs : AudioClientQueueChangedEventArgs
    { }

    public interface IAudioClientWorker : IDisposable
    {
        /// <summary>
        /// Gets the currently playing item.
        /// </summary>
        AudioItemDTO CurrentlyPlaying { get; }
        
        /// <summary>
        /// Gets the worker id assosiated with the worker.
        /// </summary>
        ulong WorkerId { get; init; }

        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> is enqueued.
        /// </summary>
        event EventHandler<AudioEnqueuedEventArgs> AudioEnqueued;

        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> is skipped.
        /// </summary>
        event EventHandler<AudioSkippedEventArgs> AudioSkipped;
        
        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> starts playing.
        /// </summary>
        event EventHandler<AudioStartedPlayingEventArgs> AudioStartedPlaying;
        
        /// <summary>
        /// Fires when an <see cref="AudioRequest"/> finishes playing.
        /// </summary>
        event EventHandler<AudioStoppedPlayingEventArgs> AudioStoppedPlaying;

        /// <summary>
        /// Clears the audio queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the clear operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enqueues a new <see cref="AudioRequest"/> to the client queue.
        /// </summary>
        /// <param name="request">The audio request.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the enqueueing operation.</returns>
        Task EnqueueAsync(AudioRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current queue information.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation.</param>
        /// <returns>A task returning an enumerable of <see cref="AudioItemDTO"/>.</returns>
        /// <remarks>
        /// The first item is the currently playing one.
        /// </remarks>
        Task<IEnumerable<AudioItemDTO>> GetQueueItemsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Skips the number of songs in the queue.
        /// </summary>
        /// <param name="songsToSkip">The number of songs to skip.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation. Defaults to <see cref="CancellationToken.None"/></param>
        /// <returns>A task representing the skip operation.</returns>
        Task SkipAsync(int songsToSkip, CancellationToken cancellationToken = default);
    }
}