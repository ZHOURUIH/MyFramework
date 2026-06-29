using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityUtility;

public class GameInspector : Editor
{
	public override void OnInspectorGUI()
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
	}
	public void setModify()
	{
		EditorUtility.SetDirty((target as Component).gameObject);
		Repaint();
	}
	protected virtual void onGUI() { }
	// 显示下拉列表,返回值表示是否已经修改过
	public static bool displayDropDown(string display, string tip, List<string> options, ref int selection, int width = 150)
	{
		GUIContent[] labels = new GUIContent[options.Count];
		int[] values = new int[options.Count];
		for (int i = 0; i < labels.Length; ++i)
		{
			values[i] = i;
			// 携带斜杠会导致下拉列表无法显示出双斜杠后面的文字,所以需要去除斜杠
			labels[i] = new(options[i].removeAll('/'));
		}
		int retValue = EditorGUILayout.IntPopup(new GUIContent(display, tip), selection, labels, values, GUILayout.Width(width));
		bool modified = retValue != selection;
		selection = retValue;
		return modified;
	}
	// 显示下拉列表,返回值表示是否已经修改过
	public static bool displayDropDown(string display, string tip, List<string> options, ref string selection, int width = 150)
	{
		GUIContent[] labels = new GUIContent[options.Count];
		int[] values = new int[options.Count];
		for (int i = 0; i < labels.Length; ++i)
		{
			values[i] = i;
			// 携带斜杠会导致下拉列表无法显示出双斜杠后面的文字,所以需要去除斜杠
			labels[i] = new(options[i].removeAll('/'));
		}
		int selectIndex = options.IndexOf(selection);
		int retValue = EditorGUILayout.IntPopup(new GUIContent(display, tip), selectIndex, labels, values, GUILayout.Width(width));
		bool modified = retValue != selectIndex;
		if (retValue >= 0 && retValue < options.Count)
		{
			selection = options[retValue];
		}
		return modified;
	}
	public static GameObject objectField(GameObject go, int width = 130)
	{
		return (GameObject)EditorGUILayout.ObjectField(go, typeof(GameObject), true, GUILayout.Width(width));
	}
	// 返回值表示是否修改过
	public static bool textField(ref string text, string labelText, int totalWidth = 100, int preLabelWidth = 50)
	{
		using (new GUILayout.HorizontalScope())
		{
			labelWidth(labelText, preLabelWidth);
			return textField(ref text, totalWidth - preLabelWidth);
		}
	}
	// 返回值表示是否修改过
	public static bool textField(ref string text, int width = 50)
	{
		string lastText = text;
		text = EditorGUILayout.TextField(text, GUILayout.Width(width));
		return lastText != text;
	}
	public static bool button(string text, int width = 100, int height = 20)
	{
		return button(text, "", width, height);
	}
	public static bool button(string text, string tip, int width = 100, int height = 20)
	{
		return GUILayout.Button(new GUIContent(text, tip), GUILayout.Width(width), GUILayout.Height(height));
	}
	public static void space(int pixel)
	{
		GUILayout.Space(pixel);
	}
	public static void label(string text)
	{
		GUILayout.Label(text);
	}
	public static void label(string text, int fontSize)
	{
		GUIStyle style = new(GUI.skin.label);
		style.fontSize = fontSize;
		GUILayout.Label(text, style);
	}
	public static void labelWidth(string text, int width)
	{
		GUIStyle style = new(GUI.skin.label);
		GUILayout.Label(text, style, GUILayout.Width(width));
	}
	public static void labelWidth(string text, int width, Color color)
	{
		GUIStyle style = new(GUI.skin.label);
		style.normal.textColor = color;
		GUILayout.Label(text, style, GUILayout.Width(width));
	}
	public static void labelWidth(string text, int width, string tip)
	{
		GUIStyle style = new(GUI.skin.label);
		GUILayout.Label(new GUIContent(text, tip), style, GUILayout.Width(width));
	}
	public static void label(string text, Color color)
	{
		GUIStyle style = new(GUI.skin.label);
		style.normal.textColor = color;
		GUILayout.Label(text, style);
	}
    public void label(string text0, string text1)
    {
        EditorGUILayout.LabelField(text0, text1);
    }
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
    public T objectField<T>(string label, UnityEngine.Object obj, params GUILayoutOption[] options) where T : UnityEngine.Object
    {
        return EditorGUILayout.ObjectField(label, obj, typeof(T), true, options) as T;
    }
    public void objectField<T>(string label, ref T obj, params GUILayoutOption[] options) where T : UnityEngine.Object
    {
        obj = EditorGUILayout.ObjectField(label, obj, typeof(T), true, options) as T;
    }
    public void space(float pixels = 10)
    {
        GUILayout.Space(pixels);
    }
    // 返回值表示是否修改过
    public static bool toggle(ref bool value, string text)
    {
        bool lastValue = value;
        value = GUILayout.Toggle(value, text);
        return value != lastValue;
    }
    // 返回值表示是否修改过
    public static bool toggle(ref bool value, string text, string tip)
    {
        bool lastValue = value;
        value = GUILayout.Toggle(value, new GUIContent(text, tip));
        return value != lastValue;
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
    // 直接显示一个float属性,并且会改变这个变量本身,返回值为是否改变过
    public bool displayFloat(string displayName, ref float value)
    {
        float retValue = EditorGUILayout.FloatField(new GUIContent(displayName), value);
        bool modified = retValue != value;
        value = retValue;
        return modified;
    }
    public bool displayVector3(string displayName, string toolTip, ref Vector3 value)
    {
        Vector3 retValue = EditorGUILayout.Vector3Field(new GUIContent(displayName, toolTip), value);
        bool modified = retValue != value;
        value = retValue;
        return modified;
    }
    public bool displayVector3(string displayName, ref Vector3 value)
    {
        Vector3 retValue = EditorGUILayout.Vector3Field(new GUIContent(displayName), value);
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
    // 直接显示一个枚举属性,返回值表示是否已经修改过
    public bool displayEnum<T>(string display, ref T value) where T : Enum
    {
        return GameEditorWindow.displayEnum(display, "", ref value);
    }
    // 显示整数的下拉框
    public int intPopup(string displayName, string[] valueDisplay, int[] values)
    {
        return EditorGUILayout.IntPopup(displayName, 0, valueDisplay, values);
    }
    // 开始绘制列表中的一项
    public void beginListContents()
    {
        EditorGUILayout.BeginHorizontal("box", GUILayout.MinHeight(10f));
        GUILayout.Space(10f);
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }
    // 结束绘制列表中的一项
    public void endListContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }
}