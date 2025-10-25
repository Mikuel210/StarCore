using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class ClientStorageService
{
	
	public static NetworkStorage<ClientContainer> ClientStorage { get; } = new();
	
	public static InstanceData? FocusedInstance => ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
		.FirstOrDefault(e => e.InstanceId == ClientStorage.Container.FocusedInstance.Value);

	public static void Initialize()
	{
		ClientStorage.ContainerChanged += async action => await SendContainerAction(action);
		ClientStorage.ContainerChanged += action => Output.Debug($"Action: {(action as ContainerPropertyUpdate)?.PropertyName}");
		ClientStorage.Container.FocusedInstanceUi.CollectionChanged += HandleFocusedInstanceUiChanged;
	}
		

	public static void FocusOnInstance(InstanceData instance) =>
		ClientStorage.Container.FocusedInstance.Value = instance.InstanceId;

	public static void HandleContainerAction(ContainerAction action) => ClientStorage.HandleContainerAction(action);
	private static async Task SendContainerAction(ContainerAction action) =>
		await ServerService.SendContainerAction<ClientContainer>(action);
	
	private static void HandleFocusedInstanceUiChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		Output.Debug($"Im here btw:: {args.Action}");
		
		switch (args.Action) {
			case NotifyCollectionChangedAction.Add:
				// action = new ContainerAddAction(propertyName, args.NewStartingIndex, args.NewItems!);
				break;

			case NotifyCollectionChangedAction.Move:
				// action = new ContainerMoveAction(propertyName, args.OldStartingIndex, args.NewStartingIndex);
				break;

			case NotifyCollectionChangedAction.Remove:
				// action = new ContainerRemoveAction(propertyName, args.OldStartingIndex);
				break;

			case NotifyCollectionChangedAction.Replace:
				// action = new ContainerReplaceAction(propertyName, args.OldStartingIndex, args.NewItems![0]);
				break;

			case NotifyCollectionChangedAction.Reset:
			default:
				// action = new ContainerResetAction(propertyName);
				break;
		}
	}
	
}