using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class EditorScaleAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		var anchor = target as ScaleAnchor;

		bool modified = false;
		modified |= toggle("保持宽高比", ref anchor.mKeepAspect);
		modified |= toggle("调整字体大小", ref anchor.mAdjustFont);
		modified |= displayInt("字体的最小尺寸", ref anchor.mMinFontSize);
		modified |= toggle("缩放时调整位置", ref anchor.mAdjustPosition);
		modified |= toggle("移除UGUI的锚点", ref anchor.mRemoveUGUIAnchor);
		if (anchor.mKeepAspect)
		{
			modified |= displayEnum("缩放方式", "", ref anchor.mAspectBase);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}