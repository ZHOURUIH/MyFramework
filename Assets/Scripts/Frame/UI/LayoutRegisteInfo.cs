using UnityEngine;
using System;

// 用于记录布局的注册信息
public struct LayoutRegisteInfo
{
	public Type mScriptType;				// 布局脚本类型
	public string mName;					// 布局名称,也是路径
	public int mID;							// 布局ID
	public bool mInResource;				// 布局是否在Resources中
	public LAYOUT_LIFE_CYCLE mLifeCycle;	// 布局的生命周期
}