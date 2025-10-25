using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using SDK;
using SDK.Communication;
using StarCore.Controls;

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
		
		ClientStorage.Container.FocusedInstanceUi.CollectionChanged += (sender, args) => 
			Dispatcher.UIThread.Post(() => HandleFocusedInstanceUiChanged(sender, args));
	}
		

	public static void FocusOnInstance(InstanceData instance) =>
		ClientStorage.Container.FocusedInstance.Value = instance.InstanceId;

	public static void HandleContainerAction(ContainerAction action) => ClientStorage.HandleContainerAction(action);
	private static async Task SendContainerAction(ContainerAction action) =>
		await ServerService.SendContainerAction<ClientContainer>(action);
	
	private static void HandleFocusedInstanceUiChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		// BUG: Child index is dismissed
		Output.Debug($"Handle UI change: {args.Action}");
		
		switch (args.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (var item in args.NewItems!) {
					var data = (UiElementData)item;
					Output.Debug($"ADD: {data}");

					if (data.ParentId is not { } parentId) {
						if (InstanceUiService.Root is not { } root) continue;
						root.ElementId = data.ElementId;
						Output.Debug($"NEW ROOT ID: {root.ElementId}");

						continue;
					}

					try {
						var control = InstanceUiService.CreateControl(data);
						var parent = (UiContainerControl)InstanceUiService.GetControl(parentId)!;
						parent.Children.Add(control);

						Output.Debug($"Parent: {parentId} | Children: {string.Join(", ", parent.Children.Select(e => e.ElementId))}");
					}
					catch (Exception e) {
						Output.Error(e);
					}
					
				}
				
				break;

			case NotifyCollectionChangedAction.Move:
				break;

			case NotifyCollectionChangedAction.Remove:
				var removedElement = (UiElementData)args.OldItems![0]!;
				var removedControl = InstanceUiService.GetControl(removedElement.ElementId)!;
				var parentControl = (UiContainerControl)InstanceUiService.GetControl((Guid)removedElement.ParentId!)!;
				parentControl.Children.Remove(removedControl);
				
				break;

			case NotifyCollectionChangedAction.Replace:
				// TODO
				// action = new ContainerReplaceAction(propertyName, args.OldStartingIndex, args.NewItems![0]);
				break;

			case NotifyCollectionChangedAction.Reset:
			default:
				InstanceUiService.Root?.Children.Clear();
				break;
		}
	}
	
}