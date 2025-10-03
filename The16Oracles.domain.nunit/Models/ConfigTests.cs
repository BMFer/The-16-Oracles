using The16Oracles.domain.Models;

namespace The16Oracles.domain.nunit.Models
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void Config_ShouldSetAndGetProperties()
        {
            // Arrange
            var config = new Config
            {
                SolutionName = "The16Oracles",
                SolutionDisplayName = "The 16 Oracles",
                ProjectVersion = "1.0.0",
                Developer = "Test Developer",
                Discords = new Discord[0]
            };

            // Assert
            Assert.That(config.SolutionName, Is.EqualTo("The16Oracles"));
            Assert.That(config.SolutionDisplayName, Is.EqualTo("The 16 Oracles"));
            Assert.That(config.ProjectVersion, Is.EqualTo("1.0.0"));
            Assert.That(config.Developer, Is.EqualTo("Test Developer"));
            Assert.That(config.Discords, Is.Not.Null);
            Assert.That(config.Discords.Length, Is.EqualTo(0));
        }

        [Test]
        public void Config_ShouldHandleNullDiscords()
        {
            // Arrange
            var config = new Config
            {
                SolutionName = "Test",
                Discords = null
            };

            // Assert
            Assert.That(config.Discords, Is.Null);
        }

        [Test]
        public void Config_ShouldStoreMultipleDiscordInstances()
        {
            // Arrange
            var discords = new Discord[]
            {
                new Discord { Id = 1, Name = "Discord1" },
                new Discord { Id = 2, Name = "Discord2" }
            };

            var config = new Config
            {
                SolutionName = "Test",
                Discords = discords
            };

            // Assert
            Assert.That(config.Discords.Length, Is.EqualTo(2));
            Assert.That(config.Discords[0].Name, Is.EqualTo("Discord1"));
            Assert.That(config.Discords[1].Name, Is.EqualTo("Discord2"));
        }
    }
}
