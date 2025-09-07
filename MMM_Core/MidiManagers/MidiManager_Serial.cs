using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.IO.Ports;

namespace MMM_Core.MidiManagers;

public class MidiSerialManager : IInputManager, IOutputManager
{
	private List<string> serialPortsAvailable = new List<string>();
	private List<SerialPort> serialPorts = new List<SerialPort>();
	private Dictionary<SerialPort, Queue<byte>> buffers = new Dictionary<SerialPort, Queue<byte>>();

	public event EventHandler<MidiEventReceivedEventArgs> EventReceived = delegate { };
	public event EventHandler<MidiEventSentEventArgs> EventSent = delegate { };
	public event EventHandler<List<string>> OnPortsUpdated = delegate { };

	public bool IsListeningForEvents { get; private set; }

	public MidiSerialManager()
	{
		IsListeningForEvents = true;
		_ = MonitorPort(); // Fire-and-forget, suppress CS4014 warning
	}

	//TODO: Add Native Event Listening for Serial Port Disconnects
	async Task MonitorPort()
	{
		while (IsListeningForEvents)
		{
			try
			{
				var availableConnections = SerialPort.GetPortNames().ToList();
				foreach (var connection in serialPorts.ToList())
				{
					if (!availableConnections.Contains(connection.PortName))
					{
						Console.WriteLine($"Serial Port {connection} disconnected.");
						RemoveConnection(connection);
					}
				}
				if (!new HashSet<string>(serialPortsAvailable).SetEquals(availableConnections) ||
					(serialPortsAvailable.Count != availableConnections.Count))
				{
					Console.WriteLine("Serial Ports Updated");
					serialPortsAvailable = availableConnections;
					OnPortsUpdated.Invoke(this, availableConnections);
				}
			}
			catch(Exception e) { Console.WriteLine("Serial Listenr Error: " + e);}
			await Task.Delay(2000); // Poll every 2 seconds
		}
	}

	public async Task AddConnection(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
	{
		SerialPort serialPort;
		serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
		serialPort.DataReceived += SerialPortDataReceived;


		try
		{
			serialPort.Open();
			serialPort.Close();
			serialPort.Open();
			serialPorts.Add(serialPort);
			buffers.Add(serialPort, new Queue<byte>());

			await Task.Delay(100);
			byte[] readyMsg = SysExParser.GenerateSysEx(0x0000, SysEx.DeviceReady, []);
			serialPort.Write(readyMsg, 0, readyMsg.Count());
		}
		catch (UnauthorizedAccessException ex)
		{
			Console.WriteLine($"Error: Serial Port {portName} is already in use. Details: {ex.Message}");
			return;
		}
		catch (IOException ex)
		{
			Console.WriteLine($"Error: Unable to open Serial Port {portName}. Details: {ex.Message}");
			return;
		}
	}

	public bool RemoveConnection(string portName)
	{
		var serialPort = serialPorts.FirstOrDefault(sp => sp.PortName == portName);
		if (serialPort != null)
		{
			return RemoveConnection(serialPort);
		}
		return true;
	}

	public bool RemoveConnection(SerialPort serialPort)
	{
		serialPorts.Remove(serialPort);
		buffers.Remove(serialPort);
		DeviceManager.Instance.CloseConnection(serialPort);
		if (serialPort != null && serialPort.IsOpen)
		{
			try
			{
				byte[] MidiReset = { 0xFF };
				serialPort.Write(MidiReset, 0, 1); // reset
				serialPort.DataReceived -= SerialPortDataReceived;
				serialPort.Close();
				return true;
			}
			catch(Exception e){
				Console.WriteLine("PortErr: " + e);
			}
		}
		return false;
	}

	public void ClearConnections()
	{
		foreach (var serialPort in serialPorts)
		{
			RemoveConnection(serialPort);
		}
	}

	private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		if (!IsListeningForEvents) return;

		//Read in Serial Message
		SerialPort sp = (SerialPort)sender;
		byte[] dataBytes = new byte[sp.BytesToRead];
		sp.Read(dataBytes, 0, dataBytes.Length);

		//Push Bytes to Port specific Queue
		foreach (var dataByte in dataBytes)
		{
			buffers[sp].Enqueue(dataByte);
		}
		IEnumerable<MidiEvent> midiEvents;

		bool hasStart = false;
		bool hasEnd = false;

		// Iterate through the buffer to check for F0 (SysEx Start) and F7 (SysEx End) // TODO: Improve Robustness
		foreach (var b in buffers[sp].ToArray())
		{
			if (b == 0xF0) hasStart = true;
			if (b == 0xF7 && hasStart)
			{
				hasEnd = true;
				break;
			}
		}

		// Return true only if both start and end are found  
		if (!(hasStart == hasEnd)) return;

		//Console.WriteLine($"Bytes: {BitConverter.ToString(buffers[sp].ToArray())}");

		//Convert Bytes into a MIDI Message
		var b2mConverter = new BytesToMidiEventConverter();
		b2mConverter.BytesFormat = BytesFormat.Device;
		try
		{
			midiEvents = b2mConverter.ConvertMultiple(buffers[sp].ToArray());
			buffers[sp].Clear();
		}
		catch (UnexpectedRunningStatusException ex)
		{
			Console.WriteLine($"Error: Serial Byte Parser. Details: {ex.Message}");
			//Console.WriteLine($"Actual bytes: {string.Join(", ", buffers[sp].ToArray())}");
			buffers[sp].Clear();
			return;
		}
		catch (NotEnoughBytesException ex)
		{
			Console.WriteLine($"Error: Not enough bytes to parse MIDI event. Details: {ex.Message}");
			Console.WriteLine($"Expected byte count: {ex.ExpectedCount}, Actual byte count: {ex.ActualCount}");
			Console.WriteLine($"Actual bytes: {string.Join(", ", buffers[sp].ToArray())}");
			return;
		}

		//Foward Midi Event to Event Received
		foreach (var midiEvent in midiEvents)
		{
			var midiEventArgs = new MidiEventReceivedEventArgs(midiEvent);
			EventReceived?.Invoke(sender, midiEventArgs);
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

	void IOutputDevice.PrepareForEventsSending() { }

	void IOutputDevice.SendEvent(MidiEvent midiEvent)
	{
		var m2bConverter = new MidiEventToBytesConverter();
		m2bConverter.BytesFormat = BytesFormat.Device;
		byte[] midiEventBytes = m2bConverter.Convert(midiEvent);
		foreach (var serialPort in serialPorts)
		{
			if (serialPort.IsOpen)
			{
				serialPort.Write(midiEventBytes, 0, midiEventBytes.Length);
			}
			else Console.WriteLine("Serial Not Available");
		}
	}

	void IDisposable.Dispose() { }

	public List<string> AvailableConnections()
	{
		return SerialPort.GetPortNames().ToList();
	}

	public List<string> ListConnections()
	{
		return serialPorts.Select(i => i.PortName).ToList();
	}
}