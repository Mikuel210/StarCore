using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls;
using SDK.Communication;

namespace StarCore.Services;

public static class InstanceUiService
{
	
	public static Control CreateControl(UiElementData data)
	{
		var elementType = Type.GetType(data.ElementType);
		if (elementType == null) throw new InvalidOperationException("Invalid UI element data type");
		
		var controlType = Type.GetType($"Ui{elementType.Name}");
		if (controlType == null) throw new NotImplementedException($"Control not implemented: Ui{elementType.Name}");

		var control = (Control)Activator.CreateInstance(controlType)!;

		foreach (var dataProperty in data.Properties) {
			var controlProperty = control.GetType().GetProperty(dataProperty.Key);
			if (controlProperty == null || !controlProperty.CanWrite) continue;

			var value = dataProperty.Value;
			var valueType = controlProperty.PropertyType;
			
			if (value is JsonElement jsonElement) 
				value = jsonElement.Deserialize(valueType);

			if (value != null && !value.GetType().IsAssignableTo(valueType)) continue;
			
			controlProperty.SetValue(control, value);
		}

		if (control.GetType().GetProperty("Children") is { } childrenProperty) {
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

}