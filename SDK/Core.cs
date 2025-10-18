using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Loader;

namespace SDK;

public static class Core
{
	public static List<Type> Modules { get; private set; } = [];
	public static List<Instance> OpenInstances { get; } = [];

	internal static event Action? ModulesLoaded;
	public static event Action<Instance>? InstanceOpened;
	public static event Action<Instance>? InstanceClosed;

	public static void Initialize()
	{
		Server.Initialize();
		LoadModules();
		OpenSystems();
	}
	private static void LoadModules() {
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
		
		// Invoke event
		ModulesLoaded?.Invoke();
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
		
		// Set title
		instance.Title = moduleName;
		
		// Open instance
		try { instance.Open(); }
		catch (Exception e) { Output.Error($"An exception was thrown when opening a {moduleName}: {e}"); }
		
		OpenInstances.Add(instance);
		InstanceOpened?.Invoke(instance);
		
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

	[Pure] public static List<Instance> GetOpenInstances(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		return OpenInstances.Where(e => e.GetType() == module).ToList();
	}
	[Pure] public static List<T> GetOpenInstances<T>() where T : Instance
	{	
		return OpenInstances
			.Where(e => e is T)
			.Cast<T>()
			.ToList();
	}
	
	[Pure] public static Instance? GetOpenInstance(Type module) {
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		return GetOpenInstances(module).FirstOrDefault();	
	}
	[Pure] public static T? GetOpenInstance<T>() where T : Instance => (T?)GetOpenInstance(typeof(T));

	[Pure] public static Instance GetSystemInstance(Type module)
	{
		if (!module.IsAssignableTo(typeof(SystemInstance)))
			throw new ArgumentException("Type must be a system module");
		
		return GetOpenInstance(module) ?? 
			   throw new InvalidOperationException($"System is not initialized: {GetModuleName(module)}");
	}
	[Pure] public static T GetSystemInstance<T>() where T : SystemInstance => (T)GetSystemInstance(typeof(T));
	
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
		InstanceClosed?.Invoke(instance);
	}
	
	#endregion
	
	#region Module Metadata

	private static object? GetMetadata<TValue, TAttribute>(Type module) where TAttribute : ModuleAttribute<TValue>
	{
		// Error checking
		if (!module.IsAssignableTo(typeof(Instance)))
			throw new ArgumentException("Type must be a module");
		
		// Get attribute value
		if (module.GetCustomAttribute<TAttribute>() is not ModuleAttribute<TValue> attribute) 
			return null;

		return attribute.Value;
	}
	
	public enum ModuleType
	{

		System,
		Protocol

	}
	[Pure] public static ModuleType GetModuleType(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		return module.IsAssignableTo(typeof(SystemInstance)) ? ModuleType.System : ModuleType.Protocol;	
	}
	[Pure] public static ModuleType GetModuleType(Instance instance) => GetModuleType(instance.GetType());
	[Pure] public static ModuleType GetModuleType<T>() where T : Instance => GetModuleType(typeof(T));
	
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
	[Pure] public static string GetModuleName(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		var attributeValue = GetMetadata<string, ModuleNameAttribute>(module);

		if (attributeValue == null) return SplitWords(module.Name);
		return (string)attributeValue;
	}
	[Pure] public static string GetModuleName(Instance instance) => GetModuleName(instance.GetType());
	[Pure] public static string GetModuleName<T>() => GetModuleName(typeof(T));
	
	[Pure] public static string GetModuleDescription(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		var attributeValue = GetMetadata<string, ModuleDescriptionAttribute>(module);
		return attributeValue as string ?? string.Empty;
	}
	[Pure] public static string GetModuleDescription(Instance instance) => GetModuleDescription(instance.GetType());
	[Pure] public static string GetModuleDescription<T>() => GetModuleDescription(typeof(T));

	[Pure] public static bool GetShowOnClient(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		var attributeValue = GetMetadata<bool, ShowOnClientAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	[Pure] public static bool GetShowOnClient(Instance instance) => GetShowOnClient(instance.GetType());
	[Pure] public static bool GetShowOnClient<T>() => GetShowOnClient(typeof(T));
	
	[Pure] public static bool CanClientOpen(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		var attributeValue = GetMetadata<bool, CanClientOpenAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	[Pure] public static bool CanClientOpen(Instance instance) => CanClientOpen(instance.GetType());
	[Pure] public static bool CanClientOpen<T>() => CanClientOpen(typeof(T));

	[Pure] public static bool GetNotifyOnOpen(Type module)
	{
		if (!module.IsAssignableTo(typeof(Instance))) 
			throw new ArgumentException("Type must be a module");
		
		var attributeValue = GetMetadata<bool, NotifyOnOpenAttribute>(module);
		return attributeValue as bool? ?? true;
	}
	[Pure] public static bool GetNotifyOnOpen(Instance instance) => GetNotifyOnOpen(instance.GetType());
	[Pure] public static bool GetNotifyOnOpen<T>() => GetNotifyOnOpen(typeof(T));
	
	#endregion

}