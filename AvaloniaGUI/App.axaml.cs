using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Avalonia.Metadata;
using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
//using AvaloniaGUI.Services;
using AvaloniaGUI.ViewModels;
using AvaloniaGUI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

using MMM_Core;

namespace AvaloniaGUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

	public override void OnFrameworkInitializationCompleted()
	{
		var collection = new ServiceCollection();

		// GUI Page Services
		collection.AddSingleton<MainViewModel>();
		collection.AddTransient<MidiPageViewModel>();
		collection.AddTransient<HomePageViewModel>();
		collection.AddTransient<MoppyPageViewModel>();
		collection.AddTransient<GuidePageViewModel>();
		collection.AddTransient<DocsPageViewModel>();
		collection.AddTransient<AboutPageViewModel>();

		collection.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
		{
			_ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
			_ when type == typeof(MidiPageViewModel) => x.GetRequiredService<MidiPageViewModel>(),
			_ when type == typeof(MoppyPageViewModel) => x.GetRequiredService<MoppyPageViewModel>(),
			_ when type == typeof(GuidePageViewModel) => x.GetRequiredService<GuidePageViewModel>(),
			_ when type == typeof(DocsPageViewModel) => x.GetRequiredService<DocsPageViewModel>(),
			_ when type == typeof(AboutPageViewModel) => x.GetRequiredService<AboutPageViewModel>(),
			_ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
		});

		//GUI Component Services
		collection.AddTransient<DeviceListViewModel>();
		collection.AddTransient<DeviceManagerViewModel>();
		collection.AddTransient<DistributorListViewModel>();
		collection.AddTransient<DistributorManagerViewModel>();
		collection.AddScoped<MidiPlayerViewModel>();

		//collection.AddSingleton<Func<Type, ComponentViewModel>>(x => type => type switch
		//{
		//	_ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),

		//	_ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
		//});

		collection.AddSingleton<PageFactory>();
		collection.AddSingleton<MMM>();
		//collection.AddSingleton<DialogService>();

		//collection.AddTransient<PrinterService>();

		// Database services
		//collection.AddTransient<ApplicationDbContext>();
		//collection.AddTransient<DatabaseService>();
		//collection.AddSingleton<Func<DatabaseService>>(x => x.GetRequiredService<DatabaseService>);
		//collection.AddSingleton<DatabaseFactory>();

		var services = collection.BuildServiceProvider();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = services.GetRequiredService<MainViewModel>()
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView()
			{
				DataContext = services.GetRequiredService<MainViewModel>()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
