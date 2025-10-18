namespace SDK.Communication;

public static class ServerBridge
{

	public static void HandleContainerAction(Client client, Type containerType, ContainerAction action)
	{
		if (containerType == typeof(ReplicatedContainer))
			Server.ReplicatedStorage.HandleContainerAction(action);
		if (containerType == typeof(ClientContainer))
			Server.ClientStorage[client.ConnectionId].HandleContainerAction(action);
	}

}