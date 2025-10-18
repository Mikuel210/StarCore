using System;
using System.Linq;
using SDK.Communication;

namespace StarCore.Services;

public static class ClientStorageService
{

	private static Guid FocusedInstanceId;
	public static InstanceData? FocusedInstance =>
		ReplicatedStorageService.ReplicatedStorage.Container.OpenInstances.FirstOrDefault(e => e.InstanceId == FocusedInstanceId);

	public static event Action? FocusedInstanceChanged;

	public static void FocusOnInstance(InstanceData instance)
	{
		FocusedInstanceId = instance.InstanceId;
		FocusedInstanceChanged?.Invoke();
	}

}