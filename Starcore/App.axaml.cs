using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using StarCore.Services;
using StarCore.ViewModels;
using StarCore.Views;

namespace StarCore;

public partial class App : Application
{

	public App()
	{
		ReplicatedStorageService.Initialize();
		ClientStorageService.Initialize();
		
		// TODO: Let users choose their own server
		_ = ServerService.ConnectAsync("http://localhost:5000");
	}

	public override void Initialize() => AvaloniaXamlLoader.Load(this);
	
	public override void OnFrameworkInitializationCompleted()
	{
		var mainViewModel = new MainViewModel();
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			DisableAvaloniaDataAnnotationValidation();
			desktop.MainWindow = new MainWindow { DataContext = mainViewModel };
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
			singleViewPlatform.MainView = new MainView { DataContext = mainViewModel };

		base.OnFrameworkInitializationCompleted();
	}
	private void DisableAvaloniaDataAnnotationValidation()
	{
		// Get an array of plugins to remove
		var dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

		// Remove each entry found
		foreach (var plugin in dataValidationPluginsToRemove) { BindingPlugins.DataValidators.Remove(plugin); }
	}

}