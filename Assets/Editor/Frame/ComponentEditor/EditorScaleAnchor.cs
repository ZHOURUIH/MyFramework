using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class EditorScaleAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		var anchor = target as ScaleAnchor;

		bool modified = false;
		modified |= toggle("KeepAspect", "是否保持宽高比进行缩放", ref anchor.mKeepAspect);
		modified |= toggle("AdjustFont", "是否同时调整字体大小", ref anchor.mAdjustFont);
		modified |= displayInt("MinFontSize", "字体的最小尺寸", ref anchor.mMinFontSize);
		modified |= toggle("AdjustPosition", "缩放时是否同时调整位置", ref anchor.mAdjustPosition);
		modified |= toggle("RemoveUGUIAnchor", "是否需要移除UGUI的锚点", ref anchor.mRemoveUGUIAnchor);
		if (anchor.mKeepAspect)
		{
			modified |= displayEnum("AspectBase", "缩放方式", ref anchor.mAspectBase);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}