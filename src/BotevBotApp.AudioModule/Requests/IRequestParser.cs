using BotevBotApp.AudioModule.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Requests
{
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
