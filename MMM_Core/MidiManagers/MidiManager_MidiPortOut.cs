using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.IO.Ports;

namespace MMM_Core.MidiManagers;

public class MidiPortOutManager : IOutputManager
{
	private readonly List<OutputDevice> midiPorts = new();

	public event EventHandler<MidiEventSentEventArgs> EventSent = delegate { };

	public MidiPortOutManager() { }

	public bool AddConnection(string portName)
	{
		var outputDevice = OutputDevice.GetByName(portName);
		if (outputDevice != null)
		{
			Console.WriteLine($"MIDI Output Port {portName} opened.");
			midiPorts.Add(outputDevice);
			return true;
		}
		Console.WriteLine($"Error: Unable to open MIDI Output Port {portName}.");
		return false;
	}

	public bool RemoveConnection(OutputDevice outputDevice)
	{
		if (outputDevice != null && midiPorts.Contains(outputDevice))
		{
			outputDevice.SendEvent(new ResetEvent());
			midiPorts.Remove(outputDevice);
			outputDevice.Dispose(); // Optionally dispose to release the device
			return true;
		}
		return false;
	}

	public void RemoveConnection(IConnection connection)
	{
		//TODO: Implement
	}
	public bool RemoveConnection(string portName)
	{
		// Find the OutputDevice instance in midiPorts by name
		var outputDevice = midiPorts.FirstOrDefault(d => d.Name == portName);
		return outputDevice != null && RemoveConnection(outputDevice);
	}

	public void ClearConnections()
	{
		foreach(var outputDevice in midiPorts)
		{
			RemoveConnection(outputDevice); 
		}
	}

	void IOutputDevice.PrepareForEventsSending() { }

	void IOutputDevice.SendEvent(MidiEvent midiEvent)
	{
		foreach (var outputDevice in midiPorts)
		{
			outputDevice.SendEvent(midiEvent);
		}
	}

	void IDisposable.Dispose()
	{
		foreach (var outputDevice in midiPorts)
		{
			outputDevice.Dispose();
		}
		midiPorts.Clear();
	}

	public List<string> AvailableConnections()
	{
		return OutputDevice.GetAll().Select(outputDevice => outputDevice.Name).ToList();
	}

	public List<string> ListConnections()
	{
		return midiPorts.ConvertAll(outputDevice => outputDevice.Name);
	}
}
