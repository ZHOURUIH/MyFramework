using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SequenceSpritePreviewBase), true)]
[CanEditMultipleObjects]
public class SequenceSpritePreviewBaseEditor : GameInspector
{
	private SerializedProperty mSlider;
	private SerializedProperty mFrames;
	private void OnEnable()
	{
		mSlider = serializedObject.FindProperty("mSlider");
		mFrames = serializedObject.FindProperty("mFrames");
	}
	protected override void onGUI()
	{
		serializedObject.Update();

		SequenceSpritePreviewBase preview = target as SequenceSpritePreviewBase;

		space();
		label("序列帧预览");

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(mSlider, new GUIContent("预览进度"));
		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
			if (preview != null)
			{
				Undo.RecordObject(preview, "Refresh Sequence Preview");
				preview.EditorRefreshBySlider();
				EditorUtility.SetDirty(preview);
			}
			return;
		}

		space();

		using (new EditorGUILayout.HorizontalScope())
		{
			if (button("刷新帧列表"))
			{
				Undo.RecordObject(preview, "Reload Sequence Frames");
				preview.EditorReloadFrames();
				EditorUtility.SetDirty(preview);
			}
			if (button("上一帧"))
			{
				Undo.RecordObject(preview, "Previous Sequence Frame");
				preview.EditorPreviousFrame();
				EditorUtility.SetDirty(preview);
			}
			if (button("下一帧"))
			{
				Undo.RecordObject(preview, "Next Sequence Frame");
				preview.EditorNextFrame();
				EditorUtility.SetDirty(preview);
			}
		}

		space();

		label("当前状态");

		int frameCount = preview != null ? preview.EditorGetFrameCount() : 0;
		int curFrame = preview != null ? preview.EditorGetCurFrame() : 0;
		label("帧数量", frameCount.ToString());
		label("当前帧", frameCount > 0 ? $"{curFrame} / {frameCount - 1}" : "无");

		space();

		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.PropertyField(mFrames, new GUIContent("帧列表"), true);
		}

		serializedObject.ApplyModifiedProperties();
	}
}