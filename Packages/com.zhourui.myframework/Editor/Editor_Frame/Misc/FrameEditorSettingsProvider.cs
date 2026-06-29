using UnityEditor;
using UnityEngine.UIElements;
using static EditorDefine;

public class FrameEditorSettingsProvider : SettingsProvider
{
    private static FrameEditorSettingsProvider mProvider;
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        if (mProvider == null)
        {
            mProvider = new FrameEditorSettingsProvider();
            using var so = new SerializedObject(FrameEditorSettings.getInstance());
            mProvider.keywords = GetSearchKeywordsFromSerializedObject(so);
        }
        return mProvider;
    }
    private SerializedObject mSerializedObject;
    public FrameEditorSettingsProvider() : base(SETTING_NAME, SettingsScope.Project){}
    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        InitGUI();
    }
    public override void OnDeactivate()
    {
        base.OnDeactivate();
        FrameEditorSettings.save();
    }
    private void InitGUI()
    {
        var setting = FrameEditorSettings.getInstance();
        mSerializedObject?.Dispose();
        mSerializedObject = new(setting);
    }
    public override void OnGUI(string searchContext)
    {
        if (mSerializedObject == null || !mSerializedObject.targetObject)
        {
            InitGUI();
        }

        mSerializedObject.Update();

        EditorGUI.BeginChangeCheck();
        SerializedProperty property = mSerializedObject.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            // 如果你已经不用这一层,就不要显示它
            if (property.name == "m_Script" || property.name == "EditorSettings")
            {
                continue;
            }
            EditorGUILayout.PropertyField(property, true);
        }
        if (EditorGUI.EndChangeCheck())
        {
            mSerializedObject.ApplyModifiedProperties();
            FrameEditorSettings.save();
        }
    }
}