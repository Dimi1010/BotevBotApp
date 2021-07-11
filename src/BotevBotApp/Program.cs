using BotevBotApp.Extensions;
using BotevBotApp.FoodModule;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotevBotApp
{
    public class Program
    {
        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                            .ConfigureServices((hostContext, services) =>
                            {
                                var loginSection = hostContext.Configuration.GetSection("LoginOptions");
                                services.Configure<LoginOptions>(loginSection);

                                var logLevel = hostContext.Configuration.GetValue<LogLevel>("Logging:LogLevel:Default");

                                var commandSection = hostContext.Configuration.GetSection("CommandOptions");
                                services.Configure<CommandOptions>(commandSection);

                                services.AddSingleton(new CommandService(new CommandServiceConfig
                                {
                                    DefaultRunMode = RunMode.Async,
                                    CaseSensitiveCommands = commandSection.GetValue<bool>("CaseSensitiveCommands"),
                                    LogLevel = logLevel.ToLogSeverity(),
                                }));

                                services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                                {
                                    LogLevel = logLevel.ToLogSeverity(),
                                }));

                                // Adds the core module information.
                                services.AddTransient<IModuleInfo, CoreModuleInfo>();

                                services.AddFoodModule();

                                services.AddSingleton<ICommandHandler, CommandHandler>();
                                services.AddHostedService<DiscordWorker>();
                            });
        }
    }
}
