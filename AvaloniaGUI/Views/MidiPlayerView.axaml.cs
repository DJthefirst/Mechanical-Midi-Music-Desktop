using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels;
using MMM_Server;
using System;

namespace AvaloniaGUI.Views;

public partial class MidiPlayerView : UserControl
{
	private bool _isSliderSelected = false;

	public MidiPlayerView()
	{
		InitializeComponent();

		PlayerNavBar.PointerEntered += (s, e) =>
		{
			(DataContext as MidiPlayerViewModel)?.SetIsNavBarSelected(true);
		};
		PlayerNavBar.PointerExited += (s, e) =>
		{
			(DataContext as MidiPlayerViewModel)?.SetIsNavBarSelected(false);
			(DataContext as MidiPlayerViewModel)?.NavigateTo((int)PlayerNavBar.Value*1000);
		};

		SongSelector.Tapped += (s, e) =>
		{
			MMM.Instance.player.Play(SongSelector.SelectedValue.ToString());
			Console.WriteLine(SongSelector.SelectedValue);
		};

		SongSelector.SelectedItem = MMM.Instance.playlist.GetCurSong().Name;
		MMM.Instance.playlist.OnSongChanged += (s, e) => {
			Avalonia.Threading.Dispatcher.UIThread.Post(() => SongSelector.SelectedItem = e.Name);
		};
	}

}