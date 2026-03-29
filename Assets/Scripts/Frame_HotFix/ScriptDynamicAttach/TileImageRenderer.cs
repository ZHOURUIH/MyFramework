using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;

// 纯Sprite显示,不支持更多的操作,暂时只支持TPAtlas,如果使用SpriteAtlas,纹理坐标会计算错误
[RequireComponent(typeof(CanvasRenderer))]
public class TileImageRenderer : MonoBehaviour
{
	public Material mMaterial;							// 如果不设置材质,则使用默认的UI材质
	public int mVertCount;								// 用于查看调试信息
	public int mTileCount;                              // 用于查看调试信息

	protected CanvasRenderer mCanvasRenderer;			// CanvasRenderer
	protected List<TileData> mTileItems;                // 直接引用的外部列表
	protected Dictionary<object, TileData> mTileMap;    // 直接引用的外部列表,mTileItems和mTileMap只能用一个,哪个方便就用哪个
	protected Texture mTexture;							// 显示的图片信息
	protected Mesh mMesh;								// 生成的模型
	protected const int mMaxTileCount = 10000;			// 最多只支持同时显示1万个Sprite,因为单个模型的索引数量限制在65535
	protected SpriteVertex[] mVertices = new SpriteVertex[mMaxTileCount * 4];
	protected int[] mIndices = new int[mMaxTileCount * 6];
	void Awake()
	{
		mCanvasRenderer = GetComponent<CanvasRenderer>();
		mMesh = new Mesh();
		mMesh.MarkDynamic();
		mCanvasRenderer.SetMaterial(mMaterial != null ? mMaterial : Canvas.GetDefaultCanvasMaterial(), null);
		// 由于索引都是相对固定的,所以可以预先计算好
		for (int i = 0; i < mIndices.Length / 6; ++i)
		{
			mIndices[i * 6 + 0] = i * 4 + 0;
			mIndices[i * 6 + 1] = i * 4 + 1;
			mIndices[i * 6 + 2] = i * 4 + 2;
			mIndices[i * 6 + 3] = i * 4 + 1;
			mIndices[i * 6 + 4] = i * 4 + 3;
			mIndices[i * 6 + 5] = i * 4 + 2;
		}
		mCanvasRenderer.SetTexture(Texture2D.whiteTexture);
	}
	public void setTileList(List<TileData> tileItems) 
	{
		mTileItems = tileItems;
		if (!tileItems.isEmpty())
		{
			setTexture(tileItems.get(0).mSpriteData.mTexture);
		}
	}
	public void setTileMap(Dictionary<object, TileData> tileItems) 
	{
		mTileMap = tileItems;
		if (!tileItems.isEmpty())
		{
			setTexture(tileItems.firstValue().mSpriteData.mTexture);
		}
	}
	public void clearTile()
	{
		UN_CLASS_LIST(mTileItems);
		UN_CLASS_LIST(mTileMap);
	}
	public List<TileData> getTiles() { return mTileItems; }
	public Dictionary<object, TileData> getTileMap() { return mTileMap; }
	public void setTexture(Texture tex) 
	{
		mTexture = tex;
		mCanvasRenderer.SetTexture(mTexture);
	}
	public Texture getTexture() { return mTexture; }
	void LateUpdate()
	{
		{
			using var a = new ProfilerScope(0);
			mVertCount = 0;
			mTileCount = 0;
			if (!mTileItems.isEmpty())
			{
				mTileCount = mTileItems.Count;
				if (mTileCount * 4 >= mVertices.Length)
				{
					logWarning("已经超出了顶点上限,最多只允许" + IToS(mVertices.Length) + "个顶点");
					return;
				}
				foreach (TileData item in mTileItems)
				{
					float sizeX = item.mSize.x;
					float sizeY = item.mSize.y;
					float posX = item.mPosition.x;
					float posY = item.mPosition.y;

					Vector2[] uvs = item.mSpriteData.mUVs;
					float startX = posX - sizeX * 0.5f;
					float itemBottomPosY = posY - sizeY * 0.5f;
					ref SpriteVertex v0 = ref mVertices[mVertCount + 0];
					ref SpriteVertex v1 = ref mVertices[mVertCount + 1];
					ref SpriteVertex v2 = ref mVertices[mVertCount + 2];
					ref SpriteVertex v3 = ref mVertices[mVertCount + 3];

					v0.mPositionX = startX;
					v0.mPositionY = itemBottomPosY + sizeY;
					v0.mUV = uvs[0];

					v1.mPositionX = startX + sizeX;
					v1.mPositionY = itemBottomPosY + sizeY;
					v1.mUV = uvs[1];

					v2.mPositionX = startX;
					v2.mPositionY = itemBottomPosY;
					v2.mUV = uvs[2];

					v3.mPositionX = startX + sizeX;
					v3.mPositionY = itemBottomPosY;
					v3.mUV = uvs[3];
					mVertCount += 4;
				}
			}
			else if (!mTileMap.isEmpty())
			{
				mTileCount = mTileMap.Count;
				if (mTileCount * 4 >= mVertices.Length)
				{
					logWarning("已经超出了顶点上限,最多只允许" + IToS(mVertices.Length) + "个顶点");
					return;
				}
				foreach (var pair in mTileMap)
				{
					TileData item = pair.Value;
					float sizeX = item.mSize.x;
					float sizeY = item.mSize.y;
					float posX = item.mPosition.x;
					float posY = item.mPosition.y;

					Vector2[] uvs = item.mSpriteData.mUVs;
					float startX = posX - sizeX * 0.5f;
					float itemBottomPosY = posY - sizeY * 0.5f;
					ref SpriteVertex v0 = ref mVertices[mVertCount + 0];
					ref SpriteVertex v1 = ref mVertices[mVertCount + 1];
					ref SpriteVertex v2 = ref mVertices[mVertCount + 2];
					ref SpriteVertex v3 = ref mVertices[mVertCount + 3];

					v0.mPositionX = startX;
					v0.mPositionY = itemBottomPosY + sizeY;
					v0.mUV = uvs[0];

					v1.mPositionX = startX + sizeX;
					v1.mPositionY = itemBottomPosY + sizeY;
					v1.mUV = uvs[1];

					v2.mPositionX = startX;
					v2.mPositionY = itemBottomPosY;
					v2.mUV = uvs[2];

					v3.mPositionX = startX + sizeX;
					v3.mPositionY = itemBottomPosY;
					v3.mUV = uvs[3];
					mVertCount += 4;
				}
			}
		}

		if (mMesh.vertexCount != mVertCount)
		{
			mMesh.Clear();
			mMesh.SetVertexBufferParams(mVertCount,
				new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
		}
		mMesh.SetIndices(mIndices, 0, mVertCount / 4 * 6, MeshTopology.Triangles, 0, false);
		mMesh.SetVertexBufferData(mVertices, 0, 0, mVertCount, 0,
			MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
		mCanvasRenderer.SetMesh(mMesh);
	}
}