using Microsoft.Extensions.DependencyInjection;
using The16Oracles.domain.Services;

// Create the service collection container
var services = new ServiceCollection();

// Define Singletons
services.AddSingleton<IDataModel, DataModel>();

// Define Transients
services.AddTransient<TestService>();

// Create the service provider
var serviceProvider = services.BuildServiceProvider();

#region Test service with depenancy injection

// Get instance of the test service with the injected DataModel dependancy;
var testService = serviceProvider.GetService<TestService>();

// Write the result of a injected DataModel method
Console.WriteLine(testService?.GetDataModelName().Result);

// Wait for the console to be closed
Console.ReadLine();

#endregion