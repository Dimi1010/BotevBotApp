using FFMpegCore;
using FFMpegCore.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// Audio playback that gets decoded using FFMpeg.
    /// </summary>
    internal class DecodingAudioPlayback : AudioPlayback
    {
        private readonly Stream encodedStream;

        public DecodingAudioPlayback(Stream encodedStream)
        {
            this.encodedStream = encodedStream;
        }

        /// <inheritdoc/>
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
