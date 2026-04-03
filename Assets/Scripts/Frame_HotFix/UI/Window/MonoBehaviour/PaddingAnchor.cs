using System;
using UnityEngine;
using static FrameBaseUtility;
using static UnityUtility;
using static WidgetUtility;
using static MathUtility;
using static FrameBaseDefine;
using static StringUtility;

// 该组件所在的物体不能有旋转,否则会计算错误
// 用于实现窗口的停靠或者四条边的单独控制
public class PaddingAnchor : MonoBehaviour
{
	protected bool mDirty = true;										// 是否需要刷新数据
	protected Vector3[] mParentSides;									// 父节点的四条边中心点的坐标
	protected Vector3 mLastPosition;									// 上一次刷新参数时的节点位置,用于判断是否需要刷新参数
	protected Vector2 mLastSize;										// 上一次刷新参数时的节点大小,用于判断是否需要刷新参数
	// 以下是保存的参数属性
	public ANCHOR_MODE mAnchorMode = ANCHOR_MODE.PADDING_PARENT_SIDE;	// 停靠方式
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
	public ComplexPoint[] mAnchorPoint;									// mAnchorMode为STRETCH_TO_PARENT_SIDE使用的,左上右下,横向中心,纵向中心的顺序
	public PaddingAnchor()
	{
		mParentSides = new Vector3[4];
		mDistanceToBoard = new ComplexPoint[4] { new(), new(), new(), new() };
		mAnchorPoint = new ComplexPoint[4] { new(), new(), new(), new() };
	}
	public void Awake()
	{
		if (Application.isPlaying)
		{
			enabled = false;
		}
	}
	public void setAnchorModeInEditor(ANCHOR_MODE mode)
	{
		if (isEditor())
		{
			mAnchorMode = mode;
			setAnchorMode(mAnchorMode);
		}
	}
	public void setHorizontalNearSideInEditor(HORIZONTAL_PADDING side)
	{
		if (isEditor())
		{
			mHorizontalNearSide = side;
			setAnchorMode(mAnchorMode);
		}
	}
	public void setVerticalNearSideInEditor(VERTICAL_PADDING side)
	{
		if (isEditor())
		{
			mVerticalNearSide = side;
			setAnchorMode(mAnchorMode);
		}
	}
	public void setRelativeDistanceInEditor(bool relativeDistance)
	{
		if (isEditor())
		{
			mRelativeDistance = relativeDistance;
			setAnchorMode(mAnchorMode);
		}
	}
	public void Reset()
	{
		// 挂载该脚本时候检查当前GameView的分辨率是否是标准分辨率
		Vector2 screenSize = getGameViewSize();
		if ((int)screenSize.x != STANDARD_WIDTH || (int)screenSize.y != STANDARD_HEIGHT)
		{
			displayDialog("错误", "当前分辨率不是标准分辨率,适配结果可能不对,请将Game视图的分辨率修改为" +
				STANDARD_WIDTH + "*" + STANDARD_HEIGHT, "确定");
			DestroyImmediate(this);
		}
	}
	public ANCHOR_MODE getAnchorMode() { return mAnchorMode; }
	public void setAnchorMode(ANCHOR_MODE mode)
	{
		mAnchorMode = mode;
		if (mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			setToPaddingParentSide(mHorizontalNearSide, mVerticalNearSide, mRelativeDistance);
		}
		else if (mAnchorMode == ANCHOR_MODE.STRETCH_TO_PARENT_SIDE)
		{
			setToNearParentSides(mRelativeDistance);
		}
	}
	public void updateRect(bool force = false)
	{
		if (!force && !mDirty)
		{
			return;
		}
		// 如果窗口带缩放,则可能适配不正确
		if (!isVectorZero(transform.localScale - Vector3.one))
		{
			logWarning("transform's scale is not 1, may not adapt correctly, " + transform.name + ", scale:" + V3ToS(transform.localScale, 6));
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
			if (mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
			{
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
			// 根据锚点和父节点大小计算各条边的值
			else if (mAnchorMode == ANCHOR_MODE.STRETCH_TO_PARENT_SIDE)
			{
				float thisLeft = mAnchorPoint[0].mRelative * mParentSides[0].x + mAnchorPoint[0].mAbsolute;
				float thisRight = mAnchorPoint[2].mRelative * mParentSides[2].x + mAnchorPoint[2].mAbsolute;
				float thisTop = mAnchorPoint[1].mRelative * mParentSides[1].y + mAnchorPoint[1].mAbsolute;
				float thisBottom = mAnchorPoint[3].mRelative * mParentSides[3].y + mAnchorPoint[3].mAbsolute;
				newSize.x = thisRight - thisLeft;
				newSize.y = thisTop - thisBottom;
				pos.x = (thisRight + thisLeft) * 0.5f;
				pos.y = (thisTop + thisBottom) * 0.5f;
			}
		}
		if (newSize.x < 0)
		{
			logError("width:" + newSize.x + " is not valid, consider to modify the PaddingAnchor! " + gameObject.name + ", parent:" + gameObject.transform.parent.name);
		}
		if (newSize.y < 0)
		{
			logError("height:" + newSize.y + " is not valid, consider to modify the PaddingAnchor! " + gameObject.name + ", parent:" + gameObject.transform.parent.name);
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
	public Vector3 getLastPosition() { return mLastPosition; }
	public Vector2 getLastSize() { return mLastSize; }
	public void setLastPosition(Vector3 lastPos) { mLastPosition = lastPos; }
	public void setLastSize(Vector2 lastSize) { mLastSize = lastSize; }
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
		point.mRelative = abs(divide(thisSide, parentSide));
		point.setAbsolute(0.0f);
		return point;
	}
	// 将锚点设置到距离相对于父节点最近的边,并且各边界到父节点对应边界的距离固定不变
	protected void setToNearParentSides(bool relative)
	{
		GameObject parent = transform.parent.gameObject;
		if (parent == null)
		{
			return;
		}
		Span<Vector3> sides = stackalloc Vector3[4];
		getSides(parent, sides);
		getParentSides(parent, mParentSides);
		for (int i = 0; i < 4; ++i)
		{
			ComplexPoint anchorPoint = mAnchorPoint[i];
			Vector3 side = sides[i];
			if (i == 0 || i == 2)
			{
				float relativeLeft = side.x - mParentSides[0].x;
				float relativeCenter = side.x;
				float relativeRight = side.x - mParentSides[2].x;
				float disToLeft = abs(relativeLeft);
				float disToCenter = abs(relativeCenter);
				float disToRight = abs(relativeRight);
				if (relative)
				{
					anchorPoint.setRelative(divide(side.x, mParentSides[i].x));
					anchorPoint.setAbsolute(0.0f);
				}
				else
				{
					// 靠近左边
					if (disToLeft < disToCenter && disToLeft < disToRight)
					{
						anchorPoint.setRelative(sign(side.x) * sign(mParentSides[i].x));
						anchorPoint.setAbsolute(relativeLeft);
					}
					// 靠近右边
					else if (disToRight < disToLeft && disToRight < disToCenter)
					{
						anchorPoint.setRelative(sign(side.x) * sign(mParentSides[i].x));
						anchorPoint.setAbsolute(relativeRight);
					}
					// 靠近中心
					else
					{
						anchorPoint.setRelative(0.0f);
						anchorPoint.setAbsolute(relativeCenter);
					}
				}
			}
			else if (i == 1 || i == 3)
			{
				float relativeTop = side.y - mParentSides[1].y;
				float relativeCenter = side.y;
				float relativeBottom = side.y - mParentSides[3].y;
				float disToTop = abs(relativeTop);
				float disToCenter = abs(relativeCenter);
				float disToBottom = abs(relativeBottom);
				if (relative)
				{
					anchorPoint.setRelative(divide(side.y, mParentSides[i].y));
					anchorPoint.setAbsolute(0.0f);
				}
				else
				{
					// 靠近顶部
					if (disToTop < disToCenter && disToTop < disToBottom)
					{
						anchorPoint.setRelative(sign(side.y) * sign(mParentSides[i].y));
						anchorPoint.setAbsolute(relativeTop);
					}
					// 靠近底部
					else if (disToBottom < disToTop && disToBottom < disToCenter)
					{
						anchorPoint.setRelative(sign(side.y) * sign(mParentSides[i].y));
						anchorPoint.setAbsolute(relativeBottom);
					}
					// 靠近中心
					else
					{
						anchorPoint.setRelative(0.0f);
						anchorPoint.setAbsolute(relativeCenter);
					}
				}
			}
		}
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
				mHorizontalPositionRelative = divide(pos.x, (parentSize.x * 0.5f));
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
				mVerticalPositionRelative = divide(pos.y, (parentSize.y * 0.5f));
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
				mHorizontalPositionAbsolute = (int)(pos.x + 0.5f * sign(pos.x));
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
				mVerticalPositionAbsolute = (int)(pos.y + 0.5f * sign(pos.y));
			}
		}
		foreach (ComplexPoint anchorPoint in mAnchorPoint)
		{
			anchorPoint.setRelative(0.0f);
			anchorPoint.setAbsolute(0.0f);
		}
	}
	// 获取当前节点在父节点坐标系下的的各条边
	protected void getSides(GameObject parent, Span<Vector3> sides)
	{
		TryGetComponent<RectTransform>(out var rectTransform);
		if (!isVectorEqual(rectTransform.pivot, new(0.5f, 0.5f)))
		{
			logError("UI的pivot错误:" + rectTransform.name);
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