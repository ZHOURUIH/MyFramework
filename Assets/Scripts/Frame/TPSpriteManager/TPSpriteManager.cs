﻿using UnityEngine;
using System;
using System.Collections.Generic;

public class UGUIAtlas
{
	public Texture2D mTexture;
}

public class AtlasLoadParam : GameBasePooledObject
{
	public AtlasLoadDone mCallback;
	public object mUserData;
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mUserData = null;
	}
}

// 用于UGUI的multi sprite管理
public class TPSpriteManager : FrameSystem
{
	protected Dictionary<string, Dictionary<string, Sprite>> mSpriteNameList;// key是图集的路径,相对于GameResources
	protected Dictionary<UGUIAtlas, Dictionary<string, Sprite>> mSpriteList;
	protected Dictionary<string, UGUIAtlas> mAtlasNameList;					// key是图集的路径,相对于GameResources
	protected Dictionary<UGUIAtlas, string> mAtlasList;                     // value是图集的路径,相对于GameResources
	protected AssetLoadDoneCallback mAtlasCallback;
	public TPSpriteManager()
	{
		mSpriteNameList = new Dictionary<string, Dictionary<string, Sprite>>();
		mSpriteList = new Dictionary<UGUIAtlas, Dictionary<string, Sprite>>();
		mAtlasNameList = new Dictionary<string, UGUIAtlas>();
		mAtlasList = new Dictionary<UGUIAtlas, string>();
		mAtlasCallback = onAtlasLoaded;
	}
	public Dictionary<string, Sprite> getSprites(string atlasName, bool errorIfNull = true)
	{
		if (!mSpriteNameList.ContainsKey(atlasName))
		{
			loadAtlas(atlasName, errorIfNull);
		}
		if (mSpriteNameList.TryGetValue(atlasName, out Dictionary<string, Sprite> spriteList))
		{
			return spriteList;
		}
		return null;
	}
	public Dictionary<string, Sprite> getSprites(UGUIAtlas atlas)
	{
		if (mSpriteList.TryGetValue(atlas, out Dictionary<string, Sprite> spriteList))
		{
			return spriteList;
		}
		return null;
	}
	public Sprite getSprite(UGUIAtlas atlas, string spriteName)
	{
		if (atlas == null || spriteName == null)
		{
			return null;
		}
		if (mSpriteList.TryGetValue(atlas, out Dictionary<string, Sprite> spriteList) && 
			spriteList.TryGetValue(spriteName, out Sprite sprite))
		{
			return sprite;
		}
		return null;
	}
	public Sprite getSprite(string atlasName, string spriteName, bool errorIfNull = true, bool loadIfNull = true)
	{
		if (!mSpriteNameList.ContainsKey(atlasName) && loadIfNull)
		{
			loadAtlas(atlasName, errorIfNull);
		}
		if (mSpriteNameList.TryGetValue(atlasName, out Dictionary<string, Sprite> spriteList) &&
			spriteList.TryGetValue(spriteName, out Sprite sprite))
		{
			return sprite;
		}
		return null;
	}
	public UGUIAtlas getAtlas(string atlasName, bool errorInNull, bool loadIfNull)
	{
		if (mAtlasNameList.TryGetValue(atlasName, out UGUIAtlas atlas))
		{
			return atlas;
		}
		if (loadIfNull)
		{
			return loadAtlas(atlasName, errorInNull);
		}
		return null;
	}
	public void getAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorInNull, bool loadIfNull)
	{
		if (mAtlasNameList.TryGetValue(atlasName, out UGUIAtlas atlas))
		{
			callback?.Invoke(atlas, userData);
			return;
		}
		if (loadIfNull)
		{
			loadAtlasAsync(atlasName, callback, userData, errorInNull);
		}
	}
	public void unloadAtlas(string atlasName)
	{
		if (!mAtlasNameList.TryGetValue(atlasName, out UGUIAtlas atlas))
		{
			return;
		}
		mAtlasNameList.Remove(atlasName);
		mAtlasList.Remove(atlas);
		mSpriteNameList[atlasName].Clear();
		mSpriteNameList.Remove(atlasName);
		mSpriteList.Remove(atlas);
		mResourceManager.unload(ref atlas.mTexture);
	}
	//---------------------------------------------------------------------------------------------------------------
	// 异步加载图集
	protected void loadAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorIfNull = true)
	{
		if (!mSpriteNameList.ContainsKey(atlasName))
		{
			var param = new AtlasLoadParam() { mCallback = callback, mUserData = userData };
			mResourceManager.loadSubResourceAsync<Sprite>(atlasName, mAtlasCallback, param, errorIfNull);
		}
	}
	// 异步加载完毕
	protected void onAtlasLoaded(UnityEngine.Object asset, UnityEngine.Object[] assets, byte[] bytes, object userData, string loadPath)
	{
		if (!mAtlasNameList.TryGetValue(loadPath, out UGUIAtlas atlas))
		{
			atlas = atlasLoaded(assets, loadPath);
		}
		var param = (AtlasLoadParam)userData;
		param.mCallback?.Invoke(atlas, param.mUserData);
	}
	// 同步加载图集
	protected UGUIAtlas loadAtlas(string atlasName, bool errorIfNull = true)
	{
		if (mSpriteNameList.ContainsKey(atlasName))
		{
			return null;
		}
		var sprites = mResourceManager.loadSubResource<Sprite>(atlasName, errorIfNull);
		return atlasLoaded(sprites, atlasName);
	}
	// 图集资源已经加载完成,从assets中解析并创建图集信息
	protected UGUIAtlas atlasLoaded<T>(T[] assets, string loadPath) where T : UnityEngine.Object
	{
		if (assets == null || assets.Length == 0)
		{
			return null;
		}
		Texture2D texture = null;
		// 找到Texture,位置不一定是第一个,需要遍历查找
		int count = assets.Length;
		for(int i = 0; i < count; ++i)
		{
			T item = assets[i];
			if (item is Texture2D)
			{
				texture = item as Texture2D;
				break;
			}
		}
		if (texture == null)
		{
			logError("can not find texture2D in loaded Texture:" + loadPath);
		}
		// 找出所有的精灵
		var spriteList = new Dictionary<string, Sprite>();
		for (int i = 0; i < count; ++i)
		{
			T item = assets[i];
			if (item is Sprite)
			{
				spriteList.Add(item.name, item as Sprite);
			}
		}
		UGUIAtlas atlas = new UGUIAtlas();
		atlas.mTexture = texture;
		mSpriteNameList.Add(loadPath, spriteList);
		mSpriteList.Add(atlas, spriteList);
		mAtlasNameList.Add(loadPath, atlas);
		mAtlasList.Add(atlas, loadPath);
		return atlas;
	}
}