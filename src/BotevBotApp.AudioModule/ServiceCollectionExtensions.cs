using BotevBotApp.AudioModule.Playback;
using BotevBotApp.AudioModule.Requests;
using BotevBotApp.AudioModule.Requests.Parsers;
using BotevBotApp.AudioModule.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BotevBotApp.AudioModule
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAudioModule(this IServiceCollection services)
        {
            // Contains information about the module.
            services.AddTransient<IModuleInfo, AudioModuleInfo>();

            services.AddTransient<IAudioClientWorkerFactory, AudioClientWorkerFactory>();

            // TODO: Add request factories here.
            services.AddTransient<YoutubeAudioRequestFactory>();
#if DEBUG
            services.AddTransient<DebugAudioRequestFactory>();
#endif

            // TODO: Add request parsers here.
            services.AddTransient<IRequestParser, YoutubeRequestParser>();
#if DEBUG
            services.AddTransient<IRequestParser, DebugRequestParser>();
#endif

            // TODO: Maybe make this transient?
            services.AddSingleton<IRequestParserService, RequestParserService>();
            services.AddSingleton<IAudioService, AudioService>();

            return services;
        }
    }
}
