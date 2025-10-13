using SDK;

namespace Modules;

[ModuleDescription("A system for testing stuff")]
public class TestSystem : SystemInstance
{

	public override void Open()
	{
		Output.Info("Hi from test system!");

		new Thread(() => {
			Thread.Sleep(10_000);
			Core.Open<TestProtocol>();
		}).Start();
	}

}