using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class ReplicatedStorageService
{
	
	public static NetworkStorage<ReplicatedContainer> ReplicatedStorage { get; } = new();

	public static void Initialize()
	{
		ReplicatedStorage.ContainerChanged += async action => await SendContainerAction(action);
		ServerService.OnConnected += ReplicatedStorage.Fetch;	
	}

	public static void HandleContainerAction(ContainerAction action) => ReplicatedStorage.HandleContainerAction(action);
	private static async Task SendContainerAction(ContainerAction action) =>
		await ServerService.SendContainerAction<ReplicatedContainer>(action);

}