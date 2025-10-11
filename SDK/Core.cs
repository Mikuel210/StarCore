using System.Reflection;
using System.Runtime.Loader;

namespace SDK;

public static class Core
{
	public static List<Type> Modules { get; private set; } = new();
	public static List<Instance> OpenInstances { get; } = new();

	public static void LoadModules() {
		// Find modules .dll
		var modulesDirectory = Path.Combine(AppContext.BaseDirectory, "Modules/bin");
		
		if (!Path.Exists(modulesDirectory)) {
			var relativePath = "../../../../Modules/bin";
			string executablePath = Assembly.GetExecutingAssembly().Location;
			string serverDirectory = Path.GetDirectoryName(executablePath)!;
			modulesDirectory = Path.Join(serverDirectory, relativePath);
		}
		
		modulesDirectory = Path.GetFullPath(modulesDirectory);

		if (!Path.Exists(modulesDirectory)) {
			Output.Error("Modules directory not found");
			return;
		}
        
		var path = Directory
			.EnumerateFiles(modulesDirectory, "Modules.dll", SearchOption.AllDirectories)
			.FirstOrDefault();

		if (path == null) {
			Output.Error("Modules.dll not found");
			return;
		}
		
		// Load modules
		var loadContext = new AssemblyLoadContext(path, isCollectible: true);
		var assembly = loadContext.LoadFromAssemblyPath(path);

		Modules = assembly.GetTypes().Where(e => e.IsAssignableTo(typeof(Instance))).ToList();
		
		// Initialize instances
		OpenSystems();
	}
	private static void OpenSystems()
	{
		Modules
			.Where(e => e.IsAssignableTo(typeof(SystemInstance)))
			.ToList()
			.ForEach(e => Open(e));
	}

	private static void Open(Instance instance)
	{
		var instanceName = instance.GetType().Name;
		OpenInstances.Add(instance);
		
		// Open instance
		try { instance.Open(); }
		catch (Exception e) { Output.Error($"An exception was thrown when opening a {instanceName}: {e}"); }
		
		Output.Info($"A new {instanceName} instance has been opened.");
	}

	
	#region Instance Management
	
	public static Instance Open(Type module)
	{
		// Error checking
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		// TODO: Throw on open system instance twice
		
		// Open instance
		var instance = (Instance)Activator.CreateInstance(module)!;
		Open(instance);
		
		return instance;
	}
	
	public static T Open<T>() where T : Instance => (T)Open(typeof(T));

	public static List<Instance> GetOpenInstances(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		return OpenInstances.Where(e => e.GetType() == module).ToList();
	}
		
	public static List<Instance> GetOpenInstances<T>() where T : Instance => GetOpenInstances(typeof(T));
	
	public static Instance? GetOpenInstance(Type module) {
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		return GetOpenInstances(module).FirstOrDefault();	
	}
	
	public static T? GetOpenInstance<T>() where T : Instance => (T?)GetOpenInstance(typeof(T));
	
	public static void Close(Instance instance)
	{
		var moduleName = instance.GetType().Name;
		
		if (instance is not ProtocolInstance protocol) {
			Output.Warning($"Attempted to close a system instance: {moduleName}");
			return;
		}
		
		// Close protocol
		try { protocol.Close(); }
		catch (Exception e) { Output.Error($"An exception was thrown when closing a {moduleName}: {e}"); }
		
		OpenInstances.Remove(instance);
	}
	
	#endregion
}