using AvaloniaGUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
	string _testName = "";


	/// <summary>
	/// Design-time only constructor
	/// </summary>
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