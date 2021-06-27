using Discord;
using Discord.Audio;

namespace BotevBotApp.Domain.AudioModule.DTO
{
    public record AudioVoiceChannelDTO
    {
        public IVoiceChannel Channel { get; init; }
        public IAudioClient AudioClient { get; init; }
    }
}
