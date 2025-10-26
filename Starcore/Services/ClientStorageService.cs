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
					AddControlFromData(data);
				}
				
				break;

			case NotifyCollectionChangedAction.Move:
				break;

			case NotifyCollectionChangedAction.Remove:
				var removedData = (UiElementData)args.OldItems![0]!;
				RemoveControlFromData(removedData);
				
				break;

			case NotifyCollectionChangedAction.Replace:
				var oldData = (UiElementData)args.OldItems![0]!;
				var newData = (UiElementData)args.NewItems![0]!;

				// Element was replaced
				if (oldData.ElementId != newData.ElementId) {
					RemoveControlFromData(oldData);
					AddControlFromData(newData);

					return;
				}
				
				// Properties were updated
				if (oldData.Properties == newData.Properties) break;
				var control = InstanceUiService.GetControl(oldData.ElementId)!;

				foreach (var property in newData.Properties) {
					var propertyInfo = control.GetType().GetProperty(property.Key)!;
					if (!propertyInfo.CanWrite) continue;

					var value = property.Value;
					var propertyType = propertyInfo.PropertyType;

					if (value is JsonElement jsonElement)
						value = jsonElement.Deserialize(propertyType, Server.JsonSerializerOptions);
					
					propertyInfo.SetValue(control, value);
				}
				
				break;

			case NotifyCollectionChangedAction.Reset:
			default:
				InstanceUiService.Root?.Children.Clear();
				break;
		}
	}

	private static void AddControlFromData(UiElementData data)
	{
		if (data.ParentId is not { } parentId) {
			if (InstanceUiService.Root is not { } root) return;
			root.ElementId = data.ElementId;

			return;
		}
					
		var control = InstanceUiService.CreateControl(data);
		var parent = (UiContainerControl)InstanceUiService.GetControl(parentId)!;
		parent.Children.Add(control);
	}

	private static void RemoveControlFromData(UiElementData data)
	{
		var removedControl = InstanceUiService.GetControl(data.ElementId)!;
		var parentControl = (UiContainerControl)InstanceUiService.GetControl((Guid)data.ParentId!)!;
		parentControl.Children.Remove(removedControl);
	}
	
}