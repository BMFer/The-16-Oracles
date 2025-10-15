using The16Oracles.DAOA.Models.Game;

namespace The16Oracles.DAOA.Interfaces;

public interface IOracleGameService
{
    Task<GameResponse<Player>> CreatePlayerAsync(CreatePlayerRequest request);
    Task<GameResponse<Player>> GetPlayerAsync(string discordUserId);
    Task<GameResponse<Player>> SubscribeToOracleAsync(SubscribeOracleRequest request);
    Task<GameResponse<Player>> UnsubscribeFromOracleAsync(string discordUserId, string oracleName);
    Task<GameResponse<Battle>> CreateBattleAsync(CreateBattleRequest request);
    Task<GameResponse<Battle>> ExecuteBattleAsync(string battleId);
    Task<GameResponse<List<LeaderboardEntry>>> GetLeaderboardAsync(int limit = 10);
    Task<GameResponse<List<OracleDefinition>>> GetAvailableOraclesAsync();
    Task<GameResponse<Player>> ClaimDailyBonusAsync(string discordUserId);
}
