using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace StarCore.Controls;

public partial class UiPanel : UserControl
{
	
	public ObservableCollection<Control> Children { get; } = [];

	public UiPanel()
	{
		InitializeComponent();
		DataContext = this;
	}

}