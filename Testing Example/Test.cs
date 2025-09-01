// See https://aka.ms/new-console-template for more information

//using Melanchall.DryWetMidi.Multimedia;
//using MMM_Core;
//using System.IO.Ports;
//using static System.Net.Mime.MediaTypeNames;

using Grpc.Net.Client;
using MMM_Server;

//var player = MMM.Instance.player;
//var playlist = MMM.Instance.playlist;

//DirectoryInfo directory = new DirectoryInfo(@"C:\Users\djber\source\repos\MMM_Core\MMM_ConsoleAppV2\DefaultMidiSongs");
//FileInfo[] midiFiles = {
//   new FileInfo(Path.Combine(directory.FullName, "NyanCat.mid")),
//   new FileInfo(Path.Combine(directory.FullName, "Tetris.mid")),
//   new FileInfo(Path.Combine(directory.FullName, "AllStar.mid")),
//   new FileInfo(Path.Combine(directory.FullName, "Flight Of The Bumblebee.mid"))
//};

//foreach (var file in midiFiles)
//{
//	playlist.AddSong(file);
//}

////Console.CancelKeyPress += (sender, e) => OnProcessExit(player, e);

////serialManager.AddConnection("COM6", 115200);
////MidiOutputConsole midiOutputConsole = new MidiOutputConsole();
////MidiCore midiCore = new MidiCore(new IInputDevice[] { (IInputDevice)player }, new IOutputDevice[] { midiOutputSystem, serialManager });
//MidiCore midiCore = new MidiCore();


////midiCore.Connect((IOutputDevice)midiOutputSystem);

class Test
{
	private readonly SystemCMDs.SystemCMDsClient clientSystemCMDs;
	private readonly PlayerCMDs.PlayerCMDsClient clientPlayerCMDs;
	private readonly Cmd cmd;

	public Test(SystemCMDs.SystemCMDsClient clientSystemCMDs, PlayerCMDs.PlayerCMDsClient clientPlayerCMDs)
	{
		this.clientSystemCMDs = clientSystemCMDs;
		this.clientPlayerCMDs = clientPlayerCMDs;
	}

	static async Task Main(string[] args)
	{
		Console.WriteLine("---------- Mechanical MIDI Music Terminal ----------");
		Console.WriteLine("To quit type 'q' or 'quit'.");
		Console.WriteLine("For help type 'h' or 'help'.");

		using var channel = GrpcChannel.ForAddress("https://localhost:7219");
		var clientSystemCMDs = new SystemCMDs.SystemCMDsClient(channel);
		var clientPlayerCMDs = new PlayerCMDs.PlayerCMDsClient(channel);

		var testInstance = new Test(clientSystemCMDs, clientPlayerCMDs);

		while (true)
		{
			Console.WriteLine("Press '<' and '>' to navigate and, 'g' to play");
			var key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Q)
			{
				Console.WriteLine("Exiting...");
				break;
			}
			await testInstance.Periodic(key);
		}
	}

	public async Task Periodic(ConsoleKey key)
	{
		switch (key)
		{
			case ConsoleKey.RightArrow:
				//player.Next();
				//Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
				break;

			case ConsoleKey.LeftArrow:
				//player.Prev();
				//Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
				break;

			case ConsoleKey.H:
				Console.WriteLine((await clientSystemCMDs.HelpAsync(new Cmd {})).Message);
				break;

			case ConsoleKey.P:
				await clientPlayerCMDs.StatusAsync(new ModeCmd { IsSetter = true, Mode = 3 });
				Console.WriteLine($"Paused");
				break;

			case ConsoleKey.G:
				await clientPlayerCMDs.StatusAsync(new ModeCmd { IsSetter = true, Mode = 1 });
				Console.WriteLine($"Play");
				break;

			case ConsoleKey.S:
				await clientPlayerCMDs.StatusAsync(new ModeCmd { IsSetter = true, Mode = 0 });
				Console.WriteLine($"Stop");
				break;

			case ConsoleKey.C:
				Console.WriteLine($"Connections:");
				//var connections = MMM.Instance.serialManager.AvailableConnections();
				//foreach (var connection in connections)
				//{
				//	Console.WriteLine($"- {connection}");
				//}
				break;

			case ConsoleKey.D3:
				//if (!ComSerial3)
				//{
				//	MMM.Instance.serialManager.AddConnection("COM3", 115200);
				//	ComSerial3 = true;
				//}
				//else
				//{
				//	MMM.Instance.serialManager.RemoveConnection("COM3");
				//	ComSerial3 = false;
				//}
				break;

			case ConsoleKey.D4:
				//if (!ComSerial4)
				//{
				//	MMM.Instance.serialManager.AddConnection("COM4", 115200);
				//	ComSerial4 = true;
				//}
				//else
				//{
				//	MMM.Instance.serialManager.RemoveConnection("COM4");
				//	ComSerial4 = false;
				//}
				break;

			case ConsoleKey.Q:
				break;

			default:
				break;
		}
	}
}