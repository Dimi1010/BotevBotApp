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

        /// <inheritdoc/>
        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            AudioOutputStream = new MemoryStream();
            cancellationToken.ThrowIfCancellationRequested();
            await innerPlayback.StartAsync(cancellationToken).ConfigureAwait(false);
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(innerPlayback.AudioOutputStream), options.InputArgumentsOptions)
                .OutputToPipe(new StreamPipeSink(AudioOutputStream), options.OutputArgumentOptions)
                .ProcessAsynchronously()
                .ConfigureAwait(false);
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
                    .ForceFormat("s16le");
            }
        }

        public static Action<FFMpegArgumentOptions> DefaultInputArgumentsOptions => null;

        public Action<FFMpegArgumentOptions> InputArgumentsOptions { get; set; } = DefaultOutputArgumentOptions;

        public Action<FFMpegArgumentOptions> OutputArgumentOptions { get; set; } = DefaultOutputArgumentOptions;
    }
}
