﻿using BotevBotApp.AudioModule.DTO;
using BotevBotApp.AudioModule.Playback;
using Discord;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Services
{
    public interface IAudioService
    {
        public Task<IAudioClientWorker> StartAudioAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the audio playback in the channel.
        /// </summary>
        /// <param name="voiceChannel">The channel in which the playback is to be stopped.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> StopAudioAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Appends the audio to the playlist of the voice channel.
        /// </summary>
        /// <param name="channelDto"></param>
        /// <param name="request">The request string.</param>
        public Task<AudioServiceResult> EnqueueAudioAsync(IVoiceChannel voiceChannel, string request, string requester, CancellationToken cancellationToken = default);

        /// <summary>
        /// Appends the audio to the playlist of the voice channel.
        /// </summary>
        /// <param name="channelDto"></param>
        /// <param name="requestDto">The audio request DTO.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> EnqueueAudioAsync(IVoiceChannel voiceChannel, AudioRequestDTO requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skips the requested number of songs from the queue.
        /// </summary>
        /// <param name="voiceChannel">The channel in which the skip is performed.</param>
        /// <param name="count">The requested number of skips.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> SkipAudioAsync(IVoiceChannel voiceChannel, int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the audio queue.
        /// </summary>
        /// <param name="voiceChannel">The channel for which the audio queue is requested.</param>
        /// <returns></returns>
        public Task<IEnumerable<AudioItemDTO>> GetAudioQueueAsync(IVoiceChannel voiceChannel, CancellationToken cancellationToken = default);
    }
}
