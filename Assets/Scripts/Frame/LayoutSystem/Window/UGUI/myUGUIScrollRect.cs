using UnityEngine;
using UnityEngine.UI;

// 滑动区域窗口
// 一般如果想要窗口是可操作滑动的,则需要打开ScrollRect上的Image的RaycastTarget
// 并且需要关闭Viewport,Content,Item等所有可能拦截射线检测的设置(实际是去掉EventTrigger,因为ScrollRect也是通过EventTrigger监听的)
// 如果希望Item既可以响应点击,ScrollRect又能够正常滑动,则需要Item是有button组件进行点击,这是UGUI的做法
// 如果使用GlobalTouchSystem则两者没有任何冲突,滑动和点击互不影响
public class myUGUIScrollRect : myUGUIObject
{
	protected ScrollRect mScrollRect;	// UGUI的ScrollRect组件
	protected myUGUIObject mViewport;	// ScrollRect下的Viewport节点
	protected myUGUIObject mContent;	// Viewport下的Content节点
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
	public void initScrollRect(myUGUIObject viewport, myUGUIObject content, float verticalPivot = 1.0f, float horizontalPivot = 0.5f)
	{
		mContent = content;
		mViewport = viewport;
		setContentPivotVertical(verticalPivot);
		setContentPivotHorizontal(horizontalPivot);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mViewport == null || mContent == null)
		{
			logError("未找到viewport或content,请确保已经在布局的init中调用了initScrillRect函数进行ScrollRect的初始化");
		}
		// 始终确保ScrollRect和Viewport的宽高都是偶数,否则可能会引起在mContent调整位置时总是重新被ScrollRect再校正回来导致Content不停抖动
		makeSizeEven(this);
		makeSizeEven(mViewport);
		// 矫正Content的位置,使之始终为整数
		if (mScrollRect.vertical && isFloatZero(mScrollRect.velocity.y) ||
			mScrollRect.horizontal && isFloatZero(mScrollRect.velocity.x))
		{
			mContent.setPosition(round(mContent.getPosition()));
		}
	}
	public myUGUIObject getContent() { return mContent; }
	public myUGUIObject getViewport() { return mViewport; }
	public ScrollRect getScrollRect() { return mScrollRect; }
	// 设置content竖直方向的轴心
	// 1.0表示将content的轴心设置到最上边,使其顶部对齐viewport顶部
	// 0.5表示将content的轴心设置到中间,使其中间对齐viewport中间
	// 0.0表示将content的轴心设置到最下边,使其底部对齐viewport底部
	public void setContentPivotVertical(float pivot) { mContent.setPivot(new Vector2(mContent.getPivot().x, pivot)); }
	// 设置content水平方向的轴心
	// 1.0表示将content的轴心设置到最右边,使其右边界对齐viewport右边界
	// 0.5表示将content的轴心设置到中间,使其中间对齐viewport中间
	// 0.0表示将content的轴心设置到最左边,使其左边界对齐viewport左边界
	public void setContentPivotHorizontal(float pivot) { mContent.setPivot(new Vector2(pivot, mContent.getPivot().y)); }
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
	public void autoAdjustContent(Vector2 itemSize)
	{
		autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, itemSize);
	}
	public void autoAdjustContent(CONTENT_ADJUST type = CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE)
	{
		autoAdjustContent(type, Vector2.zero);
	}
	// 当子节点在不断增加,需要实时计算content的位置时使用
	// 自动排列content下的子节点,并且计算然后设置content的位置
	public void autoAdjustContent(CONTENT_ADJUST adjustType, Vector2 itemSize)
	{
		if (mScrollRect.vertical)
		{
			if (adjustType == CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE)
			{
				autoGridVertical(mContent);
			}
			else if(adjustType == CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT)
			{
				autoGridFixedRootWidth(mContent, itemSize);
			}
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
			if (adjustType == CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE)
			{
				autoGridHorizontal(mContent);
			}
			else if(adjustType == CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT)
			{
				autoGridFixedRootHeight(mContent, itemSize);
			}
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
	//------------------------------------------------------------------------------------------------------------------------------
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