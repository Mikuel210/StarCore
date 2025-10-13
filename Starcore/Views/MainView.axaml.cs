using Avalonia.Controls;
using StarCore.ViewModels;

namespace StarCore.Views;

public partial class MainView : UserControl
{

	public MainView()
	{
		InitializeComponent(); 
		DataContext = new MainViewModel();
	}

}