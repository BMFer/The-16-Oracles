using The16Oracles.domain.Models;

namespace The16Oracles.domain.nunit.Models
{
    [TestFixture]
    public class OracleTests
    {
        [Test]
        public void Oracle_ShouldSetAndGetId()
        {
            // Arrange
            var oracle = new Oracle { Id = 1 };

            // Assert
            Assert.That(oracle.Id, Is.EqualTo(1));
        }

        [Test]
        public void Oracle_ShouldSetAndGetName()
        {
            // Arrange
            var oracle = new Oracle { Name = "Macro Market Trends" };

            // Assert
            Assert.That(oracle.Name, Is.EqualTo("Macro Market Trends"));
        }

        [Test]
        public void Oracle_ShouldSetBothIdAndName()
        {
            // Arrange
            var oracle = new Oracle
            {
                Id = 5,
                Name = "Black Swan Event Detection"
            };

            // Assert
            Assert.That(oracle.Id, Is.EqualTo(5));
            Assert.That(oracle.Name, Is.EqualTo("Black Swan Event Detection"));
        }

        [Test]
        public void Oracle_ShouldHandleAllSixteenOracleTypes()
        {
            // Arrange
            var oracleNames = new[]
            {
                "Macro Market Trends",
                "DeFi Liquidity Flows",
                "Whale Wallet Activity",
                "NFT Market Sentiment",
                "Black Swan Event Detection",
                "Rug Pull Risk Analysis",
                "Regulatory Risk Monitor",
                "Airdrop & Launch Tracker",
                "Emerging Market Surge Detector",
                "Layer-2 Activity Metrics",
                "Cross-Chain Interoperability",
                "Validator & Node Economics",
                "AI & Automation Narratives",
                "Tokenomics Innovation Tracker",
                "Technology Adoption Curves",
                "Stablecoin Flow Analysis"
            };

            // Act
            var oracles = new Oracle[16];
            for (int i = 0; i < 16; i++)
            {
                oracles[i] = new Oracle
                {
                    Id = i + 1,
                    Name = oracleNames[i]
                };
            }

            // Assert
            Assert.That(oracles.Length, Is.EqualTo(16));
            Assert.That(oracles[0].Name, Is.EqualTo("Macro Market Trends"));
            Assert.That(oracles[15].Name, Is.EqualTo("Stablecoin Flow Analysis"));
            Assert.That(oracles[0].Id, Is.EqualTo(1));
            Assert.That(oracles[15].Id, Is.EqualTo(16));
        }

        [Test]
        public void Oracle_ShouldAllowNullName()
        {
            // Arrange
            var oracle = new Oracle { Id = 1, Name = null };

            // Assert
            Assert.That(oracle.Name, Is.Null);
        }
    }
}
