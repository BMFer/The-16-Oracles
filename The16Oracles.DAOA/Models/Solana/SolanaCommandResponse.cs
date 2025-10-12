namespace The16Oracles.DAOA.Models.Solana
{
    /// <summary>
    /// Response from executing a Solana CLI command
    /// </summary>
    public class SolanaCommandResponse
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
