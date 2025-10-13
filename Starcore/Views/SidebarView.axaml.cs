using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StarCore.ViewModels;

namespace StarCore.Views;

public partial class SidebarView : UserControl
{

	public SidebarView()
	{
		InitializeComponent();
		DataContext = new SidebarViewModel();
	}

}