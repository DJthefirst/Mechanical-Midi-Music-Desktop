using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace MMM_Core;

public class MidiOutputSystem : IOutputDevice
{
	private OutputDevice sysOutputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");

	public event EventHandler<MidiEventSentEventArgs>? EventSent;

	public void PrepareForEventsSending()
	{
	}
	public void SendEvent(MidiEvent midiEvent)
	{
		sysOutputDevice.SendEvent(midiEvent);
	}
	public void Dispose()
	{
		EventSent = null;
	}

}


