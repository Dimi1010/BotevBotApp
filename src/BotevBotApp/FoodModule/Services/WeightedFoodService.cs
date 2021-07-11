using BotevBotApp.Services;
using BotevBotApp.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.FoodModule.Services
{
    internal class WeightedFoodService : IFoodService
    {
        private readonly FoodServiceOptions options;
        private IList<WeightedFood> foods = null;

        private object loadingLock = new();
        private Task loadingTask = null;
        private bool loaded = false;

        public WeightedFoodService(IOptionsMonitor<FoodServiceOptions> options)
        {
            this.options = options.CurrentValue;
        }

        /// <inheritdoc/>
        public Task<string> GetRandomFoodAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<string>(cancellationToken);
            return loaded ? Task.FromResult(GetRandomFoodName()) : GetRandomFoodUnloadedAsync(cancellationToken);
        }

        /// <summary>
        /// Potentially loads the food data and selects a random food.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        private async Task<string> GetRandomFoodUnloadedAsync(CancellationToken cancellationToken = default)
        {
            await MaybeLoadAsync(cancellationToken);
            return GetRandomFoodName();
        }

        /// <summary>
        /// Get a random food name from the loaded foods.
        /// </summary>
        /// <returns>The name of the chosen food.</returns>
        private string GetRandomFoodName()
        {
            return foods.RandomElementByWeight(x => x.SelectionWeight).Name;
        }

        /// <summary>
        /// Asynchronously loads the data if it has not been loaded.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        private Task MaybeLoadAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);
            lock (loadingLock)
            {
                return loadingTask ??= LoadDataAsync();
            }
        }

        /// <summary>
        /// Asynchronously loads the data.
        /// </summary>
        /// <returns>A task representing the operation.</returns>
        private async Task LoadDataAsync()
        {
            string jsonString = await File.ReadAllTextAsync(options.DataSource);
            foods = JsonSerializer.Deserialize<List<WeightedFood>>(jsonString);
            loaded = true;
        }
    }
}
