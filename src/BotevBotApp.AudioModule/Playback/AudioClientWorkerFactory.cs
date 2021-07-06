using Discord.Audio;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A factory for creating <see cref="AudioClientWorker"/> instances.
    /// </summary>
    public class AudioClientWorkerFactory : IAudioClientWorkerFactory
    {
        public IAudioClientWorker CreateAudioClientWorker(ulong workerId, IAudioClient audioClient)
        {
            return new AudioClientWorker(workerId, audioClient);
        }
    }
}
