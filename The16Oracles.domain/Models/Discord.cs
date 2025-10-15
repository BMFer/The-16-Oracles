namespace The16Oracles.domain.Models
{
    public class Discord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string CommandPrefix { get; set; }
        public string WelcomeChannelId { get; set; }
        public string AssetsChannelId { get; set; }
        public string LaunchpadUrl { get; set; }
        public string OracleWarsApiUrl { get; set; } = "https://localhost:5001";
        public Oracle[] Oracles { get; set; }
    }
}
