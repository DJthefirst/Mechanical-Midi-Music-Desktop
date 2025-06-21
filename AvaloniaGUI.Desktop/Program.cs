using System;

using Avalonia;
using Avalonia.Svg.Skia;

namespace AvaloniaGUI.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

	public static AppBuilder BuildAvaloniaApp()
	{
		GC.KeepAlive(typeof(SvgImageExtension).Assembly);
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
	}

}
