using System.Collections.ObjectModel;
using SDK;
using SDK.Communication;

namespace StarCore.Core;

public static class InstanceService
{
	
	public static ObservableCollection<InstanceEnvelope> OpenInstances { get; private set; } = new();

	static InstanceService()
	{
		ServerService.OnConnected += async () => {
			Output.Info("Connected to server");
			await ServerService.SendCommandAsync(new ClientGetOpenInstancesCommand());
		};
	}
	
	public static void UpdateOpenInstances(InstanceEnvelope[] instances) => OpenInstances = new(instances);

}