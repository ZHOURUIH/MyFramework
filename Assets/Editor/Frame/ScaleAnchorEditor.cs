using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class ScaleAnchorEditor : GameEditorBase
{
	private ScaleAnchor anchor;
	public override void OnInspectorGUI()
	{
		anchor = target as ScaleAnchor;

		bool modified = false;
		displayToggle("KeepAspect", "是否保持宽高比进行缩放", ref anchor.mKeepAspect, ref modified);
		displayToggle("AdjustFont", "是否同时调整字体大小", ref anchor.mAdjustFont, ref modified);
		displayToggle("AdjustPosition", "缩放时是否同时调整位置", ref anchor.mAdjustPosition, ref modified);
		displayToggle("RemoveUGUIAnchor", "是否需要移除UGUI的锚点", ref anchor.mRemoveUGUIAnchor, ref modified);
		if (anchor.mKeepAspect)
		{
			displayEnum("AspectBase", "缩放方式", ref anchor.mAspectBase, ref modified);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}