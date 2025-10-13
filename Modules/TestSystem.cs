using SDK;

namespace Modules;

[ModuleDescription("A system for testing multiline descriptions. Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.")]
public class TestSystem : SystemInstance
{

	public override void Open()
	{
		Output.Info("Hi from test system!");

		new Thread(() => {
			Thread.Sleep(15_000);
			Core.Open<TestProtocol>();
			Thread.Sleep(5_000);
			Core.Open<TestProtocol>();
			Thread.Sleep(5_000);
			Core.Open<TestProtocol>();
			Thread.Sleep(5_000);
			Core.Open<TestProtocol>();
			Thread.Sleep(5_000);
			Core.Open<TestProtocol>();
		}).Start();
	}

}