using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static FrameBaseHotFix;

// 用于UGUI的multi sprite管理
public class AtlasLoaderAssetBundle : AtlasLoaderBase
{
	protected HashSet<string> mDontUnloadAtlas = new();             // 即使没有引用也不会卸载的图集
	public void addDontUnloadAtlas(string fileName) { mDontUnloadAtlas.Add(fileName); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void baseUnloadAtlas(AtlasBase atlas, bool showError) 
	{
		mResourceManager.unload(ref atlas.mMainAsset, showError); 
	}
	protected override CustomAsyncOperation baseLoadAtlasAsync(string atlasName, AssetLoadDoneCallback doneCallback)
	{
		if (atlasName.endWith(".png"))
		{
			return mResourceManager.loadGameResourceAsync<Sprite>(atlasName, doneCallback);
		}
		else
		{
			return mResourceManager.loadGameResourceAsync<SpriteAtlas>(atlasName, doneCallback);
		}
	}
	protected override UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull) 
	{
		if (atlasName.endWith(".png"))
		{
			return mResourceManager.loadSubGameResource<Sprite>(atlasName, out mainAsset, errorIfNull);
		}
		else
		{
			return mResourceManager.loadSubGameResource<SpriteAtlas>(atlasName, out mainAsset, errorIfNull);
		}
	}
	protected override bool allowUnloadAtlas(string atlasName) { return !mDontUnloadAtlas.Contains(atlasName); }
}