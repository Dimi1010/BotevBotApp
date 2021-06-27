namespace BotevBotApp.AudioModule.DTO
{
    public record AudioItemDTO
    {
        /// <summary>
        /// Gets the source of the request.
        /// </summary>
        public string Source { get; init; }

        /// <summary>
        /// Gets the name of the requester.
        /// </summary>
        public string Requester { get; init; }

        /// <summary>
        /// Gets the name of the audio.
        /// </summary>
        public string Name { get; init; }
    }
}
