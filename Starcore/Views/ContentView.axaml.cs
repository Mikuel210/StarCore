using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SDK.Communication;
using StarCore.Services;
using StarCore.ViewModels;

namespace StarCore.Views;

public partial class ContentView : UserControl
{

	public ContentView()
	{
		InitializeComponent();
		DataContext = new ContentViewModel();
	}

	private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
	{
		var instanceId = ClientStorageService.ClientStorage.Container.FocusedInstance.Value;
		_ = ServerService.SendCommandAsync(new ClientCloseCommand(instanceId));
	}

}