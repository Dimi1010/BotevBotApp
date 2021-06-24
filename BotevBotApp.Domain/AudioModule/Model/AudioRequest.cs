using System.Threading;
using System.Threading.Tasks;
using BotevBotApp.Domain.Model;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal abstract class AudioRequest : DomainObject
    {
        /// <summary>
        /// Gets an <see cref="AudioPlayback"/> object representing the requested audio.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task representing the fetching operation.</returns>
        public abstract Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken);
    }
}
