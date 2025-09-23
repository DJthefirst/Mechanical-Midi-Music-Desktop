using CommunityToolkit.Mvvm.ComponentModel;
using MMM_Device;
using MMM_Core;
using System;

namespace AvaloniaGUI.Services;

public class SessionContext : ObservableObject
{
	private static readonly Lazy<SessionContext> _instance = new(() => new SessionContext());
	public static SessionContext Instance => _instance.Value;

	private DeviceEntry? _selectedDevice;
	public DeviceEntry? SelectedDevice
	{
		get => _selectedDevice;
		set => SetProperty(ref _selectedDevice, value);
	}

	private Distributor? _selectedDistributor;
	public Distributor? SelectedDistributor
	{
		get => _selectedDistributor;
		set => SetProperty(ref _selectedDistributor, value);
	}
}