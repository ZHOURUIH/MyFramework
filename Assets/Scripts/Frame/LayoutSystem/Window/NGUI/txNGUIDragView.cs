using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_NGUI

public class txNGUIDragView : txNGUITexture
{
	protected WindowComponentDragView mDragViewComponent;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		if (mBoxCollider == null)
		{
			logError("DragView must have BoxCollider!");
		}
	}
	public override void initComponents()
	{
		base.initComponents();
		mDragViewComponent = addComponent<WindowComponentDragView>(true);
	}
	public override bool getReceiveScreenMouse() { return true; }
	// 显式调用调整窗口位置
	public void autoClampPosition()
	{
		mDragViewComponent.autoClampPosition();
	}
	public void autoResetPosition()
	{
		mDragViewComponent.autoResetPosition();
	}
	public override void onMouseDown(Vector3 mousePos)
	{
		base.onMouseDown(mousePos);
		mDragViewComponent.onMouseDown(mousePos);
	}
	// 鼠标在屏幕上抬起
	public override void onScreenMouseUp(Vector3 mousePos)
	{
		base.onScreenMouseUp(mousePos);
		mDragViewComponent.onScreenMouseUp(mousePos);
	}
	public override void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime)
	{
		base.onMouseMove(ref mousePos, ref moveDelta, moveTime);
		mDragViewComponent.onMouseMove(ref mousePos, ref moveDelta, moveTime);
	}
	public override void onMouseStay(Vector3 mousePos)
	{
		base.onMouseStay(mousePos);
		mDragViewComponent.onMouseStay(mousePos);
	}
	public override void setWindowSize(Vector2 size)
	{
		base.setWindowSize(size);
		mDragViewComponent.onWindowSizeChange(size);
	}
	// 当DragView的父节点的大小改变时,需要调用该函数重新计算可拖动的范围
	public void notifyParentSizeChange()
	{
		mDragViewComponent.notifyWindowParentSizeChange();
	}
	public void autoAdjustParent(ref Vector3 parentPos, ref Vector2 parentSize, Vector2 viewportSize)
	{
		if (getClampInner())
		{
			Vector2 windowSize = getWindowSize();
			parentSize = windowSize;
			DRAG_DIRECTION dragDirection = mDragViewComponent.getDragDirection();
			if (dragDirection == DRAG_DIRECTION.DD_HORIZONTAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				parentSize.x = windowSize.x + (windowSize.x - viewportSize.x) * 2.0f;
				parentPos.x = -viewportSize.x * 0.5f + parentSize.x * 0.5f;
			}
			if (dragDirection == DRAG_DIRECTION.DD_VERTICAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				parentSize.y = windowSize.y + (windowSize.y - viewportSize.y) * 2.0f;
				parentPos.y = -viewportSize.y * 0.5f + parentSize.y * 0.5f;
			}
		}
		else
		{
			parentSize = viewportSize;
			DRAG_DIRECTION dragDirection = mDragViewComponent.getDragDirection();
			if (dragDirection == DRAG_DIRECTION.DD_HORIZONTAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				parentPos.x = 0.0f;
			}
			if (dragDirection == DRAG_DIRECTION.DD_VERTICAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				parentPos.y = 0.0f;
			}
		}
	}
	public void recalculateDragView<T>(List<T> itemList, Vector2 itemSize, float space, txUGUIObject dragViewParent, Vector2 viewportSize, int countPerLine = 0, float lineSpace = 0.0f, bool horiFirst = false) where T : IDragViewItem
	{
		// 计算拖拽窗口大小
		int count = itemList.Count;
		int widthCount = countPerLine;
		int heightCount = ceil((float)count / countPerLine);
		Vector2 windowSize = getWindowSize();
		DRAG_DIRECTION dragDirection = mDragViewComponent.getDragDirection();
		if (dragDirection == DRAG_DIRECTION.DD_HORIZONTAL || dragDirection == DRAG_DIRECTION.DD_FREE)
		{
			if (countPerLine > 0)
			{
				windowSize.x = itemSize.x * widthCount + space * (widthCount - 1);
				windowSize.y = itemSize.y * heightCount + lineSpace * (heightCount - 1);
			}
			else
			{
				windowSize.x = itemSize.x * count + space * (count - 1);
			}
		}
		if (dragDirection == DRAG_DIRECTION.DD_VERTICAL || dragDirection == DRAG_DIRECTION.DD_FREE)
		{
			if (countPerLine > 0)
			{
				windowSize.x = itemSize.x * widthCount + lineSpace * (widthCount - 1);
				windowSize.y = itemSize.y * heightCount + space * (heightCount - 1);
			}
			else
			{
				windowSize.y = itemSize.y * count + space * (count - 1);
			}
		}
		clampMin(ref windowSize.x, 0.0f);
		clampMin(ref windowSize.y, 0.0f);
		setWindowSize(windowSize);
		// 设置所有子节点的位置
		for (int i = 0; i < count; ++i)
		{
			Vector3 itemPos = itemList[i].getPosition();
			if (dragDirection == DRAG_DIRECTION.DD_HORIZONTAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				// 排成多排
				if (countPerLine > 0)
				{
					int horiIndex = horiFirst ? i % widthCount : i / heightCount;
					int vertIndex = horiFirst ? i / widthCount : i % heightCount;
					itemPos.x = (itemSize.x + space) * horiIndex + itemSize.x * 0.5f - windowSize.x * 0.5f;
					itemPos.y = (itemSize.y + lineSpace) * (heightCount - vertIndex - 1) + itemSize.y * 0.5f - windowSize.y * 0.5f;
				}
				// 只排一排
				else
				{
					itemPos.x = (itemSize.x + space) * i + itemSize.x * 0.5f - windowSize.x * 0.5f;
				}
			}
			if (dragDirection == DRAG_DIRECTION.DD_VERTICAL || dragDirection == DRAG_DIRECTION.DD_FREE)
			{
				// 排成多列
				if (countPerLine > 0)
				{
					int horiIndex = horiFirst ? i % widthCount : i / heightCount;
					int vertIndex = horiFirst ? i / widthCount : i % heightCount;
					itemPos.x = (itemSize.x + lineSpace) * (horiIndex - widthCount - 1) + itemSize.x * 0.5f - windowSize.x * 0.5f;
					itemPos.y = (itemSize.y + space) * vertIndex + itemSize.y * 0.5f - windowSize.y * 0.5f;
				}
				// 排成一列
				else
				{
					itemPos.y = (itemSize.y + space) * i + itemSize.y * 0.5f - windowSize.y * 0.5f;
				}
			}
			itemList[i].setPosition(itemPos);
		}
		// 重新计算拖拽窗口父节点的大小和位置,用于限定拖拽窗口的拖拽范围
		Vector3 parentPos = dragViewParent.getPosition();
		Vector2 parentSize = Vector2.zero;
		autoAdjustParent(ref parentPos, ref parentSize, viewportSize);
		dragViewParent.setWindowSize(parentSize);
		LT.MOVE(dragViewParent, parentPos);
		autoResetPosition();
	}
	public void setClampInner(bool inner) { mDragViewComponent.setClampInner(inner); }
	public bool isClampInner() { return mDragViewComponent.getClampInner(); }
	public void setDragDirection(DRAG_DIRECTION direction) { mDragViewComponent.setDragDirection(direction); }
	public void setMaxRelativePos(Vector3 max) { mDragViewComponent.setMaxRelativePos(max); }
	public void setMinRelativePos(Vector3 min) { mDragViewComponent.setMinRelativePos(min); }
	public void setMoveSpeedScale(float scale) { mDragViewComponent.setMoveSpeedScale(scale); }
	public void setDragViewStartCallback(OnDragViewStartCallback callback) { mDragViewComponent.setDragViewStartCallback(callback); }
	public void setDragingCallback(OnDragViewCallback draging) { mDragViewComponent.setDragingCallback(draging); }
	public void setReleaseDragCallback(OnDragViewCallback releaseDrag) { mDragViewComponent.setReleaseDragCallback(releaseDrag); }
	public void setPositionChangeCallback(OnDragViewCallback positionChange) { mDragViewComponent.setPositionChangeCallback(positionChange); }
	public void setClampType(CLAMP_TYPE clampType) { mDragViewComponent.setClampType(clampType); }
	public bool isDraging() { return mDragViewComponent.isDraging(); }
	public DRAG_DIRECTION getDragDirection() { return mDragViewComponent.getDragDirection(); }
	public Vector3 getMaxRelativePos() { return mDragViewComponent.getMaxRelativePos(); }
	public Vector3 getMinRelativePos() { return mDragViewComponent.getMinRelativePos(); }
	public void setAutoMoveToEdge(bool autoMove) { mDragViewComponent.setAutoMoveToEdge(autoMove); }
	public void setAttenuateFactor(float value) { mDragViewComponent.setAttenuateFactor(value); }
	public void setDragLengthThreshold(float value) { mDragViewComponent.setDragLengthThreshold(value); }
	public void setDragAngleThreshold(float radian) { mDragViewComponent.setDragAngleThreshold(radian); }
	public void setAutoClampSpeed(float speed) { mDragViewComponent.setAutoClampSpeed(speed); }
	//------------------------------------------------------------------------------------------------------------------------------------------
}

#endif