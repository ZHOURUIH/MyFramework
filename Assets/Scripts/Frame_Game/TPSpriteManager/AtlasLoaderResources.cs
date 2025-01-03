using UnityEngine;
using static FrameBase;
using UObject = UnityEngine.Object;

// 用于UGUI的multi sprite管理
public class AtlasLoaderResources : AtlasLoaderBase
{
	protected override void baseUnloadAtlas(UGUIAtlas atlas, bool showError) 
	{ 
		mResourceManager.unloadInResources(ref atlas.mTexture, showError);
	}
	protected override CustomAsyncOperation baseLoadAtlasAsync(string name, AssetLoadDoneCallback doneCallback)
	{
		return mResourceManager.loadInResourceAsync<Sprite>(name, doneCallback);
	}
	protected override UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull) 
	{ 
		return mResourceManager.loadInSubResource<Sprite>(atlasName, out mainAsset, errorIfNull); 
	}
}