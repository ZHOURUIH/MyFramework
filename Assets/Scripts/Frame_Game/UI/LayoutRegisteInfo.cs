using System;

// 用于记录布局的注册信息
public struct LayoutRegisteInfo
{
	public Type mScriptType;				// 布局脚本类型
	public GameLayoutCallback mCallback;	// 用于加载或者卸载后对脚本变量进行赋值
	public string mFileNameNoSuffix;		// 相对于UIPrefab的路径,且不带后缀名
}