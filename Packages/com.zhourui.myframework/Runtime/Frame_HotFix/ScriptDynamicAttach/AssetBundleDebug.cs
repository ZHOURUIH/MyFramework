using System;
using System.Collections.Generic;

[Serializable]
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