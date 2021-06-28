using BotevBotApp.AudioModule.DTO;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A factory for creating <see cref="AudioClientWorker"/> instances.
    /// </summary>
    public class AudioClientWorkerFactory : IAudioClientWorkerFactory
    {
        public IAudioClientWorker CreateAudioClientWorker(ulong workerId, AudioVoiceChannelDTO channelDTO)
        {
            return new AudioClientWorker(workerId, channelDTO.AudioClient);
        }
    }
}
