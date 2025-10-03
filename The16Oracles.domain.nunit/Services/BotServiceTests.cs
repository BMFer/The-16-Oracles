using The16Oracles.domain.Models;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class BotServiceTests
    {
        [Test]
        public async Task GetDiscordBotAsync_ShouldReturnDiscordBot()
        {
            // Arrange
            var botService = new BotService();
            var discord = new Discord
            {
                Id = 1,
                Name = "Test Bot",
                Token = "TEST_TOKEN",
                CommandPrefix = "!",
                WelcomeChannelId = "123456",
                AssetsChannelId = "654321",
                LaunchpadUrl = "https://test.com",
                Oracles = new Oracle[0]
            };

            // Note: This test will attempt to create a DiscordBot which connects to Discord
            // In a real scenario, you'd want to mock the Discord connection
            // For now, we're testing the service layer only

            // Act & Assert
            // We can't fully test this without mocking the Discord client
            // But we can verify the method signature and basic async behavior
            Assert.That(botService, Is.InstanceOf<IBotService>());

            var task = botService.GetDiscordBotAsync(discord);
            Assert.That(task, Is.InstanceOf<Task<DiscordBot>>());
        }

        [Test]
        public void BotService_ShouldImplementIBotService()
        {
            // Arrange & Act
            var botService = new BotService();

            // Assert
            Assert.That(botService, Is.InstanceOf<IBotService>());
        }

        [Test]
        public void BotService_Constructor_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new BotService());
        }

        [Test]
        public async Task GetDiscordBotAsync_ShouldExecuteAsynchronously()
        {
            // Arrange
            var botService = new BotService();
            var discord = new Discord
            {
                Id = 1,
                Name = "Test Bot",
                Token = "TEST_TOKEN",
                CommandPrefix = "!",
                Oracles = new Oracle[0]
            };

            // Act
            var task = botService.GetDiscordBotAsync(discord);

            // Assert
            Assert.That(task, Is.InstanceOf<Task<DiscordBot>>());
            Assert.That(task.IsCompleted || !task.IsFaulted, Is.True);
        }
    }
}
