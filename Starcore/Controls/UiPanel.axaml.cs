using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace StarCore.Controls;

public partial class UiPanel : UserControl
{

	// TODO: This should be updated efficiently when an update is received
	public ObservableCollection<Control> Children { get; } = [];

	public UiPanel()
	{
		InitializeComponent();
		DataContext = this;
	}

}