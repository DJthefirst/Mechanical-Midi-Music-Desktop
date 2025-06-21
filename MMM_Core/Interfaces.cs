using Melanchall.DryWetMidi.Multimedia;
using MMM_Core.MidiManagers;
using MMM_Device;
using System.IO.Ports;

namespace MMM_Core;

public class MMM
{
	//Singleton pattern to ensure only one instance of DeviceManager exists
	private static readonly MMM instance = new MMM();

	public IMidiPlaylist playlist;
	public IMidiPlayer player;

	private MidiOutputSystem midiOutputSystem = new MidiOutputSystem();
	public MidiSerialManager serialManager = new MidiSerialManager();
	public MidiPortInManager midiPortInManager = new MidiPortInManager();
	public MidiPortOutManager midiPortOutManager = new MidiPortOutManager();
	MidiCore midiCore = new MidiCore();

	static MMM() { }
	private MMM() {
		playlist = new MidiPlaylist();
		player = new MidiPlayer(playlist);

		//Connect MIDI Inputs
		midiCore.Connect((IInputDevice)player);
		midiCore.Connect((IInputDevice)serialManager);
		midiCore.Connect((IInputDevice)midiPortInManager);

		//Connect MIDI Outputs
		//midiCore.Connect((IOutputDevice)midiOutputSystem);
		midiCore.Connect((IOutputDevice)serialManager);
		midiCore.Connect((IOutputDevice)midiPortOutManager);
	}
	public static MMM Instance => instance;
}

public interface IMidiSong
{
	string Name { get; }
	FileInfo Path { get; }
}
public interface IMidiPlaylist
{
	DoubleLinkedList<IMidiSong> List { get; }
	IMidiSong? GetCurSong();
	void AddSong(FileInfo file);
	void AddSong(IMidiSong song);
	void AddDirectory(DirectoryInfo path);
	public IMidiSong? Next();
	public IMidiSong? Prev();

}
public interface IMidiPlayer
{
	IMidiPlaylist Playlist { get; }
	void Play();
	void Pause();
	void Stop();
	void Reset();
	void Repeat(bool repeat);
	public IMidiSong? Next();
	public IMidiSong? Prev();

}

public interface IManager : IDisposable
{
	public List<string> AvailableConnections();
	//public bool AddConnection(string connectionName);
	public bool RemoveConnection(string connectionName);
	public void ClearConnections();

}
public interface IInputManager : IManager, IInputDevice{}
public interface IOutputManager : IManager, IOutputDevice{}

public interface IDevice
{

}

public interface IDeviceList
{

}

