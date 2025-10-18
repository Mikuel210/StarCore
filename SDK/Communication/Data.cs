namespace SDK.Communication;

public record InstanceData(string Module, Guid InstanceId, string Title, bool CanClientClose)
{

	public static InstanceData FromInstance(Instance instance)
	{
		return new(
			instance.GetType().AssemblyQualifiedName!,
			instance.InstanceId,
			instance.Title,
			(instance as ProtocolInstance)?.CanClientClose ?? false
		);	
	}

}

public record ModuleData(string Module, Core.ModuleType ModuleType, string ModuleName, string ModuleDescription, 
	bool ShowOnClient, bool CanClientOpen, bool NotifyOnOpen)
{

	public static ModuleData FromModule(Type module)
	{
		return new(
			module.AssemblyQualifiedName!,
			Core.GetModuleType(module),
			Core.GetModuleName(module),
			Core.GetModuleDescription(module),
			Core.GetShowOnClient(module),
			Core.CanClientOpen(module),
			Core.GetNotifyOnOpen(module)
		);	
	}
	
}