namespace SDK.Communication;

public record InstanceData(Core.ModuleType ModuleType, string Title, string ModuleName, string ModuleDescription, bool CanClientOpen)
{

	public static InstanceData FromInstance(Instance instance) =>
		new(
			Core.GetModuleType(instance),
			instance.Title,
			Core.GetModuleName(instance),
			Core.GetModuleDescription(instance),
			Core.CanClientOpen(instance)
		);

}