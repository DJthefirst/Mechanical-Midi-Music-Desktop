using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaGUI.ViewModels;

public partial class MidiPlayerViewModel : ViewModelBase
{
	[ObservableProperty]
	private string? _name;

	[ObservableProperty]
	private int _duration;

	[ObservableProperty]
	private bool _isSelected;

}

