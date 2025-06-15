using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace MMM_Core;
public class MidiCore
{

	private ParseSysEx parserSysEx = new ParseSysEx();

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
		if (midiEvent == null)
			return;

		if (midiEvent is NormalSysExEvent sysExEvent)
		{
			// Extract the 14-bit address by shifting only the MSB to the right by 1 bit  
			UInt16 rawAddress = BitConverter.ToUInt16(sysExEvent.Data, 3);
			UInt16 dest = (UInt16)(((rawAddress & 0x7F00) >> 1) | (rawAddress & 0x007F));

			// If msg destination is to Server process msg and block outbound msg. 
			if (sysExEvent.Data.Length >= 5 && dest == SysEx.AddrController) // Adjusted for 14-bit address  
			{
				Console.WriteLine($"SysEx Inbound:  {BitConverter.ToString(sysExEvent.Data)}");
				((IOutputDevice)parserSysEx).SendEvent(midiEvent);
				return;  
			}
			Console.WriteLine($"SysEx Outbound: {BitConverter.ToString(sysExEvent.Data)}");
		}

		foreach (var outputDevice in OutputDevices)
		{
			outputDevice.SendEvent(e.Event);
		}
	}
}
