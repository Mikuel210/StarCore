namespace SDK;

public abstract class Client(string connectionId) : INotificationCapability
{
	
	public string ConnectionId { get; } = connectionId;
	
	internal void SendCommand(ServerCommand command) { } // TODO

}

#region Capabilities

public interface INotificationCapability
{

	void ShowNotification(string title, string body = "") => 
		(this as Client)?.SendCommand(new ServerNotificationCommand(title, body));

}

#endregion

#region Clients

public class BrowserClient(string connectionId) : Client(connectionId);

public class DesktopClient(string connectionId) : Client(connectionId);

public class MobileClient(string connectionId) : Client(connectionId);

#endregion