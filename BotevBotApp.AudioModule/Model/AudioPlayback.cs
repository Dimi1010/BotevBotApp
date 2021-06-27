using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace BotevBotApp.AudioModule.Model
{
    public abstract class AudioPlayback
    {
        /// <summary>
        /// Gets the stream on which the playback will be outputted.
        /// </summary>
        public Stream AudioOutputStream { get; protected set; }

        /// <summary>
        /// Starts outputting the playback on the AudioOutputStream.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task representing the playback operation.<br/>
        /// The task completes when the entire playback is written to the output stream.
        /// </returns>
        public abstract Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="CachedAudioPlayback"/>.
        /// </summary>
        /// <returns>An new <see cref="CachedAudioPlayback"/> instance made from the current instance.</returns>
        internal virtual CachedAudioPlayback WithCache()
        {
            return new CachedAudioPlayback(this);
        }
    }

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

    internal class DecodingAudioPlayback : AudioPlayback
    {
        private readonly Stream encodedStream;

        public DecodingAudioPlayback(Stream encodedStream)
        {
            this.encodedStream = encodedStream;
        }

        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(encodedStream))
                .OutputToPipe(new StreamPipeSink(AudioOutputStream), options => options
                    .DisableChannel(FFMpegCore.Enums.Channel.Video)
                    .WithAudioSamplingRate(48000)
                    .WithCustomArgument("-ac 2")
                    .ForceFormat("s16le")
                )
                .ProcessAsynchronously()
                .ConfigureAwait(false);
        }
    }
}
