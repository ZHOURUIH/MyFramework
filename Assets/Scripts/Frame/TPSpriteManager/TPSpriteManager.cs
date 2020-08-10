using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 用于UGUI的multi sprite管理
public class TPSpriteManager : FrameComponent
{
	protected Dictionary<string, Dictionary<string, Sprite>> mSpriteNameList;// key是图集的路径,相对于GameResources
	protected Dictionary<UnityEngine.Object, Dictionary<string, Sprite>> mSpriteList;
	protected Dictionary<string, UnityEngine.Object> mAtlasNameList;		// key是图集的路径,相对于GameResources
	protected Dictionary<UnityEngine.Object, string> mAtlasList;            // value是图集的路径,相对于GameResources
	public TPSpriteManager(string name)
		: base(name)
	{
		mSpriteNameList = new Dictionary<string, Dictionary<string, Sprite>>();
		mSpriteList = new Dictionary<UnityEngine.Object, Dictionary<string, Sprite>>();
		mAtlasNameList = new Dictionary<string, UnityEngine.Object>();
		mAtlasList = new Dictionary<UnityEngine.Object, string>();
	}
	public Dictionary<string, Sprite> getSprites(string atlasName, bool errorIfNull = true)
	{
		if(!mSpriteNameList.ContainsKey(atlasName))
		{
			loadAtlas(atlasName, errorIfNull);
		}
		if(mSpriteNameList.ContainsKey(atlasName))
		{
			return mSpriteNameList[atlasName];
		}
		return null;
	}
	public Dictionary<string, Sprite> getSprites(UnityEngine.Object atlas)
	{
		return mSpriteList.ContainsKey(atlas) ? mSpriteList[atlas] : null;
	}
	public Sprite getSprite(UnityEngine.Object atlas, string spriteName)
	{
		if(atlas == null || spriteName == null)
		{
			return null;
		}
		if(mSpriteList.ContainsKey(atlas) && mSpriteList[atlas].ContainsKey(spriteName))
		{
			return mSpriteList[atlas][spriteName];
		}
		return null;
	}
	public Sprite getSprite(string atlasName, string spriteName)
	{
		if(atlasName == null || spriteName == null)
		{
			return null;
		}
		if(!mSpriteNameList.ContainsKey(atlasName))
		{
			loadAtlas(atlasName);
		}
		if(mSpriteNameList.ContainsKey(atlasName) && mSpriteNameList[atlasName].ContainsKey(spriteName))
		{
			return mSpriteNameList[atlasName][spriteName];
		}
		return null;
	}
	public Sprite getSprite(string atlasName, string spriteName, bool errorIfNull, bool loadIfNull)
	{
		if(!mSpriteNameList.ContainsKey(atlasName) && loadIfNull)
		{
			loadAtlas(atlasName, errorIfNull);
		}
		if(mSpriteNameList.ContainsKey(atlasName) && mSpriteNameList[atlasName].ContainsKey(spriteName))
		{
			return mSpriteNameList[atlasName][spriteName];
		}
		return null;
	}
	public Texture2D getAtlas(string atlasName, bool errorInNull, bool loadIfNull)
	{
		if (!mAtlasNameList.ContainsKey(atlasName) && loadIfNull)
		{
			loadAtlas(atlasName, errorInNull);
		}
		if (mAtlasNameList.ContainsKey(atlasName))
		{
			return mAtlasNameList[atlasName] as Texture2D;
		}
		return null;
	}
	public void getAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorInNull, bool loadIfNull)
	{
		if (mAtlasNameList.ContainsKey(atlasName))
		{
			callback?.Invoke(mAtlasNameList[atlasName] as Texture2D, userData);
			return;
		}
		if (!mAtlasNameList.ContainsKey(atlasName) && loadIfNull)
		{
			loadAtlasAsync(atlasName, callback, userData, errorInNull);
		}
	}
	public void unloadAtlas(string atlasName)
	{
		if(mAtlasNameList.ContainsKey(atlasName))
		{
			UnityEngine.Object atlas = mAtlasNameList[atlasName];
			mAtlasNameList.Remove(atlasName);
			mAtlasList.Remove(atlas);
			mSpriteNameList[atlasName].Clear();
			mSpriteNameList.Remove(atlasName);
			mSpriteList.Remove(atlas);
			mResourceManager.unload(ref atlas);
		}
	}
	//---------------------------------------------------------------------------------------------------------------
	protected void onAssetLoadDone(UnityEngine.Object asset, UnityEngine.Object[] assets, byte[] bytes, object[] userData, string loadPath)
	{
		Texture2D atlas = null;
		if(!mAtlasNameList.ContainsKey(loadPath))
		{
			if (assets != null && assets.Length > 0)
			{
				// 找到Texture,位置不一定是第一个,需要遍历查找
				foreach (var item in assets)
				{
					if (item is Texture2D)
					{
						atlas = item as Texture2D;
						break;
					}
				}
				// 找出所有的精灵
				var spriteList = new Dictionary<string, Sprite>();
				foreach (var item in assets)
				{
					if (item is Sprite)
					{
						spriteList.Add(item.name, item as Sprite);
					}
				}
				mSpriteNameList.Add(loadPath, spriteList);
				mSpriteList.Add(atlas, spriteList);
				mAtlasNameList.Add(loadPath, atlas);
				mAtlasList.Add(atlas, loadPath);
			}
		}
		else
		{
			atlas = mAtlasNameList[loadPath] as Texture2D;
		}
		((AtlasLoadDone)userData[0])?.Invoke(atlas, userData[1]);
	}
	protected void loadAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorIfNull = true)
	{
		if (!mSpriteNameList.ContainsKey(atlasName))
		{
			object[] userDatas = new object[] { callback, userData };
			mResourceManager.loadSubResourceAsync<Sprite>(atlasName, onAssetLoadDone, userDatas, errorIfNull);
		}
	}
	protected void loadAtlas(string atlasName, bool errorIfNull = true)
	{
		if (mSpriteNameList.ContainsKey(atlasName))
		{
			return;
		}
		var sprites = mResourceManager.loadSubResource<Sprite>(atlasName, errorIfNull);
		if (sprites != null && sprites.Length > 0)
		{
			// 找到Texture,位置不一定是第一个,需要遍历查找
			Texture2D atlas = null;
			foreach (var item in sprites)
			{
				if (item is Texture2D)
				{
					atlas = item as Texture2D;
					break;
				}
			}
			// 找出所有的精灵
			var spriteList = new Dictionary<string, Sprite>();
			foreach (var item in sprites)
			{
				if (item is Sprite)
				{
					spriteList.Add(item.name, item as Sprite);
				}
			}
			mSpriteNameList.Add(atlasName, spriteList);
			mSpriteList.Add(atlas, spriteList);
			mAtlasNameList.Add(atlasName, atlas);
			mAtlasList.Add(atlas, atlasName);
		}
	}
}