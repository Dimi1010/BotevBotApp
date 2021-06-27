using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Model;
using System;
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

    public interface IRequestParser
    {
        /// <summary>
        /// Parses a request from a DTO.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestParseException">The request could not be parsed by the parser.</exception>
        Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default);
    }


    [Serializable]
    public class RequestParseException : Exception
    {
        public RequestParseException() { }
        public RequestParseException(string message) : base(message) { }
        public RequestParseException(string message, Exception inner) : base(message, inner) { }
        protected RequestParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
