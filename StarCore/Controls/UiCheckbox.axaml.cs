using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StarCore.Controls;

public partial class UiCheckbox : UiTextControl
{
	
	public bool IsChecked { get; set; }

	public UiCheckbox()
	{
		InitializeComponent();
	}

}