using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using AvaloniaGUI.ViewModels;
using MMM_Server;
using MMM_Core;
using System;
using System.Linq;

namespace AvaloniaGUI.Views;

public partial class MidiPlayerView : UserControl
{
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
			(DataContext as MidiPlayerViewModel)?.NavigateTo((int)PlayerNavBar.Value * 1000);
		};

		// Set SelectedItem to the current song object (IMidiSong)
		SongSelector.SelectedItem = MMM.Instance.playlist.GetCurSong();

		MMM.Instance.playlist.OnSongChanged += (s, e) =>
		{
			Avalonia.Threading.Dispatcher.UIThread.Post(() => SongSelector.SelectedItem = e);
		};

		SongSelector.SelectionChanged += (s, e) =>
		{
			var selectedSong = SongSelector.SelectedValue as IMidiSong;
			var currentSong = MMM.Instance.playlist?.GetCurSong();

			if (selectedSong == null || selectedSong == currentSong) return;
			if ((DataContext as MidiPlayerViewModel) == null) return;

			if ((DataContext as MidiPlayerViewModel).IsPlaying)
			{
				MMM.Instance.player.Play(selectedSong);
			}
			else
			{
				MMM.Instance.playlist.SetCurSong(selectedSong.Name);
				(DataContext as MidiPlayerViewModel)?.StopCommand.Execute(null);
			}
			Console.WriteLine(selectedSong?.Name);
		};

		MidiOutSelector.SelectionChanged += (s, e) =>
		{
			(DataContext as MidiPlayerViewModel)?.SelectedMidiOutputs.Clear();
			MidiOutSelector.SelectionChanged += (s, e) =>
			{
				var viewModel = DataContext as MidiPlayerViewModel;
				if (viewModel == null)
					return;

				var existingConnections = MMM.Instance.midiPortOutManager.ListConnections();

				viewModel.SelectedMidiOutputs.Clear();
				foreach (var item in MidiOutSelector.SelectedItems)
				{
					string itemName = item.ToString();
					viewModel.SelectedMidiOutputs.Add(itemName);

					if (!existingConnections.Contains(itemName))
					{
						MMM.Instance.midiPortOutManager.AddConnection(itemName);
					}
				}

				// Remove connections that are not selected
				foreach (var conn in existingConnections)
				{
					if (!viewModel.SelectedMidiOutputs.Contains(conn))
					{
						MMM.Instance.midiPortOutManager.RemoveConnection(conn);
					}
				}
			};
		};

		MidiInSelector.SelectionChanged += (s, e) =>
		{
			(DataContext as MidiPlayerViewModel)?.SelectedMidiInputs.Clear();
			MidiInSelector.SelectionChanged += (s, e) =>
			{
				var viewModel = DataContext as MidiPlayerViewModel;
				if (viewModel == null)
					return;

				// Get current connections from midiPortInManager
				var existingConnections = MMM.Instance.midiPortInManager.ListConnections();

				viewModel.SelectedMidiInputs.Clear();
				foreach (var item in MidiInSelector.SelectedItems)
				{
					string itemName = item.ToString();
					viewModel.SelectedMidiInputs.Add(itemName);

					if (!existingConnections.Contains(itemName))
					{
						MMM.Instance.midiPortInManager.AddConnection(itemName);
					}
				}

				// Remove connections that are not selected
				foreach (var conn in existingConnections)
				{
					if (!viewModel.SelectedMidiInputs.Contains(conn))
					{
						MMM.Instance.midiPortInManager.RemoveConnection(conn);
					}
				}
			};
		};
	}
}