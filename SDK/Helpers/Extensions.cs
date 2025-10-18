namespace SDK.Helpers;

public static class Extensions
{

	public static List<T> ToListSafe<T>(this IEnumerable<T> enumerable)
	{
		List<T>? output = null;

		while (output == null) {
			try { output = enumerable.ToList(); } 
			catch (InvalidOperationException) { }
		}

		return output;
	}

}