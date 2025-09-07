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
using System.Collections.ObjectModel;
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
	private bool _isPlaying = false;

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

	public ObservableCollection<string> MidiOutputs { get; } = new ObservableCollection<string>(MMM.Instance.midiPortOutManager.AvailableConnections());
	public ObservableCollection<string> MidiInputs { get; } = new ObservableCollection<string>(MMM.Instance.midiPortInManager.AvailableConnections());
	public ObservableCollection<IMidiSong> SongList { get; } = new ObservableCollection<IMidiSong>(MMM.Instance.playlist.Songs);
	public ObservableCollection<string> SelectedMidiInputs { get; } = new();
	public ObservableCollection<string> SelectedMidiOutputs { get; } = new();

	[RelayCommand]
	private void PlayPause(){
		if (IsPlaying) MMM.Instance.player.Pause();
		else MMM.Instance.player.Play();
		IsPlaying = !IsPlaying;
	}

	[RelayCommand]
	private void Stop(){
		MMM.Instance.player.Stop();
		IsPlaying = false;
		SongDuration = MMM.Instance.player.GetMaxPositionMs();
		SongPosition = 0;
	}

	[RelayCommand]
	private void Prev() { 
		MMM.Instance.player.Prev();
		StopCommand.Execute(null);
	}

	[RelayCommand]
	private void Next(){
		MMM.Instance.player.Next();
		StopCommand.Execute(null);
	}

		//[RelayCommand]
		//private void Skip() => MMM.Instance.player.Skip();

	[RelayCommand]
	private void Repeat() => MMM.Instance.player.Repeat(1);

	[RelayCommand]
	private async Task AddSongAsync()
	{
		var window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window?.StorageProvider == null) return;

		var options = new Avalonia.Platform.Storage.FilePickerOpenOptions
		{
			Title = "Select MIDI files",
			AllowMultiple = true,
			FileTypeFilter = new List<Avalonia.Platform.Storage.FilePickerFileType>
						{
							new("MIDI files") { Patterns = new[] { "*.mid", "*.midi" } }
						},
		};

		var files = await window.StorageProvider.OpenFilePickerAsync(options);
		if (files == null || files.Count == 0) return;

		foreach (var file in files.OfType<Avalonia.Platform.Storage.IStorageFile>())
		{
			var pathFile = file.Path.LocalPath;
			if (!string.IsNullOrEmpty(pathFile) && IsMidiFile(pathFile))
			{
				MMM.Instance.playlist.AddSong(new FileInfo(pathFile));
			}
		}
		UpdateSongList();
	}

	[RelayCommand]
	private async Task AddDirectoryRecursiveAsync()
	{
		var window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window?.StorageProvider == null) return;

		var options = new Avalonia.Platform.Storage.FolderPickerOpenOptions
		{
			Title = "Select Folder(s)",
			AllowMultiple = false
		};

		var folders = await window.StorageProvider.OpenFolderPickerAsync(options);
		if (folders == null || folders.Count == 0) return;

		foreach (var folder in folders.OfType<Avalonia.Platform.Storage.IStorageFolder>())
		{
			var dirInfo = new DirectoryInfo(folder.Path.LocalPath);
			if (dirInfo.Exists)
			{
				AddMidiFilesRecursive(dirInfo);
			}
		}
		UpdateSongList();
	}

	[RelayCommand]
	private void RemoveSelectedSong(string songName)
	{
		var songToRemove = MMM.Instance.playlist.GetCurSong();
		if (songToRemove != null)
		{
			MMM.Instance.playlist.RemoveSong(songToRemove);
			UpdateSongList();
		}
		StopCommand.Execute(null);
	}

	[RelayCommand]
	private void ClearPlaylist()
	{
		MMM.Instance.playlist.Songs.Clear();
		UpdateSongList();
		StopCommand.Execute(null);
	}

	private void AddMidiFilesRecursive(DirectoryInfo directory)
	{
		foreach (var file in directory.GetFiles())
		{
			if (IsMidiFile(file.FullName))
			{
				MMM.Instance.playlist.AddSong(file);
			}
		}

		foreach (var subDir in directory.GetDirectories())
		{
			AddMidiFilesRecursive(subDir);
		}
	}

	private bool IsMidiFile(string path)
	{
		var ext = Path.GetExtension(path).ToLowerInvariant();
		return ext == ".mid" || ext == ".midi";
	}

	private void UpdateSongList()
	{
		SongList.Clear();
		foreach (var song in MMM.Instance.playlist.Songs){
			SongList.Add(song);
		}
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
		SongPosition = e.CurPlaybackTime;
	}

	public void NavigateTo(int position) => MMM.Instance.player.SetPositionMs(position);
}

