using BotevBotApp.AudioModule.DTO;

namespace BotevBotApp.AudioModule.Playback
{
    /// <summary>
    /// A factory for creating new <see cref="IAudioClientWorker"/> instances.
    /// </summary>
    public interface IAudioClientWorkerFactory
    {
        public IAudioClientWorker CreateAudioClientWorker(ulong workerId, AudioVoiceChannelDTO channelDTO);
    }
}
