using FileStorageProviders;
using Microsoft.Extensions.Logging;

namespace BotevBotApp.AudioModule.Requests
{
    internal class StoredAudioRequestFactory : AudioRequestFactory
    {
        private readonly IFileStorageProvider storageProvider;

        public StoredAudioRequestFactory(IFileStorageProvider storageProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            this.storageProvider = storageProvider;
        }

        public StoredAudioRequest CreateAudioRequest(long fileId, string requester) => new StoredAudioRequest(storageProvider, fileId, requester, loggerFactory.CreateLogger<StoredAudioRequest>());
    }
}
