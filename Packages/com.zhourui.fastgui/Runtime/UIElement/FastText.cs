using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

public enum FastUITextHorizontalAlignment
{
	Left = 0,
	Center = 1,
	Right = 2,
	Justified = 3,
	Flush = 4,
	Geometry = 5,
}
public enum FastUITextVerticalAlignment
{
	Top = 0,
	Middle = 1,
	Bottom = 2,
	Baseline = 3,
	Midline = 4,
	Capline = 5,
}
public enum FastUITextWrappingMode
{
	NoWrap = 0,
	Normal = 1,
}
public enum FastUITextOverflowMode
{
	Overflow = 0,
	Truncate = 1,
}
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastText : FastUIRenderElement
{
	protected sealed class SharedAsciiCharacterCache
	{
		public int mCharacterCount;
		public TMP_Character[] mCharacters;
		public ulong mPrimaryAtlasAsciiMaskLow;
		public ulong mPrimaryAtlasAsciiMaskHigh;
		public FastDictionary<int, ushort[]> mSimpleNumberMetricRowsByFontSizeBits;
		public int mSimpleNumberMetricVersion;
	}
	protected static readonly TMP_Character[] EMPTY_ASCII_CHARACTER_CACHE = new TMP_Character[128];
	protected static readonly FastDictionary<TMP_FontAsset, SharedAsciiCharacterCache> SHARED_ASCII_CHARACTER_CACHE = new();
	protected struct LineLayout
	{
		public int mStart;
		public int mEnd;
		public float mWidth;
		public int mWordGapCount;
		public int mCharacterGapCount;
		public float mGeometryMinX;
		public float mGeometryMaxX;
		public bool mHasGeometry;
		public bool mEndsParagraph;
	}
	protected struct GlyphLayout
	{
		public TMP_Character mCharacter;
		public TMP_FontAsset mFont;
		public int mAtlasIndex;
		public int mLineIndex;
		public int mWordGapIndex;
		public int mCharacterGapIndex;
		public float mPenX;
		public float mGlyphScale;
		public Color32 mRichColor;
	}
	protected struct RenderRunLayout
	{
		public TMP_FontAsset mFont;
		public int mAtlasIndex;
		public int mGlyphStart;
		public int mGlyphCount;
		public Material mBaseMaterial;
		public Texture mTexture;
		public float mPadding;
	}
	[SerializeField] protected TMP_FontAsset mFont;
	[SerializeField, TextArea] protected string mText = string.Empty;
	[SerializeField] protected float mFontSize = 36.0f;
	[SerializeField] protected float mCharacterSpacing;
	[SerializeField] protected float mLineSpacing;
	[SerializeField] protected bool mWordWrap = true;
	[SerializeField] protected FastUITextOverflowMode mOverflowMode = FastUITextOverflowMode.Overflow;
	[SerializeField] protected bool mRichText = true;
	[SerializeField] protected FastUITextHorizontalAlignment mHorizontalAlignment = FastUITextHorizontalAlignment.Left;
	[SerializeField] protected FastUITextVerticalAlignment mVerticalAlignment = FastUITextVerticalAlignment.Middle;
	[SerializeField] protected uint mMissingCharacter = 0x25A1;
	protected Texture mRenderTexture;
	protected Material mLastFontMaterial;
	protected bool mMaterialOverride;
	protected float mMaterialPadding;
	protected TMP_Character[] mAsciiCharacterCache = EMPTY_ASCII_CHARACTER_CACHE;
	// ：Primary Atlas ASCII合法性由Shared Character Cache一次性编码成128bit位图。
	// 重复setText热路径只做char范围+bit测试，不再逐字符解引用TMP_Character/Glyph/atlasIndex。
	protected ulong mPrimaryAtlasAsciiMaskLow;
	protected ulong mPrimaryAtlasAsciiMaskHigh;
	protected LineLayout mFirstLine;
	protected LineLayout[] mExtraLines;
	protected Color32[] mColorStack;
	protected int mLineCount;
	protected int mVisibleGlyphCount;
	protected int mMissingGlyphCount;
	protected int mUnsupportedAtlasGlyphCount;
	protected int mRichColorTagCount;
	protected bool mHasRichColor;
	protected float mPreferredWidth;
	protected float mPreferredHeight;
	protected bool mLayoutDirty = true;
	protected GlyphLayout[] mGlyphLayouts;
	// 无Canvas/尚未注册时使用的本地Fallback缓存；生产Canvas路径直接写Persistent Glyph ECS。
	protected FastUITextSimpleGlyphData_ECSList mSimpleGlyphECS;
	protected FastUITextSimpleGlyphData_ECSList mSimpleGlyphDataECS;
	protected int mSimpleGlyphDataStart;
	protected int mGlyphLayoutCount;
	protected RenderRunLayout mFirstRun;
	protected RenderRunLayout[] mExtraRuns;
	protected int mRenderRunCount;
	protected int mFallbackGlyphCount;
	protected int mMultiAtlasGlyphCount;
	protected TMP_FontAsset[] mFallbackSearchPath;
	protected List<FastTextAtlasRun> mAtlasRunElements;
	protected RectTransform mAtlasRunRoot;
	protected bool mUsingAtlasRunElements;
	protected bool mSimpleSingleLinePrimaryLayout;
	// CompactIndex下完全位于Clip外的Simple Text只保留最新逻辑状态，重新进入可视区时再一次性补Geometry。
	protected bool mDeferredClipGeometryDirty;
	// ：只记录真正完成过即时Run同步的setText所在Canvas Mutation Epoch。
	// 同Epoch第二次及以后修改如果仍可证明是Primary Atlas单Run，只保留最终字符串，Flush时统一消费最终Layout。
	protected int mLastImmediateTextMutationEpoch = int.MinValue;
	protected ushort[] mSimpleNumberMetricRows;
	protected TMP_FontAsset mSimpleNumberMetricRowsFont;
	protected int mSimpleNumberMetricRowsFontSizeBits;
	protected SharedAsciiCharacterCache mSimpleNumberMetricSharedCache;
	protected int mSimpleNumberMetricSharedVersion;
	// ：Simple Primary Layout每次Rebuild都会重复读取Font face/atlas以及Text+Canvas lossyScale。
	// 字体/字号常量按FastText缓存；SDF相对缩放使用FastCanvas独立Scale版本，只在真实Scale变化时失效。
	protected TMP_FontAsset mSimpleLayoutMetricsFont;
	protected int mSimpleLayoutMetricsFontSizeBits;
	protected bool mSimpleLayoutMetricsValid;
	protected float mSimpleLayoutFontScale;
	protected float mSimpleLayoutAscent;
	protected float mSimpleLayoutDescent;
	protected float mSimpleLayoutInvAtlasWidth;
	protected float mSimpleLayoutInvAtlasHeight;
	protected FastCanvas mSimpleLayoutSDFCanvas;
	protected int mSimpleLayoutSDFScaleVersion;
	protected bool mSimpleLayoutSDFScaleValid;
	protected float mSimpleLayoutSDFScaleFactor = 1.0f;
	protected static bool sSimpleLayoutMetricsCacheEnabled = true;
	protected static int sSimpleLayoutMetricsCacheHitCount;
	protected static int sSimpleLayoutMetricsCacheMissCount;
	protected static int sSimpleLayoutSDFScaleCacheHitCount;
	protected static int sSimpleLayoutSDFScaleCacheMissCount;
	protected static bool sSimpleNumberMetricCacheEnabled = true;
	protected static bool sCoalescedTextMutationFastReplaceEnabled = false;
	protected static bool sPrimaryAtlasTextDeferredRunSyncEnabled = false;
	protected static bool sRepeatedTextMutationDeferredRunSyncEnabled = true;
	protected static int sRepeatedTextMutationDeferredRunSyncHitCount;
	protected static bool sPrimaryAtlasAsciiMaskFastCheckEnabled = true;
	// ：Simple Primary Layout复用已有128bit Primary Atlas ASCII位图，避免逐字符TMP_Character/Glyph/atlasIndex对象链检查。
	// 不新增per-Text缓存或生命周期状态；仅替换同语义eligibility读取方式。
	protected static bool sSimpleLayoutPrimaryAtlasMaskEligibilityEnabled = false;
	// ：Simple Primary Layout成功时，旧persistent glyph count会在本次重建末尾被最终count覆盖，
	// 因此延迟清零：仅空文本或Simple路径失败转Generic时才写0，避免成功Simple路径一次冗余Canvas/slot/ECS写入。
	protected static bool sSimpleLayoutDeferredPersistentGlyphClearEnabled = false;
	public static void setSimpleLayoutMetricsCacheEnabled(bool enabled)
	{
		sSimpleLayoutMetricsCacheEnabled = enabled;
	}
	public static bool isSimpleLayoutMetricsCacheEnabled()
	{
		return sSimpleLayoutMetricsCacheEnabled;
	}
	public static void resetSimpleLayoutMetricsCacheStats()
	{
		sSimpleLayoutMetricsCacheHitCount = 0;
		sSimpleLayoutMetricsCacheMissCount = 0;
		sSimpleLayoutSDFScaleCacheHitCount = 0;
		sSimpleLayoutSDFScaleCacheMissCount = 0;
	}
	public static int getSimpleLayoutMetricsCacheHitCount()
	{
		return sSimpleLayoutMetricsCacheHitCount;
	}
	public static int getSimpleLayoutMetricsCacheMissCount()
	{
		return sSimpleLayoutMetricsCacheMissCount;
	}
	public static int getSimpleLayoutSDFScaleCacheHitCount()
	{
		return sSimpleLayoutSDFScaleCacheHitCount;
	}
	public static int getSimpleLayoutSDFScaleCacheMissCount()
	{
		return sSimpleLayoutSDFScaleCacheMissCount;
	}
	public static void setSimpleNumberMetricCacheEnabled(bool enabled)
	{
		sSimpleNumberMetricCacheEnabled = enabled;
	}
	public static bool isSimpleNumberMetricCacheEnabled()
	{
		return sSimpleNumberMetricCacheEnabled;
	}
	public static void setCoalescedTextMutationFastReplaceEnabled(bool enabled)
	{
		sCoalescedTextMutationFastReplaceEnabled = enabled;
	}
	public static bool isCoalescedTextMutationFastReplaceEnabled()
	{
		return sCoalescedTextMutationFastReplaceEnabled;
	}
	public static void setPrimaryAtlasTextDeferredRunSyncEnabled(bool enabled)
	{
		sPrimaryAtlasTextDeferredRunSyncEnabled = enabled;
	}
	public static bool isPrimaryAtlasTextDeferredRunSyncEnabled()
	{
		return sPrimaryAtlasTextDeferredRunSyncEnabled;
	}
	public static void setRepeatedTextMutationDeferredRunSyncEnabled(bool enabled)
	{
		sRepeatedTextMutationDeferredRunSyncEnabled = enabled;
	}
	public static bool isRepeatedTextMutationDeferredRunSyncEnabled()
	{
		return sRepeatedTextMutationDeferredRunSyncEnabled;
	}
	public static void resetRepeatedTextMutationDeferredRunSyncHitCount()
	{
		sRepeatedTextMutationDeferredRunSyncHitCount = 0;
	}
	public static int getRepeatedTextMutationDeferredRunSyncHitCount()
	{
		return sRepeatedTextMutationDeferredRunSyncHitCount;
	}
	public static void setPrimaryAtlasAsciiMaskFastCheckEnabled(bool enabled)
	{
		sPrimaryAtlasAsciiMaskFastCheckEnabled = enabled;
	}
	public static bool isPrimaryAtlasAsciiMaskFastCheckEnabled()
	{
		return sPrimaryAtlasAsciiMaskFastCheckEnabled;
	}
	public static void setSimpleLayoutPrimaryAtlasMaskEligibilityEnabled(bool enabled)
	{
		sSimpleLayoutPrimaryAtlasMaskEligibilityEnabled = enabled;
	}
	public static bool isSimpleLayoutPrimaryAtlasMaskEligibilityEnabled()
	{
		return sSimpleLayoutPrimaryAtlasMaskEligibilityEnabled;
	}
	public static void setSimpleLayoutDeferredPersistentGlyphClearEnabled(bool enabled)
	{
		sSimpleLayoutDeferredPersistentGlyphClearEnabled = enabled;
	}
	public static bool isSimpleLayoutDeferredPersistentGlyphClearEnabled()
	{
		return sSimpleLayoutDeferredPersistentGlyphClearEnabled;
	}
	public TMP_FontAsset getFont()
	{
		return mFont;
	}
	public string getText()
	{
		return mText;
	}
	public float getFontSize()
	{
		return mFontSize;
	}
	public float getCharacterSpacing()
	{
		return mCharacterSpacing;
	}
	public float getLineSpacing()
	{
		return mLineSpacing;
	}
	public bool getWordWrap()
	{
		return mWordWrap;
	}
	public FastUITextWrappingMode getTextWrappingMode()
	{
		return mWordWrap ? FastUITextWrappingMode.Normal : FastUITextWrappingMode.NoWrap;
	}
	public FastUITextOverflowMode getOverflowMode()
	{
		return mOverflowMode;
	}
	public bool getRichText()
	{
		return mRichText;
	}
	public FastUITextHorizontalAlignment getHorizontalAlignment()
	{
		return mHorizontalAlignment;
	}
	public FastUITextVerticalAlignment getVerticalAlignment()
	{
		return mVerticalAlignment;
	}
	public int getLineCount()
	{
		ensureLayout();
		return mLineCount;
	}
	public int getVisibleGlyphCount()
	{
		ensureLayout();
		return mVisibleGlyphCount;
	}
	public int getMissingGlyphCount()
	{
		ensureLayout();
		return mMissingGlyphCount;
	}
	public int getUnsupportedAtlasGlyphCount()
	{
		ensureLayout();
		return mUnsupportedAtlasGlyphCount;
	}
	public int getFallbackGlyphCount()
	{
		ensureLayout();
		return mFallbackGlyphCount;
	}
	public int getMultiAtlasGlyphCount()
	{
		ensureLayout();
		return mMultiAtlasGlyphCount;
	}
	public int getRenderRunCount()
	{
		ensureLayout();
		return mRenderRunCount;
	}
	public bool isUsingAtlasRunElements()
	{
		ensureLayout();
		return mUsingAtlasRunElements;
	}
	public int getRichColorTagCount()
	{
		ensureLayout();
		return mRichColorTagCount;
	}
	public bool hasRichColor()
	{
		ensureLayout();
		return mHasRichColor;
	}
	public float getPreferredWidth()
	{
		ensureLayout();
		return mPreferredWidth;
	}
	public float getPreferredHeight()
	{
		ensureLayout();
		return mPreferredHeight;
	}
	public float getMaterialPadding()
	{
		return mMaterialPadding;
	}
	public bool hasMaterialOverride()
	{
		return mMaterialOverride;
	}
	public override Texture getRenderTexture()
	{
		return mRenderTexture != null ? mRenderTexture : Texture2D.whiteTexture;
	}
	public override Material getSourceRenderMaterial(Material defaultMaterial)
	{
		Material source = mMaterial != null ? mMaterial : defaultMaterial;
		return FastUITextRenderMaterialCache.get(source, getRenderTexture());
	}
	public override bool requiresTMPVertexLayout()
	{
		return true;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.Text;
	}
	// 不在Dirty入口调用hasRichColor()，否则setText/setFontSize每次都会同步触发ensureLayout。
	// mHasRichColor只会在Layout重建后变化，而所有会改变它的操作本身都会产生Geometry Dirty。
	public override bool requiresGeometryRebuildForColor()
	{
		return mHasRichColor;
	}
	public override bool geometryControlsVertexColor()
	{
		return mHasRichColor;
	}
	public override bool requiresGeometryRebuildForMatrixTransform()
	{
		return true;
	}
	public void setFont(TMP_FontAsset font)
	{
		if (mFont == font)
		{
			return;
		}
		Material oldFontMaterial = mFont != null ? mFont.material : null;
		if (mMaterial == null || mMaterial == oldFontMaterial)
		{
			mMaterialOverride = false;
		}
		mFont = font;
		invalidateSimpleLayoutMetricsCache();
		invalidateSimpleNumberMetricRows();
		refreshCharacterCache();
		syncFontRenderData(true);
		setLayoutDirty();
	}
	public override void setMaterial(Material material)
	{
		Material fontMaterial = mFont != null ? mFont.material : null;
		mMaterialOverride = material != null && material != fontMaterial;
		Material effectiveMaterial = material != null ? material : fontMaterial;
		Texture texture = getMaterialTexture(effectiveMaterial);
		if (texture == null && mFont != null)
		{
			texture = mFont.atlasTexture;
		}
		if (mMaterial == effectiveMaterial && mRenderTexture == texture)
		{
			refreshMaterialPadding();
			refreshMaterialVariants();
			return;
		}
		mMaterial = effectiveMaterial;
		mRenderTexture = texture;
		mLastFontMaterial = fontMaterial;
		refreshMaterialPadding();
		if (mCanvas != null)
		{
			mCanvas.notifyElementBatchChanged(this);
		}
		setLayoutDirty();
	}
	public override void setColor(Color color)
	{
		if (mColor == color)
		{
			return;
		}
		base.setColor(color);
		syncRunColors();
	}
	public override void setAlpha(float alpha)
	{
		if (Mathf.Approximately(mColor.a, alpha))
		{
			return;
		}
		base.setAlpha(alpha);
		syncRunColors();
	}
	public override void cull(bool cullValue)
	{
		if (mCull == cullValue)
		{
			return;
		}
		base.cull(cullValue);
		if (mAtlasRunElements != null)
		{
			for (int i = 0; i < mAtlasRunElements.Count; ++i)
			{
				FastTextAtlasRun run = mAtlasRunElements[i];
				if (run != null)
				{
					run.cull(cullValue);
				}
			}
		}
	}
	public void clearMaterialOverride()
	{
		setMaterial(null);
	}
	public void notifyMaterialPropertiesChanged()
	{
		refreshMaterialPadding();
		refreshMaterialVariants();
		// Simple Glyph缓存包含Padding/SDF相关的预计算结果，材质属性变化必须使Layout缓存失效。
		mLayoutDirty = true;
		markTextGeometryDirty();
		markRunGeometryDirty();
	}
	public void notifyFontAssetChanged()
	{
		invalidateSimpleLayoutMetricsCache();
		invalidateSharedSimpleNumberMetricRows(mFont);
		invalidateSimpleNumberMetricRows();
		refreshCharacterCache();
		syncFontRenderData(true);
		setLayoutDirty();
	}
	public void setText(string text)
	{
		text ??= string.Empty;
		if (mText == text)
		{
			return;
		}
		// ：第一次修改保持现有即时Run同步语义；同一个Canvas Flush之前再次修改同一FastText时，
		// 如果稳定Run和新字符串都能证明仍是Primary Atlas单Run，则跳过必然会被最终字符串覆盖的中间Layout/Run同步。
		// 这条路径不依赖LayoutDirty判断，因此不会像那样只覆盖原本已经延迟的Simple Font场景。
		if (tryDeferRepeatedTextMutationRunSync(text))
		{
			return;
		}
		// : 同一帧Layout已经Dirty时，Simple/单Run文本只需保留最后一次逻辑字符串。
		// 首次修改已经负责进入Canvas Dirty或Clip Deferred状态；后续重复修改无需再次执行Clip/UV/Glyph eligibility。
		// 多Atlas/Fallback Font仍保持旧的即时RenderRun同步路径。
		if (sCoalescedTextMutationFastReplaceEnabled && mLayoutDirty && !shouldSyncRenderRunsImmediately())
		{
			mText = text;
			return;
		}
		// : Clip外文字优先延迟，避免即使命中UV-only也把当前不可见Glyph上传到移动GPU。
		if (shouldDeferClipGeometryDirty())
		{
			mText = text;
			setLayoutDirty();
			return;
		}
		// 等长纯数字如果字形度量完全一致，字符变化只会改变Atlas UV。
		// 放宽SimpleSingleLine限制：只要已有Persistent Glyph布局缓存，即使原始布局不是SimpleSingleLine路径，也允许尝试UV-only。
		// 失败时仍回退完整Geometry重建。
		if (trySetSimpleNumberTextUVOnly(text))
		{
			return;
		}
		mText = text;
		setLayoutDirty(true);
		recordImmediateTextMutationEpoch();
	}
	public void setText(int value)
	{
		setText(value.ToString());
	}
	public void setText(long value)
	{
		setText(value.ToString());
	}
	private bool tryDeferRepeatedTextMutationRunSync(string text)
	{
		if (!sRepeatedTextMutationDeferredRunSyncEnabled || sCoalescedTextMutationFastReplaceEnabled || sPrimaryAtlasTextDeferredRunSyncEnabled ||
			mCanvas == null || mLastImmediateTextMutationEpoch != mCanvas.getMutationEpoch() || shouldDeferClipGeometryDirty() ||
			!canUsePrimaryAtlasDeferredRunSync(text))
		{
			return false;
		}
		mText = text;
		mLayoutDirty = true;
		++sRepeatedTextMutationDeferredRunSyncHitCount;
		return true;
	}
	private void recordImmediateTextMutationEpoch()
	{
		// setLayoutDirty(true)只有在真正完成ensureLayout+syncRenderRunElements后才会把LayoutDirty重新清零。
		// Clip Deferred、Simple Font本来就会延迟的路径、以及诊断路径都不写Epoch，避免把不存在的即时同步当成可消除工作。
		if (mCanvas != null && !mLayoutDirty)
		{
			mLastImmediateTextMutationEpoch = mCanvas.getMutationEpoch();
		}
	}
	private bool trySetSimpleNumberTextUVOnly(string text)
	{
		// : Layout尚未消费时Persistent Glyph仍对应上一份完整布局，不能在其上做UV-only差量。
		// 多次setText会继续保持Geometry Dirty，最终只对最后一份文本做一次Layout。
		if (mLayoutDirty || mCanvas == null || mUsingAtlasRunElements || mHasRichColor ||
			mFont == null || string.IsNullOrEmpty(mText) || text.Length != mText.Length || mGlyphLayoutCount <= 0 ||
			mVisibleGlyphCount <= 0 || !tryGetSimpleGlyphReadTarget(out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart) ||
			glyphData == null || glyphStart < 0 || glyphStart + mGlyphLayoutCount > glyphData.Count)
		{
			return false;
		}
		if (sSimpleNumberMetricCacheEnabled)
		{
			ushort[] rows = getSimpleNumberMetricRows();
			if (rows == null)
			{
				return false;
			}
			for (int i = 0; i < text.Length; ++i)
			{
				char oldValue = mText[i];
				char newValue = text[i];
				if (oldValue < '0' || oldValue > '9' || newValue < '0' || newValue > '9' || (rows[oldValue - '0'] & (1 << (newValue - '0'))) == 0)
				{
					return false;
				}
			}
		}
		else
		{
			FaceInfo faceInfo = mFont.faceInfo;
			float fontScale = getFontScale(faceInfo);
			for (int i = 0; i < text.Length; ++i)
			{
				char oldValue = mText[i];
				char newValue = text[i];
				if (oldValue < '0' || oldValue > '9' || newValue < '0' || newValue > '9')
				{
					return false;
				}
				TMP_Character oldCharacter = mAsciiCharacterCache[oldValue];
				TMP_Character newCharacter = mAsciiCharacterCache[newValue];
				if (!hasEquivalentSimpleGlyphMetrics(oldCharacter, newCharacter, fontScale))
				{
					return false;
				}
			}
		}
		float paddingPixels = Mathf.Max(mMaterialPadding, 0.0f);
		float invAtlasWidth = 1.0f / Mathf.Max(mFont.atlasWidth, 1);
		float invAtlasHeight = 1.0f / Mathf.Max(mFont.atlasHeight, 1);
		var uvLeftColumn = glyphData.getUVLeftColumn();
		var uvBottomColumn = glyphData.getUVBottomColumn();
		var uvRightColumn = glyphData.getUVRightColumn();
		var uvTopColumn = glyphData.getUVTopColumn();
		for (int i = 0; i < text.Length; ++i)
		{
			if (mText[i] == text[i])
			{
				continue;
			}
			GlyphRect glyphRect = mAsciiCharacterCache[text[i]].glyph.glyphRect;
			int glyphIndex = glyphStart + i;
			uvLeftColumn[glyphIndex] = (glyphRect.x - paddingPixels) * invAtlasWidth;
			uvBottomColumn[glyphIndex] = (glyphRect.y - paddingPixels) * invAtlasHeight;
			uvRightColumn[glyphIndex] = (glyphRect.x + glyphRect.width + paddingPixels) * invAtlasWidth;
			uvTopColumn[glyphIndex] = (glyphRect.y + glyphRect.height + paddingPixels) * invAtlasHeight;
		}
		mText = text;
		// 等宽纯数字复用旧Layout时，Geometry仍然有效。
		// 如果之前仅因为TextDirty进入过LayoutDirty状态，这里恢复状态，避免后续重复Geometry重建。
		mLayoutDirty = false;
		mCanvas.markTextElementUV0Dirty(this);
		return true;
	}
	private bool hasEquivalentSimpleGlyphMetrics(TMP_Character oldCharacter, TMP_Character newCharacter, float fontScale)
	{
		if (oldCharacter == null || newCharacter == null || oldCharacter.glyph == null || newCharacter.glyph == null ||
			oldCharacter.glyph.atlasIndex != 0 || newCharacter.glyph.atlasIndex != 0)
		{
			return false;
		}
		Glyph oldGlyph = oldCharacter.glyph;
		Glyph newGlyph = newCharacter.glyph;
		float oldScale = getGlyphScale(oldCharacter, oldGlyph, fontScale);
		float newScale = getGlyphScale(newCharacter, newGlyph, fontScale);
		if (!simpleMetricEqual(oldScale, newScale))
		{
			return false;
		}
		GlyphMetrics oldMetrics = oldGlyph.metrics;
		GlyphMetrics newMetrics = newGlyph.metrics;
		return simpleMetricEqual(oldMetrics.horizontalAdvance * oldScale, newMetrics.horizontalAdvance * newScale) &&
			simpleMetricEqual(oldMetrics.horizontalBearingX * oldScale, newMetrics.horizontalBearingX * newScale) &&
			simpleMetricEqual(oldMetrics.horizontalBearingY * oldScale, newMetrics.horizontalBearingY * newScale) &&
			simpleMetricEqual(oldMetrics.width * oldScale, newMetrics.width * newScale) &&
			simpleMetricEqual(oldMetrics.height * oldScale, newMetrics.height * newScale);
	}
	private static bool simpleMetricEqual(float a, float b)
	{
		return Mathf.Abs(a - b) <= 0.0001f;
	}
	public void setFontSize(float fontSize)
	{
		fontSize = Mathf.Max(fontSize, 0.01f);
		if (Mathf.Approximately(mFontSize, fontSize))
		{
			return;
		}
		mFontSize = fontSize;
		invalidateSimpleLayoutMetricsCache();
		invalidateSimpleNumberMetricRows();
		setLayoutDirty();
	}
	public void setCharacterSpacing(float spacing)
	{
		if (Mathf.Approximately(mCharacterSpacing, spacing))
		{
			return;
		}
		mCharacterSpacing = spacing;
		setLayoutDirty();
	}
	public void setLineSpacing(float spacing)
	{
		if (Mathf.Approximately(mLineSpacing, spacing))
		{
			return;
		}
		mLineSpacing = spacing;
		setLayoutDirty();
	}
	public void setWordWrap(bool wordWrap)
	{
		if (mWordWrap == wordWrap)
		{
			return;
		}
		mWordWrap = wordWrap;
		setLayoutDirty();
	}
	public void setTextWrappingMode(FastUITextWrappingMode mode)
	{
		setWordWrap(mode != FastUITextWrappingMode.NoWrap);
	}
	public void setOverflowMode(FastUITextOverflowMode mode)
	{
		if (mOverflowMode == mode)
		{
			return;
		}
		mOverflowMode = mode;
		setLayoutDirty();
	}
	public void setRichText(bool richText)
	{
		if (mRichText == richText)
		{
			return;
		}
		mRichText = richText;
		setLayoutDirty();
	}
	public void setHorizontalAlignment(FastUITextHorizontalAlignment alignment)
	{
		if (mHorizontalAlignment == alignment)
		{
			return;
		}
		mHorizontalAlignment = alignment;
		setLayoutDirty();
	}
	public void setVerticalAlignment(FastUITextVerticalAlignment alignment)
	{
		if (mVerticalAlignment == alignment)
		{
			return;
		}
		mVerticalAlignment = alignment;
		setLayoutDirty();
	}
	public void setMissingCharacter(uint unicode)
	{
		if (mMissingCharacter == unicode)
		{
			return;
		}
		mMissingCharacter = unicode;
		setLayoutDirty();
	}
	public float getLineAdvance()
	{
		ensureLayout();
		if (mFont == null)
		{
			return 0.0f;
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		return Mathf.Max(faceInfo.lineHeight * fontScale + mLineSpacing, 0.01f);
	}
	public float getTextLineHeight()
	{
		ensureLayout();
		if (mFont == null)
		{
			return 0.0f;
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		return Mathf.Max((faceInfo.ascentLine - faceInfo.descentLine) * fontScale, 0.0f);
	}
	public int getLineStartStringIndex(int lineIndex)
	{
		ensureLayout();
		if (mLineCount <= 0)
		{
			return 0;
		}
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		return getLineStartValue(lineIndex);
	}
	public int getLineEndStringIndex(int lineIndex)
	{
		ensureLayout();
		if (mLineCount <= 0)
		{
			return 0;
		}
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		return getLineEndValue(lineIndex);
	}
	public float getLineWidth(int lineIndex)
	{
		ensureLayout();
		if (mLineCount <= 0)
		{
			return 0.0f;
		}
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		return getLineWidthValue(lineIndex);
	}
	public int getLineIndexFromStringIndex(int stringIndex)
	{
		ensureLayout();
		if (mLineCount <= 0)
		{
			return 0;
		}
		stringIndex = normalizeStringIndex(stringIndex);
		for (int i = mLineCount - 1; i >= 0; --i)
		{
			if (stringIndex >= getLineStartValue(i))
			{
				return i;
			}
		}
		return 0;
	}
	public Rect getCaretLocalRect(int stringIndex, float caretWidth = 1.0f)
	{
		ensureLayout();
		caretWidth = Mathf.Max(caretWidth, 0.01f);
		if (mFont == null)
		{
			return new Rect(0.0f, 0.0f, caretWidth, 0.0f);
		}
		getLayoutMetrics(out float ascent, out float descent, out float lineAdvance, out float textHeight);
		int lineIndex = getLineIndexFromStringIndex(stringIndex);
		float baseline = getLineBaseline(lineIndex, ascent, lineAdvance, textHeight);
		float x = getStringIndexLocalX(stringIndex, lineIndex);
		return new Rect(x, baseline + descent, caretWidth, Mathf.Max(ascent - descent, 0.01f));
	}
	public bool getSelectionLocalRect(int lineIndex, int selectionStart, int selectionEnd, out Rect rect)
	{
		ensureLayout();
		rect = default;
		if (mFont == null || mLineCount <= 0)
		{
			return false;
		}
		if (selectionStart > selectionEnd)
		{
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);
		}
		selectionStart = normalizeStringIndex(selectionStart);
		selectionEnd = normalizeStringIndex(selectionEnd);
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		int lineStart = getLineStartValue(lineIndex);
		int lineEnd = getLineEndValue(lineIndex);
		int start = Mathf.Max(selectionStart, lineStart);
		int end = Mathf.Min(selectionEnd, lineEnd);
		if (end <= start)
		{
			return false;
		}
		getLayoutMetrics(out float ascent, out float descent, out float lineAdvance, out float textHeight);
		float baseline = getLineBaseline(lineIndex, ascent, lineAdvance, textHeight);
		float x0 = getStringIndexLocalX(start, lineIndex);
		float x1 = getStringIndexLocalX(end, lineIndex);
		if (x1 < x0)
		{
			(x0, x1) = (x1, x0);
		}
		rect = Rect.MinMaxRect(x0, baseline + descent, x1, baseline + ascent);
		return rect.width > 0.0f && rect.height > 0.0f;
	}
	public int getStringIndexFromLocalPosition(Vector2 localPosition)
	{
		ensureLayout();
		if (mFont == null || string.IsNullOrEmpty(mText) || mLineCount <= 0)
		{
			return 0;
		}
		getLayoutMetrics(out float ascent, out float descent, out float lineAdvance, out float textHeight);
		int bestLine = 0;
		float bestDistance = float.MaxValue;
		for (int i = 0; i < mLineCount; ++i)
		{
			float baseline = getLineBaseline(i, ascent, lineAdvance, textHeight);
			float centerY = baseline + (ascent + descent) * 0.5f;
			float distance = Mathf.Abs(localPosition.y - centerY);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestLine = i;
			}
		}
		return getNearestStringIndexOnLine(bestLine, localPosition.x);
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		mDeferredClipGeometryDirty = false;
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		builder.Clear();
		ensureLayout();
		if (mFont == null || mGlyphLayoutCount <= 0 || mUsingAtlasRunElements)
		{
			return;
		}
		float padding = mRenderRunCount > 0 ? getRenderRunLayout(0).mPadding : mMaterialPadding;
		buildGlyphRange(0, mGlyphLayoutCount, padding, builder, canvasWorldToLocal);
	}
	public void buildRenderRunGeometry(int runIndex, FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		ensureLayout();
		if (builder == null || runIndex < 0 || runIndex >= mRenderRunCount)
		{
			return;
		}
		RenderRunLayout run = getRenderRunLayout(runIndex);
		buildGlyphRange(run.mGlyphStart, run.mGlyphCount, run.mPadding, builder, canvasWorldToLocal);
	}
	protected override void Awake()
	{
		mFontSize = Mathf.Max(mFontSize, 0.01f);
		bool cloneDataApplied = FastUICloneUtility.tryApplyTextCloneData(this);
		if (!cloneDataApplied && (mFont != null || mMaterial != null))
		{
			Material fontMaterial = mFont != null ? mFont.material : null;
			mMaterialOverride = mMaterial != null && mMaterial != fontMaterial;
			mLastFontMaterial = fontMaterial;
			refreshCharacterCache();
			syncFontRenderData(false);
		}
		base.Awake();
		// 单Atlas文本不在Awake提前做一次“未注册Canvas”的Layout。
		// Register后第一次Canvas Dirty会直接写Persistent Glyph ECS，避免Prefab Instantiate阶段先建本地Glyph、FirstFlush再重建一次。
		if (fontMayUseMultipleRuns())
		{
			ensureLayout();
			syncRenderRunElements();
		}
	}
	protected override void OnEnable()
	{
		base.OnEnable();
		if ((mAtlasRunElements != null && mAtlasRunElements.Count > 0) || fontMayUseMultipleRuns())
		{
			ensureLayout();
			syncRenderRunElements();
		}
		if (mAtlasRunRoot != null && !mAtlasRunRoot.gameObject.activeSelf)
		{
			mAtlasRunRoot.gameObject.SetActive(true);
		}
	}
	protected override void OnDisable()
	{
		if (mAtlasRunRoot != null && mAtlasRunRoot.gameObject.activeSelf)
		{
			mAtlasRunRoot.gameObject.SetActive(false);
		}
		base.OnDisable();
	}
	protected override void OnRectTransformDimensionsChange()
	{
		mLayoutDirty = true;
		syncRunRectTransforms();
		markRunGeometryDirty();
		base.OnRectTransformDimensionsChange();
	}
	protected override void OnDidApplyAnimationProperties()
	{
		mFontSize = Mathf.Max(mFontSize, 0.01f);
		invalidateSimpleLayoutMetricsCache();
		invalidateSimpleNumberMetricRows();
		Material fontMaterial = mFont != null ? mFont.material : null;
		mMaterialOverride = mMaterial != null && mMaterial != fontMaterial && mMaterial != mLastFontMaterial;
		refreshCharacterCache();
		syncFontRenderData(true);
		setLayoutDirty();
		base.OnDidApplyAnimationProperties();
	}
	protected override void OnDestroy()
	{
		destroyRenderRunElements();
		mSimpleGlyphECS?.Dispose();
		mSimpleGlyphECS = null;
		base.OnDestroy();
	}
	private void setLayoutDirty(bool allowPrimaryAtlasDeferredRunSync = false)
	{
		bool wasDirty = mLayoutDirty;
		mLayoutDirty = true;
		// :生产CompactIndex下，Simple单行Text如果已经完全位于Clip外，只更新逻辑文本。
		// GPU仍保留旧Geometry作为裁剪边界，重新进入可视区时由setClipCullOutside同帧补一次最终Geometry。
		if (shouldDeferClipGeometryDirty())
		{
			mDeferredClipGeometryDirty = true;
			return;
		}
		// 同一帧连续修改Text/FontSize/Wrap/Alignment时，只需要第一次进入Canvas Dirty队列。
		// Initial Register已经主动排入Text Dirty，因此新建Text从一开始就天然处于Coalesced状态。
		if (!wasDirty)
		{
			markTextGeometryDirty();
			markRunGeometryDirty();
		}
		if (mCanvas != null && shouldSyncRenderRunsImmediately())
		{
			// : 字体即使拥有多个Atlas，只要当前稳定布局是Primary Atlas单Run，且新文本能证明
			// 所有Glyph仍来自Primary Atlas，就无需在Mutation阶段逐条ensureLayout/syncRenderRunElements。
			// Canvas TextBatch会在本帧Flush时消费最终文本；真正的Secondary Atlas/Fallback仍完整走旧即时路径。
			if (allowPrimaryAtlasDeferredRunSync && canDeferPrimaryAtlasRunSyncForCurrentText())
			{
				return;
			}
			ensureLayout();
			syncRenderRunElements();
		}
	}
	private bool shouldDeferClipGeometryDirty()
	{
		return mCanvas != null && mSimpleSingleLinePrimaryLayout && !mUsingAtlasRunElements && isClipCullOutside();
	}
	public override void setClipCullOutside(bool outside)
	{
		bool wasOutside = isClipCullOutside();
		base.setClipCullOutside(outside);
		if (!wasOutside || outside || !mDeferredClipGeometryDirty || mCanvas == null)
		{
			return;
		}
		// Clip刷新发生在首轮Dirty之后，因此要求Canvas在本帧Upload前再消费一次这批恢复可见的Text Dirty。
		markTextGeometryDirty();
		markRunGeometryDirty();
		mCanvas.requestDeferredClipTextResume();
	}
	private void ensureLayout()
	{
		ensureInit();
		if (!mLayoutDirty)
		{
			return;
		}
		mLayoutDirty = false;
		rebuildLayout();
		if (mCanvas != null)
		{
			mCanvas.syncTextLayoutRuntimeMetadata(this);
		}
	}
	private void rebuildLayout()
	{
		mLineCount = 0;
		mVisibleGlyphCount = 0;
		mMissingGlyphCount = 0;
		mUnsupportedAtlasGlyphCount = 0;
		mFallbackGlyphCount = 0;
		mMultiAtlasGlyphCount = 0;
		mRichColorTagCount = 0;
		mHasRichColor = false;
		mPreferredWidth = 0.0f;
		mPreferredHeight = 0.0f;
		mGlyphLayoutCount = 0;
		mRenderRunCount = 0;
		mUsingAtlasRunElements = false;
		mSimpleSingleLinePrimaryLayout = false;
		mSimpleGlyphDataECS = null;
		mSimpleGlyphDataStart = 0;
		if (!sSimpleLayoutDeferredPersistentGlyphClearEnabled && mCanvas != null)
		{
			mCanvas.setPersistentTextGlyphCount(this, 0);
		}
		if (mFont == null || string.IsNullOrEmpty(mText))
		{
			if (sSimpleLayoutDeferredPersistentGlyphClearEnabled && mCanvas != null)
			{
				mCanvas.setPersistentTextGlyphCount(this, 0);
			}
			return;
		}
		if (tryRebuildSimpleSingleLinePrimaryLayout())
		{
			return;
		}
		if (sSimpleLayoutDeferredPersistentGlyphClearEnabled && mCanvas != null)
		{
			mCanvas.setPersistentTextGlyphCount(this, 0);
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float primaryFontScale = getFontScale(faceInfo);
		float wrapWidth = Mathf.Max(mRectTransform.rect.width, 0.0f);
		float lineWidth = 0.0f;
		float unwrappedWidth = 0.0f;
		int lineStart = 0;
		int charIndex = 0;
		bool lineHasCharacter = false;
		int lineWordGapCount = 0;
		int lineAppliedWordGapCount = 0;
		int lineVisibleGlyphCount = 0;
		float lineGeometryMinX = float.PositiveInfinity;
		float lineGeometryMaxX = float.NegativeInfinity;
		bool lineHasGeometry = false;
		Color32 richColor = new(255, 255, 255, 255);
		int colorStackCount = 0;
		while (charIndex < mText.Length)
		{
			if (mRichText && tryParseColorOpenTag(mText, charIndex, out Color32 tagColor, out int tagLength))
			{
				mHasRichColor = true;
				++mRichColorTagCount;
				ensureColorStackCapacity(colorStackCount + 1);
				mColorStack[colorStackCount++] = richColor;
				richColor = tagColor;
				charIndex += tagLength;
				continue;
			}
			if (mRichText && tryParseColorCloseTag(mText, charIndex, out tagLength))
			{
				mHasRichColor = true;
				richColor = colorStackCount > 0 ? mColorStack[--colorStackCount] : new Color32(255, 255, 255, 255);
				charIndex += tagLength;
				continue;
			}
			int codePointLength = getCodePoint(mText, charIndex, out uint unicode);
			if (unicode == '\r')
			{
				charIndex += codePointLength;
				continue;
			}
			if (unicode == '\n')
			{
				addLine(lineStart, charIndex, trimEndSpacing(lineWidth, lineHasCharacter), lineAppliedWordGapCount, Mathf.Max(lineVisibleGlyphCount - 1, 0),
						lineGeometryMinX, lineGeometryMaxX, lineHasGeometry, true);
				mPreferredWidth = Mathf.Max(mPreferredWidth, trimEndSpacing(unwrappedWidth, lineHasCharacter));
				charIndex += codePointLength;
				lineStart = charIndex;
				lineWidth = 0.0f;
				unwrappedWidth = 0.0f;
				lineHasCharacter = false;
				lineWordGapCount = 0;
				lineAppliedWordGapCount = 0;
				lineVisibleGlyphCount = 0;
				lineGeometryMinX = float.PositiveInfinity;
				lineGeometryMaxX = float.NegativeInfinity;
				lineHasGeometry = false;
				continue;
			}
			TMP_Character character = null;
			TMP_FontAsset sourceFont = null;
			Glyph glyph = null;
			float glyphScale = 0.0f;
			float advance;
			if (unicode == '\t')
			{
				advance = getTabAdvance(primaryFontScale);
			}
			else if (tryGetLayoutCharacter(unicode, true, out character, out sourceFont))
			{
				glyph = character.glyph;
				float sourceFontScale = getFontScale(sourceFont.faceInfo);
				glyphScale = getGlyphScale(character, glyph, sourceFontScale);
				advance = glyph.metrics.horizontalAdvance * glyphScale + mCharacterSpacing;
			}
			else
			{
				advance = 0.0f;
			}
			if (mWordWrap && wrapWidth > 0.0f && lineHasCharacter && lineWidth + advance > wrapWidth)
			{
				addLine(lineStart, charIndex, trimEndSpacing(lineWidth, true), lineAppliedWordGapCount, Mathf.Max(lineVisibleGlyphCount - 1, 0),
						lineGeometryMinX, lineGeometryMaxX, lineHasGeometry, false);
				lineStart = charIndex;
				lineWidth = 0.0f;
				lineWordGapCount = 0;
				lineAppliedWordGapCount = 0;
				lineVisibleGlyphCount = 0;
				lineGeometryMinX = float.PositiveInfinity;
				lineGeometryMaxX = float.NegativeInfinity;
				lineHasGeometry = false;
			}
			float glyphPenX = lineWidth;
			lineWidth += advance;
			unwrappedWidth += advance;
			lineHasCharacter = true;
			if (character != null && glyph != null && glyph.metrics.width > 0.0f && glyph.metrics.height > 0.0f)
			{
				int atlasIndex = glyph.atlasIndex;
				if (tryGetAtlasTexture(sourceFont, atlasIndex, out Texture atlasTexture))
				{
					GlyphMetrics metrics = glyph.metrics;
					float geometryLeft = glyphPenX + metrics.horizontalBearingX * glyphScale;
					float geometryRight = geometryLeft + metrics.width * glyphScale;
					lineGeometryMinX = Mathf.Min(lineGeometryMinX, geometryLeft);
					lineGeometryMaxX = Mathf.Max(lineGeometryMaxX, geometryRight);
					lineHasGeometry = true;
					int glyphIndex = appendGlyphLayout(character, sourceFont, atlasIndex, mLineCount, lineWordGapCount, lineVisibleGlyphCount, glyphPenX, glyphScale, richColor);
					appendRenderRun(sourceFont, atlasIndex, atlasTexture, glyphIndex);
					lineAppliedWordGapCount = lineWordGapCount;
					++lineVisibleGlyphCount;
					++mVisibleGlyphCount;
					if (sourceFont != mFont)
					{
						++mFallbackGlyphCount;
					}
					if (atlasIndex > 0)
					{
						++mMultiAtlasGlyphCount;
					}
				}
				else
				{
					++mUnsupportedAtlasGlyphCount;
				}
			}
			if ((unicode == ' ' || unicode == '\t') && advance > 0.0f)
			{
				++lineWordGapCount;
			}
			charIndex += codePointLength;
		}
		addLine(lineStart, mText.Length, trimEndSpacing(lineWidth, lineHasCharacter),
			lineAppliedWordGapCount, Mathf.Max(lineVisibleGlyphCount - 1, 0), lineGeometryMinX, lineGeometryMaxX, lineHasGeometry, true);
		mPreferredWidth = Mathf.Max(mPreferredWidth, trimEndSpacing(unwrappedWidth, lineHasCharacter));
		float lineAdvance = Mathf.Max(faceInfo.lineHeight * primaryFontScale + mLineSpacing, 0.01f);
		mPreferredHeight = Mathf.Max(faceInfo.ascentLine * primaryFontScale - faceInfo.descentLine * primaryFontScale, 0.0f) + Mathf.Max(0, mLineCount - 1) * lineAdvance;
		mUsingAtlasRunElements = shouldUseAtlasRunElements();
	}
	private bool isSimplePrimaryLayoutTextEligible()
	{
		if (sSimpleLayoutPrimaryAtlasMaskEligibilityEnabled)
		{
			for (int i = 0; i < mText.Length; ++i)
			{
				char value = mText[i];
				if (value >= 128 || value == '\r' || value == '\n' || value == '\t' || (mRichText && value == '<'))
				{
					return false;
				}
				ulong bit = 1UL << (value & 63);
				ulong mask = value < 64 ? mPrimaryAtlasAsciiMaskLow : mPrimaryAtlasAsciiMaskHigh;
				if ((mask & bit) == 0UL)
				{
					return false;
				}
			}
			return true;
		}
		for (int i = 0; i < mText.Length; ++i)
		{
			char value = mText[i];
			if (value >= mAsciiCharacterCache.Length || value == '\r' || value == '\n' || value == '\t' || (mRichText && value == '<'))
			{
				return false;
			}
			TMP_Character character = mAsciiCharacterCache[value];
			if (character == null || character.glyph == null || character.glyph.atlasIndex != 0)
			{
				return false;
			}
		}
		return true;
	}
	private bool tryRebuildSimpleSingleLinePrimaryLayout()
	{
		if (mWordWrap ||
			mOverflowMode == FastUITextOverflowMode.Truncate ||
			mHorizontalAlignment > FastUITextHorizontalAlignment.Right ||
			mVerticalAlignment > FastUITextVerticalAlignment.Bottom ||
			mFont == null || string.IsNullOrEmpty(mText))
		{
			return false;
		}
		if (!tryGetAtlasTexture(mFont, 0, out Texture atlasTexture) || atlasTexture != mRenderTexture)
		{
			return false;
		}
		if (!isSimplePrimaryLayoutTextEligible())
		{
			return false;
		}
		getSimpleLayoutFontMetrics(out float fontScale, out float ascent, out float descent, out float invAtlasWidth, out float invAtlasHeight);
		float lineWidth = 0.0f;
		float paddingPixels = Mathf.Max(mMaterialPadding, 0.0f);
		float sdfScaleFactor = getSimpleLayoutSDFScaleFactor();
		acquireSimpleGlyphWriteTarget(mText.Length, out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphDataStart);
		var penXColumn = glyphData.getPenXColumn();
		var leftOffsetColumn = glyphData.getLeftOffsetColumn();
		var topOffsetColumn = glyphData.getTopOffsetColumn();
		var rightOffsetColumn = glyphData.getRightOffsetColumn();
		var bottomOffsetColumn = glyphData.getBottomOffsetColumn();
		var uvLeftColumn = glyphData.getUVLeftColumn();
		var uvBottomColumn = glyphData.getUVBottomColumn();
		var uvRightColumn = glyphData.getUVRightColumn();
		var uvTopColumn = glyphData.getUVTopColumn();
		var sdfScaleColumn = glyphData.getSDFScaleColumn();
		for (int i = 0; i < mText.Length; ++i)
		{
			TMP_Character character = mAsciiCharacterCache[mText[i]];
			Glyph glyph = character.glyph;
			float glyphScale = getGlyphScale(character, glyph, fontScale);
			float glyphPenX = lineWidth;
			GlyphMetrics metrics = glyph.metrics;
			lineWidth += metrics.horizontalAdvance * glyphScale + mCharacterSpacing;
			if (metrics.width <= 0.0f || metrics.height <= 0.0f)
			{
				continue;
			}
			GlyphRect glyphRect = glyph.glyphRect;
			float paddingPosition = paddingPixels * glyphScale;
			float leftOffset = metrics.horizontalBearingX * glyphScale - paddingPosition;
			float topOffset = metrics.horizontalBearingY * glyphScale + paddingPosition;
			int glyphIndex = glyphDataStart + mGlyphLayoutCount++;
			penXColumn[glyphIndex] = glyphPenX;
			leftOffsetColumn[glyphIndex] = leftOffset;
			topOffsetColumn[glyphIndex] = topOffset;
			rightOffsetColumn[glyphIndex] = leftOffset + metrics.width * glyphScale + paddingPosition * 2.0f;
			bottomOffsetColumn[glyphIndex] = topOffset - metrics.height * glyphScale - paddingPosition * 2.0f;
			uvLeftColumn[glyphIndex] = (glyphRect.x - paddingPixels) * invAtlasWidth;
			uvBottomColumn[glyphIndex] = (glyphRect.y - paddingPixels) * invAtlasHeight;
			uvRightColumn[glyphIndex] = (glyphRect.x + glyphRect.width + paddingPixels) * invAtlasWidth;
			uvTopColumn[glyphIndex] = (glyphRect.y + glyphRect.height + paddingPixels) * invAtlasHeight;
			sdfScaleColumn[glyphIndex] = Mathf.Max(glyphScale * sdfScaleFactor, 0.000001f);
			++mVisibleGlyphCount;
		}
		float width = trimEndSpacing(lineWidth, mText.Length > 0);
		addLine(0, mText.Length, width, 0, Mathf.Max(mGlyphLayoutCount - 1, 0), 0.0f, width, mGlyphLayoutCount > 0, true);
		mPreferredWidth = width;
		mPreferredHeight = Mathf.Max(ascent - descent, 0.0f);
		if (mGlyphLayoutCount > 0)
		{
			mRenderRunCount = 1;
			mFirstRun = new RenderRunLayout
			{
				mFont = mFont,
				mAtlasIndex = 0,
				mGlyphStart = 0,
				mGlyphCount = mGlyphLayoutCount,
				mBaseMaterial = mMaterial,
				mTexture = atlasTexture,
				mPadding = mMaterialPadding,
			};
		}
		mSimpleGlyphDataECS = glyphData;
		mSimpleGlyphDataStart = glyphDataStart;
		if (mCanvas != null)
		{
			mCanvas.setPersistentTextGlyphCount(this, mGlyphLayoutCount);
		}
		mSimpleSingleLinePrimaryLayout = true;
		return true;
	}
	// 诊断路径：只在Canvas显式开启Dirty分析时调用。生产ensureLayout/rebuildLayout保持原样。
	private int appendGlyphLayout(TMP_Character character, TMP_FontAsset font, int atlasIndex, int lineIndex,
									int wordGapIndex, int characterGapIndex, float penX, float glyphScale, Color32 richColor)
	{
		ensureGlyphLayoutCapacity(mGlyphLayoutCount + 1);
		int index = mGlyphLayoutCount++;
		mGlyphLayouts[index].mCharacter = character;
		mGlyphLayouts[index].mFont = font;
		mGlyphLayouts[index].mAtlasIndex = atlasIndex;
		mGlyphLayouts[index].mLineIndex = lineIndex;
		mGlyphLayouts[index].mWordGapIndex = wordGapIndex;
		mGlyphLayouts[index].mCharacterGapIndex = characterGapIndex;
		mGlyphLayouts[index].mPenX = penX;
		mGlyphLayouts[index].mGlyphScale = glyphScale;
		mGlyphLayouts[index].mRichColor = richColor;
		return index;
	}
	private void appendRenderRun(TMP_FontAsset font, int atlasIndex, Texture atlasTexture, int glyphIndex)
	{
		if (mRenderRunCount > 0)
		{
			int last = mRenderRunCount - 1;
			RenderRunLayout lastRun = getRenderRunLayout(last);
			if (lastRun.mFont == font && lastRun.mAtlasIndex == atlasIndex)
			{
				++lastRun.mGlyphCount;
				setRenderRunLayout(last, lastRun);
				return;
			}
		}
		int runIndex = mRenderRunCount++;
		Material material = getRunBaseMaterial(font);
		setRenderRunLayout(runIndex, new RenderRunLayout
		{
			mFont = font,
			mAtlasIndex = atlasIndex,
			mGlyphStart = glyphIndex,
			mGlyphCount = 1,
			mBaseMaterial = material,
			mTexture = atlasTexture,
			mPadding = material != null ? Mathf.Max(ShaderUtilities.GetPadding(material, false, false), 0.0f) : 0.0f,
		});
	}
	private void ensureSimpleGlyphECSCount(int required)
	{
		mSimpleGlyphECS ??= new FastUITextSimpleGlyphData_ECSList(Mathf.NextPowerOfTwo(Mathf.Max(required, 16)));
		while (mSimpleGlyphECS.Count < required)
		{
			mSimpleGlyphECS.Add(default);
		}
	}
	private void acquireSimpleGlyphWriteTarget(int required, out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart)
	{
		if (mCanvas != null && mCanvas.tryAcquirePersistentTextGlyphRange(this, required, out glyphData, out glyphStart))
		{
			return;
		}
		ensureSimpleGlyphECSCount(required);
		glyphData = mSimpleGlyphECS;
		glyphStart = 0;
	}
	private bool tryGetSimpleGlyphReadTarget(out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart)
	{
		glyphData = mSimpleGlyphDataECS;
		glyphStart = mSimpleGlyphDataStart;
		return glyphData != null;
	}
	public bool tryRefreshSimplePrimaryUV0Direct(FastUIMeshRenderer renderer, int slot, out int glyphCount, out int vertexCount)
	{
		glyphCount = 0;
		vertexCount = 0;
		if (renderer == null || mLayoutDirty || !mSimpleSingleLinePrimaryLayout || mUsingAtlasRunElements || mHasRichColor ||
			!tryGetSimpleGlyphReadTarget(out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart) || glyphData == null)
		{
			return false;
		}
		glyphCount = mGlyphLayoutCount;
		vertexCount = glyphCount * 4;
		return renderer.refreshSimpleTMPTextUV0Slot(slot, glyphData, glyphStart, glyphCount);
	}
	private int getSimpleGlyphSlotCapacity(int glyphCount)
	{
		int canvasCapacity = mCanvas != null ? mCanvas.getPersistentTextGlyphCapacity(this) : 0;
		return canvasCapacity > 0 ? canvasCapacity : Mathf.NextPowerOfTwo(Mathf.Max(glyphCount, 4));
	}
	// ：首次SimpleText Range Batch只需要保守的Vertex Capacity Hint，不触发Layout。
	// 已有Layout时严格沿用Simple Primary结果；Layout尚未建立时只接受明显满足单行Primary约束的文本，复杂语义保守回退旧路径。
	public int getInitialSimpleTextVertexCapacityHint()
	{
		if (mFont == null || string.IsNullOrEmpty(mText) || mWordWrap)
		{
			return 0;
		}
		if (!mLayoutDirty && (!mSimpleSingleLinePrimaryLayout || mUsingAtlasRunElements || mHasRichColor))
		{
			return 0;
		}
		for (int i = 0; i < mText.Length; ++i)
		{
			char value = mText[i];
			if (value == '\r' || value == '\n' || value == '\t' || (mRichText && value == '<'))
			{
				return 0;
			}
			if (mAsciiCharacterCache != null && mAsciiCharacterCache.Length > 0)
			{
				if (value >= mAsciiCharacterCache.Length)
				{
					return 0;
				}
				TMP_Character character = mAsciiCharacterCache[value];
				if (character == null || character.glyph == null || character.glyph.atlasIndex != 0)
				{
					return 0;
				}
			}
		}
		int glyphCapacity = Mathf.NextPowerOfTwo(Mathf.Max(mText.Length, 4));
		return glyphCapacity > 0 && glyphCapacity <= int.MaxValue / 4 ? glyphCapacity * 4 : 0;
	}
	private void markTextGeometryDirty()
	{
		if (mCanvas != null)
		{
			mCanvas.markTextElementGeometryDirty(this);
		}
	}
	// Canvas分配/复用VertexSlot后让下一次Layout直接绑定Persistent Glyph Range。
	public void notifyPersistentGlyphCacheAvailable()
	{
		mLayoutDirty = true;
		mSimpleGlyphDataECS = null;
		mSimpleGlyphDataStart = 0;
	}
	private void ensureGlyphLayoutCapacity(int required)
	{
		if (mGlyphLayouts != null && required <= mGlyphLayouts.Length)
		{
			return;
		}
		System.Array.Resize(ref mGlyphLayouts, Mathf.NextPowerOfTwo(Mathf.Max(required, 16)));
	}
	private void ensureRenderRunCapacity(int required)
	{
		int extraRequired = required - 1;
		if (extraRequired <= 0 || (mExtraRuns != null && extraRequired <= mExtraRuns.Length))
		{
			return;
		}
		System.Array.Resize(ref mExtraRuns, Mathf.NextPowerOfTwo(Mathf.Max(extraRequired, 2)));
	}
	private RenderRunLayout getRenderRunLayout(int index)
	{
		return index == 0 ? mFirstRun : mExtraRuns[index - 1];
	}
	private void setRenderRunLayout(int index, RenderRunLayout run)
	{
		if (index == 0)
		{
			mFirstRun = run;
			return;
		}
		ensureRenderRunCapacity(index + 1);
		mExtraRuns[index - 1] = run;
	}
	private void addLine(int start, int end, float width, int wordGapCount, int characterGapCount,
		float geometryMinX, float geometryMaxX, bool hasGeometry, bool endsParagraph)
	{
		LineLayout line = new()
		{
			mStart = start,
			mEnd = end,
			mWidth = Mathf.Max(width, 0.0f),
			mWordGapCount = Mathf.Max(wordGapCount, 0),
			mCharacterGapCount = Mathf.Max(characterGapCount, 0),
			mGeometryMinX = hasGeometry ? geometryMinX : 0.0f,
			mGeometryMaxX = hasGeometry ? geometryMaxX : 0.0f,
			mHasGeometry = hasGeometry,
			mEndsParagraph = endsParagraph,
		};
		if (mLineCount == 0)
		{
			mFirstLine = line;
		}
		else
		{
			ensureLineCapacity(mLineCount + 1);
			mExtraLines[mLineCount - 1] = line;
		}
		++mLineCount;
	}
	private void ensureLineCapacity(int required)
	{
		int extraRequired = required - 1;
		if (extraRequired <= 0 || (mExtraLines != null && extraRequired <= mExtraLines.Length))
		{
			return;
		}
		System.Array.Resize(ref mExtraLines, Mathf.NextPowerOfTwo(Mathf.Max(extraRequired, 4)));
	}
	private LineLayout getLineLayout(int index)
	{
		return index == 0 ? mFirstLine : mExtraLines[index - 1];
	}
	private int getLineStartValue(int index)
	{
		return index == 0 ? mFirstLine.mStart : mExtraLines[index - 1].mStart;
	}
	private int getLineEndValue(int index)
	{
		return index == 0 ? mFirstLine.mEnd : mExtraLines[index - 1].mEnd;
	}
	private float getLineWidthValue(int index)
	{
		return index == 0 ? mFirstLine.mWidth : mExtraLines[index - 1].mWidth;
	}
	private float getCharacterAdvance(uint unicode, float fontScale)
	{
		if (!tryGetCharacter(unicode, false, out TMP_Character character, out TMP_FontAsset sourceFont))
		{
			return 0.0f;
		}
		Glyph glyph = character.glyph;
		float sourceFontScale = sourceFont != null ? getFontScale(sourceFont.faceInfo) : fontScale;
		return glyph.metrics.horizontalAdvance * getGlyphScale(character, glyph, sourceFontScale) + mCharacterSpacing;
	}
	private float getTabAdvance(float fontScale)
	{
		float space = getCharacterAdvance(' ', fontScale);
		return space > 0.0f ? space * 4.0f : mFontSize;
	}
	private float trimEndSpacing(float width, bool hasCharacter)
	{
		return hasCharacter ? Mathf.Max(0.0f, width - mCharacterSpacing) : 0.0f;
	}
	private bool tryGetLayoutCharacter(uint unicode, bool countMissing, out TMP_Character character, out TMP_FontAsset sourceFont)
	{
		if (mFont != null && unicode < mAsciiCharacterCache.Length)
		{
			character = mAsciiCharacterCache[unicode];
			if (character != null)
			{
				sourceFont = mFont;
				return true;
			}
		}
		return tryGetCharacter(unicode, countMissing, out character, out sourceFont);
	}
	private bool tryGetCharacter(uint unicode, bool countMissing, out TMP_Character character, out TMP_FontAsset sourceFont)
	{
		character = null;
		sourceFont = null;
		if (mFont == null)
		{
			return false;
		}
		if (tryFindCharacter(mFont, unicode, 0, out character, out sourceFont))
		{
			return true;
		}
		if (countMissing)
		{
			++mMissingGlyphCount;
		}
		if (mMissingCharacter != 0 && unicode != mMissingCharacter &&
			tryFindCharacter(mFont, mMissingCharacter, 0, out character, out sourceFont))
		{
			return true;
		}
		if (unicode != '?' && tryFindCharacter(mFont, '?', 0, out character, out sourceFont))
		{
			return true;
		}
		return false;
	}
	private bool tryFindCharacter(TMP_FontAsset font, uint unicode, int depth, out TMP_Character character, out TMP_FontAsset sourceFont)
	{
		character = null;
		sourceFont = null;
		if (font == null)
		{
			return false;
		}
		ensureFallbackSearchPathCapacity(depth + 1);
		mFallbackSearchPath[depth] = font;
		if (font == mFont && unicode < mAsciiCharacterCache.Length)
		{
			character = mAsciiCharacterCache[unicode];
			if (character != null)
			{
				sourceFont = font;
				return true;
			}
		}
		var table = font.characterLookupTable;
		if (table != null && table.TryGetValue(unicode, out character))
		{
			sourceFont = font;
			return true;
		}
		var fallbackTable = font.fallbackFontAssetTable;
		if (fallbackTable == null || fallbackTable.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < fallbackTable.Count; ++i)
		{
			TMP_FontAsset fallback = fallbackTable[i];
			if (fallback == null || isFontInFallbackPath(fallback, depth))
			{
				continue;
			}
			if (tryFindCharacter(fallback, unicode, depth + 1, out character, out sourceFont))
			{
				return true;
			}
		}
		return false;
	}
	private bool isFontInFallbackPath(TMP_FontAsset font, int depth)
	{
		for (int i = 0; i <= depth; ++i)
		{
			if (mFallbackSearchPath[i] == font)
			{
				return true;
			}
		}
		return false;
	}
	private void ensureFallbackSearchPathCapacity(int required)
	{
		if (mFallbackSearchPath != null && required <= mFallbackSearchPath.Length)
		{
			return;
		}
		System.Array.Resize(ref mFallbackSearchPath, Mathf.NextPowerOfTwo(Mathf.Max(required, 16)));
	}
	public FastUITextCloneFontData createCloneFontData()
	{
		Material sourceMaterial = mMaterial;
		Material fontMaterial = mFont != null ? mFont.material : null;
		bool materialOverride = sourceMaterial != null && sourceMaterial != fontMaterial;
		Material effectiveMaterial = materialOverride ? sourceMaterial : fontMaterial;
		Texture texture = getMaterialTexture(effectiveMaterial);
		if (texture == null && mFont != null)
		{
			texture = mFont.atlasTexture;
		}
		float padding = effectiveMaterial != null ? Mathf.Max(ShaderUtilities.GetPadding(effectiveMaterial, false, false), 0.0f) : 0.0f;
		SharedAsciiCharacterCache shared = getSharedAsciiCharacterCache(mFont);
		TMP_Character[] asciiCharacters = shared != null ? shared.mCharacters : EMPTY_ASCII_CHARACTER_CACHE;
		ulong primaryMaskLow = shared != null ? shared.mPrimaryAtlasAsciiMaskLow : 0UL;
		ulong primaryMaskHigh = shared != null ? shared.mPrimaryAtlasAsciiMaskHigh : 0UL;
		return new FastUITextCloneFontData(mFont, sourceMaterial, effectiveMaterial, texture, fontMaterial, materialOverride, padding, asciiCharacters, primaryMaskLow, primaryMaskHigh);
	}
	public bool applyCloneFontData(FastUITextCloneFontData data)
	{
		if (data.mFont != mFont || data.mSourceMaterial != mMaterial)
		{
			return false;
		}
		mMaterial = data.mEffectiveMaterial;
		mRenderTexture = data.mRenderTexture;
		mLastFontMaterial = data.mLastFontMaterial;
		mMaterialOverride = data.mMaterialOverride;
		mMaterialPadding = data.mMaterialPadding;
		mAsciiCharacterCache = data.mAsciiCharacterCache ?? EMPTY_ASCII_CHARACTER_CACHE;
		mPrimaryAtlasAsciiMaskLow = data.mPrimaryAtlasAsciiMaskLow;
		mPrimaryAtlasAsciiMaskHigh = data.mPrimaryAtlasAsciiMaskHigh;
		return true;
	}
	private void refreshCharacterCache()
	{
		invalidateSimpleLayoutMetricsCache();
		invalidateSimpleNumberMetricRows();
		SharedAsciiCharacterCache shared = getSharedAsciiCharacterCache(mFont);
		if (shared != null)
		{
			buildPrimaryAtlasAsciiMasks(shared.mCharacters, out shared.mPrimaryAtlasAsciiMaskLow, out shared.mPrimaryAtlasAsciiMaskHigh);
		}
		mAsciiCharacterCache = shared != null ? shared.mCharacters : EMPTY_ASCII_CHARACTER_CACHE;
		mPrimaryAtlasAsciiMaskLow = shared != null ? shared.mPrimaryAtlasAsciiMaskLow : 0UL;
		mPrimaryAtlasAsciiMaskHigh = shared != null ? shared.mPrimaryAtlasAsciiMaskHigh : 0UL;
	}
	private void invalidateSimpleNumberMetricRows()
	{
		mSimpleNumberMetricRows = null;
		mSimpleNumberMetricRowsFont = null;
		mSimpleNumberMetricRowsFontSizeBits = 0;
		mSimpleNumberMetricSharedCache = null;
		mSimpleNumberMetricSharedVersion = 0;
	}
	private static void invalidateSharedSimpleNumberMetricRows(TMP_FontAsset font)
	{
		if (font != null && SHARED_ASCII_CHARACTER_CACHE.TryGetValue(font, out SharedAsciiCharacterCache shared) && shared != null)
		{
			shared.mSimpleNumberMetricRowsByFontSizeBits?.Clear();
			++shared.mSimpleNumberMetricVersion;
		}
	}
	private ushort[] getSimpleNumberMetricRows()
	{
		if (mFont == null)
		{
			return null;
		}
		int fontSizeBits = System.BitConverter.SingleToInt32Bits(mFontSize);
		if (mSimpleNumberMetricRows != null &&
			ReferenceEquals(mSimpleNumberMetricRowsFont, mFont) &&
			mSimpleNumberMetricRowsFontSizeBits == fontSizeBits &&
			mSimpleNumberMetricSharedCache != null &&
			mSimpleNumberMetricSharedVersion == mSimpleNumberMetricSharedCache.mSimpleNumberMetricVersion)
		{
			return mSimpleNumberMetricRows;
		}
		SharedAsciiCharacterCache shared = getSharedAsciiCharacterCache(mFont);
		if (shared == null)
		{
			return null;
		}
		shared.mSimpleNumberMetricRowsByFontSizeBits ??= new FastDictionary<int, ushort[]>();
		if (!shared.mSimpleNumberMetricRowsByFontSizeBits.TryGetValue(fontSizeBits, out ushort[] rows))
		{
			rows = buildSimpleNumberMetricRows(mFont, mFontSize, shared.mCharacters);
			shared.mSimpleNumberMetricRowsByFontSizeBits[fontSizeBits] = rows;
		}
		mSimpleNumberMetricRows = rows;
		mSimpleNumberMetricRowsFont = mFont;
		mSimpleNumberMetricRowsFontSizeBits = fontSizeBits;
		mSimpleNumberMetricSharedCache = shared;
		mSimpleNumberMetricSharedVersion = shared.mSimpleNumberMetricVersion;
		return rows;
	}
	private static ushort[] buildSimpleNumberMetricRows(TMP_FontAsset font, float fontSize, TMP_Character[] characters)
	{
		ushort[] rows = new ushort[10];
		if (font == null || characters == null || characters.Length <= '9')
		{
			return rows;
		}
		FaceInfo faceInfo = font.faceInfo;
		float pointSize = faceInfo.pointSize > 0.0f ? faceInfo.pointSize : 1.0f;
		float faceScale = faceInfo.scale > 0.0f ? faceInfo.scale : 1.0f;
		float fontScale = fontSize / pointSize * faceScale;
		for (int oldDigit = 0; oldDigit < 10; ++oldDigit)
		{
			TMP_Character oldCharacter = characters['0' + oldDigit];
			for (int newDigit = 0; newDigit < 10; ++newDigit)
			{
				if (hasEquivalentSimpleGlyphMetricsStatic(oldCharacter, characters['0' + newDigit], fontScale))
				{
					rows[oldDigit] |= (ushort)(1 << newDigit);
				}
			}
		}
		return rows;
	}
	private static bool hasEquivalentSimpleGlyphMetricsStatic(TMP_Character oldCharacter, TMP_Character newCharacter, float fontScale)
	{
		if (oldCharacter == null ||
			newCharacter == null ||
			oldCharacter.glyph == null ||
			newCharacter.glyph == null ||
			oldCharacter.glyph.atlasIndex != 0 ||
			newCharacter.glyph.atlasIndex != 0)
		{
			return false;
		}
		Glyph oldGlyph = oldCharacter.glyph;
		Glyph newGlyph = newCharacter.glyph;
		float oldCharacterScale = oldCharacter.scale > 0.0f ? oldCharacter.scale : 1.0f;
		float newCharacterScale = newCharacter.scale > 0.0f ? newCharacter.scale : 1.0f;
		float oldGlyphScale = oldGlyph.scale > 0.0f ? oldGlyph.scale : 1.0f;
		float newGlyphScale = newGlyph.scale > 0.0f ? newGlyph.scale : 1.0f;
		float oldScale = fontScale * oldCharacterScale * oldGlyphScale;
		float newScale = fontScale * newCharacterScale * newGlyphScale;
		if (!simpleMetricEqual(oldScale, newScale))
		{
			return false;
		}
		GlyphMetrics oldMetrics = oldGlyph.metrics;
		GlyphMetrics newMetrics = newGlyph.metrics;
		return simpleMetricEqual(oldMetrics.horizontalAdvance * oldScale, newMetrics.horizontalAdvance * newScale) &&
			   simpleMetricEqual(oldMetrics.horizontalBearingX * oldScale, newMetrics.horizontalBearingX * newScale) &&
			   simpleMetricEqual(oldMetrics.horizontalBearingY * oldScale, newMetrics.horizontalBearingY * newScale) &&
			   simpleMetricEqual(oldMetrics.width * oldScale, newMetrics.width * newScale) &&
			   simpleMetricEqual(oldMetrics.height * oldScale, newMetrics.height * newScale);
	}
	private static void buildPrimaryAtlasAsciiMasks(TMP_Character[] characters, out ulong maskLow, out ulong maskHigh)
	{
		maskLow = 0UL;
		maskHigh = 0UL;
		if (characters == null)
		{
			return;
		}
		int count = Mathf.Min(characters.Length, 128);
		for (int i = 0; i < count; ++i)
		{
			bool valid = i == '\r' || i == '\n' || i == '\t';
			if (!valid)
			{
				TMP_Character character = characters[i];
				valid = character != null && character.glyph != null && character.glyph.atlasIndex == 0;
			}
			if (!valid)
			{
				continue;
			}
			if (i < 64)
			{
				maskLow |= 1UL << i;
			}
			else
			{
				maskHigh |= 1UL << (i - 64);
			}
		}
	}
	private static SharedAsciiCharacterCache getSharedAsciiCharacterCache(TMP_FontAsset font)
	{
		if (font == null)
		{
			return null;
		}
		var table = font.characterLookupTable;
		int characterCount = table != null ? table.Count : 0;
		if (SHARED_ASCII_CHARACTER_CACHE.TryGetValue(font, out SharedAsciiCharacterCache shared) &&
			shared != null &&
			shared.mCharacterCount == characterCount &&
			shared.mCharacters != null)
		{
			return shared;
		}
		TMP_Character[] characters = new TMP_Character[128];
		if (table != null)
		{
			for (int i = 0; i < characters.Length; ++i)
			{
				table.TryGetValue((uint)i, out characters[i]);
			}
		}
		buildPrimaryAtlasAsciiMasks(characters, out ulong primaryMaskLow, out ulong primaryMaskHigh);
		shared = new SharedAsciiCharacterCache
		{
			mCharacterCount = characterCount,
			mCharacters = characters,
			mPrimaryAtlasAsciiMaskLow = primaryMaskLow,
			mPrimaryAtlasAsciiMaskHigh = primaryMaskHigh,
		};
		SHARED_ASCII_CHARACTER_CACHE[font] = shared;
		return shared;
	}
	private void invalidateSimpleLayoutMetricsCache()
	{
		mSimpleLayoutMetricsValid = false;
		mSimpleLayoutMetricsFont = null;
		mSimpleLayoutMetricsFontSizeBits = 0;
		mSimpleLayoutSDFScaleValid = false;
		mSimpleLayoutSDFCanvas = null;
		mSimpleLayoutSDFScaleVersion = 0;
	}
	private void getSimpleLayoutFontMetrics(out float fontScale, out float ascent, out float descent, out float invAtlasWidth, out float invAtlasHeight)
	{
		if (!sSimpleLayoutMetricsCacheEnabled || mFont == null)
		{
			FaceInfo faceInfo = mFont != null ? mFont.faceInfo : default;
			fontScale = mFont != null ? getFontScale(faceInfo) : 1.0f;
			ascent = faceInfo.ascentLine * fontScale;
			descent = faceInfo.descentLine * fontScale;
			invAtlasWidth = 1.0f / Mathf.Max(mFont != null ? mFont.atlasWidth : 1, 1);
			invAtlasHeight = 1.0f / Mathf.Max(mFont != null ? mFont.atlasHeight : 1, 1);
			return;
		}
		int fontSizeBits = System.BitConverter.SingleToInt32Bits(mFontSize);
		if (!mSimpleLayoutMetricsValid || !ReferenceEquals(mSimpleLayoutMetricsFont, mFont) || mSimpleLayoutMetricsFontSizeBits != fontSizeBits)
		{
			FaceInfo faceInfo = mFont.faceInfo;
			mSimpleLayoutFontScale = getFontScale(faceInfo);
			mSimpleLayoutAscent = faceInfo.ascentLine * mSimpleLayoutFontScale;
			mSimpleLayoutDescent = faceInfo.descentLine * mSimpleLayoutFontScale;
			mSimpleLayoutInvAtlasWidth = 1.0f / Mathf.Max(mFont.atlasWidth, 1);
			mSimpleLayoutInvAtlasHeight = 1.0f / Mathf.Max(mFont.atlasHeight, 1);
			mSimpleLayoutMetricsFont = mFont;
			mSimpleLayoutMetricsFontSizeBits = fontSizeBits;
			mSimpleLayoutMetricsValid = true;
			++sSimpleLayoutMetricsCacheMissCount;
		}
		else
		{
			++sSimpleLayoutMetricsCacheHitCount;
		}
		fontScale = mSimpleLayoutFontScale;
		ascent = mSimpleLayoutAscent;
		descent = mSimpleLayoutDescent;
		invAtlasWidth = mSimpleLayoutInvAtlasWidth;
		invAtlasHeight = mSimpleLayoutInvAtlasHeight;
	}
	private float getSimpleLayoutSDFScaleFactor()
	{
		if (!sSimpleLayoutMetricsCacheEnabled || mCanvas == null || mRectTransform == null)
		{
			return getSDFScaleFactor();
		}
		int scaleVersion = mCanvas.getTextSDFScaleCacheVersion();
		if (mSimpleLayoutSDFScaleValid && ReferenceEquals(mSimpleLayoutSDFCanvas, mCanvas) && mSimpleLayoutSDFScaleVersion == scaleVersion)
		{
			++sSimpleLayoutSDFScaleCacheHitCount;
			return mSimpleLayoutSDFScaleFactor;
		}
		float textScaleY = Mathf.Abs(mRectTransform.lossyScale.y);
		float canvasScaleY = mCanvas.getTextSDFCanvasScaleY();
		if (canvasScaleY <= 0.000001f)
		{
			canvasScaleY = 1.0f;
		}
		mSimpleLayoutSDFScaleFactor = textScaleY / canvasScaleY;
		mSimpleLayoutSDFCanvas = mCanvas;
		mSimpleLayoutSDFScaleVersion = scaleVersion;
		mSimpleLayoutSDFScaleValid = true;
		++sSimpleLayoutSDFScaleCacheMissCount;
		return mSimpleLayoutSDFScaleFactor;
	}
	private float getFontScale(FaceInfo faceInfo)
	{
		float pointSize = faceInfo.pointSize > 0.0f ? faceInfo.pointSize : 1.0f;
		float faceScale = faceInfo.scale > 0.0f ? faceInfo.scale : 1.0f;
		return mFontSize / pointSize * faceScale;
	}
	private float getGlyphScale(TMP_Character character, Glyph glyph, float fontScale)
	{
		float characterScale = character.scale > 0.0f ? character.scale : 1.0f;
		float glyphScale = glyph.scale > 0.0f ? glyph.scale : 1.0f;
		return fontScale * characterScale * glyphScale;
	}
	private float getBlockTop(Rect rect, float textHeight)
	{
		switch (mVerticalAlignment)
		{
			case FastUITextVerticalAlignment.Top:
				return rect.yMax;
			case FastUITextVerticalAlignment.Bottom:
				return rect.yMin + textHeight;
			default:
				return rect.center.y + textHeight * 0.5f;
		}
	}
	private float getLineStartX(Rect rect, int lineIndex)
	{
		LineLayout line = getLineLayout(Mathf.Clamp(lineIndex, 0, Mathf.Max(mLineCount - 1, 0)));
		switch (mHorizontalAlignment)
		{
			case FastUITextHorizontalAlignment.Center:
				return rect.center.x - line.mWidth * 0.5f;
			case FastUITextHorizontalAlignment.Right:
				return rect.xMax - line.mWidth;
			case FastUITextHorizontalAlignment.Geometry:
				return line.mHasGeometry
					? rect.center.x - (line.mGeometryMinX + line.mGeometryMaxX) * 0.5f
					: rect.center.x - line.mWidth * 0.5f;
			default:
				return rect.xMin;
		}
	}

	// TMP Truncate is a layout overflow rule, not a requirement that the padded
	// SDF quad be fully contained by the RectTransform. Material padding is allowed
	// to extend outside the logical text bounds.
	private int getTruncateVisibleLineCount(Rect rect, float ascent, float descent, float lineAdvance)
	{
		if (mOverflowMode != FastUITextOverflowMode.Truncate || mLineCount <= 0)
		{
			return mLineCount;
		}
		const float EPSILON = 0.0001f;
		float firstLineHeight = Mathf.Max(ascent - descent, 0.0f);
		if (firstLineHeight <= EPSILON)
		{
			return mLineCount;
		}
		if (rect.height + EPSILON < firstLineHeight)
		{
			return 0;
		}
		int visibleLineCount = 1;
		if (lineAdvance > EPSILON)
		{
			visibleLineCount += Mathf.FloorToInt((rect.height - firstLineHeight + EPSILON) / lineAdvance);
		}
		return Mathf.Clamp(visibleLineCount, 0, mLineCount);
	}
	private int getTruncateGlyphEnd(int visibleLineCount, float maxWidth)
	{
		if (mOverflowMode != FastUITextOverflowMode.Truncate)
		{
			return mGlyphLayoutCount;
		}
		const float EPSILON = 0.0001f;
		maxWidth = Mathf.Max(maxWidth, 0.0f);
		for (int i = 0; i < mGlyphLayoutCount; ++i)
		{
			GlyphLayout layout = mGlyphLayouts[i];
			if (layout.mLineIndex >= visibleLineCount)
			{
				return i;
			}
			Glyph glyph = layout.mCharacter?.glyph;
			if (glyph == null)
			{
				continue;
			}
			// Match TMP's logical horizontal overflow test. Do not include SDF material
			// padding here; padding belongs to rendering, not text-container fit.
			float logicalEnd = layout.mPenX + glyph.metrics.horizontalAdvance * layout.mGlyphScale;
			if (logicalEnd > maxWidth + EPSILON)
			{
				return i;
			}
		}
		return mGlyphLayoutCount;
	}
	private void getTruncateLineRenderInfo(int lineIndex, int truncateGlyphEnd,
		out float renderWidth, out float geometryMinX, out float geometryMaxX, out bool hasGeometry)
	{
		LineLayout line = getLineLayout(Mathf.Clamp(lineIndex, 0, Mathf.Max(mLineCount - 1, 0)));
		renderWidth = 0.0f;
		geometryMinX = float.PositiveInfinity;
		geometryMaxX = float.NegativeInfinity;
		hasGeometry = false;
		bool foundGlyph = false;
		for (int i = 0; i < truncateGlyphEnd && i < mGlyphLayoutCount; ++i)
		{
			GlyphLayout layout = mGlyphLayouts[i];
			if (layout.mLineIndex < lineIndex)
			{
				continue;
			}
			if (layout.mLineIndex > lineIndex)
			{
				break;
			}
			Glyph glyph = layout.mCharacter?.glyph;
			if (glyph == null)
			{
				continue;
			}
			foundGlyph = true;
			GlyphMetrics metrics = glyph.metrics;
			renderWidth = Mathf.Max(renderWidth, layout.mPenX + metrics.horizontalAdvance * layout.mGlyphScale);
			if (metrics.width > 0.0f && metrics.height > 0.0f)
			{
				float left = layout.mPenX + metrics.horizontalBearingX * layout.mGlyphScale;
				float right = left + metrics.width * layout.mGlyphScale;
				geometryMinX = Mathf.Min(geometryMinX, left);
				geometryMaxX = Mathf.Max(geometryMaxX, right);
				hasGeometry = true;
			}
		}
		if (!foundGlyph)
		{
			renderWidth = Mathf.Min(line.mWidth, Mathf.Max(mRectTransform.rect.width, 0.0f));
		}
	}
	private float getTruncateLineStartX(Rect rect, int lineIndex, int truncateGlyphEnd)
	{
		getTruncateLineRenderInfo(lineIndex, truncateGlyphEnd,
			out float renderWidth, out float geometryMinX, out float geometryMaxX, out bool hasGeometry);
		switch (mHorizontalAlignment)
		{
			case FastUITextHorizontalAlignment.Center:
				return rect.center.x - renderWidth * 0.5f;
			case FastUITextHorizontalAlignment.Right:
				return rect.xMax - renderWidth;
			case FastUITextHorizontalAlignment.Geometry:
				return hasGeometry
					? rect.center.x - (geometryMinX + geometryMaxX) * 0.5f
					: rect.center.x - renderWidth * 0.5f;
			default:
				return rect.xMin;
		}
	}
	private int getLineJustificationGapCount(int lineIndex)
	{
		LineLayout line = getLineLayout(Mathf.Clamp(lineIndex, 0, Mathf.Max(mLineCount - 1, 0)));
		return line.mWordGapCount > 0 ? line.mWordGapCount : line.mCharacterGapCount;
	}
	private bool usesWordJustificationGaps(int lineIndex)
	{
		LineLayout line = getLineLayout(Mathf.Clamp(lineIndex, 0, Mathf.Max(mLineCount - 1, 0)));
		return line.mWordGapCount > 0;
	}
	private bool isLineJustified(int lineIndex)
	{
		if (mHorizontalAlignment != FastUITextHorizontalAlignment.Justified &&
			mHorizontalAlignment != FastUITextHorizontalAlignment.Flush)
		{
			return false;
		}
		LineLayout line = getLineLayout(Mathf.Clamp(lineIndex, 0, Mathf.Max(mLineCount - 1, 0)));
		return getLineJustificationGapCount(lineIndex) > 0 &&
			(mHorizontalAlignment == FastUITextHorizontalAlignment.Flush || !line.mEndsParagraph);
	}
	private float getLineJustificationStep(Rect rect, int lineIndex)
	{
		if (!isLineJustified(lineIndex))
		{
			return 0.0f;
		}
		LineLayout line = getLineLayout(lineIndex);
		int gapCount = getLineJustificationGapCount(lineIndex);
		float extra = Mathf.Max(rect.width - line.mWidth, 0.0f);
		return gapCount > 0 ? extra / gapCount : 0.0f;
	}
	private int normalizeStringIndex(int stringIndex)
	{
		stringIndex = Mathf.Clamp(stringIndex, 0, mText != null ? mText.Length : 0);
		if (mText != null && stringIndex > 0 && stringIndex < mText.Length &&
			char.IsLowSurrogate(mText[stringIndex]) && char.IsHighSurrogate(mText[stringIndex - 1]))
		{
			--stringIndex;
		}
		return stringIndex;
	}
	private void getLayoutMetrics(out float ascent, out float descent, out float lineAdvance, out float textHeight)
	{
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		ascent = faceInfo.ascentLine * fontScale;
		descent = faceInfo.descentLine * fontScale;
		lineAdvance = Mathf.Max(faceInfo.lineHeight * fontScale + mLineSpacing, 0.01f);
		int lineCount = Mathf.Max(mLineCount, 1);
		textHeight = Mathf.Max(ascent - descent, 0.0f) + Mathf.Max(0, lineCount - 1) * lineAdvance;
	}
	private float getLineBaseline(int lineIndex, float ascent, float lineAdvance, float textHeight)
	{
		Rect rect = mRectTransform.rect;
		int safeLineIndex = Mathf.Max(lineIndex, 0);
		switch (mVerticalAlignment)
		{
			case FastUITextVerticalAlignment.Baseline:
				return rect.center.y - safeLineIndex * lineAdvance;
			case FastUITextVerticalAlignment.Midline:
				{
					FaceInfo faceInfo = mFont.faceInfo;
					float fontScale = getFontScale(faceInfo);
					return rect.center.y - faceInfo.meanLine * fontScale - safeLineIndex * lineAdvance;
				}
			case FastUITextVerticalAlignment.Capline:
				{
					FaceInfo faceInfo = mFont.faceInfo;
					float fontScale = getFontScale(faceInfo);
					return rect.center.y - faceInfo.capLine * fontScale - safeLineIndex * lineAdvance;
				}
			default:
				{
					float blockTop = getBlockTop(rect, textHeight);
					return blockTop - ascent - safeLineIndex * lineAdvance;
				}
		}
	}
	private float getStringIndexLocalX(int stringIndex, int lineIndex)
	{
		Rect rect = mRectTransform.rect;
		if (mFont == null || mLineCount <= 0)
		{
			return rect.xMin;
		}
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		int lineStart = getLineStartValue(lineIndex);
		int lineEnd = getLineEndValue(lineIndex);
		stringIndex = normalizeStringIndex(stringIndex);
		float startX = getLineStartX(rect, lineIndex);
		if (stringIndex <= lineStart)
		{
			return startX;
		}
		if (stringIndex >= lineEnd)
		{
			LineLayout line = getLineLayout(lineIndex);
			return startX + line.mWidth + getLineJustificationStep(rect, lineIndex) * getLineJustificationGapCount(lineIndex);
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		float x = startX;
		float justifyStep = getLineJustificationStep(rect, lineIndex);
		bool useWordGaps = usesWordJustificationGaps(lineIndex);
		int charIndex = lineStart;
		while (charIndex < stringIndex && charIndex < lineEnd)
		{
			if (mRichText && tryParseColorOpenTag(mText, charIndex, out _, out int tagLength))
			{
				charIndex += tagLength;
				continue;
			}
			if (mRichText && tryParseColorCloseTag(mText, charIndex, out tagLength))
			{
				charIndex += tagLength;
				continue;
			}
			int codePointLength = getCodePoint(mText, charIndex, out uint unicode);
			if (charIndex + codePointLength > stringIndex)
			{
				break;
			}
			float advance = 0.0f;
			if (unicode == '\t')
			{
				advance = getTabAdvance(fontScale);
			}
			else if (unicode != '\r' && unicode != '\n')
			{
				advance = getCharacterAdvance(unicode, fontScale);
			}
			x += advance;
			if (justifyStep > 0.0f)
			{
				if (useWordGaps)
				{
					if (unicode == ' ' || unicode == '\t')
					{
						x += justifyStep;
					}
				}
				else if (advance > 0.0f && charIndex + codePointLength < lineEnd)
				{
					x += justifyStep;
				}
			}
			charIndex += codePointLength;
		}
		return x;
	}
	private int getNearestStringIndexOnLine(int lineIndex, float localX)
	{
		lineIndex = Mathf.Clamp(lineIndex, 0, mLineCount - 1);
		int lineStart = getLineStartValue(lineIndex);
		int lineEnd = getLineEndValue(lineIndex);
		Rect rect = mRectTransform.rect;
		float penX = getLineStartX(rect, lineIndex);
		if (localX <= penX)
		{
			return lineStart;
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		float justifyStep = getLineJustificationStep(rect, lineIndex);
		bool useWordGaps = usesWordJustificationGaps(lineIndex);
		int charIndex = lineStart;
		while (charIndex < lineEnd)
		{
			if (mRichText && tryParseColorOpenTag(mText, charIndex, out _, out int tagLength))
			{
				charIndex += tagLength;
				continue;
			}
			if (mRichText && tryParseColorCloseTag(mText, charIndex, out tagLength))
			{
				charIndex += tagLength;
				continue;
			}
			int codePointLength = getCodePoint(mText, charIndex, out uint unicode);
			float advance = 0.0f;
			if (unicode == '\t')
			{
				advance = getTabAdvance(fontScale);
			}
			else if (unicode != '\r' && unicode != '\n')
			{
				advance = getCharacterAdvance(unicode, fontScale);
			}
			float nextX = penX + advance;
			if (justifyStep > 0.0f)
			{
				if (useWordGaps)
				{
					if (unicode == ' ' || unicode == '\t')
					{
						nextX += justifyStep;
					}
				}
				else if (advance > 0.0f && charIndex + codePointLength < lineEnd)
				{
					nextX += justifyStep;
				}
			}
			if (localX < (penX + nextX) * 0.5f)
			{
				return charIndex;
			}
			penX = nextX;
			charIndex += codePointLength;
		}
		return lineEnd;
	}
	private void buildGlyphRange(int glyphStart, int glyphCount, float paddingPixels, FastUIGeometryBuilder builder,
		Matrix4x4 canvasWorldToLocal)
	{
		if (glyphCount <= 0 || mFont == null || mLineCount <= 0)
		{
			return;
		}
		if (mSimpleSingleLinePrimaryLayout && glyphStart == 0 && glyphCount == mGlyphLayoutCount)
		{
			buildSimpleSingleLinePrimaryGlyphRange(builder, canvasWorldToLocal);
			return;
		}
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		float ascent = faceInfo.ascentLine * fontScale;
		float descent = faceInfo.descentLine * fontScale;
		float lineAdvance = Mathf.Max(faceInfo.lineHeight * fontScale + mLineSpacing, 0.01f);
		Rect rect = mRectTransform.rect;
		int renderLineCount = getTruncateVisibleLineCount(rect, ascent, descent, lineAdvance);
		if (renderLineCount <= 0)
		{
			return;
		}
		float textHeight = Mathf.Max(ascent - descent, 0.0f) + Mathf.Max(0, renderLineCount - 1) * lineAdvance;
		int truncateGlyphEnd = getTruncateGlyphEnd(renderLineCount, rect.width);
		bool truncate = mOverflowMode == FastUITextOverflowMode.Truncate;
		bool translationOnly = tryGetGeometryTranslation(out Vector3 translation);
		Matrix4x4 geometryMatrix = translationOnly ? Matrix4x4.identity : getGeometryLocalToCanvasMatrix(canvasWorldToLocal);
		float sdfScaleFactor = getSDFScaleFactor();
		int end = Mathf.Min(glyphStart + glyphCount, mGlyphLayoutCount);
		if (truncate)
		{
			end = Mathf.Min(end, truncateGlyphEnd);
		}
		for (int i = Mathf.Max(glyphStart, 0); i < end; ++i)
		{
			GlyphLayout layout = mGlyphLayouts[i];
			int lineIndex = Mathf.Clamp(layout.mLineIndex, 0, mLineCount - 1);
			if (truncate && lineIndex >= renderLineCount)
			{
				break;
			}
			LineLayout line = getLineLayout(lineIndex);
			int justifyGapIndex = line.mWordGapCount > 0 ? layout.mWordGapIndex : layout.mCharacterGapIndex;
			float lineStartX = truncate
				? getTruncateLineStartX(rect, lineIndex, truncateGlyphEnd)
				: getLineStartX(rect, lineIndex);
			float penX = lineStartX + layout.mPenX +
				getLineJustificationStep(rect, lineIndex) * justifyGapIndex;
			float baseline = getLineBaseline(lineIndex, ascent, lineAdvance, textHeight);
			addGlyph(builder, penX, baseline, layout.mCharacter, layout.mFont, layout.mGlyphScale, paddingPixels, layout.mRichColor,
				translationOnly, translation, geometryMatrix, sdfScaleFactor);
		}
	}
	private void buildSimpleSingleLinePrimaryGlyphRange(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		float ascent = faceInfo.ascentLine * fontScale;
		float textHeight = Mathf.Max(ascent - faceInfo.descentLine * fontScale, 0.0f);
		Rect rect = mRectTransform.rect;
		float baseline = getBlockTop(rect, textHeight) - ascent;
		float lineStartX = getLineStartX(rect, 0);
		if (!tryGetSimpleGlyphReadTarget(out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphDataStart))
		{
			return;
		}
		var penXColumn = glyphData.getPenXColumn();
		var leftOffsetColumn = glyphData.getLeftOffsetColumn();
		var topOffsetColumn = glyphData.getTopOffsetColumn();
		var rightOffsetColumn = glyphData.getRightOffsetColumn();
		var bottomOffsetColumn = glyphData.getBottomOffsetColumn();
		var uvLeftColumn = glyphData.getUVLeftColumn();
		var uvBottomColumn = glyphData.getUVBottomColumn();
		var uvRightColumn = glyphData.getUVRightColumn();
		var uvTopColumn = glyphData.getUVTopColumn();
		var sdfScaleColumn = glyphData.getSDFScaleColumn();
		if (tryGetGeometryTranslation(out Vector3 translation))
		{
			for (int i = 0; i < mGlyphLayoutCount; ++i)
			{
				int glyphIndex = glyphDataStart + i;
				addSimplePrimaryGlyphTranslation(builder, lineStartX + penXColumn[glyphIndex], baseline,
					leftOffsetColumn[glyphIndex], topOffsetColumn[glyphIndex], rightOffsetColumn[glyphIndex], bottomOffsetColumn[glyphIndex],
					uvLeftColumn[glyphIndex], uvBottomColumn[glyphIndex], uvRightColumn[glyphIndex], uvTopColumn[glyphIndex], sdfScaleColumn[glyphIndex], translation);
			}
			return;
		}
		Matrix4x4 geometryMatrix = getGeometryLocalToCanvasMatrix(canvasWorldToLocal);
		for (int i = 0; i < mGlyphLayoutCount; ++i)
		{
			int glyphIndex = glyphDataStart + i;
			addSimplePrimaryGlyphMatrix(builder, lineStartX + penXColumn[glyphIndex], baseline,
				leftOffsetColumn[glyphIndex], topOffsetColumn[glyphIndex], rightOffsetColumn[glyphIndex], bottomOffsetColumn[glyphIndex],
				uvLeftColumn[glyphIndex], uvBottomColumn[glyphIndex], uvRightColumn[glyphIndex], uvTopColumn[glyphIndex], sdfScaleColumn[glyphIndex], geometryMatrix);
		}
	}
	private void addSimplePrimaryGlyphTranslation(FastUIGeometryBuilder builder, float penX, float baseline,
		float leftOffset, float topOffset, float rightOffset, float bottomOffset, float uvLeft, float uvBottom, float uvRight, float uvTop,
		float sdfScale, Vector3 translation)
	{
		float left = penX + leftOffset;
		float top = baseline + topOffset;
		float right = penX + rightOffset;
		float bottom = baseline + bottomOffset;
		Vector3 bottomLeft = new(left + translation.x, bottom + translation.y, translation.z);
		Vector3 topLeft = new(left + translation.x, top + translation.y, translation.z);
		Vector3 topRight = new(right + translation.x, top + translation.y, translation.z);
		Vector3 bottomRight = new(right + translation.x, bottom + translation.y, translation.z);
		builder.AddTMPQuad(bottomLeft, topLeft, topRight, bottomRight,
			new Vector4(uvLeft, uvBottom, 0.0f, sdfScale), new Vector4(uvLeft, uvTop, 0.0f, sdfScale),
			new Vector4(uvRight, uvTop, 0.0f, sdfScale), new Vector4(uvRight, uvBottom, 0.0f, sdfScale),
			new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f));
	}
	private void addSimplePrimaryGlyphMatrix(FastUIGeometryBuilder builder, float penX, float baseline,
		float leftOffset, float topOffset, float rightOffset, float bottomOffset, float uvLeft, float uvBottom, float uvRight, float uvTop,
		float sdfScale, Matrix4x4 geometryMatrix)
	{
		float left = penX + leftOffset;
		float top = baseline + topOffset;
		float right = penX + rightOffset;
		float bottom = baseline + bottomOffset;
		Vector3 bottomLeft = geometryMatrix.MultiplyPoint3x4(new(left, bottom, 0.0f));
		Vector3 topLeft = geometryMatrix.MultiplyPoint3x4(new(left, top, 0.0f));
		Vector3 topRight = geometryMatrix.MultiplyPoint3x4(new(right, top, 0.0f));
		Vector3 bottomRight = geometryMatrix.MultiplyPoint3x4(new(right, bottom, 0.0f));
		builder.AddTMPQuad(bottomLeft, topLeft, topRight, bottomRight,
			new Vector4(uvLeft, uvBottom, 0.0f, sdfScale), new Vector4(uvLeft, uvTop, 0.0f, sdfScale),
			new Vector4(uvRight, uvTop, 0.0f, sdfScale), new Vector4(uvRight, uvBottom, 0.0f, sdfScale),
			new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f));
	}
	// 小批量保留直接路径，避免Canvas Batch固定开销。
	public bool tryRebuildSimplePrimaryGeometryDirect(FastUIMeshRenderer renderer, int slot, Matrix4x4 canvasWorldToLocal, Color32 color,
		out FastUIGeometryUpdateResult result, out int glyphCount)
	{
		result = default;
		glyphCount = 0;
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		ensureLayout();
		if (!mSimpleSingleLinePrimaryLayout || mUsingAtlasRunElements || mHasRichColor || mFont == null || renderer == null ||
			!tryGetSimpleGlyphReadTarget(out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart))
		{
			return false;
		}
		glyphCount = mGlyphLayoutCount;
		int glyphCapacity = getSimpleGlyphSlotCapacity(glyphCount);
		FaceInfo faceInfo = mFont.faceInfo;
		float fontScale = getFontScale(faceInfo);
		float ascent = faceInfo.ascentLine * fontScale;
		float textHeight = Mathf.Max(ascent - faceInfo.descentLine * fontScale, 0.0f);
		Rect rect = mRectTransform.rect;
		float baseline = getBlockTop(rect, textHeight) - ascent;
		float lineStartX = getLineStartX(rect, 0);
		bool translationOnly = tryGetGeometryTranslation(out Vector3 translation);
		Matrix4x4 geometryMatrix = translationOnly ? Matrix4x4.identity : getGeometryLocalToCanvasMatrix(canvasWorldToLocal);
		result = renderer.rebuildSimpleTMPTextSlot(slot, glyphData, glyphStart, glyphCount, glyphCapacity, lineStartX, baseline,
			translationOnly, translation, geometryMatrix, color);
		return true;
	}
	// Batch入口只生成轻量Text Work。
	// Glyph中间结果已经由Layout直接写入Canvas Persistent Glyph ECS，这里只引用稳定Glyph Range，不再做第二次Glyph复制。
	public bool tryAppendSimplePrimaryBatchGeometry(FastUITextBatchWorkData_ECSList workECS, FastUIMeshRenderer renderer, int slot,
		Color32 color, FastUIDirtyFlags flags, Matrix4x4 canvasWorldToLocal, bool useCachedLocalState, bool useCachedRectAlignment,
		bool useCachedGeometryMatrix, out FastUIGeometryUpdateResult result, out int glyphCount, out bool writeUV, out bool writeColor)
	{
		result = default;
		glyphCount = 0;
		writeUV = false;
		writeColor = false;
		if (workECS == null || renderer == null || mCanvas == null)
		{
			return false;
		}
		ensureInit();
		// Runtime Text Dirty进入Canvas前，FastUIRenderElement的尺寸/Transform正式入口已经同步缓存。
		// 初始Dirty仍完整刷新；这里只避免批量Text内容/字体变化时重复读取RectTransform本地状态。
		if (!useCachedLocalState)
		{
			refreshLocalGeometryCache();
			syncLocalTransformCache();
		}
		ensureLayout();
		if (!mSimpleSingleLinePrimaryLayout || mUsingAtlasRunElements || mHasRichColor || mFont == null ||
			!mCanvas.tryGetPersistentTextGlyphRange(this, out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart, out int persistentGlyphCount) ||
			glyphData == null || persistentGlyphCount != mGlyphLayoutCount)
		{
			return false;
		}
		glyphCount = persistentGlyphCount;
		int glyphCapacity = mCanvas.getPersistentTextGlyphCapacity(this);
		int previousActiveVertexCount = renderer.getSimpleTextActiveVertexCount(slot);
		result = renderer.prepareSimpleTMPTextSlot(slot, glyphCount, glyphCapacity);
		writeUV = (flags & (FastUIDirtyFlags.Geometry | FastUIDirtyFlags.UV)) != 0 ||
			result.mVertexRangeChanged || result.mIndexCountChanged || result.mIndexCapacityChanged;
		writeColor = (flags & FastUIDirtyFlags.Color) != 0 || glyphCount * 4 > previousActiveVertexCount ||
			result.mVertexRangeChanged || result.mIndexCountChanged || result.mIndexCapacityChanged;
		getSimpleLayoutFontMetrics(out _, out float ascent, out float descent, out _, out _);
		float textHeight = Mathf.Max(ascent - descent, 0.0f);
		// Runtime Text Dirty的Rect变化入口与 LocalState缓存使用同一套同步规则。
		// CachedLocalState命中时继续复用mCachedRect，避免Alignment阶段重新跨Native边界读取RectTransform.rect。
		Rect rect = useCachedRectAlignment ? mCachedRect : mRectTransform.rect;
		float baseline = getBlockTop(rect, textHeight) - ascent;
		float lineStartX = getLineStartX(rect, 0);
		// Persistent Glyph保存的是相对PenX的局部Offset；这里把Text对齐产生的lineStart/baseline合并进Affine Translation，
		// 第二阶段因此不用再读取Text对象或重复做Alignment计算。
		Matrix4x4 geometryMatrix = getGeometryLocalToCanvasMatrix(canvasWorldToLocal, useCachedGeometryMatrix);
		float alignedM03 = geometryMatrix.m00 * lineStartX + geometryMatrix.m01 * baseline + geometryMatrix.m03;
		float alignedM13 = geometryMatrix.m10 * lineStartX + geometryMatrix.m11 * baseline + geometryMatrix.m13;
		float alignedM23 = geometryMatrix.m20 * lineStartX + geometryMatrix.m21 * baseline + geometryMatrix.m23;
		workECS.Add(new FastUITextBatchWorkData
		{
			mSlot = slot,
			mVertexStart = result.mVertexStart,
			mGlyphStart = glyphStart,
			mGlyphCount = glyphCount,
			mM00 = geometryMatrix.m00,
			mM01 = geometryMatrix.m01,
			mM03 = alignedM03,
			mM10 = geometryMatrix.m10,
			mM11 = geometryMatrix.m11,
			mM13 = alignedM13,
			mM20 = geometryMatrix.m20,
			mM21 = geometryMatrix.m21,
			mM23 = alignedM23,
			mColor = color,
			mWriteUV = writeUV ? 1 : 0,
			mWriteColor = writeColor ? 1 : 0,
		});
		return true;
	}
	private void addGlyph(FastUIGeometryBuilder builder, float penX, float baseline, TMP_Character character, TMP_FontAsset sourceFont,
	float glyphScale, float paddingPixels, Color32 richColor, bool translationOnly, Vector3 translation, Matrix4x4 geometryMatrix,
	float sdfScaleFactor)
	{
		Glyph glyph = character.glyph;
		GlyphMetrics metrics = glyph.metrics;
		GlyphRect glyphRect = glyph.glyphRect;
		paddingPixels = Mathf.Max(paddingPixels, 0.0f);
		float paddingPosition = paddingPixels * glyphScale;
		float left = penX + metrics.horizontalBearingX * glyphScale - paddingPosition;
		float top = baseline + metrics.horizontalBearingY * glyphScale + paddingPosition;
		float right = left + metrics.width * glyphScale + paddingPosition * 2.0f;
		float bottom = top - metrics.height * glyphScale - paddingPosition * 2.0f;
		float atlasWidth = Mathf.Max(sourceFont != null ? sourceFont.atlasWidth : mFont.atlasWidth, 1);
		float atlasHeight = Mathf.Max(sourceFont != null ? sourceFont.atlasHeight : mFont.atlasHeight, 1);
		float uvLeft = (glyphRect.x - paddingPixels) / atlasWidth;
		float uvBottom = (glyphRect.y - paddingPixels) / atlasHeight;
		float uvRight = (glyphRect.x + glyphRect.width + paddingPixels) / atlasWidth;
		float uvTop = (glyphRect.y + glyphRect.height + paddingPixels) / atlasHeight;
		float sdfScale = Mathf.Max(glyphScale * sdfScaleFactor, 0.000001f);
		Vector3 bottomLeft;
		Vector3 topLeft;
		Vector3 topRight;
		Vector3 bottomRight;
		if (translationOnly)
		{
			bottomLeft = new Vector3(left, bottom, 0.0f) + translation;
			topLeft = new Vector3(left, top, 0.0f) + translation;
			topRight = new Vector3(right, top, 0.0f) + translation;
			bottomRight = new Vector3(right, bottom, 0.0f) + translation;
		}
		else
		{
			bottomLeft = geometryMatrix.MultiplyPoint3x4(new Vector3(left, bottom, 0.0f));
			topLeft = geometryMatrix.MultiplyPoint3x4(new Vector3(left, top, 0.0f));
			topRight = geometryMatrix.MultiplyPoint3x4(new Vector3(right, top, 0.0f));
			bottomRight = geometryMatrix.MultiplyPoint3x4(new Vector3(right, bottom, 0.0f));
		}
		Vector4 bottomLeftUV0 = new(uvLeft, uvBottom, 0.0f, sdfScale);
		Vector4 topLeftUV0 = new(uvLeft, uvTop, 0.0f, sdfScale);
		Vector4 topRightUV0 = new(uvRight, uvTop, 0.0f, sdfScale);
		Vector4 bottomRightUV0 = new(uvRight, uvBottom, 0.0f, sdfScale);
		Vector2 bottomLeftUV2 = new(0.0f, 0.0f);
		Vector2 topLeftUV2 = new(0.0f, 1.0f);
		Vector2 topRightUV2 = new(1.0f, 1.0f);
		Vector2 bottomRightUV2 = new(1.0f, 0.0f);
		if (mHasRichColor)
		{
			builder.AddTMPQuad(bottomLeft, topLeft, topRight, bottomRight, bottomLeftUV0, topLeftUV0, topRightUV0, bottomRightUV0,
				bottomLeftUV2, topLeftUV2, topRightUV2, bottomRightUV2, richColor);
		}
		else
		{
			builder.AddTMPQuad(bottomLeft, topLeft, topRight, bottomRight, bottomLeftUV0, topLeftUV0, topRightUV0, bottomRightUV0,
				bottomLeftUV2, topLeftUV2, topRightUV2, bottomRightUV2);
		}
	}
	private float getSDFScaleFactor()
	{
		float textScaleY = mRectTransform != null ? Mathf.Abs(mRectTransform.lossyScale.y) : 1.0f;
		float canvasScaleY = mCanvas != null ? Mathf.Abs(mCanvas.transform.lossyScale.y) : 1.0f;
		if (canvasScaleY <= 0.000001f)
		{
			canvasScaleY = 1.0f;
		}
		return textScaleY / canvasScaleY;
	}
	private int getCodePoint(string text, int index, out uint unicode)
	{
		char first = text[index];
		if (char.IsHighSurrogate(first) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
		{
			unicode = (uint)char.ConvertToUtf32(first, text[index + 1]);
			return 2;
		}
		unicode = first;
		return 1;
	}
	private void ensureColorStackCapacity(int required)
	{
		if (mColorStack != null && required <= mColorStack.Length)
		{
			return;
		}
		System.Array.Resize(ref mColorStack, Mathf.NextPowerOfTwo(Mathf.Max(required, 8)));
	}
	private bool tryParseColorOpenTag(string text, int index, out Color32 color, out int tagLength)
	{
		color = default;
		tagLength = 0;
		const string prefix = "<color=#";
		if (!matchLiteralIgnoreCase(text, index, prefix))
		{
			return false;
		}
		int colorStart = index + prefix.Length;
		int end = colorStart;
		int maxEnd = Mathf.Min(text.Length, colorStart + 9);
		while (end < maxEnd && text[end] != '>')
		{
			++end;
		}
		if (end >= text.Length || text[end] != '>' || !tryParseHexColor(text, colorStart, end - colorStart, out color))
		{
			return false;
		}
		tagLength = end - index + 1;
		return true;
	}
	private bool tryParseColorCloseTag(string text, int index, out int tagLength)
	{
		const string tag = "</color>";
		if (!matchLiteralIgnoreCase(text, index, tag))
		{
			tagLength = 0;
			return false;
		}
		tagLength = tag.Length;
		return true;
	}
	private bool matchLiteralIgnoreCase(string text, int index, string literal)
	{
		if (index < 0 || index + literal.Length > text.Length)
		{
			return false;
		}
		for (int i = 0; i < literal.Length; ++i)
		{
			char a = text[index + i];
			char b = literal[i];
			if (a == b)
			{
				continue;
			}
			if (a >= 'A' && a <= 'Z')
			{
				a = (char)(a + ('a' - 'A'));
			}
			if (b >= 'A' && b <= 'Z')
			{
				b = (char)(b + ('a' - 'A'));
			}
			if (a != b)
			{
				return false;
			}
		}
		return true;
	}
	private bool tryParseHexColor(string text, int start, int length, out Color32 color)
	{
		color = default;
		if (length != 3 && length != 4 && length != 6 && length != 8)
		{
			return false;
		}
		if (length <= 4)
		{
			if (!tryGetHex(text[start], out int r) || !tryGetHex(text[start + 1], out int g) || !tryGetHex(text[start + 2], out int b))
			{
				return false;
			}
			int a = 15;
			if (length == 4 && !tryGetHex(text[start + 3], out a))
			{
				return false;
			}
			color = new Color32((byte)(r * 17), (byte)(g * 17), (byte)(b * 17), (byte)(a * 17));
			return true;
		}
		if (!tryGetHexByte(text, start, out byte red) || !tryGetHexByte(text, start + 2, out byte green) ||
			!tryGetHexByte(text, start + 4, out byte blue))
		{
			return false;
		}
		byte alpha = 255;
		if (length == 8 && !tryGetHexByte(text, start + 6, out alpha))
		{
			return false;
		}
		color = new Color32(red, green, blue, alpha);
		return true;
	}
	private bool tryGetHexByte(string text, int index, out byte value)
	{
		value = 0;
		if (!tryGetHex(text[index], out int high) || !tryGetHex(text[index + 1], out int low))
		{
			return false;
		}
		value = (byte)((high << 4) | low);
		return true;
	}
	private bool tryGetHex(char value, out int result)
	{
		if (value >= '0' && value <= '9')
		{
			result = value - '0';
			return true;
		}
		if (value >= 'a' && value <= 'f')
		{
			result = value - 'a' + 10;
			return true;
		}
		if (value >= 'A' && value <= 'F')
		{
			result = value - 'A' + 10;
			return true;
		}
		result = 0;
		return false;
	}
	private bool tryGetAtlasTexture(TMP_FontAsset font, int atlasIndex, out Texture texture)
	{
		texture = null;
		if (font == null || atlasIndex < 0)
		{
			return false;
		}
		Texture2D[] atlases = font.atlasTextures;
		if (atlases != null && atlasIndex < atlases.Length)
		{
			texture = atlases[atlasIndex];
		}
		if (texture == null && atlasIndex == 0)
		{
			texture = font.atlasTexture;
		}
		return texture != null;
	}
	private Material getRunBaseMaterial(TMP_FontAsset font)
	{
		if (font == null)
		{
			return null;
		}
		return font == mFont ? mMaterial : font.material;
	}
	private bool shouldUseAtlasRunElements()
	{
		if (mRenderRunCount <= 0)
		{
			return false;
		}
		if (mRenderRunCount != 1)
		{
			return true;
		}
		RenderRunLayout run = mFirstRun;
		return run.mFont != mFont || run.mAtlasIndex != 0 || run.mBaseMaterial != mMaterial || run.mTexture != mRenderTexture;
	}
	private bool shouldSyncRenderRunsImmediately()
	{
		return (mAtlasRunElements != null && mAtlasRunElements.Count > 0) || fontMayUseMultipleRuns();
	}
	private bool canDeferPrimaryAtlasRunSyncForCurrentText()
	{
		return sPrimaryAtlasTextDeferredRunSyncEnabled && canUsePrimaryAtlasDeferredRunSync(mText);
	}
	private bool canUsePrimaryAtlasDeferredRunSync(string text)
	{
		if (mFont == null || mUsingAtlasRunElements || (mAtlasRunElements != null && mAtlasRunElements.Count > 0) || mRenderRunCount != 1)
		{
			return false;
		}
		RenderRunLayout run = mFirstRun;
		if (run.mFont != mFont || run.mAtlasIndex != 0 || run.mBaseMaterial != mMaterial || run.mTexture != mRenderTexture)
		{
			return false;
		}
		text ??= string.Empty;
		if (sPrimaryAtlasAsciiMaskFastCheckEnabled)
		{
			for (int i = 0; i < text.Length; ++i)
			{
				char value = text[i];
				if ((mRichText && value == '<') || value >= 128)
				{
					return false;
				}
				ulong bit = 1UL << (value & 63);
				ulong mask = value < 64 ? mPrimaryAtlasAsciiMaskLow : mPrimaryAtlasAsciiMaskHigh;
				if ((mask & bit) == 0UL)
				{
					return false;
				}
			}
			return true;
		}
		for (int i = 0; i < text.Length; ++i)
		{
			char value = text[i];
			if ((mRichText && value == '<') || value >= mAsciiCharacterCache.Length)
			{
				return false;
			}
			// CR/LF/TAB不产生RenderRun Glyph，本身不会切换Atlas。
			if (value == '\r' || value == '\n' || value == '\t')
			{
				continue;
			}
			TMP_Character character = mAsciiCharacterCache[value];
			if (character == null || character.glyph == null || character.glyph.atlasIndex != 0)
			{
				return false;
			}
		}
		return true;
	}
	private bool fontMayUseMultipleRuns()
	{
		if (mFont == null)
		{
			return false;
		}
		Texture2D[] atlases = mFont.atlasTextures;
		if (atlases != null && atlases.Length > 1)
		{
			return true;
		}
		var fallbackTable = mFont.fallbackFontAssetTable;
		return fallbackTable != null && fallbackTable.Count > 0;
	}
	private void syncRenderRunElements()
	{
		int desiredCount = mUsingAtlasRunElements && mCanvas != null ? mRenderRunCount : 0;
		if (desiredCount <= 0)
		{
			destroyRenderRunElements();
			return;
		}
		ensureRunRoot();
		mAtlasRunElements ??= new List<FastTextAtlasRun>(Mathf.Max(desiredCount, 1));
		while (mAtlasRunElements.Count < desiredCount)
		{
			int index = mAtlasRunElements.Count;
			GameObject runObject = new("__FastTextRun" + index, typeof(RectTransform))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			RectTransform runTransform = (RectTransform)runObject.transform;
			runTransform.SetParent(mAtlasRunRoot, false);
			setupStretchRect(runTransform);
			FastTextAtlasRun run = runObject.AddComponent<FastTextAtlasRun>();
			mAtlasRunElements.Add(run);
		}
		while (mAtlasRunElements.Count > desiredCount)
		{
			int last = mAtlasRunElements.Count - 1;
			FastTextAtlasRun run = mAtlasRunElements[last];
			mAtlasRunElements.RemoveAt(last);
			if (run != null)
			{
				run.releaseRenderSource();
				run.gameObject.SetActive(false);
				destroyObject(run.gameObject);
			}
		}
		for (int i = 0; i < desiredCount; ++i)
		{
			FastTextAtlasRun run = mAtlasRunElements[i];
			if (run == null)
			{
				continue;
			}
			run.gameObject.name = "__FastTextRun" + i;
			run.setOwnerAndRun(this, i);
			RenderRunLayout runLayout = getRenderRunLayout(i);
			run.setRenderSource(runLayout.mBaseMaterial, runLayout.mTexture);
			run.syncOwnerColor(mColor);
			run.cull(mCull);
			run.markRunGeometryDirty();
		}
	}
	private void ensureRunRoot()
	{
		if (mAtlasRunRoot != null)
		{
			return;
		}
		GameObject rootObject = new("__FastTextRuns", typeof(RectTransform))
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		mAtlasRunRoot = (RectTransform)rootObject.transform;
		mAtlasRunRoot.SetParent(mRectTransform, false);
		mAtlasRunRoot.SetSiblingIndex(0);
		setupStretchRect(mAtlasRunRoot);
	}
	private void setupStretchRect(RectTransform rectTransform)
	{
		Vector2 pivot = mRectTransform != null ? mRectTransform.pivot : new Vector2(0.5f, 0.5f);
		Vector2 size = mRectTransform != null ? mRectTransform.rect.size : Vector2.zero;
		rectTransform.anchorMin = pivot;
		rectTransform.anchorMax = pivot;
		rectTransform.pivot = pivot;
		rectTransform.sizeDelta = size;
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.localPosition = Vector3.zero;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.localScale = Vector3.one;
	}
	private void syncRunRectTransforms()
	{
		if (mAtlasRunRoot != null)
		{
			setupStretchRect(mAtlasRunRoot);
		}
		if (mAtlasRunElements == null)
		{
			return;
		}
		for (int i = 0; i < mAtlasRunElements.Count; ++i)
		{
			FastTextAtlasRun run = mAtlasRunElements[i];
			if (run != null)
			{
				setupStretchRect(run.getRectTransform());
			}
		}
	}
	private void destroyRenderRunElements()
	{
		if (mAtlasRunElements != null)
		{
			for (int i = 0; i < mAtlasRunElements.Count; ++i)
			{
				FastTextAtlasRun run = mAtlasRunElements[i];
				if (run != null)
				{
					run.releaseRenderSource();
					run.gameObject.SetActive(false);
				}
			}
			mAtlasRunElements.Clear();
		}
		if (mAtlasRunRoot != null)
		{
			GameObject root = mAtlasRunRoot.gameObject;
			mAtlasRunRoot = null;
			root.SetActive(false);
			destroyObject(root);
		}
	}
	private void destroyObject(Object target)
	{
		if (target == null)
		{
			return;
		}
		if (Application.isPlaying)
		{
			Object.Destroy(target);
		}
		else
		{
			Object.DestroyImmediate(target);
		}
	}
	private void syncRunColors()
	{
		if (mAtlasRunElements == null)
		{
			return;
		}
		for (int i = 0; i < mAtlasRunElements.Count; ++i)
		{
			FastTextAtlasRun run = mAtlasRunElements[i];
			if (run != null)
			{
				run.syncOwnerColor(mColor);
			}
		}
	}
	private void markRunGeometryDirty()
	{
		if (mAtlasRunElements == null)
		{
			return;
		}
		for (int i = 0; i < mAtlasRunElements.Count; ++i)
		{
			FastTextAtlasRun run = mAtlasRunElements[i];
			if (run != null)
			{
				run.markRunGeometryDirty();
			}
		}
	}
	private void refreshMaterialVariants()
	{
		FastUITextMaterialVariantCache.refresh(mMaterial);
		for (int i = 0; i < mRenderRunCount; ++i)
		{
			Material baseMaterial = getRenderRunLayout(i).mBaseMaterial;
			if (baseMaterial != null && baseMaterial != mMaterial)
			{
				FastUITextMaterialVariantCache.refresh(baseMaterial);
			}
		}
	}
	private void syncFontRenderData(bool notify)
	{
		Material fontMaterial = mFont != null ? mFont.material : null;
		if (!mMaterialOverride || mMaterial == null || mMaterial == mLastFontMaterial)
		{
			mMaterialOverride = false;
			mMaterial = fontMaterial;
		}
		Texture texture = getMaterialTexture(mMaterial);
		if (texture == null && mFont != null)
		{
			texture = mFont.atlasTexture;
		}
		bool batchChanged = mRenderTexture != texture || mLastFontMaterial != fontMaterial;
		mRenderTexture = texture;
		mLastFontMaterial = fontMaterial;
		refreshMaterialPadding();
		if (notify && batchChanged && mCanvas != null)
		{
			mCanvas.notifyElementBatchChanged(this);
		}
	}
	private Texture getMaterialTexture(Material material)
	{
		return material != null ? material.mainTexture : null;
	}
	private void refreshMaterialPadding()
	{
		mMaterialPadding = mMaterial != null ? Mathf.Max(ShaderUtilities.GetPadding(mMaterial, false, false), 0.0f) : 0.0f;
	}
}
