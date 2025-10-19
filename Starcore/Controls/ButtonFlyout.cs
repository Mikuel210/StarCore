using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace StarCore.Controls;

public class ButtonFlyout : Flyout, INamed
{
	
	public string? Name { get; set; }
	
	private static readonly List<ButtonFlyout> _flyouts = [];
	public ButtonFlyout() => _flyouts.Add(this);

	public static ButtonFlyout? FromName(string name) => _flyouts.FirstOrDefault(e => e.Name == name);
	
	protected override void OnOpened()
	{
		if (Content is not Control control) goto End;
		
		foreach (var button in control.GetVisualDescendants().OfType<Button>())
			button.Click += (_, _) => Hide();

		End:
		base.OnOpened();
	}

}