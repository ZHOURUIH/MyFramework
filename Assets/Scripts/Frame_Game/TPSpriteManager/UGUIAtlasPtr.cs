using UnityEngine;
using static UnityUtility;

public class UGUIAtlasPtr : ClassObject
{
	private UGUIAtlas mAtlas;						// 引用的图集
	private long mToken;                            // 引用凭证,一般不允许外部直接访问
	public override void resetProperty()
	{
		base.resetProperty();
		mAtlas = null;
		mToken = 0;
	}
	public void setAtlas(UGUIAtlas atlas)
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
	public UGUIAtlas getAtlas() { return mAtlas; }
	public long getToken() { return mToken; }
	// 只能由AtlasLoaderBase调用
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
	//------------------------------------------------------------------------------------------------------------------------------
	private void use()
	{
		if (mAtlas == null)
		{
			logError("atlas is null");
			return;
		}
		mToken = UGUIAtlas.generateToken();
		mAtlas.addReference(mToken);
	}
}