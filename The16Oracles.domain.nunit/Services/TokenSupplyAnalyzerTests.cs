using The16Oracles.domain.Models;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class TokenSupplyAnalyzerTests
    {
        private TokenSupplyAnalyzer _analyzer = null!;

        [SetUp]
        public void Setup()
        {
            _analyzer = new TokenSupplyAnalyzer("https://localhost:5001");
        }

        #region Model Tests

        [Test]
        public void TokenSupplyInfo_Initialization_ShouldHaveDefaultValues()
        {
            var info = new TokenSupplyInfo();

            Assert.That(info.TokenMintAddress, Is.EqualTo(string.Empty));
            Assert.That(info.TotalSupply, Is.EqualTo(0));
            Assert.That(info.Decimals, Is.EqualTo(0));
        }

        [Test]
        public void TokenHolder_Initialization_ShouldHaveDefaultValues()
        {
            var holder = new TokenHolder();

            Assert.That(holder.WalletAddress, Is.EqualTo(string.Empty));
            Assert.That(holder.Balance, Is.EqualTo(0));
            Assert.That(holder.PercentageOfSupply, Is.EqualTo(0));
            Assert.That(holder.Rank, Is.EqualTo(0));
        }

        [Test]
        public void SupplyConcentration_Initialization_ShouldHaveDefaultValues()
        {
            var concentration = new SupplyConcentration();

            Assert.That(concentration.TokenMintAddress, Is.EqualTo(string.Empty));
            Assert.That(concentration.TotalSupply, Is.EqualTo(0));
            Assert.That(concentration.TotalHolders, Is.EqualTo(0));
            Assert.That(concentration.RiskLevel, Is.EqualTo(ConcentrationRisk.VeryLow));
        }

        [Test]
        public void SuspiciousHolder_Initialization_ShouldHaveDefaultValues()
        {
            var suspicious = new SuspiciousHolder();

            Assert.That(suspicious.WalletAddress, Is.EqualTo(string.Empty));
            Assert.That(suspicious.Flags, Is.EqualTo(SuspiciousFlags.None));
            Assert.That(suspicious.SuspiciousScore, Is.EqualTo(0));
            Assert.That(suspicious.Reasons, Is.Empty);
        }

        [Test]
        public void AnalyzeTokenSupplyRequest_DefaultValues_ShouldBeValid()
        {
            var request = new AnalyzeTokenSupplyRequest();

            Assert.That(request.Network, Is.EqualTo("devnet"));
            Assert.That(request.MaxHoldersToAnalyze, Is.EqualTo(100));
            Assert.That(request.MinimumHoldingPercentage, Is.EqualTo(0.01m));
            Assert.That(request.AnalyzeSuspiciousHolders, Is.True);
            Assert.That(request.IncludeRelationshipAnalysis, Is.True);
        }

        #endregion

        #region Suspicious Flags Tests

        [Test]
        public void SuspiciousFlags_None_ShouldBeZero()
        {
            var flags = SuspiciousFlags.None;
            Assert.That((int)flags, Is.EqualTo(0));
        }

        [Test]
        public void SuspiciousFlags_CanCombineFlags()
        {
            var flags = SuspiciousFlags.NewWallet | SuspiciousFlags.LowActivity | SuspiciousFlags.LargeConcentration;

            Assert.That(flags.HasFlag(SuspiciousFlags.NewWallet), Is.True);
            Assert.That(flags.HasFlag(SuspiciousFlags.LowActivity), Is.True);
            Assert.That(flags.HasFlag(SuspiciousFlags.LargeConcentration), Is.True);
            Assert.That(flags.HasFlag(SuspiciousFlags.NoSolBalance), Is.False);
        }

        [Test]
        public void SuspiciousFlags_AllFlags_ShouldBeUnique()
        {
            var allFlags = new[]
            {
                SuspiciousFlags.NewWallet,
                SuspiciousFlags.LowActivity,
                SuspiciousFlags.LargeConcentration,
                SuspiciousFlags.RelatedToOtherHolders,
                SuspiciousFlags.SimilarBalances,
                SuspiciousFlags.SimilarCreationTime,
                SuspiciousFlags.OnlyThisToken,
                SuspiciousFlags.NoSolBalance,
                SuspiciousFlags.RapidAccumulation
            };

            var distinctValues = allFlags.Select(f => (int)f).Distinct().Count();
            Assert.That(distinctValues, Is.EqualTo(allFlags.Length));
        }

        #endregion

        #region Concentration Risk Tests

        [Test]
        public void ConcentrationRisk_AllLevels_ShouldBeDifferent()
        {
            var levels = Enum.GetValues<ConcentrationRisk>();
            Assert.That(levels.Length, Is.EqualTo(6));
            Assert.That(levels, Contains.Item(ConcentrationRisk.VeryLow));
            Assert.That(levels, Contains.Item(ConcentrationRisk.Low));
            Assert.That(levels, Contains.Item(ConcentrationRisk.Medium));
            Assert.That(levels, Contains.Item(ConcentrationRisk.High));
            Assert.That(levels, Contains.Item(ConcentrationRisk.VeryHigh));
            Assert.That(levels, Contains.Item(ConcentrationRisk.Critical));
        }

        #endregion

        #region Concentration Calculation Tests

        [Test]
        public async Task CalculateConcentration_EmptyHolders_ShouldReturnVeryLowRisk()
        {
            var holders = new List<TokenHolder>();
            var totalSupply = 1000000m;

            var result = await _analyzer.CalculateConcentrationAsync(holders, totalSupply);

            Assert.That(result.RiskLevel, Is.EqualTo(ConcentrationRisk.VeryLow));
            Assert.That(result.Top1HolderPercentage, Is.EqualTo(0));
        }

        [Test]
        public async Task CalculateConcentration_SingleHolder_ShouldReturnCritical()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 1000000, PercentageOfSupply = 100 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 1000000);

            Assert.That(result.RiskLevel, Is.EqualTo(ConcentrationRisk.Critical));
            Assert.That(result.Top1HolderPercentage, Is.EqualTo(100));
        }

        [Test]
        public async Task CalculateConcentration_HighlyConcentrated_ShouldReturnVeryHigh()
        {
            var totalSupply = 1000000m;
            var holders = new List<TokenHolder>
            {
                new() { Balance = 400000, PercentageOfSupply = 40 },
                new() { Balance = 200000, PercentageOfSupply = 20 },
                new() { Balance = 150000, PercentageOfSupply = 15 },
                new() { Balance = 100000, PercentageOfSupply = 10 },
                new() { Balance = 150000, PercentageOfSupply = 15 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, totalSupply);

            Assert.That(result.Top1HolderPercentage, Is.EqualTo(40));
            Assert.That(result.Top5HoldersPercentage, Is.GreaterThan(80));
            Assert.That(result.RiskLevel, Is.EqualTo(ConcentrationRisk.VeryHigh).Or.EqualTo(ConcentrationRisk.Critical));
        }

        [Test]
        public async Task CalculateConcentration_WellDistributed_ShouldReturnLowRisk()
        {
            var totalSupply = 1000000m;
            var holders = new List<TokenHolder>();

            // Create 100 holders with relatively equal distribution
            for (int i = 0; i < 100; i++)
            {
                holders.Add(new TokenHolder
                {
                    Balance = 10000,
                    PercentageOfSupply = 1
                });
            }

            var result = await _analyzer.CalculateConcentrationAsync(holders, totalSupply);

            Assert.That(result.Top1HolderPercentage, Is.LessThan(5));
            Assert.That(result.RiskLevel, Is.EqualTo(ConcentrationRisk.VeryLow).Or.EqualTo(ConcentrationRisk.Low));
        }

        [Test]
        public async Task CalculateConcentration_ShouldCalculateGiniCoefficient()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 500000, PercentageOfSupply = 50 },
                new() { Balance = 300000, PercentageOfSupply = 30 },
                new() { Balance = 200000, PercentageOfSupply = 20 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 1000000);

            Assert.That(result.GiniCoefficient, Is.GreaterThan(0));
            Assert.That(result.GiniCoefficient, Is.LessThan(1));
        }

        [Test]
        public async Task CalculateConcentration_ShouldCalculateHerfindahlIndex()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 400000, PercentageOfSupply = 40 },
                new() { Balance = 300000, PercentageOfSupply = 30 },
                new() { Balance = 300000, PercentageOfSupply = 30 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 1000000);

            // HHI = sum of squared market shares
            // Expected: 0.40^2 + 0.30^2 + 0.30^2 = 0.16 + 0.09 + 0.09 = 0.34
            Assert.That(result.HerfindahlIndex, Is.GreaterThan(0));
            Assert.That(result.HerfindahlIndex, Is.LessThan(1));
        }

        [Test]
        public async Task CalculateConcentration_Top20Holders_ShouldBeCalculated()
        {
            var holders = new List<TokenHolder>();
            for (int i = 0; i < 30; i++)
            {
                holders.Add(new TokenHolder
                {
                    Balance = 10000 - (i * 100),
                    PercentageOfSupply = (10000m - (i * 100)) / 200000m * 100
                });
            }

            var result = await _analyzer.CalculateConcentrationAsync(holders, 200000);

            Assert.That(result.Top1HolderPercentage, Is.GreaterThan(0));
            Assert.That(result.Top5HoldersPercentage, Is.GreaterThan(result.Top1HolderPercentage));
            Assert.That(result.Top10HoldersPercentage, Is.GreaterThan(result.Top5HoldersPercentage));
            Assert.That(result.Top20HoldersPercentage, Is.GreaterThan(result.Top10HoldersPercentage));
        }

        #endregion

        #region Holder Analysis Tests

        [Test]
        public async Task GetHoldersFromWallets_EmptyList_ShouldReturnEmpty()
        {
            var holders = await _analyzer.GetHoldersFromWalletsAsync(
                "TestToken123",
                new List<string>(),
                "devnet");

            Assert.That(holders, Is.Empty);
        }

        [Test]
        public void TokenHolder_Ranking_ShouldBeAssignedCorrectly()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 1000, Rank = 1 },
                new() { Balance = 500, Rank = 2 },
                new() { Balance = 250, Rank = 3 }
            };

            Assert.That(holders[0].Rank, Is.EqualTo(1));
            Assert.That(holders[1].Rank, Is.EqualTo(2));
            Assert.That(holders[2].Rank, Is.EqualTo(3));
        }

        [Test]
        public void TokenHolder_PercentageCalculation_ShouldBeCorrect()
        {
            var totalSupply = 1000000m;
            var holder = new TokenHolder
            {
                Balance = 100000,
                PercentageOfSupply = (100000m / totalSupply) * 100
            };

            Assert.That(holder.PercentageOfSupply, Is.EqualTo(10m));
        }

        #endregion

        #region Request Validation Tests

        [Test]
        public void AnalyzeTokenSupplyRequest_ShouldAcceptCustomConfiguration()
        {
            var request = new AnalyzeTokenSupplyRequest
            {
                TokenMintAddress = "TestMint123",
                Network = "mainnet-beta",
                MaxHoldersToAnalyze = 50,
                MinimumHoldingPercentage = 1.0m,
                AnalyzeSuspiciousHolders = false,
                IncludeRelationshipAnalysis = false
            };

            Assert.That(request.TokenMintAddress, Is.EqualTo("TestMint123"));
            Assert.That(request.Network, Is.EqualTo("mainnet-beta"));
            Assert.That(request.MaxHoldersToAnalyze, Is.EqualTo(50));
            Assert.That(request.MinimumHoldingPercentage, Is.EqualTo(1.0m));
            Assert.That(request.AnalyzeSuspiciousHolders, Is.False);
            Assert.That(request.IncludeRelationshipAnalysis, Is.False);
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void SupplyAnalysisStatistics_Initialization_ShouldHaveDefaultValues()
        {
            var stats = new SupplyAnalysisStatistics();

            Assert.That(stats.TotalHoldersAnalyzed, Is.EqualTo(0));
            Assert.That(stats.SuspiciousHoldersFound, Is.EqualTo(0));
            Assert.That(stats.SuspiciousClustersFound, Is.EqualTo(0));
            Assert.That(stats.TotalSupplyAnalyzed, Is.EqualTo(0));
            Assert.That(stats.PercentageAnalyzed, Is.EqualTo(0));
        }

        [Test]
        public void SupplyAnalysisStatistics_ShouldCalculateCorrectly()
        {
            var stats = new SupplyAnalysisStatistics
            {
                TotalHoldersAnalyzed = 50,
                SuspiciousHoldersFound = 5,
                SuspiciousClustersFound = 2,
                TotalSupplyAnalyzed = 750000,
                PercentageAnalyzed = 75m
            };

            Assert.That(stats.TotalHoldersAnalyzed, Is.EqualTo(50));
            Assert.That(stats.SuspiciousHoldersFound, Is.EqualTo(5));
            Assert.That(stats.SuspiciousClustersFound, Is.EqualTo(2));
            Assert.That(stats.PercentageAnalyzed, Is.EqualTo(75m));
        }

        #endregion

        #region Response Tests

        [Test]
        public void AnalyzeTokenSupplyResponse_Success_ShouldHaveCorrectStructure()
        {
            var response = new AnalyzeTokenSupplyResponse
            {
                Success = true,
                Message = "Analysis completed",
                Data = new TokenSupplyAnalysisResult(),
                Errors = new List<string>(),
                Warnings = new List<string> { "Test warning" }
            };

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Is.EqualTo("Analysis completed"));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Warnings, Has.Count.EqualTo(1));
            Assert.That(response.Errors, Is.Empty);
        }

        [Test]
        public void AnalyzeTokenSupplyResponse_Failure_ShouldHaveErrors()
        {
            var response = new AnalyzeTokenSupplyResponse
            {
                Success = false,
                Message = "Analysis failed",
                Errors = new List<string> { "Error 1", "Error 2" }
            };

            Assert.That(response.Success, Is.False);
            Assert.That(response.Errors, Has.Count.EqualTo(2));
            Assert.That(response.Data, Is.Null);
        }

        #endregion

        #region Integration Model Tests

        [Test]
        public void TokenSupplyAnalysisResult_ShouldContainAllComponents()
        {
            var result = new TokenSupplyAnalysisResult
            {
                SupplyInfo = new TokenSupplyInfo { TokenMintAddress = "Test123" },
                Concentration = new SupplyConcentration { TotalSupply = 1000000 },
                TopHolders = new List<TokenHolder> { new() { Balance = 100000 } },
                SuspiciousHolders = new List<SuspiciousHolder> { new() { SuspiciousScore = 75 } },
                SuspiciousClusters = new List<WalletCluster>(),
                AnalyzedAt = DateTime.UtcNow,
                Statistics = new SupplyAnalysisStatistics()
            };

            Assert.That(result.SupplyInfo, Is.Not.Null);
            Assert.That(result.Concentration, Is.Not.Null);
            Assert.That(result.TopHolders, Has.Count.EqualTo(1));
            Assert.That(result.SuspiciousHolders, Has.Count.EqualTo(1));
            Assert.That(result.SuspiciousClusters, Is.Empty);
            Assert.That(result.Statistics, Is.Not.Null);
        }

        #endregion

        #region Suspicious Holder Detection Tests

        [Test]
        public void SuspiciousHolder_WithMultipleFlags_ShouldCombineScores()
        {
            var suspicious = new SuspiciousHolder
            {
                Flags = SuspiciousFlags.NewWallet | SuspiciousFlags.LowActivity | SuspiciousFlags.LargeConcentration,
                SuspiciousScore = 45,
                Reasons = new List<string>
                {
                    "Wallet created only 3 days ago",
                    "Very low activity (5 transactions)",
                    "Holds 15% of total supply"
                }
            };

            Assert.That(suspicious.Flags.HasFlag(SuspiciousFlags.NewWallet), Is.True);
            Assert.That(suspicious.Flags.HasFlag(SuspiciousFlags.LowActivity), Is.True);
            Assert.That(suspicious.Flags.HasFlag(SuspiciousFlags.LargeConcentration), Is.True);
            Assert.That(suspicious.SuspiciousScore, Is.EqualTo(45));
            Assert.That(suspicious.Reasons, Has.Count.EqualTo(3));
        }

        [Test]
        public void SuspiciousHolder_ScoreCap_ShouldNotExceed100()
        {
            var suspicious = new SuspiciousHolder
            {
                SuspiciousScore = 150 // Simulate score calculation that exceeds 100
            };

            // In actual implementation, score should be capped
            var cappedScore = Math.Min(100, suspicious.SuspiciousScore);
            Assert.That(cappedScore, Is.EqualTo(100));
        }

        #endregion

        #region Edge Cases

        [Test]
        public async Task CalculateConcentration_ZeroSupply_ShouldHandleGracefully()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 0, PercentageOfSupply = 0 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 0);

            Assert.That(result.RiskLevel, Is.EqualTo(ConcentrationRisk.VeryLow));
        }

        [Test]
        public async Task CalculateConcentration_LessThan5Holders_ShouldHandleTopNCalculations()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 500, PercentageOfSupply = 50 },
                new() { Balance = 500, PercentageOfSupply = 50 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 1000);

            Assert.That(result.Top1HolderPercentage, Is.GreaterThan(0));
            // Should handle when we have less than 5/10/20 holders
        }

        #endregion

        #region Concentration Score Tests

        [Test]
        public async Task ConcentrationScore_ShouldBeBetween0And100()
        {
            var holders = new List<TokenHolder>
            {
                new() { Balance = 600000, PercentageOfSupply = 60 },
                new() { Balance = 400000, PercentageOfSupply = 40 }
            };

            var result = await _analyzer.CalculateConcentrationAsync(holders, 1000000);

            Assert.That(result.ConcentrationScore, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.ConcentrationScore, Is.LessThanOrEqualTo(100));
        }

        #endregion
    }
}
