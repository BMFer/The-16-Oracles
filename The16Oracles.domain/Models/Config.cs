namespace The16Oracles.domain.Models
{
    public class Config
    {
        public string SolutionName { get; set; }
        public string SolutionDisplayName { get; set; }
        public string ProjectVersion { get; set; }
        public string Developer { get; set; }
        public Discord[] Discords { get; set; }

    }
}
