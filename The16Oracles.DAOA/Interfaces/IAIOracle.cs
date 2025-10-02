using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Interfaces
{
    public interface IAIOracle
    {
        /// <summary>
        /// A human‐readable name for this oracle.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Evaluate this oracle against the given data bundle.
        /// </summary>
        Task<OracleResult> EvaluateAsync(DataBundle bundle);
    }
}
