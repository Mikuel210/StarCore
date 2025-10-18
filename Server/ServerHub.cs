using Microsoft.AspNetCore.SignalR;
using SDK;
using SDK.Communication;

namespace Server;

public class ServerHub : Hub
{

	private ISingleClientProxy Proxy => Clients.Client(Context.ConnectionId); 
	
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
				
				var instance = Activator.CreateInstance(clientType, Context.ConnectionId, Proxy);
				if (instance is not Client client) throw new InvalidOperationException("Invalid client type");
				
				SDK.Server.RegisterClient(client);
				break;
			
			default:
				Output.Error($"Command not implemented: {command.GetType().Name}");
				break;
		}
	}
	private void SendCommand(ServerCommand command) =>
		Proxy.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));

	public void HandleContainerAction(ContainerActionEnvelope envelope) => 
		SDK.Server.HandleContainerAction(
			Client.FromConnectionId(Context.ConnectionId)!,
			Type.GetType(envelope.ContainerType)!,
			ContainerAction.FromEnvelope(envelope)
		);
	
}