using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SDK;
using SDK.Communication;
using StarCore.Services;
using StarCore.ViewModels;

namespace StarCore.Views;

public partial class SidebarView : UserControl
{

	public SidebarView()
	{
		InitializeComponent();
		DataContext = new SidebarViewModel();
	}

	private void InstanceButton_OnClick(object? sender, RoutedEventArgs e)
	{
		if (sender is not Button { DataContext: InstanceData instance }) return;
		ClientStorageService.FocusOnInstance(instance);
	}

	private void ProtocolButton_OnClick(object? sender, RoutedEventArgs e)
	{
		if (sender is not Button { DataContext: ModuleData module }) return;
		_ = ServerService.SendCommandAsync(new ClientOpenCommand(module.Module));
	}

}