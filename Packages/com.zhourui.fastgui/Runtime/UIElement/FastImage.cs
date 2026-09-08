using UnityEngine;

// FastUI的Sprite图片组件。
// Simple/Sliced/Tiled/Filled全部通过真实Position/UV几何实现，不要求FastUI专用Shader。
// 一个FastImage仍然只占一个稳定VertexSlot，但该Slot可以映射到任意长度的GeometryRange。
public enum FastUIImageType
{
	Simple = 0,
	Sliced = 1,
	Tiled = 2,
	Filled = 3,
}
public enum FastUIImageFillMethod
{
	Horizontal = 0,
	Vertical = 1,
	Radial90 = 2,
	Radial180 = 3,
	Radial360 = 4,
}
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastImage : FastUIRenderElement
{
	[SerializeField] protected Sprite mSprite;
	protected Texture mCachedSpriteTexture;
	[SerializeField] protected FastUIImageType mType = FastUIImageType.Simple;
	[SerializeField] protected bool mFillCenter = true;
	[SerializeField] protected FastUIImageFillMethod mFillMethod = FastUIImageFillMethod.Horizontal;
	[SerializeField, Range(0.0f, 1.0f)] protected float mFillAmount = 1.0f;
	[SerializeField] protected bool mFillClockwise = true;
	[SerializeField] protected int mFillOrigin;
	[SerializeField] protected float mPixelsPerUnitMultiplier = 1.0f;
	public Sprite getSprite()
	{
		return mSprite;
	}
	public FastUIImageType getType()
	{
		return mType;
	}
	public bool getFillCenter()
	{
		return mFillCenter;
	}
	public FastUIImageFillMethod getFillMethod()
	{
		return mFillMethod;
	}
	public float getFillAmount()
	{
		return mFillAmount;
	}
	public bool getFillClockwise()
	{
		return mFillClockwise;
	}
	public int getFillOrigin()
	{
		return mFillOrigin;
	}
	public float getPixelsPerUnitMultiplier()
	{
		return mPixelsPerUnitMultiplier;
	}
	public override Texture getRenderTexture()
	{
		Texture texture = mCachedSpriteTexture;
		if (texture == null && mSprite != null)
		{
			texture = mSprite.texture;
			mCachedSpriteTexture = texture;
		}
		return texture != null ? texture : Texture2D.whiteTexture;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.Image;
	}
	public override bool tryGetSimpleUVRect(out Rect uvRect)
	{
		if (mType != FastUIImageType.Simple)
		{
			uvRect = default;
			return false;
		}
		uvRect = FastUIImageUtility.getOuterUVRect(mSprite);
		return true;
	}
	public override bool tryGetSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (mType != FastUIImageType.Simple)
		{
			bottomLeft = default;
			topLeft = default;
			topRight = default;
			bottomRight = default;
			return false;
		}
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		bottomLeft = transformGeometryPoint(canvasWorldToLocal, mLocalBottomLeft);
		topLeft = transformGeometryPoint(canvasWorldToLocal, mLocalTopLeft);
		topRight = transformGeometryPoint(canvasWorldToLocal, mLocalTopRight);
		bottomRight = transformGeometryPoint(canvasWorldToLocal, mLocalBottomRight);
		return true;
	}
	public override bool tryGetRuntimeSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, bool useCachedLocalState, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (!useCachedLocalState)
		{
			return tryGetSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
		}
		if (mType != FastUIImageType.Simple)
		{
			bottomLeft = default;
			topLeft = default;
			topRight = default;
			bottomRight = default;
			return false;
		}
		ensureInit();
		calculateRuntimeCachedSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
		return true;
	}
	public bool tryRebuildRuntimeSlicedGeometryDirect(FastUIMeshRenderer renderer, int slot, Matrix4x4 canvasWorldToLocal, Color32 color, out FastUIGeometryUpdateResult result)
	{
		result = default;
		if (renderer == null || slot < 0 || mType != FastUIImageType.Sliced)
		{
			return false;
		}
		ensureInit();
		Rect slicedRect = FastUIImageUtility.getSlicedFillRect(mCachedRect, mFillMethod, mFillAmount, mFillOrigin);
		if (!FastUIImageUtility.tryGetSlicedGrid(slicedRect, mSprite, mPixelsPerUnitMultiplier, out Vector4 x, out Vector4 y, out Vector4 u, out Vector4 v))
		{
			return false;
		}
		bool translationOnly = tryGetGeometryTranslation(out Vector3 translation);
		Matrix4x4 geometryMatrix = translationOnly ? default : getGeometryLocalToCanvasMatrix(canvasWorldToLocal, true);
		result = renderer.rebuildSlicedImageSlot(slot, x, y, u, v, mFillCenter, translationOnly, translation, geometryMatrix, color);
		return true;
	}
	public void setSprite(Sprite sprite)
	{
		if (mSprite == sprite)
		{
			return;
		}
		Texture oldTexture = mCachedSpriteTexture;
		mSprite = sprite;
		Texture newTexture = mSprite != null ? mSprite.texture : null;
		mCachedSpriteTexture = newTexture;
		if (oldTexture != newTexture)
		{
			notifyBatchChanged();
		}
		// 普通Simple图片切换Sprite时，Rect和顶点拓扑都不变，只需要更新UV。
		markVertexDirty(mType == FastUIImageType.Simple ? FastUIDirtyFlags.UV0Only : FastUIDirtyFlags.Geometry);
	}
	public void setType(FastUIImageType type)
	{
		if (mType == type)
		{
			return;
		}
		mType = type;
		if (mCanvas != null)
		{
			mCanvas.notifyInitialSimpleQuadCandidateChanged(this);
		}
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
	public void setFillCenter(bool fillCenter)
	{
		if (mFillCenter == fillCenter)
		{
			return;
		}
		mFillCenter = fillCenter;
		if (mType == FastUIImageType.Sliced || mType == FastUIImageType.Tiled)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setFillMethod(FastUIImageFillMethod fillMethod)
	{
		if (mFillMethod == fillMethod)
		{
			return;
		}
		mFillMethod = fillMethod;
		mFillOrigin = FastUIImageUtility.clampFillOrigin(mFillMethod, mFillOrigin);
		if (mType == FastUIImageType.Filled || mType == FastUIImageType.Sliced)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setFillAmount(float fillAmount)
	{
		fillAmount = Mathf.Clamp01(fillAmount);
		if (Mathf.Approximately(mFillAmount, fillAmount))
		{
			return;
		}
		mFillAmount = fillAmount;
		if (mType == FastUIImageType.Filled || mType == FastUIImageType.Sliced)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setFillClockwise(bool clockwise)
	{
		if (mFillClockwise == clockwise)
		{
			return;
		}
		mFillClockwise = clockwise;
		if (mType == FastUIImageType.Filled)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setFillOrigin(int fillOrigin)
	{
		fillOrigin = FastUIImageUtility.clampFillOrigin(mFillMethod, fillOrigin);
		if (mFillOrigin == fillOrigin)
		{
			return;
		}
		mFillOrigin = fillOrigin;
		if (mType == FastUIImageType.Filled || mType == FastUIImageType.Sliced)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setPixelsPerUnitMultiplier(float multiplier)
	{
		multiplier = Mathf.Max(multiplier, 0.01f);
		if (Mathf.Approximately(mPixelsPerUnitMultiplier, multiplier))
		{
			return;
		}
		mPixelsPerUnitMultiplier = multiplier;
		if (mType == FastUIImageType.Sliced || mType == FastUIImageType.Tiled)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setNativeSize()
	{
		if (mSprite == null)
		{
			return;
		}
		setSize(FastUIImageUtility.getNativeSize(mSprite));
	}
	// FastImage先在RectTransform局部空间生成真实几何，再统一转换到Canvas局部空间。
	// 几何Builder由Canvas复用；Tiled即使生成大量Quad也不会为每帧Dirty产生临时数组GC。
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		builder.Clear();
		Rect rect = mRectTransform.rect;
		switch (mType)
		{
			case FastUIImageType.Sliced:
				FastUIImageUtility.buildSliced(
					builder,
					rect,
					mSprite,
					mFillCenter,
					mPixelsPerUnitMultiplier,
					mFillMethod,
					mFillAmount,
					mFillOrigin);
				break;
			case FastUIImageType.Tiled:
				FastUIImageUtility.buildTiled(builder, rect, mSprite, mFillCenter, mPixelsPerUnitMultiplier);
				break;
			case FastUIImageType.Filled:
				FastUIImageUtility.buildFilled(builder, rect, mSprite, mFillMethod, mFillAmount, mFillClockwise, mFillOrigin);
				break;
			default:
				FastUIImageUtility.buildSimple(builder, rect, mSprite);
				break;
		}
		Vector3[] positions = builder.getPositions();
		int vertexCount = builder.getVertexCount();
		if (tryGetGeometryTranslation(out Vector3 translation))
		{
			for (int i = 0; i < vertexCount; ++i)
			{
				positions[i] += translation;
			}
		}
		else
		{
			Matrix4x4 geometryMatrix = getGeometryLocalToCanvasMatrix(canvasWorldToLocal);
			for (int i = 0; i < vertexCount; ++i)
			{
				positions[i] = geometryMatrix.MultiplyPoint3x4(positions[i]);
			}
		}
	}
	protected override void Awake()
	{
		mCachedSpriteTexture = mSprite != null ? mSprite.texture : null;
		base.Awake();
	}

#if UNITY_EDITOR
	// Sprite Editor changes (Border, Pivot, slicing, etc.) reimport the source texture.
	// Unity can recreate the Sprite / Texture native objects while this component still
	// holds the old cached texture and the Canvas batch still points at that old binding.
	// Refresh the complete Sprite-derived state after the import has finished.
	public void refreshEditorSpriteAsset()
	{
		if (Application.isPlaying)
		{
			return;
		}

		Texture texture = mSprite != null ? mSprite.texture : null;
		mCachedSpriteTexture = texture;

		// Reimport can change both the texture object and Sprite UV / Border data.
		// Force the normal FastGUI paths to rebuild batch binding and geometry.
		notifyBatchChanged();
		if (mCanvas != null)
		{
			mCanvas.notifyInitialSimpleQuadCandidateChanged(this);
		}
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
#endif
	protected override void OnDidApplyAnimationProperties()
	{
		mFillAmount = Mathf.Clamp01(mFillAmount);
		mPixelsPerUnitMultiplier = Mathf.Max(mPixelsPerUnitMultiplier, 0.01f);
		mFillOrigin = FastUIImageUtility.clampFillOrigin(mFillMethod, mFillOrigin);
		Texture texture = mSprite != null ? mSprite.texture : null;
		if (texture != mCachedSpriteTexture)
		{
			mCachedSpriteTexture = texture;
			notifyBatchChanged();
		}
		if (mCanvas != null)
		{
			mCanvas.notifyInitialSimpleQuadCandidateChanged(this);
		}
		base.OnDidApplyAnimationProperties();
	}
}
