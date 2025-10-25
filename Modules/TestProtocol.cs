using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");

		Root.AddChild(new TextLabel("1"));
		
		var label = new TextLabel("MovingLabel");
		Root.AddChild(label);

		var panel = new Panel();
		panel.AddChild(new TextLabel("2"));
		Root.AddChild(panel);
		
		new Thread(() => {
			Thread.Sleep(4000);
			panel.AddChild(label);	
		}).Start();
	}

}