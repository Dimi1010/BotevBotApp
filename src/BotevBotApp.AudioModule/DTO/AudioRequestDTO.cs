namespace BotevBotApp.AudioModule.DTO
{
    public record AudioRequestDTO
    {
        /// <summary>
        /// The request string.
        /// </summary>
        public string Request { get; init; }

        /// <summary>
        /// The name of the requester.
        /// </summary>
        public string Requester { get; init; }
    }
}
