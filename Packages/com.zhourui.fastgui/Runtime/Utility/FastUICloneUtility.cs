using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// FastGUI模板克隆只读取模板派生数据，绝不缓存/复用克隆出来的节点。
// 用户层只需要传入template/parent/count；Baked模板解析、CloneData、容量预留和快速注册都由FastGUI内部处理。
public readonly struct FastUITextCloneFontData
{
	public readonly TMP_FontAsset mFont;
	public readonly Material mSourceMaterial;
	public readonly Material mEffectiveMaterial;
	public readonly Texture mRenderTexture;
	public readonly Material mLastFontMaterial;
	public readonly bool mMaterialOverride;
	public readonly float mMaterialPadding;
	public readonly TMP_Character[] mAsciiCharacterCache;
	public readonly ulong mPrimaryAtlasAsciiMaskLow;
	public readonly ulong mPrimaryAtlasAsciiMaskHigh;
	public FastUITextCloneFontData(TMP_FontAsset font, Material sourceMaterial, Material effectiveMaterial, Texture renderTexture,
		Material lastFontMaterial, bool materialOverride, float materialPadding, TMP_Character[] asciiCharacterCache)
		: this(font, sourceMaterial, effectiveMaterial, renderTexture, lastFontMaterial, materialOverride, materialPadding, asciiCharacterCache, 0UL, 0UL)
	{
	}
	public FastUITextCloneFontData(TMP_FontAsset font, Material sourceMaterial, Material effectiveMaterial, Texture renderTexture,
		Material lastFontMaterial, bool materialOverride, float materialPadding, TMP_Character[] asciiCharacterCache,
		ulong primaryAtlasAsciiMaskLow, ulong primaryAtlasAsciiMaskHigh)
	{
		mFont = font;
		mSourceMaterial = sourceMaterial;
		mEffectiveMaterial = effectiveMaterial;
		mRenderTexture = renderTexture;
		mLastFontMaterial = lastFontMaterial;
		mMaterialOverride = materialOverride;
		mMaterialPadding = materialPadding;
		mAsciiCharacterCache = asciiCharacterCache;
		mPrimaryAtlasAsciiMaskLow = primaryAtlasAsciiMaskLow;
		mPrimaryAtlasAsciiMaskHigh = primaryAtlasAsciiMaskHigh;
	}
}

public static class FastUICloneUtility
{
	private sealed class CloneTemplateData
	{
		public readonly GameObject mTemplate;
		public readonly int mRenderElementCount;
		public readonly int mTextCount;
		public readonly FastUITextCloneFontData[] mTextFontData;
		public readonly bool mRegistrationFastPathSupported;
		private CloneTemplateData(GameObject template, int renderElementCount, int textCount, FastUITextCloneFontData[] textFontData, bool registrationFastPathSupported)
		{
			mTemplate = template;
			mRenderElementCount = renderElementCount;
			mTextCount = textCount;
			mTextFontData = textFontData ?? Array.Empty<FastUITextCloneFontData>();
			mRegistrationFastPathSupported = registrationFastPathSupported;
		}
		public bool tryGetTextFontData(TMP_FontAsset font, Material sourceMaterial, out FastUITextCloneFontData data)
		{
			for (int i = 0; i < mTextFontData.Length; ++i)
			{
				FastUITextCloneFontData current = mTextFontData[i];
				if (current.mFont == font && current.mSourceMaterial == sourceMaterial)
				{
					data = current;
					return true;
				}
			}
			data = default;
			return false;
		}
		public static CloneTemplateData create(GameObject template)
		{
			if (template == null)
			{
				return new CloneTemplateData(null, 0, 0, Array.Empty<FastUITextCloneFontData>(), false);
			}
			List<FastUIRenderElement> graphics = new(16);
			template.GetComponentsInChildren(true, graphics);
			List<FastText> texts = new(4);
			template.GetComponentsInChildren(true, texts);
			List<FastUITextCloneFontData> fontData = new(texts.Count);
			for (int i = 0; i < texts.Count; ++i)
			{
				FastUITextCloneFontData current = texts[i].createCloneFontData();
				bool exists = false;
				for (int j = 0; j < fontData.Count; ++j)
				{
					if (fontData[j].mFont == current.mFont && fontData[j].mSourceMaterial == current.mSourceMaterial)
					{
						exists = true;
						break;
					}
				}
				if (!exists)
				{
					fontData.Add(current);
				}
			}
			bool supported = template.GetComponentInChildren<FastCanvas>(true) == null &&
				template.GetComponentInChildren<FastMask>(true) == null &&
				template.GetComponentInChildren<FastRectMask2D>(true) == null;
			return new CloneTemplateData(template, graphics.Count, texts.Count, fontData.ToArray(), supported);
		}
	}
	private sealed class CloneSession
	{
		public CloneSession mPrevious;
		public CloneTemplateData mData;
		public Transform mTargetParent;
		public FastCanvas mCanvas;
		public bool mRegistrationFastPath;
		public FastUIRenderElement[] mPendingGraphics;
		public int mPendingGraphicCount;
		public int mBulkRegisterCount;
		public int mBulkRegisterFlushCount;
		public bool mRegistryOrderValid = true;
		public bool mRegistryOrderValidated;
		public int mExpectedPendingPerClone = -1;
		public readonly List<FastUIRenderElement> mValidationGraphics = new(16);
		public void appendPendingGraphic(FastUIRenderElement element)
		{
			if (element == null)
			{
				return;
			}
			if (mPendingGraphics == null)
			{
				mPendingGraphics = new FastUIRenderElement[16];
			}
			else if (mPendingGraphicCount >= mPendingGraphics.Length)
			{
				Array.Resize(ref mPendingGraphics, Mathf.Max(mPendingGraphicCount + 1, mPendingGraphics.Length << 1));
			}
			mPendingGraphics[mPendingGraphicCount++] = element;
		}
	}
	private static CloneSession sCurrentSession;
	private static int sLastBulkRegisterCount;
	private static int sLastBulkRegisterFlushCount;
	public static int getLastBulkRegisterCount()
	{
		return sLastBulkRegisterCount;
	}
	public static int getLastBulkRegisterFlushCount()
	{
		return sLastBulkRegisterFlushCount;
	}
	public static GameObject instantiate(GameObject template, Transform parent)
	{
		if (template == null)
		{
			return null;
		}
		CloneTemplateData data = CloneTemplateData.create(template);
		CloneSession session = beginCloneSession(data, parent, 1);
		try
		{
			int pendingStart = session.mPendingGraphicCount;
			GameObject clone = UnityEngine.Object.Instantiate(template, parent, false);
			validateCloneRegistryOrder(session, clone, pendingStart);
			return clone;
		}
		finally
		{
			endCloneSession(session);
		}
	}
	public static GameObject[] instantiate(GameObject template, Transform parent, int count)
	{
		if (template == null || count <= 0)
		{
			return Array.Empty<GameObject>();
		}
		CloneTemplateData data = CloneTemplateData.create(template);
		GameObject[] result = new GameObject[count];
		CloneSession session = beginCloneSession(data, parent, count);
		try
		{
			for (int i = 0; i < count; ++i)
			{
				int pendingStart = session.mPendingGraphicCount;
				result[i] = UnityEngine.Object.Instantiate(template, parent, false);
				validateCloneRegistryOrder(session, result[i], pendingStart);
			}
		}
		finally
		{
			endCloneSession(session);
		}
		return result;
	}
	private static CloneSession beginCloneSession(CloneTemplateData data, Transform parent, int count)
	{
		if (sCurrentSession == null)
		{
			sLastBulkRegisterCount = 0;
			sLastBulkRegisterFlushCount = 0;
		}
		FastCanvas canvas = parent != null ? parent.GetComponentInParent<FastCanvas>(true) : null;
		bool hasParentMask = parent != null && (parent.GetComponentInParent<FastMask>(true) != null || parent.GetComponentInParent<FastRectMask2D>(true) != null);
		bool fastRegistration = canvas != null && data.mRegistrationFastPathSupported && !hasParentMask;
		int expectedRenderElementCount = multiplyCount(data.mRenderElementCount, count);
		int expectedTextCount = multiplyCount(data.mTextCount, count);
		if (fastRegistration)
		{
			int renderCapacity = addSaturated(canvas.getRenderOrderSlotCount(), expectedRenderElementCount);
			int textCapacity = addSaturated(canvas.getTextElementCount(), expectedTextCount);
			canvas.beginCloneRegistrationBatch(expectedRenderElementCount, expectedTextCount, renderCapacity, textCapacity, parent);
		}
		CloneSession session = new()
		{
			mPrevious = sCurrentSession,
			mData = data,
			mTargetParent = parent,
			mCanvas = canvas,
			mRegistrationFastPath = fastRegistration,
			mPendingGraphics = fastRegistration && expectedRenderElementCount > 0 ? new FastUIRenderElement[expectedRenderElementCount] : null,
		};
		sCurrentSession = session;
		return session;
	}
	private static void endCloneSession(CloneSession session)
	{
		if (session == null)
		{
			return;
		}
		try
		{
			flushSessionRegistration(session);
		}
		finally
		{
			sCurrentSession = session.mPrevious;
			if (session.mRegistrationFastPath && session.mCanvas != null)
			{
				session.mCanvas.setCloneRegistrationRegistryOrderValidated(session.mRegistryOrderValid && session.mRegistryOrderValidated);
				session.mCanvas.endCloneRegistrationBatch();
			}
			if (session.mPrevious != null)
			{
				session.mPrevious.mBulkRegisterCount += session.mBulkRegisterCount;
				session.mPrevious.mBulkRegisterFlushCount += session.mBulkRegisterFlushCount;
			}
			else
			{
				sLastBulkRegisterCount = session.mBulkRegisterCount;
				sLastBulkRegisterFlushCount = session.mBulkRegisterFlushCount;
			}
		}
	}
	private static void validateCloneRegistryOrder(CloneSession session, GameObject clone, int pendingStart)
	{
		if (session == null || !session.mRegistrationFastPath || !session.mRegistryOrderValid || clone == null)
		{
			return;
		}
		int pendingCount = session.mPendingGraphicCount - pendingStart;
		if (session.mExpectedPendingPerClone < 0)
		{
			session.mExpectedPendingPerClone = pendingCount;
		}
		else if (pendingCount != session.mExpectedPendingPerClone)
		{
			session.mRegistryOrderValid = false;
			return;
		}
		if (session.mRegistryOrderValidated)
		{
			return;
		}
		// 只对第一份Clone做一次真实Hierarchy DFS校验。后续Clone来自同一模板，只检查回调数量一致，
		// 避免为了省一次全Canvas扫描反而给每个Clone再做一遍GetComponentsInChildren。
		List<FastUIRenderElement> validation = session.mValidationGraphics;
		validation.Clear();
		clone.GetComponentsInChildren(true, validation);
		int writeIndex = 0;
		for (int i = 0; i < validation.Count; ++i)
		{
			FastUIRenderElement element = validation[i];
			if (element == null || element.getCanvas() != session.mCanvas)
			{
				continue;
			}
			if (pendingStart + writeIndex >= session.mPendingGraphicCount || session.mPendingGraphics[pendingStart + writeIndex] != element)
			{
				session.mRegistryOrderValid = false;
				return;
			}
			++writeIndex;
		}
		session.mRegistryOrderValid = writeIndex == pendingCount;
		session.mRegistryOrderValidated = session.mRegistryOrderValid;
	}
	private static void flushSessionRegistration(CloneSession session)
	{
		if (session == null || !session.mRegistrationFastPath || session.mCanvas == null || session.mPendingGraphicCount <= 0)
		{
			return;
		}
		int count = session.mCanvas.registerCloneBatch(session.mPendingGraphics, session.mPendingGraphicCount);
		session.mBulkRegisterCount += count;
		++session.mBulkRegisterFlushCount;
		for (int i = 0; i < session.mPendingGraphicCount; ++i)
		{
			session.mPendingGraphics[i] = null;
		}
		session.mPendingGraphicCount = 0;
	}
	public static void flushPendingRegistration(FastCanvas canvas)
	{
		if (canvas == null)
		{
			return;
		}
		for (CloneSession session = sCurrentSession; session != null; session = session.mPrevious)
		{
			if (session.mCanvas == canvas)
			{
				flushSessionRegistration(session);
			}
		}
	}
	private static int addSaturated(int a, int b)
	{
		long result = (long)Mathf.Max(a, 0) + Mathf.Max(b, 0);
		return result >= int.MaxValue ? int.MaxValue : (int)result;
	}
	private static int multiplyCount(int value, int count)
	{
		long result = (long)Mathf.Max(value, 0) * Mathf.Max(count, 0);
		return result >= int.MaxValue ? int.MaxValue : (int)result;
	}
	public static bool tryBindGraphic(FastUIRenderElement element, bool registerIfActive)
	{
		CloneSession session = sCurrentSession;
		if (session == null || !session.mRegistrationFastPath || element == null || session.mCanvas == null || session.mTargetParent == null)
		{
			return false;
		}
		RectTransform rect = element.getRectTransform();
		if (rect == null || !rect.IsChildOf(session.mTargetParent))
		{
			return false;
		}
		if (!element.bindCloneCanvas(session.mCanvas, registerIfActive, true))
		{
			return false;
		}
		if (registerIfActive && element.tryMarkCloneRegistrationPending())
		{
			session.appendPendingGraphic(element);
		}
		return true;
	}

	public static bool tryBindVisibility(FastUIVisibility visibility)
	{
		CloneSession session = sCurrentSession;
		if (session == null || !session.mRegistrationFastPath || visibility == null || session.mCanvas == null || session.mTargetParent == null)
		{
			return false;
		}
		RectTransform rect = visibility.getRectTransform();
		if (rect == null || !rect.IsChildOf(session.mTargetParent))
		{
			return false;
		}
		return visibility.bindCloneCanvas(session.mCanvas);
	}

	public static bool tryApplyTextCloneData(FastText text)
	{
		CloneSession session = sCurrentSession;
		if (session == null || text == null || session.mData == null)
		{
			return false;
		}
		if (!session.mData.tryGetTextFontData(text.getFont(), text.getMaterial(), out FastUITextCloneFontData data))
		{
			return false;
		}
		return text.applyCloneFontData(data);
	}
}
