namespace SDK;

public static class Output
{
	private static void WriteTitledMessage(string title, object? message, ConsoleColor backgroundColor,
		ConsoleColor foregroundColor = ConsoleColor.Black) {
		Console.ForegroundColor = foregroundColor;
		Console.BackgroundColor = backgroundColor;
		Console.Write(title.ToUpper());
		
		Console.ResetColor();
		Console.WriteLine(" " + message);
	}
	
	public static void Success(object? message) => WriteTitledMessage("Success", message, ConsoleColor.Green);
	public static void Error(object? message) => WriteTitledMessage("Error", message, ConsoleColor.Red);
	public static void Warning(object? message) => WriteTitledMessage("Warning", message, ConsoleColor.Yellow);
	public static void Info(object? message) => WriteTitledMessage("Info", message, ConsoleColor.Cyan);
	public static void Debug(object? message) => WriteTitledMessage("Debug", message, ConsoleColor.Magenta);
}