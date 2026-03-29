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
	protected AtlasLoader mAtlasLoader = new();							// 从AssetBundle中加载,因为之前考虑也兼容从Resources中加载,但是由于热更层只会从GameResources中加载,所以就去除了兼容Resources
	protected Dictionary<string, ResourceRef<SpriteAtlas>> mAtlasList;	// 已加载的SpriteAtlas列表
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
		mAtlasLoader.destroyAll();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAtlasLoader.update(elapsedTime);
	}
	public void addDontUnloadAtlas(string fileName) { mAtlasLoader.addDontUnloadAtlas(fileName); }
	public SafeDictionary<string, AtlasBase> getAtlasList() { return mAtlasLoader.getAtlasList(); }
	// 获取位于GameResources目录中的图集,如果不存在则可以选择是否同步加载
	public AtlasRef getAtlas(string atlasName, bool errorInNull = true, bool loadIfNull = true)
	{
		return mAtlasLoader.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 异步加载位于GameResources中的图集,atlasName是GameResources下的相对路径,带后缀
	public CustomAsyncOperation getAtlasAsyncSafe(IRecyclable owner, string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull = true, bool loadIfNull = true)
	{
		return mAtlasLoader.getAtlasAsyncSafe(owner, atlasName, callback, errorInNull, loadIfNull);
	}
	// 卸载图集
	public void unloadAtlas(ref AtlasRef atlasPtr)
	{
		mAtlasLoader.unloadAtlas(atlasPtr);
		UN_CLASS(ref atlasPtr);
	}
	public void unloadAtlas(List<AtlasRef> atlasPtrList)
	{
		atlasPtrList.For(item => mAtlasLoader.unloadAtlas(item));
		UN_CLASS_LIST(atlasPtrList);
	}
	public void unloadAtlas<Key>(Dictionary<Key, AtlasRef> atlasPtrList)
	{
		atlasPtrList.forValue(item => mAtlasLoader.unloadAtlas(item));
		UN_CLASS_LIST(atlasPtrList);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onAtlasRequested(string name, Action<SpriteAtlas> action)
	{
		if (mAtlasPathList == null)
		{
			mAtlasPathList = new();
			var text = mResourceManager.loadGameResource<TextAsset>(R_MISC_PATH + ATLAS_PATH_CONFIG);
			foreach(string line in text.getResource().text.splitLine())
			{
				mAtlasPathList.Add(getFileNameNoSuffixNoDir(line), line);
			}
			mResourceManager.unload(ref text);
		}

		mAtlasList ??= new();
		ResourceRef<SpriteAtlas> atlas = mAtlasList.get(name);
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
		action(atlas.getResource());
	}
}