using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameEditorWindow : EditorWindow
{
	public void OnGUI()
	{
		onGUI();
		GUIUtility.ExitGUI();
	}
	protected virtual void onGUI() { }
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