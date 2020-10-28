using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaddingAnchor), true)]
public class PaddingAnchorEditor : GameEditorBase
{
	private PaddingAnchor anchor;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		anchor = target as PaddingAnchor;
		anchor.mAdjustFont = displayToggle("AdjustFont", anchor.mAdjustFont, out bool adjustFontModified);
		// 类似于EditorGUILayout.EnumPopup这种方式显示属性是用于可以在编辑器修改,并且触发指定逻辑
		var anchorMode = (ANCHOR_MODE)displayEnum("AnchorMode", anchor.mAnchorMode, out bool anchorModeModified);
		if (anchorMode != anchor.mAnchorMode)
		{
			anchor.setAnchorModeInEditor(anchorMode);
		}
		var relativePos = displayToggle("RelativePosition", anchor.mRelativeDistance, out bool relativePosModified);
		if (relativePos != anchor.mRelativeDistance)
		{
			anchor.setRelativeDistanceInEditor(relativePos);
		}
		// 只是停靠到父节点的某个位置
		bool horiPaddingModified = false;
		bool vertPaddingModified = false;
		if (anchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			var horizontalPadding = (HORIZONTAL_PADDING)displayEnum("HorizontalPaddingSide", anchor.mHorizontalNearSide, out horiPaddingModified);
			if (horizontalPadding != anchor.mHorizontalNearSide)
			{
				anchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			var verticalPadding = (VERTICAL_PADDING)displayEnum("VerticalPaddingSide", anchor.mVerticalNearSide, out vertPaddingModified);
			if (verticalPadding != anchor.mVerticalNearSide)
			{
				anchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTER模式下才会显示mHorizontalPosition
			if (anchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty用于只是简单的使用默认方式显示属性,用于编辑器中直接修改值,不能触发任何其他逻辑
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTER模式下才会显示mVerticalPosition
			if (anchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// 显示边距离参数
			if (anchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				anchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
			{
				displayProperty("mDistanceToBoard", "DistanceToBoard");
			}
		}
		else
		{
			displayProperty("mAnchorPoint", "AnchorPoint");
		}
		// 由于不确定hasModifiedProperties在ApplyModifiedProperties以后是否会改变,所以预先获取
		bool dirty = serializedObject.hasModifiedProperties || 
					 adjustFontModified || 
					 anchorModeModified || 
					 relativePosModified || 
					 horiPaddingModified || 
					 vertPaddingModified;
		serializedObject.ApplyModifiedProperties();
		if(dirty)
		{
			EditorUtility.SetDirty(target);
		}
	}
}