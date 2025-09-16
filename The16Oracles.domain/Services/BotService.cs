using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    public interface IBotService
    {
        Task<DiscordBot> GetDiscordBotAsync(Discord discord);
    }

    public class BotService : IBotService
    {
        public async Task<DiscordBot> GetDiscordBotAsync(Discord discord)
        {
            return await Task.Run(() => {
                return new DiscordBot(discord);
             });
        }
    }
}
