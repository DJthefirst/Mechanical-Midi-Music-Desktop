using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.IO.Ports;
using MMM_Device;

namespace MMM_Core.MidiManagers;

public class SerialConnection : SerialPort, IConnection
{
	public SerialConnection(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
	: base(portName, baudRate, parity, dataBits, stopBits) { }
	public string ConnectionString => PortName;
	public IInputManager InputManager => MidiSerialManager.Instance;
	public IOutputManager OutputManager => MidiSerialManager.Instance;
	public void SendEvent(MidiEvent midiEvent)
	{
		var m2bConverter = new MidiEventToBytesConverter();
		m2bConverter.BytesFormat = BytesFormat.Device;
		byte[] midiEventBytes = m2bConverter.Convert(midiEvent);

		if (this.IsOpen)
		{
			this.Write(midiEventBytes, 0, midiEventBytes.Length);
		}
		else Console.WriteLine("Serial Not Available");
	}
}

public class MidiSerialManager : IInputManager, IOutputManager
{
	private static readonly Lazy<MidiSerialManager> _instance = new(() => new MidiSerialManager());
	public static MidiSerialManager Instance => _instance.Value;

	private List<string> serialPortsAvailable = new List<string>();
	private List<SerialConnection> serialPorts = new List<SerialConnection>();
	private Dictionary<SerialConnection, Queue<byte>> buffers = new Dictionary<SerialConnection, Queue<byte>>();

	public event EventHandler<MidiEventReceivedEventArgs> EventReceived = delegate { };
	public event EventHandler<MidiEventSentEventArgs> EventSent = delegate { };
	public event EventHandler<List<string>> OnPortsUpdated = delegate { };

	public bool IsListeningForEvents { get; private set; }

	private MidiSerialManager()
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
				var availableConnections = SerialConnection.GetPortNames().ToList();
				foreach (var connection in serialPorts.ToList())
				{
					if (!availableConnections.Contains(connection.PortName))
					{
						Console.WriteLine($"Serial Port {connection} disconnected.");
						RemoveConnection(connection);
						OnPortsUpdated.Invoke(this, availableConnections);
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
			catch (Exception e) { Console.WriteLine("Serial Listenr Error: " + e); }
			await Task.Delay(2000); // Poll every 2 seconds
		}
	}

	public async Task AddConnection(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
	{
		SerialConnection serialPort;
		serialPort = new SerialConnection(portName, baudRate, parity, dataBits, stopBits);
		serialPort.DataReceived += SerialPortDataReceived;

		try
		{
			serialPort.Open();
			serialPort.Close();
			serialPort.Open();
			serialPorts.Add(serialPort);
			buffers.Add(serialPort, new Queue<byte>());

			await Task.Delay(100);
			MMM_Msg readyMsg = MMM_Msg.GenerateSysEx(0x0000, SysEx.DeviceReady, []);
			serialPort.Write(readyMsg.buffer, 0, readyMsg.buffer.Length);
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
			RemoveConnection(serialPort);
		}
		return true;
	}

	public bool RemoveConnection(SerialConnection serialPort)
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
				OnPortsUpdated.Invoke(this, AvailableConnections());
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("PortErr: " + e);
			}
		}
		return false;
	}

	public void FreeConnection(string str)
	{
		foreach (var serialPort in serialPorts.ToList())
		{
			if (Device.GetConnectionString(serialPort) == str)
			{
				RemoveConnection(serialPort);
				break;
			}
		}
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

		var sp = (SerialConnection)sender;
		var buffer = buffers[sp];

		// Read all available bytes and enqueue them
		var dataBytes = new byte[sp.BytesToRead];
		sp.Read(dataBytes, 0, dataBytes.Length);
		foreach (var b in dataBytes) buffer.Enqueue(b);

		var completeMessages = new List<byte[]>();

		// Extract complete SysEx messages (F0 ... F7)
		while (buffer.Count > 0)
		{
			// Discard bytes until start byte (F0) is found
			if (buffer.Peek() != 0xF0)
			{
				buffer.Dequeue();
				continue;
			}

			int endIndex = -1, idx = 0;
			foreach (var b in buffer)
			{
				if (idx > 0 && b == 0xF0)
				{
					// Found another F0 before F7, discard up to this F0
					for (int i = 0; i < idx; i++) buffer.Dequeue();
					idx = 0;
					continue;
				}
				if (b == 0xF7)
				{
					endIndex = idx;
					break;
				}
				idx++;
			}

			if (endIndex == -1) break; // No end found, wait for more data

			// Extract message
			var message = new byte[endIndex + 1];
			for (int i = 0; i <= endIndex; i++) message[i] = buffer.Dequeue();
			completeMessages.Add(message);
		}

		// Convert and send only complete messages
		foreach (var msg in completeMessages)
		{
			var b2mConverter = new BytesToMidiEventConverter { BytesFormat = BytesFormat.Device };
			try
			{
				foreach (var midiEvent in b2mConverter.ConvertMultiple(msg))
				{
					EventReceived?.Invoke(sender, new MidiEventReceivedEventArgs(midiEvent));
				}
			}
			catch (UnexpectedRunningStatusException ex)
			{
				Console.WriteLine($"Error: Serial Byte Parser. Details: {ex.Message}");
			}
			catch (NotEnoughBytesException ex)
			{
				Console.WriteLine($"Error: Not enough bytes to parse MIDI event. Details: {ex.Message}");
				Console.WriteLine($"Expected byte count: {ex.ExpectedCount}, Actual byte count: {ex.ActualCount}");
			}
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
		foreach (var serialPort in serialPorts)
		{
			serialPort.SendEvent(midiEvent);
		}
	}

	void IDisposable.Dispose() { }

	public List<string> AvailableConnections()
	{
		return SerialConnection.GetPortNames().ToList();
	}

	public List<string> ListConnections()
	{
		return serialPorts.Select(i => i.PortName).ToList();
	}
}