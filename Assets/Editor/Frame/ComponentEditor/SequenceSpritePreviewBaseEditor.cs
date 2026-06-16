using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SequenceSpritePreviewBase), true)]
public class SequenceSpritePreviewBaseEditor : GameEditorBase
{
    private SerializedProperty mSlider;
    private SerializedProperty mFrames;
    private SerializedProperty mLoop;
    private SerializedProperty mFPS;
    private SerializedProperty mPreviewInEditor;
    private void OnEnable()
    {
        mSlider = serializedObject.FindProperty("mSlider");
        mFrames = serializedObject.FindProperty("mFrames");
        mLoop = serializedObject.FindProperty("mLoop");
        mFPS = serializedObject.FindProperty("mFPS");
        mPreviewInEditor = serializedObject.FindProperty("mPreviewInEditor");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SequenceSpritePreviewBase preview = target as SequenceSpritePreviewBase;

        space();
        label("序列帧预览");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(mPreviewInEditor, new GUIContent("编辑器中预览"));
        EditorGUILayout.PropertyField(mLoop, new GUIContent("循环播放"));
        EditorGUILayout.PropertyField(mFPS, new GUIContent("播放帧率"));
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

        using (new EditorGUILayout.HorizontalScope())
        {
            if (button("播放"))
            {
                Undo.RecordObject(preview, "Play Sequence Preview");
                preview.Play();
                EditorUtility.SetDirty(preview);
            }
            if (button("暂停"))
            {
                Undo.RecordObject(preview, "Pause Sequence Preview");
                preview.Pause();
                EditorUtility.SetDirty(preview);
            }
            if (button("继续"))
            {
                Undo.RecordObject(preview, "Resume Sequence Preview");
                preview.Resume();
                EditorUtility.SetDirty(preview);
            }
            if (button("停止"))
            {
                Undo.RecordObject(preview, "Stop Sequence Preview");
                preview.Stop();
                EditorUtility.SetDirty(preview);
            }
        }

        space();

        label("当前状态");

        int frameCount = preview != null ? preview.EditorGetFrameCount() : 0;
        int curFrame = preview != null ? preview.EditorGetCurFrame() : 0;
        bool playing = preview != null && preview.EditorIsPlaying();
        label("帧数量", frameCount.ToString());
        label("当前帧", frameCount > 0 ? $"{curFrame} / {frameCount - 1}" : "无");
        label("播放状态", playing ? "播放中" : "未播放");

        space();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(mFrames, new GUIContent("帧列表"), true);
        }

        serializedObject.ApplyModifiedProperties();
        if (preview != null && preview.EditorIsPlaying())
        {
            Repaint();
        }
    }
}