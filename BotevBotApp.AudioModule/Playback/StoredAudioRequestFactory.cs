using FileStorageProviders;

namespace BotevBotApp.AudioModule.Playback
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
