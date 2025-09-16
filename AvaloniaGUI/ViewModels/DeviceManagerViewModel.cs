using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
				Name = SelectedDevice?.Name ?? string.Empty;
				Id = SelectedDevice?.SYSEX_DEV_ID ?? 0;
				OmniMode = SelectedDevice?.OmniMode ?? false;
			}
		};
	}

	[ObservableProperty]
	private Device? _selectedDevice;

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
		device.Name = Name;
		device.SYSEX_DEV_ID = Id;
		device.OmniMode = OmniMode;

		IConnection? connection = SelectedDevice != null ? DeviceManager.Instance.GetConnection(SelectedDevice) : null;
		if (connection == null) return;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(Id, SysEx.SetDeviceName, device.GetDeviceName());
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());
	}
}

