using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class ScaleAnchorEditor : GameEditorBase
{
	private ScaleAnchor anchor;
	public override void OnInspectorGUI()
	{
		anchor = target as ScaleAnchor;
		anchor.mKeepAspect = displayToggle("KeepAspect", anchor.mKeepAspect);
		anchor.mAdjustFont = displayToggle("AdjustFont", anchor.mAdjustFont);
		if (anchor.mKeepAspect)
		{
			var aspectBase = (ASPECT_BASE)displayEnum("AspectBase", anchor.mAspectBase);
			anchor.mAspectBase = aspectBase;
		}
	}
}