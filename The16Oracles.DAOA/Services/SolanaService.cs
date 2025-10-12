using System.Diagnostics;
using System.Text;
using System.Text.Json;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models.Solana;

namespace The16Oracles.DAOA.Services
{
    /// <summary>
    /// Service for executing Solana CLI commands
    /// </summary>
    public class SolanaService : ISolanaService
    {
        private readonly ILogger<SolanaService> _logger;
        private readonly IConfiguration _configuration;

        public SolanaService(ILogger<SolanaService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<SolanaCommandResponse> ExecuteCommandAsync(SolanaCommandRequest request)
        {
            var commandBuilder = new StringBuilder("solana ");
            commandBuilder.Append(request.Command);

            // Add arguments
            foreach (var arg in request.Arguments)
            {
                commandBuilder.Append($" {arg.Key} {arg.Value}");
            }

            // Add global flags
            AppendGlobalFlags(commandBuilder, request.Flags);

            var command = commandBuilder.ToString();
            return await ExecuteSolanaCliAsync(command, request.Command);
        }

        public async Task<SolanaCommandResponse> GetBalanceAsync(BalanceRequest request)
        {
            var commandBuilder = new StringBuilder("solana balance");

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" {request.Address}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "balance");
        }

        public async Task<SolanaCommandResponse> TransferAsync(TransferRequest request)
        {
            var commandBuilder = new StringBuilder($"solana transfer {request.RecipientAddress} {request.Amount}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "transfer");
        }

        public async Task<SolanaCommandResponse> AirdropAsync(AirdropRequest request)
        {
            var commandBuilder = new StringBuilder($"solana airdrop {request.Amount}");

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" {request.Address}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "airdrop");
        }

        public async Task<SolanaCommandResponse> GetAccountAsync(AccountRequest request)
        {
            var commandBuilder = new StringBuilder($"solana account {request.Address}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "account");
        }

        public async Task<SolanaCommandResponse> GetTransactionHistoryAsync(TransactionHistoryRequest request)
        {
            var commandBuilder = new StringBuilder($"solana transaction-history {request.Address}");

            if (request.Limit.HasValue)
            {
                commandBuilder.Append($" --limit {request.Limit.Value}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "transaction-history");
        }

        public async Task<SolanaCommandResponse> GetBlockAsync(BlockRequest request)
        {
            var commandBuilder = new StringBuilder($"solana block {request.Slot}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "block");
        }

        public async Task<SolanaCommandResponse> GetEpochInfoAsync(EpochInfoRequest request)
        {
            var commandBuilder = new StringBuilder("solana epoch-info");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "epoch-info");
        }

        public async Task<SolanaCommandResponse> GetValidatorsAsync(ValidatorsRequest request)
        {
            var commandBuilder = new StringBuilder("solana validators");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "validators");
        }

        public async Task<SolanaCommandResponse> GetStakeAccountAsync(StakeAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"solana stake-account {request.AccountAddress}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "stake-account");
        }

        public async Task<SolanaCommandResponse> CreateStakeAccountAsync(CreateStakeAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"solana create-stake-account {request.AccountAddress} {request.Amount}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "create-stake-account");
        }

        public async Task<SolanaCommandResponse> DelegateStakeAsync(DelegateStakeRequest request)
        {
            var commandBuilder = new StringBuilder($"solana delegate-stake {request.StakeAccount} {request.VoteAccount}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "delegate-stake");
        }

        public async Task<SolanaCommandResponse> GetVoteAccountAsync(VoteAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"solana vote-account {request.AccountAddress}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "vote-account");
        }

        public async Task<SolanaCommandResponse> GetSupplyAsync(SupplyRequest request)
        {
            var commandBuilder = new StringBuilder("solana supply");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "supply");
        }

        public async Task<SolanaCommandResponse> GetInflationAsync(InflationRequest request)
        {
            var commandBuilder = new StringBuilder("solana inflation");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "inflation");
        }

        public async Task<SolanaCommandResponse> GetLargestAccountsAsync(LargestAccountsRequest request)
        {
            var commandBuilder = new StringBuilder("solana largest-accounts");

            if (request.Limit.HasValue)
            {
                commandBuilder.Append($" --limit {request.Limit.Value}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "largest-accounts");
        }

        public async Task<SolanaCommandResponse> ConfirmTransactionAsync(ConfirmRequest request)
        {
            var commandBuilder = new StringBuilder($"solana confirm {request.Signature}");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "confirm");
        }

        public async Task<SolanaCommandResponse> GetPrioritizationFeesAsync(PrioritizationFeesRequest request)
        {
            var commandBuilder = new StringBuilder("solana recent-prioritization-fees");
            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "recent-prioritization-fees");
        }

        public async Task<SolanaCommandResponse> GetBlockHeightAsync(SolanaGlobalFlags flags)
        {
            var commandBuilder = new StringBuilder("solana block-height");
            AppendGlobalFlags(commandBuilder, flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "block-height");
        }

        public async Task<SolanaCommandResponse> GetSlotAsync(SolanaGlobalFlags flags)
        {
            var commandBuilder = new StringBuilder("solana slot");
            AppendGlobalFlags(commandBuilder, flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "slot");
        }

        public async Task<SolanaCommandResponse> GetClusterVersionAsync(SolanaGlobalFlags flags)
        {
            var commandBuilder = new StringBuilder("solana cluster-version");
            AppendGlobalFlags(commandBuilder, flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "cluster-version");
        }

        public async Task<SolanaCommandResponse> GetGenesisHashAsync(SolanaGlobalFlags flags)
        {
            var commandBuilder = new StringBuilder("solana genesis-hash");
            AppendGlobalFlags(commandBuilder, flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "genesis-hash");
        }

        public async Task<SolanaCommandResponse> GetTransactionCountAsync(SolanaGlobalFlags flags)
        {
            var commandBuilder = new StringBuilder("solana transaction-count");
            AppendGlobalFlags(commandBuilder, flags);

            return await ExecuteSolanaCliAsync(commandBuilder.ToString(), "transaction-count");
        }

        private void AppendGlobalFlags(StringBuilder commandBuilder, SolanaGlobalFlags flags)
        {
            if (!string.IsNullOrEmpty(flags.Url))
            {
                commandBuilder.Append($" --url {flags.Url}");
            }

            if (!string.IsNullOrEmpty(flags.Keypair))
            {
                commandBuilder.Append($" --keypair {flags.Keypair}");
            }

            if (!string.IsNullOrEmpty(flags.Config))
            {
                commandBuilder.Append($" --config {flags.Config}");
            }

            if (!string.IsNullOrEmpty(flags.Commitment))
            {
                commandBuilder.Append($" --commitment {flags.Commitment}");
            }

            if (!string.IsNullOrEmpty(flags.Output))
            {
                commandBuilder.Append($" --output {flags.Output}");
            }

            if (flags.Verbose)
            {
                commandBuilder.Append(" --verbose");
            }

            if (flags.NoAddressLabels)
            {
                commandBuilder.Append(" --no-address-labels");
            }

            if (flags.SkipPreflight)
            {
                commandBuilder.Append(" --skip-preflight");
            }

            if (flags.UseQuic)
            {
                commandBuilder.Append(" --use-quic");
            }

            if (flags.UseTpuClient)
            {
                commandBuilder.Append(" --use-tpu-client");
            }

            if (flags.UseUdp)
            {
                commandBuilder.Append(" --use-udp");
            }
        }

        private async Task<SolanaCommandResponse> ExecuteSolanaCliAsync(string command, string commandName)
        {
            var response = new SolanaCommandResponse
            {
                Command = command,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Executing Solana CLI command: {Command}", command);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                response.Output = await outputTask;
                response.Error = await errorTask;
                response.ExitCode = process.ExitCode;
                response.Success = process.ExitCode == 0;

                // Try to parse JSON output if --output json flag was used
                if (response.Success && !string.IsNullOrWhiteSpace(response.Output))
                {
                    try
                    {
                        response.Data = JsonSerializer.Deserialize<object>(response.Output);
                    }
                    catch
                    {
                        // Not JSON, leave Data as null
                    }
                }

                if (!response.Success)
                {
                    _logger.LogError("Solana CLI command failed: {Command}, Error: {Error}", command, response.Error);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Solana CLI command: {Command}", command);
                response.Success = false;
                response.Error = ex.Message;
                response.ExitCode = -1;
                return response;
            }
        }
    }
}
