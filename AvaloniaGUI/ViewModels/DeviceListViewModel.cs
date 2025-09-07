using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Device;
using MMM_Core;
using MMM_Server;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceListViewModel : ComponentViewModel
{
	public DeviceListViewModel() : base(PageComponentNames.DeviceList)
	{
		RefreshSerialPorts();
	}

	public ObservableCollection<int> BaudRates { get; } = new(){
		300,1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
	};

	[ObservableProperty]
	private string? _selectedDevice;

	[ObservableProperty]
	private string? _selectedPort;

	[ObservableProperty]
	private string _name = "ToDo?";

	public ObservableCollection<string> SerialPortList { get; } = new();
	public ObservableCollection<Device> SerialDeviceList { get; } = new();

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
			//MMM.Instance.serialManager.RemoveConnection(SelectedDevice);
			SelectedDevice = null;
		}
	}

	[RelayCommand]
	public void Update(Device device)
	{

	}
}

