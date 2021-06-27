using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Services
{
    public class RequestParserService : IRequestParserService
    {
        private readonly IEnumerable<IRequestParser> requestParsers;

        public RequestParserService(IEnumerable<IRequestParser> requestParsers)
        {
            this.requestParsers = requestParsers;
        }

        /// <inheritdoc/>
        public async Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var parser in requestParsers)
            {
                try
                {
                    return await parser.ParseRequestAsync(requestDto, cancellationToken).ConfigureAwait(false);
                }
                catch(RequestParseException)
                {
                    // Swallows the exception and proceeds to the next parser down the line.
                }
            }
            throw new RequestParseException($"The provided request {requestDto} could not be parsed.");
        }
    }
}
