using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using MMM_Core;
using MMM_Device;
using MMM_Server.Grpc_Services;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceManagerViewModel : ComponentViewModel
{
	public SessionContext Context => SessionContext.Instance;


	public DeviceManagerViewModel() : base(PageComponentNames.DeviceManager)
	{
		Context.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Context.SelectedDevice))
			{
				SelectedDevice = Context.SelectedDevice;
				Name = SelectedDevice?.Device.Name ?? string.Empty;
				Id = SelectedDevice?.Id ?? 0;
				OmniMode = SelectedDevice?.Device.OmniMode ?? false;
			}
		};
	}

	[ObservableProperty]
	private DeviceEntry? _selectedDevice;

	[ObservableProperty]
	private int _id = 0;

	[ObservableProperty]
	private string _name = string.Empty;

	[ObservableProperty]
	private bool _omniMode = false;

	[RelayCommand]
	public void Update()
	{
		Device device = new Device();
		// Pad Name to exactly 20 characters with spaces
		device.Name = Name.Length >= 20 ? Name.Substring(0, 20) : Name.PadRight(20, ' ');
		device.Id = Id;
		device.OmniMode = OmniMode;

		IConnection? connection = SelectedDevice?.Connection;
		if (connection == null) return;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(Id, SysEx.SetDeviceName, device.GetDeviceName());
		connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(Id, SysEx.GetDeviceConstruct, []);
		connection.SendEvent(msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Reset()
	{

		IConnection? connection = SelectedDevice?.Connection;
		if (connection == null) return;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(Id, SysEx.ResetDeviceConfig, []);
		connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(Id, SysEx.GetDeviceConstruct, []);
		connection.SendEvent(msg.ToMidiEvent());
	}
}

