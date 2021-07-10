using Microsoft.Extensions.Logging;
using System;

namespace BotevBotApp.AudioModule.Requests
{
    /// <summary>
    /// Represents an audio request to a remote source.
    /// </summary>
    public abstract class RemoteAudioRequest : AudioRequest
    {
        protected Uri Url { get; private set; }

        protected RemoteAudioRequest(Uri url, string requester, ILogger<RemoteAudioRequest> logger) : base(requester, logger)
        {
            if (!ValidateUrl(url))
                throw new InvalidUrlException($"The provided url: {url} is invalid.");
            Url = url;
        }

        /// <summary>
        /// Validates the url.
        /// </summary>
        /// <param name="url">The url to validate.</param>
        /// <returns>True if the url is valid, false otherwise.</returns>
        protected virtual bool ValidateUrl(Uri url)
        {
            Logger.LogTrace($"Validating url: {url}");
            return true;
        }
    }


    [Serializable]
    public class InvalidUrlException : Exception
    {
        public InvalidUrlException() { }
        public InvalidUrlException(string message) : base(message) { }
        public InvalidUrlException(string message, Exception inner) : base(message, inner) { }
        protected InvalidUrlException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
