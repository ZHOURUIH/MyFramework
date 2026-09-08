using System.Collections.Generic;
using UnityEngine;

// FastText纯SDF文字的GPU轻量Material缓存。
// 仅对没有Outline/Underlay/Glow等额外效果的标准TMP SDF材质启用FastUI/TMP SDF；
// 其他材质保持原Shader，确保自定义文字效果和已有项目行为不被性能快路径改变。
public static class FastUITextRenderMaterialCache
{
	private struct Key : System.IEquatable<Key>
	{
		public int mSourceID;
		public int mTextureID;
		public Key(Material source, Texture texture)
		{
			mSourceID = FastUnityObjectIDUtility.getLegacyIntID(source);
			mTextureID = FastUnityObjectIDUtility.getLegacyIntID(texture);
		}
		public override readonly int GetHashCode()
		{
			unchecked
			{
				return mSourceID * 397 ^ mTextureID;
			}
		}
		public readonly bool Equals(Key other)
		{
			return mSourceID == other.mSourceID && mTextureID == other.mTextureID;
		}
		public override readonly bool Equals(object obj)
		{
			return obj is Key other && Equals(other);
		}
	}
	private sealed class Entry
	{
		public Material mSource;
		public Texture mTexture;
		public Material mRuntime;
	}
	private static readonly FastDictionary<Key, Entry> mEntries = new();
	private static readonly HashSet<int> mUnsupportedSourceIDs = new();
	private static readonly List<Key> mRemoveKeys = new();
	private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
	private static readonly int FACE_COLOR_ID = Shader.PropertyToID("_FaceColor");
	private static readonly int FACE_DILATE_ID = Shader.PropertyToID("_FaceDilate");
	private static readonly int OUTLINE_COLOR_ID = Shader.PropertyToID("_OutlineColor");
	private static readonly int OUTLINE_WIDTH_ID = Shader.PropertyToID("_OutlineWidth");
	private static readonly int OUTLINE_SOFTNESS_ID = Shader.PropertyToID("_OutlineSoftness");
	private static readonly int WEIGHT_NORMAL_ID = Shader.PropertyToID("_WeightNormal");
	private static readonly int SCALE_RATIO_A_ID = Shader.PropertyToID("_ScaleRatioA");
	private static readonly int GRADIENT_SCALE_ID = Shader.PropertyToID("_GradientScale");
	private static readonly int FACE_TEX_ID = Shader.PropertyToID("_FaceTex");
	private static readonly int OUTLINE_TEX_ID = Shader.PropertyToID("_OutlineTex");
	private static Shader mShader;
	private static bool mShaderErrorLogged;
	private static Material mLastSource;
	private static Texture mLastTexture;
	private static Material mLastRuntime;
	public static Material get(Material source, Texture texture)
	{
		if (source == null)
		{
			return null;
		}
		if (ReferenceEquals(source, mLastSource) && ReferenceEquals(texture, mLastTexture) && mLastRuntime != null)
		{
			return mLastRuntime;
		}
		if (isCompatibilityMaterial(source))
		{
			mLastSource = source;
			mLastTexture = texture;
			mLastRuntime = source;
			return source;
		}
		if (texture == null)
		{
			texture = source.mainTexture;
		}
		if (ReferenceEquals(source, mLastSource) && ReferenceEquals(texture, mLastTexture) && mLastRuntime != null)
		{
			return mLastRuntime;
		}
		Key key = new(source, texture);
		if (mEntries.TryGetValue(key, out Entry entry) && entry.mSource == source && entry.mTexture == texture && entry.mRuntime != null)
		{
			mLastSource = source;
			mLastTexture = texture;
			mLastRuntime = entry.mRuntime;
			return entry.mRuntime;
		}
		if (!canUseFastPath(source))
		{
			mLastSource = source;
			mLastTexture = texture;
			mLastRuntime = source;
			return source;
		}
		Shader shader = getShader();
		if (shader == null)
		{
			mLastSource = source;
			mLastTexture = texture;
			mLastRuntime = source;
			return source;
		}
		Material runtime = new(shader)
		{
			name = source.name + " [FastUI Text GPU]",
			hideFlags = HideFlags.HideAndDontSave,
		};
		copyProperties(source, texture, runtime);
		mEntries[key] = new Entry
		{
			mSource = source,
			mTexture = texture,
			mRuntime = runtime,
		};
		mLastSource = source;
		mLastTexture = texture;
		mLastRuntime = runtime;
		return runtime;
	}
	public static void refresh(Material source)
	{
		if (ReferenceEquals(source, mLastSource))
		{
			mLastSource = null;
			mLastTexture = null;
			mLastRuntime = null;
		}
		if (source == null || mEntries.Count == 0)
		{
			return;
		}
		bool supported = canUseFastPath(source, true);
		mRemoveKeys.Clear();
		foreach (KeyValuePair<Key, Entry> pair in mEntries)
		{
			Entry entry = pair.Value;
			if (entry.mSource != source)
			{
				continue;
			}
			if (!supported || entry.mRuntime == null)
			{
				mRemoveKeys.Add(pair.Key);
				destroyMaterial(entry.mRuntime);
				continue;
			}
			copyProperties(source, entry.mTexture, entry.mRuntime);
			FastUIMaskMaterialCache.refresh(entry.mRuntime);
		}
		for (int i = 0; i < mRemoveKeys.Count; ++i)
		{
			mEntries.Remove(mRemoveKeys[i]);
		}
		mRemoveKeys.Clear();
	}
	public static bool isCompatibilityMaterial(Material material)
	{
		return material != null && material.shader != null && material.shader.name == "FastUI/TMP SDF";
	}
	public static int getCachedMaterialCount()
	{
		return mEntries.Count;
	}
	private static bool canUseFastPath(Material source, bool forceRefresh = false)
	{
		if (source == null || source.shader == null)
		{
			return false;
		}
		int sourceID = FastUnityObjectIDUtility.getLegacyIntID(source);
		if (!forceRefresh && mUnsupportedSourceIDs.Contains(sourceID))
		{
			return false;
		}
		string shaderName = source.shader.name;
		if (shaderName != "TextMeshPro/Distance Field" && shaderName != "TextMeshPro/Mobile/Distance Field")
		{
			mUnsupportedSourceIDs.Add(sourceID);
			return false;
		}
		string[] keywords = source.shaderKeywords;
		if (keywords != null)
		{
			for (int i = 0; i < keywords.Length; ++i)
			{
				string keyword = keywords[i];
				if (string.IsNullOrEmpty(keyword) || keyword == "OUTLINE_ON")
				{
					continue;
				}
				// Underlay / Glow / Bevel and other TMP feature variants still need
				// their dedicated FastUI implementation before they can use this shader.
				mUnsupportedSourceIDs.Add(sourceID);
				return false;
			}
		}
		// Standard TMP Outline presets use only scalar/color properties and are now
		// supported by FastUI/TMP SDF. Texture-driven face/outline effects remain unsupported.
		if (hasTexture(source, FACE_TEX_ID) || hasTexture(source, OUTLINE_TEX_ID))
		{
			mUnsupportedSourceIDs.Add(sourceID);
			return false;
		}
		mUnsupportedSourceIDs.Remove(sourceID);
		return true;
	}
	private static bool hasTexture(Material material, int id)
	{
		return material.HasProperty(id) && material.GetTexture(id) != null;
	}
	private static Shader getShader()
	{
		if (mShader != null)
		{
			return mShader;
		}
		mShader = Resources.Load<Shader>("FastUITMPSDF");
		if (mShader == null)
		{
			mShader = Shader.Find("FastUI/TMP SDF");
		}
		if (mShader == null && !mShaderErrorLogged)
		{
			mShaderErrorLogged = true;
			Debug.LogError("[FastUI] 找不到FastUITMPSDF.shader,FastText将回退源Material.");
		}
		return mShader;
	}
	private static void copyProperties(Material source, Texture texture, Material target)
	{
		if (source == null || target == null)
		{
			return;
		}
		if (target.HasProperty(MAIN_TEX_ID))
		{
			target.SetTexture(MAIN_TEX_ID, texture != null ? texture : source.mainTexture);
		}
		copyColor(source, target, FACE_COLOR_ID);
		copyColor(source, target, OUTLINE_COLOR_ID);
		copyFloat(source, target, FACE_DILATE_ID);
		copyFloat(source, target, OUTLINE_WIDTH_ID);
		copyFloat(source, target, OUTLINE_SOFTNESS_ID);
		copyFloat(source, target, WEIGHT_NORMAL_ID);
		copyFloat(source, target, SCALE_RATIO_A_ID);
		copyFloat(source, target, GRADIENT_SCALE_ID);
	}
	private static void copyFloat(Material source, Material target, int id)
	{
		if (source.HasProperty(id) && target.HasProperty(id))
		{
			target.SetFloat(id, source.GetFloat(id));
		}
	}
	private static void copyColor(Material source, Material target, int id)
	{
		if (source.HasProperty(id) && target.HasProperty(id))
		{
			target.SetColor(id, source.GetColor(id));
		}
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
