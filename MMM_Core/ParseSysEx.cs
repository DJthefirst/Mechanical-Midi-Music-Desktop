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

public class SysExMsg
{
    private int len;
    public byte[] buffer;
    public SysExMsg(byte[] buffer)
    {
        this.len = buffer.Count();
        bool validSysEx = ((len >= 7) && (buffer[0] == 0xF0) && (buffer[1] == SysEx.ManufacturerID));
        if (!validSysEx) { throw new ArgumentOutOfRangeException(); }
        this.buffer = buffer;
    }
    public int Source() { return (buffer[2] << 7) | buffer[3]; }
    public int Destination()
    {
        return (buffer[4] << 7) | buffer[5];
    }
    public byte Type() { return buffer[6]; }
    public byte[] Payload() { return buffer[7..]; }

}

internal class SysExParser : IInputDevice, IOutputDevice, IDisposable
{

    public event EventHandler<MidiEventReceivedEventArgs> EventReceived = delegate { };
    public event EventHandler<MidiEventSentEventArgs> EventSent = delegate { };
    public bool IsListeningForEvents { get; private set; }

    public bool OnEventReceived(object? sender, MidiEvent midiEvent)
    {
		// Check if event is SysEx
		if (!(midiEvent is NormalSysExEvent sysExEvent)) return false;
		
        // Extract the 14-bit address by shifting only the MSB to the right by 1 bit  
        UInt16 rawAddress = BitConverter.ToUInt16(sysExEvent.Data, 3);
        UInt16 dest = (UInt16)(((rawAddress & 0x7F00) >> 1) | (rawAddress & 0x007F));

        // If msg destination is to Server process msg and block outbound msg. 
        if (!(sysExEvent.Data.Length >= 5 && dest == SysEx.AddrController)) {
			Console.WriteLine($"SysEx Outbound: {BitConverter.ToString(sysExEvent.Data)}");
			return false;// Adjusted for 14-bit address
        }
            
        var m2bConverter = new MidiEventToBytesConverter();
        m2bConverter.BytesFormat = BytesFormat.Device;
        byte[] midiEventBytes = m2bConverter.Convert(midiEvent);

        //try{
            SysExMsg sysExMsg = new SysExMsg(midiEventBytes);
            Console.WriteLine($"SysEx Inbound:  {BitConverter.ToString(sysExEvent.Data)}");
            ProcessMessage(sender, sysExMsg);
		//}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error sending message: {ex.Message}");
        //}

        return true; //Block outbound msg
	}

	private void ProcessMessage(object? sender, SysExMsg msg)
	{
		switch (msg.Type())
		{
			case SysEx.DeviceReady:
				SendMessage(msg.Source(), SysEx.GetDeviceConstruct);
				break;
			case SysEx.ResetDeviceConfig:
				SendMessage(msg.Source(), SysEx.ResetDeviceConfig);
				break;
			case SysEx.DiscoverDevices:
				SendMessage(SysEx.AddrBroadcast, SysEx.DiscoverDevices);
				break;
			case SysEx.GetDeviceConstructWithDistributors:
				break;
			case SysEx.GetDeviceConstruct:
                Device device = new Device(msg.Payload());
                device.ConnectionString = GetConnectionString(sender);
				DeviceManager.Instance.AddDevice(device);
				SendMessage(msg.Source(), SysEx.GetNumOfDistributors);
				//SendMessage(msg.Source(), SysEx.GetAllDistributors); //TODO: Client Device returns multiple messages
				break;
            case SysEx.GetDeviceName:
				DeviceManager.Instance.Devices[msg.Source()].Name = BitConverter.ToString(msg.Payload()[0..20]);
                break;
            case SysEx.GetDeviceBoolean:
                break;
            case SysEx.SetDeviceConstructWithDistributors:
                break;
            case SysEx.SetDeviceConstruct:
                break;
            case SysEx.SetDeviceName:
                break;
            case SysEx.SetDeviceBoolean:
                break;
            case SysEx.RemoveDistributor:
                break;
            case SysEx.RemoveAllDistributors:
                break;
            case SysEx.GetNumOfDistributors:
                for (byte i = 0; i < msg.Payload()[0]; ++i) SendMessage(msg.Source(), SysEx.GetDistributorConstruct, [i]);
                break;
            case SysEx.GetAllDistributors:
				DeviceManager.Instance.Devices[msg.Source()].Distributors.Add(new Distributor(msg.Payload()));
				break;
            case SysEx.AddDistributor:
                break;
            case SysEx.ToggleMuteDistributor:
                break;
            case SysEx.GetDistributorConstruct:
				DeviceManager.Instance.Devices[msg.Source()].Distributors.Add(new Distributor(msg.Payload()));
                break;
            case SysEx.GetDistributorChannels:
                break;
            case SysEx.GetDistributorInstruments:
                break;
            case SysEx.GetDistributorMethod:
                break;
            case SysEx.GetDistributorBoolValues:
                break;
            case SysEx.GetDistributorMinMaxNotes:
                break;
            case SysEx.GetDistributorNumPolyphonicNotes:
                break;
            case SysEx.SetDistributorConstruct:
                break;
            case SysEx.SetDistributorChannels:
                break;
            case SysEx.SetDistributorInstruments:
                break;
            case SysEx.SetDistributorMethod:
                break;
            case SysEx.SetDistributorBoolValues:
                break;
            case SysEx.SetDistributorMinMaxNotes:
                break;
            case SysEx.SetDistributorNumPolyphonicNotes:
                break;


            default:
                Console.WriteLine("Debug Unkown SysEx");
                return;
        }
    }

	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Send Messages
	////////////////////////////////////////////////////////////////////////////////////////////////////



	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Send SysEx Messages
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal static byte[] GenerateSysEx(int destinationID, byte msgType, List<byte> payload)
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
		return header.Concat(payload).Concat(tail).ToArray();
	}

	public void SendMessage(int destinationID, byte msgType)
    {
        SendMessage(destinationID, msgType, []);
    }
    public void SendMessage(int destinationID, byte msgType, List<byte> payload)
    {

        byte[] sysExMsg = GenerateSysEx(destinationID, msgType, payload);

        var b2mConverter = new BytesToMidiEventConverter();
        b2mConverter.BytesFormat = BytesFormat.Device;
        try
        {
            MidiEvent midiEvent = b2mConverter.Convert(sysExMsg.ToArray());
            var eventArgs = new MidiEventReceivedEventArgs(midiEvent); // Wrap the MidiEvent in the correct EventArgs type  
            EventReceived?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
	
	////////////////////////////////////////////////////////////////////////////////////////////////////
	//DryWetMidi Input/Output Ports
	////////////////////////////////////////////////////////////////////////////////////////////////////

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
        //Convert DryWetMidi SysEx event into a SysExMsg
        var m2bConverter = new MidiEventToBytesConverter();
		m2bConverter.BytesFormat = BytesFormat.Device;
		byte[] midiEventBytes = m2bConverter.Convert(midiEvent);

        try
        {
            SysExMsg sysExMsg = new SysExMsg(midiEventBytes);
            ProcessMessage(null, sysExMsg);
        }
        catch {
			Console.WriteLine(BitConverter.ToString(midiEventBytes).Replace("-", " "));
            Console.WriteLine("Invalid MMM SysEx Msg"); 
        }
    }
	void IDisposable.Dispose() { }

    internal string GetConnectionString(object? sender)
    {
        // Determine connection string type
        string connectionString = string.Empty;
        if (sender is SerialPort serialPort)
            connectionString = $"Serial:{serialPort.PortName}";
        else if (sender is string s && System.Net.IPAddress.TryParse(s, out var ip))
            connectionString = $"IPV4:{ip}";
        else if (sender is System.Net.IPEndPoint endPoint)
            connectionString = $"IPV4:{endPoint.Address}";
        else
            connectionString = sender?.ToString() ?? "Unknown";
        return connectionString;
	}
}

