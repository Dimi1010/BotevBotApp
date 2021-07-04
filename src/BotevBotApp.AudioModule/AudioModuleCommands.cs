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
    public class AudioModuleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IAudioService audioService;
        private readonly ConcurrentDictionary<ulong, IAudioClient> audioClients = new ConcurrentDictionary<ulong, IAudioClient>();
        private readonly SemaphoreSlim audioClientsLock = new SemaphoreSlim(1);

        public AudioModuleCommands(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayMusicAsync(string request)
        {
            IVoiceChannel channel = GetVoiceChannelFromUser();

            if (channel is null)
            {
                await ReplyAsync("User must be in a voice channel or a voice channel must be passed as an argument.").ConfigureAwait(false);
                return;
            }

            var requestDto = new AudioRequestDTO
            {
                Requester = Context.User.Username,
                Request = request,
            };

            await audioClientsLock.WaitAsync();
            if (!audioClients.TryGetValue(channel.Id, out IAudioClient audioClient))
            {
                audioClient = await channel.ConnectAsync().ConfigureAwait(false);

                IAudioClient updateFactory(ulong id, IAudioClient oldClient)
                {
                    if (oldClient.ConnectionState != ConnectionState.Disconnected)
                    {
                        oldClient.Dispose();
                    }

                    return audioClient;
                }

                audioClients.AddOrUpdate(channel.Id, audioClient, updateFactory);
            }
            audioClientsLock.Release();

            var voiceChannelDto = new AudioVoiceChannelDTO
            {
                Channel = channel,
                AudioClient = audioClient,
            };

            await audioService.EnqueueAudioAsync(voiceChannelDto, requestDto).ConfigureAwait(false);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public Task StopPlaybackAsync()
        {
            var channel = GetVoiceChannelFromUser();
            if (audioClients.TryRemove(channel.Id, out _))
            {
                return audioService.StopAudioAsync(channel);
            }

            return Task.CompletedTask;
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

            await ReplyAsync(msgBuilder.ToString());
        }

        private IVoiceChannel GetVoiceChannelFromUser()
        {
            return (Context.User as IGuildUser)?.VoiceChannel;
        }
    }
}
