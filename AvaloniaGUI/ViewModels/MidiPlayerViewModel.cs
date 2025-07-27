using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaGUI.ViewModels;

public partial class MidiPlayerViewModel() : ComponentViewModel(PageComponentNames.Player)
{
	[ObservableProperty]
	private string? _name;

	[NotifyPropertyChangedFor(nameof(TimeDuration))]
	[ObservableProperty]
	private int _songDuration = 180;

	[NotifyPropertyChangedFor(nameof(TimePosition))]
	[ObservableProperty]
	private int _songPosition = 10;

	private string? TimeDuration => GenerateTimestamp(SongDuration);

	private string? TimePosition => GenerateTimestamp(SongPosition);

	[ObservableProperty]
	private bool _isSelected;

	private string GenerateTimestamp(int seconds)
	{
		var minutes = seconds / 60;
		var remainingSeconds = seconds % 60;
		return $"{minutes:D2}:{remainingSeconds:D2}";
	}
}

