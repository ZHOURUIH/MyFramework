using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class myUGUIScrollRect : myUGUIObject
{
	protected ScrollRect mScrollRect;
	public myUGUIScrollRect()
	{
		mEnable = true;
	}
	public override void init()
	{
		base.init();
		mScrollRect = mObject.GetComponent<ScrollRect>();
		if (mScrollRect == null)
		{
			mScrollRect = mObject.AddComponent<ScrollRect>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mScrollRect == null)
		{
			logError(Typeof(this) + " can not find " + typeof(ScrollRect) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mScrollRect.vertical && isFloatZero(mScrollRect.velocity.y) ||
			mScrollRect.horizontal && isFloatZero(mScrollRect.velocity.x))
		{
			mScrollRect.content.localPosition = round(mScrollRect.content.localPosition);
		}
	}
	public ScrollRect getScrollRect() { return mScrollRect; }
	public Vector2 getNormalizedPosition() { return mScrollRect.normalizedPosition; }
	// 设置Content的相对位置，x，y分别为横向和纵向，值的范围是0-1
	// 0表示Content的左边或下边与父节点的左边或下边对齐，1表示Content的右边或上边与父节点的右边或上边对齐
	public void setNormalizedPosition(Vector2 pos) { mScrollRect.normalizedPosition = pos; }
	public void setNormalizedPositionX(float x) { mScrollRect.horizontalNormalizedPosition = x; }
	public void setNormalizedPositionY(float y) { mScrollRect.verticalNormalizedPosition = y; }
	// 使Content的上边界与ScrollRect的上边界对齐,实际上是跟Viewport对齐
	public void alignContentTop() 
	{
		mScrollRect.velocity = new Vector2(mScrollRect.velocity.x, 0.0f);
		mScrollRect.verticalNormalizedPosition = 1.0f;
	}
	public void alignContentBottom()
	{
		mScrollRect.velocity = new Vector2(mScrollRect.velocity.x, 0.0f);
		mScrollRect.verticalNormalizedPosition = 0.0f;
	}
	// 设置Content的顶部在Viewport中的坐标,一般在Content高度变化时,会保持顶部的位置不变,向下拉伸Content长度
	public void setContentTopPos(float top)
	{
		Vector3 curContentPos = mScrollRect.content.localPosition;
		curContentPos.y = top - mScrollRect.content.rect.size.y * 0.5f;
		mScrollRect.content.localPosition = curContentPos;
		mScrollRect.velocity = new Vector2(mScrollRect.velocity.x, 0.0f);
	}
	public float getContentTopPos()
	{
		return mScrollRect.content.localPosition.y + mScrollRect.content.rect.size.y * 0.5f;
	}
}