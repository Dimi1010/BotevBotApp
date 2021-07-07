using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp.CommandModules
{
    [RequireOwner]
    public class DebugModule : ModuleBase<SocketCommandContext>
    {
        private readonly ISet<int> testSet = new HashSet<int>();

        [Command("AddValue")]
        public Task AddToSetAsync(int value)
        {
            testSet.Add(value);
            return ReplyAsync($"Value added: {value}");
        }

        [Command("GetValues")]
        public Task GetValuesAsync()
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = Context.Client.CurrentUser.Username,
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                },
                Title = "Set Values",
            };
            foreach (var item in testSet)
            {
                builder.AddField("Value", item);
            }
            return ReplyAsync(embed: builder.Build());
        }
    }
}
