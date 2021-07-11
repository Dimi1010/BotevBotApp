using BotevBotApp.FoodModule.Services;
using BotevBotApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BotevBotApp.FoodModule
{
    public static class ModuleSetup
    {
        public static IServiceCollection AddFoodModule(this IServiceCollection services)
        {
            services.AddTransient<IModuleInfo, FoodModuleInfo>();
            services.AddOptions<FoodServiceOptions>().BindConfiguration("FoodServiceOptions");

            services.AddTransient<IFoodService, WeightedFoodService>();

            return services;
        }
    }
}
