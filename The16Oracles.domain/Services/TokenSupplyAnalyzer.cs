using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    /// <summary>
    /// Interface for token supply concentration analyzer
    /// </summary>
    public interface ITokenSupplyAnalyzer
    {
        Task<AnalyzeTokenSupplyResponse> AnalyzeTokenSupplyAsync(AnalyzeTokenSupplyRequest request);
        Task<TokenSupplyInfo?> GetTokenSupplyAsync(string tokenMintAddress, string network = "devnet");
        Task<List<TokenHolder>> GetTopHoldersAsync(string tokenMintAddress, string network = "devnet", int maxHolders = 100);
        Task<SupplyConcentration> CalculateConcentrationAsync(List<TokenHolder> holders, decimal totalSupply);
    }

    /// <summary>
    /// Service for analyzing token supply concentration and identifying suspicious holders
    /// Uses DAOA endpoints to retrieve token supply and holder information
    /// </summary>
    public class TokenSupplyAnalyzer : ITokenSupplyAnalyzer
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IWalletRelationshipAnalyzer _walletAnalyzer;

        public TokenSupplyAnalyzer(string baseUrl = "https://localhost:5001")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _baseUrl = baseUrl;
            _walletAnalyzer = new WalletRelationshipAnalyzer(baseUrl);
        }

        /// <summary>
        /// Main method to analyze token supply concentration and identify suspicious holders
        /// </summary>
        public async Task<AnalyzeTokenSupplyResponse> AnalyzeTokenSupplyAsync(AnalyzeTokenSupplyRequest request)
        {
            var startTime = DateTime.UtcNow;
            var response = new AnalyzeTokenSupplyResponse { Success = true };
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.TokenMintAddress))
                {
                    return new AnalyzeTokenSupplyResponse
                    {
                        Success = false,
                        Message = "Token mint address is required"
                    };
                }

                var result = new TokenSupplyAnalysisResult
                {
                    AnalyzedAt = DateTime.UtcNow
                };

                // Step 1: Get token supply information
                var supplyInfo = await GetTokenSupplyAsync(request.TokenMintAddress, request.Network);
                if (supplyInfo == null)
                {
                    return new AnalyzeTokenSupplyResponse
                    {
                        Success = false,
                        Message = $"Could not retrieve supply information for token {request.TokenMintAddress}"
                    };
                }
                result.SupplyInfo = supplyInfo;

                // Step 2: Get top holders
                var topHolders = await GetTopHoldersAsync(
                    request.TokenMintAddress,
                    request.Network,
                    request.MaxHoldersToAnalyze);

                if (topHolders.Count == 0)
                {
                    warnings.Add("No token holders found");
                }

                // Filter by minimum holding percentage
                var filteredHolders = topHolders
                    .Where(h => h.PercentageOfSupply >= request.MinimumHoldingPercentage)
                    .ToList();

                result.TopHolders = filteredHolders;

                // Step 3: Calculate supply concentration metrics
                result.Concentration = await CalculateConcentrationAsync(filteredHolders, supplyInfo.TotalSupply);

                // Step 4: Analyze suspicious holders if requested
                if (request.AnalyzeSuspiciousHolders && filteredHolders.Any())
                {
                    var suspiciousHolders = await IdentifySuspiciousHoldersAsync(
                        filteredHolders,
                        supplyInfo,
                        request.Network,
                        request.IncludeRelationshipAnalysis,
                        request.WalletAnalysisConfig);

                    result.SuspiciousHolders = suspiciousHolders;

                    // Step 5: Identify suspicious clusters if relationship analysis is enabled
                    if (request.IncludeRelationshipAnalysis && suspiciousHolders.Count >= 2)
                    {
                        var suspiciousClusters = await IdentifySuspiciousClustersAsync(
                            suspiciousHolders,
                            request.WalletAnalysisConfig ?? new AnalysisConfiguration());

                        result.SuspiciousClusters = suspiciousClusters;
                    }
                }

                // Step 6: Calculate statistics
                result.Statistics = new SupplyAnalysisStatistics
                {
                    TotalHoldersAnalyzed = topHolders.Count,
                    SuspiciousHoldersFound = result.SuspiciousHolders.Count,
                    SuspiciousClustersFound = result.SuspiciousClusters.Count,
                    TotalSupplyAnalyzed = filteredHolders.Sum(h => h.Balance),
                    PercentageAnalyzed = filteredHolders.Sum(h => h.PercentageOfSupply),
                    AnalysisDuration = DateTime.UtcNow - startTime
                };

                response.Data = result;
                response.Message = $"Successfully analyzed token {request.TokenMintAddress}: " +
                                 $"{topHolders.Count} holders, {result.Concentration.RiskLevel} concentration risk, " +
                                 $"{result.SuspiciousHolders.Count} suspicious holders found";
                response.Warnings = warnings;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                return new AnalyzeTokenSupplyResponse
                {
                    Success = false,
                    Message = $"Analysis failed: {ex.Message}",
                    Errors = errors.Concat(new[] { ex.ToString() }).ToList(),
                    Warnings = warnings
                };
            }
        }

        /// <summary>
        /// Get token supply information from DAOA
        /// </summary>
        public async Task<TokenSupplyInfo?> GetTokenSupplyAsync(string tokenMintAddress, string network = "devnet")
        {
            try
            {
                var requestBody = new
                {
                    tokenAddress = tokenMintAddress,
                    flags = new { url = network }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/spl-token/supply", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string output = result?.output?.ToString() ?? "";

                    // Parse supply from output (format varies)
                    var match = Regex.Match(output, @"([\d.]+)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var supply))
                    {
                        return new TokenSupplyInfo
                        {
                            TokenMintAddress = tokenMintAddress,
                            TotalSupply = supply,
                            RetrievedAt = DateTime.UtcNow
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token supply: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get top holders of a token
        /// </summary>
        public async Task<List<TokenHolder>> GetTopHoldersAsync(string tokenMintAddress, string network = "devnet", int maxHolders = 100)
        {
            try
            {
                var holders = new List<TokenHolder>();

                // Get token supply first
                var supplyInfo = await GetTokenSupplyAsync(tokenMintAddress, network);
                if (supplyInfo == null)
                    return holders;

                // For now, we'll use the accounts endpoint to get holders
                // Note: This requires iterating through known wallets or using a different approach
                // In production, you'd want to use a token metadata service or indexer

                // This is a simplified implementation - in production you'd use:
                // - Helius API, QuickNode, or other indexing service
                // - On-chain program to scan all token accounts
                // - Pre-indexed database of token holders

                // For demo purposes, we'll create a method that can analyze provided wallet addresses
                // The user will need to provide wallet addresses to analyze

                return holders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top holders: {ex.Message}");
                return new List<TokenHolder>();
            }
        }

        /// <summary>
        /// Get holders from a list of wallet addresses
        /// This is a helper method since we can't easily enumerate all holders on-chain
        /// </summary>
        public async Task<List<TokenHolder>> GetHoldersFromWalletsAsync(
            string tokenMintAddress,
            List<string> walletAddresses,
            string network = "devnet")
        {
            var holders = new List<TokenHolder>();
            var supplyInfo = await GetTokenSupplyAsync(tokenMintAddress, network);

            if (supplyInfo == null)
                return holders;

            foreach (var walletAddress in walletAddresses)
            {
                try
                {
                    var profile = await _walletAnalyzer.GetWalletProfileAsync(walletAddress, network);
                    if (profile != null)
                    {
                        var tokenHolding = profile.TokenHoldings
                            .FirstOrDefault(t => t.TokenMintAddress == tokenMintAddress);

                        if (tokenHolding != null && tokenHolding.Balance > 0)
                        {
                            var holder = new TokenHolder
                            {
                                WalletAddress = walletAddress,
                                Balance = tokenHolding.Balance,
                                PercentageOfSupply = supplyInfo.TotalSupply > 0
                                    ? (tokenHolding.Balance / supplyInfo.TotalSupply) * 100
                                    : 0,
                                WalletProfile = profile
                            };
                            holders.Add(holder);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting holder info for {walletAddress}: {ex.Message}");
                }
            }

            // Sort by balance and assign ranks
            holders = holders.OrderByDescending(h => h.Balance).ToList();
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].Rank = i + 1;
            }

            return holders;
        }

        /// <summary>
        /// Calculate supply concentration metrics
        /// </summary>
        public async Task<SupplyConcentration> CalculateConcentrationAsync(List<TokenHolder> holders, decimal totalSupply)
        {
            var concentration = new SupplyConcentration
            {
                TotalSupply = totalSupply,
                TotalHolders = holders.Count
            };

            if (!holders.Any() || totalSupply == 0)
            {
                concentration.RiskLevel = ConcentrationRisk.VeryLow;
                concentration.RiskDescription = "No holders or supply data available";
                return concentration;
            }

            // Sort by balance descending
            var sortedHolders = holders.OrderByDescending(h => h.Balance).ToList();

            // Calculate top holder percentages
            if (sortedHolders.Count >= 1)
                concentration.Top1HolderPercentage = sortedHolders[0].PercentageOfSupply;

            if (sortedHolders.Count >= 5)
                concentration.Top5HoldersPercentage = sortedHolders.Take(5).Sum(h => h.PercentageOfSupply);

            if (sortedHolders.Count >= 10)
                concentration.Top10HoldersPercentage = sortedHolders.Take(10).Sum(h => h.PercentageOfSupply);

            if (sortedHolders.Count >= 20)
                concentration.Top20HoldersPercentage = sortedHolders.Take(20).Sum(h => h.PercentageOfSupply);

            // Calculate Gini coefficient (0 = perfect equality, 1 = perfect inequality)
            concentration.GiniCoefficient = CalculateGiniCoefficient(sortedHolders.Select(h => h.Balance).ToList());

            // Calculate Herfindahl-Hirschman Index (sum of squared market shares)
            concentration.HerfindahlIndex = sortedHolders.Sum(h => (h.PercentageOfSupply / 100) * (h.PercentageOfSupply / 100));

            // Calculate overall concentration score (0-100)
            concentration.ConcentrationScore = CalculateConcentrationScore(concentration);

            // Determine risk level
            concentration.RiskLevel = DetermineConcentrationRisk(concentration);
            concentration.RiskDescription = GetRiskDescription(concentration);

            return await Task.FromResult(concentration);
        }

        /// <summary>
        /// Identify suspicious holders based on multiple criteria
        /// </summary>
        private async Task<List<SuspiciousHolder>> IdentifySuspiciousHoldersAsync(
            List<TokenHolder> holders,
            TokenSupplyInfo supplyInfo,
            string network,
            bool includeRelationshipAnalysis,
            AnalysisConfiguration? config)
        {
            var suspiciousHolders = new List<SuspiciousHolder>();
            config ??= new AnalysisConfiguration();

            foreach (var holder in holders)
            {
                var suspicious = new SuspiciousHolder
                {
                    WalletAddress = holder.WalletAddress,
                    Balance = holder.Balance,
                    PercentageOfSupply = holder.PercentageOfSupply,
                    WalletProfile = holder.WalletProfile
                };

                var flags = SuspiciousFlags.None;
                var reasons = new List<string>();
                decimal score = 0;

                if (holder.WalletProfile != null)
                {
                    var profile = holder.WalletProfile;
                    var metadata = profile.Metadata;

                    // Check 1: New wallet (created within 7 days)
                    if (metadata.FirstTransactionDate.HasValue &&
                        (DateTime.UtcNow - metadata.FirstTransactionDate.Value).TotalDays < 7)
                    {
                        flags |= SuspiciousFlags.NewWallet;
                        reasons.Add($"Wallet created only {(DateTime.UtcNow - metadata.FirstTransactionDate.Value).TotalDays:F0} days ago");
                        score += 15;
                    }

                    // Check 2: Low activity (fewer than 10 transactions)
                    if (metadata.TotalTransactions < 10)
                    {
                        flags |= SuspiciousFlags.LowActivity;
                        reasons.Add($"Very low activity ({metadata.TotalTransactions} transactions)");
                        score += 10;
                    }

                    // Check 3: Large concentration (>5% of supply)
                    if (holder.PercentageOfSupply > 5)
                    {
                        flags |= SuspiciousFlags.LargeConcentration;
                        reasons.Add($"Holds {holder.PercentageOfSupply:F2}% of total supply");
                        score += 20;
                    }

                    // Check 4: Only holds this token
                    if (profile.TokenHoldings.Count == 1)
                    {
                        flags |= SuspiciousFlags.OnlyThisToken;
                        reasons.Add("Only holds this token (no diversification)");
                        score += 12;
                    }

                    // Check 5: No SOL balance (can't pay fees)
                    if (profile.SolBalance < 0.01m)
                    {
                        flags |= SuspiciousFlags.NoSolBalance;
                        reasons.Add($"Insufficient SOL for fees ({profile.SolBalance} SOL)");
                        score += 15;
                    }

                    // Check 6: Combination flags
                    if (flags.HasFlag(SuspiciousFlags.NewWallet) &&
                        flags.HasFlag(SuspiciousFlags.LargeConcentration) &&
                        flags.HasFlag(SuspiciousFlags.LowActivity))
                    {
                        reasons.Add("HIGH RISK: New wallet with large holding and low activity");
                        score += 30;
                    }
                }

                // Only add if suspicious
                if (flags != SuspiciousFlags.None)
                {
                    suspicious.Flags = flags;
                    suspicious.Reasons = reasons;
                    suspicious.SuspiciousScore = Math.Min(100, score);
                    suspiciousHolders.Add(suspicious);
                }
            }

            // Step: Check for relationships between suspicious holders
            if (includeRelationshipAnalysis && suspiciousHolders.Count >= 2)
            {
                var walletAddresses = suspiciousHolders.Select(h => h.WalletAddress).ToList();
                var analysisRequest = new AnalyzeWalletsRequest
                {
                    WalletAddresses = walletAddresses,
                    Configuration = config,
                    Network = network
                };

                var relationshipResult = await _walletAnalyzer.AnalyzeWalletsAsync(analysisRequest);

                if (relationshipResult.Success && relationshipResult.Data != null)
                {
                    // Add relationship information to suspicious holders
                    foreach (var suspicious in suspiciousHolders)
                    {
                        var relatedWallets = relationshipResult.Data.Relationships
                            .Where(r => r.Wallet1Address == suspicious.WalletAddress ||
                                      r.Wallet2Address == suspicious.WalletAddress)
                            .Where(r => r.RelationshipScore >= 0.5m)
                            .Select(r => r.Wallet1Address == suspicious.WalletAddress ?
                                       r.Wallet2Address : r.Wallet1Address)
                            .ToList();

                        if (relatedWallets.Any())
                        {
                            suspicious.Flags |= SuspiciousFlags.RelatedToOtherHolders;
                            suspicious.RelatedWallets = relatedWallets;
                            suspicious.Reasons.Add($"Related to {relatedWallets.Count} other large holders");
                            suspicious.SuspiciousScore += 25;

                            // Cap at 100
                            suspicious.SuspiciousScore = Math.Min(100, suspicious.SuspiciousScore);
                        }
                    }
                }
            }

            return suspiciousHolders.OrderByDescending(h => h.SuspiciousScore).ToList();
        }

        /// <summary>
        /// Identify clusters of suspicious holders
        /// </summary>
        private async Task<List<WalletCluster>> IdentifySuspiciousClustersAsync(
            List<SuspiciousHolder> suspiciousHolders,
            AnalysisConfiguration config)
        {
            var suspiciousClusters = new List<WalletCluster>();

            if (suspiciousHolders.Count < 2)
                return suspiciousClusters;

            var walletProfiles = suspiciousHolders
                .Where(h => h.WalletProfile != null)
                .Select(h => h.WalletProfile!)
                .ToList();

            if (walletProfiles.Count < 2)
                return suspiciousClusters;

            // Use existing wallet relationship analyzer to find clusters
            var relationships = await _walletAnalyzer.FindRelationshipsAsync(walletProfiles, config);

            // Identify clusters using graph traversal
            var processed = new HashSet<string>();
            var clusterId = 0;

            foreach (var holder in suspiciousHolders)
            {
                if (processed.Contains(holder.WalletAddress))
                    continue;

                var cluster = new WalletCluster
                {
                    ClusterId = $"suspicious-cluster-{++clusterId}",
                    WalletAddresses = new List<string> { holder.WalletAddress }
                };

                var relatedWallets = new Queue<string>();
                relatedWallets.Enqueue(holder.WalletAddress);
                processed.Add(holder.WalletAddress);

                while (relatedWallets.Count > 0)
                {
                    var currentWallet = relatedWallets.Dequeue();

                    var connectedRelationships = relationships
                        .Where(r => (r.Wallet1Address == currentWallet || r.Wallet2Address == currentWallet) &&
                                   r.RelationshipScore >= 0.5m)
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

                // Only add clusters with multiple wallets
                if (cluster.WalletAddresses.Count > 1)
                {
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
                        .FirstOrDefault()?.Key ?? RelationshipType.SuspiciousActivity;

                    var totalPercentage = suspiciousHolders
                        .Where(h => cluster.WalletAddresses.Contains(h.WalletAddress))
                        .Sum(h => h.PercentageOfSupply);

                    cluster.Description = $"Suspicious cluster of {cluster.WalletAddresses.Count} related wallets " +
                                        $"holding {totalPercentage:F2}% of supply (Avg score: {cluster.AverageRelationshipScore:P0})";

                    suspiciousClusters.Add(cluster);
                }
            }

            return suspiciousClusters;
        }

        #region Helper Methods

        /// <summary>
        /// Calculate Gini coefficient for income inequality
        /// </summary>
        private decimal CalculateGiniCoefficient(List<decimal> balances)
        {
            if (balances.Count == 0)
                return 0;

            var sorted = balances.OrderBy(b => b).ToList();
            decimal n = sorted.Count;
            decimal sum = 0;

            for (int i = 0; i < sorted.Count; i++)
            {
                sum += (2 * (i + 1) - n - 1) * sorted[i];
            }

            var totalBalance = sorted.Sum();
            if (totalBalance == 0)
                return 0;

            return sum / (n * totalBalance);
        }

        /// <summary>
        /// Calculate overall concentration score (0-100)
        /// </summary>
        private decimal CalculateConcentrationScore(SupplyConcentration concentration)
        {
            decimal score = 0;

            // Weight different factors
            score += concentration.Top1HolderPercentage * 0.4m;  // 40% weight
            score += concentration.Top5HoldersPercentage * 0.15m; // 15% weight
            score += concentration.GiniCoefficient * 30;          // 30% weight
            score += concentration.HerfindahlIndex * 15;          // 15% weight

            return Math.Min(100, score);
        }

        /// <summary>
        /// Determine risk level based on concentration metrics
        /// </summary>
        private ConcentrationRisk DetermineConcentrationRisk(SupplyConcentration concentration)
        {
            var score = concentration.ConcentrationScore;

            // Critical: Extremely concentrated (potential rug risk)
            if (concentration.Top1HolderPercentage > 50 || score > 80)
                return ConcentrationRisk.Critical;

            // Very High: Highly concentrated
            if (concentration.Top1HolderPercentage > 30 || score > 60)
                return ConcentrationRisk.VeryHigh;

            // High: Significant concentration
            if (concentration.Top5HoldersPercentage > 70 || score > 45)
                return ConcentrationRisk.High;

            // Medium: Moderate concentration
            if (concentration.Top10HoldersPercentage > 60 || score > 30)
                return ConcentrationRisk.Medium;

            // Low: Some concentration
            if (score > 15)
                return ConcentrationRisk.Low;

            // Very Low: Well distributed
            return ConcentrationRisk.VeryLow;
        }

        /// <summary>
        /// Get human-readable risk description
        /// </summary>
        private string GetRiskDescription(SupplyConcentration concentration)
        {
            return concentration.RiskLevel switch
            {
                ConcentrationRisk.Critical =>
                    $"CRITICAL: Extremely concentrated supply. Top holder: {concentration.Top1HolderPercentage:F1}%. High rug pull risk.",
                ConcentrationRisk.VeryHigh =>
                    $"VERY HIGH: Highly concentrated supply. Top holder: {concentration.Top1HolderPercentage:F1}%. Significant risk.",
                ConcentrationRisk.High =>
                    $"HIGH: Significant concentration. Top 5 holders: {concentration.Top5HoldersPercentage:F1}%. Exercise caution.",
                ConcentrationRisk.Medium =>
                    $"MEDIUM: Moderate concentration. Top 10 holders: {concentration.Top10HoldersPercentage:F1}%. Monitor closely.",
                ConcentrationRisk.Low =>
                    $"LOW: Some concentration but reasonable distribution. Gini: {concentration.GiniCoefficient:F2}",
                ConcentrationRisk.VeryLow =>
                    $"VERY LOW: Well-distributed supply across holders. Gini: {concentration.GiniCoefficient:F2}",
                _ => "Unknown risk level"
            };
        }

        #endregion
    }
}
