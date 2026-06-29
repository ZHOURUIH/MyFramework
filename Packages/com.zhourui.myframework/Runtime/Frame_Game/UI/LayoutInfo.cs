using System;

// 布局信息
public struct LayoutInfo
{
	public GameLayoutCallback mCallback;// 加载完成的回调
	public Type mType;					// 布局脚本类型
	public int mRenderOrder;			// 显示顺序
}