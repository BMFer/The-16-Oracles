namespace The16Oracles.DAOA.Models.Solana
{
    /// <summary>
    /// Base request for Solana CLI commands
    /// </summary>
    public class SolanaCommandRequest
    {
        /// <summary>
        /// The Solana CLI subcommand to execute
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Additional arguments for the command
        /// </summary>
        public Dictionary<string, string> Arguments { get; set; } = new();

        /// <summary>
        /// Global flags
        /// </summary>
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Global flags available for all Solana CLI commands
    /// </summary>
    public class SolanaGlobalFlags
    {
        /// <summary>
        /// URL for Solana's JSON RPC or moniker (mainnet-beta, testnet, devnet, localhost)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Filepath or URL to a keypair
        /// </summary>
        public string? Keypair { get; set; }

        /// <summary>
        /// Configuration file to use
        /// </summary>
        public string? Config { get; set; }

        /// <summary>
        /// Return information at the selected commitment level (processed, confirmed, finalized)
        /// </summary>
        public string? Commitment { get; set; }

        /// <summary>
        /// Return information in specified output format (json, json-compact)
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Show additional information
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Do not use address labels in the output
        /// </summary>
        public bool NoAddressLabels { get; set; }

        /// <summary>
        /// Skip the preflight check when sending transactions
        /// </summary>
        public bool SkipPreflight { get; set; }

        /// <summary>
        /// Use QUIC when sending transactions
        /// </summary>
        public bool UseQuic { get; set; }

        /// <summary>
        /// Use TPU client when sending transactions
        /// </summary>
        public bool UseTpuClient { get; set; }

        /// <summary>
        /// Use UDP when sending transactions
        /// </summary>
        public bool UseUdp { get; set; }
    }
}
