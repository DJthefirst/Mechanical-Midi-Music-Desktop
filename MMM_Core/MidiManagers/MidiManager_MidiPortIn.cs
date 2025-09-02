using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.IO.Ports;
using System.Linq;

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

	public bool RemoveConnection(InputDevice inputDevice)
	{
		if (inputDevice != null && midiPorts.Contains(inputDevice))
		{
			midiPorts.Remove(inputDevice);
			inputDevice.Dispose(); // Optionally dispose to release the device
			return true;
		}
		return false;
	}

	public bool RemoveConnection(string portName)
	{
		// Find the OutputDevice instance in midiPorts by name
		var inputDevice = midiPorts.FirstOrDefault(d => d.Name == portName);
		return inputDevice != null && RemoveConnection(inputDevice);
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