using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameKeyframe), true)]
public class EditorGameKeyframe : GameInspector
{
    protected override void onGUI()
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
			beginListContents();
			if (GUILayout.Button("X", GUILayout.Width(20)))
			{
				deleteKeyList ??= new();
				deleteKeyList.Add(item);
			}
			else
			{
				GUI.changed = false;
				string name = item.mName;
				if (name.isEmpty())
				{
					name = item.mID.ToString();
				}
				using (new GUILayout.VerticalScope())
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label("ID:" + item.mID.ToString(), GUILayout.Width(80));
						GUILayout.Label("Name:", GUILayout.Width(50));
						item.mName = EditorGUILayout.TextField(item.mName);
					}
					item.mCurve = EditorGUILayout.CurveField(item.mCurve, GUILayout.Height(20));
				}
				// 如果曲线有改动,则标记整个预设有改动
				if (GUI.changed)
				{
					EditorUtility.SetDirty(target);
				}
			}
			endListContents();
		}
		if (deleteKeyList == null)
		{
			return;
		}
		deleteKeyList.For(item => keyframe.destroyKeyframe(item));
		EditorUtility.SetDirty(target);
	}
}
