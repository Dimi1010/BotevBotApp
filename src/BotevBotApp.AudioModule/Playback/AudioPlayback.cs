using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    public abstract class AudioPlayback : IDisposable, IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the output audio stream.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the operation.<br/>
        /// The task returns a <see cref="Stream"/> object containing the audio data.
        /// </returns>
        public abstract Task<Stream> GetAudioStreamAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="CachedAudioPlayback"/>.
        /// </summary>
        /// <returns>A new <see cref="CachedAudioPlayback"/> instance made from the current instance.</returns>
        public virtual CachedAudioPlayback WithCache() => new CachedAudioPlayback(this);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="DecodingAudioPlayback"/> with default options.
        /// </summary>
        /// <returns>A new <see cref="DecodingAudioPlayback"/> instance made from the current instance.</returns>
        public virtual DecodingAudioPlayback WithDecoding() => WithDecoding(DecodingAudioPlaybackOptions.Default);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="DecodingAudioPlayback"/>.
        /// </summary>
        /// <param name="options">Options with which to construct <see cref="DecodingAudioPlayback"/>.</param>
        /// <returns>A new <see cref="DecodingAudioPlayback"/> instance made from the current instance.</returns>
        public virtual DecodingAudioPlayback WithDecoding(DecodingAudioPlaybackOptions options) => new DecodingAudioPlayback(this, options);

        /// <summary>
        /// Helper method to throw <see cref="ObjectDisposedException"/> if the object has been already disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Signified weather it has been called by <see cref="IDisposable.Dispose"/> or by the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            //if (disposing)
            //{
            //    // TODO: dispose managed state (managed objects)
            //}

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            IsDisposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }
    }
}
