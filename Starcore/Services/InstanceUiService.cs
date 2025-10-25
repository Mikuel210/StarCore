using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls;
using SDK;
using SDK.Communication;
using StarCore.Controls;

namespace StarCore.Services;

public static class InstanceUiService
{
	
	public static UiPanel? Root { get; set; }
	
	public static UiControl CreateControl(UiElementData data)
	{
		var elementType = Type.GetType(data.ElementType);
		if (elementType == null) throw new InvalidOperationException("Invalid UI element data type");
		
		Output.Debug($"ELEMENT TYPE: {elementType.Name}");
		
		var controlType = GetTypeByName($"Ui{elementType.Name}");
		if (controlType == null) throw new NotImplementedException($"Control not implemented: Ui{elementType.Name}");
		

		var control = (UiControl)Activator.CreateInstance(controlType)!;
		control.ElementId = data.ElementId;

		// Apply properties
		foreach (var dataProperty in data.Properties) {
			Output.Info(dataProperty.Key);
			
			var controlProperty = control.GetType().GetProperty(dataProperty.Key);
			if (controlProperty == null || !controlProperty.CanWrite) continue;
			
			Output.Info(1);

			var value = dataProperty.Value;
			var valueType = controlProperty.PropertyType;
			
			if (value is JsonElement jsonElement) 
				value = jsonElement.Deserialize(valueType);

			if (value != null && !value.GetType().IsAssignableTo(valueType)) continue;
			
			Output.Info(2);
			Output.Info(value);
			Output.Info(value.GetType());
			
			controlProperty.SetValue(control, value);
			
			Output.Info(controlProperty.GetValue(control));
		}

		// Create children
		if (control is UiContainerControl && control.GetType().GetProperty("Children") is { } childrenProperty) {
			if (childrenProperty.PropertyType != typeof(ObservableCollection<Control>)) goto End;

			var instanceUi = ClientStorageService.ClientStorage.Container.FocusedInstanceUi.ToList();
			var controls = data.GetChildren(instanceUi).Select(CreateControl).ToList();
			
			var controlCollection = (ObservableCollection<Control>)childrenProperty.GetValue(control)!;
			controlCollection.Clear();
			controls.ForEach(controlCollection.Add);
		}

		End:
		return control;
	}
	
	private static Type? GetTypeByName(string name)
	{
		var assembly = Assembly.GetAssembly(typeof(UiControl));
		return assembly?.GetTypes().FirstOrDefault(e => e.Name == name);
	}

	public static UiControl? GetControl(Guid elementId)
	{
		List<UiControl> currentControls = [Root!];
		List<UiControl> nextControls = [];

		while (currentControls.Count > 0) {
			foreach (var control in currentControls) {
				if (control.ElementId == elementId) return control;
				if (control is not UiContainerControl container) continue;

				foreach (var child in container.Children)
					nextControls.Add(child);
			}

			currentControls = nextControls;
			nextControls = [];
		}

		return null;
	}

}