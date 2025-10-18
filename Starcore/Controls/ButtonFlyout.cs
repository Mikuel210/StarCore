using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace StarCore.Controls;

public class ButtonFlyout : Flyout
{
	protected override void OnOpened()
	{
		if (Content is not Control control) goto End;
		
		foreach (var button in control.GetVisualDescendants().OfType<Button>())
			button.Click += (_, _) => Hide();

		End:
		base.OnOpened();
	}
}