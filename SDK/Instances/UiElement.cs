using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using SDK.Communication;

namespace SDK.Instances;

public abstract class UiElement : INotifyPropertyChanged
{

	public event PropertyChangedEventHandler? PropertyChanged;

	public Guid ElementId { get; } = Guid.NewGuid();

	public UiElement? Parent { get; set; }

}

public abstract class TextElement : UiElement
{

	public string Text { get; set; } = string.Empty;

}

public abstract class ContainerElement : UiElement
{

	// TODO: Children

}

public class Panel : ContainerElement;

public class TextLabel : TextElement;

