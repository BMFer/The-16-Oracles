using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    /// <summary>
    /// Interface for token creator analyzer
    /// </summary>
    public interface ITokenCreatorAnalyzer
    {
        Task<GetTokenCreatorsResponse> GetTokenCreatorsAsync(GetTokenCreatorsRequest request);
        Task<TokenCreatorInfo?> GetTokenCreatorInfoAsync(string tokenMintAddress, string network = "devnet");
        Task<GroupTokensByCreatorResponse> GroupTokensByCreatorAsync(GroupTokensByCreatorRequest request);
        Task<List<TokenAccountCreatorInfo>> GetAccountCreatorInfoAsync(List<string> accountAddresses, string network = "devnet");
    }

    /// <summary>
    /// Service for analyzing token creators and mint authorities
    /// Uses DAOA endpoints to retrieve token metadata and creator information
    /// </summary>
    public class TokenCreatorAnalyzer : ITokenCreatorAnalyzer
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TokenCreatorAnalyzer(string baseUrl = "https://localhost:5001")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(3)
            };
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get token creators from a list of account addresses
        /// </summary>
        public async Task<GetTokenCreatorsResponse> GetTokenCreatorsAsync(GetTokenCreatorsRequest request)
        {
            var startTime = DateTime.UtcNow;
            var response = new GetTokenCreatorsResponse { Success = true };
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // Validate request
                if (request.AccountAddresses == null || request.AccountAddresses.Count == 0)
                {
                    return new GetTokenCreatorsResponse
                    {
                        Success = false,
                        Message = "No account addresses provided"
                    };
                }

                var result = new TokenCreatorsResult
                {
                    AnalyzedAt = DateTime.UtcNow
                };

                // Get account information for all addresses
                var accounts = await GetAccountCreatorInfoAsync(request.AccountAddresses, request.Network);

                // Filter zero balances if requested
                if (!request.IncludeZeroBalances)
                {
                    accounts = accounts.Where(a => a.Balance > 0).ToList();
                }

                result.Accounts = accounts;

                // Get unique creators
                var uniqueCreators = new Dictionary<string, TokenCreatorInfo>();
                foreach (var account in accounts)
                {
                    if (account.CreatorInfo?.MintAuthority != null)
                    {
                        var creatorAddress = account.CreatorInfo.MintAuthority;
                        if (!uniqueCreators.ContainsKey(creatorAddress))
                        {
                            uniqueCreators[creatorAddress] = account.CreatorInfo;
                        }
                    }
                }

                result.UniqueCreators = uniqueCreators;

                // Calculate statistics
                result.Statistics = new TokenCreatorStatistics
                {
                    TotalAccountsAnalyzed = request.AccountAddresses.Count,
                    TotalUniqueTokens = accounts.Select(a => a.TokenMintAddress).Distinct().Count(),
                    TotalUniqueCreators = uniqueCreators.Count,
                    AccountsWithNoAuthority = accounts.Count(a => a.CreatorInfo?.MintAuthority == null),
                    AccountsWithSameAuthority = accounts
                        .Where(a => a.CreatorInfo?.MintAuthority != null)
                        .GroupBy(a => a.CreatorInfo!.MintAuthority)
                        .Count(g => g.Count() > 1),
                    AnalysisDuration = DateTime.UtcNow - startTime
                };

                response.Data = result;
                response.Message = $"Successfully analyzed {accounts.Count} accounts, found {uniqueCreators.Count} unique creators";
                response.Warnings = warnings;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                return new GetTokenCreatorsResponse
                {
                    Success = false,
                    Message = $"Analysis failed: {ex.Message}",
                    Errors = errors.Concat(new[] { ex.ToString() }).ToList(),
                    Warnings = warnings
                };
            }
        }

        /// <summary>
        /// Get creator information for a list of account addresses
        /// </summary>
        public async Task<List<TokenAccountCreatorInfo>> GetAccountCreatorInfoAsync(List<string> accountAddresses, string network = "devnet")
        {
            var accountInfoList = new List<TokenAccountCreatorInfo>();

            foreach (var accountAddress in accountAddresses)
            {
                try
                {
                    // First, get account details to find the token mint
                    var accountInfo = await GetAccountDetailsAsync(accountAddress, network);

                    if (accountInfo != null)
                    {
                        // Then get creator info for that token mint
                        if (!string.IsNullOrEmpty(accountInfo.TokenMintAddress))
                        {
                            var creatorInfo = await GetTokenCreatorInfoAsync(accountInfo.TokenMintAddress, network);
                            accountInfo.CreatorInfo = creatorInfo;
                        }

                        accountInfoList.Add(accountInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing account {accountAddress}: {ex.Message}");
                }
            }

            return accountInfoList;
        }

        /// <summary>
        /// Get account details including token mint and balance
        /// </summary>
        private async Task<TokenAccountCreatorInfo?> GetAccountDetailsAsync(string accountAddress, string network)
        {
            try
            {
                var requestBody = new
                {
                    address = accountAddress,
                    flags = new { url = network, output = "json" }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/spl-token/display", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Parse the data field which contains JSON output
                    if (result?.data != null)
                    {
                        var data = result.data;

                        // Check if this is a token account
                        if (data["address"] != null)
                        {
                            decimal.TryParse(data["tokenAmount"]?["uiAmount"]?.ToString(), out decimal bal);
                            return new TokenAccountCreatorInfo
                            {
                                AccountAddress = accountAddress,
                                TokenMintAddress = data["mint"]?.ToString() ?? "",
                                Owner = data["owner"]?.ToString() ?? "",
                                Balance = bal
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting account details for {accountAddress}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get creator/mint authority information for a token
        /// </summary>
        public async Task<TokenCreatorInfo?> GetTokenCreatorInfoAsync(string tokenMintAddress, string network = "devnet")
        {
            try
            {
                var requestBody = new
                {
                    address = tokenMintAddress,
                    flags = new { url = network, output = "json" }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/spl-token/display", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Parse the data field which contains JSON output
                    if (result?.data != null)
                    {
                        var data = result.data;

                        // Check if this is a mint account
                        if (data["address"] != null)
                        {
                            int.TryParse(data["decimals"]?.ToString(), out int dec);
                            decimal.TryParse(data["supply"]?.ToString(), out decimal supply);

                            var creatorInfo = new TokenCreatorInfo
                            {
                                TokenMintAddress = tokenMintAddress,
                                MintAuthority = data["mintAuthority"]?.ToString(),
                                FreezeAuthority = data["freezeAuthority"]?.ToString(),
                                Decimals = dec,
                                Supply = supply,
                                IsInitialized = data["isInitialized"]?.ToString()?.ToLower() == "true",
                                RetrievedAt = DateTime.UtcNow
                            };

                            return creatorInfo;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token creator info for {tokenMintAddress}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Group tokens by their creator/mint authority
        /// </summary>
        public async Task<GroupTokensByCreatorResponse> GroupTokensByCreatorAsync(GroupTokensByCreatorRequest request)
        {
            try
            {
                // First get all token creators
                var creatorsRequest = new GetTokenCreatorsRequest
                {
                    AccountAddresses = request.AccountAddresses,
                    Network = request.Network,
                    IncludeZeroBalances = request.IncludeZeroBalances
                };

                var creatorsResponse = await GetTokenCreatorsAsync(creatorsRequest);

                if (!creatorsResponse.Success || creatorsResponse.Data == null)
                {
                    return new GroupTokensByCreatorResponse
                    {
                        Success = false,
                        Message = "Failed to get token creators",
                        Errors = creatorsResponse.Errors
                    };
                }

                // Group by creator
                var groupedByCreator = creatorsResponse.Data.Accounts
                    .Where(a => a.CreatorInfo?.MintAuthority != null)
                    .GroupBy(a => a.CreatorInfo!.MintAuthority!)
                    .Select(g => new TokensByCreator
                    {
                        CreatorAddress = g.Key,
                        Tokens = g.ToList(),
                        TotalTokenCount = g.Count(),
                        TotalValueHeld = g.Sum(a => a.Balance)
                    })
                    .Where(t => t.TotalTokenCount >= request.MinimumTokenCount)
                    .OrderByDescending(t => t.TotalTokenCount)
                    .ToList();

                return new GroupTokensByCreatorResponse
                {
                    Success = true,
                    Message = $"Found {groupedByCreator.Count} creators",
                    Data = groupedByCreator
                };
            }
            catch (Exception ex)
            {
                return new GroupTokensByCreatorResponse
                {
                    Success = false,
                    Message = $"Grouping failed: {ex.Message}",
                    Errors = new List<string> { ex.ToString() }
                };
            }
        }
    }
}
