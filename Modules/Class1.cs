using SDK;

namespace Modules;

public class TestSystem : SystemInstance
{

	public override void Open()
	{
		Output.Info("Hi from test system!");
	}

}