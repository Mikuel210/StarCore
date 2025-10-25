using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SDK;
using SDK.Communication;
using StarCore.Controls;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class ContentViewModel : ViewModelBase
{

	[ObservableProperty] private bool _canClose;
	[ObservableProperty] private string _title = string.Empty;

	public ContentViewModel()
	{
		ReplicatedStorageService.ReplicatedStorage.ContainerUpdated += Update;
		ClientStorageService.ClientStorage.Container.FocusedInstance.ValueChanged += _ => Update();
		ClientStorageService.ClientStorage.Container.FocusedInstance.ValueUpdated += _ => Update();
		
		Update();
	}

	private void Update()
	{
		CanClose = ClientStorageService.FocusedInstance?.CanClientClose ?? false;
		Title = ClientStorageService.FocusedInstance?.Title ?? string.Empty;
		
		Output.Debug(Title);

		if (!CanClose) ButtonFlyout.Hide("CloseFlyout");
	}

}