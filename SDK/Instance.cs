using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using SDK.Instances;

namespace SDK;

public abstract class Instance : INotifyPropertyChanged
{
	
	public event PropertyChangedEventHandler? PropertyChanged;
	
	public Guid InstanceId { get; } = Guid.NewGuid();
	public string Title { get; set; } = string.Empty;

	public ObservableCollection<UiElement> InstanceUi { get; } = [];

	public static Instance? FromInstanceId(string instanceId) =>
		Core.OpenInstances.FirstOrDefault(e => e.InstanceId.ToString() == instanceId);
	public static Instance? FromInstanceId(Guid instanceId) => FromInstanceId(instanceId.ToString());
	
	public virtual void Open() { }
	public virtual void Loop() { }

}

public abstract class SystemInstance : Instance;

public abstract class ProtocolInstance : Instance
{

	public bool CanClientClose { get; set; } = true;
	
	public virtual void Close() { }

}