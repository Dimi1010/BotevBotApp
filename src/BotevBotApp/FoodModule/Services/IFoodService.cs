using System.Threading;
using System.Threading.Tasks;

namespace BotevBotApp.Services
{
    public interface IFoodService
    {
        Task<string> GetRandomFoodAsync(CancellationToken cancellationToken = default);
    }
}