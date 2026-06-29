using System;

// 布局加载时的一些信息
public class LayoutLoadInfo : ClassObject
{
	public GameLayout mLayout;		// 加载的布局
	public Type mType;				// 布局类型
	public int mOrder;				// 布局渲染顺序
	public LAYOUT_ORDER mOrderType;	// 布局渲染顺序类型
	public bool mIsScene;			// 是否为场景UI
	public override void resetProperty()
	{
		base.resetProperty();
		mType = null;
		mOrder = 0;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mLayout = null;
		mIsScene = false;
	}
}