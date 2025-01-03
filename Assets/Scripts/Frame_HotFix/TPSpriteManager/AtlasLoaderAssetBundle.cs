using UnityEngine;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static FrameBase;

// 用于UGUI的multi sprite管理
public class AtlasLoaderAssetBundle : AtlasLoaderBase
{
	protected HashSet<string> mDontUnloadAtlas = new();             // 即使没有引用也不会卸载的图集
	public void addDontUnloadAtlas(string fileName) { mDontUnloadAtlas.Add(fileName); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void baseUnloadAtlas(UGUIAtlas atlas, bool showError) 
	{
		mResourceManager.unload(ref atlas.mMainAsset, showError); 
	}
	protected override CustomAsyncOperation baseLoadAtlasAsync(string name, AssetLoadDoneCallback doneCallback)
	{
		return mResourceManager.loadGameResourceAsync<Sprite>(name, doneCallback);
	}
	protected override UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull) 
	{
		return mResourceManager.loadGameSubResource<Sprite>(atlasName, out mainAsset, errorIfNull); 
	}
	protected override bool allowUnloadAtlas(string atlasName) { return !mDontUnloadAtlas.Contains(atlasName); }
}