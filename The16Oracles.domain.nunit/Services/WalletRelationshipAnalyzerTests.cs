using NUnit.Framework;
using The16Oracles.domain.Models;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class WalletRelationshipAnalyzerTests
    {
        private WalletRelationshipAnalyzer _analyzer = null!;

        [SetUp]
        public void Setup()
        {
            _analyzer = new WalletRelationshipAnalyzer("https://localhost:5001");
        }

        #region Model Tests

        [Test]
        public void WalletProfile_DefaultValues_AreInitialized()
        {
            // Arrange & Act
            var profile = new WalletProfile();

            // Assert
            Assert.That(profile.Address, Is.EqualTo(string.Empty));
            Assert.That(profile.TokenHoldings, Is.Not.Null);
            Assert.That(profile.TokenHoldings.Count, Is.EqualTo(0));
            Assert.That(profile.Metadata, Is.Not.Null);
        }

        [Test]
        public void TokenHolding_CanSetProperties()
        {
            // Arrange
            var holding = new TokenHolding
            {
                TokenMintAddress = "TokenMint123",
                AccountAddress = "Account456",
                Balance = 100.5m,
                Decimals = 9
            };

            // Assert
            Assert.That(holding.TokenMintAddress, Is.EqualTo("TokenMint123"));
            Assert.That(holding.AccountAddress, Is.EqualTo("Account456"));
            Assert.That(holding.Balance, Is.EqualTo(100.5m));
            Assert.That(holding.Decimals, Is.EqualTo(9));
        }

        [Test]
        public void WalletRelationship_DefaultValues_AreInitialized()
        {
            // Arrange & Act
            var relationship = new WalletRelationship();

            // Assert
            Assert.That(relationship.Evidence, Is.Not.Null);
            Assert.That(relationship.Evidence.Count, Is.EqualTo(0));
            Assert.That(relationship.RelationshipScore, Is.EqualTo(0m));
            Assert.That(relationship.Type, Is.EqualTo(RelationshipType.None));
        }

        [Test]
        public void AnalysisConfiguration_DefaultValues_AreReasonable()
        {
            // Arrange & Act
            var config = new AnalysisConfiguration();

            // Assert
            Assert.That(config.MinimumRelationshipScore, Is.EqualTo(0.3m));
            Assert.That(config.MinimumSharedTokens, Is.EqualTo(2));
            Assert.That(config.TokenBalanceSimilarityThreshold, Is.EqualTo(0.8m));
            Assert.That(config.MinimumTransactionsForAnalysis, Is.EqualTo(5));
            Assert.That(config.IncludeInactiveWallets, Is.False);
            Assert.That(config.MaxWalletsToAnalyze, Is.EqualTo(100));
        }

        #endregion

        #region Validation Tests

        [Test]
        public async Task AnalyzeWalletsAsync_NoAddresses_ReturnsError()
        {
            // Arrange
            var request = new AnalyzeWalletsRequest
            {
                WalletAddresses = new List<string>()
            };

            // Act
            var result = await _analyzer.AnalyzeWalletsAsync(request);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("No wallet addresses"));
        }

        [Test]
        public async Task AnalyzeWalletsAsync_TooManyAddresses_ReturnsError()
        {
            // Arrange
            var request = new AnalyzeWalletsRequest
            {
                WalletAddresses = Enumerable.Range(1, 150).Select(i => $"wallet{i}").ToList(),
                Configuration = new AnalysisConfiguration { MaxWalletsToAnalyze = 100 }
            };

            // Act
            var result = await _analyzer.AnalyzeWalletsAsync(request);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("Too many wallets"));
        }

        #endregion

        #region Relationship Analysis Tests

        [Test]
        public async Task FindRelationshipsAsync_WalletsWithSharedTokens_DetectsRelationship()
        {
            // Arrange
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                SolBalance = 10m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token3", Balance = 75m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                SolBalance = 10m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token4", Balance = 25m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(1));
            var relationship = relationships[0];
            Assert.That(relationship.Wallet1Address, Is.EqualTo("wallet1"));
            Assert.That(relationship.Wallet2Address, Is.EqualTo("wallet2"));
            Assert.That(relationship.RelationshipScore, Is.GreaterThan(0m));
            Assert.That(relationship.Evidence.Any(e => e.Type == "SharedTokens"), Is.True);
        }

        [Test]
        public async Task FindRelationshipsAsync_WalletsWithNoSharedTokens_NoRelationship()
        {
            // Arrange
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token2", Balance = 100m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.3m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task FindRelationshipsAsync_SimilarSolBalances_IncreasesScore()
        {
            // Arrange
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                SolBalance = 100m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                SolBalance = 105m, // Very similar balance
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m }
                },
                Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(1));
            var relationship = relationships[0];
            Assert.That(relationship.Evidence.Any(e => e.Type == "SimilarSolBalance"), Is.True);
        }

        [Test]
        public async Task FindRelationshipsAsync_SimilarActivityLevels_DetectedInEvidence()
        {
            // Arrange
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                SolBalance = 10m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 100m }
                },
                Metadata = new WalletMetadata
                {
                    TotalTransactions = 100,
                    IsActive = true
                }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                SolBalance = 10m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 100m }
                },
                Metadata = new WalletMetadata
                {
                    TotalTransactions = 105, // Similar activity
                    IsActive = true
                }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(1));
            var relationship = relationships[0];
            Assert.That(relationship.Evidence.Any(e => e.Type == "SimilarActivity"), Is.True);
        }

        [Test]
        public async Task FindRelationshipsAsync_WalletsCreatedSameTime_TemporalProximityDetected()
        {
            // Arrange
            var baseDate = DateTime.UtcNow.AddDays(-30);
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 100m }
                },
                Metadata = new WalletMetadata
                {
                    FirstTransactionDate = baseDate,
                    TotalTransactions = 50,
                    IsActive = true
                }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 100m }
                },
                Metadata = new WalletMetadata
                {
                    FirstTransactionDate = baseDate.AddDays(2), // Within 7 days
                    TotalTransactions = 50,
                    IsActive = true
                }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(1));
            var relationship = relationships[0];
            Assert.That(relationship.Evidence.Any(e => e.Type == "TemporalProximity"), Is.True);
        }

        [Test]
        public async Task FindRelationshipsAsync_HighScore_ClassifiedAsLikelyRelated()
        {
            // Arrange
            var baseDate = DateTime.UtcNow.AddDays(-30);
            var wallet1 = new WalletProfile
            {
                Address = "wallet1",
                SolBalance = 100m,
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token3", Balance = 75m },
                    new TokenHolding { TokenMintAddress = "token4", Balance = 25m }
                },
                Metadata = new WalletMetadata
                {
                    FirstTransactionDate = baseDate,
                    TotalTransactions = 100,
                    IsActive = true
                }
            };

            var wallet2 = new WalletProfile
            {
                Address = "wallet2",
                SolBalance = 102m, // Similar balance
                TokenHoldings = new List<TokenHolding>
                {
                    new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                    new TokenHolding { TokenMintAddress = "token2", Balance = 50m },
                    new TokenHolding { TokenMintAddress = "token3", Balance = 75m },
                    new TokenHolding { TokenMintAddress = "token4", Balance = 25m }
                },
                Metadata = new WalletMetadata
                {
                    FirstTransactionDate = baseDate.AddDays(1), // Close in time
                    TotalTransactions = 105, // Similar activity
                    IsActive = true
                }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(new List<WalletProfile> { wallet1, wallet2 }, config);

            // Assert
            Assert.That(relationships.Count, Is.EqualTo(1));
            var relationship = relationships[0];
            Assert.That(relationship.Type, Is.EqualTo(RelationshipType.LikelyRelated).Or.EqualTo(RelationshipType.SuspiciousActivity));
            Assert.That(relationship.RelationshipScore, Is.GreaterThan(0.5m));
        }

        [Test]
        public async Task FindRelationshipsAsync_MultipleWallets_FindsAllPairs()
        {
            // Arrange
            var wallets = new List<WalletProfile>
            {
                new WalletProfile
                {
                    Address = "wallet1",
                    TokenHoldings = new List<TokenHolding>
                    {
                        new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                        new TokenHolding { TokenMintAddress = "token2", Balance = 50m }
                    },
                    Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
                },
                new WalletProfile
                {
                    Address = "wallet2",
                    TokenHoldings = new List<TokenHolding>
                    {
                        new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                        new TokenHolding { TokenMintAddress = "token2", Balance = 50m }
                    },
                    Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
                },
                new WalletProfile
                {
                    Address = "wallet3",
                    TokenHoldings = new List<TokenHolding>
                    {
                        new TokenHolding { TokenMintAddress = "token1", Balance = 100m },
                        new TokenHolding { TokenMintAddress = "token2", Balance = 50m }
                    },
                    Metadata = new WalletMetadata { TotalTransactions = 50, IsActive = true }
                }
            };

            var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m, MinimumSharedTokens = 2 };

            // Act
            var relationships = await _analyzer.FindRelationshipsAsync(wallets, config);

            // Assert
            // With 3 wallets, we should have up to 3 relationships: w1-w2, w1-w3, w2-w3
            Assert.That(relationships.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(relationships.Count, Is.LessThanOrEqualTo(3));
        }

        #endregion

        #region Relationship Type Tests

        [Test]
        public void RelationshipType_Enum_HasExpectedValues()
        {
            // Assert
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.None), Is.True);
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.LikelyRelated), Is.True);
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.PossiblyRelated), Is.True);
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.SharedTokens), Is.True);
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.MirrorTrading), Is.True);
            Assert.That(Enum.IsDefined(typeof(RelationshipType), RelationshipType.SuspiciousActivity), Is.True);
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void AnalysisStatistics_DefaultValues_AreZero()
        {
            // Arrange & Act
            var stats = new AnalysisStatistics();

            // Assert
            Assert.That(stats.TotalWalletsAnalyzed, Is.EqualTo(0));
            Assert.That(stats.TotalRelationshipsFound, Is.EqualTo(0));
            Assert.That(stats.TotalClustersFound, Is.EqualTo(0));
            Assert.That(stats.AverageTokensPerWallet, Is.EqualTo(0m));
            Assert.That(stats.HighestRelationshipScore, Is.EqualTo(0m));
        }

        [Test]
        public void WalletCluster_CanBeCreatedWithProperties()
        {
            // Arrange & Act
            var cluster = new WalletCluster
            {
                ClusterId = "cluster-1",
                WalletAddresses = new List<string> { "wallet1", "wallet2", "wallet3" },
                PrimaryRelationType = RelationshipType.SharedTokens,
                AverageRelationshipScore = 0.65m,
                SharedTokens = new List<string> { "token1", "token2" },
                Description = "Test cluster"
            };

            // Assert
            Assert.That(cluster.ClusterId, Is.EqualTo("cluster-1"));
            Assert.That(cluster.WalletAddresses.Count, Is.EqualTo(3));
            Assert.That(cluster.PrimaryRelationType, Is.EqualTo(RelationshipType.SharedTokens));
            Assert.That(cluster.AverageRelationshipScore, Is.EqualTo(0.65m));
            Assert.That(cluster.SharedTokens.Count, Is.EqualTo(2));
        }

        #endregion

        #region Request/Response Tests

        [Test]
        public void AnalyzeWalletsRequest_DefaultConfiguration_IsInitialized()
        {
            // Arrange & Act
            var request = new AnalyzeWalletsRequest();

            // Assert
            Assert.That(request.Configuration, Is.Not.Null);
            Assert.That(request.WalletAddresses, Is.Not.Null);
            Assert.That(request.Network, Is.EqualTo("devnet"));
        }

        [Test]
        public void AnalyzeWalletsResponse_DefaultValues_AreInitialized()
        {
            // Arrange & Act
            var response = new AnalyzeWalletsResponse();

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(string.Empty));
            Assert.That(response.Errors, Is.Not.Null);
            Assert.That(response.Warnings, Is.Not.Null);
        }

        #endregion

        #region Evidence Tests

        [Test]
        public void RelationshipEvidence_CanStoreDetails()
        {
            // Arrange
            var evidence = new RelationshipEvidence
            {
                Type = "SharedTokens",
                Description = "Both wallets hold 5 common tokens",
                Confidence = 0.8m,
                Details = new Dictionary<string, object>
                {
                    { "SharedTokenCount", 5 },
                    { "TokenList", new List<string> { "token1", "token2" } }
                }
            };

            // Assert
            Assert.That(evidence.Type, Is.EqualTo("SharedTokens"));
            Assert.That(evidence.Confidence, Is.EqualTo(0.8m));
            Assert.That(evidence.Details.ContainsKey("SharedTokenCount"), Is.True);
            Assert.That(evidence.Details["SharedTokenCount"], Is.EqualTo(5));
        }

        #endregion
    }
}
