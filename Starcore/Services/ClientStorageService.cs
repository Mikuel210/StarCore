using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SDK.Communication;

namespace StarCore.Services;

public static class ClientStorageService
{
	
	public static NetworkStorage<ClientContainer> ClientStorage { get; } = new();
	
	public static InstanceData? FocusedInstance => ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances
		.FirstOrDefault(e => e.InstanceId == ClientStorage.Container.FocusedInstance.Value);
	
	public static void Initialize() =>
		ClientStorage.ContainerChanged += async action => await SendContainerAction(action);

	public static void FocusOnInstance(InstanceData instance) =>
		ClientStorage.Container.FocusedInstance.Value = instance.InstanceId;

	public static void HandleContainerAction(ContainerAction action) => ClientStorage.HandleContainerAction(action);
	private static async Task SendContainerAction(ContainerAction action) =>
		await ServerService.SendContainerAction<ClientContainer>(action);

}