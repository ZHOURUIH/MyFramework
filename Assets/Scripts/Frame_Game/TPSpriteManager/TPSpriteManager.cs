using System.Collections.Generic;
using static FrameUtility;
using static FrameEditorUtility;

// 用于UGUI的multi sprite管理
public class TPSpriteManager : FrameSystem
{
	protected AtlasLoaderAssetBundle mAssetBundleAtlasManager = new();	// 从AssetBundle中加载
	protected AtlasLoaderResources mResourcesAtlasManager = new();		// 从Resources中加载
	public TPSpriteManager()
	{
		if (isEditor())
		{
			mCreateObject = true;
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAssetBundleAtlasManager.update();
		mResourcesAtlasManager.update();
	}
	// 获取位于GameResources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlas(string atlasName, bool errorInNull, bool loadIfNull)
	{
		return mAssetBundleAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 获取位于Resources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlasInResources(string atlasName, bool errorInNull, bool loadIfNull)
	{
		return mResourcesAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 卸载图集
	public void unloadAtlas(ref UGUIAtlasPtr atlasPtr)
	{
		mAssetBundleAtlasManager.unloadAtlas(atlasPtr);
		UN_CLASS(ref atlasPtr);
	}
	public void unloadAtlas(List<UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr)
		{
			mAssetBundleAtlasManager.unloadAtlas(item);
		}
		UN_CLASS_LIST(atlasPtr);
	}
	public void unloadAtlasInResourcecs(ref UGUIAtlasPtr atlasPtr)
	{
		mResourcesAtlasManager.unloadAtlas(atlasPtr);
		UN_CLASS(ref atlasPtr);
	}
	public void unloadAtlasInResourcecs(List<UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr)
		{
			mResourcesAtlasManager.unloadAtlas(item);
		}
		UN_CLASS_LIST(atlasPtr);
	}
}