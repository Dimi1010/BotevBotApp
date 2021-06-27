using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Services
{
    public interface IRequestParserService
    {
        /// <summary>
        /// Parses the <see cref="AudioRequestDTO"/> into a <see cref="AudioRequest"/>.
        /// </summary>
        /// <param name="requestDto">The request dto to be parsed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation.</param>
        /// <returns>The parsed request.</returns>
        public Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses a request string directly into an <see cref="AudioRequest"/>.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <param name="requester">The requester.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation.</param>
        /// <returns>The parsed request.</returns>
        public Task<AudioRequest> ParseRequestAsync(string request, string requester, CancellationToken cancellationToken = default)
            => ParseRequestAsync(new AudioRequestDTO { Request = request, Requester = requester }, cancellationToken);
    }
}
