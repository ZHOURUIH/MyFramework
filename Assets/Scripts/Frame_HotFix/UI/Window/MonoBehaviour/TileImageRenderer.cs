using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;

// 纯Sprite显示,不支持更多的操作,暂时只支持TPAtlas,如果使用SpriteAtlas,纹理坐标会计算错误
// 由于是手动生成Mesh,所以不太适合在同一个Canvas中与其他节点进行排序显示,一般都是渲染在Canvas内的最底层或者最上层
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileImageRenderer : MonoBehaviour
{
	public int mVertCount;									// 用于在面板上查看调试信息
	public int mTileCount;									// 用于在面板上查看调试信息
	protected Dictionary<object, TileRenderData> mTileMap;	// 直接引用的外部列表,mTileItems和mTileMap只能用一个,哪个方便就用哪个
	protected MeshFilter mMeshFilter;
	protected MeshRenderer mMeshRenderer;
	protected List<TileRenderData> mTileItems;              // 直接引用的外部列表
	protected MaterialPropertyBlock mPropBlock;
	protected Texture mLastTexture;
	protected int mLastIndexTileCount;
	protected Mesh mMesh;									// 生成的模型
	protected const int mMaxTileCount = 10000;				// 最多只支持同时显示1万个Sprite,因为单个模型的索引数量限制在65535
	protected SpriteVertex[] mVertices = new SpriteVertex[mMaxTileCount * 4];
	protected int[] mIndices = new int[mMaxTileCount * 6];
	protected bool mDirty;
	void Awake()
	{
		ensureInit();
	}
	void OnDisable()
	{
		if (mMesh != null)
		{
			mMesh.Clear();
		}
		mLastTexture = null;
	}
	private void OnEnable()
	{
		mDirty = true;
		mLastIndexTileCount = 0;
		mLastTexture = null;
	}
	public void setSortingLayerName(string name) 
	{
		ensureInit();
		mMeshRenderer.sortingLayerName = name; 
	}
	public void setSortingOrder(int order) 
	{
		ensureInit();
		mMeshRenderer.sortingOrder = order; 
	}
	public void setTileList(List<TileRenderData> tileItems) 
	{
		mTileItems = tileItems;
		mDirty = true;
	}
	public void setTileMap(Dictionary<object, TileRenderData> tileItems) 
	{
		mTileMap = tileItems;
		mDirty = true;
	}
	public void clearTile()
	{
		UN_CLASS_LIST(mTileItems);
		UN_CLASS_LIST(mTileMap);
		mDirty = true;
	}
	public int getTileCount() { return mTileMap != null ? mTileMap.Count : mTileItems.count(); }
	void Update()
	{
		if (!mDirty)
		{
			return;
		}
		mDirty = false;
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
				// 这里的属性名字固定的,一般也就使用默认的材质即可
				Texture tex = mTileItems.get(0).mSpriteData.mTexture;
				if (mLastTexture != tex)
				{
					mLastTexture = tex;
					mPropBlock.SetTexture("_MainTex", tex);
					mMeshRenderer.SetPropertyBlock(mPropBlock);
				}
				foreach (TileRenderData item in mTileItems)
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
				Texture tex = mTileMap.firstValue().mSpriteData.mTexture;
				if (mLastTexture != tex)
				{
					mLastTexture = tex;
					mPropBlock.SetTexture("_MainTex", tex);
					mMeshRenderer.SetPropertyBlock(mPropBlock);
				}
				foreach (var pair in mTileMap)
				{
					TileRenderData item = pair.Value;
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

		if (mMesh.vertexCount < mVertCount)
		{
			mMesh.Clear();
			mMesh.SetVertexBufferParams(mVertCount,
				new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
		}
		if (mLastIndexTileCount != mTileCount)
		{
			mLastIndexTileCount = mTileCount;
			mMesh.SetIndices(mIndices, 0, mVertCount / 4 * 6, MeshTopology.Triangles, 0, false);
		}
		mMesh.SetVertexBufferData(mVertices, 0, 0, mVertCount, 0,
			MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 由于创建节点时可能会有与没有启用active而没有调用Awake,所以只能在任何可能需要的地方去尝试初始化
	protected void ensureInit()
	{
		if (mMeshFilter != null)
		{
			return;
		}
		mMeshFilter = GetComponent<MeshFilter>();
		mMeshRenderer = GetComponent<MeshRenderer>();
		mPropBlock = new();
		mMesh = new Mesh();
		mMesh.MarkDynamic();
		// 超大包围盒，防止被裁剪
		mMesh.bounds = new Bounds(Vector3.zero, new Vector3(100000, 100000, 1000));
		mMeshFilter.sharedMesh = mMesh;
		if (mMeshRenderer.sharedMaterial == null)
		{
			mMeshRenderer.sharedMaterial = new(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
		}
		setSortingLayerName("Default");
		setSortingOrder(0);
		for (int i = 0; i < mIndices.Length / 6; ++i)
		{
			mIndices[i * 6 + 0] = i * 4 + 0;
			mIndices[i * 6 + 1] = i * 4 + 1;
			mIndices[i * 6 + 2] = i * 4 + 2;
			mIndices[i * 6 + 3] = i * 4 + 1;
			mIndices[i * 6 + 4] = i * 4 + 3;
			mIndices[i * 6 + 5] = i * 4 + 2;
		}
	}
}