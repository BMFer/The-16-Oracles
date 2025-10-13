using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using The16Oracles.DAOA.Services;
using The16Oracles.DAOA.Models.Solana;

namespace The16Oracles.DAOA.nunit.Services
{
    /// <summary>
    /// Tests for SolanaService
    /// </summary>
    [TestFixture]
    public class SolanaServiceTests
    {
        private Mock<ILogger<SolanaService>> _mockLogger = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private SolanaService _solanaService = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SolanaService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _solanaService = new SolanaService(_mockLogger.Object, _mockConfiguration.Object);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new SolanaService(_mockLogger.Object, _mockConfiguration.Object);

            // Assert
            Assert.That(service, Is.Not.Null);
        }

        #endregion

        #region GetBalanceAsync Tests

        [Test]
        public async Task GetBalanceAsync_WithAddressAndDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new BalanceRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetBalanceAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana balance"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
            Assert.That(response.Timestamp, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task GetBalanceAsync_WithoutAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new BalanceRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetBalanceAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Is.EqualTo("solana balance"));
        }

        [Test]
        public async Task GetBalanceAsync_WithUrlFlag_ShouldIncludeUrlInCommand()
        {
            // Arrange
            var request = new BalanceRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags { Url = "devnet" }
            };

            // Act
            var response = await _solanaService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--url devnet"));
        }

        [Test]
        public async Task GetBalanceAsync_WithOutputJsonFlag_ShouldIncludeOutputInCommand()
        {
            // Arrange
            var request = new BalanceRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags { Output = "json" }
            };

            // Act
            var response = await _solanaService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--output json"));
        }

        #endregion

        #region TransferAsync Tests

        [Test]
        public async Task TransferAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new TransferRequest
            {
                RecipientAddress = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Amount = 1.5m,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.TransferAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana transfer"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
            Assert.That(response.Command, Does.Contain("1.5"));
        }

        [Test]
        public async Task TransferAsync_WithUrlFlag_ShouldIncludeUrlInCommand()
        {
            // Arrange
            var request = new TransferRequest
            {
                RecipientAddress = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Amount = 1.0m,
                Flags = new SolanaGlobalFlags { Url = "testnet" }
            };

            // Act
            var response = await _solanaService.TransferAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--url testnet"));
        }

        #endregion

        #region AirdropAsync Tests

        [Test]
        public async Task AirdropAsync_WithAmountOnly_ShouldReturnResponse()
        {
            // Arrange
            var request = new AirdropRequest
            {
                Amount = 2.0m,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.AirdropAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana airdrop"));
            Assert.That(response.Command, Does.Contain("2"));
        }

        [Test]
        public async Task AirdropAsync_WithAddressAndAmount_ShouldReturnResponse()
        {
            // Arrange
            var request = new AirdropRequest
            {
                Amount = 1.0m,
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.AirdropAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
        }

        #endregion

        #region GetAccountAsync Tests

        [Test]
        public async Task GetAccountAsync_WithValidAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new AccountRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana account"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
        }

        #endregion

        #region GetTransactionHistoryAsync Tests

        [Test]
        public async Task GetTransactionHistoryAsync_WithoutLimit_ShouldReturnResponse()
        {
            // Arrange
            var request = new TransactionHistoryRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetTransactionHistoryAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana transaction-history"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
        }

        [Test]
        public async Task GetTransactionHistoryAsync_WithLimit_ShouldIncludeLimitInCommand()
        {
            // Arrange
            var request = new TransactionHistoryRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Limit = 10,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetTransactionHistoryAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--limit 10"));
        }

        #endregion

        #region GetBlockAsync Tests

        [Test]
        public async Task GetBlockAsync_WithValidSlot_ShouldReturnResponse()
        {
            // Arrange
            var request = new BlockRequest
            {
                Slot = 123456789,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetBlockAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana block"));
            Assert.That(response.Command, Does.Contain("123456789"));
        }

        #endregion

        #region GetEpochInfoAsync Tests

        [Test]
        public async Task GetEpochInfoAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new EpochInfoRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetEpochInfoAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana epoch-info"));
        }

        [Test]
        public async Task GetEpochInfoAsync_WithUrlFlag_ShouldIncludeUrlInCommand()
        {
            // Arrange
            var request = new EpochInfoRequest
            {
                Flags = new SolanaGlobalFlags { Url = "mainnet-beta" }
            };

            // Act
            var response = await _solanaService.GetEpochInfoAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--url mainnet-beta"));
        }

        #endregion

        #region GetValidatorsAsync Tests

        [Test]
        public async Task GetValidatorsAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new ValidatorsRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetValidatorsAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana validators"));
        }

        #endregion

        #region Stake Account Tests

        [Test]
        public async Task GetStakeAccountAsync_WithValidAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new StakeAccountRequest
            {
                AccountAddress = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetStakeAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana stake-account"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
        }

        [Test]
        public async Task CreateStakeAccountAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new CreateStakeAccountRequest
            {
                AccountAddress = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Amount = 5.0m,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.CreateStakeAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana create-stake-account"));
            Assert.That(response.Command, Does.Contain("DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK"));
            Assert.That(response.Command, Does.Contain("5"));
        }

        [Test]
        public async Task DelegateStakeAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new DelegateStakeRequest
            {
                StakeAccount = "StakeAccount111111111111111111111111111111111",
                VoteAccount = "VoteAccount111111111111111111111111111111111",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.DelegateStakeAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana delegate-stake"));
            Assert.That(response.Command, Does.Contain("StakeAccount111111111111111111111111111111111"));
            Assert.That(response.Command, Does.Contain("VoteAccount111111111111111111111111111111111"));
        }

        #endregion

        #region GetVoteAccountAsync Tests

        [Test]
        public async Task GetVoteAccountAsync_WithValidAddress_ShouldReturnResponse()
        {
            // Arrange
            var request = new VoteAccountRequest
            {
                AccountAddress = "VoteAccount111111111111111111111111111111111",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetVoteAccountAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana vote-account"));
            Assert.That(response.Command, Does.Contain("VoteAccount111111111111111111111111111111111"));
        }

        #endregion

        #region Cluster Information Tests

        [Test]
        public async Task GetSupplyAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new SupplyRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetSupplyAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana supply"));
        }

        [Test]
        public async Task GetInflationAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new InflationRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetInflationAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana inflation"));
        }

        [Test]
        public async Task GetLargestAccountsAsync_WithoutLimit_ShouldReturnResponse()
        {
            // Arrange
            var request = new LargestAccountsRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetLargestAccountsAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana largest-accounts"));
        }

        [Test]
        public async Task GetLargestAccountsAsync_WithLimit_ShouldIncludeLimitInCommand()
        {
            // Arrange
            var request = new LargestAccountsRequest
            {
                Limit = 20,
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetLargestAccountsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--limit 20"));
        }

        #endregion

        #region Transaction Tests

        [Test]
        public async Task ConfirmTransactionAsync_WithValidSignature_ShouldReturnResponse()
        {
            // Arrange
            var request = new ConfirmRequest
            {
                Signature = "5VYYqFmfFfM7xQpbPkJaEPfF6fX6nJZPfH6fZ3fYfZfZ",
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.ConfirmTransactionAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana confirm"));
            Assert.That(response.Command, Does.Contain("5VYYqFmfFfM7xQpbPkJaEPfF6fX6nJZPfH6fZ3fYfZfZ"));
        }

        [Test]
        public async Task GetPrioritizationFeesAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var request = new PrioritizationFeesRequest
            {
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.GetPrioritizationFeesAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana recent-prioritization-fees"));
        }

        #endregion

        #region Simple Command Tests

        [Test]
        public async Task GetBlockHeightAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetBlockHeightAsync(flags);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana block-height"));
        }

        [Test]
        public async Task GetSlotAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetSlotAsync(flags);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana slot"));
        }

        [Test]
        public async Task GetClusterVersionAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetClusterVersionAsync(flags);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana cluster-version"));
        }

        [Test]
        public async Task GetGenesisHashAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetGenesisHashAsync(flags);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana genesis-hash"));
        }

        [Test]
        public async Task GetTransactionCountAsync_WithDefaultFlags_ShouldReturnResponse()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetTransactionCountAsync(flags);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana transaction-count"));
        }

        #endregion

        #region ExecuteCommandAsync Tests

        [Test]
        public async Task ExecuteCommandAsync_WithValidCommand_ShouldReturnResponse()
        {
            // Arrange
            var request = new SolanaCommandRequest
            {
                Command = "ping",
                Arguments = new Dictionary<string, string>(),
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.ExecuteCommandAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Command, Does.Contain("solana ping"));
        }

        [Test]
        public async Task ExecuteCommandAsync_WithArguments_ShouldIncludeArgumentsInCommand()
        {
            // Arrange
            var request = new SolanaCommandRequest
            {
                Command = "test-command",
                Arguments = new Dictionary<string, string>
                {
                    { "--arg1", "value1" },
                    { "--arg2", "value2" }
                },
                Flags = new SolanaGlobalFlags()
            };

            // Act
            var response = await _solanaService.ExecuteCommandAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--arg1 value1"));
            Assert.That(response.Command, Does.Contain("--arg2 value2"));
        }

        #endregion

        #region Global Flags Tests

        [Test]
        public async Task GetBalanceAsync_WithAllGlobalFlags_ShouldIncludeAllFlagsInCommand()
        {
            // Arrange
            var request = new BalanceRequest
            {
                Address = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Flags = new SolanaGlobalFlags
                {
                    Url = "devnet",
                    Keypair = "/path/to/keypair.json",
                    Config = "/path/to/config.yml",
                    Commitment = "finalized",
                    Output = "json",
                    Verbose = true,
                    NoAddressLabels = true,
                    SkipPreflight = true,
                    UseQuic = true,
                    UseTpuClient = true,
                    UseUdp = true
                }
            };

            // Act
            var response = await _solanaService.GetBalanceAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--url devnet"));
            Assert.That(response.Command, Does.Contain("--keypair /path/to/keypair.json"));
            Assert.That(response.Command, Does.Contain("--config /path/to/config.yml"));
            Assert.That(response.Command, Does.Contain("--commitment finalized"));
            Assert.That(response.Command, Does.Contain("--output json"));
            Assert.That(response.Command, Does.Contain("--verbose"));
            Assert.That(response.Command, Does.Contain("--no-address-labels"));
            Assert.That(response.Command, Does.Contain("--skip-preflight"));
            Assert.That(response.Command, Does.Contain("--use-quic"));
            Assert.That(response.Command, Does.Contain("--use-tpu-client"));
            Assert.That(response.Command, Does.Contain("--use-udp"));
        }

        [Test]
        public async Task GetEpochInfoAsync_WithCommitmentFlag_ShouldIncludeCommitmentInCommand()
        {
            // Arrange
            var request = new EpochInfoRequest
            {
                Flags = new SolanaGlobalFlags { Commitment = "confirmed" }
            };

            // Act
            var response = await _solanaService.GetEpochInfoAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--commitment confirmed"));
        }

        [Test]
        public async Task GetValidatorsAsync_WithKeypairFlag_ShouldIncludeKeypairInCommand()
        {
            // Arrange
            var request = new ValidatorsRequest
            {
                Flags = new SolanaGlobalFlags { Keypair = "~/.config/solana/id.json" }
            };

            // Act
            var response = await _solanaService.GetValidatorsAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--keypair ~/.config/solana/id.json"));
        }

        [Test]
        public async Task TransferAsync_WithVerboseFlag_ShouldIncludeVerboseInCommand()
        {
            // Arrange
            var request = new TransferRequest
            {
                RecipientAddress = "DYw8jCTfwHNRJhhmFcbXvVDTqWMEVFBX6ZKUmG5CNSKK",
                Amount = 1.0m,
                Flags = new SolanaGlobalFlags { Verbose = true }
            };

            // Act
            var response = await _solanaService.TransferAsync(request);

            // Assert
            Assert.That(response.Command, Does.Contain("--verbose"));
        }

        #endregion

        #region Response Properties Tests

        [Test]
        public async Task GetSlotAsync_ResponseShouldHaveTimestamp()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();
            var beforeCall = DateTime.UtcNow;

            // Act
            var response = await _solanaService.GetSlotAsync(flags);

            // Assert
            var afterCall = DateTime.UtcNow;
            Assert.That(response.Timestamp, Is.GreaterThanOrEqualTo(beforeCall));
            Assert.That(response.Timestamp, Is.LessThanOrEqualTo(afterCall));
        }

        [Test]
        public async Task GetClusterVersionAsync_ResponseShouldHaveCommandProperty()
        {
            // Arrange
            var flags = new SolanaGlobalFlags();

            // Act
            var response = await _solanaService.GetClusterVersionAsync(flags);

            // Assert
            Assert.That(response.Command, Is.Not.Null);
            Assert.That(response.Command, Is.Not.Empty);
        }

        #endregion
    }
}
