using System;
using System.Collections.Generic;

[Serializable]
// 结构体,用于在编辑器中显示AssetBundle调试信息
public struct AssetBundleDebug
{
	public string mBundleName;
	public List<AssetInfo> mAssetList;
	public List<string> mParentBundles;
	public List<string> mChildBundles;
	public AssetBundleDebug(string name)
	{
		mBundleName = name;
		mAssetList = new();
		mParentBundles = new();
		mChildBundles = new();
	}
}