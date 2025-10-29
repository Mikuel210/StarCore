using SDK;
using SDK.Communication;
using SDK.Instances;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	private readonly TextLabel _label = new();
	private int _seconds = 10;

	public override void Open()
	{
		Root.AddChild(new TextLabel("This is an automation. It can define UI elements, perform native actions and interact with the framework"));
		Root.AddChild(_label);
	}

	public override void Loop()
	{
		_label.Text = $"I will close myself in {_seconds} seconds...";
		Thread.Sleep(1000);
		
		if (--_seconds == 0) Core.Close(this);
	}

}