using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaddingAnchor), true)]
public class PaddingAnchorEditor : GameEditorBase
{
	protected PaddingAnchor mPaddingAnchor;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		mPaddingAnchor = target as PaddingAnchor;

		bool modified = false;
		displayToggle("AdjustFont", "是否同时调整字体大小", ref mPaddingAnchor.mAdjustFont, ref modified);
		// 类似于EditorGUILayout.EnumPopup这种方式显示属性是用于可以在编辑器修改,并且触发指定逻辑
		var anchorMode = displayEnum("AnchorMode", "停靠类型", mPaddingAnchor.mAnchorMode, ref modified);
		if (anchorMode != mPaddingAnchor.mAnchorMode)
		{
			mPaddingAnchor.setAnchorModeInEditor(anchorMode);
		}
		var relativePos = displayToggle("RelativePosition", "存储使用相对值还是绝对值", mPaddingAnchor.mRelativeDistance, ref modified);
		if (relativePos != mPaddingAnchor.mRelativeDistance)
		{
			mPaddingAnchor.setRelativeDistanceInEditor(relativePos);
		}
		// 只是停靠到父节点的某个位置
		if (mPaddingAnchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			HORIZONTAL_PADDING horizontalPadding = displayEnum("HorizontalPaddingSide", "水平停靠类型", mPaddingAnchor.mHorizontalNearSide, ref modified);
			if (horizontalPadding != mPaddingAnchor.mHorizontalNearSide)
			{
				mPaddingAnchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			VERTICAL_PADDING verticalPadding = displayEnum("VerticalPaddingSide", "竖直停靠类型", mPaddingAnchor.mVerticalNearSide, ref modified);
			if (verticalPadding != mPaddingAnchor.mVerticalNearSide)
			{
				mPaddingAnchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTER模式下才会显示mHorizontalPosition
			if (mPaddingAnchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty用于只是简单的使用默认方式显示属性,用于编辑器中直接修改值,不能触发任何其他逻辑
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTER模式下才会显示mVerticalPosition
			if (mPaddingAnchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// 显示边距离参数
			if (mPaddingAnchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				mPaddingAnchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
			{
				displayProperty("mDistanceToBoard", "DistanceToBoard");
			}
		}
		else
		{
			displayProperty("mAnchorPoint", "AnchorPoint");
		}

		// 由于不确定hasModifiedProperties在ApplyModifiedProperties以后是否会改变,所以预先获取
		bool dirty = serializedObject.hasModifiedProperties || modified;
		serializedObject.ApplyModifiedProperties();
		if(dirty)
		{
			EditorUtility.SetDirty(target);
		}
	}
}