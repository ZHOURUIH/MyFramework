using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastInputFieldSelectionGraphic : FastUIRenderElement
{
	// 选择区域通常只有1~4个Rect，但多行选择时会按需增长；这里按完整Rect连续访问且属于组件本地Scratch，保留小型AoS数组比ECS容器更合适。
	protected Rect[] mRects = new Rect[4];
	protected int mRectCount;
	public override Texture getRenderTexture()
	{
		return Texture2D.whiteTexture;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.InputFieldSelection;
	}
	public int getRectCount()
	{
		return mRectCount;
	}
	public Rect getRect(int index)
	{
		return index >= 0 && index < mRectCount ? mRects[index] : default;
	}
	public void setRects(Rect[] rects, int count)
	{
		count = Mathf.Max(count, 0);
		ensureRectCapacity(count);
		bool changed = mRectCount != count;
		for (int i = 0; i < count; ++i)
		{
			Rect rect = rects[i];
			if (mRects[i] != rect)
			{
				mRects[i] = rect;
				changed = true;
			}
		}
		mRectCount = count;
		if (changed)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setRect(Rect rect)
	{
		ensureRectCapacity(1);
		if (mRectCount == 1 && mRects[0] == rect)
		{
			return;
		}
		mRects[0] = rect;
		mRectCount = 1;
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
	public void clearRects()
	{
		if (mRectCount == 0)
		{
			return;
		}
		mRectCount = 0;
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		builder.Clear();
		for (int i = 0; i < mRectCount; ++i)
		{
			Rect rect = mRects[i];
			if (rect.width <= 0.0f || rect.height <= 0.0f)
			{
				continue;
			}
			Vector3 bottomLeft = transformGeometryPoint(canvasWorldToLocal, new Vector3(rect.xMin, rect.yMin, 0.0f));
			Vector3 topLeft = transformGeometryPoint(canvasWorldToLocal, new Vector3(rect.xMin, rect.yMax, 0.0f));
			Vector3 topRight = transformGeometryPoint(canvasWorldToLocal, new Vector3(rect.xMax, rect.yMax, 0.0f));
			Vector3 bottomRight = transformGeometryPoint(canvasWorldToLocal, new Vector3(rect.xMax, rect.yMin, 0.0f));
			builder.AddQuad(bottomLeft, topLeft, topRight, bottomRight, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
		}
	}
	private void ensureRectCapacity(int required)
	{
		if (required <= mRects.Length)
		{
			return;
		}
		System.Array.Resize(ref mRects, Mathf.NextPowerOfTwo(Mathf.Max(required, 4)));
	}
}
