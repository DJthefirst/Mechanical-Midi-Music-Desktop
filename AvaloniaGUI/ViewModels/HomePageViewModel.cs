using AvaloniaGUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaGUI.ViewModels;

public partial class HomePageViewModel : PageViewModel
{
	[ObservableProperty]
	private ComponentViewModel _midiPlayerComponent;
	[ObservableProperty]
	private ComponentViewModel _deviceListComponent;
	[ObservableProperty]
	private ComponentViewModel _deviceManagerComponent;
	[ObservableProperty]
	private ComponentViewModel _distributorListComponent;
	[ObservableProperty]
	private ComponentViewModel _distributorManagerComponent;

	[ObservableProperty]
	public bool _isFullWidth = true;

	[ObservableProperty]
	public bool _isMinWidth = false;

	[RelayCommand]
	public void UpdateWidth(double width)
	{
		IsFullWidth = (width > 1250);
		IsMinWidth = (width < 950);
	}

	// Allow nullable PageFactory for now in designer... ideally get it working
#pragma warning disable CS8618, CS9264
	public HomePageViewModel(
		MidiPlayerViewModel midiPlayerViewModel,
		DeviceListViewModel deviceListViewModel,
		DeviceManagerViewModel deviceManagerViewModel,
		DistributorListViewModel distributorListViewModel,
		DistributorManagerViewModel distributorManagerViewModel)
		: base(ApplicationPageNames.Home)
	{
		MidiPlayerComponent = midiPlayerViewModel;
		DeviceListComponent = deviceListViewModel;
		DeviceManagerComponent = deviceManagerViewModel;
		DistributorListComponent = distributorListViewModel;
		DistributorManagerComponent = distributorManagerViewModel;
	}
#pragma warning restore CS8618, CS9264
}

public partial class DesignHomePageViewModel : HomePageViewModel
{
	public DesignHomePageViewModel()
		: base(
			new MidiPlayerViewModel(),
			new DeviceListViewModel(),
			new DeviceManagerViewModel(new DeviceListViewModel()),
			new DistributorListViewModel(),
			new DistributorManagerViewModel())
	{
	}
}