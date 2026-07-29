using System;

[Serializable]
// 结构体,用于在编辑器中显示UGUI图集调试信息
public struct UGUIAtlasDebug
{
	public string mAtlasName;
	public int mRefCount;
}