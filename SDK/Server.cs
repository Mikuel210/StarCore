using SDK.Communication;

namespace SDK;

public static class Server
{

	public static List<Client> ConnectedClients { get; } = new();

	static Server()
	{
		// Notify on open instances changed
		Core.OpenInstancesChanged += () => {
			var instanceData = Core.OpenInstances.Select(InstanceData.FromInstance).ToArray();
			
			foreach (var client in ConnectedClients)
				client.SendCommand(new ServerGetOpenInstancesCommand(instanceData));
		};
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

}