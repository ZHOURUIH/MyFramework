using System;
using UnityEngine;

// GeometryRange更新结果。
// VertexRangeChanged表示顶点地址发生变化；IndexCountChanged表示实际Quad数量变化；IndexCapacityChanged表示保留Index区间需要重新布局。
public struct FastUIGeometryUpdateResult
{
	public int mVertexStart;
	public int mVertexCount;
	public bool mVertexRangeChanged;
	public bool mIndexCountChanged;
	public bool mIndexCapacityChanged;
	public FastUIGeometryUpdateResult(int vertexStart, int vertexCount, bool vertexRangeChanged, bool indexCountChanged, bool indexCapacityChanged)
	{
		mVertexStart = vertexStart;
		mVertexCount = vertexCount;
		mVertexRangeChanged = vertexRangeChanged;
		mIndexCountChanged = indexCountChanged;
		mIndexCapacityChanged = indexCapacityChanged;
	}
}
// FastUI可变几何的临时构建缓冲。
// Canvas在主线程逐个消费Dirty元素时复用同一个实例，因此不会因为Sliced、Tiled或Filled反复产生托管数组GC。
// 当前几何以Quad为基本存储单元；Filled产生三角形时使用退化Quad，DrawOrder只维护一种固定6索引模板。
public sealed class FastUIGeometryBuilder
{
	// 这里只是单个Dirty元素构建期间复用的Scratch Buffer，不保存跨帧业务状态。
	// 持久顶点仍统一存放在EasyECS；Scratch使用数组可以避免每处理一个元素都触发ECS结构变化和Column重新获取。
	private Vector3[] mPositions = new Vector3[16];
	private Vector2[] mUVs = new Vector2[16];
	private Vector4[] mTMPUV0s = new Vector4[16];
	private Vector2[] mTMPUV2s = new Vector2[16];
	private Color32[] mColors = new Color32[16];
	private int mVertexCount;
	private bool mHasVertexColors;
	private bool mHasTMPVertexData;
	public int getVertexCount()
	{
		return mVertexCount;
	}
	public int getQuadCount()
	{
		return mVertexCount >> 2;
	}
	public Vector3[] getPositions()
	{
		return mPositions;
	}
	public Vector2[] getUVs()
	{
		return mUVs;
	}
	public Vector4[] getTMPUV0s()
	{
		return mTMPUV0s;
	}
	public Vector2[] getTMPUV2s()
	{
		return mTMPUV2s;
	}
	public Color32[] getColors()
	{
		return mColors;
	}
	public bool hasVertexColors()
	{
		return mHasVertexColors;
	}
	public bool hasTMPVertexData()
	{
		return mHasTMPVertexData;
	}
	public void Clear()
	{
		mVertexCount = 0;
		mHasVertexColors = false;
		mHasTMPVertexData = false;
	}
	// 添加一个标准Quad。顶点顺序始终沿多边形边界保持一致，DrawOrder据此生成固定索引模板。
	public void AddQuad(
		Vector3 bottomLeft,
		Vector3 topLeft,
		Vector3 topRight,
		Vector3 bottomRight,
		Vector2 uvBottomLeft,
		Vector2 uvTopLeft,
		Vector2 uvTopRight,
		Vector2 uvBottomRight)
	{
		ensureCapacity(mVertexCount + 4);
		int start = mVertexCount;
		mPositions[start + 0] = bottomLeft;
		mPositions[start + 1] = topLeft;
		mPositions[start + 2] = topRight;
		mPositions[start + 3] = bottomRight;
		mUVs[start + 0] = uvBottomLeft;
		mUVs[start + 1] = uvTopLeft;
		mUVs[start + 2] = uvTopRight;
		mUVs[start + 3] = uvBottomRight;
		mVertexCount += 4;
	}
	public void AddQuad(
		Vector3 bottomLeft,
		Vector3 topLeft,
		Vector3 topRight,
		Vector3 bottomRight,
		Vector2 uvBottomLeft,
		Vector2 uvTopLeft,
		Vector2 uvTopRight,
		Vector2 uvBottomRight,
		Color32 color)
	{
		int start = mVertexCount;
		AddQuad(bottomLeft, topLeft, topRight, bottomRight, uvBottomLeft, uvTopLeft, uvTopRight, uvBottomRight);
		mColors[start + 0] = color;
		mColors[start + 1] = color;
		mColors[start + 2] = color;
		mColors[start + 3] = color;
		mHasVertexColors = true;
	}
	// 添加TMP兼容Quad。UV0使用Vector4，UV2写入第二套纹理坐标；普通Image仍不会触碰这两组Scratch数据。
	public void AddTMPQuad(
		Vector3 bottomLeft,
		Vector3 topLeft,
		Vector3 topRight,
		Vector3 bottomRight,
		Vector4 uv0BottomLeft,
		Vector4 uv0TopLeft,
		Vector4 uv0TopRight,
		Vector4 uv0BottomRight,
		Vector2 uv2BottomLeft,
		Vector2 uv2TopLeft,
		Vector2 uv2TopRight,
		Vector2 uv2BottomRight)
	{
		ensureCapacity(mVertexCount + 4);
		int start = mVertexCount;
		mPositions[start + 0] = bottomLeft;
		mPositions[start + 1] = topLeft;
		mPositions[start + 2] = topRight;
		mPositions[start + 3] = bottomRight;
		mUVs[start + 0] = new Vector2(uv0BottomLeft.x, uv0BottomLeft.y);
		mUVs[start + 1] = new Vector2(uv0TopLeft.x, uv0TopLeft.y);
		mUVs[start + 2] = new Vector2(uv0TopRight.x, uv0TopRight.y);
		mUVs[start + 3] = new Vector2(uv0BottomRight.x, uv0BottomRight.y);
		mTMPUV0s[start + 0] = uv0BottomLeft;
		mTMPUV0s[start + 1] = uv0TopLeft;
		mTMPUV0s[start + 2] = uv0TopRight;
		mTMPUV0s[start + 3] = uv0BottomRight;
		mTMPUV2s[start + 0] = uv2BottomLeft;
		mTMPUV2s[start + 1] = uv2TopLeft;
		mTMPUV2s[start + 2] = uv2TopRight;
		mTMPUV2s[start + 3] = uv2BottomRight;
		mVertexCount += 4;
		mHasTMPVertexData = true;
	}
	public void AddTMPQuad(
		Vector3 bottomLeft,
		Vector3 topLeft,
		Vector3 topRight,
		Vector3 bottomRight,
		Vector4 uv0BottomLeft,
		Vector4 uv0TopLeft,
		Vector4 uv0TopRight,
		Vector4 uv0BottomRight,
		Vector2 uv2BottomLeft,
		Vector2 uv2TopLeft,
		Vector2 uv2TopRight,
		Vector2 uv2BottomRight,
		Color32 color)
	{
		int start = mVertexCount;
		AddTMPQuad(bottomLeft, topLeft, topRight, bottomRight, uv0BottomLeft, uv0TopLeft, uv0TopRight, uv0BottomRight,
			uv2BottomLeft, uv2TopLeft, uv2TopRight, uv2BottomRight);
		mColors[start + 0] = color;
		mColors[start + 1] = color;
		mColors[start + 2] = color;
		mColors[start + 3] = color;
		mHasVertexColors = true;
	}
	public void AddQuad(Rect positionRect, Rect uvRect)
	{
		AddQuad(
			new Vector3(positionRect.xMin, positionRect.yMin, 0.0f),
			new Vector3(positionRect.xMin, positionRect.yMax, 0.0f),
			new Vector3(positionRect.xMax, positionRect.yMax, 0.0f),
			new Vector3(positionRect.xMax, positionRect.yMin, 0.0f),
			new Vector2(uvRect.xMin, uvRect.yMin),
			new Vector2(uvRect.xMin, uvRect.yMax),
			new Vector2(uvRect.xMax, uvRect.yMax),
			new Vector2(uvRect.xMax, uvRect.yMin)
		);
	}
	// Filled Radial可能产生三角形。为了让整个FastUI继续只维护一种索引模板，
	// 三角形使用“第3/4顶点重合”的退化Quad表示，第二个三角形面积为0，不需要额外索引格式。
	public void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
	{
		AddQuad(p0, p1, p2, p2, uv0, uv1, uv2, uv2);
	}
	private void ensureCapacity(int requiredVertexCount)
	{
		if (requiredVertexCount <= mPositions.Length)
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredVertexCount, 16));
		Array.Resize(ref mPositions, capacity);
		Array.Resize(ref mUVs, capacity);
		Array.Resize(ref mTMPUV0s, capacity);
		Array.Resize(ref mTMPUV2s, capacity);
		Array.Resize(ref mColors, capacity);
	}
}
