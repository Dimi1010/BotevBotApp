using Discord.Audio;
using Microsoft.Extensions.Logging;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A factory for creating <see cref="PreloadingAudioClientWorker"/> instances.
    /// </summary>
    internal class PreloadingAudioClientWorkerFactory : IAudioClientWorkerFactory
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly int keepPreloaded;

        public PreloadingAudioClientWorkerFactory(ILoggerFactory loggerFactory, int keepPreloaded = 2)
        {
            this.loggerFactory = loggerFactory;
            this.keepPreloaded = keepPreloaded;
        }

        public IAudioClientWorker CreateAudioClientWorker(ulong workerId, IAudioClient audioClient)
        {
            return new PreloadingAudioClientWorker(workerId, audioClient, loggerFactory.CreateLogger<PreloadingAudioClientWorker>(), keepPreloaded);
        }
    }
}
