using Microsoft.AspNetCore.SignalR;
using SDK.Communication;

namespace SDK;

public abstract class Client(string connectionId, IClientProxy proxy) : INotificationCapability
{
	
	public string ConnectionId { get; } = connectionId;
	
	internal void SendCommand(ServerCommand command) => 
		proxy.SendAsync("HandleCommand", CommandEnvelope.FromCommand(command));

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