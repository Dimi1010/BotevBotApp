using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using Discord;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Services
{
    public class AudioService : IAudioService
    {
        private readonly ILogger<AudioService> logger;
        private readonly IRequestParserService requestParser;
        private readonly IAudioClientWorkerFactory audioClientWorkerFactory;
        private readonly ConcurrentDictionary<ulong, IAudioClientWorker> workers = new();
        private readonly SemaphoreSlim audioClientsLock = new SemaphoreSlim(1);

        public AudioService(IRequestParserService requestParser, IAudioClientWorkerFactory audioClientWorkerFactory, ILogger<AudioService> logger)
        {
            this.requestParser = requestParser;
            this.audioClientWorkerFactory = audioClientWorkerFactory;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IAudioClientWorker> StartAudioAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Starting audio for channel {voiceChannel.Id}");
            await audioClientsLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            if(!workers.TryGetValue(voiceChannel.Id, out var worker))
            {
                logger.LogTrace($"Creating new audio connection for channel {voiceChannel.Id}");
                var audioClient = await voiceChannel.ConnectAsync().ConfigureAwait(false);
                logger.LogTrace($"Creating new audio worker for channel {voiceChannel.Id}");
                worker = audioClientWorkerFactory.CreateAudioClientWorker(voiceChannel.Id, audioClient);
                workers.TryAdd(voiceChannel.Id, worker);
                logger.LogTrace($"New audio connection worker for channel {voiceChannel.Id} created.");
            }
            audioClientsLock.Release();
            return worker;
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> StopAudioAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Stopping audio for channel {voiceChannel.Id}");
            if (workers.TryRemove(voiceChannel.Id, out var clientWorker))
            {
                logger.LogTrace($"Disposing of audio connection worker for channel {voiceChannel.Id}");
                clientWorker.Dispose();
            }

            return Task.FromResult(AudioServiceResult.Success);
        }

        /// <inheritdoc/>
        public Task<AudioServiceResult> EnqueueAudioAsync(IVoiceChannel voiceChannel, string request, string requester, CancellationToken cancellationToken = default)
            => EnqueueAudioAsync(voiceChannel, new AudioRequestDTO { Request = request, Requester = requester }, cancellationToken);

        /// <inheritdoc/>
        public async Task<AudioServiceResult> EnqueueAudioAsync(IVoiceChannel voiceChannel, AudioRequestDTO requestDto, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Enqueueing audio for channel {voiceChannel.Id}");
            if(!workers.TryGetValue(voiceChannel.Id, out var client))
            {
                logger.LogTrace($"Audio client not found for channel {voiceChannel.Id}. Starting new client.");
                client = await StartAudioAsync(voiceChannel, cancellationToken).ConfigureAwait(false);
            }

            logger.LogTrace($"Parsing request for channel {voiceChannel.Id}");
            var request = await requestParser.ParseRequestAsync(requestDto, cancellationToken).ConfigureAwait(false);

            logger.LogTrace($"Enqueueing parsed request for channe; {voiceChannel.Id}");
            await client.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public async Task<AudioServiceResult> SkipAudioAsync(IVoiceChannel voiceChannel, int count = 1, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Skipping audio for channel {voiceChannel.Id}");
            if (workers.TryGetValue(voiceChannel.Id, out var clientWorker))
            {
                await clientWorker.SkipAsync(count, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                logger.LogTrace($"Audio client for channel {voiceChannel.Id} not found.");
            }

            return AudioServiceResult.Success;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<AudioItemDTO>> GetAudioQueueAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Getting audio queue for channel {voiceChannel.Id}");
            if (workers.TryGetValue(voiceChannel.Id, out var audioClient))
            {
                return audioClient.GetQueueItemsAsync(cancellationToken);
            }
            return Task.FromResult(Enumerable.Empty<AudioItemDTO>());
        }
    }
}
