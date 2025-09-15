using System;
using System.CodeDom.Compiler;
using System.IO.Ports;
using System.Reflection.Metadata;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using MMM_Device;

namespace MMM_Core;

//SYSEX Custom Protocal
public struct SysEx
{
    public const int Start = 0xF0;

    public const int AddrBroadcast = 0;
    public const int AddrController = 0x3FFF; //14 bit

    public const byte ManufacturerID = 0x7D; //Educational MIDI ID

    public const byte DeviceReady = 0x00;
    public const byte ResetDeviceConfig = 0x01;
    public const byte DiscoverDevices = 0x02;
    public const byte Message = 0x03;

    public const byte GetDeviceConstructWithDistributors = 0x10;
    public const byte GetDeviceConstruct = 0x11;
    public const byte GetDeviceName = 0x12;
    public const byte GetDeviceBoolean = 0x13;

    public const byte SetDeviceConstructWithDistributors = 0x20;
    public const byte SetDeviceConstruct = 0x21;
    public const byte SetDeviceName = 0x22;
    public const byte SetDeviceBoolean = 0x23;

    public const byte GetNumOfDistributors = 0x30;
    public const byte AddDistributor = 0x31;
    public const byte GetAllDistributors = 0x32;
    public const byte RemoveDistributor = 0x33;
    public const byte RemoveAllDistributors = 0x34;
    public const byte ToggleMuteDistributor = 0x35;

    public const byte GetDistributorConstruct = 0x40;
    public const byte GetDistributorChannels = 0x41;
    public const byte GetDistributorInstruments = 0x42;
    public const byte GetDistributorMethod = 0x43;
    public const byte GetDistributorBoolValues = 0x44;
    public const byte GetDistributorMinMaxNotes = 0x45;
    public const byte GetDistributorNumPolyphonicNotes = 0x46;

    public const byte SetDistributorConstruct = 0x50;
    public const byte SetDistributorChannels = 0x51;
    public const byte SetDistributorInstruments = 0x52;
    public const byte SetDistributorMethod = 0x53;
    public const byte SetDistributorBoolValues = 0x54;
    public const byte SetDistributorMinMaxNotes = 0x55;
    public const byte SetDistributorNumPolyphonicNotes = 0x56;
}

public class MMM_Msg
{
    private int len;
    public byte[] buffer;

	public MMM_Msg(byte[] msg)
	{
		Initialize(msg);
	}

	public MMM_Msg(MidiEvent msg)
	{
		var m2bConverter = new MidiEventToBytesConverter { BytesFormat = BytesFormat.Device };
		byte[] midiEventBytes = m2bConverter.Convert(msg);
		Initialize(midiEventBytes);
	}

	private void Initialize(byte[] data)
	{
		len = data.Length;
		if (len < 7 || data[0] != SysEx.Start || data[1] != SysEx.ManufacturerID)
			throw new ArgumentOutOfRangeException( "Invalid SysEx message.");
		buffer = data;
	}

	public int Source() { return (buffer[2] << 7) | buffer[3]; }
    public int Destination()
    {
        return (buffer[4] << 7) | buffer[5];
    }
    public byte Type() { return buffer[6]; }
    public byte[] Payload() { return buffer[7..]; }

    public MidiEvent ToMidiEvent()
    {
        var b2mConverter = new BytesToMidiEventConverter();
        b2mConverter.BytesFormat = BytesFormat.Device;
        return b2mConverter.Convert(buffer);
	}

	internal static MMM_Msg GenerateSysEx(int destinationID, byte msgType, byte[] payload)
	{
		byte[] header = new byte[]
		{
		   SysEx.Start,
		   SysEx.ManufacturerID,
		   0x7F, //source Destination  
           0x7F, //source Destination  
           (byte)(destinationID >> 7),
		   (byte)(destinationID & 0x7F),
		   msgType
		};
		byte[] tail = new byte[] { 0xF7 };
		return new MMM_Msg(header.Concat(payload).Concat(tail).ToArray());
	}

}

internal class MMM_Parser : IDisposable
{
	//Singleton
	private static readonly Lazy<MMM_Parser> _instance = new(() => new MMM_Parser());
	public static MMM_Parser Instance => _instance.Value;
    private MMM_Parser() { }

	//Midi Event Handling In/Out
    public event EventHandler<MidiEventSentEventArgs> EventSent = delegate { };
    public bool IsListeningForEvents { get; private set; }

	public void OnEventReceived(object? sender, byte[] bytes)
	{

		// Convert bytes to MidiEvent using DryWetMidi
		var b2mConverter = new BytesToMidiEventConverter();
		b2mConverter.BytesFormat = BytesFormat.Device;
		MidiEvent midiEvent = null;
		try
		{
			midiEvent = b2mConverter.Convert(bytes.ToArray());
            OnEventReceived(sender, midiEvent);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error converting bytes to MidiEvent: {ex.Message}");
			return;
		}
	}

	public void OnEventReceived(object? sender, MidiEvent midiEvent)
    {
		// Check if event is SysEx
		if (midiEvent is not NormalSysExEvent sysExEvent || sysExEvent.Data.Length < 5)
		{
			EventSent.Invoke(sender, new MidiEventSentEventArgs(midiEvent));
			return;
		}

		// Extract the 14-bit address
		ushort rawAddress = BitConverter.ToUInt16(sysExEvent.Data, 3);
		ushort dest = (ushort)(((rawAddress & 0x7F00) >> 1) | (rawAddress & 0x007F));

		// If msg destination is not to Server block outbound msg.
		if (dest != SysEx.AddrController)
		{
			Console.WriteLine($"SysEx Passthrough: {BitConverter.ToString(sysExEvent.Data)}");
			EventSent.Invoke(sender, new MidiEventSentEventArgs(midiEvent));
			return;
		}

        try
        {
            MMM_Msg sysExMsg = new MMM_Msg(midiEvent);
            Console.WriteLine($"SysEx Inbound:  {BitConverter.ToString(sysExEvent.Data)}");
            ProcessMessage(sender, sysExMsg);
        }
        catch
        {
            Console.WriteLine($"Invalid MMM SysEx Msg: {BitConverter.ToString(sysExEvent.Data)}");
		}
	}

	private void ProcessMessage(object? sender, MMM_Msg msg)
	{
        IConnection? connection = sender as IConnection;

		switch (msg.Type())
		{
			case SysEx.DeviceReady:
				HandleDeviceReady(sender, msg);
				break;
			case SysEx.ResetDeviceConfig:
				HandleResetDeviceConfig(sender, msg);
				break;
			case SysEx.DiscoverDevices:
				HandleDiscoverDevices(sender, msg);
				break;
			case SysEx.GetDeviceConstructWithDistributors:
				// Not implemented
				break;
			case SysEx.GetDeviceConstruct:
				HandleGetDeviceConstruct(sender, msg, connection);
				break;
			case SysEx.GetDeviceName:
				HandleGetDeviceName(sender, msg, connection);
				break;
			case SysEx.GetDeviceBoolean:
				// Not implemented
				break;
			case SysEx.SetDeviceConstructWithDistributors:
				// Not implemented
				break;
			case SysEx.SetDeviceConstruct:
				// Not implemented
				break;
			case SysEx.SetDeviceName:
				// Not implemented
				break;
			case SysEx.SetDeviceBoolean:
				// Not implemented
				break;
			case SysEx.RemoveDistributor:
				// Not implemented
				break;
			case SysEx.RemoveAllDistributors:
				// Not implemented
				break;
			case SysEx.GetNumOfDistributors:
				HandleGetNumOfDistributors(sender, msg);
				break;
			case SysEx.GetAllDistributors:
				HandleGetAllDistributors(sender, msg, connection);
				break;
			case SysEx.AddDistributor:
				// Not implemented
				break;
			case SysEx.ToggleMuteDistributor:
				// Not implemented
				break;
			case SysEx.GetDistributorConstruct:
				HandleGetDistributorConstruct(sender, msg, connection);
				break;
			case SysEx.GetDistributorChannels:
				// Not implemented
				break;
			case SysEx.GetDistributorInstruments:
				// Not implemented
				break;
			case SysEx.GetDistributorMethod:
				// Not implemented
				break;
			case SysEx.GetDistributorBoolValues:
				// Not implemented
				break;
			case SysEx.GetDistributorMinMaxNotes:
				// Not implemented
				break;
			case SysEx.GetDistributorNumPolyphonicNotes:
				// Not implemented
				break;
			case SysEx.SetDistributorConstruct:
				// Not implemented
				break;
			case SysEx.SetDistributorChannels:
				// Not implemented
				break;
			case SysEx.SetDistributorInstruments:
				// Not implemented
				break;
			case SysEx.SetDistributorMethod:
				// Not implemented
				break;
			case SysEx.SetDistributorBoolValues:
				// Not implemented
				break;
			case SysEx.SetDistributorMinMaxNotes:
				// Not implemented
				break;
			case SysEx.SetDistributorNumPolyphonicNotes:
				// Not implemented
				break;
			default:
				Console.WriteLine("Debug Unkown SysEx");
				return;
		}
	}

	private void HandleDeviceReady(object? sender, MMM_Msg msg)
	{
		SendMessage(sender, msg.Source(), SysEx.GetDeviceConstruct);
	}

	private void HandleResetDeviceConfig(object? sender, MMM_Msg msg)
	{
		SendMessage(sender, msg.Source(), SysEx.ResetDeviceConfig);
	}

	private void HandleDiscoverDevices(object? sender, MMM_Msg msg)
	{
		SendMessage(sender, SysEx.AddrBroadcast, SysEx.DiscoverDevices);
	}

	private void HandleGetDeviceConstruct(object? sender, MMM_Msg msg, IConnection? connection)
	{
		Device device = new Device(msg.Payload());
		device.ConnectionString = Device.GetConnectionString(sender);
		if (connection == null)
		{
			Console.WriteLine("Error: Connection Source is null");
			return;
		}
		DeviceManager.Instance.AddDevice(connection, device);
		SendMessage(sender, msg.Source(), SysEx.GetNumOfDistributors);
		//SendMessage(msg.Source(), SysEx.GetAllDistributors); //TODO: Client Device returns multiple messages
	}

	private void HandleGetDeviceName(object? sender, MMM_Msg msg, IConnection? connection)
	{
		if (connection == null)
		{
			Console.WriteLine("Error: Connection Source is null");
			return;
		}
		DeviceManager.Instance.Devices[(connection, msg.Source())].Name = BitConverter.ToString(msg.Payload()[0..20]);
	}

	private void HandleGetNumOfDistributors(object? sender, MMM_Msg msg)
	{
		for (UInt16 i = 0; i < msg.Payload()[0]; ++i)
			SendMessage(sender, msg.Source(), SysEx.GetDistributorConstruct, [(byte)(i >> 7), (byte)(i & 0x7F)]);
	}

	private void HandleGetAllDistributors(object? sender, MMM_Msg msg, IConnection? connection)
	{
		if (connection == null)
		{
			Console.WriteLine("Error: Connection Source is null");
			return;
		}
		DeviceManager.Instance.Devices[(connection, msg.Source())].Distributors.Add(new Distributor(msg.Payload()));
	}

	private void HandleGetDistributorConstruct(object? sender, MMM_Msg msg, IConnection? connection)
	{
		if (connection == null)
		{
			Console.WriteLine("Error: Connection Source is null");
			return;
		}
		var distributors = DeviceManager.Instance.Devices[(connection, msg.Source())].Distributors;
		var distributor = new Distributor(msg.Payload());

		// Check if distributors contains a distributor with matching Index
		if (!distributors.Any(d => d.Index == distributor.Index))
			distributors.Add(distributor);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Send SysEx Messages
	////////////////////////////////////////////////////////////////////////////////////////////////////


	public void SendMessage(object? sender, int destinationID, byte msgType)
    {
        SendMessage(sender, destinationID, msgType, []);
    }
    public void SendMessage(object? sender, int destinationID, byte msgType, byte[] payload)
    {

        MMM_Msg msg = MMM_Msg.GenerateSysEx(destinationID, msgType, payload);

        var b2mConverter = new BytesToMidiEventConverter();
        b2mConverter.BytesFormat = BytesFormat.Device;
        try
        {
            MidiEvent midiEvent = b2mConverter.Convert(msg.buffer.ToArray());
            var eventArgs = new MidiEventReceivedEventArgs(midiEvent); // Wrap the MidiEvent in the correct EventArgs type  

			// Fwd message response back to sender
			if (sender is IConnection connection && connection.OutputManager != null) 
            {
				connection.SendEvent(midiEvent);
            }
            else
            {
                EventSent.Invoke(sender, new MidiEventSentEventArgs(midiEvent));
			}
		}
		catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
	
	////////////////////////////////////////////////////////////////////////////////////////////////////
	//DryWetMidi Input/Output Ports
	////////////////////////////////////////////////////////////////////////////////////////////////////

  //  void OnSendEvent(MidiEvent midiEvent)
  //  {
  //      //Convert DryWetMidi SysEx event into a MMM_Msg
  //      var m2bConverter = new MidiEventToBytesConverter();
		//m2bConverter.BytesFormat = BytesFormat.Device;
		//byte[] midiEventBytes = m2bConverter.Convert(midiEvent);

  //      try
  //      {
  //          MMM_Msg sysExMsg = new MMM_Msg(midiEventBytes);
  //          ProcessMessage(null, sysExMsg);
  //      }
  //      catch {
		//	Console.WriteLine(BitConverter.ToString(midiEventBytes).Replace("-", " "));
  //          Console.WriteLine("Invalid MMM SysEx Msg"); 
  //      }
  //  }
	void IDisposable.Dispose() { }
}

