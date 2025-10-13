using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SDK;

namespace StarCore.Core;

public class ServerService
{

	private readonly HubConnection _connection;

	public ServerService(string url)
	{
		// Create connection
		_connection = new HubConnectionBuilder()
			.WithUrl($"{url}/hub")
			.WithAutomaticReconnect()
			.Build();

		_connection.On<CommandEnvelope>("HandleCommand", HandleCommand);
	}

	public async Task ConnectAsync()
	{
		// Start connection
		await _connection.StartAsync();
		
		// Send connection request
		Type clientType = typeof(DesktopClient);
		if (OperatingSystem.IsBrowser()) clientType = typeof(BrowserClient);
		if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) clientType = typeof(MobileClient);

		await SendCommandAsync(new ClientConnectCommand(clientType.AssemblyQualifiedName!));
	}

	public async Task DisconnectAsync() => await _connection.StopAsync();
	
	private void HandleCommand(CommandEnvelope envelope)
	{
		var command = ServerCommand.FromEnvelope(envelope);
		Output.Info($"Command received from server: {command}");
		
		switch (command) {
			default:
				Output.Error($"Command not implemented: {command.GetType().Name}");
				break;
		}
	}
	public async Task SendCommandAsync(ClientCommand command) => 
		await _connection.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));

}