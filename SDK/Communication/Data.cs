using SDK.Instances;

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

public record UiElementData(Guid ElementId, Guid? ParentId, string ElementType, Dictionary<string, object?> Properties)
{

	public static UiElementData FromUiElement(UiElement uiElement)
	{
		var properties = new Dictionary<string, object?>();
		var elementType = uiElement.GetType();

		foreach (var property in elementType.GetProperties()) {
			var name = property.Name;
			if (uiElement is ContainerElement && name == "Children") continue;
			
			var value = property.GetValue(uiElement);
			properties.Add(name, value);
		}	
		
		return new(uiElement.ElementId, uiElement.Parent?.ElementId, elementType.AssemblyQualifiedName!, properties);
	}

	public List<UiElementData> GetChildren(List<UiElementData> elements) => 
		elements.Where(e => e.ParentId == ElementId).ToList();
		
}