namespace SDK;

public abstract class Instance
{

	public string Title { get; set; }
	
	public virtual void Open() { }
	public virtual void Loop() { }

}

public abstract class SystemInstance : Instance;

public abstract class ProtocolInstance : Instance
{

	public bool CanClose { get; set; } = true;
	
	public virtual void Close() { }

}