using Avalonia.Controls;
using Avalonia.Interactivity;
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

		InstanceUiService.Root = InstanceUi;
	}

	private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
	{
		var instanceId = ClientStorageService.ClientStorage.Container.FocusedInstance.Value;
		if (instanceId is { } id) _ = ServerService.SendCommandAsync(new ClientCloseCommand(id));
	}

}