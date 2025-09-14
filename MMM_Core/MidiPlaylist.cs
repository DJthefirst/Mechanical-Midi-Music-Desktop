using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MMM_Core;

public class MidiSong : IMidiSong
{
	public string Name { get; private set; }
	public FileInfo Path { get; private set; }
	public double Duration { get; set; }

	public string DurationTimestamp => TimeSpan.FromMilliseconds(Duration).ToString(@"mm\:ss");
	public int Index { get; set; }

	public MidiSong(FileInfo path)
	{
		Name = path.Name;
		Path = path;
		Duration = (double)MidiFile.Read(path.FullName).GetDuration<MetricTimeSpan>().TotalMilliseconds;
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
	
	public void SetCurSong(string name)
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
		try
		{
			AddSong(new MidiSong(file));
		}
		catch
		{
			Console.WriteLine($"Invalid File: {file}");
		}
	}

	public void AddDirectory(DirectoryInfo path)
	{
		bool setIndex = false;
		if (Songs.Count == 0)
			setIndex = true;

		foreach (var midiFile in path.GetFiles("*.mid"))
		{
			var song = new MidiSong(midiFile);
			if (Songs.Any(s => s.Path.FullName.Equals(song.Path.FullName, StringComparison.OrdinalIgnoreCase)))
				continue;
			AddSong(song);
		}

		if (setIndex && Songs.Count > 0)
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
	}

	public void AddSong(IMidiSong song)
	{
		if (Songs.Any(s => s.Path.FullName.Equals(song.Path.FullName, StringComparison.OrdinalIgnoreCase))) return;
		Songs?.Add(song);
		if (Songs.Count == 1)
		{
			curSongIndex = 0;
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
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
		if (Songs.Count == 0) return null;
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
		if (Songs.Count == 0) return null;
		int newIndex = (Songs.Count + curSongIndex - 1) % Songs.Count;
		if (newIndex != curSongIndex)
		{
			curSongIndex = newIndex;
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
		return Songs[curSongIndex];
	}

	public void RemoveSong(IMidiSong song)
	{
		Songs?.Remove(song);
		if (Songs.Count > 0)
		{
			curSongIndex = Math.Clamp(curSongIndex, 0, Songs.Count - 1);
			OnSongChanged?.Invoke(this, Songs[curSongIndex]);
		}
	}
}

