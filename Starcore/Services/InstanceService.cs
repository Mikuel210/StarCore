using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SDK;
using SDK.Communication;

namespace StarCore.Services;

public static class InstanceService
{
	
	public static List<InstanceData> OpenInstances { get; private set; } = new();
	public static event Action? OpenInstancesUpdated;
	
	public static Guid? FocusedInstanceId { get; private set; }
	public static event Action? FocusedInstanceUpdated;
	
	public static InstanceData? FocusedInstance => 
		OpenInstances.FirstOrDefault(e => e.InstanceId == FocusedInstanceId);

	public static void Initialize()
	{
		ServerService.OnConnected += async () => 
			await ServerService.SendCommandAsync(new ClientGetOpenInstancesCommand());
	}

	public static void UpdateOpenInstances(InstanceData[] instances)
	{
		OpenInstances = new(instances);
		OpenInstancesUpdated?.Invoke();
		FocusedInstanceUpdated?.Invoke();
	}
	public static void FocusOnInstance(InstanceData instance)
	{
		FocusedInstanceId = instance.InstanceId;
		FocusedInstanceUpdated?.Invoke();
	}

}