using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StarCore.Controls;

public partial class UiTextLabel : UserControl
{

	public string Text { get; set; } = string.Empty;

	public UiTextLabel()
	{
		InitializeComponent();
		DataContext = this;
	}

}