using System;

// 布局信息
public struct LayoutInfo
{
	public LAYOUT_ORDER mOrderType;		// 显示顺序类型
	public Type mType;					// 布局脚本类型
	public string mName;				// 布局名字
	public bool mIsScene;				// 是否为场景布局,场景布局不会挂在UGUIRoot下面
	public int mRenderOrder;			// 显示顺序
}