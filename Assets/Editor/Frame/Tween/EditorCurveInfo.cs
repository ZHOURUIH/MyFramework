using System.Collections.Generic;
using System.Reflection;

public static class EditorCurveInfo
{
	private static string[] mNames;
	private static int[] mIDs;
	public static string[] getNames()
	{
		Build();
		return mNames;
	}
	public static int[] getIDs()
	{
		Build();
		return mIDs;
	}
	private static void Build()
	{
		if (mNames != null)
		{
			return;
		}
		List<string> names = new();
		List<int> ids = new();
		foreach (FieldInfo field in typeof(KEY_CURVE).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (names.addIf(field.Name, field.FieldType == typeof(int) && field.Name != "NONE" && field.Name != "MAX_BUILDIN_CURVE"))
			{
				ids.Add((int)field.GetValue(null));
			}
		}
		mNames = names.ToArray();
		mIDs = ids.ToArray();
	}
}