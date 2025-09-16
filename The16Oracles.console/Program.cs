using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using The16Oracles.domain.Models;
using The16Oracles.domain.Services;

// Load up the configuration data for the solutions settings.
var config = LoadConfig();

var RubbishCollectorConfig = config?.Discords.Where(discord => discord.Id == 1).FirstOrDefault();

Console.WriteLine(RubbishCollectorConfig?.Name);

// Create the service collection container
var services = new ServiceCollection();

// Define Singletons
services.AddSingleton<IDataModel, DataModel>();
services.AddSingleton<IBotService, BotService>();

// Define Transients
services.AddTransient<TestService>();
services.AddTransient<BotService>();

// Create the service provider
var serviceProvider = services.BuildServiceProvider();

#region Test services and depenancy injection

// Get instance of the test service with the injected DataModel dependancy;
var testService = serviceProvider.GetService<TestService>();
var botService = serviceProvider.GetService<BotService>();

// Write the result of a injected DataModel method
Console.WriteLine(testService?.GetDataModelName().Result);

var rubbishBot = await botService.GetDiscordBotAsync(RubbishCollectorConfig);

Console.WriteLine($"The {rubbishBot.Name} bot is loaded and ready!");

// Wait for the console to be closed
Console.ReadLine();

#endregion

// TODO: Add this code to the bot so it will be
// able to load it's configurations on demand.
static Config? LoadConfig()
{
    if (File.Exists("config.json"))
    {
        // Collect tyhe data from the config file.
        var data = File.ReadAllText("config.json");
        // return the configuration as a strong type object.
        return JsonConvert.DeserializeObject<Config>(data);
    }

    // Send exception for missing configuration data.
    throw new Exception("Missing configuration data");
}