using Avalonia.Threading;
using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using MMM_Core;
using MMM_Core.MidiManagers;
using MMM_Device;
using MMM_Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using static Grpc.Core.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AvaloniaGUI.ViewModels;

//.OrderBy(entry => entry.Device.SYSEX_DEV_ID)
//.ThenBy(entry => entry.Device.Name)
//.ToList();

public partial class DeviceListViewModel : ComponentViewModel
{
	public SessionContext Context => SessionContext.Instance;
	public DeviceListViewModel() : base(PageComponentNames.DeviceList)
	{
		RefreshSerialPorts();
		MMM.Instance.serialManager.OnPortsUpdated += (s, e) => RefreshSerialPorts();

		DeviceManager.Instance.OnDeviceListChanged += (s, e) =>
		{
			Dispatcher.UIThread.Post(() => RefreshDeviceList());	
		};

		DeviceManager.Instance.OnDeviceChanged += (s, entry) =>
		{
			Dispatcher.UIThread.Post(() => RefreshDeviceList(entry));
		};
	}

	public ObservableCollection<int> BaudRates { get; } = new(){
		300,1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
	};
	public ObservableCollection<string> SerialPortList { get; } = new();
	public ObservableCollection<DeviceEntry> DevicesList { get; set; } = new();

	[ObservableProperty]
	private DeviceEntry? _selectedDevice;

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
		var connectedDevices = DeviceManager.Instance.Devices;
		foreach (var entry in connectedDevices)
		{
			DevicesList.Add(entry);
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
			Console.WriteLine($"Removing Device: {SelectedDevice?.Device.Name} on {SelectedDevice?.Device.ConnectionString}");
			var connection = SelectedDevice?.Connection as SerialConnection;
			if (connection != null)
			{
				MMM.Instance.serialManager.RemoveConnection(connection);
			}
			SelectedDevice = null;
			Context.SelectedDevice = null;
		}
	}

	[RelayCommand]
	public void Update(Device device)
	{

	}
	partial void OnSelectedDeviceChanged(DeviceEntry? value)
	{
		if (SelectedDevice != null)
		{
			Console.WriteLine($"Context {value?.Connection.ConnectionString}");
			Context.SelectedDevice = value;
		}
	}

	private void RefreshDeviceList()
	{
		var tempDevContext = Context.SelectedDevice;
		var currentDevices = DeviceManager.Instance.Devices;

		// Remove devices not present anymore
		for (int i = DevicesList.Count - 1; i >= 0; i--)
		{
			if (!currentDevices.Contains(DevicesList[i]))
			{
				Console.WriteLine($"Removed {DevicesList[i].Connection.ConnectionString}");
				DevicesList.RemoveAt(i);
			}
		}
	}

	private void RefreshDeviceList(DeviceEntry entry)
	{
		var currentDevices = DeviceManager.Instance.Devices;

		// Add new Devices
		var idx = DevicesList.IndexOf(entry);
		if (idx == -1)
		{
			DevicesList.Add(entry);
		}

		// Remove devices not present anymore
		for (int i = DevicesList.Count - 1; i >= 0; i--)
		{
			if (!currentDevices.Contains(DevicesList[i]))
			{
				Console.WriteLine($"Removed {DevicesList[i].Connection.ConnectionString}");

				if (SelectedDevice != null && DevicesList[i].Equals(SelectedDevice.Value))
				{
					SelectedDevice = null;
					Context.SelectedDevice = null;
				}

				DevicesList.RemoveAt(i);
			}
		}
	}
}

