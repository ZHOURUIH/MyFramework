using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;

// 也是用于批量渲染同一个图集中的Sprite,提高渲染效率,但是由于是使用的Graphic,所以渲染效率会低于TileImageRenderer
// 但是优点是方便参与Canvas内的渲染排序
public class TileImageGraphic : Graphic
{
	public List<TileRenderData> mTileItems;
	public Dictionary<object, TileRenderData> mTileMap;
	public int mTileCount;                              // 用于查看调试信息
	protected const int MAX_TILE = 10000;               // 最多只支持同时显示1万个Sprite,因为单个模型的索引数量限制在65535
	public override Texture mainTexture
	{
		get
		{
			if (mTileItems != null && mTileItems.Count > 0)
			{
				return mTileItems[0].mSpriteData.mTexture;
			}
			else if (mTileMap != null && mTileMap.Count > 0)
			{
				return mTileMap.firstValue()?.mSpriteData.mTexture;
			}
			return null;
		}
	}
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		using var a = new ProfilerScope(0);
		vh.Clear();
		if (!mTileItems.isEmpty())
		{
			mTileCount = mTileItems.Count;
			if (mTileItems.Count >= MAX_TILE)
			{
				logWarning("已经超出了地砖上限,最多只允许" + IToS(MAX_TILE) + "个地砖");
				return;
			}
			foreach (TileRenderData item in mTileItems)
			{
				float sizeX = item.mSize.x;
				float sizeY = item.mSize.y;
				Vector2[] uvs = item.mSpriteData.mUVs;
				float startX = item.mPosition.x - sizeX * 0.5f;
				float bottomY = item.mPosition.y - sizeY * 0.5f;
				int baseIndex = vh.currentVertCount;
				vh.AddVert(new(startX, bottomY + sizeY), color, uvs[0]);
				vh.AddVert(new(startX + sizeX, bottomY + sizeY), color, uvs[1]);
				vh.AddVert(new(startX, bottomY), color, uvs[2]);
				vh.AddVert(new(startX + sizeX, bottomY), color, uvs[3]);
				vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 2);
				vh.AddTriangle(baseIndex + 1, baseIndex + 3, baseIndex + 2);
			}
		}
		else if (!mTileMap.isEmpty())
		{
			mTileCount = mTileMap.Count;
			if (mTileMap.Count >= MAX_TILE)
			{
				logWarning("已经超出了地砖上限,最多只允许" + IToS(MAX_TILE) + "个地砖");
				return;
			}
			foreach (var pair in mTileMap)
			{
				TileRenderData item = pair.Value;
				float sizeX = item.mSize.x;
				float sizeY = item.mSize.y;
				Vector2[] uvs = item.mSpriteData.mUVs;
				float startX = item.mPosition.x - sizeX * 0.5f;
				float bottomY = item.mPosition.y - sizeY * 0.5f;
				int baseIndex = vh.currentVertCount;
				vh.AddVert(new(startX, bottomY + sizeY), color, uvs[0]);
				vh.AddVert(new(startX + sizeX, bottomY + sizeY), color, uvs[1]);
				vh.AddVert(new(startX, bottomY), color, uvs[2]);
				vh.AddVert(new(startX + sizeX, bottomY), color, uvs[3]);
				vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 2);
				vh.AddTriangle(baseIndex + 1, baseIndex + 3, baseIndex + 2);
			}
		}
	}
	public void SetTiles(List<TileRenderData> tiles)
	{
		mTileItems = tiles;
		SetVerticesDirty();
	}
	public void SetTileMap(Dictionary<object, TileRenderData> map)
	{
		mTileMap = map;
		SetVerticesDirty();
	}
	public void Update()
	{
		SetVerticesDirty();
	}
}