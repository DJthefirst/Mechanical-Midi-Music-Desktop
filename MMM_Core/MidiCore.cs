using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace MMM_Core;
public class MidiCore
{

	private SysExParser parserSysEx = new SysExParser();

	public MidiCore()
	{
		//TODO: Check if inputDevices and outputDevices are null or empty
		InputDevices = new List<IInputDevice>();
		OutputDevices = new List<IOutputDevice>();
		Connect((IInputDevice)parserSysEx);
	}

	public List<IInputDevice> InputDevices { get; }
	public List<IOutputDevice> OutputDevices { get; }
	public DevicesConnectorEventCallback EventCallback { get; set; }

	public void Connect(IInputDevice input)
	{
		InputDevices.Add(input);
		input.EventReceived += OnEventReceived;
	}
	public void Connect(IOutputDevice output)
	{
		OutputDevices.Add(output);
	}

	public void Disconnect(IInputDevice input)
	{
		InputDevices.Remove(input);
		input.EventReceived -= OnEventReceived;
	}

	public void Disconnect(IOutputDevice output)
	{
		OutputDevices.Remove(output);
	}

	private void OnEventReceived(object? sender, MidiEventReceivedEventArgs e)
	{
		var inputMidiEvent = e.Event;
		var eventCallback = EventCallback;

		var midiEvent = eventCallback == null ? inputMidiEvent : eventCallback(inputMidiEvent);
		if (midiEvent == null) return;

		// Give SysEx Parser first chance in case of Server SysEx event
		if (parserSysEx.OnEventReceived(sender, midiEvent)) return;

		// Forward event to all output devices if not handled by SysEx Parser
		foreach (var outputDevice in OutputDevices)
		{
			outputDevice.SendEvent(e.Event);
		}
	}
}
