using System.Threading;
using System.Threading.Tasks;
using BotevBotApp.Domain.AudioModule.DTO;
using FileStorageProviders;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal class StoredAudioRequest : AudioRequest
    {
        private readonly IFileStorageProvider storageProvider;
        private readonly long fileId;

        public StoredAudioRequest(IFileStorageProvider storageProvider, long fileId, string requester) : base(requester)
        {
            this.storageProvider = storageProvider;
            this.fileId = fileId;
        }

        public override async Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default)
        {
            var result = await storageProvider.GetFileDataAsync(fileId, cancellationToken);
            return new DecodingAudioPlayback(result);
        }

        public override async Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await storageProvider.GetFileMetadataAsync(fileId, cancellationToken);
            return new AudioItemDTO {
                Name = metadata.Filename,
                Requester = Requester,
                Source = typeof(StoredAudioRequest).Name,
            };
        }
    }
}
