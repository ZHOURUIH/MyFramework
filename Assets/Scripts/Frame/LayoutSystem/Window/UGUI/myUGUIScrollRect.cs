using UnityEngine;
using UnityEngine.UI;

// 滑动区域窗口
// 一般如果想要窗口是可操作滑动的,需要关闭Viewport,Content,Item等所有可能拦截射线检测的设置
public class myUGUIScrollRect : myUGUIObject
{
	protected ScrollRect mScrollRect;
	protected myUGUIObject mViewport;
	protected myUGUIObject mContent;
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
			mContent.setPosition(round(mContent.getPosition()));
		}
	}
	public void setContent(myUGUIObject content) { mContent = content; }
	public void setViewport(myUGUIObject viewport) { mViewport = viewport; }
	public myUGUIObject getContent() { return mContent; }
	public myUGUIObject getViewport() { return mViewport; }
	public ScrollRect getScrollRect() { return mScrollRect; }
	// 设置content默认的位置
	// true表示将content的轴心设置到最上边,方便使其顶部对齐viewport顶部
	// false表示将content的轴心设置到最下边,方便使其底部对齐viewport底部
	public void setContentDefaultAlignTop(bool top) { mContent.setPivot(new Vector2(0.5f, top ? 1.0f : 0.0f)); }
	public Vector2 getNormalizedPosition() { return mScrollRect.normalizedPosition; }
	// 设置Content的相对位置，x，y分别为横向和纵向，值的范围是0-1
	// 0表示Content的左边或下边与父节点的左边或下边对齐，1表示Content的右边或上边与父节点的右边或上边对齐
	public void setNormalizedPosition(Vector2 pos) { mScrollRect.normalizedPosition = pos; }
	public void setNormalizedPositionX(float x) { mScrollRect.horizontalNormalizedPosition = x; }
	public void setNormalizedPositionY(float y) { mScrollRect.verticalNormalizedPosition = y; }
	// 根据content的pivot.y计算出并改变content的当前位置
	public void alignContentPivotVertical() { alignContentY(mContent.getPivot().y); }
	// 根据content的pivot.x计算出并改变content的当前位置
	public void alignContentPivotHorizontal() { alignContentX(mContent.getPivot().x); }
	// 使Content的上边界与ScrollRect的上边界对齐,实际上是跟Viewport对齐
	public void alignContentTop() { alignContentY(1.0f); }
	// 使Content的下边界与ScrollRect的下边界对齐,实际上是跟Viewport对齐
	public void alignContentBottom() { alignContentY(0.0f); }
	// 使Content的左边界与ScrollRect的左边界对齐,实际上是跟Viewport对齐
	public void alignContentLeft() { alignContentX(0.0f); }
	// 使Content的右边界与ScrollRect的右边界对齐,实际上是跟Viewport对齐
	public void alignContentRight() { alignContentX(1.0f); }
	// 自动调整Content窗口的大小和位置
	public void autoAdjustContent()
	{
		if (mScrollRect.vertical)
		{
			autoGridVertical(mContent);
			// 当Content的大小小于Viewport时,Content顶部对齐Viewport顶部(实际是根据content的pivot计算)
			if (mViewport.getWindowSize().y >= mContent.getWindowSize().y)
			{
				alignContentY(mContent.getPivot().y);
			}
			// 当Content的大小大于Viewport时,Content底部对齐Viewport底部(实际是根据content的pivot计算)
			else
			{
				alignContentY(1.0f - mContent.getPivot().y);
			}
		}
		else if (mScrollRect.horizontal)
		{
			autoGridHorizontal(mContent);
			if (mViewport.getWindowSize().x >= mContent.getWindowSize().x)
			{
				alignContentLeft();
			}
			else
			{
				alignContentRight();
			}
		}
	}
	// 设置Content的顶部在Viewport中的坐标,一般在Content高度变化时,会保持顶部的位置不变,向下拉伸Content长度
	public void setContentTopPos(float top)
	{
		mContent.setPositionY(top - mContent.getWindowSize().y * 0.5f);
		mScrollRect.velocity = new Vector2(mScrollRect.velocity.x, 0.0f);
	}
	public float getContentTopPos() { return mContent.getPosition().y + mContent.getWindowSize().y * 0.5f; }
	//-----------------------------------------------------------------------------------------------------------------------------------------------
	// 
	protected void alignContentY(float y)
	{
		mScrollRect.velocity = new Vector2(mScrollRect.velocity.x, 0.0f);
		mScrollRect.verticalNormalizedPosition = y;
	}
	protected void alignContentX(float x)
	{
		mScrollRect.velocity = new Vector2(0.0f, mScrollRect.velocity.y);
		mScrollRect.horizontalNormalizedPosition = x;
	}
}