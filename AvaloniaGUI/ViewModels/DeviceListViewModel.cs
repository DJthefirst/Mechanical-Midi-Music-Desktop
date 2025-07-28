using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Device;

namespace AvaloniaGUI.ViewModels;

public partial class DeviceListViewModel() : ComponentViewModel(PageComponentNames.DeviceList)
{
	[ObservableProperty]
	private Device? _selectedDevice;



	[RelayCommand]
	public void Update(Device device)
	{

	}
}

