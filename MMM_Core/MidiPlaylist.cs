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

	public MidiSong(FileInfo path)
	{
		Name = path.Name;
		Path = path;
	}

	public MidiSong(string name, FileInfo path)
	{
		Name = name;
		Path = path;
	}

}

public class MidiPlaylist : DoubleLinkedList<IMidiSong>, IMidiPlaylist
{
	private DoubleLinkedList<IMidiSong>.ListNode? curSong = null;

	public DoubleLinkedList<IMidiSong> List => this;

	public IMidiSong? GetCurSong()
	{
		return curSong?.Data;
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
		if (curSong == null)
		{
			AddFirst(song);
			curSong = head;
		}
		else
		{
			AddLast(song);
		}
	}

	public IMidiSong? Next()
	{
		curSong = curSong?.Next;
		Console.WriteLine("curSong");
		return curSong?.Data;
	}

	public IMidiSong? Prev()
	{
		curSong = curSong?.Prev;
		return curSong?.Data;
	}
}

