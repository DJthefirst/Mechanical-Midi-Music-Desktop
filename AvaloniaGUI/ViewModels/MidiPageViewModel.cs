using AvaloniaGUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;



namespace AvaloniaGUI.ViewModels;

public partial class MidiPageViewModel : PageViewModel{

	[ObservableProperty]
	private ComponentViewModel _midiPlayerPage;

	[ObservableProperty]
	string _testName = "";


	/// <summary>
	/// Design-time only constructor
	/// </summary>
	// Allow nullable PageFactory for now in designer... ideally get it working
#pragma warning disable CS8618, CS9264
	public MidiPageViewModel(MidiPlayerViewModel midiPlayerViewModel)
		: base(ApplicationPageNames.Midi)
	{
		MidiPlayerPage = midiPlayerViewModel;
	}
#pragma warning restore CS8618, CS9264


}

public partial class DesignMidiPageViewModel : MidiPageViewModel
{
	public DesignMidiPageViewModel()
		: base(
			new MidiPlayerViewModel())
	{
	}
}
