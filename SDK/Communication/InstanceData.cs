namespace SDK.Communication;

public record InstanceData(string Title, string ModuleName, string ModuleDescription, bool CanClientOpen)
{

	public static InstanceData FromInstance(Instance instance) =>
		new(
			instance.Title,
			Core.GetModuleName(instance),
			Core.GetModuleDescription(instance),
			Core.CanClientOpen(instance)
		);

}

public record InstanceDataArray
{

	

}

public record InstanceArrayEnvelope(InstanceData[] InstanceEnvelopes)
{

	public static InstanceArrayEnvelope FromInstances(Instance[] instances) =>
		new(instances.Select(InstanceData.FromInstance).ToArray());
	
	public Instan

}