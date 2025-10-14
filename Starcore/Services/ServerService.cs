using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class ServerService
{

	public static HubConnection? Connection { get; private set; }
	public static event Action? OnConnected;
	
	public static async Task ConnectAsync(string url)
	{
		// Disconnect
		if (Connection == null || Connection.State != HubConnectionState.Disconnected) await DisconnectAsync();
		
		// Create connection
		Connection = new HubConnectionBuilder()
			.WithUrl($"{url}/hub")
			.WithAutomaticReconnect()
			.Build();

		Connection.On<CommandEnvelope>("HandleCommand", HandleCommand);
		
		// Start connection
		await Connection.StartAsync();
		
		// Send connection request
		Type clientType = typeof(DesktopClient);
		if (OperatingSystem.IsBrowser()) clientType = typeof(BrowserClient);
		if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) clientType = typeof(MobileClient);

		await SendCommandAsync(new ClientConnectCommand(clientType.AssemblyQualifiedName!));
		
		// Invoke event
		OnConnected?.Invoke();
	}
	public static async Task DisconnectAsync()
	{
		if (Connection != null) 
			await Connection.StopAsync();	
	}
	
	private static void HandleCommand(CommandEnvelope envelope)
	{
		var command = ServerCommand.FromEnvelope(envelope);
		Output.Info($"Command received from server: {command}");
		
		switch (command) {
			case ServerGetInstancesCommand getOpenInstancesCommand:
				InstanceService.UpdateOpenInstances(getOpenInstancesCommand.InstanceData);
				break;
			
			default:
				Output.Error($"Command not implemented: {command.GetType().Name}");
				break;
		}
	}
	public static async Task SendCommandAsync(ClientCommand command)
	{
		if (Connection != null)
			await Connection.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));	
	}

}