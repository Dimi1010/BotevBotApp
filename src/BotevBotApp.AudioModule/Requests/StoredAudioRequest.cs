using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using FileStorageProviders;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Requests
{
    internal class StoredAudioRequest : AudioRequest
    {
        private readonly IFileStorageProvider storageProvider;
        private readonly long fileId;

        public StoredAudioRequest(IFileStorageProvider storageProvider, long fileId, string requester, ILogger<StoredAudioRequest> logger) : base(requester, logger)
        {
            this.storageProvider = storageProvider;
            this.fileId = fileId;
        }

        public override async Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogDebug($"Getting audio playback with id {fileId}.");
            var result = await storageProvider.GetFileDataAsync(fileId, cancellationToken).ConfigureAwait(false);
            return new StreamSourceAudioPlayback(result).WithTranscoding();
        }

        public override async Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await storageProvider.GetFileMetadataAsync(fileId, cancellationToken).ConfigureAwait(false);
            Logger.LogTrace($"Generating audio item with Name = {metadata.Filename}, Requester = {Requester}");
            return new AudioItemDTO
            {
                Name = metadata.Filename,
                Requester = Requester,
                Source = typeof(StoredAudioRequest).Name,
            };
        }
    }
}
