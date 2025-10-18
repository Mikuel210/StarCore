using Microsoft.AspNetCore.SignalR;
using SDK;
using SDK.Communication;

namespace Server;

public class ServerHub : Hub
{

	private ISingleClientProxy Proxy => Clients.Client(Context.ConnectionId);
	private Client Client => Client.FromConnectionId(Context.ConnectionId)!;
	
	public override Task OnDisconnectedAsync(Exception? exception)
	{
		// Unregister client on the server
		SDK.Server.UnregisterClient(Context.ConnectionId);

		return Task.CompletedTask;
	}
	
	public void HandleCommand(CommandEnvelope envelope)
	{
		var command = ClientCommand.FromEnvelope(envelope);
		Output.Info($"Command received from client: {command}");

		switch (command) {
			case ClientConnectCommand connectCommand:
				var clientType = Type.GetType(connectCommand.ClientType);
				if (clientType is null) throw new InvalidOperationException("Invalid client type");
				
				var clientInstance = Activator.CreateInstance(clientType, Context.ConnectionId, Proxy);
				if (clientInstance is not Client client) throw new InvalidOperationException("Invalid client type");
				
				SDK.Server.RegisterClient(client);
				break;
			
			case ClientOpenCommand openCommand:
				var type = Core.Modules.FirstOrDefault(e => e.AssemblyQualifiedName == openCommand.Module);
				if (type == null) break; 
				
				var openedProtocol = Core.Open(type);
				SDK.Server.ClientStorage[Client.ConnectionId].Container.FocusedInstance.Value = openedProtocol.InstanceId;
				
				break;
			
			case ClientCloseCommand closeCommand:
				var protocolToClose = Instance.FromInstanceId(closeCommand.InstanceId);
				if (protocolToClose != null) Core.Close(protocolToClose);

				break;
			
			default:
				Output.Error($"Command not implemented: {command.GetType().Name}");
				break;
		}
	}
	private void SendCommand(ServerCommand command) =>
		Proxy.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));

	public void HandleContainerAction(ContainerActionEnvelope envelope)
	{
		var client = Client.FromConnectionId(Context.ConnectionId)!;
		var containerType = Type.GetType(envelope.ContainerType);
		var action = ContainerAction.FromEnvelope(envelope);
		
		if (containerType == typeof(ReplicatedContainer))
			SDK.Server.ReplicatedStorage.HandleContainerAction(action);
		if (containerType == typeof(ClientContainer))
			SDK.Server.ClientStorage[client.ConnectionId].HandleContainerAction(action);
	}
	
}