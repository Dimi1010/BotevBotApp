using BotevBotApp.AudioModule.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Requests.Parsers
{
    internal class DebugRequestParser : IRequestParser
    {
        private const string prefix = "debug file ";
        private readonly DebugAudioRequestFactory requestFactory;

        public DebugRequestParser(DebugAudioRequestFactory requestFactory)
        {
            this.requestFactory = requestFactory;
        }

        public Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!requestDto.Request.StartsWith(prefix))
                throw new RequestParseException("Parser requires a debug type request and a 'file prefix'.");

            return Task.FromResult<AudioRequest>(requestFactory.CreateAudioRequest(requestDto.Request.Remove(0, prefix.Length), requestDto.Requester));
        }
    }
}
