using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SDK.Communication;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class ContentViewModel : ViewModelBase
{

	[ObservableProperty] private string _title = string.Empty;

	public ContentViewModel()
	{
		ReplicatedStorageService.ReplicatedStorage.ContainerUpdated += Update;
		ClientStorageService.ClientStorage.Container.FocusedInstances.NetworkCollectionChanged += (_, _) => Update();
		
		Update();
	}

	private void Update()
	{
		var focusedInstanceId = ClientStorageService.ClientStorage.Container.FocusedInstances.ElementAtOrDefault(0);

		var focusedInstance = ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
			.FirstOrDefault(e => e.InstanceId == focusedInstanceId);
		
		Title = focusedInstance?.Title ?? string.Empty;	
	}

}