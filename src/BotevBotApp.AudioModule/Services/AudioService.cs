﻿using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Services
{
    public class AudioService : IAudioService
    {
        private readonly IRequestParserService requestParser;
        private readonly IAudioClientWorkerFactory audioClientWorkerFactory;
        private readonly ConcurrentDictionary<ulong, IAudioClientWorker> workers = new();

        public AudioService(IRequestParserService requestParser, IAudioClientWorkerFactory audioClientWorkerFactory)
        {
            this.requestParser = requestParser;
            this.audioClientWorkerFactory = audioClientWorkerFactory;
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, string request, string requester, CancellationToken cancellationToken = default)
            => EnqueueAudioAsync(channelDto, new AudioRequestDTO { Request = request, Requester = requester }, cancellationToken);

        /// <inheritdoc/>
        public async Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            var client = workers.GetOrAdd(channelDto.Channel.Id, (id) => audioClientWorkerFactory.CreateAudioClientWorker(id, channelDto));

            var request = await requestParser.ParseRequestAsync(requestDto, cancellationToken).ConfigureAwait(false);

            await client.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public async Task<AudioServiceResult> SkipAudioAsync(IVoiceChannel voiceChannel, int count = 1, CancellationToken cancellationToken = default)
        {
            if (workers.TryGetValue(voiceChannel.Id, out var clientWorker))
            {
                await clientWorker.SkipAsync(count, cancellationToken).ConfigureAwait(false);
            }

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> StopAudioAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default)
        {
            if (workers.TryRemove(voiceChannel.Id, out var clientWorker))
            {
                clientWorker.Dispose();
            }

            return Task.FromResult(AudioServiceResult.Success);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<AudioItemDTO>> GetAudioQueueAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default)
        {
            if(workers.TryGetValue(voiceChannel.Id, out var audioClient))
            {
                return audioClient.GetQueueItemsAsync(cancellationToken);
            }
            return Task.FromResult(Enumerable.Empty<AudioItemDTO>());
        }
    }
}