using UnityEngine.U2D;

// 对SpriteAtlas的封装,主要是实现对Sprite的缓存
public class UGUIAtlas : AtlasBase
{
	public SpriteAtlas mSpriteAtlas;			// 图集对象,尽量不要直接访问公有变量
	protected string mAtlasName;                // 由于直接访问.name每次都会有GC,所以使用一个变量存储
	public override bool isValid()				{ return mSpriteAtlas != null; }
	public override string getName()			{ return mAtlasName; }
	public SpriteAtlas getAtlas()				{ return mSpriteAtlas; }
	public void setAtlas(SpriteAtlas atlas)		
	{
		mSpriteAtlas = atlas;
		mAtlasName = mSpriteAtlas.name;
	}
}