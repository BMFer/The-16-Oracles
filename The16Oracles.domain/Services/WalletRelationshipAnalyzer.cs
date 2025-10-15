using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    /// <summary>
    /// Interface for the wallet relationship analyzer service
    /// </summary>
    public interface IWalletRelationshipAnalyzer
    {
        Task<AnalyzeWalletsResponse> AnalyzeWalletsAsync(AnalyzeWalletsRequest request);
        Task<WalletProfile?> GetWalletProfileAsync(string address, string network = "devnet");
        Task<List<WalletRelationship>> FindRelationshipsAsync(List<WalletProfile> wallets, AnalysisConfiguration config);
    }

    /// <summary>
    /// Service for analyzing wallet relationships using Solana and SPL Token data
    /// Calls The16Oracles.DAOA endpoints to retrieve wallet and token information
    /// </summary>
    public class WalletRelationshipAnalyzer : IWalletRelationshipAnalyzer
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public WalletRelationshipAnalyzer(string baseUrl = "https://localhost:5001")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Main method to analyze relationships between multiple wallets
        /// </summary>
        public async Task<AnalyzeWalletsResponse> AnalyzeWalletsAsync(AnalyzeWalletsRequest request)
        {
            var startTime = DateTime.UtcNow;
            var response = new AnalyzeWalletsResponse { Success = true };
            var wallets = new List<WalletProfile>();
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // Validate request
                if (request.WalletAddresses == null || request.WalletAddresses.Count == 0)
                {
                    return new AnalyzeWalletsResponse
                    {
                        Success = false,
                        Message = "No wallet addresses provided"
                    };
                }

                if (request.WalletAddresses.Count > request.Configuration.MaxWalletsToAnalyze)
                {
                    return new AnalyzeWalletsResponse
                    {
                        Success = false,
                        Message = $"Too many wallets. Maximum allowed: {request.Configuration.MaxWalletsToAnalyze}"
                    };
                }

                // Fetch wallet profiles
                foreach (var address in request.WalletAddresses)
                {
                    try
                    {
                        var profile = await GetWalletProfileAsync(address, request.Network ?? "devnet");
                        if (profile != null)
                        {
                            // Filter inactive wallets if configured
                            if (request.Configuration.IncludeInactiveWallets || profile.Metadata.IsActive)
                            {
                                wallets.Add(profile);
                            }
                            else
                            {
                                warnings.Add($"Wallet {address} is inactive and was excluded from analysis");
                            }
                        }
                        else
                        {
                            warnings.Add($"Could not retrieve profile for wallet {address}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error fetching wallet {address}: {ex.Message}");
                    }
                }

                if (wallets.Count < 2)
                {
                    return new AnalyzeWalletsResponse
                    {
                        Success = false,
                        Message = "Need at least 2 valid wallets to analyze relationships",
                        Warnings = warnings,
                        Errors = errors
                    };
                }

                // Find relationships
                var relationships = await FindRelationshipsAsync(wallets, request.Configuration);

                // Identify clusters
                var clusters = IdentifyClusters(wallets, relationships, request.Configuration);

                // Build analysis result
                var analysis = new WalletClusterAnalysis
                {
                    Wallets = wallets,
                    Relationships = relationships,
                    Clusters = clusters,
                    AnalyzedAt = DateTime.UtcNow,
                    Statistics = CalculateStatistics(wallets, relationships, clusters, startTime)
                };

                response.Data = analysis;
                response.Message = $"Successfully analyzed {wallets.Count} wallets, found {relationships.Count} relationships and {clusters.Count} clusters";
                response.Warnings = warnings;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                return new AnalyzeWalletsResponse
                {
                    Success = false,
                    Message = $"Analysis failed: {ex.Message}",
                    Errors = errors.Concat(new[] { ex.ToString() }).ToList(),
                    Warnings = warnings
                };
            }
        }

        /// <summary>
        /// Get comprehensive profile for a wallet including SOL balance and token holdings
        /// </summary>
        public async Task<WalletProfile?> GetWalletProfileAsync(string address, string network = "devnet")
        {
            try
            {
                var profile = new WalletProfile
                {
                    Address = address,
                    LastUpdated = DateTime.UtcNow
                };

                // Get SOL balance
                var balanceResult = await GetSolBalanceAsync(address, network);
                if (balanceResult.HasValue)
                {
                    profile.SolBalance = balanceResult.Value;
                }

                // Get token accounts
                var tokenAccounts = await GetTokenAccountsAsync(address, network);
                if (tokenAccounts != null)
                {
                    profile.TokenHoldings = tokenAccounts;
                }

                // Get transaction history metadata
                var transactionMetadata = await GetTransactionMetadataAsync(address, network);
                if (transactionMetadata != null)
                {
                    profile.Metadata = transactionMetadata;
                }

                profile.Metadata.UniqueTokenCount = profile.TokenHoldings.Count;
                profile.Metadata.IsActive = profile.Metadata.TotalTransactions > 0 &&
                                           profile.Metadata.LastTransactionDate > DateTime.UtcNow.AddDays(-30);

                return profile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting wallet profile for {address}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find relationships between wallets using pattern matching
        /// </summary>
        public async Task<List<WalletRelationship>> FindRelationshipsAsync(List<WalletProfile> wallets, AnalysisConfiguration config)
        {
            var relationships = new List<WalletRelationship>();

            // Compare each pair of wallets
            for (int i = 0; i < wallets.Count; i++)
            {
                for (int j = i + 1; j < wallets.Count; j++)
                {
                    var wallet1 = wallets[i];
                    var wallet2 = wallets[j];

                    var relationship = await AnalyzeWalletPairAsync(wallet1, wallet2, config);

                    if (relationship.RelationshipScore >= config.MinimumRelationshipScore)
                    {
                        relationships.Add(relationship);
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Analyze relationship between a pair of wallets
        /// </summary>
        private async Task<WalletRelationship> AnalyzeWalletPairAsync(WalletProfile wallet1, WalletProfile wallet2, AnalysisConfiguration config)
        {
            var relationship = new WalletRelationship
            {
                Wallet1Address = wallet1.Address,
                Wallet2Address = wallet2.Address,
                AnalyzedAt = DateTime.UtcNow
            };

            var evidence = new List<RelationshipEvidence>();
            decimal totalScore = 0m;

            // Pattern 1: Shared tokens
            var sharedTokens = wallet1.TokenHoldings
                .Select(t => t.TokenMintAddress)
                .Intersect(wallet2.TokenHoldings.Select(t => t.TokenMintAddress))
                .ToList();

            if (sharedTokens.Count >= config.MinimumSharedTokens)
            {
                var sharedTokenScore = Math.Min(1m, sharedTokens.Count / 10m) * 0.3m;
                totalScore += sharedTokenScore;

                evidence.Add(new RelationshipEvidence
                {
                    Type = "SharedTokens",
                    Description = $"Both wallets hold {sharedTokens.Count} common tokens",
                    Confidence = sharedTokenScore,
                    Details = new Dictionary<string, object>
                    {
                        { "SharedTokenCount", sharedTokens.Count },
                        { "SharedTokens", sharedTokens }
                    }
                });
            }

            // Pattern 2: Similar token balances
            var balanceSimilarity = CalculateBalanceSimilarity(wallet1, wallet2, sharedTokens);
            if (balanceSimilarity >= config.TokenBalanceSimilarityThreshold)
            {
                var balanceScore = balanceSimilarity * 0.25m;
                totalScore += balanceScore;

                evidence.Add(new RelationshipEvidence
                {
                    Type = "SimilarBalances",
                    Description = $"Token balances are {balanceSimilarity:P0} similar",
                    Confidence = balanceScore,
                    Details = new Dictionary<string, object>
                    {
                        { "SimilarityScore", balanceSimilarity }
                    }
                });
            }

            // Pattern 3: Similar SOL balances
            var solBalanceDiff = Math.Abs(wallet1.SolBalance - wallet2.SolBalance);
            var avgSolBalance = (wallet1.SolBalance + wallet2.SolBalance) / 2;
            if (avgSolBalance > 0)
            {
                var solSimilarity = 1m - (solBalanceDiff / avgSolBalance);
                if (solSimilarity > 0.8m)
                {
                    var solScore = solSimilarity * 0.15m;
                    totalScore += solScore;

                    evidence.Add(new RelationshipEvidence
                    {
                        Type = "SimilarSolBalance",
                        Description = $"SOL balances are within {(1 - solSimilarity):P1} of each other",
                        Confidence = solScore,
                        Details = new Dictionary<string, object>
                        {
                            { "Wallet1SolBalance", wallet1.SolBalance },
                            { "Wallet2SolBalance", wallet2.SolBalance },
                            { "SimilarityScore", solSimilarity }
                        }
                    });
                }
            }

            // Pattern 4: Temporal analysis
            if (wallet1.Metadata.FirstTransactionDate.HasValue && wallet2.Metadata.FirstTransactionDate.HasValue)
            {
                var timeDiff = Math.Abs((wallet1.Metadata.FirstTransactionDate.Value - wallet2.Metadata.FirstTransactionDate.Value).TotalDays);
                if (timeDiff < 7)
                {
                    var timeScore = (7 - (decimal)timeDiff) / 7 * 0.2m;
                    totalScore += timeScore;

                    evidence.Add(new RelationshipEvidence
                    {
                        Type = "TemporalProximity",
                        Description = $"Wallets created within {timeDiff:F0} days of each other",
                        Confidence = timeScore,
                        Details = new Dictionary<string, object>
                        {
                            { "DaysDifference", timeDiff },
                            { "Wallet1FirstTx", wallet1.Metadata.FirstTransactionDate },
                            { "Wallet2FirstTx", wallet2.Metadata.FirstTransactionDate }
                        }
                    });
                }
            }

            // Pattern 5: Activity correlation
            if (wallet1.Metadata.TotalTransactions > 0 && wallet2.Metadata.TotalTransactions > 0)
            {
                var txDiff = Math.Abs(wallet1.Metadata.TotalTransactions - wallet2.Metadata.TotalTransactions);
                var avgTx = (wallet1.Metadata.TotalTransactions + wallet2.Metadata.TotalTransactions) / 2m;
                var activitySimilarity = 1m - (txDiff / avgTx);

                if (activitySimilarity > 0.75m)
                {
                    var activityScore = activitySimilarity * 0.1m;
                    totalScore += activityScore;

                    evidence.Add(new RelationshipEvidence
                    {
                        Type = "SimilarActivity",
                        Description = $"Transaction counts are {activitySimilarity:P0} similar",
                        Confidence = activityScore,
                        Details = new Dictionary<string, object>
                        {
                            { "Wallet1TxCount", wallet1.Metadata.TotalTransactions },
                            { "Wallet2TxCount", wallet2.Metadata.TotalTransactions },
                            { "SimilarityScore", activitySimilarity }
                        }
                    });
                }
            }

            relationship.RelationshipScore = totalScore;
            relationship.Evidence = evidence;
            relationship.Type = DetermineRelationshipType(totalScore, evidence);

            return relationship;
        }

        /// <summary>
        /// Calculate similarity of token balances between two wallets
        /// </summary>
        private decimal CalculateBalanceSimilarity(WalletProfile wallet1, WalletProfile wallet2, List<string> sharedTokens)
        {
            if (!sharedTokens.Any())
                return 0m;

            decimal totalSimilarity = 0m;
            int comparedTokens = 0;

            foreach (var tokenMint in sharedTokens)
            {
                var balance1 = wallet1.TokenHoldings.FirstOrDefault(t => t.TokenMintAddress == tokenMint)?.Balance ?? 0;
                var balance2 = wallet2.TokenHoldings.FirstOrDefault(t => t.TokenMintAddress == tokenMint)?.Balance ?? 0;

                if (balance1 > 0 || balance2 > 0)
                {
                    var diff = Math.Abs(balance1 - balance2);
                    var avg = (balance1 + balance2) / 2;
                    var similarity = avg > 0 ? 1m - (diff / avg) : 0m;
                    totalSimilarity += Math.Max(0, similarity);
                    comparedTokens++;
                }
            }

            return comparedTokens > 0 ? totalSimilarity / comparedTokens : 0m;
        }

        /// <summary>
        /// Determine relationship type based on score and evidence
        /// </summary>
        private RelationshipType DetermineRelationshipType(decimal score, List<RelationshipEvidence> evidence)
        {
            if (score >= 0.7m)
                return RelationshipType.LikelyRelated;

            if (score >= 0.5m)
                return RelationshipType.PossiblyRelated;

            // Check for specific patterns
            var hasSharedTokens = evidence.Any(e => e.Type == "SharedTokens");
            var hasSimilarBalances = evidence.Any(e => e.Type == "SimilarBalances");
            var hasTemporalProximity = evidence.Any(e => e.Type == "TemporalProximity");

            if (hasSharedTokens && hasSimilarBalances && hasTemporalProximity)
                return RelationshipType.SuspiciousActivity;

            if (hasSharedTokens)
                return RelationshipType.SharedTokens;

            if (hasSimilarBalances)
                return RelationshipType.MirrorTrading;

            return RelationshipType.None;
        }

        /// <summary>
        /// Identify clusters of related wallets
        /// </summary>
        private List<WalletCluster> IdentifyClusters(List<WalletProfile> wallets, List<WalletRelationship> relationships, AnalysisConfiguration config)
        {
            var clusters = new List<WalletCluster>();
            var processed = new HashSet<string>();
            var clusterId = 0;

            foreach (var wallet in wallets)
            {
                if (processed.Contains(wallet.Address))
                    continue;

                var cluster = new WalletCluster
                {
                    ClusterId = $"cluster-{++clusterId}",
                    WalletAddresses = new List<string> { wallet.Address }
                };

                // Find all related wallets
                var relatedWallets = new Queue<string>();
                relatedWallets.Enqueue(wallet.Address);
                processed.Add(wallet.Address);

                while (relatedWallets.Count > 0)
                {
                    var currentWallet = relatedWallets.Dequeue();

                    var connectedRelationships = relationships
                        .Where(r => (r.Wallet1Address == currentWallet || r.Wallet2Address == currentWallet) &&
                                   r.RelationshipScore >= config.MinimumRelationshipScore)
                        .ToList();

                    foreach (var rel in connectedRelationships)
                    {
                        var otherWallet = rel.Wallet1Address == currentWallet ? rel.Wallet2Address : rel.Wallet1Address;

                        if (!processed.Contains(otherWallet))
                        {
                            cluster.WalletAddresses.Add(otherWallet);
                            relatedWallets.Enqueue(otherWallet);
                            processed.Add(otherWallet);
                        }
                    }
                }

                if (cluster.WalletAddresses.Count > 1)
                {
                    // Calculate cluster statistics
                    var clusterRelationships = relationships
                        .Where(r => cluster.WalletAddresses.Contains(r.Wallet1Address) &&
                                   cluster.WalletAddresses.Contains(r.Wallet2Address))
                        .ToList();

                    cluster.AverageRelationshipScore = clusterRelationships.Any()
                        ? clusterRelationships.Average(r => r.RelationshipScore)
                        : 0m;

                    cluster.PrimaryRelationType = clusterRelationships
                        .GroupBy(r => r.Type)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? RelationshipType.None;

                    // Find shared tokens across all wallets in cluster
                    var clusterWallets = wallets.Where(w => cluster.WalletAddresses.Contains(w.Address)).ToList();
                    if (clusterWallets.Any())
                    {
                        var sharedTokens = clusterWallets
                            .Select(w => w.TokenHoldings.Select(t => t.TokenMintAddress).ToHashSet())
                            .Aggregate((h1, h2) => { h1.IntersectWith(h2); return h1; })
                            .ToList();

                        cluster.SharedTokens = sharedTokens;
                    }

                    cluster.Description = $"Cluster of {cluster.WalletAddresses.Count} wallets with {cluster.PrimaryRelationType} relationship pattern";

                    clusters.Add(cluster);
                }
            }

            return clusters;
        }

        /// <summary>
        /// Calculate analysis statistics
        /// </summary>
        private AnalysisStatistics CalculateStatistics(List<WalletProfile> wallets, List<WalletRelationship> relationships, List<WalletCluster> clusters, DateTime startTime)
        {
            return new AnalysisStatistics
            {
                TotalWalletsAnalyzed = wallets.Count,
                TotalRelationshipsFound = relationships.Count,
                TotalClustersFound = clusters.Count,
                AverageTokensPerWallet = wallets.Any() ? (decimal)wallets.Average(w => w.TokenHoldings.Count) : 0m,
                HighestRelationshipScore = relationships.Any() ? relationships.Max(r => r.RelationshipScore) : 0m,
                AnalysisDuration = DateTime.UtcNow - startTime
            };
        }

        #region DAOA API Calls

        /// <summary>
        /// Get SOL balance for a wallet
        /// </summary>
        private async Task<decimal?> GetSolBalanceAsync(string address, string network)
        {
            try
            {
                var requestBody = new
                {
                    address = address,
                    flags = new { url = network }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/solana/balance", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Parse output to extract balance (format: "X.XXXXXXXXX SOL")
                    string output = result?.output?.ToString() ?? "";
                    var match = Regex.Match(output, @"([\d.]+)\s*SOL");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var balance))
                    {
                        return balance;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get token accounts for a wallet
        /// </summary>
        private async Task<List<TokenHolding>> GetTokenAccountsAsync(string owner, string network)
        {
            try
            {
                var requestBody = new
                {
                    owner = owner,
                    flags = new { url = network, output = "json" }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/spl-token/accounts", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var holdings = new List<TokenHolding>();

                    // Parse JSON output
                    if (result?.data != null)
                    {
                        var data = result.data;
                        if (data is JArray array)
                        {
                            foreach (var item in array)
                            {
                                var holding = new TokenHolding
                                {
                                    TokenMintAddress = item["mint"]?.ToString() ?? "",
                                    AccountAddress = item["address"]?.ToString() ?? "",
                                    Balance = decimal.TryParse(item["amount"]?.ToString(), out var bal) ? bal : 0,
                                    Decimals = int.TryParse(item["decimals"]?.ToString(), out var dec) ? dec : 0
                                };

                                if (!string.IsNullOrEmpty(holding.TokenMintAddress))
                                {
                                    holdings.Add(holding);
                                }
                            }
                        }
                    }

                    return holdings;
                }

                return new List<TokenHolding>();
            }
            catch
            {
                return new List<TokenHolding>();
            }
        }

        /// <summary>
        /// Get transaction metadata for a wallet
        /// </summary>
        private async Task<WalletMetadata?> GetTransactionMetadataAsync(string address, string network)
        {
            try
            {
                var requestBody = new
                {
                    address = address,
                    limit = 100,
                    flags = new { url = network }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/solana/transaction-history", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var metadata = new WalletMetadata();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string output = result?.output?.ToString() ?? "";

                    // Parse transaction count from output
                    var lines = output.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                    metadata.TotalTransactions = lines.Count;

                    if (metadata.TotalTransactions > 0)
                    {
                        metadata.LastTransactionDate = DateTime.UtcNow.AddDays(-1);
                        metadata.FirstTransactionDate = DateTime.UtcNow.AddDays(-30);
                    }
                }

                return metadata;
            }
            catch
            {
                return new WalletMetadata();
            }
        }

        #endregion
    }
}
