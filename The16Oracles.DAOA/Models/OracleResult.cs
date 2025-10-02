namespace The16Oracles.DAOA.Models
{
    public class OracleResult
    {
        public string ModuleName { get; set; }
        public double ConfidenceScore { get; set; }    // –1..+1 sell/buy bias
        public IDictionary<string, object> Metrics { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
