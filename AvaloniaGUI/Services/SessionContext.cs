using CommunityToolkit.Mvvm.ComponentModel;
using MMM_Device;
using System;

namespace AvaloniaGUI.Services;

public class SessionContext : ObservableObject
{
	private static readonly Lazy<SessionContext> _instance = new(() => new SessionContext());
	public static SessionContext Instance => _instance.Value;

	private Device? _selectedDevice;
	public Device? SelectedDevice
	{
		get => _selectedDevice;
		set
		{
			if (SetProperty(ref _selectedDevice, value))
			{
				Console.WriteLine("Updated");
			}
		}
	}

	private Distributor? _selectedDistributor;
	public Distributor? SelectedDistributor
	{
		get => _selectedDistributor;
		set => SetProperty(ref _selectedDistributor, value);
	}
}