using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;

namespace BotevBotApp.AudioModule.Requests
{
    public class YoutubeAudioRequest : RemoteAudioRequest
    {
        public const string ExpectedHost = "www.youtube.com";

        public YoutubeAudioRequest(string requester, Uri url) : base(requester, url)
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

            var bestCandidate = videos.First(video => video.Resolution == videos.Min(v => v.Resolution));

            cancellationToken.ThrowIfCancellationRequested();

            var videoStream = await bestCandidate.StreamAsync().ConfigureAwait(false);

            return new StreamSourceAudioPlayback(videoStream).WithCache().WithDecoding();
        }

        public override async Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            var youtube = YouTube.Default;
            var video = await youtube.GetVideoAsync(Url.ToString()).ConfigureAwait(false);
            return new AudioItemDTO { Requester = Requester, Name = video.Title, Source = "YouTube" };
        }
    }
}
