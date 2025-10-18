using SDK;

namespace Modules;

[ModuleDescription("A system for testing multiline descriptions. Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.")]
public class TestSystem : SystemInstance
{

	public override void Open()
	{
		Output.Info("Hi from test system!");
		
		new Thread(() => {
			while (true) {
				Core.GetOpenInstances<TestProtocol>().ForEach(e => e.Title = "Focus on RUNTIME");
				Thread.Sleep(1_000);
				Core.GetOpenInstances<TestProtocol>().ForEach(e => e.Title = "Focus on Starkit");
				Thread.Sleep(1_000);	
			}
		}).Start();
	}

}