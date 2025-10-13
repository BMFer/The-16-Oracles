using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using The16Oracles.DAOA.Services;
using The16Oracles.DAOA.Models.SplToken;

namespace The16Oracles.DAOA.nunit.Services
{
    /// <summary>
    /// Tests for SplTokenService
    /// </summary>
    [TestFixture]
    public class SplTokenServiceTests
    {
        private Mock<ILogger<SplTokenService>> _mockLogger = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private SplTokenService _splTokenService = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SplTokenService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _splTokenService = new SplTokenService(_mockLogger.Object, _mockConfiguration.Object);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new SplTokenService(_mockLogger.Object, _mockConfiguration.Object);

            // Assert
            Assert.That(service, Is.Not.Null);
        }

        #endregion

        #region Account Management Tests

        [Test]
        public async Task GetAccountsAsync_WithTokenMintAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new TokenAccountsRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAccountsAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token accounts"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
            Assert.That(response.Timestamp, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task GetAccountsAsync_WithAddressesOnlyFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new TokenAccountsRequest
            {
                AddressesOnly = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAccountsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--addresses-only"));
        }

        [Test]
        public async Task GetAccountsAsync_WithDelegatedFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new TokenAccountsRequest
            {
                Delegated = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAccountsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--delegated"));
        }

        [Test]
        public async Task GetAccountsAsync_WithExternallyCloseableFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new TokenAccountsRequest
            {
                ExternallyCloseable = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAccountsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--externally-closeable"));
        }

        [Test]
        public async Task GetAccountsAsync_WithOwner_ShouldIncludeOwnerInCommand()
        {
            // Arrange
            var request = new TokenAccountsRequest
            {
                Owner = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAccountsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--owner 5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC"));
        }

        [Test]
        public async Task GetAddressAsync_WithTokenAndOwner_ShouldReturnResponse()
        {
            // Arrange
            var request = new TokenAddressRequest
            {
                Token = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Owner = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetAddressAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token address"));
            Assert.That(response.Command, Does.Contain("--token EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
            Assert.That(response.Command, Does.Contain("--owner 5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC"));
        }

        [Test]
        public async Task GetBalanceAsync_WithTokenMintAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token balance"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        [Test]
        public async Task GetBalanceAsync_WithAccountAddress_ShouldIncludeAddressInCommand()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                Address = "TokenAccountAddress111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--address TokenAccountAddress111111111111111111111111"));
        }

        [Test]
        public async Task CreateAccountAsync_WithTokenMintAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new CreateTokenAccountRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CreateAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token create-account"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        [Test]
        public async Task CreateAccountAsync_WithImmutableFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new CreateTokenAccountRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Immutable = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CreateAccountAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--immutable"));
        }

        [Test]
        public async Task CloseAccountAsync_WithTokenMintAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new CloseTokenAccountRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CloseAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token close"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        [Test]
        public async Task CloseAccountAsync_WithRecipient_ShouldIncludeRecipientInCommand()
        {
            // Arrange
            var request = new CloseTokenAccountRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Recipient = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CloseAccountAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--recipient 5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC"));
        }

        [Test]
        public async Task GarbageCollectAsync_WithCloseEmptyFlag_ShouldReturnResponse()
        {
            // Arrange
            var request = new GarbageCollectRequest
            {
                CloseEmptyAssociatedAccounts = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GarbageCollectAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token gc"));
            Assert.That(response.Command, Does.Contain("--close-empty-associated-accounts"));
        }

        #endregion

        #region Token Operations Tests

        [Test]
        public async Task CreateTokenAsync_WithDefaults_ShouldReturnResponse()
        {
            // Arrange
            var request = new CreateTokenRequest
            {
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CreateTokenAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token create-token"));
        }

        [Test]
        public async Task CreateTokenAsync_WithDecimals_ShouldIncludeDecimalsInCommand()
        {
            // Arrange
            var request = new CreateTokenRequest
            {
                Decimals = 6,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CreateTokenAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--decimals 6"));
        }

        [Test]
        public async Task CreateTokenAsync_WithEnableFreeze_ShouldIncludeFreezeInCommand()
        {
            // Arrange
            var request = new CreateTokenRequest
            {
                EnableFreeze = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CreateTokenAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--enable-freeze"));
        }

        [Test]
        public async Task MintAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new MintTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 1000.5m,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.MintAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token mint"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
            Assert.That(response.Command, Does.Contain("1000.5"));
        }

        [Test]
        public async Task MintAsync_WithRecipient_ShouldIncludeRecipientInCommand()
        {
            // Arrange
            var request = new MintTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 100m,
                RecipientAddress = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.MintAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC"));
        }

        [Test]
        public async Task BurnAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new BurnTokensRequest
            {
                AccountAddress = "TokenAccount111111111111111111111111111111",
                Amount = 50.25m,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.BurnAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token burn"));
            Assert.That(response.Command, Does.Contain("TokenAccount111111111111111111111111111111"));
            Assert.That(response.Command, Does.Contain("50.25"));
        }

        [Test]
        public async Task TransferAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new TransferTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 100.5m,
                RecipientAddress = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.TransferAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token transfer"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
            Assert.That(response.Command, Does.Contain("100.5"));
            Assert.That(response.Command, Does.Contain("5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC"));
        }

        [Test]
        public async Task TransferAsync_WithFundFlag_ShouldIncludeFundInCommand()
        {
            // Arrange
            var request = new TransferTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 100m,
                RecipientAddress = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Fund = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.TransferAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--fund"));
        }

        [Test]
        public async Task TransferAsync_WithAllowUnfundedRecipientFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new TransferTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 100m,
                RecipientAddress = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                AllowUnfundedRecipient = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.TransferAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--allow-unfunded-recipient"));
        }

        [Test]
        public async Task GetSupplyAsync_WithTokenAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new TokenSupplyRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.GetSupplyAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token supply"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        [Test]
        public async Task CloseMintAsync_WithTokenAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new CloseMintRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.CloseMintAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token close-mint"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        #endregion

        #region Token Delegation Tests

        [Test]
        public async Task ApproveAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new ApproveRequest
            {
                AccountAddress = "TokenAccount111111111111111111111111111111",
                Amount = 500m,
                DelegateAddress = "Delegate111111111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.ApproveAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token approve"));
            Assert.That(response.Command, Does.Contain("TokenAccount111111111111111111111111111111"));
            Assert.That(response.Command, Does.Contain("500"));
            Assert.That(response.Command, Does.Contain("Delegate111111111111111111111111111111111"));
        }

        [Test]
        public async Task RevokeAsync_WithAccountAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new RevokeRequest
            {
                AccountAddress = "TokenAccount111111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.RevokeAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token revoke"));
            Assert.That(response.Command, Does.Contain("TokenAccount111111111111111111111111111111"));
        }

        #endregion

        #region Token Authority Tests

        [Test]
        public async Task AuthorizeAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new AuthorizeRequest
            {
                Address = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                AuthorityType = "mint",
                NewAuthority = "NewAuthority11111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.AuthorizeAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token authorize"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
            Assert.That(response.Command, Does.Contain("mint"));
            Assert.That(response.Command, Does.Contain("NewAuthority11111111111111111111111111111"));
        }

        #endregion

        #region Freeze/Thaw Tests

        [Test]
        public async Task FreezeAsync_WithAccountAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new FreezeAccountRequest
            {
                AccountAddress = "TokenAccount111111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.FreezeAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token freeze"));
            Assert.That(response.Command, Does.Contain("TokenAccount111111111111111111111111111111"));
        }

        [Test]
        public async Task ThawAsync_WithAccountAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new ThawAccountRequest
            {
                AccountAddress = "TokenAccount111111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.ThawAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token thaw"));
            Assert.That(response.Command, Does.Contain("TokenAccount111111111111111111111111111111"));
        }

        #endregion

        #region Native SOL Wrapping Tests

        [Test]
        public async Task WrapAsync_WithAmount_ShouldReturnResponse()
        {
            // Arrange
            var request = new WrapRequest
            {
                Amount = 5.5m,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.WrapAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token wrap"));
            Assert.That(response.Command, Does.Contain("5.5"));
        }

        [Test]
        public async Task WrapAsync_WithCreateAuxFlag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new WrapRequest
            {
                Amount = 1m,
                CreateAux = true,
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.WrapAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--create-aux-account"));
        }

        [Test]
        public async Task UnwrapAsync_WithAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new UnwrapRequest
            {
                Address = "WrappedAccount111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.UnwrapAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token unwrap"));
            Assert.That(response.Command, Does.Contain("WrappedAccount111111111111111111111111111"));
        }

        [Test]
        public async Task SyncNativeAsync_WithAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new SyncNativeRequest
            {
                Address = "NativeAccount111111111111111111111111111111",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.SyncNativeAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token sync-native"));
            Assert.That(response.Command, Does.Contain("NativeAccount111111111111111111111111111111"));
        }

        #endregion

        #region Display Tests

        [Test]
        public async Task DisplayAsync_WithAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new DisplayRequest
            {
                Address = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.DisplayAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token display"));
            Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
        }

        #endregion

        #region ExecuteCommandAsync Tests

        [Test]
        public async Task ExecuteCommandAsync_WithValidCommand_ShouldReturnResponse()
        {
            // Arrange
            var request = new SplTokenCommandRequest
            {
                Command = "accounts",
                Arguments = new Dictionary<string, string>(),
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.ExecuteCommandAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("spl-token accounts"));
        }

        [Test]
        public async Task ExecuteCommandAsync_WithArguments_ShouldIncludeArgumentsInCommand()
        {
            // Arrange
            var request = new SplTokenCommandRequest
            {
                Command = "custom-command",
                Arguments = new Dictionary<string, string>
                {
                    { "--arg1", "value1" },
                    { "--arg2", "value2" }
                },
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.ExecuteCommandAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--arg1 value1"));
            Assert.That(response.Command, Does.Contain("--arg2 value2"));
        }

        #endregion

        #region Global Flags Tests

        [Test]
        public async Task GetBalanceAsync_WithUrlFlag_ShouldIncludeUrlInCommand()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags { Url = "devnet" }
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--url devnet"));
        }

        [Test]
        public async Task GetBalanceAsync_WithOutputJsonFlag_ShouldIncludeOutputInCommand()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags { Output = "json" }
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--output json"));
        }

        [Test]
        public async Task GetBalanceAsync_WithProgram2022Flag_ShouldIncludeFlagInCommand()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags { Program2022 = true }
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--program-2022"));
        }

        [Test]
        public async Task GetBalanceAsync_WithAllGlobalFlags_ShouldIncludeAllFlagsInCommand()
        {
            // Arrange
            var request = new TokenBalanceRequest
            {
                TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags
                {
                    Config = "/path/to/config.yml",
                    FeePayer = "/path/to/fee-payer.json",
                    Output = "json",
                    ProgramId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA",
                    Program2022 = true,
                    Url = "mainnet-beta",
                    Verbose = true,
                    WithComputeUnitLimit = 200000,
                    WithComputeUnitPrice = 1000
                }
            };

            // Act
            var response = await _splTokenService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--config /path/to/config.yml"));
            Assert.That(response.Command, Does.Contain("--fee-payer /path/to/fee-payer.json"));
            Assert.That(response.Command, Does.Contain("--output json"));
            Assert.That(response.Command, Does.Contain("--program-id TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"));
            Assert.That(response.Command, Does.Contain("--program-2022"));
            Assert.That(response.Command, Does.Contain("--url mainnet-beta"));
            Assert.That(response.Command, Does.Contain("--verbose"));
            Assert.That(response.Command, Does.Contain("--with-compute-unit-limit 200000"));
            Assert.That(response.Command, Does.Contain("--with-compute-unit-price 1000"));
        }

        [Test]
        public async Task CreateTokenAsync_WithVerboseFlag_ShouldIncludeVerboseInCommand()
        {
            // Arrange
            var request = new CreateTokenRequest
            {
                Flags = new SplTokenGlobalFlags { Verbose = true }
            };

            // Act
            var response = await _splTokenService.CreateTokenAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--verbose"));
        }

        [Test]
        public async Task TransferAsync_WithFeePayerFlag_ShouldIncludeFeePayerInCommand()
        {
            // Arrange
            var request = new TransferTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 100m,
                RecipientAddress = "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
                Flags = new SplTokenGlobalFlags { FeePayer = "~/.config/solana/id.json" }
            };

            // Act
            var response = await _splTokenService.TransferAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--fee-payer ~/.config/solana/id.json"));
        }

        [Test]
        public async Task MintAsync_WithComputeUnitFlags_ShouldIncludeFlagsInCommand()
        {
            // Arrange
            var request = new MintTokensRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Amount = 1000m,
                Flags = new SplTokenGlobalFlags
                {
                    WithComputeUnitLimit = 150000,
                    WithComputeUnitPrice = 500
                }
            };

            // Act
            var response = await _splTokenService.MintAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--with-compute-unit-limit 150000"));
            Assert.That(response.Command, Does.Contain("--with-compute-unit-price 500"));
        }

        #endregion

        #region Response Properties Tests

        [Test]
        public async Task GetSupplyAsync_ResponseShouldHaveTimestamp()
        {
            // Arrange
            var request = new TokenSupplyRequest
            {
                TokenAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };
            var beforeCall = DateTime.UtcNow;

            // Act
            var response = await _splTokenService.GetSupplyAsync(request);

            // Assert
            var afterCall = DateTime.UtcNow;
            Assert.That(response.Timestamp, Is.GreaterThanOrEqualTo(beforeCall));
            Assert.That(response.Timestamp, Is.LessThanOrEqualTo(afterCall));
        }

        [Test]
        public async Task DisplayAsync_ResponseShouldHaveCommandProperty()
        {
            // Arrange
            var request = new DisplayRequest
            {
                Address = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
                Flags = new SplTokenGlobalFlags()
            };

            // Act
            var response = await _splTokenService.DisplayAsync(request);

            // Assert
            Assert.That(response.Command, Is.Not.Null);
            Assert.That(response.Command, Is.Not.Empty);
        }

        #endregion
    }
}
