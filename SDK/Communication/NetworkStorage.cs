using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;

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
			if (!_notifyChanges) return;
			NetworkCollectionChanged?.Invoke(sender, e);
		};	
	}
	
	void INetworkCollection.SetNotifyChanges(bool notifyChanges) => _notifyChanges = notifyChanges;

}

#endregion

#region Containers

public abstract class Container
{

	public record struct Property(string Name, object? Value);

	public List<Property> ToProperties()
	{
		var properties = GetType().GetProperties();
		var output = new List<Property>();

		foreach (var property in properties) {
			object? value = null;

			if (property.PropertyType.IsAssignableTo(typeof(INetworkValue))) {
				var networkValue = (INetworkValue)property.GetValue(this)!;
				value = networkValue.Value;
			} else if (property.PropertyType.IsAssignableTo(typeof(INetworkCollection))) {
				var networkCollection = (INetworkCollection)property.GetValue(this)!;
				value = networkCollection;
			}
			
			output.Add(new(property.Name, value));
		}

		return output;
	}

	public void Populate(List<Property> properties)
	{
		foreach (var property in properties) {
			var propertyName = property.Name;
			var propertyValue = property.Value;

			var propertyInfo = GetType().GetProperty(propertyName);
			if (propertyInfo == null) continue;

			var propertyType = propertyInfo.PropertyType;

			// Get value
			if (propertyValue is JsonElement jsonElement) {
				if (propertyType.IsAssignableTo(typeof(INetworkCollection)))
					propertyValue = jsonElement.Deserialize(propertyType, Server.JsonSerializerOptions);
				else if (propertyType.IsAssignableTo(typeof(INetworkValue))) 
					propertyValue = jsonElement.Deserialize(propertyType.GenericTypeArguments[0], Server.JsonSerializerOptions);
			}
			
			// Assign value
			if (propertyType.IsAssignableTo(typeof(INetworkValue))) {
				var networkValue = (INetworkValue)propertyInfo.GetValue(this)!;
				networkValue.Value = propertyValue;
			}
			else if (propertyType.IsAssignableTo(typeof(INetworkCollection)))
				propertyInfo.SetValue(this, propertyValue);
		}
	}

}

public class ReplicatedContainer : Container
{

	public NetworkCollection<ModuleData> Modules { get; set; } = [];
	public NetworkCollection<InstanceData> OpenInstances { get; set; } = [];

}

public class ClientContainer : Container
{

	public NetworkValue<Guid> FocusedInstance { get; } = new();

}

#endregion

#region Container Actions

public abstract record ContainerAction
{

	public static ContainerAction FromEnvelope(ContainerActionEnvelope envelope)
	{
		Type actionType = Type.GetType(envelope.ActionType)!;
		if (actionType is null) throw new InvalidOperationException("Invalid envelope type");
		
		// Deserialize payload
		var constructor = actionType.GetConstructors().Single();
		var parameters = constructor.GetParameters();
		var payload = new List<object?>();

		for (int i = 0; i < parameters.Length; i++) {
			var parameter = parameters[i];
			var parameterType = parameter.ParameterType;
			var payloadObject = envelope.Payload[i];
			
			if (payloadObject is not JsonElement jsonElement) {
				if (payloadObject is IConvertible)
					payload.Add(Convert.ChangeType(payloadObject, parameterType));
				else
					payload.Add(payloadObject);
				
				continue;
			}
			
			object? value = JsonSerializer.Deserialize(
				jsonElement.GetRawText(), 
				parameterType, 
				Server.JsonSerializerOptions
			);
			
			payload.Add(value);
		}
		
		// Create instance
		object instance = constructor.Invoke(payload.ToArray());
		if (instance is not ContainerAction action) throw new InvalidOperationException("Invalid envelope payload");

		return action;
	}	

}

public abstract record ContainerPropertyUpdate(string PropertyName) : ContainerAction;

public record ContainerFetchAction : ContainerAction;
public record ContainerPostAction(List<Container.Property> Properties) : ContainerAction;
	
public record ContainerSetAction(string PropertyName, object? Value) : ContainerPropertyUpdate(PropertyName);
public record ContainerAddAction(string PropertyName, int Index, IList Values) : ContainerPropertyUpdate(PropertyName);
public record ContainerMoveAction(string PropertyName, int OldIndex, int NewIndex) : ContainerPropertyUpdate(PropertyName);
public record ContainerRemoveAction(string PropertyName, int Index) : ContainerPropertyUpdate(PropertyName);
public record ContainerReplaceAction(string PropertyName, int Index, object? Value) : ContainerPropertyUpdate(PropertyName);
public record ContainerResetAction(string PropertyName) : ContainerPropertyUpdate(PropertyName);

public record struct ContainerActionEnvelope(string ContainerType, string ActionType, object?[] Payload)
{

	public static ContainerActionEnvelope FromAction(Type containerType, ContainerAction action)
	{
		var envelope = new ContainerActionEnvelope();
		
		// Set type
		var actionType = action.GetType();
		envelope.ContainerType = containerType.AssemblyQualifiedName!;
		envelope.ActionType = actionType.AssemblyQualifiedName!;
		
		// Set payload
		var constructor = actionType.GetConstructors().Single();
		var parameters = constructor.GetParameters();
		List<object?> payload = new();

		foreach (var parameter in parameters) {
			var property = actionType.GetProperty(parameter.Name!)!;
			var value = property.GetValue(action);
			
			payload.Add(value);
		}
		
		envelope.Payload = payload.ToArray();
		return envelope;
	}

	public static ContainerActionEnvelope FromAction<T>(ContainerAction action) => FromAction(typeof(T), action);

}

#endregion

#region Storage

public class NetworkStorage<T> where T : Container, new()
{

	public T Container { get; }
	public event Action<ContainerAction>? ContainerChanged;
	public event Action? ContainerUpdated; // From the other side

	public NetworkStorage(T container)
	{
		Container = container;
		Subscribe();
	}
	public NetworkStorage() : this(new()) { }

	private void Subscribe(bool subscribeToValues = true)
	{
		// Subscribe to property updates
		var properties = Container.GetType().GetProperties();

		foreach (var property in properties) {
			var networkProperty = property.GetValue(Container);
			var propertyName = property.Name;

			switch (networkProperty) {
				case INetworkValue networkValue:
					if (!subscribeToValues) continue;
					
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
	public void HandleContainerAction(ContainerAction action)
	{
		if (action is not ContainerPropertyUpdate update) {
			switch (action) {
				case ContainerFetchAction:
					ContainerChanged?.Invoke(new ContainerPostAction(Container.ToProperties()));
					break;
				
				case ContainerPostAction postAction:
					Container.Populate(postAction.Properties);
					Subscribe(false);
					
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
				HandleNetworkValueAction(networkValue, action);
				break;
			
			case INetworkCollection networkCollection:
				HandleNetworkCollectionAction(networkCollection, action);
				break;
		}
		
		// Fire container updated
		ContainerUpdated?.Invoke();
	}

	private void HandleNetworkValueAction(INetworkValue networkValue, ContainerAction action)
	{
		if (action is not ContainerSetAction setAction)
			throw new InvalidOperationException("Invalid container action type");

		var value = setAction.Value;
		var valueType = networkValue.GetType().GenericTypeArguments[0];

		if (value is JsonElement jsonElement) 
			value = jsonElement.Deserialize(valueType);
		
		if (value != null && !value.GetType().IsAssignableTo(valueType))
			throw new InvalidOperationException("Invalid container set action value type");
		
		// Setting interface value won't trigger network update
		networkValue.Value = value;
	}
	private void HandleNetworkCollectionAction(INetworkCollection networkCollection, ContainerAction action)
	{
		var collectionType = networkCollection.GetType().GenericTypeArguments[0];
		
		// Don't notify changes when a network update is processed
		networkCollection.SetNotifyChanges(false);
		
		switch (action) {
			case ContainerSetAction:
				throw new InvalidOperationException("Container set action is not supported for network collections");
			
			case ContainerAddAction addAction:
				var values = new List<object?>();

				foreach (var value in addAction.Values) {
					if (value is JsonElement jsonElement)
						values.Add(jsonElement.Deserialize(collectionType, Server.JsonSerializerOptions));
					else if (value is IConvertible)
						values.Add(Convert.ChangeType(value, collectionType));
				}

				values.Reverse();

				foreach (var value in values)
					networkCollection.Insert(addAction.Index, value);
				
				break;
			
			case ContainerMoveAction moveAction:
				networkCollection.Move(moveAction.OldIndex, moveAction.NewIndex);
				break;
			
			case ContainerRemoveAction removeAction:
				networkCollection.RemoveAt(removeAction.Index);
				break;
			
			case ContainerReplaceAction replaceAction:
				object? value1 = null;
				
				if (replaceAction.Value is JsonElement jsonElement1)
					value1 = jsonElement1.Deserialize(collectionType, Server.JsonSerializerOptions);
				else if (replaceAction.Value is IConvertible)
					value1 = Convert.ChangeType(value1, collectionType);
				
				networkCollection[replaceAction.Index] = value1;
				break;
			
			case ContainerResetAction:
				networkCollection.Clear();
				break;
		}
		
		networkCollection.SetNotifyChanges(true);
	}
	
}

#endregion