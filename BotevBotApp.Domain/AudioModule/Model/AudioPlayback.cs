using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotevBotApp.Domain.Model;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal abstract class AudioPlayback : DomainObject
    {
        /// <summary>
        /// Gets the stream on which the playback will be outputted.
        /// </summary>
        public Stream AudioOutputStream { get; }

        /// <summary>
        /// Starts outputting the playback on the AudioOutputStream.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task representing the playback operation.</returns>
        public abstract Task StartAsync(CancellationToken cancellationToken = default);
    }
}
