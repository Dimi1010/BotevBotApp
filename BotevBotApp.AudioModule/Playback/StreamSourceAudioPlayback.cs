using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A wrapper class over a stream.
    /// </summary>
    internal class StreamSourceAudioPlayback : AudioPlayback
    {
        public StreamSourceAudioPlayback(Stream sourceStream)
        {
            AudioOutputStream = sourceStream;
        }

        /// <summary>
        /// Overrides StartAsync to be a noop as this is a wrapper class over a stream.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation. The current method is a noop, so it will never be cancelled.</param>
        /// <returns>A completed task.</returns>
        public override Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
