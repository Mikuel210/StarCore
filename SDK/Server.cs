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