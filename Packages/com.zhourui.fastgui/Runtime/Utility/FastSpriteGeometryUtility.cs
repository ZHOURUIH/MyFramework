using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Sprites;

internal struct FastSpriteSimpleQuadData
{
	public Vector2 mPosition0;
	public Vector2 mPosition1;
	public Vector2 mPosition2;
	public Vector2 mPosition3;
	public Vector2 mUV0;
	public Vector2 mUV1;
	public Vector2 mUV2;
	public Vector2 mUV3;
	public ushort mIndex0;
	public ushort mIndex1;
	public ushort mIndex2;
	public ushort mIndex3;
	public ushort mIndex4;
	public ushort mIndex5;

	public readonly Vector2 getPosition(int index)
	{
		return index switch
		{
			0 => mPosition0,
			1 => mPosition1,
			2 => mPosition2,
			_ => mPosition3,
		};
	}

	public readonly Vector2 getUV(int index)
	{
		return index switch
		{
			0 => mUV0,
			1 => mUV1,
			2 => mUV2,
			_ => mUV3,
		};
	}

	public readonly ushort getIndex(int index)
	{
		return index switch
		{
			0 => mIndex0,
			1 => mIndex1,
			2 => mIndex2,
			3 => mIndex3,
			4 => mIndex4,
			_ => mIndex5,
		};
	}
}

internal static class FastSpriteGeometryUtility
{
	private const float EPSILON = 0.0001f;
	private const int MAX_TILED_AXIS_COUNT = 256;
	private const int MAX_TILED_QUAD_COUNT = 4096;

	private sealed class SpriteMeshData
	{
		public Vector2[] mVertices;
		public Vector2[] mUV;
		public ushort[] mTriangles;
		public Vector3 mBoundsCenter;
		public Texture mTexture;
		public bool mSimpleQuadValid;
		public FastSpriteSimpleQuadData mSimpleQuad;
		public int mSimpleGeometryID;
		public int mSimpleAssetID;
	}

	private readonly struct SimpleGeometryKey : IEquatable<SimpleGeometryKey>
	{
		private readonly Vector2 mPosition0;
		private readonly Vector2 mPosition1;
		private readonly Vector2 mPosition2;
		private readonly Vector2 mPosition3;
		private readonly ushort mIndex0;
		private readonly ushort mIndex1;
		private readonly ushort mIndex2;
		private readonly ushort mIndex3;
		private readonly ushort mIndex4;
		private readonly ushort mIndex5;

		public SimpleGeometryKey(FastSpriteSimpleQuadData quad)
		{
			mPosition0 = quad.mPosition0;
			mPosition1 = quad.mPosition1;
			mPosition2 = quad.mPosition2;
			mPosition3 = quad.mPosition3;
			mIndex0 = quad.mIndex0;
			mIndex1 = quad.mIndex1;
			mIndex2 = quad.mIndex2;
			mIndex3 = quad.mIndex3;
			mIndex4 = quad.mIndex4;
			mIndex5 = quad.mIndex5;
		}

		public bool Equals(SimpleGeometryKey other)
		{
			return mPosition0.Equals(other.mPosition0) &&
				mPosition1.Equals(other.mPosition1) &&
				mPosition2.Equals(other.mPosition2) &&
				mPosition3.Equals(other.mPosition3) &&
				mIndex0 == other.mIndex0 &&
				mIndex1 == other.mIndex1 &&
				mIndex2 == other.mIndex2 &&
				mIndex3 == other.mIndex3 &&
				mIndex4 == other.mIndex4 &&
				mIndex5 == other.mIndex5;
		}

		public override bool Equals(object obj)
		{
			return obj is SimpleGeometryKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = mPosition0.GetHashCode();
				hash = hash * 397 ^ mPosition1.GetHashCode();
				hash = hash * 397 ^ mPosition2.GetHashCode();
				hash = hash * 397 ^ mPosition3.GetHashCode();
				hash = hash * 397 ^ mIndex0;
				hash = hash * 397 ^ mIndex1;
				hash = hash * 397 ^ mIndex2;
				hash = hash * 397 ^ mIndex3;
				hash = hash * 397 ^ mIndex4;
				hash = hash * 397 ^ mIndex5;
				return hash;
			}
		}
	}

	private const int SPRITE_MESH_HOT_CACHE_SIZE = 256;
	private static readonly FastDictionary<int, SpriteMeshData> sSpriteMeshCache = new();
	private static readonly Dictionary<SimpleGeometryKey, int> sSimpleGeometryIDs = new();
	private static int sNextSimpleGeometryID = 1;
	private static int sNextSimpleAssetID = 1;
	private static readonly Sprite[] sSpriteMeshHotSprites = new Sprite[SPRITE_MESH_HOT_CACHE_SIZE];
	private static readonly SpriteMeshData[] sSpriteMeshHotData = new SpriteMeshData[SPRITE_MESH_HOT_CACHE_SIZE];

	internal static bool tryGetSimpleQuadData(Sprite sprite, out FastSpriteSimpleQuadData quad)
	{
		quad = default;
		if (sprite == null)
		{
			return false;
		}
		SpriteMeshData mesh = getSpriteMeshData(sprite);
		if (!mesh.mSimpleQuadValid)
		{
			return false;
		}
		quad = mesh.mSimpleQuad;
		return true;
	}

	internal static void getSimpleMeshHeader(
		Sprite sprite,
		out int vertexCount,
		out int indexCount,
		out bool quadValid,
		out Vector3 boundsCenter,
		out Texture texture,
		out int simpleGeometryID,
		out int simpleAssetID)
	{
		if (sprite == null)
		{
			vertexCount = 0;
			indexCount = 0;
			quadValid = false;
			boundsCenter = Vector3.zero;
			texture = null;
			simpleGeometryID = 0;
			simpleAssetID = 0;
			return;
		}
		SpriteMeshData mesh = getSpriteMeshData(sprite);
		vertexCount = mesh.mVertices.Length;
		indexCount = mesh.mTriangles.Length;
		quadValid = mesh.mSimpleQuadValid;
		boundsCenter = mesh.mBoundsCenter;
		texture = mesh.mTexture;
		simpleGeometryID = mesh.mSimpleGeometryID;
		simpleAssetID = mesh.mSimpleAssetID;
	}

	internal static void getSimpleMeshInfo(
		Sprite sprite,
		out int vertexCount,
		out int indexCount,
		out bool quadValid,
		out FastSpriteSimpleQuadData quad,
		out Vector3 boundsCenter,
		out Texture texture,
		out int simpleGeometryID,
		out int simpleAssetID)
	{
		getSimpleMeshHeader(
			sprite,
			out vertexCount,
			out indexCount,
			out quadValid,
			out boundsCenter,
			out texture,
			out simpleGeometryID,
			out simpleAssetID);
		quad = quadValid && sprite != null ? getSpriteMeshData(sprite).mSimpleQuad : default;
	}

	internal static void clearSpriteMeshCache()
	{
		sSpriteMeshCache.Clear();
		// Geometry IDs are process-lifetime interned values. Existing renderers can still hold
		// cached IDs when the Sprite mesh cache is cleared, so recycling the ID namespace would
		// make unrelated geometry compare equal. Keeping this tiny numeric-key table also lets a
		// reloaded Sprite with identical geometry recover the same ID.
		Array.Clear(sSpriteMeshHotSprites, 0, sSpriteMeshHotSprites.Length);
		Array.Clear(sSpriteMeshHotData, 0, sSpriteMeshHotData.Length);
	}

	public static void build(
		FastSpriteRenderer renderer,
		Matrix4x4 worldToBatch,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		vertices.Clear();
		indices.Clear();
		if (renderer == null || renderer.getSprite() == null)
		{
			return;
		}

		Sprite sprite = renderer.getSprite();
		Matrix4x4 localToBatch = worldToBatch * renderer.transform.localToWorldMatrix;
		Color32 color = renderer.getColor();
		buildInternal(renderer, sprite, localToBatch, color, true, vertices, indices);
	}

	// Builds geometry in renderer-local space. FastSpriteBatch keeps these local positions
	// so Transform-only changes can update only vertex positions without regenerating UV,
	// color, topology, sliced grids, or tiled quads.
	internal static void buildLocal(
		FastSpriteRenderer renderer,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		vertices.Clear();
		indices.Clear();
		if (renderer == null || renderer.getSprite() == null)
		{
			return;
		}

		Sprite sprite = renderer.getSprite();
		Color32 color = renderer.getColor();
		buildInternal(renderer, sprite, Matrix4x4.identity, color, false, vertices, indices);
	}

	private static void buildInternal(
		FastSpriteRenderer renderer,
		Sprite sprite,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		switch (renderer.getDrawMode())
		{
			case SpriteDrawMode.Sliced:
				buildSliced(renderer, sprite, localToBatch, color, transformPositions, vertices, indices);
				break;
			case SpriteDrawMode.Tiled:
				buildTiled(renderer, sprite, localToBatch, color, transformPositions, vertices, indices);
				break;
			default:
				buildSimple(renderer, sprite, localToBatch, color, transformPositions, vertices, indices);
				break;
		}
	}

	public static Bounds getLocalBounds(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.getSprite() == null)
		{
			return new Bounds(Vector3.zero, Vector3.zero);
		}
		if (renderer.getDrawMode() == SpriteDrawMode.Simple)
		{
			return renderer.getSprite().bounds;
		}
		Rect rect = getLocalRect(renderer, renderer.getSprite());
		return new Bounds(
			new Vector3(rect.center.x, rect.center.y, 0.0f),
			new Vector3(rect.width, rect.height, 0.0f));
	}

	public static Bounds getWorldBounds(FastSpriteRenderer renderer)
	{
		Bounds local = getLocalBounds(renderer);
		if (renderer == null)
		{
			return local;
		}
		Matrix4x4 matrix = renderer.transform.localToWorldMatrix;
		Vector3 center = matrix.MultiplyPoint3x4(local.center);
		Vector3 ext = local.extents;
		Vector3 axisX = matrix.MultiplyVector(new Vector3(ext.x, 0.0f, 0.0f));
		Vector3 axisY = matrix.MultiplyVector(new Vector3(0.0f, ext.y, 0.0f));
		Vector3 axisZ = matrix.MultiplyVector(new Vector3(0.0f, 0.0f, ext.z));
		Vector3 worldExtents = new(
			Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
			Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
			Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
		return new Bounds(center, worldExtents * 2.0f);
	}

	private static void buildSimple(
		FastSpriteRenderer renderer,
		Sprite sprite,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		// Use the Sprite's imported mesh so Mesh Type = Tight behaves like SpriteRenderer.
		// Geometry is cached per Sprite to avoid allocating sprite.vertices / uv / triangles
		// arrays on every transform or color update.
		SpriteMeshData mesh = getSpriteMeshData(sprite);
		bool reverseWinding = renderer.getFlipX() ^ renderer.getFlipY();
		for (int i = 0; i < mesh.mVertices.Length; ++i)
		{
			Vector2 local = mesh.mVertices[i];
			if (renderer.getFlipX())
			{
				local.x = -local.x;
			}
			if (renderer.getFlipY())
			{
				local.y = -local.y;
			}
			Vector3 position = new(local.x, local.y, 0.0f);
			if (transformPositions)
			{
				position = localToBatch.MultiplyPoint3x4(position);
			}
			vertices.Add(new FastSpriteVertex(position, color, mesh.mUV[i]));
		}
		for (int i = 0; i + 2 < mesh.mTriangles.Length; i += 3)
		{
			if (reverseWinding)
			{
				indices.Add(mesh.mTriangles[i]);
				indices.Add(mesh.mTriangles[i + 2]);
				indices.Add(mesh.mTriangles[i + 1]);
			}
			else
			{
				indices.Add(mesh.mTriangles[i]);
				indices.Add(mesh.mTriangles[i + 1]);
				indices.Add(mesh.mTriangles[i + 2]);
			}
		}
	}

	private static void buildSliced(
		FastSpriteRenderer renderer,
		Sprite sprite,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		if (sprite.border == Vector4.zero)
		{
			buildStretched(renderer, sprite, localToBatch, color, transformPositions, vertices, indices);
			return;
		}

		Rect rect = getLocalRect(renderer, sprite);
		float ppu = Mathf.Max(sprite.pixelsPerUnit, EPSILON);
		Vector4 border = sprite.border / ppu;
		adjustBorderToRect(rect, ref border);

		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		float[] xs =
		{
			rect.xMin, rect.xMin + border.x, rect.xMax - border.z, rect.xMax
		};
		float[] ys =
		{
			rect.yMin, rect.yMin + border.y, rect.yMax - border.w, rect.yMax
		};
		float[] us =
		{
			outer.x, inner.x, inner.z, outer.z
		};
		float[] vs =
		{
			outer.y, inner.y, inner.w, outer.w
		};

		// Shared 4x4 grid: 16 vertices / 54 indices, matching the natural nine-slice topology.
		for (int y = 0; y < 4; ++y)
		{
			int vIndex = renderer.getFlipY() ? 3 - y : y;
			for (int x = 0; x < 4; ++x)
			{
				int uIndex = renderer.getFlipX() ? 3 - x : x;
				Vector3 position = new(xs[x], ys[y], 0.0f);
				if (transformPositions)
				{
					position = localToBatch.MultiplyPoint3x4(position);
				}
				vertices.Add(new FastSpriteVertex(position, color, new Vector2(us[uIndex], vs[vIndex])));
			}
		}
		for (int y = 0; y < 3; ++y)
		{
			for (int x = 0; x < 3; ++x)
			{
				int a = y * 4 + x;
				int b = (y + 1) * 4 + x;
				int c = (y + 1) * 4 + x + 1;
				int d = y * 4 + x + 1;
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				indices.Add(c);
				indices.Add(d);
				indices.Add(a);
			}
		}
	}

	private static void buildTiled(
		FastSpriteRenderer renderer,
		Sprite sprite,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		Rect rect = getLocalRect(renderer, sprite);
		Rect nativeRect = getNativeLocalRect(sprite);
		if (renderer.getTileMode() == SpriteTileMode.Adaptive)
		{
			float thresholdScale = 1.0f + Mathf.Clamp01(renderer.getAdaptiveModeThreshold());
			float scaleX = nativeRect.width > EPSILON ? rect.width / nativeRect.width : 1.0f;
			float scaleY = nativeRect.height > EPSILON ? rect.height / nativeRect.height : 1.0f;
			if (scaleX < thresholdScale && scaleY < thresholdScale)
			{
				// Adaptive behaves like a stretched Sprite before the tiling threshold.
				buildStretched(renderer, sprite, localToBatch, color, transformPositions, vertices, indices);
				return;
			}
		}

		float ppu = Mathf.Max(sprite.pixelsPerUnit, EPSILON);
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		Vector4 border = sprite.border / ppu;
		adjustBorderToRect(rect, ref border);

		float x0 = rect.xMin;
		float x1 = rect.xMin + border.x;
		float x2 = rect.xMax - border.z;
		float x3 = rect.xMax;
		float y0 = rect.yMin;
		float y1 = rect.yMin + border.y;
		float y2 = rect.yMax - border.w;
		float y3 = rect.yMax;

		float centerPixelWidth = Mathf.Max(sprite.rect.width - sprite.border.x - sprite.border.z, 0.0f);
		float centerPixelHeight = Mathf.Max(sprite.rect.height - sprite.border.y - sprite.border.w, 0.0f);
		float tileWidth = centerPixelWidth > EPSILON ? centerPixelWidth / ppu : Mathf.Max(x2 - x1, EPSILON);
		float tileHeight = centerPixelHeight > EPSILON ? centerPixelHeight / ppu : Mathf.Max(y2 - y1, EPSILON);

		if (sprite.border == Vector4.zero)
		{
			tileArea(renderer, localToBatch, color, transformPositions, rect,
				new Rect(outer.x, outer.y, outer.z - outer.x, outer.w - outer.y),
				tileWidth, tileHeight, true, true, vertices, indices);
			return;
		}

		addQuad(renderer, localToBatch, color, transformPositions, x0, y0, x1, y1, outer.x, outer.y, inner.x, inner.y, vertices, indices);
		addQuad(renderer, localToBatch, color, transformPositions, x2, y0, x3, y1, inner.z, outer.y, outer.z, inner.y, vertices, indices);
		addQuad(renderer, localToBatch, color, transformPositions, x0, y2, x1, y3, outer.x, inner.w, inner.x, outer.w, vertices, indices);
		addQuad(renderer, localToBatch, color, transformPositions, x2, y2, x3, y3, inner.z, inner.w, outer.z, outer.w, vertices, indices);

		if (x2 > x1 + EPSILON)
		{
			tileArea(renderer, localToBatch, color, transformPositions,
				new Rect(x1, y0, x2 - x1, y1 - y0),
				new Rect(inner.x, outer.y, inner.z - inner.x, inner.y - outer.y),
				tileWidth, Mathf.Max(y1 - y0, EPSILON), true, false, vertices, indices);
			tileArea(renderer, localToBatch, color, transformPositions,
				new Rect(x1, y2, x2 - x1, y3 - y2),
				new Rect(inner.x, inner.w, inner.z - inner.x, outer.w - inner.w),
				tileWidth, Mathf.Max(y3 - y2, EPSILON), true, false, vertices, indices);
		}
		if (y2 > y1 + EPSILON)
		{
			tileArea(renderer, localToBatch, color, transformPositions,
				new Rect(x0, y1, x1 - x0, y2 - y1),
				new Rect(outer.x, inner.y, inner.x - outer.x, inner.w - inner.y),
				Mathf.Max(x1 - x0, EPSILON), tileHeight, false, true, vertices, indices);
			tileArea(renderer, localToBatch, color, transformPositions,
				new Rect(x2, y1, x3 - x2, y2 - y1),
				new Rect(inner.z, inner.y, outer.z - inner.z, inner.w - inner.y),
				Mathf.Max(x3 - x2, EPSILON), tileHeight, false, true, vertices, indices);
		}
		if (x2 > x1 + EPSILON && y2 > y1 + EPSILON)
		{
			tileArea(renderer, localToBatch, color, transformPositions,
				new Rect(x1, y1, x2 - x1, y2 - y1),
				new Rect(inner.x, inner.y, inner.z - inner.x, inner.w - inner.y),
				tileWidth, tileHeight, true, true, vertices, indices);
		}
	}

	private static void buildStretched(
		FastSpriteRenderer renderer,
		Sprite sprite,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		Rect rect = getLocalRect(renderer, sprite);
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		addQuad(renderer, localToBatch, color, transformPositions,
			rect.xMin, rect.yMin, rect.xMax, rect.yMax,
			outer.x, outer.y, outer.z, outer.w,
			vertices, indices);
	}

	private static void tileArea(
		FastSpriteRenderer renderer,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		Rect positionRect,
		Rect uvRect,
		float tileWidth,
		float tileHeight,
		bool tileX,
		bool tileY,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		if (positionRect.width <= EPSILON || positionRect.height <= EPSILON)
		{
			return;
		}
		int countX = tileX ? Mathf.Clamp(Mathf.CeilToInt(positionRect.width / Mathf.Max(tileWidth, EPSILON)), 1, MAX_TILED_AXIS_COUNT) : 1;
		int countY = tileY ? Mathf.Clamp(Mathf.CeilToInt(positionRect.height / Mathf.Max(tileHeight, EPSILON)), 1, MAX_TILED_AXIS_COUNT) : 1;
		if (countX * countY > MAX_TILED_QUAD_COUNT)
		{
			if (countX >= countY)
			{
				countX = Mathf.Max(1, MAX_TILED_QUAD_COUNT / countY);
			}
			else
			{
				countY = Mathf.Max(1, MAX_TILED_QUAD_COUNT / countX);
			}
		}

		float cellWidth = tileX ? Mathf.Max(tileWidth, EPSILON) : positionRect.width;
		float cellHeight = tileY ? Mathf.Max(tileHeight, EPSILON) : positionRect.height;
		for (int y = 0; y < countY; ++y)
		{
			float py0 = positionRect.yMin + y * cellHeight;
			float py1 = Mathf.Min(py0 + cellHeight, positionRect.yMax);
			if (py1 <= py0 + EPSILON)
			{
				continue;
			}
			float fy = tileY ? (py1 - py0) / cellHeight : 1.0f;
			for (int x = 0; x < countX; ++x)
			{
				float px0 = positionRect.xMin + x * cellWidth;
				float px1 = Mathf.Min(px0 + cellWidth, positionRect.xMax);
				if (px1 <= px0 + EPSILON)
				{
					continue;
				}
				float fx = tileX ? (px1 - px0) / cellWidth : 1.0f;
				addQuad(renderer, localToBatch, color, transformPositions,
					px0, py0, px1, py1,
					uvRect.xMin, uvRect.yMin,
					Mathf.Lerp(uvRect.xMin, uvRect.xMax, fx),
					Mathf.Lerp(uvRect.yMin, uvRect.yMax, fy),
					vertices, indices);
			}
		}
	}

	private static void addQuad(
		FastSpriteRenderer renderer,
		Matrix4x4 localToBatch,
		Color32 color,
		bool transformPositions,
		float x0, float y0, float x1, float y1,
		float u0, float v0, float u1, float v1,
		List<FastSpriteVertex> vertices,
		List<int> indices)
	{
		if (renderer.getFlipX())
		{
			(u0, u1) = (u1, u0);
		}
		if (renderer.getFlipY())
		{
			(v0, v1) = (v1, v0);
		}

		int start = vertices.Count;
		Vector3 p0 = new(x0, y0, 0.0f);
		Vector3 p1 = new(x0, y1, 0.0f);
		Vector3 p2 = new(x1, y1, 0.0f);
		Vector3 p3 = new(x1, y0, 0.0f);
		if (transformPositions)
		{
			p0 = localToBatch.MultiplyPoint3x4(p0);
			p1 = localToBatch.MultiplyPoint3x4(p1);
			p2 = localToBatch.MultiplyPoint3x4(p2);
			p3 = localToBatch.MultiplyPoint3x4(p3);
		}
		vertices.Add(new FastSpriteVertex(p0, color, new Vector2(u0, v0)));
		vertices.Add(new FastSpriteVertex(p1, color, new Vector2(u0, v1)));
		vertices.Add(new FastSpriteVertex(p2, color, new Vector2(u1, v1)));
		vertices.Add(new FastSpriteVertex(p3, color, new Vector2(u1, v0)));

		indices.Add(start + 0);
		indices.Add(start + 1);
		indices.Add(start + 2);
		indices.Add(start + 2);
		indices.Add(start + 3);
		indices.Add(start + 0);
	}

	private static SpriteMeshData getSpriteMeshData(Sprite sprite)
	{
		int referenceHash = RuntimeHelpers.GetHashCode(sprite);
		int hotSlot = (referenceHash ^ (referenceHash >> 7) ^ (referenceHash >> 15)) & (SPRITE_MESH_HOT_CACHE_SIZE - 1);
		if (ReferenceEquals(sSpriteMeshHotSprites[hotSlot], sprite))
		{
			SpriteMeshData hot = sSpriteMeshHotData[hotSlot];
			if (hot != null)
			{
				return hot;
			}
		}

		int id = FastUnityObjectIDUtility.getLegacyIntID(sprite);
		if (sSpriteMeshCache.TryGetValue(id, out SpriteMeshData cached))
		{
			sSpriteMeshHotSprites[hotSlot] = sprite;
			sSpriteMeshHotData[hotSlot] = cached;
			return cached;
		}
		SpriteMeshData data = new()
		{
			mVertices = sprite.vertices,
			mUV = sprite.uv,
			mTriangles = sprite.triangles,
			mBoundsCenter = sprite.bounds.center,
			mTexture = sprite.texture,
		};
		if (data.mVertices.Length == 4 && data.mUV.Length == 4 && data.mTriangles.Length == 6)
		{
			data.mSimpleQuadValid = true;
			data.mSimpleQuad = new FastSpriteSimpleQuadData
			{
				mPosition0 = data.mVertices[0],
				mPosition1 = data.mVertices[1],
				mPosition2 = data.mVertices[2],
				mPosition3 = data.mVertices[3],
				mUV0 = data.mUV[0],
				mUV1 = data.mUV[1],
				mUV2 = data.mUV[2],
				mUV3 = data.mUV[3],
				mIndex0 = data.mTriangles[0],
				mIndex1 = data.mTriangles[1],
				mIndex2 = data.mTriangles[2],
				mIndex3 = data.mTriangles[3],
				mIndex4 = data.mTriangles[4],
				mIndex5 = data.mTriangles[5],
			};
			SimpleGeometryKey geometryKey = new(data.mSimpleQuad);
			if (!sSimpleGeometryIDs.TryGetValue(geometryKey, out int geometryID))
			{
				geometryID = sNextSimpleGeometryID++;
				sSimpleGeometryIDs.Add(geometryKey, geometryID);
			}
			data.mSimpleGeometryID = geometryID;
			data.mSimpleAssetID = sNextSimpleAssetID++;
		}
		sSpriteMeshCache[id] = data;
		sSpriteMeshHotSprites[hotSlot] = sprite;
		sSpriteMeshHotData[hotSlot] = data;
		return data;
	}

	private static Rect getNativeLocalRect(Sprite sprite)
	{
		float ppu = Mathf.Max(sprite.pixelsPerUnit, EPSILON);
		Rect pixelRect = sprite.rect;
		Vector2 pivot = sprite.pivot;
		return Rect.MinMaxRect(
			-pivot.x / ppu,
			-pivot.y / ppu,
			(pixelRect.width - pivot.x) / ppu,
			(pixelRect.height - pivot.y) / ppu);
	}

	private static Rect getLocalRect(FastSpriteRenderer renderer, Sprite sprite)
	{
		if (renderer.getDrawMode() == SpriteDrawMode.Simple)
		{
			return getNativeLocalRect(sprite);
		}
		Vector2 size = renderer.getSize();
		if (size.x <= EPSILON || size.y <= EPSILON)
		{
			return new Rect(0.0f, 0.0f, Mathf.Max(size.x, 0.0f), Mathf.Max(size.y, 0.0f));
		}
		Rect spriteRect = sprite.rect;
		Vector2 pivot = sprite.pivot;
		float pivotX = spriteRect.width > EPSILON ? pivot.x / spriteRect.width : 0.5f;
		float pivotY = spriteRect.height > EPSILON ? pivot.y / spriteRect.height : 0.5f;
		float xMin = -size.x * pivotX;
		float yMin = -size.y * pivotY;
		return new Rect(xMin, yMin, size.x, size.y);
	}

	private static void adjustBorderToRect(Rect rect, ref Vector4 border)
	{
		float horizontal = border.x + border.z;
		if (horizontal > rect.width && horizontal > EPSILON)
		{
			float scale = rect.width / horizontal;
			border.x *= scale;
			border.z *= scale;
		}
		float vertical = border.y + border.w;
		if (vertical > rect.height && vertical > EPSILON)
		{
			float scale = rect.height / vertical;
			border.y *= scale;
			border.w *= scale;
		}
	}
}
