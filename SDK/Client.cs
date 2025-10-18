using Microsoft.AspNetCore.SignalR;
using SDK.Communication;

namespace SDK;

public abstract class Client(string connectionId, IClientProxy proxy) : INotificationCapability
{
	
	public string ConnectionId { get; } = connectionId;

	public static Client? FromConnectionId(string connectionId) =>
		Server.ConnectedClients.FirstOrDefault(e => e.ConnectionId == connectionId);
	
	internal void SendCommand(ServerCommand command) => 
		proxy.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));
	internal void SendContainerAction(Type containerType, ContainerAction action) =>
		proxy.SendAsync("HandleContainerAction", ContainerActionEnvelope.FromAction(containerType, action));
	internal void SendContainerAction<T>(ContainerAction action) => SendContainerAction(typeof(T), action);

}

#region Capabilities

public interface INotificationCapability
{

	void ShowNotification(string title, string body = "") => 
		(this as Client)?.SendCommand(new ServerNotificationCommand(title, body));

}

#endregion

#region Clients

public class BrowserClient(string connectionId, IClientProxy proxy) : Client(connectionId, proxy);

public class DesktopClient(string connectionId, IClientProxy proxy) : Client(connectionId, proxy);

public class MobileClient(string connectionId, IClientProxy proxy) : Client(connectionId, proxy);

#endregion