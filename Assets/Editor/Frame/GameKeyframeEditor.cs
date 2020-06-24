//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2018 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameKeyframe), true)]
public class GameKeyframeEditor : GameEditorBase
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		EditorGUIUtility.labelWidth = 110.0f;
		base.OnInspectorGUI();
		DrawCommonProperties();
	}

	protected void DrawCommonProperties ()
	{
		GameKeyframe keyframe = target as GameKeyframe;
		BeginContents();
		GUI.changed = false;
		EditorGUILayout.CurveField("Curve", keyframe.mCurve, GUILayout.Width(170f), GUILayout.Height(62f));
		EndContents();
	}
}
