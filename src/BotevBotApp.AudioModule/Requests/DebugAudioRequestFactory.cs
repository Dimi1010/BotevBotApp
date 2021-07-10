using Microsoft.Extensions.Logging;

namespace BotevBotApp.AudioModule.Requests
{
    internal class DebugAudioRequestFactory : AudioRequestFactory
    {
        public DebugAudioRequestFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public DebugAudioRequest CreateAudioRequest(string filepath, string requester) => new DebugAudioRequest(filepath, requester, loggerFactory.CreateLogger<DebugAudioRequest>());
    }
}
