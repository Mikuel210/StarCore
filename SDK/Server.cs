using System.Collections.Specialized;
using System.Text.Json;
using SDK.Communication;
using SDK.Helpers;

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

			instance.InstanceUi.CollectionChanged += (_, e) => UpdateInstanceUi(instance, e);
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
	
	private static void FetchInstanceUi(Instance instance)
	{
		// TODO: This needs a rework
		// This fetches for all clients which makes no sense
		// When an instance opens no client will have it open
		// Clients should request it
		// Or we could listen for changes to clients' focused instance
	}

	private static void UpdateInstanceUi(Instance instance, NotifyCollectionChangedEventArgs args)
	{
		Output.Debug($"UI: {string.Join(", ", instance.InstanceUi)}");
						
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
		ClientStorage.Add(client.ConnectionId, new());
	}
	public static void UnregisterClient(string connectionId)
	{
		Output.Info($"Client disconnected: {connectionId}");
		ConnectedClients.RemoveAll(e => e.ConnectionId == connectionId);
		
		// Remove client storage
		ClientStorage.Remove(connectionId);
	}

}