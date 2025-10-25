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

			// TODO: Subscribe to property changes of elements as well
			instance.Root.children.CollectionChanged += (_, e) => UpdateInstanceUi(instance, e);
		};

		Core.InstanceClosed += instance =>
			ReplicatedStorage.Container.OpenInstances
				.RemoveAt(ReplicatedStorage.Container.OpenInstances
					.Where(e => e.InstanceId == instance.InstanceId)
					.Select(ReplicatedStorage.Container.OpenInstances.IndexOf)
					.Single());

		ReplicatedStorage.ContainerChanged += action =>
			ConnectedClients.ForEach(e => e.SendContainerAction<ReplicatedContainer>(action));
	}
	
	private static void FetchInstanceUi(Client client)
	{
		var focusedInstanceUi = ClientStorage[client.ConnectionId].Container.FocusedInstanceUi;
		focusedInstanceUi.Clear();

		if (ClientStorage[client.ConnectionId].Container.FocusedInstance.Value is not { } instanceId) {
			Output.Warning("returning");
			return;
		}
		Output.Warning(instanceId);
		var instance = Instance.FromInstanceId(instanceId)!;
		
		// Recursively add data
		List<UiElement> currentElements = [instance.Root];
		List<UiElement> nextElements = [];

		while (currentElements.Count > 0) {
			foreach (var element in currentElements) {
				focusedInstanceUi.Add(UiElementData.FromUiElement(element));
				
				if (element is ContainerElement container)
					nextElements.AddRange(container.Children);
			}

			currentElements = nextElements;
			nextElements = [];
		}
		
		Output.Debug(string.Join(" | ", ClientStorage[client.ConnectionId].Container.FocusedInstanceUi));
	}

	private static void UpdateInstanceUi(Instance instance, NotifyCollectionChangedEventArgs args)
	{
		// TODO
		// This should update the instance UI efficiently for all clients focusing on it
		
		Output.Debug($"UI: {string.Join(", ", instance.Root)}");
						
		switch (args.Action) {
			case NotifyCollectionChangedAction.Add:
				// TODO: Update network list, subscribe to children changes of new elements
				// TODO: when the children change, subscribe to their property changes
				
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
						
			case NotifyCollectionChangedAction.Reset: default:
				// action = new ContainerResetAction(propertyName);
				break;
		}
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