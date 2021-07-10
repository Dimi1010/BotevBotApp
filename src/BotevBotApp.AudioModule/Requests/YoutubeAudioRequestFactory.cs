using Microsoft.Extensions.Logging;
using System;

namespace BotevBotApp.AudioModule.Requests
{
    internal class YoutubeAudioRequestFactory : AudioRequestFactory
    {
        public YoutubeAudioRequestFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        public YoutubeAudioRequest CreateAudioRequest(Uri url, string requester) => new YoutubeAudioRequest(url, requester, loggerFactory.CreateLogger<YoutubeAudioRequest>());
    }
}
