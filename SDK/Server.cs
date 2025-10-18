using System.Text.Json;
using SDK.Communication;

namespace SDK;

public static class Server
{

	public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { PropertyNameCaseInsensitive = true };

	internal static NetworkStorage<ReplicatedContainer> ReplicatedStorage { get; } = new(new() {
		OpenInstances = [],
		ReplicatedString = new("Replicated string")
	});
	
	public static List<Client> ConnectedClients { get; } = [];

	internal static void Initialize()
	{
		Core.InstanceOpened += instance => {
			ReplicatedStorage.Container.OpenInstances.Add(InstanceData.FromInstance(instance));

			// Listen for changes
			instance.PropertyChanged += (_, _) => {
				var index = ReplicatedStorage.Container.OpenInstances
					.IndexOf(ReplicatedStorage.Container.OpenInstances
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
			ConnectedClients.ForEach(e => e.SendContainerAction(action));
	}
	
	public static void RegisterClient(Client client)
	{
		Output.Info($"Client connected: {client.ConnectionId}");
		ConnectedClients.Add(client);
	}
	public static void UnregisterClient(string connectionId)
	{
		Output.Info($"Client disconnected: {connectionId}");
		ConnectedClients.RemoveAll(e => e.ConnectionId == connectionId);
	}
	
	public static void HandleContainerAction(ContainerAction action) =>
		ReplicatedStorage.HandleContainerAction(action);

}