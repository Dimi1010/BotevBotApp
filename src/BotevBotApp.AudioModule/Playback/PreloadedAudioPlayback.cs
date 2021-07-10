namespace BotevBotApp.AudioModule.Playback
{
    public class PreloadedAudioPlayback : CachedAudioPlayback
    {
        public PreloadedAudioPlayback(AudioPlayback innerPlayback) : base(innerPlayback)
        {
            CacheAudioStreamAsync();
        }

        /// <summary>
        /// Overrides the <see cref="AudioPlayback.WithPreloading()"/> method so it returns the current instance.
        /// </summary>
        /// <returns>The current instance.</returns>
        /// <remarks>
        /// As the current playback is already preloaded and cached, preloading it again is redundant.
        /// </remarks>
        public override PreloadedAudioPlayback WithPreloading()
        {
            return this;
        }
    }
}
