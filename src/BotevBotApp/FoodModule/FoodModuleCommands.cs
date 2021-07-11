using BotevBotApp.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp.FoodModule
{
    public class FoodModuleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFoodService foodService;

        public FoodModuleCommands(IFoodService foodService)
        {
            this.foodService = foodService;
        }

        [Command("food")]
        public async Task GetRandomFood() => await ReplyAsync(message: await foodService.GetRandomFoodAsync());
    }
}
