using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SDK.Communication;
using StarCore.Services;

namespace StarCore.ViewModels;

public partial class ContentViewModel : ViewModelBase
{

	[ObservableProperty] private string _title = string.Empty;

	public ContentViewModel()
	{
		ReplicatedStorageService.ReplicatedStorage.ContainerUpdated += Update;
		ClientStorageService.FocusedInstanceChanged += Update;
		
		Update();
	}

	private void Update() => Title = ClientStorageService.FocusedInstance?.Title ?? string.Empty;

}