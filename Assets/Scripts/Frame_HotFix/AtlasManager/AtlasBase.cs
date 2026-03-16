using UnityEngine;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static UnityUtility;

// 封装的Atlas的基类,通用于SpriteAtlas或MultiSprite的图集
public abstract class AtlasBase
{
	protected Dictionary<string, Sprite> mSpriteList = new();		// 图集中包含的所有图片列表
	protected Dictionary<Sprite, string> mSpriteNameList = new();   // 用于通过Sprite获取名字,由于直接调用.name会有GC,所以通过查找
	protected HashSet<long> mReferenceTokenList = new();			// 引用凭证,用于判断是否在其他地方被引用
	protected static long mTokenSeed;								// 用于生成一个引用凭证
	protected string mFilePath;										// 图集文件路径,相对于GameResources
	public UObject mMainAsset;										// 图集中精灵的主资源,不一定与mTexture一致,用于卸载资源
	public Sprite getSprite(string name) { return !name.isEmpty() ? mSpriteList.get(name) : null; }
	public abstract bool isValid();
	public Dictionary<string, Sprite> getSpriteList() { return mSpriteList; }
	public abstract string getName();
	public string getFilePath() { return mFilePath; }
	public bool hasSprite(Sprite sprite) 
	{
		using var a = new ProfilerScope("hasSprite");
		return sprite != null && mSpriteNameList.ContainsKey(sprite); 
	}
	public string getFirstSpriteName() { return mSpriteList.firstKey(); }
	public static long generateToken() { return ++mTokenSeed; }
	public int getReferenceCount() { return mReferenceTokenList.Count; }
	public void setFilePath(string filePath) { mFilePath = filePath; }
	public void addSprite(Sprite sprite) 
	{
		string name = sprite.name;
		mSpriteList.Add(name, sprite);
		mSpriteNameList.Add(sprite, name); 
	}
	public Vector2 getFirstSpriteSize()
	{
		Sprite sprite = mSpriteList.firstValue();
		if (sprite != null)
		{
			return sprite.rect.size;
		}
		return Vector2.zero;
	}
	public void destroy()
	{
		mSpriteList.Clear();
		mSpriteNameList.Clear();
		mFilePath = null;
		mMainAsset = null;
	}
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
			logError("移除引用凭证失败,可能是重复移除一个图集");
		}
	}
}