using UnityEngine;

// FastUI的RawImage：只负责“直接Texture + UVRect + RectTransform单Quad”这一明确语义。
// Canvas/颜色/材质/Mask/Transform/Dirty等通用Graphic能力位于FastUIRenderElement。
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastRawImage : FastUIRenderElement
{
	[SerializeField] protected Texture mTexture;
	[SerializeField] protected Rect mUVRect = new(0.0f, 0.0f, 1.0f, 1.0f);
	public Texture getTexture()
	{
		return mTexture;
	}
	public Rect getUVRect()
	{
		return mUVRect;
	}
	public override Texture getRenderTexture()
	{
		return mTexture != null ? mTexture : Texture2D.whiteTexture;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.RawImage;
	}
	public override bool tryGetSimpleUVRect(out Rect uvRect)
	{
		uvRect = mUVRect;
		return true;
	}
	public override bool tryGetSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		ensureInit();
		calculatePositionVertices(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
		return true;
	}
	public override bool tryGetRuntimeSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, bool useCachedLocalState, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (!useCachedLocalState)
		{
			return tryGetSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
		}
		ensureInit();
		calculateRuntimeCachedSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
		return true;
	}
	public void setTexture(Texture texture)
	{
		if (mTexture == texture)
		{
			return;
		}
		mTexture = texture;
		if (mCanvas != null)
		{
			mCanvas.notifyElementTextureChanged(this);
		}
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicBatchChanged();
		}
	}
	public void setUVRect(Rect uvRect)
	{
		if (mUVRect == uvRect)
		{
			return;
		}
		mUVRect = uvRect;
		markVertexDirty(FastUIDirtyFlags.UV0Only);
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicGeometryChanged();
		}
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		builder.Clear();
		calculatePositionVertices(canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight);
		float uvLeft = mUVRect.x;
		float uvBottom = mUVRect.y;
		float uvRight = mUVRect.x + mUVRect.width;
		float uvTop = mUVRect.y + mUVRect.height;
		builder.AddQuad(bottomLeft, topLeft, topRight, bottomRight,
			new Vector2(uvLeft, uvBottom), new Vector2(uvLeft, uvTop), new Vector2(uvRight, uvTop), new Vector2(uvRight, uvBottom));
	}
	public void buildVertexStreams(Matrix4x4 canvasWorldToLocal, Vector3[] positions, Color32[] colors, Vector2[] uvs, int vertexStart)
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		updatePositionVertices(canvasWorldToLocal, positions, vertexStart);
		updateColorVertices(colors, vertexStart);
		updateUVVertices(uvs, vertexStart);
	}
	public void updateVertexStreams(Matrix4x4 canvasWorldToLocal, Vector3[] positions, Color32[] colors, Vector2[] uvs, int vertexStart, FastUIDirtyFlags flags)
	{
		if ((flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0)
		{
			updatePositionVertices(canvasWorldToLocal, positions, vertexStart);
		}
		if ((flags & FastUIDirtyFlags.Color) != 0)
		{
			updateColorVertices(colors, vertexStart);
		}
		if ((flags & FastUIDirtyFlags.AnyUV) != 0)
		{
			updateUVVertices(uvs, vertexStart);
		}
	}
	public void getPositionVertices(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		ensureInit();
		calculatePositionVertices(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
	}
	private void updatePositionVertices(Matrix4x4 canvasWorldToLocal, Vector3[] positions, int vertexStart)
	{
		calculatePositionVertices(canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight);
		positions[vertexStart + 0] = bottomLeft;
		positions[vertexStart + 1] = topLeft;
		positions[vertexStart + 2] = topRight;
		positions[vertexStart + 3] = bottomRight;
	}
	protected void calculatePositionVertices(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (mDirectChildOfCanvas)
		{
			if (mCachedLocalRotation == Quaternion.identity && mCachedLocalScale == Vector3.one)
			{
				Vector3 renderOriginOffset = getRenderOriginOffset();
				bottomLeft = mLocalBottomLeft + mCachedLocalPosition - renderOriginOffset;
				topLeft = mLocalTopLeft + mCachedLocalPosition - renderOriginOffset;
				topRight = mLocalTopRight + mCachedLocalPosition - renderOriginOffset;
				bottomRight = mLocalBottomRight + mCachedLocalPosition - renderOriginOffset;
			}
			else
			{
				bottomLeft = transformLocalPoint(mLocalBottomLeft);
				topLeft = transformLocalPoint(mLocalTopLeft);
				topRight = transformLocalPoint(mLocalTopRight);
				bottomRight = transformLocalPoint(mLocalBottomRight);
			}
		}
		else
		{
			Matrix4x4 matrix = canvasWorldToLocal * mRectTransform.localToWorldMatrix;
			bottomLeft = matrix.MultiplyPoint3x4(mLocalBottomLeft);
			topLeft = matrix.MultiplyPoint3x4(mLocalTopLeft);
			topRight = matrix.MultiplyPoint3x4(mLocalTopRight);
			bottomRight = matrix.MultiplyPoint3x4(mLocalBottomRight);
		}
	}
	private void updateColorVertices(Color32[] colors, int vertexStart)
	{
		Color32 color = mColor;
		colors[vertexStart + 0] = color;
		colors[vertexStart + 1] = color;
		colors[vertexStart + 2] = color;
		colors[vertexStart + 3] = color;
	}
	private void updateUVVertices(Vector2[] uvs, int vertexStart)
	{
		float uvLeft = mUVRect.x;
		float uvBottom = mUVRect.y;
		float uvRight = mUVRect.x + mUVRect.width;
		float uvTop = mUVRect.y + mUVRect.height;
		uvs[vertexStart + 0] = new Vector2(uvLeft, uvBottom);
		uvs[vertexStart + 1] = new Vector2(uvLeft, uvTop);
		uvs[vertexStart + 2] = new Vector2(uvRight, uvTop);
		uvs[vertexStart + 3] = new Vector2(uvRight, uvBottom);
	}
}
