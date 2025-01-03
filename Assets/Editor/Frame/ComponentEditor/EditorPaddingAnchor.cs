using UnityEditor;
using UnityEngine;
using static MathUtility;

[CustomEditor(typeof(PaddingAnchor), true)]
public class EditorPaddingAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		var paddingAnchor = target as PaddingAnchor;

		bool modified = false;
		paddingAnchor.gameObject.TryGetComponent<RectTransform>(out var rectTrans);
		if (rectTrans != null && !Application.isPlaying)
		{
			bool needRefresh = false;
			Vector3 curPos = rectTrans.localPosition;
			if (!isVectorEqual(paddingAnchor.getLastPosition(), curPos))
			{
				paddingAnchor.setLastPosition(curPos);
				needRefresh = true;
				modified = true;
			}
			Vector2 curSize = rectTrans.rect.size;
			if (!isVectorEqual(paddingAnchor.getLastSize(), curSize))
			{
				paddingAnchor.setLastSize(curSize);
				needRefresh = true;
				modified = true;
			}
			if (needRefresh)
			{
				paddingAnchor.setAnchorMode(paddingAnchor.getAnchorMode());
			}
		}

		modified |= toggle("调整字体大小", ref paddingAnchor.mAdjustFont);
		modified |= displayInt("字体的最小尺寸", ref paddingAnchor.mMinFontSize);
		// 类似于EditorGUILayout.EnumPopup这种方式显示属性是用于可以在编辑器修改,并且触发指定逻辑
		ANCHOR_MODE anchorMode = displayEnum("停靠类型", paddingAnchor.mAnchorMode);
		if (anchorMode != paddingAnchor.mAnchorMode)
		{
			modified = true;
			paddingAnchor.setAnchorModeInEditor(anchorMode);
		}
		bool relativePos = toggle("是否存储相对值", paddingAnchor.mRelativeDistance);
		if (relativePos != paddingAnchor.mRelativeDistance)
		{
			modified = true;
			paddingAnchor.setRelativeDistanceInEditor(relativePos);
		}
		// 只是停靠到父节点的某个位置
		if (paddingAnchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			HORIZONTAL_PADDING horizontalPadding = displayEnum("水平停靠类型", "", paddingAnchor.mHorizontalNearSide);
			if (horizontalPadding != paddingAnchor.mHorizontalNearSide)
			{
				modified = true;
				paddingAnchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			VERTICAL_PADDING verticalPadding = displayEnum("竖直停靠类型", "", paddingAnchor.mVerticalNearSide);
			if (verticalPadding != paddingAnchor.mVerticalNearSide)
			{
				modified = true;
				paddingAnchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTER模式下才会显示mHorizontalPosition
			if (paddingAnchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty用于只是简单的使用默认方式显示属性,用于编辑器中直接修改值,不能触发任何其他逻辑
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTER模式下才会显示mVerticalPosition
			if (paddingAnchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// 显示边距离参数
			if (paddingAnchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				paddingAnchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
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