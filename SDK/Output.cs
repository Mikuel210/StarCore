namespace SDK;

public static class Output
{
	private const bool DEBUG = false; 
	
	public static void WriteTitledMessage(string title, string message, ConsoleColor backgroundColor,
		ConsoleColor foregroundColor = ConsoleColor.Black) {
		Console.ForegroundColor = foregroundColor;
		Console.BackgroundColor = backgroundColor;
		Console.Write(title.ToUpper());
		
		Console.ResetColor();
		Console.WriteLine(" " + message);
	}
	
	public static void Success(string message) => WriteTitledMessage("Success", message, ConsoleColor.Green);
	public static void Error(string message) => WriteTitledMessage("Error", message, ConsoleColor.Red);
	public static void Warning(string message) => WriteTitledMessage("Warning", message, ConsoleColor.Yellow);
	public static void Info(string message) => WriteTitledMessage("Info", message, ConsoleColor.Cyan);
	public static void Debug(string message) {
		if (DEBUG)
			WriteTitledMessage("Debug", message, ConsoleColor.Magenta);
	}
}