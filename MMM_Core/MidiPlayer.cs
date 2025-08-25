using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Reflection.Metadata;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using System.Numerics;

namespace MMM_Core;

public enum PLAYER_STATUS
{
	STOP = 0,
	START = 1,
	PLAY = 2,
	PAUSE = 3
}

public class MidiPlayer : IMidiPlayer, IInputDevice
{
	private Playback? playback;
	private IMidiPlaylist playlist;
	private uint repeat = 0;
	private bool autoPlay = true;
	private bool loop = true;

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

	public PLAYER_STATUS GetStatus()
	{
		if (IsPlaying())
			return PLAYER_STATUS.PLAY;
		else if (playback?.GetCurrentTime<MetricTimeSpan>().TotalSeconds > 0)
			return PLAYER_STATUS.PAUSE;
		else
			return PLAYER_STATUS.STOP;
	}

	public void Repeat(uint repeat)
	{
		this.repeat = repeat;
	}

	public void AutoPlay(bool autoPlay)
	{
		this.autoPlay = autoPlay;
	}

	public void Loop(bool loop)
	{
		this.loop = loop;
	}

	public void Play()
	{
		if (GetStatus() == PLAYER_STATUS.PAUSE)
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

	public double GetMaxPositionMs()
	{
		if (playback == null) return 0;
		return (double)playback.GetDuration<MetricTimeSpan>().TotalMilliseconds;
	}
	public double GetPositionMs()
	{
		if (playback == null) return 0;
		return (double)playback.GetCurrentTime<MetricTimeSpan>().TotalMilliseconds;
	}
	public double SetPositionMs(double pos)
	{
		if (playback == null) return 0;
		playback.MoveToTime(new MetricTimeSpan((long)pos));
		return (double)playback.GetCurrentTime<MetricTimeSpan>().TotalMilliseconds;
	}

	public double NavigateMs(double increment)
	{
		double curPos = GetPositionMs();
		if (playback is null) return curPos;

		double maxPos = playback.GetDuration<MetricTimeSpan>().TotalMilliseconds;
		double newPos = curPos + increment;
		newPos = Math.Clamp(newPos, 0, maxPos);
		playback.MoveToTime(new MetricTimeSpan((long)newPos));

		//TODO: Optimize to avoid calling GetPositionMs again
		return GetPositionMs();
	}

	private void OnPlayerFinsihed()
	{
		if (!autoPlay) return;
		if (playback == null) return;
		if (repeat > 0)
		{
			playback.MoveToStart();
			playback.Start();
			repeat--;
		}
		else
		{
			// Move to next song if not at the end, or loop if autoPlay == 2

			bool atEnd = ReferenceEquals(playlist.GetCurSong(), playlist.Songs.Last());

			if (!loop && atEnd)
				return; // Stop at end if not looping

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
	public byte[] ToByteArray()
	{
		using var ms = new MemoryStream();
		using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
		{
			// Write basic fields except playlist and current song
			writer.Write(autoPlay);
			writer.Write(loop);
			writer.Write((uint)GetStatus());
			writer.Write(repeat);
		}
		return 
			ms.ToArray();
	}
	public void FromByteArray(byte[] data)
	{
		using var ms = new MemoryStream(data);
		using (var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, true))
		{
			// Read fields in the same order as written in ToByteArray
			autoPlay = reader.ReadBoolean();
			loop = reader.ReadBoolean();
			var status = (PLAYER_STATUS)reader.ReadUInt32();
			repeat = reader.ReadUInt32();

			switch (status)
			{
				case PLAYER_STATUS.PLAY:
					Play();
					break;
				case PLAYER_STATUS.PAUSE:
					Pause();
					break;
				case PLAYER_STATUS.STOP:
				default:
					Stop();
					break;
			}
		}
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
