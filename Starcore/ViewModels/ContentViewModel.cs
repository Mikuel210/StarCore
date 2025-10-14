using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SDK.Communication;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class ContentViewModel : ViewModelBase
{

	[ObservableProperty] private string _title;

	public ContentViewModel()
	{
		InstanceService.FocusedInstanceUpdated += Update;
		Update();
	}

	private void Update() => Title = InstanceService.FocusedInstance?.Title ?? string.Empty;

}