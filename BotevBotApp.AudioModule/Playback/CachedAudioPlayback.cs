using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// Wrapper over an <see cref="AudioPlayback"/> that caches the playback <see cref="Stream"/> into a <see cref="MemoryStream"/>.
    /// </summary>
    internal sealed class CachedAudioPlayback : AudioPlayback
    {
        private readonly AudioPlayback innerPlayback;

        /// <summary>
        /// Gets weather the playback stream has been cached.
        /// </summary>
        public bool Cached { get; private set; } = false;

        public CachedAudioPlayback(AudioPlayback playback)
        {
            innerPlayback = playback;
            AudioOutputStream = new MemoryStream();
        }

        /// <inheritdoc/>
        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Cached)
            {
                AudioOutputStream.Position = 0;
            }
            else
            {
                var innerStart = innerPlayback.StartAsync(cancellationToken).ConfigureAwait(false);
                await innerPlayback.AudioOutputStream.CopyToAsync(AudioOutputStream, cancellationToken).ConfigureAwait(false);
                await innerStart;
                Cached = true;
            }
        }

        /// <summary>
        /// Overrides the <see cref="AudioPlayback.WithCache()"/> method so it returns the current instance.
        /// </summary>
        /// <returns>The current instance.</returns>
        /// <remarks>
        /// As the current playback is already cached, it is redundant to create a cache of a cache.
        /// </remarks>
        internal override CachedAudioPlayback WithCache()
        {
            return this;
        }
    }
}
