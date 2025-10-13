using SDK;

namespace Modules;

public class TestProtocol : ProtocolInstance
{

	public override void Open()
	{
		Output.Info("Hi from test protocol!");
		
		new Thread(() => {
			while (true)
				ChangeTitle();
		}).Start();
	}

	private void ChangeTitle()
	{
		Title = "Focus on Starkit";
		Thread.Sleep(1000);
		Title = "Focus on RUNTIME";
		Thread.Sleep(1000);
	}

}