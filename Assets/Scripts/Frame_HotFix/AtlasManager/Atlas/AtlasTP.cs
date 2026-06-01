using UnityEngine;
using UObject = UnityEngine.Object;

// 对TexturePacket生成的MultiSprite图集的封装,主要是实现对Sprite的缓存
public class AtlasTP : AtlasBase
{
	protected Texture2D mTexture;				// 图集对象
	protected string mTextureName;              // 由于直接访问.name每次都会有GC,所以使用一个变量存储
	public AtlasTP(ResourceRef<UObject> asset) : base(asset) { }
	public override bool isValid()				{ return mTexture != null; }
	public override string getName()			{ return mTextureName; }
	public Texture2D getAtlas()					{ return mTexture; }
	public void setAtlas(Texture2D atlas)		
	{
		mTexture = atlas; 
		mTextureName = mTexture.name; 
	}
}