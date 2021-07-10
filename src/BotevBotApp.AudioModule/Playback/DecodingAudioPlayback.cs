using FFMpegCore;
using FFMpegCore.Pipes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// Audio playback that gets decoded using FFMpeg.
    /// </summary>
    public class DecodingAudioPlayback : AudioPlayback
    {
        private readonly AudioPlayback innerPlayback;
        private readonly DecodingAudioPlaybackOptions options;

        public DecodingAudioPlayback(AudioPlayback innerPlayback) : this(innerPlayback, DecodingAudioPlaybackOptions.Default) { }

        public DecodingAudioPlayback(AudioPlayback innerPlayback, DecodingAudioPlaybackOptions options)
        {
            this.innerPlayback = innerPlayback;
            this.options = options;
        }

        public override async Task<Stream> GetAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var outputStream = new MemoryStream();
            using var inputStream = await innerPlayback.GetAudioStreamAsync(cancellationToken);
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream), options.InputArgumentsOptions)
                .OutputToPipe(new StreamPipeSink(outputStream), options.OutputArgumentOptions)
                .ProcessAsynchronously()
                .ConfigureAwait(false);
            outputStream.Position = 0;
            return outputStream;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            base.Dispose(disposing);
            if (disposing)
            {
                innerPlayback.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            if (IsDisposed) return;

            await base.DisposeAsyncCore().ConfigureAwait(false);
            await innerPlayback.DisposeAsync().ConfigureAwait(false);
        }
    }

    public class DecodingAudioPlaybackOptions
    {
        public static DecodingAudioPlaybackOptions Default => new DecodingAudioPlaybackOptions();

        public static Action<FFMpegArgumentOptions> DefaultOutputArgumentOptions
        {
            get
            {
                return options => options
                    .DisableChannel(FFMpegCore.Enums.Channel.Video)
                    .WithAudioSamplingRate(48000)
                    .WithCustomArgument("-ac 2")
                    .WithAudioCodec("pcm_s16le")
                    .ForceFormat("s16le");
            }
        }

        public static Action<FFMpegArgumentOptions> DefaultInputArgumentsOptions => null;

        public Action<FFMpegArgumentOptions> InputArgumentsOptions { get; set; } = DefaultOutputArgumentOptions;

        public Action<FFMpegArgumentOptions> OutputArgumentOptions { get; set; } = DefaultOutputArgumentOptions;
    }
}
