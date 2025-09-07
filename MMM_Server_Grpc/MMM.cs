using Melanchall.DryWetMidi.Multimedia;
using MMM_Core;
using MMM_Core.MidiManagers;

namespace MMM_Server;

public sealed class MMM
{
	//Singleton pattern to ensure only one instance of DeviceManager exists
	private static MMM instance = new MMM();

	public IMidiPlaylist playlist;
	public IMidiPlayer player;

	private MidiOutputSystem midiOutputSystem = new MidiOutputSystem();
	public MidiSerialManager serialManager = new MidiSerialManager();
	public MidiPortInManager midiPortInManager = new MidiPortInManager();
	public MidiPortOutManager midiPortOutManager = new MidiPortOutManager();
	MidiCore midiCore = new MidiCore();

	public DeviceManager deviceManager = DeviceManager.Instance;

	public static void Reset() { }

	static MMM() { }
	private MMM()
	{
		playlist = new MidiPlaylist();
		player = new MidiPlayer(playlist);

		// Add default MIDI files to playlist
		DirectoryInfo directory = new DirectoryInfo(@"C:\Users\djber\source\repos\MMM_Core\MMM_ConsoleAppV2\DefaultMidiSongs");
		var midiFiles = new[]{
			new FileInfo(Path.Combine(directory.FullName, "NyanCat.mid")),
			new FileInfo(Path.Combine(directory.FullName, "Tetris.mid")),
			new FileInfo(Path.Combine(directory.FullName, "AllStar.mid")),
			new FileInfo(Path.Combine(directory.FullName, "Flight Of The Bumblebee.mid"))
		};

		foreach (var file in midiFiles)
		{
			playlist.AddSong(file);
		}

		//Connect MIDI Inputs
		midiCore.Connect((IInputDevice)player);
		midiCore.Connect((IInputDevice)serialManager);
		midiCore.Connect((IInputDevice)midiPortInManager);

		//Connect MIDI Outputs
		//midiCore.Connect((IOutputDevice)midiOutputSystem);
		midiCore.Connect((IOutputDevice)serialManager);
		midiCore.Connect((IOutputDevice)midiPortOutManager);

		////serialManager.AddConnection("COM6", 115200);
		}
	public static MMM Instance => instance;
}

