using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");

		var label = new TextLabel("Hi :)");
		Root.AddChild(label);

		var panel = new Panel();
		panel.AddChild(new TextLabel("bye"));
		Root.AddChild(panel);
		
		new Thread(() => {
			while (true) {
				label.Text = "HI!!";
				
				Root.AddChild(new TextLabel("Hi :)"));
				Thread.Sleep(1000);
				
				label.Text = "BYE!!";

				var label1 = new TextLabel("Omg");
				Root.AddChild(label1);
				Thread.Sleep(1000);
				
				Root.RemoveChild(label1);
				Thread.Sleep(1000);
			}
		}).Start();
	}

}