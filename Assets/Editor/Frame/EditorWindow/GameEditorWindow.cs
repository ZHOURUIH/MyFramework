using System;
using UnityEditor;
using UnityEngine;
using static UnityUtility;

public class GameEditorWindow : EditorWindow
{
	public void OnGUI()
	{
		try
		{
			onGUI();
		}
		catch(Exception e)
		{
			Debug.Log("exception:" + e.Message + ", stack:" + e.StackTrace);
		}
		GUIUtility.ExitGUI();
	}
	protected virtual void onGUI() { }
	// 直接显示一个枚举属性,返回值表示是否已经修改过
	public bool displayEnum<T>(string display, string tip, ref T value) where T : Enum
	{
		string[] names = Enum.GetNames(typeof(T));
		GUIContent[] labels = new GUIContent[names.Length];
		int[] values = new int[names.Length];
		int valueIndex = 0;
		for(int i = 0; i < labels.Length; ++i)
		{
			values[i] = i;
			labels[i] = new GUIContent(getEnumLabel(typeof(T), names[i]), getEnumToolTip(typeof(T), names[i]));
			if (value.ToString() == names[i])
			{
				valueIndex = i;
			}
		}
		int retValue = EditorGUILayout.IntPopup(new GUIContent(display, tip), valueIndex, labels, values);
		bool modified = retValue != valueIndex;
		value = (T)Enum.Parse(typeof(T), names[retValue]);
		return modified;
	}
	// 返回值表示是否修改过
	protected bool textField(ref string text, string labelText, int width = 50)
	{
		GUILayout.BeginHorizontal();
		label(labelText);
		bool modified = textField(ref text, width);
		GUILayout.EndHorizontal();
		return modified;
	}
	// 返回值表示是否修改过
	protected bool textField(ref string text, int width = 50)
	{
		string lastText = text;
		text = GUILayout.TextField(text, GUILayout.Width(width));
		return lastText != text;
	}
	protected bool button(string text, int width = 50, int height = 20)
	{
		return button(text, "", width, height);
	}
	protected bool button(string text, string tip, int width = 50, int height = 20)
	{
		return GUILayout.Button(new GUIContent(text, tip), GUILayout.Width(width), GUILayout.Height(height));
	}
	// 返回值表示是否修改过
	protected bool toggle(ref bool value, string text)
	{
		bool lastValue = value;
		value = GUILayout.Toggle(value, text);
		return value != lastValue;
	}
	// 返回值表示是否修改过
	protected bool toggle(ref bool value, string text, string tip)
	{
		bool lastValue = value;
		value = GUILayout.Toggle(value, new GUIContent(text, tip));
		return value != lastValue;
	}
	protected void space(int pixel)
	{
		GUILayout.Space(pixel);
	}
	protected void label(string text)
	{
		GUILayout.Label(text);
	}
	protected void label(string text, int fontSize)
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.fontSize = fontSize;
		GUILayout.Label(text, style);
	}
	protected void label(string text, Color color)
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.normal.textColor = color;
		GUILayout.Label(text, style);
	}
	protected void beginHorizontal()
	{
		GUILayout.BeginHorizontal();
	}
	protected void endHorizontal()
	{
		GUILayout.EndHorizontal();
	}
	protected void beginVertical()
	{
		GUILayout.BeginVertical();
	}
	protected void endVertical()
	{
		GUILayout.EndVertical();
	}
}