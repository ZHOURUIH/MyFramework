using System;
using System.Collections.Generic;
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
		catch (ExitGUIException)
		{
			// 这个异常是正常现象,不认为是一个错误
		}
		catch (Exception e)
		{
			Debug.Log("exception:" + e.Message + ", stack:" + e.StackTrace);
		}
		GUIUtility.ExitGUI();
	}
	protected virtual void onGUI() { }
	// 显示下拉列表,返回值表示是否已经修改过
	public static bool displayDropDown(string display, string tip, List<string> options, ref int selection)
	{
		GUIContent[] labels = new GUIContent[options.Count];
		int[] values = new int[options.Count];
		for (int i = 0; i < labels.Length; ++i)
		{
			values[i] = i;
			// 携带斜杠会导致下拉列表无法显示出双斜杠后面的文字,所以需要去除斜杠
			labels[i] = new(options[i].removeAll('/'));
		}
		int retValue = EditorGUILayout.IntPopup(new GUIContent(display, tip), selection, labels, values);
		bool modified = retValue != selection;
		selection = retValue;
		return modified;
	}
	// 直接显示一个枚举属性,返回值表示是否已经修改过
	public static bool displayEnum<T>(string display, string tip, ref T value) where T : Enum
	{
		string[] names = Enum.GetNames(typeof(T));
		GUIContent[] labels = new GUIContent[names.Length];
		int[] values = new int[names.Length];
		int valueIndex = 0;
		for(int i = 0; i < labels.Length; ++i)
		{
			string name = names[i];
			values[i] = i;
			labels[i] = new(getEnumLabel(typeof(T), name), getEnumToolTip(typeof(T), name));
			if (value.ToString() == name)
			{
				valueIndex = i;
			}
		}
		int retValue = EditorGUILayout.IntPopup(new GUIContent(display, tip), valueIndex, labels, values);
		value = (T)Enum.Parse(typeof(T), names[retValue]);
		return retValue != valueIndex;
	}
	// 返回值表示是否修改过
	protected bool textField(ref string text, string labelText, int width = 50)
	{
		using (new GUILayout.HorizontalScope())
		{
			label(labelText);
			return textField(ref text, width);
		}
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
		GUIStyle style = new(GUI.skin.label);
		style.fontSize = fontSize;
		GUILayout.Label(text, style);
	}
	protected void labelWidth(string text, int width)
	{
		GUIStyle style = new(GUI.skin.label);
		GUILayout.Label(text, style, GUILayout.Width(width));
	}
	protected void labelWidth(string text, int width, string tip)
	{
		GUIStyle style = new(GUI.skin.label);
		GUILayout.Label(new GUIContent(text, tip), style, GUILayout.Width(width));
	}
	protected void label(string text, Color color)
	{
		GUIStyle style = new(GUI.skin.label);
		style.normal.textColor = color;
		GUILayout.Label(text, style);
	}
	protected void beginHorizontal(params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal(options);
	}
	protected void endHorizontal()
	{
		GUILayout.EndHorizontal();
	}
	protected void beginVertical(params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical(options);
	}
	protected void endVertical()
	{
		GUILayout.EndVertical();
	}
}