using UnityEngine.U2D;
using UObject = UnityEngine.Object;

// 对SpriteAtlas的封装,主要是实现对Sprite的缓存
public class AtlasUGUI : AtlasBase
{
	protected SpriteAtlas mSpriteAtlas;			// 图集对象
	protected string mAtlasName;                // 由于直接访问.name每次都会有GC,所以使用一个变量存储
	public AtlasUGUI(ResourceRef<UObject> asset) : base(asset) { }
	public override bool isValid()				{ return mSpriteAtlas != null; }
	public override string getName()			{ return mAtlasName; }
	public SpriteAtlas getAtlas()				{ return mSpriteAtlas; }
	public void setAtlas(SpriteAtlas atlas)		
	{
		mSpriteAtlas = atlas;
		mAtlasName = mSpriteAtlas.name;
	}
}