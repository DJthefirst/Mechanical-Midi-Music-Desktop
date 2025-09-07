using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using MMM_Server;
using MMM_Core;
using MMM_Device;

namespace AvaloniaGUI.Views;

public partial class DeviceListView : UserControl
{
	public DeviceListView()
	{
		InitializeComponent();
	}
}