using Avalonia.Threading;
using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using MMM_Core;
using MMM_Device;
using MMM_Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceListViewModel : ComponentViewModel
{
	public SessionContext Context => SessionContext.Instance;
	public DeviceListViewModel() : base(PageComponentNames.DeviceList)
	{
		RefreshSerialPorts();
		MMM.Instance.serialManager.OnPortsUpdated += (s, e) => RefreshSerialPorts();

		DeviceManager.Instance.OnListUpdated += (s, e) =>
		{
			Dispatcher.UIThread.Post(() =>
			{
				DevicesList.Clear();
				foreach (var device in DeviceManager.Instance.Devices.Values)
				{
					Console.WriteLine($"Device Added: {device.Name} on {device.ConnectionString}");
					DevicesList.Add(device);
				}
				if (Context.SelectedDevice != null && DevicesList.Contains(Context.SelectedDevice))
					SelectedDevice = Context.SelectedDevice;
			});
		};

		DeviceManager.Instance.DeviceUpdated += (s, e) =>
		{
			Dispatcher.UIThread.Post(() =>
			{
				DevicesList.Clear();
				foreach (var device in DeviceManager.Instance.Devices.Values)
				{
					DevicesList.Add(device);
				}
				if (Context.SelectedDevice != null && DevicesList.Contains(Context.SelectedDevice))
					SelectedDevice = Context.SelectedDevice;
			});
		};
	}

	public ObservableCollection<int> BaudRates { get; } = new(){
		300,1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
	};
	public ObservableCollection<string> SerialPortList { get; } = new();
	public ObservableCollection<Device> DevicesList { get; } = new();

	[ObservableProperty]
	private Device? _selectedDevice;

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
			Console.WriteLine($"Removing Device: {SelectedDevice.Name} on {SelectedDevice.ConnectionString}");
			MMM.Instance.serialManager.FreeConnection(SelectedDevice.ConnectionString);
			SelectedDevice = null;
			Context.SelectedDevice = null;
		}
	}

	[RelayCommand]
	public void Update(Device device)
	{

	}
	partial void OnSelectedDeviceChanged(Device? value)
	{
		if (SelectedDevice != null)
		{
			Context.SelectedDevice = value;
		}
	}
}

