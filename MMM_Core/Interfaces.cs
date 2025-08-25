using Melanchall.DryWetMidi.Multimedia;
using MMM_Core.MidiManagers;
using MMM_Device;
using System.IO.Ports;

namespace MMM_Core;

public interface IMidiSong
{
	string Name { get; }
	FileInfo Path { get; }
	int Index { get;}
}
public interface IMidiPlaylist
{
	List<IMidiSong> Songs { get; }
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
	PLAYER_STATUS GetStatus();
	void Play();
	void Pause();
	void Stop();
	void Reset();
	// 0 = no repeat, 1 = repeat once, 2 = repeat forever
	void Repeat(uint repeat);
	// true = play next song
	void AutoPlay(bool autoPlay);
	// true = at end of playlist loop to start
	void Loop(bool loop);
	public IMidiSong? Next();
	public IMidiSong? Prev();
	public double GetMaxPositionMs();
	public double GetPositionMs();
	public double SetPositionMs(double position);
	public double NavigateMs(double increment);
	public byte[] ToByteArray();
	public void FromByteArray(byte[] data);

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

