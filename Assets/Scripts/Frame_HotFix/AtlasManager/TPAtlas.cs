using UnityEngine;
using static StringUtility;

// 对TexturePacket生成的MultiSprite图集的封装,主要是实现对Sprite的缓存
public class TPAtlas : AtlasBase
{
	public Texture2D mTexture;									// 图集对象,尽量不要直接访问公有变量
	public override bool isValid()				{ return mTexture != null; }
	public override string getName()			{ return mTexture != null ? mTexture.name : EMPTY; }
	public Texture2D getAtlas()					{ return mTexture; }
	public void setAtlas(Texture2D atlas)		{ mTexture = atlas; }
}