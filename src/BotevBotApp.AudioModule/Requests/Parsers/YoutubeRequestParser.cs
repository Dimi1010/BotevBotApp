using BotevBotApp.AudioModule.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Requests.Parsers
{
    public class YoutubeRequestParser : IRequestParser
    {
        /// <inheritdoc/>
        public Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ParseRequestStart(requestDto.Request))
                throw new RequestParseException("Request does not match: 'https://www.youtube.com/' or 'https://youtu.be/'");

            return Task.FromResult<AudioRequest>(new YoutubeAudioRequest(requestDto.Requester, new Uri(requestDto.Request)));
        }

        private static bool ParseRequestStart(string request)
        {
            //https://www.youtube.com/watch?v=GE0sFH6I5BE
            //https://youtu.be/GE0sFH6I5BE

            return request.StartsWith("https://www.youtube.com/") || request.StartsWith("https://youtu.be/");
        }
    }
}
