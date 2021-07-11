namespace BotevBotApp.AudioModule.DTO
{
    public record AudioServiceResult
    {
        public static AudioServiceResult Success => new AudioServiceResult();
        public static AudioServiceResult ErrorFromReason(string reason) => new AudioServiceResult { IsError = true, ErrorReason = reason };

        public bool IsSuccess => !IsError;

        public bool IsError { get; init; } = false;
        public string ErrorReason { get; init; } = null;
    }
}
