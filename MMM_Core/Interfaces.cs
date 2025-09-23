using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using MMM_Core.MidiManagers;
using MMM_Device;
using System.IO.Ports;
using static MMM_Core.MidiPlayer;

namespace MMM_Core;

public interface IMidiSong
{
	string Name { get; }
	FileInfo Path { get; }
	double Duration { get; }
	string DurationTimestamp { get; }
	int Index { get;}
}
public interface IMidiPlaylist
{
	public event EventHandler<IMidiSong>? OnSongChanged;
	List<IMidiSong> Songs { get; }
	IMidiSong? GetCurSong();
	IMidiSong? GetSongByName(string name);
	void SetCurSong(string name);
	void AddSong(FileInfo file);
	void AddSong(IMidiSong song);
	void AddDirectory(DirectoryInfo path);
	void RemoveSong(IMidiSong song);
	public IMidiSong? Next();
	public IMidiSong? Prev();
}

public interface IMidiPlayer
{

	IMidiPlaylist Playlist { get; }
	public event EventHandler<PlaybackUpdateEventArgs> OnPlaybackTimeUpdated;
	public event EventHandler<MidiEventReceivedEventArgs> EventReceived;
	PLAYER_STATUS GetStatus();
	void Play();
	public void Play(string songName);
	public void Play(IMidiSong song);
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
	public void RemoveConnection(IConnection connection);
	public bool RemoveConnection(string connectionName);
	public void ClearConnections();

}
public interface IInputManager : IManager, IInputDevice{}
public interface IOutputManager : IManager, IOutputDevice{}

public interface IConnection
{
	public event EventHandler Updated;
	public void Update();
	string ConnectionString { get; }
	public IManager? Manager { get; }
	public void Close() { Manager?.RemoveConnection(this);}
	public void SendEvent(MidiEvent midiEvent);
}


