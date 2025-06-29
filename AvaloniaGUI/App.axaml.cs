using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Avalonia.Metadata;
//using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
// using AvaloniaGUI.Services;
using AvaloniaGUI.ViewModels;
using AvaloniaGUI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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
		collection.AddSingleton<MainViewModel>();
		//collection.AddTransient<ActionsPageViewModel>();
		//collection.AddTransient<HistoryPageViewModel>();
		//collection.AddTransient<HomePageViewModel>();
		//collection.AddTransient<MacrosPageViewModel>();
		//collection.AddTransient<ProcessPageViewModel>();
		//collection.AddTransient<ReporterPageViewModel>();
		//collection.AddTransient<SettingsPageViewModel>();

		//collection.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
		//{
		//	_ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
		//	_ when type == typeof(ProcessPageViewModel) => x.GetRequiredService<ProcessPageViewModel>(),
		//	_ when type == typeof(MacrosPageViewModel) => x.GetRequiredService<MacrosPageViewModel>(),
		//	_ when type == typeof(ActionsPageViewModel) => x.GetRequiredService<ActionsPageViewModel>(),
		//	_ when type == typeof(ReporterPageViewModel) => x.GetRequiredService<ReporterPageViewModel>(),
		//	_ when type == typeof(HistoryPageViewModel) => x.GetRequiredService<HistoryPageViewModel>(),
		//	_ when type == typeof(SettingsPageViewModel) => x.GetRequiredService<SettingsPageViewModel>(),
		//	_ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
		//});

		//collection.AddSingleton<PageFactory>();
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
