using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Root.AddChild(new TextLabel("This is a protocol: an interactive automation. It can define UI elements, perform native actions and interact with the framework"));
		Root.AddChild(new TextLabel("You can close it by clicking on the \"close\" button on the top right"));
	}

}