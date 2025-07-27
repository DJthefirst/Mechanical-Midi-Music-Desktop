using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using AvaloniaGUI.ViewModels;
using ExCSS;
using System.Diagnostics;

namespace AvaloniaGUI.Views;

public partial class HomePageView : UserControl
{
	public HomePageView()
	{
		InitializeComponent();
		this.SizeChanged += OnSizeChanged;
	}

	private void OnSizeChanged(object? sender, SizeChangedEventArgs args)
	{
		var width = args.NewSize.Width;
		Debug.WriteLine($"Width: {width}");
		(DataContext as HomePageViewModel)?.UpdateWidthCommand?.Execute(width);
	}
}