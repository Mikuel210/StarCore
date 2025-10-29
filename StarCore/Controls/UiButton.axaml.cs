using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SDK.Instances;

namespace StarCore.Controls;

public partial class UiButton : UiTextControl
{

	public UiButton()
	{
		InitializeComponent();
		DataContext = this;
	}

}