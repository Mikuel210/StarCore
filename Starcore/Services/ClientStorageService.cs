using System;
using System.Linq;
using System.Threading.Tasks;
using SDK.Communication;

namespace StarCore.Services;

public static class ClientStorageService
{
	
	public static NetworkStorage<ClientContainer> ClientStorage { get; } = new();
	
	
	public static void Initialize() =>
		ClientStorage.ContainerChanged += async action => await SendContainerAction(action);
	
	public static void FocusOnInstance(InstanceData instance)
	{
		// TODO: Have multiple focused instances
		if (ClientStorage.Container.FocusedInstances.Count > 0)
			ClientStorage.Container.FocusedInstances[0] = instance.InstanceId;
		else
			ClientStorage.Container.FocusedInstances.Add(instance.InstanceId);
	}

	public static void HandleContainerAction(ContainerAction action) => ClientStorage.HandleContainerAction(action);
	private static async Task SendContainerAction(ContainerAction action) =>
		await ServerService.SendContainerAction<ClientContainer>(action);

}