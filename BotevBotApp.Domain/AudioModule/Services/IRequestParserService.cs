using BotevBotApp.Domain.AudioModule.DTO;
using BotevBotApp.Domain.AudioModule.Model;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.Domain.AudioModule.Services
{
    public interface IRequestParserService
    {
        /// <summary>
        /// Parses a request string into an <see cref="AudioRequestDTO"/>.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <returns>The parsed request DTO.</returns>
        public Task<AudioRequestDTO> ParseRequestStringAsync(string request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses the <see cref="AudioRequestDTO"/> into a <see cref="AudioRequest"/>/
        /// </summary>
        /// <param name="requestDto">The request dto to be parsed.</param>
        /// <returns>An audio request.</returns>
        internal Task<AudioRequest> ParseRequestAsync(AudioRequestDTO requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses a request string directly into an <see cref="AudioRequest"/>.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <returns>The parsed request.</returns>
        internal async Task<AudioRequest> ParseRequestAsync(string request, CancellationToken cancellationToken = default) => await ParseRequestAsync(await ParseRequestStringAsync(request).ConfigureAwait(false)).ConfigureAwait(false);
    }
}
