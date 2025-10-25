using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StarCore.Controls;

public partial class UiTextLabel : UiTextControl
{

	public UiTextLabel()
	{
		InitializeComponent();
		DataContext = this;
	}

}