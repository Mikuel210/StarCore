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
		InstanceService.OnOpenInstancesUpdated += Update;
		Update();
	}

	private void Update()
	{
		// Schedule changes on the UI thread
		Dispatcher.UIThread.Post(() => {
			OpenSystems.Clear();
			OpenProtocols.Clear();
				
			InstanceService.OpenInstances
				.Where(e => e.ModuleType == Core.ModuleType.System)
				.ToList()
				.ForEach(OpenSystems.Add);
	
			InstanceService.OpenInstances
				.Where(e => e.ModuleType == Core.ModuleType.Protocol)
				.ToList()
				.ForEach(OpenProtocols.Add);
		});
	}

}