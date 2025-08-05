using System;
using UnityEditor;

public struct EditorModifyScope : IDisposable
{
	public GameInspector mInspector;
	public EditorModifyScope(GameInspector inspector)
	{
		mInspector = inspector;
		EditorGUI.BeginChangeCheck();
	}
	public readonly void Dispose()
	{
		if (EditorGUI.EndChangeCheck())
		{
			mInspector.setModify();
		}
	}
}