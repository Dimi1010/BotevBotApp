using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// Wrapper over an <see cref="AudioPlayback"/> that caches the playback <see cref="Stream"/> into a <see cref="MemoryStream"/>.
    /// </summary>
    public class CachedAudioPlayback : AudioPlayback
    {
        private readonly AudioPlayback innerPlayback;
        private readonly object cachingLock = new();
        private MemoryStream cachedStream = null;
        private Task cachingTask = null;

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
        protected Task<Stream> GetCacheCopyAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!Cached)
                throw new PlaybackNotCachedException();

            var outputStream = new MemoryStream(cachedStream.Capacity);
            cachedStream.WriteTo(outputStream);
            outputStream.Position = 0;
            return Task.FromResult<Stream>(outputStream);
        }

        /// <summary>
        /// Processes and caches the audio stream from <see cref="innerPlayback"/> and returns a copy it.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        protected async Task<Stream> GetAndCacheAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            await CacheAudioStreamAsync(cancellationToken);
            return await GetCacheCopyAsync(cancellationToken);
        }
        
        /// <summary>
        /// Caches the audio stream asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the caching operation.</returns>
        protected Task CacheAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            lock (cachingLock)
            {
               return cachingTask ??= CacheAudioStreamInternalAsync();
            }
        }

        /// <summary>
        /// The internal asynchronous caching operation.
        /// </summary>
        /// <returns>A task representing the caching operation.</returns>
        /// <remarks>
        /// Cannot be cancelled because it may corrupt the cache.
        /// </remarks>
        private async Task CacheAudioStreamInternalAsync()
        {
            ThrowIfDisposed();

            using var innerStream = await innerPlayback.GetAudioStreamAsync().ConfigureAwait(false);
            // Cancellation of the copy to cache partway will corrupt the cache.
            cachedStream = new MemoryStream();
            await innerStream.CopyToAsync(cachedStream).ConfigureAwait(false);
            Cached = true;

            // Disposes of inner playback as its already cached.
            await innerPlayback.DisposeAsync().ConfigureAwait(false);
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


    [Serializable]
    public class CacheException : Exception
    {
        public CacheException() { }
        public CacheException(string message) : base(message) { }
        public CacheException(string message, Exception inner) : base(message, inner) { }
        protected CacheException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class PlaybackAlreadyCachedException : CacheException
    {
        public PlaybackAlreadyCachedException() { }
        public PlaybackAlreadyCachedException(string message) : base(message) { }
        public PlaybackAlreadyCachedException(string message, Exception inner) : base(message, inner) { }
        protected PlaybackAlreadyCachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class PlaybackNotCachedException : CacheException
    {
        public PlaybackNotCachedException() { }
        public PlaybackNotCachedException(string message) : base(message) { }
        public PlaybackNotCachedException(string message, Exception inner) : base(message, inner) { }
        protected PlaybackNotCachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
