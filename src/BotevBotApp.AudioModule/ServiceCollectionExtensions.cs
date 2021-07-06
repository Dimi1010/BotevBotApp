using BotevBotApp.AudioModule.Playback;
using BotevBotApp.AudioModule.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp.AudioModule
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAudioModule(this IServiceCollection services)
        {
            // Contains information about the module.
            services.AddTransient<IModuleInfo, AudioModuleInfo>();

            services.AddTransient<IAudioClientWorkerFactory, AudioClientWorkerFactory>();
            
            // TODO: Add request parsers here.

            // TODO: Maybe make this transient?
            services.AddSingleton<IRequestParserService, RequestParserService>();
            services.AddSingleton<IAudioService, AudioService>();

            return services;
        }
    }
}
