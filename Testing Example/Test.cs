// See https://aka.ms/new-console-template for more information

using Melanchall.DryWetMidi.Multimedia;
using MMM_Core;
using MMM_Core.MidiManagers;
using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;

var player = MMM.Instance.player;
var playlist = MMM.Instance.playlist;

DirectoryInfo directory = new DirectoryInfo(@"C:\Users\djber\source\repos\MMM_Core\MMM_ConsoleAppV2\DefaultMidiSongs");
FileInfo[] midiFiles = {
   new FileInfo(Path.Combine(directory.FullName, "NyanCat.mid")),
   new FileInfo(Path.Combine(directory.FullName, "Tetris.mid")),
   new FileInfo(Path.Combine(directory.FullName, "AllStar.mid")),
   new FileInfo(Path.Combine(directory.FullName, "Flight Of The Bumblebee.mid"))
};

foreach (var file in midiFiles)
{
	playlist.AddSong(file);
}

AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnProcessExit(player, e);
//Console.CancelKeyPress += (sender, e) => OnProcessExit(player, e);

//serialManager.AddConnection("COM6", 115200);
//MidiOutputConsole midiOutputConsole = new MidiOutputConsole();
//MidiCore midiCore = new MidiCore(new IInputDevice[] { (IInputDevice)player }, new IOutputDevice[] { midiOutputSystem, serialManager });
MidiCore midiCore = new MidiCore();


//midiCore.Connect((IOutputDevice)midiOutputSystem);

Console.WriteLine("---------- Mechanical MIDI Music Terminal ----------");
Console.WriteLine("To quit type 'q' or 'quit'.");
Console.WriteLine("For help type 'h' or 'help'.");

MMM.Instance.midiPortInManager.AddConnection("loopMIDI Port");

bool ComSerial3 = false;
bool ComSerial4 = false;

while (true)
{
	Console.WriteLine("Press '<' and '>' to navigate and, 'g' to play");
	switch (Console.ReadKey(true).Key)
	{
		case ConsoleKey.RightArrow:
			player.Next();
			Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
			break;

		case ConsoleKey.LeftArrow:
			player.Prev();
			Console.WriteLine($"Now playing: {player.Playlist.GetCurSong()?.Name}");
			break;

		case ConsoleKey.S:
			player.Stop();
			Console.WriteLine($"Stopped");
			break;

		case ConsoleKey.P:
			player.Pause();
			Console.WriteLine($"Paused");
			break;

		case ConsoleKey.G:
			player.Play();
			Console.WriteLine($"Play");
			break;

		case ConsoleKey.C:
			Console.WriteLine($"Connections:");
			var connections = MMM.Instance.serialManager.AvailableConnections();
			foreach (var connection in connections)
			{
				Console.WriteLine($"- {connection}");
			}
			break;

		case ConsoleKey.D3:
			if (!ComSerial3)
			{
				MMM.Instance.serialManager.AddConnection("COM3", 115200);
				ComSerial3 = true;
			}
			else
			{
				MMM.Instance.serialManager.RemoveConnection("COM3");
				ComSerial3 = false;
			}
			break;

		case ConsoleKey.D4:
			if (!ComSerial4)
			{
				MMM.Instance.serialManager.AddConnection("COM4", 115200);
				ComSerial4 = true;
			}
			else
			{
				MMM.Instance.serialManager.RemoveConnection("COM4");
				ComSerial4 = false;
			}
			break;

		case ConsoleKey.Q:
			goto end;

		default:
			break;
	}
}
end:

static void OnProcessExit(IMidiPlayer player, EventArgs e)
{
	player.Stop();
	Console.WriteLine("Stopped playing");
	Environment.Exit(0);
}