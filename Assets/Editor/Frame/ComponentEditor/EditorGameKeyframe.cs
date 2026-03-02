using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameKeyframe), true)]
public class EditorGameKeyframe : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		GUILayout.Space(6f);
		EditorGUIUtility.labelWidth = 110.0f;
		base.OnInspectorGUI();

		if (GUILayout.Button("创建"))
		{
			var keyframe = target as GameKeyframe;
			keyframe.createKeyframe();
			EditorUtility.SetDirty(target);
		}

		DrawCommonProperties();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void DrawCommonProperties()
	{
		List<CurveInfo> deleteKeyList = null;
		var keyframe = target as GameKeyframe;
		foreach (CurveInfo item in keyframe.mCurveList.safe())
		{
			beginContents();
			if (GUILayout.Button("X", GUILayout.Width(20)))
			{
				deleteKeyList ??= new();
				deleteKeyList.Add(item);
			}
			else
			{
				GUI.changed = false;
				EditorGUILayout.CurveField(item.mID.ToString(), item.mCurve, GUILayout.Width(170f), GUILayout.Height(62f));
				// 如果曲线有改动,则标记整个预设有改动
				if (GUI.changed)
				{
					EditorUtility.SetDirty(target);
				}
			}
			endContents();
		}
		if (deleteKeyList == null)
		{
			return;
		}
		deleteKeyList.For(item => keyframe.destroyKeyframe(item));
		EditorUtility.SetDirty(target);
	}
}
