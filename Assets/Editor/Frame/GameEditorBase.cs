using UnityEngine;
using UnityEditor;
using System;

public class GameEditorBase : Editor
{
	public SerializedProperty findProperty(string name)
	{
		var property = serializedObject.FindProperty(name);
		if(property == null)
		{
			Debug.LogError("找不到属性:" + name + ",确保属性存在并且访问权限为public");
		}
		return property;
	}
	public void displayProperty(string propertyName, string displayName, bool includeChildren = true)
	{
		var property = findProperty(propertyName);
		EditorGUILayout.PropertyField(property, new GUIContent(displayName), includeChildren);
	}
	// 直接显示一个bool属性,并且返回修改以后的值
	public bool displayToggle(string displayName, bool value)
	{
		return EditorGUILayout.Toggle(displayName, value);
	}
	public Enum displayEnum(string displayName, Enum value)
	{
		return EditorGUILayout.EnumPopup(displayName, value);
	}
	public int intPopup(string displayName, string[] valueDisplay, int[] values)
	{
		return EditorGUILayout.IntPopup(displayName, 0, valueDisplay, values);
	}
	static public void BeginContents()
	{
		EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
		GUILayout.Space(10f);
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}
	static public void EndContents()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(3f);
	}
}