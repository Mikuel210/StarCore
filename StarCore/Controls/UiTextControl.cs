using System;
using Avalonia;

namespace StarCore.Controls;

public class UiTextControl : UiControl
{

	public static readonly StyledProperty<string> TextProperty =
		AvaloniaProperty.Register<UiTextControl, string>(nameof(Text), string.Empty);

	public string Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

}