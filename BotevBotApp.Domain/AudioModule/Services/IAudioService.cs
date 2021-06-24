using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotevBotApp.Domain.AudioModule.DTO;
using System.Threading;

namespace BotevBotApp.Domain.AudioModule.Services
{
    public interface IAudioService
    {
        /// <summary>
        /// Appends the audio to the playlist of the voice channel.
        /// </summary>
        /// <param name="channelDto"></param>
        /// <param name="request">The request string.</param>
        public Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, string request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Appends the audio to the playlist of the voice channel.
        /// </summary>
        /// <param name="channelDto"></param>
        /// <param name="requestDto">The audio request DTO.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> EnqueueAudioAsync(AudioVoiceChannelDTO channelDto, AudioRequestDTO requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skips the requested number of songs from the queue.
        /// </summary>
        /// <param name="channel">The channel in which the skip is performed.</param>
        /// <param name="count">The requested number of skips.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> SkipAudioAsync(AudioVoiceChannelDTO channelDto, int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the audio playback in the channel.
        /// </summary>
        /// <param name="channel">The channel in which the playback is to be stopped.</param>
        /// <returns></returns>
        public Task<AudioServiceResult> StopAudioAsync(AudioVoiceChannelDTO channelDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the audio queue.
        /// </summary>
        /// <param name="channel">The channel for which the audio queue is requested.</param>
        /// <returns></returns>
        public Task<IEnumerable<AudioItemDTO>> GetAudioQueueAsync(AudioVoiceChannelDTO channelDto, CancellationToken cancellationToken = default);
    }
}
