using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AvaloniaGUI.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
	private readonly PageFactory _pageFactory;
	//private readonly DatabaseFactory _databaseFactory;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MidiPageIsActive))]
	[NotifyPropertyChangedFor(nameof(HomePageIsActive))]
	private PageViewModel _currentPage;

	//[ObservableProperty]
	//private DialogViewModel _dialog;

	public bool HomePageIsActive => CurrentPage.PageName == ApplicationPageNames.Home;
	public bool MidiPageIsActive => CurrentPage.PageName == ApplicationPageNames.Midi;

	/// <summary>
	/// Design-time only constructor
	/// </summary>
	// Allow nullable PageFactory for now in designer... ideally get it working
#pragma warning disable CS8618, CS9264
	public MainViewModel()
	{
		//CurrentPage = new SettingsPageViewModel(new DatabaseFactory(() => new DatabaseService(new ApplicationDbContext())));
		//CurrentPage = new MoppyPageViewModel();
	}
#pragma warning restore CS8618, CS9264

	public MainViewModel(PageFactory pageFactory)
	{
		_pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
		CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>();
	}

	[RelayCommand]
	private void GoToHome() => CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>();

	[RelayCommand]
	private void GoToMidi() => CurrentPage = _pageFactory.GetPageViewModel<MidiPageViewModel>(afterCreation => afterCreation.TestName = "TestName");

	[RelayCommand]
	private void GoToMoppy() => CurrentPage = _pageFactory.GetPageViewModel<MoppyPageViewModel>();
	[RelayCommand]
	private void GoToGettingStarted() => CurrentPage = _pageFactory.GetPageViewModel<GuidePageViewModel>();
	[RelayCommand]
	private void GoToDocumentation() => CurrentPage = _pageFactory.GetPageViewModel<DocsPageViewModel>();
	[RelayCommand]
	private void GoToAbout() => CurrentPage = _pageFactory.GetPageViewModel<AboutPageViewModel>();
}