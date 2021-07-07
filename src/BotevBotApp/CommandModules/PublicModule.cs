using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp.CommandModules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task PingAsync() => ReplyAsync("Pong!");
    }
}
