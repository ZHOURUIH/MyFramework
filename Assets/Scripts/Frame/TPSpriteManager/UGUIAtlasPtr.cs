using UnityEngine;
using System;
using static UnityUtility;

public struct UGUIAtlasPtr
{
	public static UGUIAtlasPtr Default;
	public UGUIAtlas mAtlas;        // 引用的图集
	public long mToken;				// 引用凭证,一般不允许外部直接访问
	public UGUIAtlasPtr(UGUIAtlas atlas)
	{
		mAtlas = atlas;
		mToken = 0;
		if (mAtlas?.mTexture != null)
		{
			use();
		}
	}
	public bool isValid() { return mAtlas != null; }
	public Sprite getSprite(string name) { return mAtlas?.getSprite(name); }
	public string getName() { return mAtlas?.mFilePath; }
	public Texture2D getTexture() { return mAtlas?.mTexture; }
	public void use() 
	{
		if (mAtlas == null)
		{
			logError("atlas is null");
			return;
		}
		mToken = mAtlas.generateToken();
		mAtlas.addReference(mToken);
	}
	public void unuse()
	{
		if (mAtlas == null)
		{
			logError("atlas is null");
			return;
		}
		mAtlas.removeReference(mToken);
		mToken = 0;
	}
}