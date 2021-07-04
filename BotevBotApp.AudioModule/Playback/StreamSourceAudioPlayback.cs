using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A wrapper class over a stream.
    /// </summary>
    /// <remarks>
    /// Takes over management and disposal of the provided stream.
    /// </remarks>
    internal class StreamSourceAudioPlayback : AudioPlayback
    {
        private readonly Stream sourceStream;
        private Task<Stream> cachedSourceStreamTask;

        /// <summary>
        /// A wrapper class over a stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <remarks>
        /// Management over the provided stream disposal is taken over by the object.
        /// </remarks>
        public StreamSourceAudioPlayback(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
        }

        /// <inheritdoc/>
        public override Task<Stream> GetAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            return cachedSourceStreamTask ??= Task.FromResult(sourceStream);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            
            base.Dispose(disposing);
            if (disposing)
            {
                sourceStream.Dispose();
            }
            cachedSourceStreamTask = null;
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            if (IsDisposed) return;

            await base.DisposeAsyncCore().ConfigureAwait(false);
            await sourceStream.DisposeAsync().ConfigureAwait(false);
            cachedSourceStreamTask = null;
        }
    }
}
