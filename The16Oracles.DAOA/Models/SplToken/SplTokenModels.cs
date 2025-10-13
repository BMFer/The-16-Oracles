namespace The16Oracles.DAOA.Models.SplToken
{
    /// <summary>
    /// Global flags available for all SPL Token CLI commands
    /// </summary>
    public class SplTokenGlobalFlags
    {
        /// <summary>
        /// Configuration file to use
        /// </summary>
        public string? Config { get; set; }

        /// <summary>
        /// Specify the fee-payer account keypair file, ASK keyword, or pubkey of offline signer
        /// </summary>
        public string? FeePayer { get; set; }

        /// <summary>
        /// Return information in specified output format (json, json-compact)
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// SPL Token program id
        /// </summary>
        public string? ProgramId { get; set; }

        /// <summary>
        /// Use token extension program token 2022
        /// </summary>
        public bool Program2022 { get; set; }

        /// <summary>
        /// URL for Solana's JSON RPC or moniker (mainnet-beta, testnet, devnet, localhost)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Show additional information
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Set compute unit limit for transaction, in compute units
        /// </summary>
        public long? WithComputeUnitLimit { get; set; }

        /// <summary>
        /// Set compute unit price for transaction, in increments of 0.000001 lamports per compute unit
        /// </summary>
        public long? WithComputeUnitPrice { get; set; }
    }

    /// <summary>
    /// Response from executing an SPL Token CLI command
    /// </summary>
    public class SplTokenCommandResponse
    {
        /// <summary>
        /// The command that was executed
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the command executed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Standard output from the command
        /// </summary>
        public string Output { get; set; } = string.Empty;

        /// <summary>
        /// Error output if the command failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Exit code from the command
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Timestamp when the command was executed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Parsed result data (if output is JSON)
        /// </summary>
        public object? Data { get; set; }
    }
}
