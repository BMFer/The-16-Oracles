using The16Oracles.domain.Models;

namespace The16Oracles.domain.nunit.Models
{
    [TestFixture]
    public class DiscordTests
    {
        [Test]
        public void Discord_ShouldSetAndGetAllProperties()
        {
            // Arrange
            var discord = new Discord
            {
                Id = 1,
                Name = "Test Discord",
                Token = "TEST_TOKEN",
                CommandPrefix = "!",
                WelcomeChannelId = "123456789",
                AssetsChannelId = "987654321",
                LaunchpadUrl = "https://test.com",
                Oracles = new Oracle[0]
            };

            // Assert
            Assert.That(discord.Id, Is.EqualTo(1));
            Assert.That(discord.Name, Is.EqualTo("Test Discord"));
            Assert.That(discord.Token, Is.EqualTo("TEST_TOKEN"));
            Assert.That(discord.CommandPrefix, Is.EqualTo("!"));
            Assert.That(discord.WelcomeChannelId, Is.EqualTo("123456789"));
            Assert.That(discord.AssetsChannelId, Is.EqualTo("987654321"));
            Assert.That(discord.LaunchpadUrl, Is.EqualTo("https://test.com"));
            Assert.That(discord.Oracles, Is.Not.Null);
        }

        [Test]
        public void Discord_ShouldHandleNullOracles()
        {
            // Arrange
            var discord = new Discord
            {
                Id = 1,
                Name = "Test",
                Oracles = null
            };

            // Assert
            Assert.That(discord.Oracles, Is.Null);
        }

        [Test]
        public void Discord_ShouldStoreMultipleOracles()
        {
            // Arrange
            var oracles = new Oracle[]
            {
                new Oracle { Id = 1, Name = "Oracle1" },
                new Oracle { Id = 2, Name = "Oracle2" },
                new Oracle { Id = 3, Name = "Oracle3" }
            };

            var discord = new Discord
            {
                Id = 1,
                Name = "Test Discord",
                Oracles = oracles
            };

            // Assert
            Assert.That(discord.Oracles.Length, Is.EqualTo(3));
            Assert.That(discord.Oracles[0].Name, Is.EqualTo("Oracle1"));
            Assert.That(discord.Oracles[1].Name, Is.EqualTo("Oracle2"));
            Assert.That(discord.Oracles[2].Name, Is.EqualTo("Oracle3"));
        }

        [Test]
        public void Discord_ShouldAllowDifferentCommandPrefixes()
        {
            // Arrange & Act
            var discord1 = new Discord { CommandPrefix = "!" };
            var discord2 = new Discord { CommandPrefix = "/" };
            var discord3 = new Discord { CommandPrefix = "?" };

            // Assert
            Assert.That(discord1.CommandPrefix, Is.EqualTo("!"));
            Assert.That(discord2.CommandPrefix, Is.EqualTo("/"));
            Assert.That(discord3.CommandPrefix, Is.EqualTo("?"));
        }
    }
}
