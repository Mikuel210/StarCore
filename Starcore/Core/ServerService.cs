using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Server;

namespace StarCore.Core;

public class HubService
{

	private string _url;
	private HubConnection _connection;

	public HubService(string url)
	{
		_url = url;
		
		_connection = new HubConnectionBuilder()
			.WithUrl($"{_url}/hub")
			.WithAutomaticReconnect()
			.Build();

		_connection.On<Command>("ServerToClient", HandleCommand);
	}
	
	public async Task ConnectAsync() => await _connection.StartAsync();

	public async Task DisconnectAsync() => await _connection.StopAsync();

	private void HandleCommand(Command command) => Console.WriteLine($"Command received: {command.Message}");
	public async void SendCommandAsync(Command command) => await _connection.SendAsync("ClientToServer", command);

}