using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using SDK;

namespace Server;

public class ServerHub : Hub
{
	
	public void HandleCommand(CommandEnvelope envelope)
	{
		var command = ClientCommand.FromEnvelope(envelope);
		Output.Info($"Command received from client: {command}");

		switch (command) {
			case ClientConnectCommand connectCommand:
				var clientType = Type.GetType(connectCommand.ClientType);
				if (clientType is null) throw new InvalidOperationException("Invalid client type");
				
				var instance = Activator.CreateInstance(clientType, Context.ConnectionId);
				if (instance is not Client client) throw new InvalidOperationException("Invalid client type");
				
				SDK.Server.RegisterClient(client);
				
				break;
			
			default:
				Output.Error($"Command not implemented: {command.GetType().Name}");
				break;
		}
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		// Unregister client on the server
		SDK.Server.UnregisterClient(Context.ConnectionId);

		return Task.CompletedTask;
	}

}