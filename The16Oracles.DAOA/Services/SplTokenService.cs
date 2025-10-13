using System.Diagnostics;
using System.Text;
using System.Text.Json;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models.SplToken;

namespace The16Oracles.DAOA.Services
{
    /// <summary>
    /// Service for executing SPL Token CLI commands
    /// </summary>
    public class SplTokenService : ISplTokenService
    {
        private readonly ILogger<SplTokenService> _logger;
        private readonly IConfiguration _configuration;

        public SplTokenService(ILogger<SplTokenService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // Account Management

        public async Task<SplTokenCommandResponse> GetAccountsAsync(TokenAccountsRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token accounts");

            if (!string.IsNullOrEmpty(request.TokenMintAddress))
            {
                commandBuilder.Append($" {request.TokenMintAddress}");
            }

            if (request.AddressesOnly)
            {
                commandBuilder.Append(" --addresses-only");
            }

            if (request.Delegated)
            {
                commandBuilder.Append(" --delegated");
            }

            if (request.ExternallyCloseable)
            {
                commandBuilder.Append(" --externally-closeable");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "accounts");
        }

        public async Task<SplTokenCommandResponse> GetAddressAsync(TokenAddressRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token address");

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            if (!string.IsNullOrEmpty(request.Token))
            {
                commandBuilder.Append($" --token {request.Token}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "address");
        }

        public async Task<SplTokenCommandResponse> GetBalanceAsync(TokenBalanceRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token balance");

            if (!string.IsNullOrEmpty(request.TokenMintAddress))
            {
                commandBuilder.Append($" {request.TokenMintAddress}");
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" --address {request.Address}");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "balance");
        }

        public async Task<SplTokenCommandResponse> CreateAccountAsync(CreateTokenAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token create-account {request.TokenMintAddress}");

            if (!string.IsNullOrEmpty(request.AccountKeypair))
            {
                commandBuilder.Append($" {request.AccountKeypair}");
            }

            if (request.Immutable)
            {
                commandBuilder.Append(" --immutable");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "create-account");
        }

        public async Task<SplTokenCommandResponse> CloseAccountAsync(CloseTokenAccountRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token close");

            if (!string.IsNullOrEmpty(request.TokenMintAddress))
            {
                commandBuilder.Append($" {request.TokenMintAddress}");
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" --address {request.Address}");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            if (!string.IsNullOrEmpty(request.CloseAuthority))
            {
                commandBuilder.Append($" --close-authority {request.CloseAuthority}");
            }

            if (!string.IsNullOrEmpty(request.Recipient))
            {
                commandBuilder.Append($" --recipient {request.Recipient}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "close");
        }

        public async Task<SplTokenCommandResponse> GarbageCollectAsync(GarbageCollectRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token gc");

            if (request.CloseEmptyAssociatedAccounts)
            {
                commandBuilder.Append(" --close-empty-associated-accounts");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "gc");
        }

        // Token Operations

        public async Task<SplTokenCommandResponse> CreateTokenAsync(CreateTokenRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token create-token");

            if (!string.IsNullOrEmpty(request.TokenKeypair))
            {
                commandBuilder.Append($" {request.TokenKeypair}");
            }

            if (!string.IsNullOrEmpty(request.MintAuthority))
            {
                commandBuilder.Append($" --mint-authority {request.MintAuthority}");
            }

            if (request.Decimals.HasValue)
            {
                commandBuilder.Append($" --decimals {request.Decimals.Value}");
            }

            if (request.EnableFreeze)
            {
                commandBuilder.Append(" --enable-freeze");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "create-token");
        }

        public async Task<SplTokenCommandResponse> MintAsync(MintTokensRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token mint {request.TokenAddress} {request.Amount}");

            if (!string.IsNullOrEmpty(request.RecipientAddress))
            {
                commandBuilder.Append($" {request.RecipientAddress}");
            }

            if (!string.IsNullOrEmpty(request.MintAuthority))
            {
                commandBuilder.Append($" --mint-authority {request.MintAuthority}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "mint");
        }

        public async Task<SplTokenCommandResponse> BurnAsync(BurnTokensRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token burn {request.AccountAddress} {request.Amount}");

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "burn");
        }

        public async Task<SplTokenCommandResponse> TransferAsync(TransferTokensRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token transfer {request.TokenAddress} {request.Amount} {request.RecipientAddress}");

            if (!string.IsNullOrEmpty(request.From))
            {
                commandBuilder.Append($" --from {request.From}");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            if (request.Fund)
            {
                commandBuilder.Append(" --fund");
            }

            if (request.AllowUnfundedRecipient)
            {
                commandBuilder.Append(" --allow-unfunded-recipient");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "transfer");
        }

        public async Task<SplTokenCommandResponse> GetSupplyAsync(TokenSupplyRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token supply {request.TokenAddress}");

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "supply");
        }

        public async Task<SplTokenCommandResponse> CloseMintAsync(CloseMintRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token close-mint {request.TokenAddress}");

            if (!string.IsNullOrEmpty(request.CloseAuthority))
            {
                commandBuilder.Append($" --close-authority {request.CloseAuthority}");
            }

            if (!string.IsNullOrEmpty(request.Recipient))
            {
                commandBuilder.Append($" --recipient {request.Recipient}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "close-mint");
        }

        // Token Delegation

        public async Task<SplTokenCommandResponse> ApproveAsync(ApproveRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token approve {request.AccountAddress} {request.Amount} {request.DelegateAddress}");

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "approve");
        }

        public async Task<SplTokenCommandResponse> RevokeAsync(RevokeRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token revoke {request.AccountAddress}");

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "revoke");
        }

        // Token Authority

        public async Task<SplTokenCommandResponse> AuthorizeAsync(AuthorizeRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token authorize {request.Address} {request.AuthorityType} {request.NewAuthority}");

            if (!string.IsNullOrEmpty(request.CurrentAuthority))
            {
                commandBuilder.Append($" --authority {request.CurrentAuthority}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "authorize");
        }

        // Freeze/Thaw Operations

        public async Task<SplTokenCommandResponse> FreezeAsync(FreezeAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token freeze {request.AccountAddress}");

            if (!string.IsNullOrEmpty(request.FreezeAuthority))
            {
                commandBuilder.Append($" --freeze-authority {request.FreezeAuthority}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "freeze");
        }

        public async Task<SplTokenCommandResponse> ThawAsync(ThawAccountRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token thaw {request.AccountAddress}");

            if (!string.IsNullOrEmpty(request.FreezeAuthority))
            {
                commandBuilder.Append($" --freeze-authority {request.FreezeAuthority}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "thaw");
        }

        // Native SOL Wrapping

        public async Task<SplTokenCommandResponse> WrapAsync(WrapRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token wrap {request.Amount}");

            if (!string.IsNullOrEmpty(request.WalletKeypair))
            {
                commandBuilder.Append($" --wallet-keypair {request.WalletKeypair}");
            }

            if (request.CreateAux)
            {
                commandBuilder.Append(" --create-aux-account");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "wrap");
        }

        public async Task<SplTokenCommandResponse> UnwrapAsync(UnwrapRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token unwrap");

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" {request.Address}");
            }

            if (!string.IsNullOrEmpty(request.WalletKeypair))
            {
                commandBuilder.Append($" --wallet-keypair {request.WalletKeypair}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "unwrap");
        }

        public async Task<SplTokenCommandResponse> SyncNativeAsync(SyncNativeRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token sync-native");

            if (!string.IsNullOrEmpty(request.Address))
            {
                commandBuilder.Append($" {request.Address}");
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                commandBuilder.Append($" --owner {request.Owner}");
            }

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "sync-native");
        }

        // Display

        public async Task<SplTokenCommandResponse> DisplayAsync(DisplayRequest request)
        {
            var commandBuilder = new StringBuilder($"spl-token display {request.Address}");

            AppendGlobalFlags(commandBuilder, request.Flags);

            return await ExecuteSplTokenCliAsync(commandBuilder.ToString(), "display");
        }

        // Generic Command Execution

        public async Task<SplTokenCommandResponse> ExecuteCommandAsync(SplTokenCommandRequest request)
        {
            var commandBuilder = new StringBuilder("spl-token ");
            commandBuilder.Append(request.Command);

            // Add arguments
            foreach (var arg in request.Arguments)
            {
                commandBuilder.Append($" {arg.Key} {arg.Value}");
            }

            // Add global flags
            AppendGlobalFlags(commandBuilder, request.Flags);

            var command = commandBuilder.ToString();
            return await ExecuteSplTokenCliAsync(command, request.Command);
        }

        // Helper Methods

        private void AppendGlobalFlags(StringBuilder commandBuilder, SplTokenGlobalFlags flags)
        {
            if (!string.IsNullOrEmpty(flags.Config))
            {
                commandBuilder.Append($" --config {flags.Config}");
            }

            if (!string.IsNullOrEmpty(flags.FeePayer))
            {
                commandBuilder.Append($" --fee-payer {flags.FeePayer}");
            }

            if (!string.IsNullOrEmpty(flags.Output))
            {
                commandBuilder.Append($" --output {flags.Output}");
            }

            if (!string.IsNullOrEmpty(flags.ProgramId))
            {
                commandBuilder.Append($" --program-id {flags.ProgramId}");
            }

            if (flags.Program2022)
            {
                commandBuilder.Append(" --program-2022");
            }

            if (!string.IsNullOrEmpty(flags.Url))
            {
                commandBuilder.Append($" --url {flags.Url}");
            }

            if (flags.Verbose)
            {
                commandBuilder.Append(" --verbose");
            }

            if (flags.WithComputeUnitLimit.HasValue)
            {
                commandBuilder.Append($" --with-compute-unit-limit {flags.WithComputeUnitLimit.Value}");
            }

            if (flags.WithComputeUnitPrice.HasValue)
            {
                commandBuilder.Append($" --with-compute-unit-price {flags.WithComputeUnitPrice.Value}");
            }
        }

        private async Task<SplTokenCommandResponse> ExecuteSplTokenCliAsync(string command, string commandName)
        {
            var response = new SplTokenCommandResponse
            {
                Command = command,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Executing SPL Token CLI command: {Command}", command);

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
                    _logger.LogError("SPL Token CLI command failed: {Command}, Error: {Error}", command, response.Error);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SPL Token CLI command: {Command}", command);
                response.Success = false;
                response.Error = ex.Message;
                response.ExitCode = -1;
                return response;
            }
        }
    }
}
