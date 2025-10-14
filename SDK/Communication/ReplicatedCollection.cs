using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableCollections;

namespace SDK.Communication;

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

public interface INetworkProperty;

public interface INetworkValue : INetworkProperty;

public interface INetworkCollection : INetworkProperty
{

	event NotifyCollectionChangedEventHandler? CollectionChanged;

}


public class NetworkValue<T> : INetworkValue;

public class NetworkCollection<T> : ObservableCollection<T>, INetworkCollection;
public class NetworkDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, INetworkCollection;


public class ReplicatedStorage
{

	// Properties
	public NetworkCollection<InstanceData> OpenInstances { get; set; } = [];

}

public class ClientStorage
{

	// Properties
	public NetworkCollection<Guid> FocusedInstanceIds { get; set; } = [];
	private ObservableDictionary<string, int> FocusedInstanceUis;
	
}









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