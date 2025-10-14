using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StarCore.ViewModels;

namespace StarCore.Views;

public partial class ContentView : UserControl
{

	public ContentView()
	{
		InitializeComponent();
		DataContext = new ContentViewModel();
	}

}