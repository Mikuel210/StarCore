using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
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
		
		switch (args.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (var item in args.NewItems!) {
					var data = (UiElementData)item;

					if (data.ParentId is not { } parentId) {
						if (InstanceUiService.Root is not { } root) continue;
						root.ElementId = data.ElementId;

						continue;
					}
					
					var control = InstanceUiService.CreateControl(data);
					var parent = (UiContainerControl)InstanceUiService.GetControl(parentId)!;
					parent.Children.Add(control);
				}
				
				break;

			case NotifyCollectionChangedAction.Move:
				break;

			case NotifyCollectionChangedAction.Remove:
				var removedData = (UiElementData)args.OldItems![0]!;
				var removedControl = InstanceUiService.GetControl(removedData.ElementId)!;
				var parentControl = (UiContainerControl)InstanceUiService.GetControl((Guid)removedData.ParentId!)!;
				parentControl.Children.Remove(removedControl);
				
				break;

			case NotifyCollectionChangedAction.Replace:
				// TODO: Two possibilities, the thing got replaced or its properties were changed

				var oldData = (UiElementData)args.OldItems![0]!;
				var newData = (UiElementData)args.NewItems![0]!;

				if (oldData.ElementId == newData.ElementId) {
					var control = InstanceUiService.GetControl(oldData.ElementId)!;

					if (oldData.ParentId != newData.ParentId) {
						if (oldData.ParentId is { } oldParentId) {
							var oldParent = (UiContainerControl)InstanceUiService.GetControl(oldParentId)!;
							oldParent.Children.Remove(control);
						}

						if (newData.ParentId is { } newParentId) {
							var newParent = (UiContainerControl)InstanceUiService.GetControl(newParentId)!;
							newParent.Children.Add(control);
						}
					}

					if (oldData.Properties == newData.Properties) break;

					foreach (var property in newData.Properties) {
						var propertyInfo = control.GetType().GetProperty(property.Key)!;
						if (!propertyInfo.CanWrite) continue;

						var value = property.Value;
						var propertyType = propertyInfo.PropertyType;

						if (value is JsonElement jsonElement)
							value = jsonElement.Deserialize(propertyType, Server.JsonSerializerOptions);
						
						propertyInfo.SetValue(control, value);
					}
				}
				
				// TODO: Else...
				
				// action = new ContainerReplaceAction(propertyName, args.OldStartingIndex, args.NewItems![0]);
				break;

			case NotifyCollectionChangedAction.Reset:
			default:
				InstanceUiService.Root?.Children.Clear();
				break;
		}
	}
	
}