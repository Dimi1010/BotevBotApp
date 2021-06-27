using BotevBotApp.Domain.AudioModule.DTO;
using BotevBotApp.Domain.AudioModule.Model;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.Domain.AudioModule.Services
{
    public class RequestParserService : IRequestParserService
    {
        public Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<AudioRequestDTO> ParseRequestStringAsync(string request, string requester, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
