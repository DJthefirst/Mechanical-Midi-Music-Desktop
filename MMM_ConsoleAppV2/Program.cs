// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using CONSOLE_GUI;
using Grpc.Net.Client;
using MMM_Core;
using MMM_Server_Grpc;

class Program
{
	public static bool IsRunning = true;

	static void Main(string[] args)
	{
		var serviceCollection = new ServiceCollection();

		using var channel = GrpcChannel.ForAddress("https://localhost:7219");
		serviceCollection.AddTransient(sp => new SystemCMDs.SystemCMDsClient(channel));
		serviceCollection.AddTransient(sp => new PlayerCMDs.PlayerCMDsClient(channel));
		serviceCollection.AddTransient(sp => new PlaylistCMDs.PlaylistCMDsClient(channel));
		serviceCollection.AddTransient(sp => new ConnectionCMDs.ConnectionCMDsClient(channel));
		serviceCollection.AddTransient<ConsoleGUI>();

		var serviceProvider = serviceCollection.BuildServiceProvider();
		var clientSystemCMDs = serviceProvider.GetRequiredService<SystemCMDs.SystemCMDsClient>();
		var gui = serviceProvider.GetRequiredService<ConsoleGUI>();

		Console.CancelKeyPress += (sender, e) =>
		{
			e.Cancel = true;
			IsRunning = false;
			OnProcessExit(clientSystemCMDs);
		};

		gui.ConsoleInit();

		while (IsRunning)
		{
			gui.ConsolePeriodic();
		}
	}

	static void OnProcessExit(SystemCMDs.SystemCMDsClient clientSystemCMDs)
	{
		try
		{
			clientSystemCMDs.Quit(new Cmd { });
			Console.WriteLine("Stopped playing");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during shutdown: {ex.Message}");
		}
	}
}