using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Core;
using MMM_Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaGUI.ViewModels;

public partial class MidiPlayerViewModel : ComponentViewModel
{
	public MidiPlayerViewModel() : base(PageComponentNames.Player)
	{
		MMM.Instance.player.OnPlaybackTimeUpdated += UpdatePlaybackTime;
	}

	[ObservableProperty]
	private string? _name;

	[NotifyPropertyChangedFor(nameof(TimeDuration))]
	[ObservableProperty]
	private double _songDuration = 180;

	[NotifyPropertyChangedFor(nameof(TimePosition),nameof(NavBarValue))]
	[ObservableProperty]
	private double _songPosition = 0;

	private bool _isNavBarSelected = false;
	public void SetIsNavBarSelected(bool isSelected) => _isNavBarSelected = isSelected;

	private double NavBarValue => _isNavBarSelected ? NavBarValue : SongPosition;

	private string? TimeDuration => GenerateTimestamp((int)SongDuration/1000);

	private string? TimePosition => GenerateTimestamp((int)SongPosition/1000);


	[ObservableProperty]
	private List<string> _songNames = MMM.Instance.playlist.Songs.ConvertAll(song => song.Name);

	[RelayCommand]
	private void Play() => MMM.Instance.player.Play();

	[RelayCommand]
	private void Stop() => MMM.Instance.player.Stop();

	[RelayCommand]
	private void Prev() => MMM.Instance.player.Prev();

	[RelayCommand]
	private void Next() => MMM.Instance.player.Next();

	//[RelayCommand]
	//private void Skip() => MMM.Instance.player.Skip();

	[RelayCommand]
	private void Repeat() => MMM.Instance.player.Repeat(1);

    [RelayCommand]
	private async Task AddSongAsync()
	{
		var window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

		if (window?.StorageProvider == null)
			return;

		var options = new Avalonia.Platform.Storage.FilePickerOpenOptions
		{
			Title = "Select MIDI files or folders",
			AllowMultiple = true,
			FileTypeFilter = new List<Avalonia.Platform.Storage.FilePickerFileType>
						{
							new("MIDI files") { Patterns = new[] { "*.mid", "*.midi" } },
							new("All files") { Patterns = new[] { "*" } }
						},
		};

		var files = await window.StorageProvider.OpenFilePickerAsync(options);
		if (files == null || files.Count == 0)
			return;

		foreach (var file in files)
		{
			if (file is Avalonia.Platform.Storage.IStorageFolder folder)
			{
				var path = folder.Path.LocalPath;
				if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
				{
					AddDirectoryRecursive(new DirectoryInfo(path));
				}
				continue;
			}

			if (file is not Avalonia.Platform.Storage.IStorageFile localFile)
				continue;

			var pathFile = localFile.Path.LocalPath;
			if (string.IsNullOrEmpty(pathFile))
				continue;

			if (Directory.Exists(pathFile))
			{
				AddDirectoryRecursive(new DirectoryInfo(pathFile));
			}
			else if (File.Exists(pathFile) && IsMidiFile(pathFile))
			{
				MMM.Instance.playlist.AddSong(new FileInfo(pathFile));
			}
		}
		SongNames = MMM.Instance.playlist.Songs.ConvertAll(song => song.Name);
	}

	private void AddDirectoryRecursive(DirectoryInfo dir)
	{
		foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
		{
			if (IsMidiFile(file.FullName))
			{
				MMM.Instance.playlist.AddSong(file);
			}
		}
	}

	private bool IsMidiFile(string path)
	{
		var ext = Path.GetExtension(path).ToLowerInvariant();
		return ext == ".mid" || ext == ".midi";
	}


	//[RelayCommand]
	//private void RemoveSong() => MMM.Instance.player.Repeat(1);

	private string GenerateTimestamp(int seconds)
	{
		var minutes = seconds / 60;
		var remainingSeconds = seconds % 60;
		return $"{minutes:D2}:{remainingSeconds:D2}";
	}

	private void UpdatePlaybackTime(object? sender, PlaybackUpdateEventArgs e)
	{
		SongDuration = e.MaxDuration;
		SongPosition = (int)e.CurPlaybackTime;
	}

	public void NavigateTo(int position) => MMM.Instance.player.SetPositionMs(position);
}

