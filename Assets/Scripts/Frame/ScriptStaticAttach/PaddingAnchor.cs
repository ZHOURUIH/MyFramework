using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static UnityUtility;
using static WidgetUtility;
using static MathUtility;
using static FrameDefine;
using static StringUtility;

[Serializable]
public struct ComplexPoint
{
	public float mRelative;
	public int mAbsolute;
	public void setRelative(float relative) { mRelative = relative; }
	public void setAbsolute(float absolute) { mAbsolute = (int)(absolute + 0.5f * Mathf.Sign(absolute)); }
}

// 该组件所在的物体不能有旋转,否则会计算错误
public class PaddingAnchor : MonoBehaviour
{
	private Vector3[] mSides;						// 用于避免GC
	private Vector3[] mLocalCorners;				// 用于避免GC
	protected bool mDirty;							// 是否需要刷新数据
	protected Vector3[] mParentSides;               // 父节点的四条边中心点的坐标
	protected Vector3 mLastPosition;				// 上一次刷新参数时的节点位置,用于判断是否需要刷新参数
	protected Vector2 mLastSize;					// 上一次刷新参数时的节点大小,用于判断是否需要刷新参数
	// 以下是保存的参数属性
	public ANCHOR_MODE mAnchorMode;					// 停靠方式
	public HORIZONTAL_PADDING mHorizontalNearSide;	// 横向停靠方式
	public VERTICAL_PADDING mVerticalNearSide;		// 纵向停靠方式
	public float mHorizontalPositionRelative;		// 横向停靠的百分比
	public int mHorizontalPositionAbsolute;			// 横向停靠的绝对值
	public float mVerticalPositionRelative;			// 纵向停靠的百分比
	public int mVerticalPositionAbsolute;			// 纵向停靠的绝对值
	public int mMinFontSize;						// 字体的最小大小
	public bool mRelativeDistance;					// 是否使用相对距离
	public bool mAdjustFont;						// 是否连字体大小也一起调整
	// 边相对于父节点对应边的距离,Relative是相对于宽或者高的一半,范围0-1,从0到1是对应从中心到各边,Absolute是Relative计算完以后的偏移量,带正负
	public ComplexPoint[] mDistanceToBoard;			// mAnchorMode为PADDING_PARENT_SIDE使用的,左上右下,横向中心,纵向中心的顺序
	public ComplexPoint[] mAnchorPoint;				// mAnchorMode为STRETCH_TO_PARENT_SIDE使用的,左上右下,横向中心,纵向中心的顺序
	public PaddingAnchor()
	{
		mAnchorMode = ANCHOR_MODE.PADDING_PARENT_SIDE;
		mDirty = true;
		mMinFontSize = 12;
		mAdjustFont = true;
		mSides = new Vector3[4];
		mLocalCorners = new Vector3[4];
		mParentSides = new Vector3[4];
		mDistanceToBoard = new ComplexPoint[4] { new ComplexPoint(), new ComplexPoint(), new ComplexPoint(), new ComplexPoint() };
		mAnchorPoint = new ComplexPoint[4] { new ComplexPoint(), new ComplexPoint(), new ComplexPoint(), new ComplexPoint() };
	}
#if UNITY_EDITOR
	public void setAnchorModeInEditor(ANCHOR_MODE mode)
	{
		mAnchorMode = mode;
		setAnchorMode(mAnchorMode);
	}
	public void setHorizontalNearSideInEditor(HORIZONTAL_PADDING side)
	{
		mHorizontalNearSide = side;
		setAnchorMode(mAnchorMode);
	}
	public void setVerticalNearSideInEditor(VERTICAL_PADDING side)
	{
		mVerticalNearSide = side;
		setAnchorMode(mAnchorMode);
	}
	public void setRelativeDistanceInEditor(bool relativeDistance)
	{
		mRelativeDistance = relativeDistance;
		setAnchorMode(mAnchorMode);
	}
	public void Reset()
	{
		// 挂载该脚本时候检查当前GameView的分辨率是否是标准分辨率
		Vector2 screenSize = getGameViewSize();
		if ((int)screenSize.x != STANDARD_WIDTH || (int)screenSize.y != STANDARD_HEIGHT)
		{
			EditorUtility.DisplayDialog("错误", "当前分辨率不是标准分辨率,适配结果可能不对,请将Game视图的分辨率修改为" +
				STANDARD_WIDTH + "*" + STANDARD_HEIGHT, "确定");
			DestroyImmediate(this);
		}
	}
#endif
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
			logWarning("transform's scale is not 1, may not adapt correctely, " + transform.name + ", scale:" + vector3ToString(transform.localScale, 6));
		}
		mDirty = false;
		var rectTransform = GetComponent<RectTransform>();
		Vector2 newSize = rectTransform.rect.size;
		GameObject parent = transform.parent.gameObject;
		Vector2 parentSize = parent.GetComponent<RectTransform>().rect.size;
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
	// 将锚点设置到距离相对于父节点最近的边,并且各边界到父节点对应边界的距离固定不变
	protected void setToNearParentSides(bool relative)
	{
		GameObject parent = transform.parent.gameObject;
		if (parent == null)
		{
			return;
		}
		Vector3[] sides = getSides(parent);
		getParentSides(parent, mParentSides);
		for (int i = 0; i < 4; ++i)
		{
			if (i == 0 || i == 2)
			{
				float relativeLeft = sides[i].x - mParentSides[0].x;
				float relativeCenter = sides[i].x;
				float relativeRight = sides[i].x - mParentSides[2].x;
				float disToLeft = abs(relativeLeft);
				float disToCenter = abs(relativeCenter);
				float disToRight = abs(relativeRight);
				if (relative)
				{
					mAnchorPoint[i].setRelative(sides[i].x / mParentSides[i].x);
					mAnchorPoint[i].setAbsolute(0.0f);
				}
				else
				{
					// 靠近左边
					if (disToLeft < disToCenter && disToLeft < disToRight)
					{
						mAnchorPoint[i].setRelative(sign(sides[i].x) * sign(mParentSides[i].x));
						mAnchorPoint[i].setAbsolute(relativeLeft);
					}
					// 靠近右边
					else if (disToRight < disToLeft && disToRight < disToCenter)
					{
						mAnchorPoint[i].setRelative(sign(sides[i].x) * sign(mParentSides[i].x));
						mAnchorPoint[i].setAbsolute(relativeRight);
					}
					// 靠近中心
					else
					{
						mAnchorPoint[i].setRelative(0.0f);
						mAnchorPoint[i].setAbsolute(relativeCenter);
					}
				}
			}
			else if (i == 1 || i == 3)
			{
				float relativeTop = sides[i].y - mParentSides[1].y;
				float relativeCenter = sides[i].y;
				float relativeBottom = sides[i].y - mParentSides[3].y;
				float disToTop = abs(relativeTop);
				float disToCenter = abs(relativeCenter);
				float disToBottom = abs(relativeBottom);
				if (relative)
				{
					mAnchorPoint[i].setRelative(sides[i].y / mParentSides[i].y);
					mAnchorPoint[i].setAbsolute(0.0f);
				}
				else
				{
					// 靠近顶部
					if (disToTop < disToCenter && disToTop < disToBottom)
					{
						mAnchorPoint[i].setRelative(sign(sides[i].y) * sign(mParentSides[i].y));
						mAnchorPoint[i].setAbsolute(relativeTop);
					}
					// 靠近底部
					else if (disToBottom < disToTop && disToBottom < disToCenter)
					{
						mAnchorPoint[i].setRelative(sign(sides[i].y) * sign(mParentSides[i].y));
						mAnchorPoint[i].setAbsolute(relativeBottom);
					}
					// 靠近中心
					else
					{
						mAnchorPoint[i].setRelative(0.0f);
						mAnchorPoint[i].setAbsolute(relativeCenter);
					}
				}
			}
		}
	}
	// 停靠父节点的指定边界,并且大小不改变
	protected void setToPaddingParentSide(HORIZONTAL_PADDING horizontalSide, VERTICAL_PADDING verticalSide, bool relativeDistance)
	{
		Vector3[] sides = null;
		Vector2 pos = transform.localPosition;
		GameObject parent = transform.parent.gameObject;
		Vector2 parentSize = parent.GetComponent<RectTransform>().rect.size;
		if (parent != null)
		{
			sides = getSides(parent);
			getParentSides(parent, mParentSides);
		}
		int count = mDistanceToBoard.Length;
		for (int i = 0; i < count; ++i)
		{
			mDistanceToBoard[i].setRelative(0.0f);
			mDistanceToBoard[i].setAbsolute(0.0f);
		}
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
				mHorizontalPositionRelative = pos.x / (parentSize.x * 0.5f);
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
				mVerticalPositionRelative = pos.y / (parentSize.y * 0.5f);
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
		for (int i = 0; i < 4; ++i)
		{
			mAnchorPoint[i].setRelative(0.0f);
			mAnchorPoint[i].setAbsolute(0.0f);
		}
	}
	public ComplexPoint sideToAbsolute(float thisSide, float parentSide)
	{
		ComplexPoint point = new ComplexPoint();
		point.mRelative = 1.0f;
		point.setAbsolute(thisSide - parentSide);
		return point;
	}
	public ComplexPoint sideToRelative(float thisSide, float parentSide)
	{
		ComplexPoint point = new ComplexPoint();
		point.mRelative = abs(thisSide / parentSide);
		point.setAbsolute(0.0f);
		return point;
	}
	// 获取当前节点在父节点坐标系下的的各条边
	protected Vector3[] getSides(GameObject parent)
	{
		generateLocalCorners(parent);
		cornerToSide(mLocalCorners, mSides);
		return mSides;
	}
	protected void generateLocalCorners(GameObject parent, bool includeRotation = false)
	{
		RectTransform rectTransform = GetComponent<RectTransform>();
		// 去除旋转
		Quaternion lastQuat = rectTransform.rotation;
		if (!includeRotation)
		{
			rectTransform.rotation = Quaternion.identity;
		}
		Vector2 size = rectTransform.rect.size;
		mLocalCorners[0] = new Vector3(-size.x * 0.5f, -size.y * 0.5f);
		mLocalCorners[1] = new Vector3(-size.x * 0.5f, size.y * 0.5f);
		mLocalCorners[2] = new Vector3(size.x * 0.5f, size.y * 0.5f);
		mLocalCorners[3] = new Vector3(size.x * 0.5f, -size.y * 0.5f);
		for (int i = 0; i < 4; ++i)
		{
			Vector3 worldCorner = localToWorld(rectTransform, mLocalCorners[i]);
			mLocalCorners[i] = worldToLocal(parent.transform, worldCorner);
		}
		if (!includeRotation)
		{
			rectTransform.rotation = lastQuat;
		}
	}
}