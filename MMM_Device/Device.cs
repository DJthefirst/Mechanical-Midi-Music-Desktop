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
	const int BOOL_MUTED = 0x0001;
	const int BOOL_OMNIMODE = 0x0002;

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
	private bool _muted = false;
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

	private void SetDeviceBoolean(int deviceBoolValue)
	{
		Muted = ((deviceBoolValue & BOOL_MUTED) != 0);
		OmniMode = ((deviceBoolValue & BOOL_OMNIMODE) != 0);
	}

	public void SetDeviceConstruct(byte[] deviceObj)
	{
		Id = (deviceObj[0] << 7) | (deviceObj[1] << 0);
		MaxNumInstruments = deviceObj[2];
		NumSubInstruments = deviceObj[3];
		Instrument = (InstrumentType)deviceObj[4];
		Platform = (PlatformType)deviceObj[5];
		MinMidiNote = deviceObj[6];
		MaxMidiNote = deviceObj[7];
		FirmwareVersion = (deviceObj[8] << 7) | (deviceObj[9] << 0);
		int boolValue = (deviceObj[10] << 7) | (deviceObj[11] << 0);
		SetDeviceBoolean(boolValue);

		Name = System.Text.Encoding.ASCII
			.GetString(deviceObj.AsSpan(20, NUM_NAME_BYTES))
			.TrimEnd('\0');
    }

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

	private int GetDeviceBoolean()
    {
        int deviceBoolValue = 0;
        if (Muted) deviceBoolValue |= BOOL_MUTED;
        if (OmniMode) deviceBoolValue |= BOOL_OMNIMODE;
        return deviceBoolValue;
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

		int boolValue = GetDeviceBoolean();
		
		deviceObj[0] = (byte)((Id >> 7) & 0x7F); //Device ID MSB
		deviceObj[1] = (byte)((Id >> 0) & 0x7F); //Device ID LSB
		deviceObj[2] = MaxNumInstruments;
		deviceObj[3] = NumSubInstruments;
		deviceObj[4] = (byte)Instrument;
		deviceObj[5] = (byte)Platform;
		deviceObj[6] = MinMidiNote;
		deviceObj[7] = MaxMidiNote;
		deviceObj[8] = (byte)((FirmwareVersion >> 7) & 0x7F);
		deviceObj[9] = (byte)((FirmwareVersion >> 0) & 0x7F);
		deviceObj[10] = (byte)((boolValue >> 7) & 0x7F);
		deviceObj[11] = (byte)((boolValue >> 0) & 0x7F);
		
		// Bytes 12-19: Reserved
		for (int i = 12; i < 20; i++)
		{
			deviceObj[i] = 0;
		}

		// Bytes 20-39: Device Name
		for (int i = 0; i < NUM_NAME_BYTES; i++)
		{
			if (Name.Length > i) deviceObj[20 + i] = (byte)Name[i];
			else deviceObj[20 + i] = 0x20; //Hex for space
		}

		return deviceObj;
	}
}

