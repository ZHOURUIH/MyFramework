using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using UObject = UnityEngine.Object;

// 用于UGUI的multi sprite管理
public class TPSpriteManager : FrameSystem
{
	protected AtlasLoaderAssetBundle mAssetBundleAtlasManager;
	protected AtlasLoaderResources mResourcesAtlasManager;
	public TPSpriteManager()
	{
		mAssetBundleAtlasManager = new AtlasLoaderAssetBundle();
		mResourcesAtlasManager = new AtlasLoaderResources();
#if UNITY_EDITOR
		mCreateObject = true;
#endif
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<TPSpriteManagerDebug>();
#endif
	}
	public void destroyAll()
	{
		mAssetBundleAtlasManager.destroyAll();
		mResourcesAtlasManager.destroyAll();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAssetBundleAtlasManager.update();
		mResourcesAtlasManager.update();
	}
	public void addDontUnloadAtlas(string fileName) { mAssetBundleAtlasManager.addDontUnloadAtlas(fileName); }
#if UNITY_EDITOR
	public Dictionary<string, UGUIAtlas> getAtlasList() { return mAssetBundleAtlasManager.getAtlasList(); }
	public Dictionary<string, UGUIAtlas> getAtlasListInResources() { return mResourcesAtlasManager.getAtlasList(); }
#endif
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
	// 异步加载位于GameResources中的图集
	public void getAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorInNull, bool loadIfNull)
	{
		mAssetBundleAtlasManager.getAtlasAsync(atlasName, callback, userData, errorInNull, loadIfNull);
	}
	// 异步加载位于Resources中的图集
	public void getAtlasInResourcesAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorInNull, bool loadIfNull)
	{
		mResourcesAtlasManager.getAtlasAsync(atlasName, callback, userData, errorInNull, loadIfNull);
	}
	// 卸载图集
	public void unloadAtlas(ref UGUIAtlasPtr atlasPtr)
	{
		mAssetBundleAtlasManager.unloadAtlas(ref atlasPtr);
	}
	public void unloadAtlasInResourcecs(ref UGUIAtlasPtr atlasPtr)
	{
		mResourcesAtlasManager.unloadAtlas(ref atlasPtr);
	}
	// 图集资源已经加载完成,从assets中解析并创建图集信息
	public static UGUIAtlas atlasLoaded<T>(T[] assets, string loadPath) where T : UObject
	{
		if (assets == null || assets.Length == 0)
		{
			return null;
		}
		UGUIAtlas atlas = new UGUIAtlas();
		atlas.mFilePath = loadPath;
		// 找到Texture,位置不一定是第一个,需要遍历查找
		int count = assets.Length;
		for (int i = 0; i < count; ++i)
		{
			T item = assets[i];
			// 找出所有的精灵
			if (item is Sprite)
			{
				atlas.mSpriteList.Add(item.name, item as Sprite);
			}
			// 找到Texture2D
			else if (item is Texture2D)
			{
				atlas.mTexture = item as Texture2D;
			}
		}
		if (atlas.mTexture == null)
		{
			logError("can not find texture2D in loaded Texture:" + loadPath);
		}
		return atlas;
	}
}