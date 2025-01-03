using System;
using UnityEngine;
using UnityEditor;

public class GameEditorBase : Editor
{
	public SerializedProperty findProperty(string name)
	{
		SerializedProperty property = serializedObject.FindProperty(name);
		if (property == null)
		{
			Debug.LogError("找不到属性:" + name + ",确保属性存在并且访问权限为public");
		}
		return property;
	}
	public void displayProperty(string propertyName, string displayName, string toolTip = "", bool includeChildren = true)
	{
		SerializedProperty property = findProperty(propertyName);
		EditorGUILayout.PropertyField(property, new(displayName, toolTip), includeChildren);
	}
	// 直接显示一个bool属性,返回修改以后的值
	public bool toggle(string displayName, bool value)
	{
		return toggle(displayName, "", value);
	}
	// 直接显示一个bool属性,返回修改以后的值
	public bool toggle(string displayName, string toolTip, bool value)
	{
		return EditorGUILayout.Toggle(new GUIContent(displayName, toolTip), value);
	}
	// 直接显示一个bool属性,并且会改变这个变量本身,返回值为是否改变过
	public bool toggle(string displayName, ref bool value)
	{
		return toggle(displayName, "", ref value);
	}
	// 直接显示一个bool属性,并且会改变这个变量本身,返回值为是否改变过
	public bool toggle(string displayName, string toolTip, ref bool value)
	{
		bool retValue = EditorGUILayout.Toggle(new GUIContent(displayName, toolTip), value);
		bool modified = retValue != value;
		value = retValue;
		return modified;
	}
	// 直接显示一个int属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public int displayInt(string displayName, string toolTip, int value, ref bool modified)
	{
		int retValue = EditorGUILayout.IntField(new GUIContent(displayName, toolTip), value);
		modified |= retValue != value;
		return retValue;
	}
	// 直接显示一个int属性,并且会改变这个变量本身,返回值为是否改变过
	public bool displayInt(string displayName, ref int value)
	{
		return displayInt(displayName, "", ref value);
	}
	// 直接显示一个int属性,并且会改变这个变量本身,返回值为是否改变过
	public bool displayInt(string displayName, string toolTip, ref int value)
	{
		int retValue = EditorGUILayout.IntField(new GUIContent(displayName, toolTip), value);
		bool modified = retValue != value;
		value = retValue;
		return modified;
	}
	// 直接显示一个float属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public float displayFloat(string displayName, string toolTip, float value, ref bool modified)
	{
		float retValue = EditorGUILayout.FloatField(new GUIContent(displayName, toolTip), value);
		modified |= retValue != value;
		return retValue;
	}
	// 直接显示一个float属性,并且会改变这个变量本身,返回值为是否改变过
	public bool displayFloat(string displayName, string toolTip, ref float value)
	{
		float retValue = EditorGUILayout.FloatField(new GUIContent(displayName, toolTip), value);
		bool modified = retValue != value;
		value = retValue;
		return modified;
	}
	// 直接显示一个枚举属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public T displayEnum<T>(string displayName, T value, ref bool modified) where T : Enum
	{
		Enum retValue = value;
		GameEditorWindow.displayEnum(displayName, "", ref retValue);
		modified |= retValue.CompareTo(value) != 0;
		return (T)retValue;
	}
	// 直接显示一个枚举属性,返回修改以后的值,如果被修改过,则会设置modified为true,否则不改变modefied的值
	public T displayEnum<T>(string displayName, string toolTip, T value, ref bool modified) where T : Enum
	{
		Enum retValue = value;
		GameEditorWindow.displayEnum(displayName, toolTip, ref retValue);
		modified |= retValue.CompareTo(value) != 0;
		return (T)retValue;
	}
	// 直接显示一个枚举属性,返回修改以后的值
	public T displayEnum<T>(string displayName, T value) where T : Enum
	{
		return displayEnum(displayName, "", value);
	}
	// 直接显示一个枚举属性,返回修改以后的值
	public T displayEnum<T>(string displayName, string toolTip, T value) where T : Enum
	{
		GameEditorWindow.displayEnum(displayName, toolTip, ref value);
		return value;
	}
	// 直接显示一个枚举属性,返回值表示是否已经修改过
	public bool displayEnum<T>(string display, string tip, ref T value) where T : Enum
	{
		return GameEditorWindow.displayEnum(display, tip, ref value);
	}
	// 显示整数的下拉框
	public int intPopup(string displayName, string[] valueDisplay, int[] values)
	{
		return EditorGUILayout.IntPopup(displayName, 0, valueDisplay, values);
	}
	public void beginContents()
	{
		EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
		GUILayout.Space(10f);
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}
	public void endContents()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(3f);
	}
}