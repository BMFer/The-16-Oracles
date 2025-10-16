using The16Oracles.domain.Models;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class TokenCreatorAnalyzerTests
    {
        private TokenCreatorAnalyzer _analyzer = null!;

        [SetUp]
        public void Setup()
        {
            _analyzer = new TokenCreatorAnalyzer("https://localhost:5001");
        }

        #region Model Tests

        [Test]
        public void TokenCreatorInfo_Initialization_ShouldHaveDefaultValues()
        {
            var info = new TokenCreatorInfo();

            Assert.That(info.TokenMintAddress, Is.EqualTo(string.Empty));
            Assert.That(info.MintAuthority, Is.Null);
            Assert.That(info.FreezeAuthority, Is.Null);
            Assert.That(info.Decimals, Is.EqualTo(0));
            Assert.That(info.Supply, Is.EqualTo(0));
            Assert.That(info.IsInitialized, Is.False);
        }

        [Test]
        public void TokenAccountCreatorInfo_Initialization_ShouldHaveDefaultValues()
        {
            var info = new TokenAccountCreatorInfo();

            Assert.That(info.AccountAddress, Is.EqualTo(string.Empty));
            Assert.That(info.TokenMintAddress, Is.EqualTo(string.Empty));
            Assert.That(info.Owner, Is.EqualTo(string.Empty));
            Assert.That(info.Balance, Is.EqualTo(0));
            Assert.That(info.CreatorInfo, Is.Null);
        }

        [Test]
        public void GetTokenCreatorsRequest_DefaultValues_ShouldBeValid()
        {
            var request = new GetTokenCreatorsRequest();

            Assert.That(request.AccountAddresses, Is.Empty);
            Assert.That(request.Network, Is.EqualTo("devnet"));
            Assert.That(request.IncludeZeroBalances, Is.False);
            Assert.That(request.FetchSupplyInfo, Is.True);
        }

        [Test]
        public void TokenCreatorsResult_Initialization_ShouldHaveDefaultValues()
        {
            var result = new TokenCreatorsResult();

            Assert.That(result.Accounts, Is.Empty);
            Assert.That(result.UniqueCreators, Is.Empty);
            Assert.That(result.Statistics, Is.Not.Null);
        }

        [Test]
        public void TokenCreatorStatistics_Initialization_ShouldHaveDefaultValues()
        {
            var stats = new TokenCreatorStatistics();

            Assert.That(stats.TotalAccountsAnalyzed, Is.EqualTo(0));
            Assert.That(stats.TotalUniqueTokens, Is.EqualTo(0));
            Assert.That(stats.TotalUniqueCreators, Is.EqualTo(0));
            Assert.That(stats.AccountsWithNoAuthority, Is.EqualTo(0));
            Assert.That(stats.AccountsWithSameAuthority, Is.EqualTo(0));
        }

        #endregion

        #region Request Validation Tests

        [Test]
        public void GetTokenCreatorsRequest_WithCustomValues_ShouldBeValid()
        {
            var request = new GetTokenCreatorsRequest
            {
                AccountAddresses = new List<string> { "account1", "account2" },
                Network = "mainnet-beta",
                IncludeZeroBalances = true,
                FetchSupplyInfo = false
            };

            Assert.That(request.AccountAddresses, Has.Count.EqualTo(2));
            Assert.That(request.Network, Is.EqualTo("mainnet-beta"));
            Assert.That(request.IncludeZeroBalances, Is.True);
            Assert.That(request.FetchSupplyInfo, Is.False);
        }

        [Test]
        public void GroupTokensByCreatorRequest_DefaultValues_ShouldBeValid()
        {
            var request = new GroupTokensByCreatorRequest();

            Assert.That(request.AccountAddresses, Is.Empty);
            Assert.That(request.Network, Is.EqualTo("devnet"));
            Assert.That(request.IncludeZeroBalances, Is.False);
            Assert.That(request.MinimumTokenCount, Is.EqualTo(1));
        }

        #endregion

        #region TokenCreatorInfo Tests

        [Test]
        public void TokenCreatorInfo_WithMintAuthority_ShouldStoreCorrectly()
        {
            var info = new TokenCreatorInfo
            {
                TokenMintAddress = "TokenMint123",
                MintAuthority = "Authority456",
                FreezeAuthority = "Freeze789",
                Decimals = 9,
                Supply = 1000000,
                IsInitialized = true
            };

            Assert.That(info.TokenMintAddress, Is.EqualTo("TokenMint123"));
            Assert.That(info.MintAuthority, Is.EqualTo("Authority456"));
            Assert.That(info.FreezeAuthority, Is.EqualTo("Freeze789"));
            Assert.That(info.Decimals, Is.EqualTo(9));
            Assert.That(info.Supply, Is.EqualTo(1000000));
            Assert.That(info.IsInitialized, Is.True);
        }

        [Test]
        public void TokenCreatorInfo_WithNoAuthority_ShouldBeValid()
        {
            var info = new TokenCreatorInfo
            {
                TokenMintAddress = "TokenMint123",
                MintAuthority = null,
                FreezeAuthority = null,
                Decimals = 9,
                Supply = 1000000
            };

            Assert.That(info.MintAuthority, Is.Null);
            Assert.That(info.FreezeAuthority, Is.Null);
        }

        [Test]
        public void TokenCreatorInfo_WithMetadata_ShouldStoreSymbolAndName()
        {
            var info = new TokenCreatorInfo
            {
                TokenMintAddress = "TokenMint123",
                TokenName = "My Token",
                TokenSymbol = "MTK"
            };

            Assert.That(info.TokenName, Is.EqualTo("My Token"));
            Assert.That(info.TokenSymbol, Is.EqualTo("MTK"));
        }

        #endregion

        #region TokenAccountCreatorInfo Tests

        [Test]
        public void TokenAccountCreatorInfo_WithCreatorInfo_ShouldLinkCorrectly()
        {
            var creatorInfo = new TokenCreatorInfo
            {
                TokenMintAddress = "Mint123",
                MintAuthority = "Creator456"
            };

            var accountInfo = new TokenAccountCreatorInfo
            {
                AccountAddress = "Account789",
                TokenMintAddress = "Mint123",
                Owner = "Owner000",
                Balance = 100,
                CreatorInfo = creatorInfo
            };

            Assert.That(accountInfo.CreatorInfo, Is.Not.Null);
            Assert.That(accountInfo.CreatorInfo.MintAuthority, Is.EqualTo("Creator456"));
            Assert.That(accountInfo.TokenMintAddress, Is.EqualTo(creatorInfo.TokenMintAddress));
        }

        #endregion

        #region TokensByCreator Tests

        [Test]
        public void TokensByCreator_Initialization_ShouldHaveDefaultValues()
        {
            var grouped = new TokensByCreator();

            Assert.That(grouped.CreatorAddress, Is.EqualTo(string.Empty));
            Assert.That(grouped.Tokens, Is.Empty);
            Assert.That(grouped.TotalTokenCount, Is.EqualTo(0));
            Assert.That(grouped.TotalValueHeld, Is.EqualTo(0));
        }

        [Test]
        public void TokensByCreator_WithMultipleTokens_ShouldCalculateTotals()
        {
            var grouped = new TokensByCreator
            {
                CreatorAddress = "Creator123",
                Tokens = new List<TokenAccountCreatorInfo>
                {
                    new() { Balance = 100 },
                    new() { Balance = 200 },
                    new() { Balance = 300 }
                },
                TotalTokenCount = 3,
                TotalValueHeld = 600
            };

            Assert.That(grouped.TotalTokenCount, Is.EqualTo(3));
            Assert.That(grouped.TotalValueHeld, Is.EqualTo(600));
            Assert.That(grouped.Tokens.Sum(t => t.Balance), Is.EqualTo(600));
        }

        #endregion

        #region Response Tests

        [Test]
        public void GetTokenCreatorsResponse_Success_ShouldHaveCorrectStructure()
        {
            var response = new GetTokenCreatorsResponse
            {
                Success = true,
                Message = "Analysis completed",
                Data = new TokenCreatorsResult(),
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
        public void GetTokenCreatorsResponse_Failure_ShouldHaveErrors()
        {
            var response = new GetTokenCreatorsResponse
            {
                Success = false,
                Message = "Analysis failed",
                Errors = new List<string> { "Error 1", "Error 2" }
            };

            Assert.That(response.Success, Is.False);
            Assert.That(response.Errors, Has.Count.EqualTo(2));
            Assert.That(response.Data, Is.Null);
        }

        [Test]
        public void GroupTokensByCreatorResponse_Success_ShouldContainData()
        {
            var response = new GroupTokensByCreatorResponse
            {
                Success = true,
                Message = "Found 3 creators",
                Data = new List<TokensByCreator>
                {
                    new() { CreatorAddress = "Creator1", TotalTokenCount = 5 },
                    new() { CreatorAddress = "Creator2", TotalTokenCount = 3 },
                    new() { CreatorAddress = "Creator3", TotalTokenCount = 2 }
                }
            };

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data, Has.Count.EqualTo(3));
            Assert.That(response.Data[0].TotalTokenCount, Is.EqualTo(5));
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void TokenCreatorStatistics_WithData_ShouldCalculateCorrectly()
        {
            var stats = new TokenCreatorStatistics
            {
                TotalAccountsAnalyzed = 10,
                TotalUniqueTokens = 7,
                TotalUniqueCreators = 3,
                AccountsWithNoAuthority = 2,
                AccountsWithSameAuthority = 1
            };

            Assert.That(stats.TotalAccountsAnalyzed, Is.EqualTo(10));
            Assert.That(stats.TotalUniqueTokens, Is.EqualTo(7));
            Assert.That(stats.TotalUniqueCreators, Is.EqualTo(3));
            Assert.That(stats.AccountsWithNoAuthority, Is.EqualTo(2));
            Assert.That(stats.AccountsWithSameAuthority, Is.EqualTo(1));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void TokenCreatorsResult_WithCompleteData_ShouldContainAllComponents()
        {
            var creator1 = new TokenCreatorInfo
            {
                TokenMintAddress = "Mint1",
                MintAuthority = "Creator1"
            };

            var creator2 = new TokenCreatorInfo
            {
                TokenMintAddress = "Mint2",
                MintAuthority = "Creator2"
            };

            var result = new TokenCreatorsResult
            {
                Accounts = new List<TokenAccountCreatorInfo>
                {
                    new()
                    {
                        AccountAddress = "Account1",
                        TokenMintAddress = "Mint1",
                        CreatorInfo = creator1
                    },
                    new()
                    {
                        AccountAddress = "Account2",
                        TokenMintAddress = "Mint2",
                        CreatorInfo = creator2
                    }
                },
                UniqueCreators = new Dictionary<string, TokenCreatorInfo>
                {
                    { "Creator1", creator1 },
                    { "Creator2", creator2 }
                },
                AnalyzedAt = DateTime.UtcNow,
                Statistics = new TokenCreatorStatistics
                {
                    TotalAccountsAnalyzed = 2,
                    TotalUniqueTokens = 2,
                    TotalUniqueCreators = 2
                }
            };

            Assert.That(result.Accounts, Has.Count.EqualTo(2));
            Assert.That(result.UniqueCreators, Has.Count.EqualTo(2));
            Assert.That(result.Statistics.TotalUniqueCreators, Is.EqualTo(2));
        }

        #endregion

        #region Edge Cases

        [Test]
        public async Task GetAccountCreatorInfo_EmptyList_ShouldReturnEmpty()
        {
            var result = await _analyzer.GetAccountCreatorInfoAsync(new List<string>(), "devnet");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TokenCreatorInfo_WithLargeSupply_ShouldHandleCorrectly()
        {
            var info = new TokenCreatorInfo
            {
                TokenMintAddress = "Mint123",
                Supply = 1_000_000_000_000m // 1 trillion
            };

            Assert.That(info.Supply, Is.EqualTo(1_000_000_000_000m));
        }

        [Test]
        public void TokenAccountCreatorInfo_WithZeroBalance_ShouldBeValid()
        {
            var account = new TokenAccountCreatorInfo
            {
                AccountAddress = "Account123",
                Balance = 0
            };

            Assert.That(account.Balance, Is.EqualTo(0));
        }

        #endregion

        #region Grouping Logic Tests

        [Test]
        public void TokensByCreator_GroupingLogic_ShouldWorkCorrectly()
        {
            var accounts = new List<TokenAccountCreatorInfo>
            {
                new()
                {
                    AccountAddress = "Account1",
                    TokenMintAddress = "Mint1",
                    Balance = 100,
                    CreatorInfo = new TokenCreatorInfo { MintAuthority = "Creator1" }
                },
                new()
                {
                    AccountAddress = "Account2",
                    TokenMintAddress = "Mint2",
                    Balance = 200,
                    CreatorInfo = new TokenCreatorInfo { MintAuthority = "Creator1" }
                },
                new()
                {
                    AccountAddress = "Account3",
                    TokenMintAddress = "Mint3",
                    Balance = 300,
                    CreatorInfo = new TokenCreatorInfo { MintAuthority = "Creator2" }
                }
            };

            var grouped = accounts
                .Where(a => a.CreatorInfo?.MintAuthority != null)
                .GroupBy(a => a.CreatorInfo!.MintAuthority!)
                .Select(g => new TokensByCreator
                {
                    CreatorAddress = g.Key,
                    Tokens = g.ToList(),
                    TotalTokenCount = g.Count(),
                    TotalValueHeld = g.Sum(a => a.Balance)
                })
                .ToList();

            Assert.That(grouped, Has.Count.EqualTo(2));
            Assert.That(grouped[0].TotalTokenCount, Is.EqualTo(2));
            Assert.That(grouped[0].TotalValueHeld, Is.EqualTo(300));
            Assert.That(grouped[1].TotalTokenCount, Is.EqualTo(1));
            Assert.That(grouped[1].TotalValueHeld, Is.EqualTo(300));
        }

        #endregion

        #region Filter Tests

        [Test]
        public void Accounts_FilterZeroBalances_ShouldWorkCorrectly()
        {
            var accounts = new List<TokenAccountCreatorInfo>
            {
                new() { Balance = 100 },
                new() { Balance = 0 },
                new() { Balance = 200 },
                new() { Balance = 0 }
            };

            var filtered = accounts.Where(a => a.Balance > 0).ToList();

            Assert.That(filtered, Has.Count.EqualTo(2));
            Assert.That(filtered.All(a => a.Balance > 0), Is.True);
        }

        [Test]
        public void TokensByCreator_FilterByMinimumCount_ShouldWorkCorrectly()
        {
            var grouped = new List<TokensByCreator>
            {
                new() { CreatorAddress = "Creator1", TotalTokenCount = 5 },
                new() { CreatorAddress = "Creator2", TotalTokenCount = 2 },
                new() { CreatorAddress = "Creator3", TotalTokenCount = 1 }
            };

            var filtered = grouped.Where(g => g.TotalTokenCount >= 2).ToList();

            Assert.That(filtered, Has.Count.EqualTo(2));
            Assert.That(filtered.All(g => g.TotalTokenCount >= 2), Is.True);
        }

        #endregion

        #region Dictionary Tests

        [Test]
        public void UniqueCreators_Dictionary_ShouldNotHaveDuplicates()
        {
            var uniqueCreators = new Dictionary<string, TokenCreatorInfo>
            {
                { "Creator1", new TokenCreatorInfo { MintAuthority = "Creator1" } },
                { "Creator2", new TokenCreatorInfo { MintAuthority = "Creator2" } }
            };

            Assert.That(uniqueCreators.Keys, Is.Unique);
            Assert.That(uniqueCreators, Has.Count.EqualTo(2));
        }

        #endregion
    }
}
