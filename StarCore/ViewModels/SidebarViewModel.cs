using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using SDK;
using SDK.Communication;
using SDK.Helpers;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class SidebarViewModel : ViewModelBase
{

	public ObservableCollection<ModuleData> OpenableModules { get; } = [];
	public ObservableCollection<InstanceData> OpenSystems { get; } = [];
	public ObservableCollection<InstanceData> OpenProtocols { get; } = [];

	public SidebarViewModel()
	{
		ClientStorageService.ClientStorage.Container.FocusedInstance.ValueChanged += _ => UpdateInstances();
		ReplicatedStorageService.ReplicatedStorage.ContainerUpdated += UpdateInstances;

		ReplicatedStorageService.PostReceived += () => {
			UpdateModules();
			UpdateInstances();
		};
		
		UpdateModules();
		UpdateInstances();
	}

	public void UpdateModules()
	{
		// Schedule changes on the UI thread
		Dispatcher.UIThread.Post(() => {
			OpenableModules.Clear();

			ReplicatedStorageService.ReplicatedStorage.Container.Modules
				.Where(e => e.ModuleType == Core.ModuleType.Protocol && e.CanClientOpen)
				.ToList()
				.ForEach(OpenableModules.Add);
		});
	}
	private void UpdateInstances()
	{
		// Schedule changes on the UI thread
		Dispatcher.UIThread.Post(() => {
			OpenSystems.Clear();
			OpenProtocols.Clear();
				
			GetOpenInstancesFromModuleType(Core.ModuleType.System).ForEach(OpenSystems.Add);
			GetOpenInstancesFromModuleType(Core.ModuleType.Protocol).ForEach(OpenProtocols.Add);
		});
	}
	private List<InstanceData> GetOpenInstancesFromModuleType(Core.ModuleType moduleType)
	{
		var modules = ReplicatedStorageService.ReplicatedStorage.Container.Modules
			.Where(e => e.ModuleType == moduleType)
			.Select(e => e.Module);

		return ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
			.Where(e => modules.Contains(e.Module))
			.ToListSafe();
	}
	
}