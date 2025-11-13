using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static FrameUtility;
using static FrameDefine;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 用于管理SpriteAtlas和MultiSprite封装以后的对象
public class AtlasManager : FrameSystem
{
	protected AtlasLoaderAssetBundle mAssetBundleAtlasManager = new();	// 从AssetBundle中加载
	protected AtlasLoaderResources mResourcesAtlasManager = new();		// 从Resources中加载
	protected Dictionary<string, SpriteAtlas> mAtlasList;				// 已加载的SpriteAtlas列表
	protected Dictionary<string, string> mAtlasPathList;				// 根据图集名字查找SpriteAtlas文件的路径
	public AtlasManager()
	{
		if (isEditor())
		{
			mCreateObject = true;
		}
	}
	public override void init()
	{
		base.init();
		// 注册一下SpriteAtlas的回调,否则在真机上没办法自动加载SpriteAtlas中的Sprite
		SpriteAtlasManager.atlasRequested += onAtlasRequested;
		if (isEditor())
		{
			mObject.AddComponent<TPSpriteManagerDebug>();
		}
	}
	public override void destroy()
	{
		base.destroy();
		SpriteAtlasManager.atlasRequested -= onAtlasRequested;
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
	public SafeDictionary<string, AtlasBase> getAtlasList() { return mAssetBundleAtlasManager.getAtlasList(); }
	public SafeDictionary<string, AtlasBase> getAtlasListInResources() { return mResourcesAtlasManager.getAtlasList(); }
	// 获取位于GameResources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlas(string atlasName, bool errorInNull = true, bool loadIfNull = true)
	{
		return mAssetBundleAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 获取位于Resources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlasInResources(string atlasName, bool errorInNull = true, bool loadIfNull = true)
	{
		return mResourcesAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 异步加载位于GameResources中的图集,atlasName是GameResources下的相对路径,带后缀
	public CustomAsyncOperation getAtlasAsync(string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull = true, bool loadIfNull = true)
	{
		return mAssetBundleAtlasManager.getAtlasAsync(atlasName, callback, errorInNull, loadIfNull);
	}
	// 异步加载位于Resources中的图集
	public CustomAsyncOperation getAtlasInResourcesAsync(string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull = true, bool loadIfNull = true)
	{
		return mResourcesAtlasManager.getAtlasAsync(atlasName, callback, errorInNull, loadIfNull);
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
	public void unloadAtlas<Key>(Dictionary<Key, UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr.Values)
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onAtlasRequested(string name, Action<SpriteAtlas> action)
	{
		if (mAtlasPathList == null)
		{
			mAtlasPathList = new();
			var text = mResourceManager.loadGameResource<TextAsset>(R_MISC_PATH + ATLAS_PATH_CONFIG);
			foreach(string line in splitLine(text.text))
			{
				mAtlasPathList.Add(getFileNameNoSuffixNoDir(line), line);
			}
		}

		mAtlasList ??= new();
		SpriteAtlas atlas = mAtlasList.get(name);
		if (atlas == null)
		{
			string path = mAtlasPathList.get(name);
			if (path == null)
			{
				Debug.LogError("在AtlasPathConfig中找不到指定名字的图集:" + name);
				action(null);
				return;
			}
			atlas = mResourceManager.loadGameResource<SpriteAtlas>(path);
			mAtlasList.Add(name, atlas);
		}
		action(atlas);
	}
}