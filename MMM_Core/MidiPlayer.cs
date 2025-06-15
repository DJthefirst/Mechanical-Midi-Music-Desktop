using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Reflection.Metadata;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using System.Numerics;

namespace MMM_Core;



public class MidiPlayer : IMidiPlayer, IInputDevice
{
	private Playback? playback;
	private IMidiPlaylist playlist;
	private bool repeat = false;

	public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

	public IMidiPlaylist Playlist => playlist;

	public MidiPlayer(IMidiPlaylist playlist)
	{
		this.playlist = playlist;
	}

	public bool IsPlaying()
	{
		return playback?.IsRunning ?? false;
	}

	public void Repeat(bool repeat)
	{
		this.repeat = repeat;
	}

	public void Play()
	{
		if (playback?.GetCurrentTime<MetricTimeSpan>().TotalSeconds > 0)
		{
			playback?.Start();
			return;
		}

		var currentSong = playlist.GetCurSong();
		if (currentSong == null) return;
		var midiFile = MidiFile.Read(currentSong.Path.FullName);
		playback = midiFile.GetPlayback();
		playback.EventPlayed += (sender, e) =>
		{
			EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(e.Event));
		};
		Console.WriteLine("Should be playing now");
		playback.Finished += delegate (object? sender, EventArgs e)
		{
			OnPlayerFinsihed();
		};
		playback.Start();
	}
	public void Pause()
	{
		playback?.Stop();
	}
	public void Stop()
	{
		playback?.Stop();
		playback?.MoveToStart();
	}
	public void Reset()
	{
		playback?.Stop();
		playback?.MoveToStart();
		playback?.Dispose();
		playback = null;
	}

	public IMidiSong? Next()
	{
		bool wasPlaying = IsPlaying();
		var song = playlist.Next();
		Reset();

		if (wasPlaying) Play();

		return song;
	}

	public IMidiSong? Prev()
	{
		bool wasPlaying = IsPlaying();
		var song = playlist.Prev();
		Reset();

		if (wasPlaying) Play();

		return song;
	}

	private void OnPlayerFinsihed()
	{
		if (playback == null) return;
		if (repeat)
		{
			playback.MoveToStart();
			playback.Start();
		}
		else
		{
			//TODO see if this actually works
			playlist.Next();
			playback.MoveToStart();
			Play();
		}
	}

	public bool IsListeningForEvents { get; private set; }
	public void StartEventsListening()
	{
		IsListeningForEvents = true;
	}

	public void StopEventsListening()
	{

		IsListeningForEvents = false;
	}
	public void Dispose()
	{
	}

	//private void OnEventPlayed(object sender, MidiEventPlayedEventArgs e)
	//{
	//	var midiDevice = (MidiDevice)sender;
	//	Console.WriteLine($"Event played on '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
	//}
	//private void OnEventSent(object sender, MidiEventSentEventArgs e)
	//{
	//	var midiDevice = (MidiDevice)sender;
	//	Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
	//}
}
