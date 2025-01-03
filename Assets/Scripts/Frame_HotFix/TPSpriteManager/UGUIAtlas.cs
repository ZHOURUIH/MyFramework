using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using UObject = UnityEngine.Object;

public class UGUIAtlas
{
	protected HashSet<long> mReferenceTokenList = new();	// 引用凭证,用于判断是否在其他地方被引用
	protected static long mTokenSeed;						// 用于生成一个引用凭证
	public Dictionary<string, Sprite> mSpriteList = new();	// 图集中包含的所有图片列表
	public Texture2D mTexture;								// 图集图片
	public string mFilePath;								// 图集文件路径,相对于GameResources
	public UObject mMainAsset;								// 图集中精灵的主资源,不一定与mTexture一致,用于卸载资源
	public Sprite getSprite(string name)
	{
		return !name.isEmpty() ? mSpriteList.get(name) : null;
	}
	public void destroy()
	{
		mSpriteList.Clear();
		mTexture = null;
		mFilePath = null;
		mMainAsset = null;
	}
	public static long generateToken() { return ++mTokenSeed; }
	public int getReferenceCount() { return mReferenceTokenList.Count; }
	public void addReference(long token)
	{
		if (!mReferenceTokenList.Add(token))
		{
			logError("添加引用凭证失败");
		}
	}
	public void removeReference(long token)
	{
		if (!mReferenceTokenList.Remove(token))
		{
			logError("移除引用凭证失败");
		}
	}
}