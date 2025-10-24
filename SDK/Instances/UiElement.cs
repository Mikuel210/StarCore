using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using SDK.Communication;

namespace SDK.Instances;

#region Base Classes

public abstract class UiElement : INotifyPropertyChanged
{

	public event PropertyChangedEventHandler? PropertyChanged;
	public Guid ElementId { get; } = Guid.NewGuid();

	private ContainerElement? _parent;
	public ContainerElement? Parent
	{
		get => _parent;

		set {
			_parent?.children.Remove(this);
			_parent = value;
			_parent?.children.Add(this);
		}
	}

}

public abstract class ContainerElement : UiElement
{

	internal readonly ObservableCollection<UiElement> children = [];
	public ReadOnlyCollection<UiElement> Children => children.AsReadOnly();
	
	// TODO: API
	public void AddChild(UiElement child) => child.Parent = this;
	public void RemoveChild(UiElement child) => child.Parent = null;

}

public abstract class TextElement(string text = "") : UiElement
{

	public string Text { get; set; } = text;

}

#endregion

#region UI Elements

public class Panel : ContainerElement;

public class TextLabel(string text = "") : TextElement(text);

#endregion