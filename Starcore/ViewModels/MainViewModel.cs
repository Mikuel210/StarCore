using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SDK;
using StarCore.Core;

namespace StarCore.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	private ServerService _serverService;
	
	public MainViewModel()
	{
		// TODO: Allow users to select a server
		_serverService = new("http://localhost:5000");
		_ = Connect();
	}

	public async Task Connect()
	{
		await _serverService.ConnectAsync();
	}
}