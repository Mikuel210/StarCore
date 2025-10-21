using SDK;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");
		
		InstanceUi.Add(new TextLabel());
	}

}