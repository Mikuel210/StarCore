using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace StarCore.Controls;

public partial class UiPanel : UiContainerControl
{
	
	public UiPanel()
	{
		InitializeComponent();
		DataContext = this;
	}

}