using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    internal sealed class DebugSamplePlayback : AudioPlayback
    {
        private readonly string filepath;

        public DebugSamplePlayback(string filepath)
        {
            this.filepath = filepath;
        }

        public override Task<Stream> GetAudioStreamAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(File.OpenRead(filepath));
        }
    }
}
