using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class ServerService
{

	public static NetworkStorage<ReplicatedContainer> ReplicatedStorage { get; } = new(new());
	
	public static HubConnection? Connection { get; private set; }
	public static event Action? OnConnected;

	static ServerService()
	{
		// Set up replicated storage
		ReplicatedStorage.ContainerChanged += async action => await SendContainerAction(action);

		ReplicatedStorage.Container.OpenInstances.CollectionChanged += (_, _) =>
			Output.Info(string.Join(", ", ReplicatedStorage.Container.OpenInstances));
	}
	
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
		Connection.On<ContainerActionEnvelope>("HandleContainerAction", HandleContainerAction);
		
		// Start connection
		await Connection.StartAsync();
		
		// Send connection request
		Type clientType = typeof(DesktopClient);
		if (OperatingSystem.IsBrowser()) clientType = typeof(BrowserClient);
		if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) clientType = typeof(MobileClient);

		await SendCommandAsync(new ClientConnectCommand(clientType.AssemblyQualifiedName!));
		
		// Fetch storage
		ReplicatedStorage.Fetch();
		
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

	
	private static void HandleContainerAction(ContainerActionEnvelope envelope)
	{
		// TODO: Recognize container type
        ReplicatedStorage.HandleContainerAction(ContainerAction.FromEnvelope(envelope));
		
		Output.Info($"RECEIVED ACTION: {envelope.ActionType}, {ContainerAction.FromEnvelope(envelope)}");
		
		Output.Info(string.Join(", ", ReplicatedStorage.Container.OpenInstances));
		Output.Info(ReplicatedStorage.Container.ReplicatedString.Value);
	}
	private static async Task SendContainerAction(ContainerAction action)
	{
		if (Connection != null)
			await Connection.SendAsync("HandleContainerAction", ContainerActionEnvelope.FromAction(action));
		else {
			Output.Info("DISREGARD:");	
		}
		
		Output.Info($"SENT ACTION: {action}");
	}

}