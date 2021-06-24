using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using BotevBotApp.Domain.AudioModule.DTO;
using BotevBotApp.Domain.AudioModule.Model;
using System.Collections.Concurrent;
using System.Threading;

namespace BotevBotApp.Domain.AudioModule.Services
{
    internal class AudioService : IAudioService
    {
        private readonly IRequestParserService requestParser;
        private readonly ConcurrentDictionary<ulong, AudioClientWorker> workers = new();

        /// <inheritdoc/>
        public async Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, string request, CancellationToken cancellationToken = default) 
            => await EnqueueAudioAsync(channelDto, await requestParser.ParseRequestStringAsync(request, cancellationToken), cancellationToken);

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
                await clientWorker.SkipAsync(count, cancellationToken);
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
            throw new NotImplementedException();
        }
    }
}
