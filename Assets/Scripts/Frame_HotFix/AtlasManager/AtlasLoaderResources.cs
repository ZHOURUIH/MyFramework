using UnityEngine;
using UnityEngine.U2D;
using static FrameBaseHotFix;
using UObject = UnityEngine.Object;

// 用于UGUI的multi sprite管理
public class AtlasLoaderResources : AtlasLoaderBase
{
	protected override void baseUnloadAtlas(AtlasBase atlas, bool showError) 
	{
		if (atlas is UGUIAtlas uguiAtlas)
		{
			mResourceManager.unloadInResources(ref uguiAtlas.mSpriteAtlas, showError);
		}
		else if (atlas is TPAtlas tpAtlas)
		{
			mResourceManager.unloadInResources(ref tpAtlas.mTexture, showError);
		}
	}
	protected override CustomAsyncOperation baseLoadAtlasAsync(string atlasName, AssetLoadDoneCallback doneCallback)
	{
		if (atlasName.endWith(".png"))
		{
			return mResourceManager.loadInResourceAsync<Sprite>(atlasName, doneCallback);
		}
		else
		{
			return mResourceManager.loadInResourceAsync<SpriteAtlas>(atlasName, doneCallback);
		}
	}
	protected override UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull) 
	{
		if (atlasName.endWith(".png"))
		{
			return mResourceManager.loadSubInResource<Sprite>(atlasName, out mainAsset, errorIfNull);
		}
		else
		{
			return mResourceManager.loadSubInResource<SpriteAtlas>(atlasName, out mainAsset, errorIfNull);
		}
	}
}