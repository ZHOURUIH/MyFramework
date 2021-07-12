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
	public void displayProperty(string propertyName, string displayName, string toolTip = "", bool includeChildren = true)
	{
		var property = findProperty(propertyName);
		EditorGUILayout.PropertyField(property, new GUIContent(displayName, toolTip), includeChildren);
	}
	// 直接显示一个bool属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public bool displayToggle(string displayName, string toolTip, bool value, ref bool modified)
	{
		bool retValue = EditorGUILayout.Toggle(new GUIContent(displayName, toolTip), value);
		if(retValue != value)
		{
			modified = true;
		}
		return retValue;
	}
	// 直接显示一个bool属性,并且会改变这个变量本身,如果被修改过,则会设置modified为true,否则不改变modefied的值,返回值为是否改变过
	public bool displayToggle(string displayName, string toolTip, ref bool value, ref bool modified)
	{
		bool retValue = EditorGUILayout.Toggle(new GUIContent(displayName, toolTip), value);
		bool thisModified = retValue != value;
		if (thisModified)
		{
			modified = true;
		}
		value = retValue;
		return thisModified;
	}
	// 直接显示一个int属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public int displayInt(string displayName, string toolTip, int value, ref bool modified)
	{
		int retValue = EditorGUILayout.IntField(new GUIContent(displayName, toolTip), value);
		if (retValue != value)
		{
			modified = true;
		}
		return retValue;
	}
	// 直接显示一个int属性,并且会改变这个变量本身,如果被修改过,则会设置modified为true,否则不改变modefied的值,返回值为是否改变过
	public bool displayInt(string displayName, string toolTip, ref int value, ref bool modified)
	{
		int retValue = EditorGUILayout.IntField(new GUIContent(displayName, toolTip), value);
		bool thisModified = retValue != value;
		if (thisModified)
		{
			modified = true;
		}
		value = retValue;
		return thisModified;
	}
	// 直接显示一个float属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public float displayFloat(string displayName, string toolTip, float value, ref bool modified)
	{
		float retValue = EditorGUILayout.FloatField(new GUIContent(displayName, toolTip), value);
		if (retValue != value)
		{
			modified = true;
		}
		return retValue;
	}
	// 直接显示一个float属性,并且会改变这个变量本身,如果被修改过,则会设置modified为true,否则不改变modefied的值,返回值为是否改变过
	public bool displayFloat(string displayName, string toolTip, ref float value, ref bool modified)
	{
		float retValue = EditorGUILayout.FloatField(new GUIContent(displayName, toolTip), value);
		bool thisModified = retValue != value;
		if (thisModified)
		{
			modified = true;
		}
		value = retValue;
		return thisModified;
	}
	// 直接显示一个枚举属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public T displayEnum<T>(string displayName, string toolTip, T value, ref bool modified) where T : Enum
	{
		Enum retValue = EditorGUILayout.EnumPopup(new GUIContent(displayName, toolTip), value);
		if (retValue.CompareTo(value) != 0)
		{
			modified = true;
		}
		return (T)retValue;
	}
	// 直接显示一个枚举属性,并且会改变这个变量本身,如果被修改过,则会设置modified为true,否则不改变modefied的值,返回值为是否改变过
	public bool displayEnum<T>(string displayName, string toolTip, ref T value, ref bool modified) where T : Enum
	{
		Enum retValue = EditorGUILayout.EnumPopup(new GUIContent(displayName, toolTip), value);
		bool thisModified = retValue.CompareTo(value) != 0;
		if (thisModified)
		{
			modified = true;
		}
		value = (T)retValue;
		return thisModified;
	}
	// 显示整数的下拉框
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