using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");
		
		InstanceUi.Add(new TextLabel());
		((TextLabel)InstanceUi[0]).Text = "hiiii";
	}

}