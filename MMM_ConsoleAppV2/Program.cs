// See https://aka.ms/new-console-template for more information

using Melanchall.DryWetMidi.Multimedia;
using MMM_Core;
using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;
using CONSOLE_GUI;

ConsoleGUI gui = new ConsoleGUI();


DirectoryInfo directory = new DirectoryInfo(@"C:\Users\djber\source\repos\MMM_Core\MMM_ConsoleAppV2\DefaultMidiSongs");
FileInfo[] midiFiles = {
   new FileInfo(Path.Combine(directory.FullName, "NyanCat.mid")),
   new FileInfo(Path.Combine(directory.FullName, "Tetris.mid")),
   new FileInfo(Path.Combine(directory.FullName, "AllStar.mid")),
   new FileInfo(Path.Combine(directory.FullName, "Flight Of The Bumblebee.mid"))
};

foreach (var file in midiFiles)
{
	MMM.Instance.playlist.AddSong(file);
}

AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnProcessExit(MMM.Instance.player, e);
//Console.CancelKeyPress += (sender, e) => OnProcessExit(player, e);

gui.ConsoleInit();

while (true)
{

	gui.ConsolePeriodic();
	//Console.WriteLine("Press '<' and '>' to navigate and, 'g' to play");
	//switch (Console.ReadKey(true).Key)
	//{
	//	case ConsoleKey.RightArrow:
	//		player.Next();
	//		Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
	//		break;

	//	case ConsoleKey.LeftArrow:
	//		player.Prev();
	//		Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
	//		break;

	//	case ConsoleKey.S:
	//		player.Stop();
	//		Console.WriteLine($"Stopped");
	//		break;

	//	case ConsoleKey.P:
	//		player.Pause();
	//		Console.WriteLine($"Paused");
	//		break;

	//	case ConsoleKey.G:
	//		player.Play();
	//		Console.WriteLine($"Play");
	//		break;

	//	case ConsoleKey.C:
	//		Console.WriteLine($"Connections:");
	//		var connections = serialManager.AvailableConnections();
	//		foreach (var connection in connections)
	//		{
	//			Console.WriteLine($"- {connection}");
	//		}
	//		break;

	//	case ConsoleKey.Q:
	//		goto end;

	//	default:
	//		break;
	//}
}
//end:

static void OnProcessExit(IMidiPlayer player, EventArgs e)
{
	player.Stop();
	Console.WriteLine("Stopped playing");
	Environment.Exit(0);
}