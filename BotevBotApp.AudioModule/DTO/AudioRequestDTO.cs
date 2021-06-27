using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp.Domain.AudioModule.DTO
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
