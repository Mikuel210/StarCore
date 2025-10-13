using System.Reflection;
using System.Text.Json;

namespace SDK;

public abstract record Command
{

	public static Command FromEnvelope(CommandEnvelope envelope)
	{
		Type commandType = Type.GetType(envelope.CommandType)!;
		if (commandType is null) throw new InvalidOperationException("Invalid envelope type");
		
		// Deserialize payload
		var constructor = commandType.GetConstructors().Single();
		var parameters = constructor.GetParameters();
		var payload = new List<object?>();

		for (int i = 0; i < parameters.Length; i++) {
			var parameter = parameters[i];
			var payloadObject = envelope.Payload[i];

			if (payloadObject is not JsonElement jsonElement) {
				payload.Add(payloadObject);
				continue;
			}

			var parameterType = parameter.ParameterType;
			object? value = jsonElement.Deserialize(parameterType);
			
			payload.Add(value);
		}

		// Create instance
		object instance = constructor.Invoke(payload.ToArray());
		if (instance is not Command command) throw new InvalidOperationException("Invalid envelope payload");

		return command;
	}

}
public abstract record ServerCommand : Command
{

	public new static ServerCommand FromEnvelope(CommandEnvelope envelope)
	{
		return Command.FromEnvelope(envelope) as ServerCommand ??
			   throw new InvalidOperationException("Invalid envelope type");
	}

}
public abstract record ClientCommand : Command
{

	public new static ClientCommand FromEnvelope(CommandEnvelope envelope)
	{
		return Command.FromEnvelope(envelope) as ClientCommand ??
			   throw new InvalidOperationException("Invalid envelope type");
	}

}

public record struct CommandEnvelope(string CommandType, object?[] Payload)
{

	public static CommandEnvelope FromCommand(Command command)
	{
		var envelope = new CommandEnvelope();
		
		// Set type
		var commandType = command.GetType();
		envelope.CommandType = commandType.AssemblyQualifiedName!;
		
		// Set payload
		var constructor = commandType.GetConstructors().Single();
		var parameters = constructor.GetParameters();
		List<object?> payload = new();

		foreach (var parameter in parameters) {
			var property = commandType.GetProperty(parameter.Name!)!;
			var value = property.GetValue(command);
			
			payload.Add(value);
		}

		// Send
		envelope.Payload = payload.ToArray();
		return envelope;
	}

}


#region Server Commands

public record ServerPingCommand(string Message) : ServerCommand;
public record ServerNotificationCommand(string Title, string Body) : ServerCommand;

#endregion

#region Client Commands

public record ClientPingCommand(string Message) : ClientCommand;
public record ClientConnectCommand(string ClientType) : ClientCommand;

#endregion