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
	private ComponentViewModel _midiPlayerPage;

	[ObservableProperty]
	string _testName = "";


	/// <summary>
	/// Design-time only constructor
	/// </summary>
	// Allow nullable PageFactory for now in designer... ideally get it working
#pragma warning disable CS8618, CS9264
	public HomePageViewModel(MidiPlayerViewModel midiPlayerViewModel)
		: base(ApplicationPageNames.Home)
	{
		MidiPlayerPage = midiPlayerViewModel;
	}
#pragma warning restore CS8618, CS9264


}