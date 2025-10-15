using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SDK.Communication;

#region Properties

public interface INetworkProperty;

public interface INetworkValue : INetworkProperty
{ 
	
	object? Value { get; set; }
	event Action<object?>? ValueChanged;

}
public interface INetworkCollection : INetworkProperty, IList
{
	
	event NotifyCollectionChangedEventHandler? NetworkCollectionChanged;
	internal void SetNotifyChanges(bool notifyChanges);
	
	void Move(int oldIndex, int newIndex);

}

public class NetworkValue<T>(T value) : INetworkValue
{

	object? INetworkValue.Value { get; set; } = value;
	public T Value
	{
		get => (T)((INetworkValue)this).Value!;
		
		set {
			((INetworkValue)this).Value = value;
			ValueChanged?.Invoke(value);
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public NetworkValue() : this(default!) { }

}

public class NetworkCollection<T> : ObservableCollection<T>, INetworkCollection
{

	public event NotifyCollectionChangedEventHandler? NetworkCollectionChanged;
	private bool _notifyChanges = true;

	public NetworkCollection()
	{
		CollectionChanged += (sender, e) => {
			if (_notifyChanges)
				NetworkCollectionChanged?.Invoke(sender, e);
		};	
	}
	
	void INetworkCollection.SetNotifyChanges(bool notifyChanges) => _notifyChanges = notifyChanges;

}

#endregion

#region Containers

public abstract class Container;

public class ReplicatedContainer : Container
{

	public NetworkCollection<InstanceData> OpenInstances { get; } = [];
	public NetworkValue<string> ReplicatedString { get; } = new(string.Empty);

}

public struct ContainerEnvelope
{

	public static ContainerEnvelope FromContainer(Container container) => new();

}

#endregion

#region Storage

public abstract record ContainerAction;
public abstract record ContainerPropertyUpdate(string PropertyName) : ContainerAction;

public record ContainerFetchAction : ContainerAction;
public record ContainerPostAction(ContainerEnvelope Envelope) : ContainerAction;
	
public record ContainerSetAction(string PropertyName, object? Value) : ContainerPropertyUpdate(PropertyName);
public record ContainerAddAction(string PropertyName, int Index, IList Values) : ContainerPropertyUpdate(PropertyName);
public record ContainerMoveAction(string PropertyName, int OldIndex, int NewIndex) : ContainerPropertyUpdate(PropertyName);
public record ContainerRemoveAction(string PropertyName, int Index) : ContainerPropertyUpdate(PropertyName);
public record ContainerReplaceAction(string PropertyName, int Index, object? Value) : ContainerPropertyUpdate(PropertyName);
public record ContainerResetAction(string PropertyName) : ContainerPropertyUpdate(PropertyName);

public class NetworkStorage
{

	public Container Container { get; }
	public event Action<ContainerAction>? ContainerChanged;

	public NetworkStorage(Container container)
	{
		Container = container;
		Subscribe();
	}

	private void Subscribe()
	{
		// Subscribe to property updates
		var properties = Container.GetType().GetProperties();

		foreach (var property in properties) {
			var networkProperty = property.GetValue(Container);
			var propertyName = property.Name;

			switch (networkProperty) {
				case INetworkValue networkValue:
					networkValue.ValueChanged += value => 
						ContainerChanged?.Invoke(new ContainerSetAction(propertyName, value));
					
					break;
				
				case INetworkCollection networkCollection:
					SubscribeToNetworkCollection(propertyName, networkCollection);
					break;
			}
		}
	}
	private void SubscribeToNetworkCollection(string propertyName, INetworkCollection networkCollection)
	{
		networkCollection.NetworkCollectionChanged += (_, args) => {
			ContainerAction action;
						
			switch (args.Action) {
				case NotifyCollectionChangedAction.Add:
					action = new ContainerAddAction(propertyName, args.NewStartingIndex, args.NewItems!);
					break;
							
				case NotifyCollectionChangedAction.Move:
					action = new ContainerMoveAction(propertyName, args.OldStartingIndex, args.NewStartingIndex);
					break;
							
				case NotifyCollectionChangedAction.Remove:
					action = new ContainerRemoveAction(propertyName, args.OldStartingIndex);
					break;
							
				case NotifyCollectionChangedAction.Replace:
					action = new ContainerReplaceAction(propertyName, args.OldStartingIndex, args.NewItems![0]);
					break;
							
				case NotifyCollectionChangedAction.Reset: default:
					action = new ContainerResetAction(propertyName);
					break;
			}

			ContainerChanged?.Invoke(action);
		};
	}
	
	public void Fetch() => ContainerChanged?.Invoke(new ContainerFetchAction());
	public void ProcessContainerAction(ContainerAction action)
	{
		if (action is not ContainerPropertyUpdate update) {
			switch (action) {
				case ContainerFetchAction:
					ContainerChanged?.Invoke(new ContainerPostAction(ContainerEnvelope.FromContainer(Container)));
					break;
				
				case ContainerPostAction postAction:
					// TODO: Replace container and resubscribe!!!
					
					Subscribe();
					
					break;
			}
			
			return;
		}
		
		// Get property
		var property = Container.GetType().GetProperty(update.PropertyName);
		if (property is null) throw new InvalidOperationException("Invalid container action property name");

		var propertyValue = property.GetValue(Container);

		switch (propertyValue) {
			case INetworkValue networkValue:
				ProcessNetworkValueAction(networkValue, action);
				break;
			
			case INetworkCollection networkCollection:
				ProcessNetworkCollectionAction(networkCollection, action);
				break;
		}
	}

	private void ProcessNetworkValueAction(INetworkValue networkValue, ContainerAction action)
	{
		if (action is not ContainerSetAction setAction)
			throw new InvalidOperationException("Invalid container action type");
		
		if (setAction.Value != null && networkValue.GetType().GenericTypeArguments.Length > 0 && 
			!setAction.Value.GetType().IsAssignableTo(networkValue.GetType().GenericTypeArguments[0]))
			throw new InvalidOperationException("Invalid container set action value type");
		
		// Setting interface value won't trigger network update
		networkValue.Value = setAction.Value;
	}
	private void ProcessNetworkCollectionAction(INetworkCollection networkCollection, ContainerAction action)
	{
		// Don't notify changes when a network update is processed
		networkCollection.SetNotifyChanges(false);
		
		switch (action) {
			case ContainerSetAction:
				throw new InvalidOperationException("Container set action is not supported for network collections");
			
			case ContainerAddAction addAction:
				for (int i = addAction.Values.Count - 1; i >= 0; i--) {
					int index = addAction.Index + i;
					networkCollection.Insert(index, addAction.Values[i]);
				}
				
				break;
			
			case ContainerMoveAction moveAction:
				networkCollection.Move(moveAction.OldIndex, moveAction.NewIndex);
				break;
			
			case ContainerRemoveAction removeAction:
				networkCollection.RemoveAt(removeAction.Index);
				break;
			
			case ContainerReplaceAction replaceAction:
				networkCollection[replaceAction.Index] = replaceAction.Value;
				break;
			
			case ContainerResetAction:
				networkCollection.Clear();
				break;
		}
		
		networkCollection.SetNotifyChanges(true);
	}
	
}

#endregion