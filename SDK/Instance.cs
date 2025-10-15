using System.ComponentModel;
using SDK.Communication;

namespace SDK;

public abstract class Instance : INotifyPropertyChanged
{
	
	public event PropertyChangedEventHandler? PropertyChanged;

	public Guid InstanceId { get; } = Guid.NewGuid();
	public string Title { get; set; } = string.Empty;

	public Instance()
	{
		// TODO
		
		/*
		PropertyChanged += (_, _) => {
			foreach (var client in Server.ConnectedClients)
				client.SendCommand(new ServerUpdateInstanceCommand(InstanceData.FromInstance(this)));
		};
		*/
	}
	
	public virtual void Open() { }
	public virtual void Loop() { }

}

public abstract class SystemInstance : Instance;

public abstract class ProtocolInstance : Instance
{

	public bool CanClientClose { get; set; } = true;
	
	public virtual void Close() { }

}