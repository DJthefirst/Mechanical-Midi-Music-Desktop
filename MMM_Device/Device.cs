using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO.Ports;

namespace MMM_Device;
public enum InstrumentType
{
	None = 0,
	SW_PWM = 1,
	HW_PWM = 2,
	StepHW_PWM,
	StepSW_PWM,
	ShiftRegister,
	StepperMotor = 0x10,
	FloppyDrive = 0x11
};

public enum PlatformType
{
    ESP32 = 1,
    ESP8266,
    ArduinoUno,
    ArduinoMega,
    ArduinoDue,
    ArduinoMicro,
    ArduinoNano
};


public partial class Device : ObservableObject
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Constants
	////////////////////////////////////////////////////////////////////////////////////////////////////
	
	const byte NUM_NAME_BYTES = 20;
	const byte NUM_CFG_BYTES = 40;
	const byte BOOL_OMNIMODE = 0x01;

	public Device() { }
	public Device(byte[] deviceConstruct)
	{
		this.SetDeviceConstruct(deviceConstruct);
	}



	//Interupt frequency. A smaller resolution produces more accurate notes but leads to instability.
	public int TIMER_RESOLUTION = 8;

	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Implied Values
	////////////////////////////////////////////////////////////////////////////////////////////////////

	[ObservableProperty]
	private string _connectionString = "Unknown";
	[ObservableProperty]
	private int _numDistributors = 0;

	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Explicit Values
	////////////////////////////////////////////////////////////////////////////////////////////////////

	[ObservableProperty]
	private int _id = 0x0001;
	[ObservableProperty]
	private string _name = "New Device";
	[ObservableProperty]
	private bool _omniMode = false;
	[ObservableProperty]
	private byte _maxPolyphonicNotes = 1;
	[ObservableProperty]
	private byte _maxNumInstruments = 1;
	[ObservableProperty]
	private byte _numSubInstruments = 1;
	[ObservableProperty]
	private byte _minMidiNote = 0;
	[ObservableProperty]
	private byte _maxMidiNote = 127;

	[ObservableProperty]
	private InstrumentType _instrument = InstrumentType.None;
	[ObservableProperty]
	private PlatformType _platform = PlatformType.ESP32;
	[ObservableProperty]
	private int _firmwareVersion = 0;

	public ObservableCollection<Distributor> Distributors { get; } = new();

	////////////////////////////////////////////////////////////////////////////////////////////////////
	//Methods
	////////////////////////////////////////////////////////////////////////////////////////////////////


	public void AddDistributor(Distributor distributor)
	{
		Distributors.Add(distributor);
	}

	public void AddDistributor(byte[] bytes)
	{
		Distributor distributor = new(bytes);
		if(distributor.Index is int idx && idx < Distributors.Count)
		{
			Distributors[idx] = distributor;
		}
		else {
			Distributors.Add(distributor);
		}		
	}

	private void SetDeviceBoolean(byte deviceBoolByte)
	{
		OmniMode = ((deviceBoolByte & 0x01) != 0);
	}

	public void SetDeviceConstruct(byte[] deviceObj)
	{
		Id = (deviceObj[0] << 7) | (deviceObj[1] << 0);
		SetDeviceBoolean(deviceObj[2]);
		MaxNumInstruments = deviceObj[3];
		NumSubInstruments = deviceObj[4];
		Instrument = (InstrumentType)deviceObj[5];
		Platform = (PlatformType)deviceObj[6];
		MinMidiNote = deviceObj[7];
		MaxMidiNote = deviceObj[8];
		FirmwareVersion = (deviceObj[9] << 7) | (deviceObj[10] << 0);

		Name = System.Text.Encoding.ASCII
			.GetString(deviceObj.AsSpan(20, NUM_NAME_BYTES))
			.TrimEnd('\0');
    }

	//public void SetAllDistributors(byte[] data)
	//{
	//    int numDistributors = (data.Length / Distributor.NUM_CFG_BYTES);
	//    Distributors.Clear();
	//    for (int i = 0; i < numDistributors; ++i)
	//    {
	//        int idx = i * Distributor.NUM_CFG_BYTES;
	//        Distributors.Add(new Distributor(data[(idx)..(idx+Distributor.NUM_CFG_BYTES)]));
	//    }
	//}

	public static string GetConnectionString(object? sender)
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

	private byte GetDeviceBoolean()
    {
        byte deviceBoolByte = 0;
        if (OmniMode) deviceBoolByte |= (1 << 0);
        return deviceBoolByte;
    }

    public byte[] GetDeviceName()
    {
        byte[] nameObj = new byte[NUM_NAME_BYTES];
		for (int i = 0; i < NUM_NAME_BYTES; i++)
		{
			if (Name.Length > i) nameObj[i] = (byte)Name.ToCharArray()[i];
			else nameObj[i] = 0;
		}
		return nameObj;
	}

	public byte[] GetDeviceConstruct()
	{
		byte[] deviceObj = new byte[NUM_CFG_BYTES];

		// Use backing fields and properties instead of undefined constants
		deviceObj[0] = (byte)((Id >> 7) & 0x7F); //Device ID MSB
		deviceObj[1] = (byte)((Id >> 0) & 0x7F); //Device ID LSB
		deviceObj[2] = (byte)GetDeviceBoolean();
		deviceObj[3] = MaxNumInstruments;
		deviceObj[4] = NumSubInstruments;
		deviceObj[5] = (byte)Instrument;
		deviceObj[6] = (byte)Platform;
		deviceObj[7] = MinMidiNote;
		deviceObj[8] = MaxMidiNote;
		deviceObj[9] = (byte)((FirmwareVersion >> 7) & 0x7F);
		deviceObj[10] = (byte)((FirmwareVersion >> 0) & 0x7F);

		for (int i = 0; i < NUM_NAME_BYTES; i++)
		{
			if (Name.Length > i) deviceObj[20 + i] = (byte)Name[i];
			else deviceObj[20 + i] = 0x20; //Hex for space
		}

		return deviceObj;
	}
}

