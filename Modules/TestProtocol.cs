using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");
		
		Root.AddChild(new TextLabel("Hi :)"));

		var panel = new Panel();
		panel.AddChild(new TextLabel("bye"));
		Root.AddChild(panel);
	}

}