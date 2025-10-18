using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class ReplicatedStorageService
{
	
	public static NetworkStorage<ReplicatedContainer> ReplicatedStorage { get; } = new(new());

	public static void Initialize()
	{
		ReplicatedStorage.ContainerChanged += async action => await SendContainerAction(action);
		ServerService.OnConnected += ReplicatedStorage.Fetch;	
	}

	public static void HandleContainerAction(ContainerAction action)
	{
		ReplicatedStorage.HandleContainerAction(action);
		
		// Debug
		Output.Debug($"RECEIVED ACTION: {action}");
		Output.Debug(string.Join(", ", ReplicatedStorage.Container.OpenInstances));
		Output.Debug(ReplicatedStorage.Container.ReplicatedString.Value);
	}
	private static async Task SendContainerAction(ContainerAction action)
	{
		if (ServerService.Connection != null)
			await ServerService.Connection.SendAsync("HandleContainerAction", ContainerActionEnvelope.FromAction(action));
	}

}