using Microsoft.Extensions.DependencyInjection;
using The16Oracles.domain.Services;

var services = new ServiceCollection();

services.AddSingleton<ITestService, TestService>();
services.AddTransient<TestService>();

var serviceProvider = services.BuildServiceProvider();
var worker = serviceProvider.GetService<TestService>();

Console.WriteLine(worker.Name);

Console.ReadLine();
