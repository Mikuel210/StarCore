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
		
		var panel = new Panel();
		panel.AddChild(new TextLabel("2"));
		Root.AddChild(panel);
		
		var label = new TextLabel("MovingLabel");
		panel.AddChild(label);
		
		new Thread(() => {
			/*new Thread(() => {
				while (true) {
					label.Text = "MovingLabel";
					Thread.Sleep(750);
					label.Text = "I'm moving btw";
					Thread.Sleep(750);
				}
			}).Start();*/
			
			while (true) {
				Thread.Sleep(3000);
				label.Parent = Root;
				Thread.Sleep(3000);
				label.Parent = panel;
			}
		}).Start();
	}

}