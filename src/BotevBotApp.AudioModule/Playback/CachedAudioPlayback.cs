using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// Wrapper over an <see cref="AudioPlayback"/> that caches the playback <see cref="Stream"/> into a <see cref="MemoryStream"/>.
    /// </summary>
    public sealed class CachedAudioPlayback : AudioPlayback
    {
        private readonly AudioPlayback innerPlayback;
        private MemoryStream cachedStream = null;

        /// <summary>
        /// Gets weather the playback stream has been cached.
        /// </summary>
        public bool Cached { get; private set; } = false;

        /// <summary>
        /// Constructs new instance of a <see cref="CachedAudioPlayback"/> over another <see cref="AudioPlayback"/>.
        /// </summary>
        /// <param name="innerPlayback">The audio playback to cache.</param>
        /// <exception cref="ArgumentNullException">The provided playback was null.</exception>
        public CachedAudioPlayback(AudioPlayback innerPlayback)
        {
            if (innerPlayback is null)
                throw new ArgumentNullException(nameof(innerPlayback));

            this.innerPlayback = innerPlayback;
        }

        /// <inheritdoc/>
        public override Task<Stream> GetAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            cancellationToken.ThrowIfCancellationRequested();
            return Cached ? GetCacheCopyAsync(cancellationToken) : GetAndCacheAudioStreamAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a copy of the cached <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        private Task<Stream> GetCacheCopyAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            cancellationToken.ThrowIfCancellationRequested();
            var outputStream = new MemoryStream(cachedStream.Capacity);
            cachedStream.WriteTo(outputStream);
            return Task.FromResult<Stream>(outputStream);
        }

        /// <summary>
        /// Processes and caches the audio stream from <see cref="innerPlayback"/> and returns a copy it.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        private async Task<Stream> GetAndCacheAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            cancellationToken.ThrowIfCancellationRequested();
            using var innerStream = await innerPlayback.GetAudioStreamAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            // Cancellation of the copy to cache partway will corrupt the cache.
            cachedStream = new MemoryStream();
            await innerStream.CopyToAsync(cachedStream, CancellationToken.None).ConfigureAwait(false);
            Cached = true;
            return await GetCacheCopyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            
            base.Dispose(disposing);
            if (disposing)
            {
                innerPlayback.Dispose();
                cachedStream?.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            if (IsDisposed) return;

            await base.DisposeAsyncCore().ConfigureAwait(false);
            await innerPlayback.DisposeAsync().ConfigureAwait(false);
            if (cachedStream is not null)
                await cachedStream.DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Overrides the <see cref="AudioPlayback.WithCache()"/> method so it returns the current instance.
        /// </summary>
        /// <returns>The current instance.</returns>
        /// <remarks>
        /// As the current playback is already cached, it is redundant to create a cache of a cache.
        /// </remarks>
        public override CachedAudioPlayback WithCache()
        {
            return this;
        }
    }
}
