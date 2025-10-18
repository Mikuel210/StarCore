using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using SDK;
using SDK.Communication;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class SidebarViewModel : ViewModelBase
{

	public ObservableCollection<InstanceData> OpenSystems { get; } = [];
	public ObservableCollection<InstanceData> OpenProtocols { get; } = [];

	public SidebarViewModel()
	{
		ReplicatedStorageService.ReplicatedStorage.ContainerUpdated += Update;
		ClientStorageService.FocusedInstanceChanged += Update;
		
		Update();
	}

	private void Update()
	{
		// Schedule changes on the UI thread
		Dispatcher.UIThread.Post(() => {
			OpenSystems.Clear();
			OpenProtocols.Clear();
				
			ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
				.Where(e => e.ModuleType == Core.ModuleType.System)
				.ToList()
				.ForEach(OpenSystems.Add);
	
			ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
				.Where(e => e.ModuleType == Core.ModuleType.Protocol)
				.ToList()
				.ForEach(OpenProtocols.Add);
		});
	}
	
	public void FocusOnInstance(InstanceData instance) => ClientStorageService.FocusOnInstance(instance);
	
}