using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace StarCore.Controls;

// TODO: Redo the control hierarchy
public class UiContainerControl : UiControl
{

	public ObservableCollection<UiControl> Children { get; } = [];

}