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
		
		Output.Success("Modules were loaded successfully");
	}
	private static void OpenSystems()
	{
		Modules
			.Where(e => e.IsAssignableTo(typeof(SystemInstance)))
			.ToList()
			.ForEach(e => Open(e));
	}
	
	public static void Tick()
	{
		foreach (var instance in OpenInstances)
			instance.Loop();
	}
	
	#region Instance Management
	
	private static void Open(Instance instance)
	{
		var moduleName = GetModuleName(instance);
		OpenInstances.Add(instance);
		
		// Set title
		instance.Title = moduleName;
		
		// Open instance
		try { instance.Open(); }
		catch (Exception e) { Output.Error($"An exception was thrown when opening a {moduleName}: {e}"); }
		
		Output.Info($"A new {moduleName} instance has been opened");
	}
	public static Instance Open(Type module)
	{
		// Error checking
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		if (module.IsAssignableTo(typeof(SystemInstance)) && GetOpenInstance(module) != null)
			throw new InvalidOperationException("System is already open");
		
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
	public static List<T> GetOpenInstances<T>() where T : Instance
	{	
		return OpenInstances
			.Where(e => e is T)
			.Cast<T>()
			.ToList();
	}
	
	public static Instance? GetOpenInstance(Type module) {
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		return GetOpenInstances(module).FirstOrDefault();	
	}
	public static T? GetOpenInstance<T>() where T : Instance => (T?)GetOpenInstance(typeof(T));

	public static Instance GetSystemInstance(Type module)
	{
		if (!module.IsAssignableTo(typeof(SystemInstance)))
			throw new ArgumentException("Type must be a system module");
		
		return GetOpenInstance(module) ?? 
			   throw new InvalidOperationException($"System is not initialized: {GetModuleName(module)}");
	}
	public static T GetSystemInstance<T>() where T : SystemInstance => (T)GetSystemInstance(typeof(T));
	
	public static void Close(Instance instance)
	{
		var moduleName = GetModuleName(instance);
		
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
	
	#region Module Metadata

	private static object? GetMetadata<TValue, TAttribute>(Type module) where TAttribute : ModuleAttribute<TValue>
	{
		// Error checking
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		// Get attribute value
		var attribute = module.GetCustomAttribute<TAttribute>() as ModuleAttribute<TValue>;
		if (attribute == null) return null;

		return attribute.Value;
	}

	private static string SplitWords(string input)
	{
		string output = string.Empty;

		for (int i = 0; i < input.Length; i++) {
			var character = input[i];

			if (char.IsUpper(character) && i != 0) output += ' ';
			output += character;
		}

		return output;
	}
	public static string GetModuleName(Type module)
	{
		var attributeValue = GetMetadata<string, ModuleNameAttribute>(module);

		if (attributeValue == null) return SplitWords(module.Name);
		return (string)attributeValue;
	}
	public static string GetModuleName(Instance instance) => GetModuleName(instance.GetType());
	public static string GetModuleName<T>() => GetModuleName(typeof(T));
	
	public static string GetModuleDescription(Type module)
	{
		var attributeValue = GetMetadata<string, ModuleDescriptionAttribute>(module);
		return attributeValue as string ?? string.Empty;
	}
	public static string GetModuleDescription(Instance instance) => GetModuleDescription(instance.GetType());
	public static string GetModuleDescription<T>() => GetModuleDescription(typeof(T));

	public static bool GetShowOnClient(Type module)
	{
		var attributeValue = GetMetadata<bool, ShowOnClientAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	public static bool GetShowOnClient(Instance instance) => GetShowOnClient(instance.GetType());
	public static bool GetShowOnClient<T>() => GetShowOnClient(typeof(T));
	
	public static bool CanClientOpen(Type module)
	{
		var attributeValue = GetMetadata<bool, CanClientOpenAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	public static bool CanClientOpen(Instance instance) => CanClientOpen(instance.GetType());
	public static bool CanClientOpen<T>() => CanClientOpen(typeof(T));

	public static bool GetNotifyOnOpen(Type module)
	{
		var attributeValue = GetMetadata<bool, NotifyOnOpenAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	public static bool GetNotifyOnOpen(Instance instance) => GetNotifyOnOpen(instance.GetType());
	public static bool GetNotifyOnOpen<T>() => GetNotifyOnOpen(typeof(T));
	
	#endregion

}