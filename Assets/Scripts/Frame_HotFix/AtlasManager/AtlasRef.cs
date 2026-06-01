using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 开放给外部以持有UGUIAtlas或者TPAtlas,可以进行引用计数
// 因为每一个持有的地方都需要记录一个独立的Token,用于检测释放的合法性
// 所以需要使用AtlasRef来封装UGUIAtlas或者TPAtlas给外部使用
public class AtlasRef : ClassObject
{
	private AtlasBase mAtlas;				// 引用的图集
	private long mToken;                    // 引用凭证,一般不允许外部直接访问
	private static long mTokenSeed;         // 用于生成一个引用凭证
	public override void resetProperty()
	{
		base.resetProperty();
		mAtlas = null;
		mToken = 0;
	}
	public void setAtlas(AtlasBase atlas)
	{
		mAtlas = atlas;
		mToken = 0;
		if (mAtlas == null)
		{
			logError("atlas is null");
			return;
		}
		mToken = generateToken();
		mAtlas.addReference(mToken);
	}
	public bool isValid()								{ return mAtlas != null && mAtlas.isValid(); }
	public Sprite getSprite(string name)				{ return mAtlas?.getSprite(name); }
	public bool hasSprite(Sprite sprite)				{ return mAtlas != null && mAtlas.hasSprite(sprite); }
	public string getFirstSpriteName()					{ return mAtlas?.getFirstSpriteName(); }
	public Dictionary<string, Sprite> getSpriteList()	{ return mAtlas?.getSpriteList(); }
	public string getFilePath()							{ return mAtlas?.getFilePath(); }
	public AtlasBase getAtlas()							{ return mAtlas; }
	public string getAtlasSingleName()					{ return mAtlas?.getName(); }
	public Vector2 getFirstSpriteSize()					{ return mAtlas.getFirstSpriteSize(); }
	public long getToken()								{ return mToken; }
	// 只能由AtlasLoaderBase调用
	public override void destroy()
	{
		base.destroy();
		if (mAtlas == null)
		{
			logError("atlas is null");
			return;
		}
		mAtlas.removeReference(ref mToken);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static long generateToken() { return ++mTokenSeed; }
}