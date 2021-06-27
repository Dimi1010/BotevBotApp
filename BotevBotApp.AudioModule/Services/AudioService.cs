using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Model;
using System.Collections.Concurrent;
using System.Threading;

namespace BotevBotApp.AudioModule.Services
{
    public class AudioService : IAudioService
    {
        private readonly IRequestParserService requestParser;
        private readonly ConcurrentDictionary<ulong, AudioClientWorker> workers = new();

        public AudioService(IRequestParserService requestParser)
        {
            this.requestParser = requestParser;
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, string request, string requester, CancellationToken cancellationToken = default)
            => EnqueueAudioAsync(channelDto, new AudioRequestDTO { Request = request, Requester = requester }, cancellationToken);

        /// <inheritdoc/>
        public async Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            var client = workers.GetOrAdd(channelDto.Channel.Id, (id) => new AudioClientWorker(id, channelDto.AudioClient));

            var request = await requestParser.ParseRequestAsync(requestDto, cancellationToken).ConfigureAwait(false);

            await client.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public async Task<AudioServiceResult> SkipAudioAsync(AudioVoiceChannelDTO channelDto, int count = 1, CancellationToken cancellationToken = default)
        {
            if (workers.TryGetValue(channelDto.Channel.Id, out var clientWorker))
            {
                await clientWorker.SkipAsync(count, cancellationToken).ConfigureAwait(false);
            }

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> StopAudioAsync(AudioVoiceChannelDTO channelDto, CancellationToken cancellationToken = default)
        {
            if (workers.TryRemove(channelDto.Channel.Id, out var clientWorker))
            {
                clientWorker.Dispose();
            }

            return Task.FromResult(AudioServiceResult.Success);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<AudioItemDTO>> GetAudioQueueAsync(AudioVoiceChannelDTO channelDto, CancellationToken cancellationToken = default)
        {
            if(workers.TryGetValue(channelDto.Channel.Id, out var audioClient))
            {
                return audioClient.GetQueueItemsAsync(cancellationToken);
            }
            return Task.FromResult(Enumerable.Empty<AudioItemDTO>());
        }
    }
}
