using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotevBotApp
{
    public class CommandHandler : ICommandHandler
    {
        private readonly ILogger<CommandHandler> logger;
        private readonly CommandOptions options;
        private readonly IServiceProvider services;
        private readonly CommandService commandService;
        private readonly DiscordSocketClient socketClient;

        public CommandHandler(ILogger<CommandHandler> logger, IOptions<CommandOptions> options, IServiceProvider services, CommandService commandService, DiscordSocketClient socketClient)
        {
            this.logger = logger;
            this.options = options.Value;
            this.services = services;
            this.commandService = commandService;
            this.socketClient = socketClient;

            this.commandService.CommandExecuted += CommandExecutedAsync;
            this.socketClient.MessageReceived += MessageReceivedAsync;
        }

        /// <inheritdoc/>
        public async Task InitializeModulesAsync()
        {
            logger.LogInformation("Initializing default modules.");
            await commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
        }

        /// <inheritdoc/>
        public async Task InitializeModulesAsync(IEnumerable<Type> modules)
        {
            logger.LogInformation("Initializing modules.");
            foreach (var module in modules)
            {
                logger.LogDebug($"Initializing module: {module}");
                await commandService.AddModuleAsync(module, services);
            }
        }

        /// <inheritdoc/>
        public async Task InitializeModulesAsync(IEnumerable<Assembly> assemblies)
        {
            logger.LogInformation("Initializing modules.");
            foreach (var assembly in assemblies)
            {
                logger.LogDebug($"Initializing modules from assembly: {assembly}");
                await commandService.AddModulesAsync(assembly, services);
            }
        }

        /// <summary>
        /// Logic for recognising if a message is a command.
        /// </summary>
        /// <param name="rawMessage">The received message.</param>
        /// <returns>A task representing the operation.</returns>
        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            logger.LogTrace($"Received message: {rawMessage}");
            
            // Ingores system messages.
            if (rawMessage is not SocketUserMessage message || message.Source != MessageSource.User)
            {
                logger.LogTrace($"Message ignored: {rawMessage}");
                return;
            }

            if (!PrefixCheck(message, out var prefixEndPosition)) return;

            // Command execution.
            var context = new SocketCommandContext(socketClient, message);
            logger.LogDebug($"Executing command from message {message}");
            await commandService.ExecuteAsync(context, prefixEndPosition, services);
        }

        /// <summary>
        /// Contains prefix check logic.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <param name="prefixEndPosition">The positin on which the prefix ends.</param>
        /// <returns>True if the message contains a prefix, false otherwise.</returns>
        private bool PrefixCheck(SocketUserMessage message, out int prefixEndPosition)
        {
            logger.LogTrace($"Performing prefix check on message: {message}");
            prefixEndPosition = 0;
            if (message.HasMentionPrefix(socketClient.CurrentUser, ref prefixEndPosition)
                || message.HasStringPrefix(options.Prefix, ref prefixEndPosition))
            {
                logger.LogTrace($"Prefix check on message {message} succeeded.");
                return true;
            }

            logger.LogTrace($"Prefix check on message {message} failed.");
            return false;
        }

        /// <summary>
        /// Handles post-execution command logic.
        /// </summary>
        /// <param name="command">Information about the command that was executed.</param>
        /// <param name="context">The context in which the command was executed.</param>
        /// <param name="result">The result of the command.</param>
        /// <returns>A task representing the operation.</returns>
        private Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // The command is unspecified, indicating search failure.
            if (!command.IsSpecified)
            {
                logger.LogTrace($"Unspecified command: {context.Message}");
                return Task.CompletedTask;
            }

            // The command executed successfully.
            if (result.IsSuccess)
            {
                logger.LogTrace($"Command successful: {command.Value?.Name}");
                return Task.CompletedTask;
            }
            // The command executed unsucessfully.
            else
            {
                return HandleErrorInExecutionAsync(command, context, result);
            }
        }

        /// <summary>
        /// Internal helper method to avoid the creating an async state machine for CommandExecutedAsync's hot paths.
        /// </summary>
        /// <param name="command">Forwarded command info.</param>
        /// <param name="context">Forwarded command context.</param>
        /// <param name="result">Forwarded command result.</param>
        /// <returns>A task representing the operation.</returns>
        private async Task HandleErrorInExecutionAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            logger.LogTrace($"Command {command.Value?.Name} failed with result: {result}");
            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}
