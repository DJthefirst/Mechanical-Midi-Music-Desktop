using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.IO.Ports;

namespace MMM_Core.MidiManagers;

public class MidiPortInManager : IInputManager
{
	private List<InputDevice> midiPorts = new List<InputDevice>();

	public event EventHandler<MidiEventReceivedEventArgs> EventReceived = delegate { };

	public bool IsListeningForEvents { get; private set; }

	public MidiPortInManager()
	{
		IsListeningForEvents = true;
	}

	public bool AddConnection(string portName)
	{
		InputDevice inputDevice;

		try
		{
			inputDevice = InputDevice.GetByName(portName);
		}
		catch
		{
			return false;
		}

		inputDevice.EventReceived += (sender, e) => EventReceived?.Invoke(this, e);

		inputDevice.StartEventsListening();
		midiPorts.Add(inputDevice);
		return true;
	}

	public bool RemoveConnection(string portName)
	{
		InputDevice inputDevice = InputDevice.GetByName(portName);
		return RemoveConnection(inputDevice);
	}

	public bool RemoveConnection(InputDevice portName)
	{
		if (midiPorts.Contains(portName))
		{
			midiPorts.Remove(portName);
			return true;
		}
		return false;
	}

	public void ClearConnections()
	{
		foreach (var inputDevice in midiPorts)
		{
			RemoveConnection(inputDevice);
		}
	}

	void IInputDevice.StartEventsListening()
	{
		IsListeningForEvents = true;
	}

	void IInputDevice.StopEventsListening()
	{
		IsListeningForEvents = false;
	}

	void IDisposable.Dispose() { }

	public List<string> AvailableConnections()
	{
		return InputDevice.GetAll().Select(inputDevice => inputDevice.Name).ToList();
	}

	public List<string> ListConnections()
	{
		return midiPorts.ConvertAll(inputDevice => inputDevice.Name);
	}
}