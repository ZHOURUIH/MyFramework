using System;
using UnityEngine;
using static FrameBaseUtility;
using static UnityUtility;
using static WidgetUtility;
using static MathUtility;

// 该组件所在的物体不能有旋转,否则会计算错误
// 用于实现窗口的停靠或者四条边的单独控制
public class ResPaddingAnchor : MonoBehaviour
{
	protected bool mDirty = true;										// 是否需要刷新数据
	protected Vector3[] mParentSides;									// 父节点的四条边中心点的坐标
	protected Vector3 mLastPosition;									// 上一次刷新参数时的节点位置,用于判断是否需要刷新参数
	protected Vector2 mLastSize;										// 上一次刷新参数时的节点大小,用于判断是否需要刷新参数
	// 以下是保存的参数属性
	public HORIZONTAL_PADDING mHorizontalNearSide;						// 横向停靠方式
	public VERTICAL_PADDING mVerticalNearSide;							// 纵向停靠方式
	public float mHorizontalPositionRelative;							// 横向停靠的百分比
	public int mHorizontalPositionAbsolute;								// 横向停靠的绝对值
	public float mVerticalPositionRelative;								// 纵向停靠的百分比
	public int mVerticalPositionAbsolute;								// 纵向停靠的绝对值
	public int mMinFontSize = 12;										// 字体的最小大小
	public bool mRelativeDistance;										// 是否使用相对距离
	public bool mAdjustFont = true;										// 是否连字体大小也一起调整
	// 边相对于父节点对应边的距离,Relative是相对于宽或者高的一半,范围0-1,从0到1是对应从中心到各边,Absolute是Relative计算完以后的偏移量,带正负
	public ComplexPoint[] mDistanceToBoard;								// mAnchorMode为PADDING_PARENT_SIDE使用的,左上右下,横向中心,纵向中心的顺序
	public ResPaddingAnchor()
	{
		mParentSides = new Vector3[4];
		mDistanceToBoard = new ComplexPoint[4] { new(), new(), new(), new() };
	}
	public void OnValidate()
	{
		if (isEditor())
		{
			setToPaddingParentSide(mHorizontalNearSide, mVerticalNearSide, mRelativeDistance);
		}
	}
	public void updateRect(bool force = false)
	{
		if (!force && !mDirty)
		{
			return;
		}
		// 如果窗口带缩放,则可能适配不正确
		if (transform.localScale != Vector3.one)
		{
			logWarningBase("transform's scale is not 1, may not adapt correctely, " + transform.name + ", scale:" + transform.localScale);
		}
		mDirty = false;
		TryGetComponent<RectTransform>(out var rectTransform);
		Vector2 newSize = rectTransform.rect.size;
		GameObject parent = transform.parent.gameObject;
		parent.TryGetComponent<RectTransform>(out var parentTrans);
		Vector2 parentSize = parentTrans.rect.size;
		Vector3 pos = transform.localPosition;
		if (parent != null)
		{
			getParentSides(parent, mParentSides);
			// 仅仅停靠到父节点的某条边,只需要根据当前大小和父节点大小计算位置
			// 横向位置
			if (mHorizontalNearSide == HORIZONTAL_PADDING.LEFT_IN)
			{
				pos.x = mDistanceToBoard[0].mRelative * mParentSides[0].x + mDistanceToBoard[0].mAbsolute + newSize.x * 0.5f;
			}
			else if (mHorizontalNearSide == HORIZONTAL_PADDING.LEFT_OUT)
			{
				pos.x = mDistanceToBoard[0].mRelative * mParentSides[0].x + mDistanceToBoard[0].mAbsolute - newSize.x * 0.5f;
			}
			else if (mHorizontalNearSide == HORIZONTAL_PADDING.RIGHT_IN)
			{
				pos.x = mDistanceToBoard[2].mRelative * mParentSides[2].x + mDistanceToBoard[2].mAbsolute - newSize.x * 0.5f;
			}
			else if (mHorizontalNearSide == HORIZONTAL_PADDING.RIGHT_OUT)
			{
				pos.x = mDistanceToBoard[2].mRelative * mParentSides[2].x + mDistanceToBoard[2].mAbsolute + newSize.x * 0.5f;
			}
			else if (mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				pos.x = mHorizontalPositionRelative * parentSize.x * 0.5f + mHorizontalPositionAbsolute;
			}
			// 纵向位置
			if (mVerticalNearSide == VERTICAL_PADDING.TOP_IN)
			{
				pos.y = mDistanceToBoard[1].mRelative * mParentSides[1].y + mDistanceToBoard[1].mAbsolute - newSize.y * 0.5f;
			}
			else if (mVerticalNearSide == VERTICAL_PADDING.TOP_OUT)
			{
				pos.y = mDistanceToBoard[1].mRelative * mParentSides[1].y + mDistanceToBoard[1].mAbsolute + newSize.y * 0.5f;
			}
			else if (mVerticalNearSide == VERTICAL_PADDING.BOTTOM_IN)
			{
				pos.y = mDistanceToBoard[3].mRelative * mParentSides[3].y + mDistanceToBoard[3].mAbsolute + newSize.y * 0.5f;
			}
			else if (mVerticalNearSide == VERTICAL_PADDING.BOTTOM_OUT)
			{
				pos.y = mDistanceToBoard[3].mRelative * mParentSides[3].y + mDistanceToBoard[3].mAbsolute - newSize.y * 0.5f;
			}
			else if (mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				pos.y = mVerticalPositionRelative * parentSize.y * 0.5f + mVerticalPositionAbsolute;
			}
		}
		if (newSize.x < 0)
		{
			logErrorBase("width:" + newSize.x + " is not valid, consider to modify the PaddingAnchor! " + gameObject.name + ", parent:" + gameObject.transform.parent.name);
		}
		if (newSize.y < 0)
		{
			logErrorBase("height:" + newSize.y + " is not valid, consider to modify the PaddingAnchor! " + gameObject.name + ", parent:" + gameObject.transform.parent.name);
		}
		if (mAdjustFont)
		{
			setRectSizeWithFontSize(rectTransform, newSize, mMinFontSize);
		}
		else
		{
			setRectSize(rectTransform, newSize);
		}
		transform.localPosition = round(pos);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected ComplexPoint sideToAbsolute(float thisSide, float parentSide)
	{
		ComplexPoint point = new();
		point.mRelative = 1.0f;
		point.setAbsolute(thisSide - parentSide);
		return point;
	}
	protected ComplexPoint sideToRelative(float thisSide, float parentSide)
	{
		ComplexPoint point = new();
		point.mRelative = Mathf.Approximately(parentSide, 0.0f) ? 0.0f : Mathf.Abs(thisSide / parentSide);
		point.setAbsolute(0.0f);
		return point;
	}
	// 停靠父节点的指定边界,并且大小不改变
	protected void setToPaddingParentSide(HORIZONTAL_PADDING horizontalSide, VERTICAL_PADDING verticalSide, bool relativeDistance)
	{
		Span<Vector3> sides = stackalloc Vector3[4];
		Vector2 pos = transform.localPosition;
		GameObject parent = transform.parent.gameObject;
		parent.TryGetComponent<RectTransform>(out var parentTrans);
		Vector2 parentSize = parentTrans.rect.size;
		if (parent != null)
		{
			getSides(parent, sides);
			getParentSides(parent, mParentSides);
		}
		mDistanceToBoard[0] = new();
		mDistanceToBoard[1] = new();
		mDistanceToBoard[2] = new();
		mDistanceToBoard[3] = new();
		float parentLeft = mParentSides[0].x;
		float parentRight = mParentSides[2].x;
		float parentTop = mParentSides[1].y;
		float parentBottom = mParentSides[3].y;
		float thisLeft = sides[0].x;
		float thisRight = sides[2].x;
		float thisTop = sides[1].y;
		float thisBottom = sides[3].y;
		if (relativeDistance)
		{
			// 相对于左右边界
			// 计算当前窗口的左边界相对于父节点左边界的相对距离的绝对值或者比例
			if (horizontalSide == HORIZONTAL_PADDING.LEFT_IN)
			{
				mDistanceToBoard[0] = sideToRelative(thisLeft, parentLeft);
			}
			// 计算当前窗口的右边界相对于父节点左边界的相对距离的绝对值或者比例
			else if (horizontalSide == HORIZONTAL_PADDING.LEFT_OUT)
			{
				mDistanceToBoard[0] = sideToRelative(thisRight, parentLeft);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.RIGHT_IN)
			{
				mDistanceToBoard[2] = sideToRelative(thisRight, parentRight);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.RIGHT_OUT)
			{
				mDistanceToBoard[2] = sideToRelative(thisLeft, parentRight);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.CENTER)
			{
				mHorizontalPositionRelative = Mathf.Approximately(parentSize.x, 0.0f) ? 0.0f : pos.x / (parentSize.x * 0.5f);
				mHorizontalPositionAbsolute = 0;
			}
			if (verticalSide == VERTICAL_PADDING.TOP_IN)
			{
				mDistanceToBoard[1] = sideToRelative(thisTop, parentTop);
			}
			else if (verticalSide == VERTICAL_PADDING.TOP_OUT)
			{
				mDistanceToBoard[1] = sideToRelative(thisBottom, parentTop);
			}
			else if (verticalSide == VERTICAL_PADDING.BOTTOM_IN)
			{
				mDistanceToBoard[3] = sideToRelative(thisBottom, parentBottom);
			}
			else if (verticalSide == VERTICAL_PADDING.BOTTOM_OUT)
			{
				mDistanceToBoard[3] = sideToRelative(thisTop, parentBottom);
			}
			else if (verticalSide == VERTICAL_PADDING.CENTER)
			{
				mVerticalPositionRelative = Mathf.Approximately(parentSize.y, 0.0f) ? 0.0f : pos.y / (parentSize.y * 0.5f);
				mVerticalPositionAbsolute = 0;
			}
		}
		else
		{
			// 相对于左右边界
			// 计算当前窗口的左边界相对于父节点左边界的相对距离的绝对值或者比例
			if (horizontalSide == HORIZONTAL_PADDING.LEFT_IN)
			{
				mDistanceToBoard[0] = sideToAbsolute(thisLeft, parentLeft);
			}
			// 计算当前窗口的右边界相对于父节点左边界的相对距离的绝对值或者比例
			else if (horizontalSide == HORIZONTAL_PADDING.LEFT_OUT)
			{
				mDistanceToBoard[0] = sideToAbsolute(thisRight, parentLeft);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.RIGHT_IN)
			{
				mDistanceToBoard[2] = sideToAbsolute(thisRight, parentRight);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.RIGHT_OUT)
			{
				mDistanceToBoard[2] = sideToAbsolute(thisLeft, parentRight);
			}
			else if (horizontalSide == HORIZONTAL_PADDING.CENTER)
			{
				mHorizontalPositionRelative = 0.0f;
				mHorizontalPositionAbsolute = (int)(pos.x + 0.5f * Mathf.Sign(pos.x));
			}
			if (verticalSide == VERTICAL_PADDING.TOP_IN)
			{
				mDistanceToBoard[1] = sideToAbsolute(thisTop, parentTop);
			}
			else if (verticalSide == VERTICAL_PADDING.TOP_OUT)
			{
				mDistanceToBoard[1] = sideToAbsolute(thisBottom, parentTop);
			}
			else if (verticalSide == VERTICAL_PADDING.BOTTOM_IN)
			{
				mDistanceToBoard[3] = sideToAbsolute(thisBottom, parentBottom);
			}
			else if (verticalSide == VERTICAL_PADDING.BOTTOM_OUT)
			{
				mDistanceToBoard[3] = sideToAbsolute(thisTop, parentBottom);
			}
			else if (verticalSide == VERTICAL_PADDING.CENTER)
			{
				mVerticalPositionRelative = 0.0f;
				mVerticalPositionAbsolute = (int)(pos.y + 0.5f * Mathf.Sign(pos.y));
			}
		}
	}
	// 获取当前节点在父节点坐标系下的的各条边
	protected void getSides(GameObject parent, Span<Vector3> sides)
	{
		TryGetComponent<RectTransform>(out var rectTransform);
		if (rectTransform.pivot != new Vector2(0.5f, 0.5f))
		{
			logErrorBase("UI的pivot错误:" + rectTransform.name);
		}
		Span<Vector3> localCorners = stackalloc Vector3[4];
		generateLocalCorners(parent, localCorners);
		cornerToSide(localCorners, sides);
	}
	protected void generateLocalCorners(GameObject parent, Span<Vector3> localCorners, bool includeRotation = false)
	{
		if (localCorners.Length != 4)
		{
			return;
		}
		TryGetComponent<RectTransform>(out var rectTransform);
		// 去除旋转
		Quaternion lastQuat = rectTransform.rotation;
		if (!includeRotation)
		{
			rectTransform.rotation = Quaternion.identity;
		}
		Vector2 size = rectTransform.rect.size;
		localCorners[0] = new(-size.x * 0.5f, -size.y * 0.5f);
		localCorners[1] = new(-size.x * 0.5f, size.y * 0.5f);
		localCorners[2] = new(size.x * 0.5f, size.y * 0.5f);
		localCorners[3] = new(size.x * 0.5f, -size.y * 0.5f);
		for (int i = 0; i < 4; ++i)
		{
			localCorners[i] = worldToLocal(parent.transform, localToWorld(rectTransform, localCorners[i]));
		}
		if (!includeRotation)
		{
			rectTransform.rotation = lastQuat;
		}
	}
}