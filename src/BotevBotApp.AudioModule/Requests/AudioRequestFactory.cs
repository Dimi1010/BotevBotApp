using Microsoft.Extensions.Logging;

namespace BotevBotApp.AudioModule.Requests
{
    internal abstract class AudioRequestFactory
    {
        protected readonly ILoggerFactory loggerFactory;

        public AudioRequestFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }
    }
}