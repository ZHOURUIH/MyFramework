using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class ScaleAnchorEditor : GameEditorBase
{
	private ScaleAnchor anchor;
	public override void OnInspectorGUI()
	{
		anchor = target as ScaleAnchor;
		anchor.mKeepAspect = displayToggle("KeepAspect", anchor.mKeepAspect, out bool keepAspectModified);
		anchor.mAdjustFont = displayToggle("AdjustFont", anchor.mAdjustFont, out bool adjustFountModified);
		bool aspectBaseModified = false;
		if (anchor.mKeepAspect)
		{
			var aspectBase = (ASPECT_BASE)displayEnum("AspectBase", anchor.mAspectBase, out aspectBaseModified);
			anchor.mAspectBase = aspectBase;
		}
		if(keepAspectModified || adjustFountModified || aspectBaseModified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}