using System.Collections.Generic;
using UnityEngine;

public static class FastUIMaskMaterialCache
{
	private struct Key : System.IEquatable<Key>
	{
		public int mSourceID;
		public FastUIMaskState mState;
		public Key(Material source, FastUIMaskState state)
		{
			mSourceID = FastUnityObjectIDUtility.getLegacyIntID(source);
			mState = state;
		}
		public override int GetHashCode()
		{
			unchecked
			{
				return mSourceID * 397 ^ mState.GetHashCode();
			}
		}
		public readonly bool Equals(Key other)
		{
			return mSourceID == other.mSourceID && mState == other.mState;
		}
		public override readonly bool Equals(object obj)
		{
			return obj is Key other && Equals(other);
		}
	}
	private sealed class Entry
	{
		public Material mSource;
		public Material mMaterial;
		public int mReferenceCount;
	}
	private static readonly FastDictionary<Key, Entry> mEntries = new();
	private static readonly HashSet<int> mUnsupportedShaderIDs = new();
	private static Material mUGUIDefaultMaterial;
	private static bool mUGUIDefaultMaterialErrorReported;
	private static Material mLastSource;
	private static FastUIMaskState mLastState;
	private static Entry mLastEntry;
	private static readonly int STENCIL_ID = Shader.PropertyToID("_Stencil");
	private static readonly int STENCIL_COMP_ID = Shader.PropertyToID("_StencilComp");
	private static readonly int STENCIL_OP_ID = Shader.PropertyToID("_StencilOp");
	private static readonly int STENCIL_WRITE_MASK_ID = Shader.PropertyToID("_StencilWriteMask");
	private static readonly int STENCIL_READ_MASK_ID = Shader.PropertyToID("_StencilReadMask");
	private static readonly int COLOR_MASK_ID = Shader.PropertyToID("_ColorMask");
	private static readonly int USE_ALPHA_CLIP_ID = Shader.PropertyToID("_UseUIAlphaClip");
	public static Material acquire(Material source, FastUIMaskState state)
	{
		if (source == null || state.mMode == FastUIMaskMaterialMode.None)
		{
			return source;
		}
		if (ReferenceEquals(source, mLastSource) && state == mLastState && mLastEntry != null && mLastEntry.mMaterial != null)
		{
			++mLastEntry.mReferenceCount;
			return mLastEntry.mMaterial;
		}
		Key key = new(source, state);
		if (mEntries.TryGetValue(key, out Entry entry))
		{
			++entry.mReferenceCount;
			mLastSource = source;
			mLastState = state;
			mLastEntry = entry;
			return entry.mMaterial;
		}
		Material material = createMaterial(source, state);
		if (material == null)
		{
			return source;
		}
		entry = new Entry
		{
			mSource = source,
			mMaterial = material,
			mReferenceCount = 1,
		};
		mEntries.Add(key, entry);
		mLastSource = source;
		mLastState = state;
		mLastEntry = entry;
		return material;
	}
	public static void release(Material source, FastUIMaskState state, Material runtimeMaterial)
	{
		if (source == null || runtimeMaterial == null || runtimeMaterial == source || state.mMode == FastUIMaskMaterialMode.None)
		{
			return;
		}
		Key key = new(source, state);
		if (!mEntries.TryGetValue(key, out Entry entry) || entry.mMaterial != runtimeMaterial)
		{
			return;
		}
		--entry.mReferenceCount;
		if (entry.mReferenceCount > 0)
		{
			return;
		}
		mEntries.Remove(key);
		if (ReferenceEquals(entry, mLastEntry))
		{
			mLastSource = null;
			mLastState = default;
			mLastEntry = null;
		}
		destroyMaterial(entry.mMaterial);
	}
	public static void refresh(Material source)
	{
		if (source == null || mEntries.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<Key, Entry> pair in mEntries)
		{
			Entry entry = pair.Value;
			if (entry.mSource != source || entry.mMaterial == null)
			{
				continue;
			}
			copySourceProperties(source, entry.mMaterial);
			applyState(entry.mMaterial, pair.Key.mState);
		}
	}
	public static int getCachedMaterialCount()
	{
		return mEntries.Count;
	}
	public static bool isMaterialSupported(Material source)
	{
		if (source == null)
		{
			return false;
		}
		if (isSpriteDefault(source))
		{
			return getUGUIDefaultMaterial() != null;
		}
		return hasStencilProperties(source);
	}
	private static Material createMaterial(Material source, FastUIMaskState state)
	{
		bool useUGUIDefault = isSpriteDefault(source);
		Material template = useUGUIDefault ? getUGUIDefaultMaterial() : source;
		if (template == null)
		{
			reportUnsupported(source);
			return null;
		}
		if (!hasStencilProperties(template))
		{
			reportUnsupported(source);
			return null;
		}
		Material material = new(template);
		if (useUGUIDefault)
		{
			copySourceProperties(source, material);
		}
		material.name = source.name + " [FastMask " + state.mMode + " D" + state.mDepth + "]";
		material.hideFlags = HideFlags.HideAndDontSave;
		applyState(material, state);
		return material;
	}
	private static void copySourceProperties(Material source, Material target)
	{
		if (source == null || target == null)
		{
			return;
		}
		target.CopyPropertiesFromMaterial(source);
	}
	private static void applyState(Material material, FastUIMaskState state)
	{
		if (material == null)
		{
			return;
		}
		material.SetFloat(STENCIL_ID, state.mStencilRef);
		material.SetFloat(STENCIL_COMP_ID, (int)state.mCompareFunction);
		material.SetFloat(STENCIL_OP_ID, (int)state.mStencilOp);
		material.SetFloat(STENCIL_WRITE_MASK_ID, state.mWriteMask);
		material.SetFloat(STENCIL_READ_MASK_ID, state.mReadMask);
		material.SetFloat(COLOR_MASK_ID, state.mColorMask);
		if (material.HasProperty(USE_ALPHA_CLIP_ID))
		{
			material.SetFloat(USE_ALPHA_CLIP_ID, state.mUseAlphaClip ? 1.0f : 0.0f);
		}
	}
	private static bool hasStencilProperties(Material material)
	{
		return material != null && material.HasProperty(STENCIL_ID) && material.HasProperty(STENCIL_COMP_ID) &&
			material.HasProperty(STENCIL_OP_ID) && material.HasProperty(STENCIL_WRITE_MASK_ID) &&
			material.HasProperty(STENCIL_READ_MASK_ID) && material.HasProperty(COLOR_MASK_ID);
	}
	private static bool isSpriteDefault(Material source)
	{
		return source != null && source.shader != null && source.shader.name == "Sprites/Default";
	}
	private static Material getUGUIDefaultMaterial()
	{
		if (mUGUIDefaultMaterial != null)
		{
			return mUGUIDefaultMaterial;
		}
		Material material = Canvas.GetDefaultCanvasMaterial();
		if (material != null && hasStencilProperties(material))
		{
			mUGUIDefaultMaterial = material;
			return mUGUIDefaultMaterial;
		}
		if (!mUGUIDefaultMaterialErrorReported)
		{
			mUGUIDefaultMaterialErrorReported = true;
			string shaderName = material != null && material.shader != null ? material.shader.name : "null";
			Debug.LogError("[FastMask] UGUI默认材质不支持Stencil属性,Shader:" + shaderName +
				". FastMask默认图片Mask需要UGUI默认材质提供_Stencil/_StencilComp/_StencilOp/_StencilWriteMask/_StencilReadMask/_ColorMask.");
		}
		return null;
	}
	private static void reportUnsupported(Material source)
	{
		if (source == null || source.shader == null)
		{
			return;
		}
		int shaderID = FastUnityObjectIDUtility.getLegacyIntID(source.shader);
		if (!mUnsupportedShaderIDs.Add(shaderID))
		{
			return;
		}
		Debug.LogError("[FastMask] Material Shader不支持Stencil属性:" + source.shader.name +
			". 自定义Shader需要提供_Stencil/_StencilComp/_StencilOp/_StencilWriteMask/_StencilReadMask/_ColorMask.");
	}
	private static void destroyMaterial(Material material)
	{
		if (material == null)
		{
			return;
		}
		if (Application.isPlaying)
		{
			Object.Destroy(material);
		}
		else
		{
			Object.DestroyImmediate(material);
		}
	}
}
