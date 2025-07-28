using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Device;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceManagerViewModel : ComponentViewModel
{
	private readonly DeviceListViewModel _deviceListViewModel;

	public DeviceManagerViewModel(DeviceListViewModel deviceListViewModel)
		: base(PageComponentNames.DeviceManager)
	{
		_deviceListViewModel = deviceListViewModel;
	}

	[ObservableProperty]
	private string _name = "Device Manager";

	[RelayCommand]
	public void SaveName()
	{
		Device device = new Device();

		device.Name = "New Device";
		device.OmniMode = false;
		device.SYSEX_DEV_ID = 1;
		_deviceListViewModel.Update(device);
	}


	//---------- Device Configuration ----------

}

