using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;

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
		try {
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
		catch (Exception e) {
			Output.Error($"EXCEPTION ON POPULATE: {e}");
		}
	}

}

public class ReplicatedContainer : Container
{

	public NetworkCollection<InstanceData> OpenInstances { get; set; } = [];
	public NetworkValue<string> ReplicatedString { get; init; } = new(string.Empty);

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

public record struct ContainerActionEnvelope(string ActionType, object?[] Payload)
{

	public static ContainerActionEnvelope FromAction(ContainerAction action)
	{
		var envelope = new ContainerActionEnvelope();
		
		// Set type
		var actionType = action.GetType();
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

		// Send
		envelope.Payload = payload.ToArray();
		return envelope;
	}

}

#endregion

#region Storage

public class NetworkStorage<T> where T : Container
{

	public T Container { get; }
	public event Action<ContainerAction>? ContainerChanged;

	public NetworkStorage(T container)
	{
		Container = container;
		Subscribe();
	}

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
					Output.Debug($"Fetch received, posting: {string.Join(", ", Container.ToProperties())}");
					ContainerChanged?.Invoke(new ContainerPostAction(Container.ToProperties()));
					break;
				
				case ContainerPostAction postAction:
					Output.Debug($"Post received, populating: {string.Join(", ", postAction.Properties)}");
					Container.Populate(postAction.Properties);
					Output.Debug($"Populated: {string.Join(", ", Container.ToProperties())}");
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
	}

	private void HandleNetworkValueAction(INetworkValue networkValue, ContainerAction action)
	{
		if (action is not ContainerSetAction setAction)
			throw new InvalidOperationException("Invalid container action type");
		
		if (setAction.Value != null && networkValue.GetType().GenericTypeArguments.Length > 0 && 
			!setAction.Value.GetType().IsAssignableTo(networkValue.GetType().GenericTypeArguments[0]))
			throw new InvalidOperationException("Invalid container set action value type");
		
		// Setting interface value won't trigger network update
		networkValue.Value = setAction.Value;
	}
	private void HandleNetworkCollectionAction(INetworkCollection networkCollection, ContainerAction action)
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