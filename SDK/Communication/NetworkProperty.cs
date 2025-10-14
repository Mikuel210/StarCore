using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SDK.Communication;

#region Comments

// Open instances -> clients and server // server
// Focused instances -> individual clients and server // client
// Focused instances UI -> individual clients and server // server and client

// ABSTRACTION LAYER
// ------------------------------------------------------------------------------
// REPLICATED PROPERTY \   INetworkProperty
// CLIENT PROPERTY     /   |----> Access: Access.Server | Access.Client

// INetworkCollection -> CollectionChanged -> Notification
// New communication protocol -> HandleNetworkUpdate(Update | Add | Remove | Move | Clear, PropertyName, ReplicatedStorage | ClientStorage)

// Replicated storage
// Client storage \
// Server storage / The same

/*

 OLD CODE
 ------------------


public abstract class ReplicatedStorage
{

	public ReplicatedCollection<InstanceData> OpenInstances { get; set; } = [];

}

public class ReplicatedServerStorage : ReplicatedStorage
{

	public ReplicatedServerStorage()
	{
		// Subscribe to updates of properties
		var properties = GetType().GetProperties();

		var replicatedProperties = properties.Where(e =>
			e.GetType().IsAssignableTo(typeof(IReplicatedProperty)));

		foreach (var property in replicatedProperties) {
			var value = property.GetValue(this);

			switch (value) {
				case IReplicatedCollection replicatedCollection:
					SubscribeToReplicatedCollection(replicatedCollection);
					break;
			}
		}
	}

	private void SubscribeToReplicatedCollection(IReplicatedCollection replicatedCollection)
	{
		replicatedCollection.CollectionChanged += (_, args) => {
			switch (args.Action) {
				case NotifyCollectionChangedAction.Add:

					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Remove:
					break;

				case NotifyCollectionChangedAction.Replace:
					break;

				case NotifyCollectionChangedAction.Reset:
					break;
			}
		};
	}

}

public class ReplicatedClientStorage : ReplicatedStorage
{

	public ReplicatedClientStorage()
	{

	}

}

*/

#endregion

#region Properties

public interface INetworkProperty;

public interface INetworkValue : INetworkProperty
{
	
	event Action<object?>? ValueChanged;

}
public interface INetworkCollection : INetworkProperty
{

	event NotifyCollectionChangedEventHandler? CollectionChanged;

}

public class NetworkValue<T>(T value) : INetworkValue
{

	private T _value = value;
	public T Value
	{
		get => _value;

		set {
			_value = value;
			ValueChanged?.Invoke(value);
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public NetworkValue() : this(default!) { }

}
public class NetworkCollection<T> : ObservableCollection<T>, INetworkCollection;

#endregion

#region Containers

public record Container;
public record ReplicatedContainer(NetworkCollection<InstanceData> OpenInstances, NetworkValue<string> ReplicatedString);

#endregion


#region Storage

public abstract record ContainerChangeAction(string propertyName);
public record ContainerSetAction(string propertyName, object? value) : ContainerChangeAction(propertyName);
public record ContainerAddAction(string propertyName, IList values) : ContainerChangeAction(propertyName);
public record ContainerMoveAction(string propertyName, int oldIndex, int newIndex) : ContainerChangeAction(propertyName);
public record ContainerRemoveAction(string propertyName, int index) : ContainerChangeAction(propertyName);
public record ContainerReplaceAction(string propertyName, int index, object? value) : ContainerChangeAction(propertyName);
public record ContainerResetAction(string propertyName, IList valuesAdded, IList valuesRemoved) : ContainerChangeAction(propertyName);

public class Storage
{

	public Container Container { get; }
	public event Action<ContainerChangeAction>? ContainerChanged;

	public Storage(Container container)
	{
		Container = container;
		
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
					networkCollection.CollectionChanged += (_, args) => {
						ContainerChangeAction action;
						
						switch (args.Action) {
							case NotifyCollectionChangedAction.Add:
								action = new ContainerAddAction(propertyName, args.NewItems!);
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
							
							case NotifyCollectionChangedAction.Reset:
								// TODO: i think actions might involve multiple items???
								break;
						}

						ContainerChanged?.Invoke(action);
					};
					
					break;
			}
		}
	}

}

#endregion