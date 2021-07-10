using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;

namespace BotevBotApp.AudioModule.Requests
{
    internal class YoutubeAudioRequest : RemoteAudioRequest
    {
        public const string ExpectedHost = "www.youtube.com";

        public YoutubeAudioRequest(Uri url, string requester) : base(url, requester)
        {
        }

        protected override bool ValidateUrl(Uri url)
        {
            return url.Host == ExpectedHost;
        }

        public override async Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var youtube = YouTube.Default;
            var videos = await youtube.GetAllVideosAsync(Url.ToString()).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var bestAudioCandidate = videos.FirstOrDefault(x => x.AdaptiveKind == AdaptiveKind.Audio);

            cancellationToken.ThrowIfCancellationRequested();

            if (bestAudioCandidate is not null)
            {
                var audioStream = await bestAudioCandidate.StreamAsync().ConfigureAwait(false);

                var decodingOptions = DecodingAudioPlaybackOptions.Default;

                decodingOptions.InputArgumentsOptions = options =>
                {
                    // Explicit setting of audio codec as ffmpeg cannot infer codec from pipe input.
                    options.WithAudioCodec(bestAudioCandidate.AudioFormat.ToString());
                };

                return new StreamSourceAudioPlayback(audioStream)
                    .WithCache()
                    .WithDecoding(decodingOptions)
                    .WithCache();
            }
            else
            {
                // Returns empty stream if can't find audio.
                return new StreamSourceAudioPlayback(new MemoryStream());
            }
        }

        public override async Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            var youtube = YouTube.Default;
            var video = await youtube.GetVideoAsync(Url.ToString()).ConfigureAwait(false);
            return new AudioItemDTO { Requester = Requester, Name = video.Title, Source = "YouTube" };
        }
    }
}
