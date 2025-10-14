namespace SDK.Communication;

public record InstanceData(Guid InstanceId, Core.ModuleType ModuleType, string Title, string ModuleName, 
	string ModuleDescription, bool CanClientOpen, bool CanClientClose)
{

	public static InstanceData FromInstance(Instance instance)
	{
		return new(
			instance.InstanceId,
			Core.GetModuleType(instance),
			instance.Title,
			Core.GetModuleName(instance),
			Core.GetModuleDescription(instance),
			Core.CanClientOpen(instance),
			(instance as ProtocolInstance)?.CanClientClose ?? false
		);	
	}

}