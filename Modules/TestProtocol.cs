using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");

		
		var panel1 = new Panel();
		panel1.AddChild(new TextLabel("1"));
		Root.AddChild(panel1);

		var panel2 = new Panel();
		panel2.AddChild(new TextLabel("2"));
		var label = new Checkbox("MovingLabel");
		panel2.AddChild(label);
		Root.AddChild(panel2);
		
		new Thread(() => {
			new Thread(() => {
				while (true) {
					label.Text = "MovingLabel";
					Thread.Sleep(750);
					label.Text = "I'm moving btw";
					Thread.Sleep(750);
				}
			}).Start();
			
			while (true) {
				Thread.Sleep(3000);
				label.Parent = panel1;
				label.IsChecked = false;
				Thread.Sleep(3000);
				label.Parent = panel2;
				label.IsChecked = true;
			}
		}).Start();
	}

}