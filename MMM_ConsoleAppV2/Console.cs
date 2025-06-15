


using Melanchall.DryWetMidi.Multimedia;
using MMM_Console;
using MMM_Core;

namespace CONSOLE_GUI;

public class ConsoleGUI()
{

    public void ConsoleInit()
    {
        Console.WriteLine("---------- Mechanical MIDI Music Terminal ----------");
        Console.WriteLine("To quit type 'q' or 'quit'.");
        Console.WriteLine("For help type 'h' or 'help'.");
    }

    public void ConsolePeriodic()
    {

        Console.Write("Mechaical MIDI Music: ");
        string? userInput = Console.ReadLine();
        if (userInput == null || userInput == "") return;
        string[] cmd = userInput.Split(" ", 2);
        string? pram1 = (cmd.Length > 1) ? cmd[1] : null;
        //string? pram2 = (cmd.Length > 2) ? cmd[2] : null;

        switch (cmd[0].ToUpper())
        {
            case "Q":
            case "QUIT":
                Cmd_QUIT(); return;
            case "H":
            case "HELP":
                Cmd_HELP(pram1); break;
            case "SA":
            case "SERIALAVAILABLE": Cmd_SERIALAVAILABLE(); break;
            case "SL":
            case "SERIALLIST": Cmd_SERIALLIST(); break;
            case "SC":
            case "SERIALCONNECT": Cmd_SERIALCONNECT(pram1); break;
            case "SD":
            case "SERIALDISCONNECT": Cmd_SERIALDISCONNECT(pram1); break;
            case "SR":
            case "SERIALRESET": Cmd_SERIALRESET(); break;
            case "IA":
            case "MIDIINAVAILABLE": Cmd_MIDIINAVAILABLE(); break;
            case "IL":
            case "MIDIINLIST": Cmd_MIDIINLIST(); break;
            case "IC":
            case "MIDIINCONNECT": Cmd_MIDIINCONNECT(pram1); break;
            case "ID":
            case "MIDIINDISCONNECT": Cmd_MIDIINDISCONNECT(pram1); break;
            case "IR":
            case "MIDIINRESET": Cmd_MIDIINRESET(); break;
            case "OA":
            case "MIDIOUTAVAILABLE": Cmd_MIDIOUTAVAILABLE(); break;
            case "OL":
            case "MIDIOUTLIST": Cmd_MIDIOUTLIST(); break;
            case "OC":
            case "MIDIOUTCONNECT": Cmd_MIDIOUTCONNECT(pram1); break;
            case "OD":
            case "MIDIOUTDISCONNECT": Cmd_MIDIOUTDISCONNECT(pram1); break;
            case "OR":
            case "MIDIOUTRESET": Cmd_MIDIOUTRESET(); break;
            case "SONGADD": Cmd_SONGADD(pram1); break;
            case "SONGREMOVE": Cmd_SONGREMOVE(pram1); break;
            case "SONGLIST": Cmd_SONGLIST(); break;
            case "REPEAT": Cmd_REPEAT(pram1); break;
            case "PLAYLISTCLEAR": Cmd_PLAYLISTCLEAR(); break;
            case "LOOP": Cmd_LOOP(pram1); break;
            case "AUTOPLAY": Cmd_AUTOPLAY(pram1); break;
            case "PL":
            case "PLAY": Cmd_PLAY(pram1); break;
            case "PA":
            case "PAUSE": Cmd_PAUSE(); break;
            case "S":
            case "STOP": Cmd_STOP(); break;
			case "ST":
			case "START": Cmd_START(); break;
			case "NE":
			case "NEXT": Cmd_NEXT(); break;
			case "PR":
			case "PREV": Cmd_PREV(); break;

            default:
                Console.WriteLine("Unrecognized Command!");
                Console.WriteLine("For help type 'help'");
                break;
        }
    }

    void Cmd_QUIT()
    {
		MMM.Instance.player.Stop();
        Console.WriteLine("MMM Terminated!");
        Console.ReadLine();
    }
    void Cmd_HELP(string? cmd)
    {
        if (cmd == null) { CommandList.display(); return; }
        CommandList.display(cmd);
    }
    void Cmd_SERIALAVAILABLE()
    {
        var ports = MMM.Instance.serialManager.AvailableConnections();
        if (ports.Count == 0) { Console.WriteLine("No available Serial ports"); return; }
        Console.WriteLine("---------- Available Serial Ports ----------\n");
        foreach (var port in ports)
        {
            Console.WriteLine("Name: " + port);
        }
        Console.WriteLine();
    }
    void Cmd_SERIALLIST()
    {
        var ports = MMM.Instance.serialManager.ListConnections();
		if (ports.Count == 0) { Console.WriteLine("No connected Serial ports"); return; }
        Console.WriteLine("---------- Connected Serial Ports ----------\n");
        foreach (var port in ports)
        {
            Console.WriteLine("Name: " + port);
        }
        Console.WriteLine();
    }
    void Cmd_SERIALCONNECT(string? cmd)
    {
        MMM.Instance.serialManager.AddConnection(cmd);
    }
    void Cmd_SERIALDISCONNECT(string? cmd)
    {
		MMM.Instance.serialManager.RemoveConnection(cmd);
	}
    void Cmd_SERIALRESET()
    {
        Console.WriteLine("TODO");
    }
    void Cmd_MIDIINAVAILABLE()
    {
        var ports = MMM.Instance.midiPortInManager.AvailableConnections();
        if (ports.Count == 0) { Console.WriteLine("No available MIDI In ports"); return; }
        Console.WriteLine("---------- Available MIDI In Ports ----------\n");
        foreach (var port in ports)
			Console.WriteLine("ID: " + port);
		    // Console.WriteLine("ID: " + InputDevice.GetByName(port).ToString());

		//{
		//    Console.WriteLine("ID: " + port.Item1);
		//    Console.WriteLine("Name: " + port.Item2);
		//    Console.WriteLine("Manufacturer: " + port.Item3);
		//    Console.WriteLine("Version: " + port.Item4);
		//    Console.WriteLine();
		//}
	}
    void Cmd_MIDIINLIST()
    {
		var ports = MMM.Instance.midiPortInManager.ListConnections();
        if (ports.Count == 0) { Console.WriteLine("No connected MIDI In ports"); return; }
        Console.WriteLine("---------- Connected MIDI In Ports ----------\n");
        foreach (var port in ports)
			Console.WriteLine("ID: " + port);

		//{
		//    Console.WriteLine("ID: " + port.Item1);
		//    Console.WriteLine("Name: " + port.Item2);
		//    Console.WriteLine("Manufacturer: " + port.Item3);
		//    Console.WriteLine("Version: " + port.Item4);
		//    Console.WriteLine();
		//}
	}
    void Cmd_MIDIINCONNECT(string? cmd)
    {
        MMM.Instance.midiPortInManager.AddConnection(cmd);
	}
    void Cmd_MIDIINDISCONNECT(string? cmd)
    {
        MMM.Instance.midiPortInManager.RemoveConnection(cmd);
	}
    void Cmd_MIDIINRESET()
    {
        MMM.Instance.midiPortInManager.ClearConnections();
	}
    void Cmd_MIDIOUTAVAILABLE()
    {
        //var ports = MMM_CoreDesktop.GetAvailableMidiOut();
        //if (ports.Count == 0) { Console.WriteLine("No available MIDI Out ports"); return; }
        //Console.WriteLine("---------- Available MIDI Out Ports ----------\n");
        //foreach (var port in ports)
        //{
        //    Console.WriteLine("ID: " + port.Item1);
        //    Console.WriteLine("Name: " + port.Item2);
        //    Console.WriteLine("Manufacturer: " + port.Item3);
        //    Console.WriteLine("Version: " + port.Item4);
        //    Console.WriteLine();
        //}
    }
    void Cmd_MIDIOUTLIST()
    {
        //var ports = MMM_CoreDesktop.ListConnectedOutputs();
        //if (ports.Count == 0) { Console.WriteLine("No connected MIDI Out ports"); return; }
        //Console.WriteLine("---------- Connected MIDI Out Ports ----------\n");
        //foreach (var port in ports)
        //{
        //    Console.WriteLine("ID: " + port.Item1);
        //    Console.WriteLine("Name: " + port.Item2);
        //    Console.WriteLine("Manufacturer: " + port.Item3);
        //    Console.WriteLine("Version: " + port.Item4);
        //    Console.WriteLine();
        //}
    }
    void Cmd_MIDIOUTCONNECT(string? cmd)
    {
        //MMM_CoreDesktop.AddMidiOutput(cmd);
    }
    void Cmd_MIDIOUTDISCONNECT(string? cmd)
    {
        //MMM_CoreDesktop.RemoveMidiOutput(cmd);
    }
    void Cmd_MIDIOUTRESET()
    {
        //MMM_CoreDesktop.ClearMidiOutputs();
    }
    void Cmd_SONGADD(string? cmd)
    {
		try
		{
			if (Directory.Exists(cmd))
			{
				MMM.Instance.playlist.AddDirectory(new DirectoryInfo(cmd));
			}
			else if (File.Exists(cmd) && Path.GetExtension(cmd).Equals(".mid", StringComparison.OrdinalIgnoreCase))
			{
				MMM.Instance.playlist.AddSong(new FileInfo(cmd));
			}
			else
			{
				Console.WriteLine("The provided path is invalid or not a MIDI file.");
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"An error occurred: {e.Message}");
		}
	}

    void Cmd_SONGREMOVE(string? cmd)
    {
		//MMM.Instance.playlist.RemoveSong(cmd);
    }
    void Cmd_SONGLIST()
    {
        var songNames = MMM.Instance.playlist.List.Select( i=> i.Name).ToList();
        if (songNames.Count == 0) { Console.WriteLine("No songs in playlist.\n"); return; }
        Console.WriteLine("\n---------- " + songNames.Count + " Songs in Playlist ----------");
        foreach (var name in songNames)
        {
            Console.WriteLine(name);
        }
        Console.WriteLine();
    }
    void Cmd_PLAYLISTCLEAR()
    {
		MMM.Instance.playlist.List.Clear();
    }
    void Cmd_REPEAT(string? cmd)
    {
        //MMM_CoreDesktop.RepeatSong(cmd.ToLower() == "true");
    }
    void Cmd_LOOP(string? cmd)
    {
        //MMM_CoreDesktop.LoopPlaylist(cmd.ToLower() == "true");
    }
    void Cmd_AUTOPLAY(string? cmd)
    {
        //MMM_CoreDesktop.AutoPlay(cmd.ToLower() == "true");
    }
    void Cmd_PLAY(string? cmd)
    {
        if (cmd == null) MMM.Instance.player.Play();
        //else
        //{
        //    MMM_CoreDesktop.Play(cmd);
        //    MMM_CoreDesktop.Stop();
        //    MMM_CoreDesktop.Play();
        //}
    }
    void Cmd_PAUSE()
    {
		MMM.Instance.player.Pause();
    }
    void Cmd_STOP()
    {
		MMM.Instance.player.Stop();
    }
    void Cmd_START()
    {
		MMM.Instance.player.Stop();
		MMM.Instance.player.Play();
    }
    void Cmd_NEXT()
    {
		MMM.Instance.player.Next();

	}
    void Cmd_PREV()
    {
		MMM.Instance.player.Prev();
	}

}