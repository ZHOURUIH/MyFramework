using System;

// 用于记录布局的注册信息
public struct LayoutRegisteInfo
{
	public Type mScriptType;				// 布局脚本类型
	public bool mInResource;				// 布局是否在Resources中
	public LAYOUT_LIFE_CYCLE mLifeCycle;    // 布局的生命周期
	public LayoutScriptCallback mCallback;	// 用于加载或者卸载后对脚本变量进行赋值
}