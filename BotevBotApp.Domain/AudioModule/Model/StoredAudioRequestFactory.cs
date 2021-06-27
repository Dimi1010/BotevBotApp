using FileStorageProviders;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal class StoredAudioRequestFactory
    {
        private readonly IFileStorageProvider storageProvider;

        public StoredAudioRequestFactory(IFileStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        public StoredAudioRequest CreateAudioRequest(long fileId, string requester) => new StoredAudioRequest(storageProvider, fileId, requester);
    }
}
