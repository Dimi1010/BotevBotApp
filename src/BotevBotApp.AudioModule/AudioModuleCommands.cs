using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule
{
    [RequireOwner]
    public class AudioModuleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IAudioService audioService;

        public AudioModuleCommands(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        [Command("join", RunMode = RunMode.Async)]
        public Task JoinAsync()
        {
            IVoiceChannel channel = GetVoiceChannelFromUser();

            if (channel is null)
            {
                return ReplyAsync("User must be in a voice channel.");
            }

            return audioService.StartAudioAsync(channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("stop")]
        public Task LeaveAsync()
        {
            IVoiceChannel channel = GetVoiceChannelFromUser();

            if (channel is null)
            {
                return ReplyAsync("User must be in a voice channel.");
            }

            return audioService.StopAudioAsync(channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        public Task PlayMusicAsync(string request)
        {
            IVoiceChannel channel = GetVoiceChannelFromUser();

            if (channel is null)
            {
                return ReplyAsync("User must be in a voice channel.");
            }

            var requestDto = new AudioRequestDTO
            {
                Requester = Context.User.Username,
                Request = request,
            };

            return audioService.EnqueueAudioAsync(channel, requestDto);
        }

        [Command("queue", RunMode = RunMode.Async)]
        public async Task GetQueueAsync()
        {
            var channel = GetVoiceChannelFromUser();

            var queue = await audioService.GetAudioQueueAsync(channel).ConfigureAwait(false);

            var msgBuilder = new StringBuilder();
            
            int idx = -1;
            foreach (var item in queue)
            {
                idx++;
                msgBuilder.AppendLine($"{idx} {(idx == 1 ? ">>" : "--")} Name: {item.Name} | Requester: {item.Requester} | Source: {item.Source}");
            }

            await ReplyAsync(msgBuilder.ToString()).ConfigureAwait(false);
        }

        private IVoiceChannel GetVoiceChannelFromUser()
        {
            return (Context.User as IGuildUser)?.VoiceChannel;
        }
    }
}
