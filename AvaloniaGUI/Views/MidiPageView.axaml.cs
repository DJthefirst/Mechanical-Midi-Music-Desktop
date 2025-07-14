using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels;
using System;

namespace AvaloniaGUI.Views;

public partial class MidiPageView : UserControl
{
    public MidiPageView()
    {
        InitializeComponent();
    }

    private void OnPlayerUpdated()
    {
        // Get View Model
        var viewModel = MidiPlayerComponent.DataContext as MidiPageViewModel;

        // Type Check
        //viewModel?.RefreshComponents();
    }

	protected override void OnInitialized()
	{
        // Fire on Initial Refresh
        OnPlayerUpdated();

		base.OnInitialized();
	}
}