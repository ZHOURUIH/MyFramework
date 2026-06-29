#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// 用于实现名字的渲染
[CustomPropertyDrawer(typeof(CustomLabelAttribute))]
public class CustomLabelDrawer : PropertyDrawer
{
	protected GUIContent mLabel;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		mLabel ??= new((attribute as CustomLabelAttribute).getLabel());
		EditorGUI.PropertyField(position, property, mLabel);
	}
}
#endif