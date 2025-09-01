using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MMM_Core;

public class MidiSong : IMidiSong
{
	public string Name { get; private set; }
	public FileInfo Path { get; private set; }

	public int Index { get; set; }

	public MidiSong(FileInfo path)
	{
		Name = path.Name;
		Path = path;
		Index = -1;
	}

	public MidiSong(string name, FileInfo path)
	{
		Name = name;
		Path = path;
		Index = -1;
	}
}

public class MidiPlaylist : IMidiPlaylist
{
	public int curSongIndex = 0;
	public List<IMidiSong> Songs { get; private set; } = new List<IMidiSong>();

	public event EventHandler<IMidiSong>? OnSongChanged;

	public IMidiSong? GetCurSong()
	{
		if (Songs is null || Songs.Count == 0)
			return null;
		return  Songs[curSongIndex];
	}

	public IMidiSong? GetSongByName(string name)
	{
		if (Songs is null) return null;
		return Songs.Find(song => song.Name == name);
	}
	
	public void SertCurSong(string name)
	{
		if (Songs is null) return;
		var index = Songs.FindIndex(song => song.Name == name);
		if (index != -1 && curSongIndex != index)
		{
			curSongIndex = index;
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
	}

	public void AddSong(FileInfo file)
	{
		AddSong(new MidiSong(file));
	}

	public void AddDirectory(DirectoryInfo path)
	{
		foreach (var midiFile in path.GetFiles("*.mid"))
		{
			AddSong(new MidiSong(midiFile));
		}
	}

	public void AddSong(IMidiSong song)
	{
		Songs?.Add(song);

	}

	public IMidiSong? Peek()
	{
		if (Songs is not null && curSongIndex + 1 < Songs.Count)
			return Songs[curSongIndex + 1];
		return null;
	}

	public IMidiSong? Next()
	{
		if (Songs is null) return null;
		int newIndex = (curSongIndex + 1) % Songs.Count;
		if (newIndex != curSongIndex)
		{
			curSongIndex = newIndex;
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
		return Songs[curSongIndex];
	}

	public IMidiSong? Prev()
	{
		if (Songs is null) return null;
		int newIndex = (Songs.Count + curSongIndex - 1) % Songs.Count;
		if (newIndex != curSongIndex)
		{
			curSongIndex = newIndex;
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
		return Songs[curSongIndex];
	}
}

