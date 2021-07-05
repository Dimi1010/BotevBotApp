using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp
{
    public sealed class DiscordWorker : IHostedService, IDisposable
    {
        private readonly ILogger<DiscordWorker> logger;
        private readonly DiscordSocketClient socketClient;
        private readonly ICommandHandler commandHandler;
        private readonly LoginOptions loginOptions;
        private bool disposedValue;

        public DiscordWorker(ILogger<DiscordWorker> logger, DiscordSocketClient socketClient, ICommandHandler commandHandler, IOptions<LoginOptions> loginOptions)
        {
            this.logger = logger;
            this.socketClient = socketClient;
            this.commandHandler = commandHandler;
            this.loginOptions = loginOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Starting Service...");
            await commandHandler.InitializeModulesAsync();
            await socketClient.LoginAsync(TokenType.Bot, loginOptions.Token);
            await socketClient.StartAsync();
            socketClient.Log += SocketClient_Log;
            logger.LogInformation("Service Started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Stopping service...");
            await socketClient.StopAsync();
            await socketClient.LogoutAsync();
            cancellationToken.ThrowIfCancellationRequested();
            socketClient.Log -= SocketClient_Log;
            logger.LogInformation("Service Stopped.");
        }

        private Task SocketClient_Log(LogMessage message)
        {
            // Exceptional control flow.
            if (message.Exception is not null)
            {
                logger.Log(message.Severity.ToLogLevel(), message.Exception, message.Message);
            }
            // Regular control flow.
            else
            {
                logger.Log(message.Severity.ToLogLevel(), message.Message);
            }
            return Task.CompletedTask;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    socketClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
