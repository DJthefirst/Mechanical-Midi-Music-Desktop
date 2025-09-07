using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Device;
using MMM_Core;
using MMM_Server;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Threading;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceListViewModel : ComponentViewModel
{
	public DeviceListViewModel() : base(PageComponentNames.DeviceList)
	{
		RefreshSerialPorts();

		DeviceManager.Instance.OnListUpdated += (s, e) =>
		{
			Dispatcher.UIThread.Post(() =>
			{
				DevicesList.Clear();
				foreach (var device in DeviceManager.Instance.Devices.Values)
				{
					DevicesList.Add(device);
				}
			});
		};

		Device device1 = new Device()
		{
			Name = "Test Device",
			ConnectionString = "COM3",
			SYSEX_DEV_ID = 1,
			MAX_NUM_INSTRUMENTS = 2,
			NUM_SUBINSTRUMENTS = 1,
			INSTRUMENT_TYPE = InstrumentType.ShiftRegister,
			PLATFORM_TYPE = PlatformType.ESP32,
			MIN_MIDI_NOTE = 0,
			MAX_MIDI_NOTE = 127,
			FIRMWARE_VERSION = 0x0001
		};

		Device device2 = new Device()
		{
			Name = "Test Device 2",
			ConnectionString = "COM5",
			SYSEX_DEV_ID = 2,
			MAX_NUM_INSTRUMENTS = 2,
			NUM_SUBINSTRUMENTS = 1,
			INSTRUMENT_TYPE = InstrumentType.FloppyDrive,
			PLATFORM_TYPE = PlatformType.ArduinoDue,
			MIN_MIDI_NOTE = 0,
			MAX_MIDI_NOTE = 127,
			FIRMWARE_VERSION = 0x0001
		};
		///DevicesList.Add(device1);
		//DevicesList.Add(device2);

	}

	public ObservableCollection<int> BaudRates { get; } = new(){
		300,1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
	};
	public ObservableCollection<string> SerialPortList { get; } = new();
	public ObservableCollection<Device> DevicesList { get; } = new();

	[ObservableProperty]
	private string? _selectedDevice;

	[ObservableProperty]
	private string? _selectedPort;

	[ObservableProperty]
	private string _name = "ToDo?";

	[RelayCommand]
	private void RefreshSerialPorts()
	{
		SerialPortList.Clear();
		var availablePorts = MMM.Instance.serialManager.AvailableConnections();
		var connectedPorts = MMM.Instance.serialManager.ListConnections();

		foreach (var port in availablePorts)
		{
			if (!connectedPorts.Contains(port))
			{
				SerialPortList.Add(port);
			}
		}
	}

	private void RefreshConnectedDevices()
	{
		DevicesList.Clear();
		var connectedDevices = DeviceManager.Instance.Devices.Values;
		foreach (var device in connectedDevices)
		{
			DevicesList.Add(device);
		}
	}

	[RelayCommand]
	private void AddSerialDevice()
	{
		if (SelectedPort == null) return;
		MMM.Instance.serialManager.AddConnection(SelectedPort);
		RefreshSerialPorts();
	}

	[RelayCommand]
	private void RemoveSelectedDevice()
	{
		if (SelectedDevice != null)
		{
			MMM.Instance.serialManager.RemoveConnection(SelectedDevice);
			SelectedDevice = null;
		}
	}

	[RelayCommand]
	public void Update(Device device)
	{

	}
}

