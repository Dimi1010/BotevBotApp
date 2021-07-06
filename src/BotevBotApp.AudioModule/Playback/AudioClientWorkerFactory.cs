using Discord.Audio;
using Microsoft.Extensions.Logging;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A factory for creating <see cref="AudioClientWorker"/> instances.
    /// </summary>
    public class AudioClientWorkerFactory : IAudioClientWorkerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public AudioClientWorkerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IAudioClientWorker CreateAudioClientWorker(ulong workerId, IAudioClient audioClient)
        {
            return new AudioClientWorker(workerId, audioClient, loggerFactory.CreateLogger<AudioClientWorker>());
        }
    }
}
