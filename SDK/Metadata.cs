namespace SDK;

[AttributeUsage(AttributeTargets.Class)]
public abstract class ModuleAttribute<T>(T value) : Attribute { public T Value { get; } = value; }

public class ModuleNameAttribute(string name) : ModuleAttribute<string>(name);
public class ModuleDescriptionAttribute(string description) : ModuleAttribute<string>(description);
public class ShowOnClientAttribute(bool showOnClient) : ModuleAttribute<bool>(showOnClient);
public class CanClientOpenAttribute(bool canClientOpen) : ModuleAttribute<bool>(canClientOpen);
public class NotifyOnOpenAttribute(bool notifyOnOpen) : ModuleAttribute<bool>(notifyOnOpen);