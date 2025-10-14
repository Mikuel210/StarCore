using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
		if (sender is Button { DataContext: InstanceData instance })
			InstanceService.FocusOnInstance(instance);
	}

}