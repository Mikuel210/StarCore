using SDK;
using SDK.Instances;

namespace Modules;

public class TestSystem : SystemInstance
{

	public override void Open()
	{
		Root.AddChild(new TextLabel("Welcome to StarCore! Connection to the server is successful, all systems nominal"));
		Root.AddChild(new TextLabel("This is a test system and you're seeing some UI elements that it's defined"));
		Root.AddChild(new TextLabel("Go ahead and open a protocol from the \"open\" dropdown"));
	}

}