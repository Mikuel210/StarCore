using SDK.Communication;

namespace SDK;

public static class Server
{

	internal static ReplicatedStorage ReplicatedStorage { get; } = new();
	public static List<Client> ConnectedClients { get; } = [];
	
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