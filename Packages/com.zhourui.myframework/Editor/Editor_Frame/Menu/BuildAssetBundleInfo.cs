using System.Collections.Generic;

public class BuildAssetBundleInfo
{
	public List<string> mAssetNames = new();    // 带Resources下相对路径,带后缀
	public List<string> mDependencies = new();  // 所有依赖的AssetBundle
	public string mBundleName;					// 所属AssetBundle
	public BuildAssetBundleInfo(string bundleName)
	{
		mBundleName = bundleName;
	}
	public void AddDependence(string dep)
	{
		mDependencies.Add(dep);
	}
}