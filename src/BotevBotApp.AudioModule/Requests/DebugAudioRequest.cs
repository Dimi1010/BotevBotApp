using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Requests
{
    internal sealed class DebugAudioRequest : AudioRequest
    {
        private readonly string filepath;
        
        public DebugAudioRequest(string filepath, string requester, ILogger<DebugAudioRequest> logger) : base(requester, logger)
        {
            this.filepath = filepath;
        }

        public override Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("Getting audio playback.");
            return Task.FromResult<AudioPlayback>(new DebugSamplePlayback(filepath));
        }

        private Task<AudioItemDTO> cachedTask;
        public override Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace($"Generating audio item with Name = {filepath}, Requester = {Requester}");
            return cachedTask ??= Task.FromResult(new AudioItemDTO
            {
                Name = filepath,
                Requester = Requester,
                Source = typeof(StoredAudioRequest).Name,
            });
        }
    }
}
