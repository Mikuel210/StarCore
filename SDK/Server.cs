using System.Collections.Specialized;
using System.Text.Json;
using SDK.Communication;
using SDK.Helpers;
using SDK.Instances;

namespace SDK;

public static class Server
{

	public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { PropertyNameCaseInsensitive = true };

	public static NetworkStorage<ReplicatedContainer> ReplicatedStorage { get; } = new();
	public static Dictionary<string, NetworkStorage<ClientContainer>> ClientStorage { get; } = new();
	private static List<Guid> _subscribedElements = [];
	public static List<Client> ConnectedClients { get; } = [];
	
	internal static void Initialize()
	{
		Core.ModulesLoaded += () =>
			Core.Modules.ForEach(e => ReplicatedStorage.Container.Modules.Add(ModuleData.FromModule(e)));
		
		Core.InstanceOpened += instance => {
			ReplicatedStorage.Container.OpenInstances.Add(InstanceData.FromInstance(instance));

			// Listen for changes
			instance.PropertyChanged += (_, _) => {
				var index = ReplicatedStorage.Container.OpenInstances
					.IndexOf(ReplicatedStorage.Container.OpenInstances
						.ToListSafe()
						.First(e => e.InstanceId == instance.InstanceId));
				
				ReplicatedStorage.Container.OpenInstances[index] = InstanceData.FromInstance(instance);
			};
		};

		Core.InstanceClosed += instance => {
			ReplicatedStorage.Container.OpenInstances
				.RemoveAt(ReplicatedStorage.Container.OpenInstances
					.Where(e => e.InstanceId == instance.InstanceId)
					.Select(ReplicatedStorage.Container.OpenInstances.IndexOf)
					.Single());
		};

		ReplicatedStorage.ContainerChanged += action =>
			ConnectedClients.ForEach(e => e.SendContainerAction<ReplicatedContainer>(action));
	}
	
	private static void FetchInstanceUi(Client client)
	{
		var focusedInstanceUi = ClientStorage[client.ConnectionId].Container.FocusedInstanceUi;
		focusedInstanceUi.Clear();

		if (ClientStorage[client.ConnectionId].Container.FocusedInstance.Value is not { } instanceId) return;
		var instance = Instance.FromInstanceId(instanceId)!;
		
		// Recursively add data
		List<UiElement> currentElements = [instance.Root];
		List<UiElement> nextElements = [];

		while (currentElements.Count > 0) {
			foreach (var element in currentElements) {
				AddUiElement(focusedInstanceUi, instance, element);
				
				if (element is ContainerElement container)
					nextElements.AddRange(container.Children);
			}

			currentElements = nextElements;
			nextElements = [];
		}
	}

	private static void UpdateInstanceUi(Instance instance, NotifyCollectionChangedEventArgs args)
	{
		// Get clients
		List<string> connectionIds = ClientStorage
			.Where(e => e.Value.Container.FocusedInstance.Value == instance.InstanceId)
			.Select(e => e.Key).ToList();

		foreach (var connectionId in connectionIds) {
			var instanceUi = ClientStorage[connectionId].Container.FocusedInstanceUi;
			
			switch (args.Action) {
				case NotifyCollectionChangedAction.Add:
					// TODO: Update network list, subscribe to children changes of new elements
					// TODO: when the children change, subscribe to their property changes
					// TODO: I should also have in account child index somehow

					foreach (var newElement in args.NewItems!)
						AddUiElement(instanceUi, instance, (UiElement)newElement);
					
					// action = new ContainerAddAction(propertyName, args.NewStartingIndex, args.NewItems!);
					break;
						
				case NotifyCollectionChangedAction.Move:
					// action = new ContainerMoveAction(propertyName, args.OldStartingIndex, args.NewStartingIndex);
					break;
						
				case NotifyCollectionChangedAction.Remove:
					var removedElement = (UiElement)args.OldItems![0]!;
					RemoveUiElement(instanceUi, removedElement);
					
					break;
						
				case NotifyCollectionChangedAction.Replace:
					// action = new ContainerReplaceAction(propertyName, args.OldStartingIndex, args.NewItems![0]);
					break;
						
				case NotifyCollectionChangedAction.Reset: default:
					// action = new ContainerResetAction(propertyName);
					break;
			}	
		}
	}

	private static void AddUiElement(NetworkCollection<UiElementData> instanceUi, Instance instance, UiElement element)
	{
		instanceUi.Add(UiElementData.FromUiElement(element));
		
		// Subscribe to updates
		if (_subscribedElements.Contains(element.ElementId)) return;
		_subscribedElements.Add(element.ElementId);
		
		if (element is ContainerElement container)
			container.children.CollectionChanged += (_, e) => UpdateInstanceUi(instance, e);
		
		element.PropertyChanged += (_, args) => {
			List<string> connectionIds = ClientStorage
				.Where(e => e.Value.Container.FocusedInstance.Value == instance.InstanceId)
				.Select(e => e.Key).ToList();

			if (connectionIds.Count == 0) return;
			
			var name = args.PropertyName!;
			if (name == nameof(UiElement.Parent)) return;
			
			var value = element.GetType().GetProperty(name)!.GetValue(element); 

			foreach (var connectionId in connectionIds) {
				var instanceUi = ClientStorage[connectionId].Container.FocusedInstanceUi;
				var data = instanceUi.First(e => e.ElementId == element.ElementId);
				var dataIndex = instanceUi.IndexOf(data);
				
				var properties = data.Properties;
				properties[name] = value;
					
				instanceUi[dataIndex] = data with { Properties = properties };
			}
		};
	}

	private static void RemoveUiElement(NetworkCollection<UiElementData> instanceUi, UiElement element)
	{
		var removedElementId = element.ElementId;
		var data = instanceUi.Where(e => e.ElementId == removedElementId);
		
		data.ToList().ForEach(e => instanceUi.Remove(e));
		element.Dispose();
		_subscribedElements.Remove(element.ElementId);
	}
	
	public static void RegisterClient(Client client)
	{
		Output.Info($"Client connected: {client.ConnectionId}");
		ConnectedClients.Add(client);
		
		// Create client storage
		var storage = new NetworkStorage<ClientContainer>();
		ClientStorage.Add(client.ConnectionId, storage);
		
		storage.ContainerChanged += action =>
			ConnectedClients.ForEach(e => e.SendContainerAction<ClientContainer>(action));
		
		// Subscribe to focused instance changed
		storage.ContainerChanged += action => {
			if (action is ContainerSetAction { PropertyName: nameof(ClientContainer.FocusedInstance) })
				FetchInstanceUi(client);
		};
		
		storage.ContainerUpdated += () => FetchInstanceUi(client);
	}
	public static void UnregisterClient(string connectionId)
	{
		Output.Info($"Client disconnected: {connectionId}");
		ConnectedClients.RemoveAll(e => e.ConnectionId == connectionId);
		
		// Remove client storage and unsubscribe from events
		ClientStorage[connectionId].Dispose();
		ClientStorage.Remove(connectionId);
	}

}