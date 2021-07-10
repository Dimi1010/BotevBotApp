using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
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
        
        public DebugAudioRequest(string filepath, string requester) : base(requester)
        {
            this.filepath = filepath;
        }

        public override Task<AudioPlayback> GetAudioPlaybackAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AudioPlayback>(new DebugSamplePlayback(filepath));
        }

        private Task<AudioItemDTO> cachedTask;
        public override Task<AudioItemDTO> ToAudioItemAsync(CancellationToken cancellationToken = default)
        {
            return cachedTask ??= Task.FromResult(new AudioItemDTO
            {
                Name = filepath,
                Requester = Requester,
                Source = typeof(StoredAudioRequest).Name,
            });
        }
    }
}
