﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule.Playback
{
    public abstract class AudioPlayback
    {
        /// <summary>
        /// Gets the stream on which the playback will be outputted.
        /// </summary>
        public Stream AudioOutputStream { get; protected set; }

        /// <summary>
        /// Starts outputting the playback on the AudioOutputStream.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task representing the playback operation.<br/>
        /// The task completes when the entire playback is written to the output stream.
        /// </returns>
        public abstract Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="CachedAudioPlayback"/>.
        /// </summary>
        /// <returns>A new <see cref="CachedAudioPlayback"/> instance made from the current instance.</returns>
        public virtual CachedAudioPlayback WithCache() => new CachedAudioPlayback(this);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="DecodingAudioPlayback"/> with default options.
        /// </summary>
        /// <returns>A new <see cref="DecodingAudioPlayback"/> instance made from the current instance.</returns>
        public virtual DecodingAudioPlayback WithDecoding() => WithDecoding(DecodingAudioPlaybackOptions.Default);

        /// <summary>
        /// Wraps the current <see cref="AudioPlayback"/> instance into <see cref="DecodingAudioPlayback"/>.
        /// </summary>
        /// <param name="options">Options with which to construct <see cref="DecodingAudioPlayback"/>.</param>
        /// <returns>A new <see cref="DecodingAudioPlayback"/> instance made from the current instance.</returns>
        public virtual DecodingAudioPlayback WithDecoding(DecodingAudioPlaybackOptions options) => new DecodingAudioPlayback(this, options);
    }
}
