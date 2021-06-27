using System.Threading;
using System.Threading.Tasks;
using BotevBotApp.Domain.AudioModule.DTO;

namespace BotevBotApp.Domain.AudioModule.Model
{
    public abstract class AudioRequest
    {
        /// <summary>
        /// Gets the name of the requester.
        /// </summary>
        public string Requester { get; private init; }

        public AudioRequest(string requester)
        {
            Requester = requester;
        }

        /// <summary>
        /// Gets an <see cref="AudioPlayback"/> object representing the requested audio.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task representing the fetching operation.</returns>
        public abstract Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a <see cref="AudioPlayback"/> object representing the requested audio.
        /// </summary>
        /// <param name="cachePlayback">Determines if the returned <see cref="AudioPlayback"/> should have caching enabled.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task representing the fetching operation.</returns>
        public async Task<AudioPlayback> GetAudioPlaybackAsync(bool cachePlayback = false, CancellationToken cancellationToken = default)
        {
            var playback = await GetAudioPlaybackAsync(cancellationToken);
            return cachePlayback ? playback.WithCache() : playback;
        }

        /// <summary>
        /// Gets an audio item dto that describes the request.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that returns the <see cref="AudioRequestDTO"/>.</returns>
        public abstract Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default);
    }
}
