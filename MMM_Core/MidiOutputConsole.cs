using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace MMM_Core;

public class MidiOutputConsole : IOutputDevice
{
	public event EventHandler<MidiEventSentEventArgs>? EventSent;

	public void PrepareForEventsSending()
	{
	}
	public void SendEvent(MidiEvent midiEvent)
	{
		Console.WriteLine(midiEvent);
	}
	public void Dispose()
	{
		EventSent = null;
	}

}


