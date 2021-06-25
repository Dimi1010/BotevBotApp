using System.Threading;
using System.Threading.Tasks;
using BotevBotApp.Domain.AudioModule.DTO;
using BotevBotApp.Domain.Model;
using FileStorageProviders;

namespace BotevBotApp.Domain.AudioModule.Model
{
    internal abstract class AudioRequest : DomainObject
    {
        /// <summary>
        /// Gets the name of the requester.
        /// </summary>
        public string Requester { get; protected set; }

        /// <summary>
        /// Gets an <see cref="AudioPlayback"/> object representing the requested audio.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task representing the fetching operation.</returns>
        public abstract Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets an audio item dto that describes the request.
        /// </summary>
        /// <returns>An audio item dto.</returns>
        public abstract AudioItemDTO ToAudioItem();
    }

    internal class StoredAudioRequest : AudioRequest
    {
        private readonly IFileStorageProvider storageProvider;
        private readonly long fileId;

        public override async Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await storageProvider.GetFileDataAsync(fileId);
            return new DecodingAudioPlayback(result).WithCache();
        }

        public override AudioItemDTO ToAudioItem()
        {
            throw new System.NotImplementedException();
        }
    }
}
