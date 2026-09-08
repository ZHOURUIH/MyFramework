using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EasyECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteGPUDrivenVertex
{
	public Vector3 mPosition;
	public Vector2 mUV;

	public FastSpriteGPUDrivenVertex(Vector3 position, Vector2 uv)
	{
		mPosition = position;
		mUV = uv;
	}
}

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteGPUDrivenQuadAsset
{
	public Vector4 mPosition01;
	public Vector4 mPosition23;
	public Vector4 mUV01;
	public Vector4 mUV23;
	public uint mPackedIndices;
	public uint mPadding0;
	public uint mPadding1;
	public uint mPadding2;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteGPUMatrixPatch
{
	public int mSlot;
	public int mPadding0;
	public int mPadding1;
	public int mPadding2;
	public Matrix4x4 mValue;

	public FastSpriteGPUMatrixPatch(int slot, Matrix4x4 value)
	{
		mSlot = slot;
		mPadding0 = 0;
		mPadding1 = 0;
		mPadding2 = 0;
		mValue = value;
	}
}

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteGPUColorPatch
{
	public int mSlot;
	public int mPadding0;
	public int mPadding1;
	public int mPadding2;
	public Vector4 mValue;

	public FastSpriteGPUColorPatch(int slot, Vector4 value)
	{
		mSlot = slot;
		mPadding0 = 0;
		mPadding1 = 0;
		mPadding2 = 0;
		mValue = value;
	}
}

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteGPUScalarPatch
{
	public int mSlot;
	public int mValue;

	public FastSpriteGPUScalarPatch(int slot, int value)
	{
		mSlot = slot;
		mValue = value;
	}
}

internal readonly struct FastSpriteGPURenderStateKey : IEquatable<FastSpriteGPURenderStateKey>
{
	public readonly int mMaterialID;
	public readonly int mTextureID;
	public readonly int mVertexCount;

	public FastSpriteGPURenderStateKey(int materialID, int textureID, int vertexCount)
	{
		mMaterialID = materialID;
		mTextureID = textureID;
		mVertexCount = vertexCount;
	}

	public bool Equals(FastSpriteGPURenderStateKey other)
	{
		return mMaterialID == other.mMaterialID && mTextureID == other.mTextureID && mVertexCount == other.mVertexCount;
	}

	public override bool Equals(object obj)
	{
		return obj is FastSpriteGPURenderStateKey other && Equals(other);
	}
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = mMaterialID;
			hash = (hash * 397) ^ mTextureID;
			hash = (hash * 397) ^ mVertexCount;
			return hash;
		}
	}
}

internal struct FastSpriteGPUPlanCachedRun
{
	public int mStart;
	public int mCount;
	public int mRenderStateID;
	public int mFirstSlot;
}

internal sealed class FastSpriteGPUPlanCachedSpan
{
	public int mLayoutRevision;
	public int mBuildStamp;
	// Global order offset used to reuse an unchanged retained span without copying slots.
	public int mLastOrderStart = -1;
	public int mSlotCount;
	public int mRunCount;
	public int[] mSlots = Array.Empty<int>();
	public FastSpriteGPUPlanCachedRun[] mRuns = Array.Empty<FastSpriteGPUPlanCachedRun>();

	public void beginBuild(int sourceCount, int layoutRevision, int buildStamp)
	{
		mLayoutRevision = layoutRevision;
		mBuildStamp = buildStamp;
		mSlotCount = 0;
		mRunCount = 0;
		if (mSlots.Length < sourceCount)
		{
			Array.Resize(ref mSlots, Mathf.NextPowerOfTwo(Mathf.Max(sourceCount, 8)));
		}
	}

	public void appendSlot(int slot, int renderStateID)
	{
		int slotIndex = mSlotCount++;
		mSlots[slotIndex] = slot;
		if (mRunCount > 0)
		{
			ref FastSpriteGPUPlanCachedRun previous = ref mRuns[mRunCount - 1];
			if (previous.mRenderStateID == renderStateID)
			{
				++previous.mCount;
				return;
			}
		}
		if (mRuns.Length <= mRunCount)
		{
			Array.Resize(ref mRuns, Mathf.NextPowerOfTwo(Mathf.Max(mRunCount + 1, 4)));
		}
		mRuns[mRunCount++] = new FastSpriteGPUPlanCachedRun
		{
			mStart = slotIndex,
			mCount = 1,
			mRenderStateID = renderStateID,
			mFirstSlot = slot,
		};
	}
}

// Stable-slot GPU backend: retained CPU order, sparse instance updates and indirect draws.
internal sealed class FastSpriteGPUDrivenBackend : IDisposable
{
	internal struct FrameStats
	{
		public double mDirtyBuildMS;
		public double mUploadMS;
		public double mCommandBuildMS;
		public int mDrawCount;
		public int mInstanceCount;
		public int mDirtyRendererCount;
		public int mHotWriteCount;
		public int mSpriteAssetCommitCount;
		public int mGeometryUploadBytes;
		public int mInstanceUploadBytes;
		public int mOrderUploadBytes;
		public int mGeometryUploadCalls;
		public int mInstanceUploadCalls;
		public int mOrderUploadCalls;
		public int mDirtyDensityChunkDispatchCount;
	}

	private const int VERTEX_STRIDE = 20;
	private const int QUAD_ASSET_STRIDE = 80;
	private const int MATRIX_STRIDE = 64;
	private const int COLOR_STRIDE = 16;
	private const int INT_STRIDE = 4;
	private const int ORDER_STRIDE = 4;
	private const int INDIRECT_ARG_STRIDE = 4;
	private const int INDIRECT_ARGS_PER_DRAW = 4;
	private const int INDIRECT_ARGS_BYTES_PER_DRAW = INDIRECT_ARG_STRIDE * INDIRECT_ARGS_PER_DRAW;
	private const int VIRTUAL_TEXTURE_SLOT_COUNT = 8;
	private const int INSTANCE_TEXTURE_SLOT_SHIFT = 8;
	private const int INSTANCE_TEXTURE_SLOT_FALLBACK = 15;
	private const int MATRIX_PATCH_STRIDE = 80;
	private const int COLOR_PATCH_STRIDE = 32;
	private const int SCALAR_PATCH_STRIDE = 8;
	private const int SCATTER_COUNT_STRIDE = 4;
	private const int SCATTER_THREAD_GROUP_SIZE = 64;
	private const int SCATTER_MIN_SAVED_BYTES = 1024;
	private const int DIRTY_DENSITY_CHUNK_MIN_COUNT = 12288;
	private const int DIRTY_DENSITY_CHUNK_SIZE = 2048;
	private const int MIN_GEOMETRY_BLOCK = 8;
	private const int QUAD_BINDING_BIAS = 2;
	private const int QUAD_ASSET_LOOKUP_CACHE_SIZE = 256;
	private const int INSTANCE_DIRTY_MATRIX = 1 << 0;
	private const int INSTANCE_DIRTY_COLOR = 1 << 1;
	private const int INSTANCE_DIRTY_GEOMETRY = 1 << 2;
	private const int INSTANCE_DIRTY_ROOT = 1 << 3;
	private const int INSTANCE_DIRTY_FLAGS = 1 << 4;
	private const int INSTANCE_FLAG_FLIP_X = 1 << 0;
	private const int INSTANCE_FLAG_FLIP_Y = 1 << 1;
	private const int INSTANCE_FLAG_ACTIVE = 1 << 2;
	private const int INSTANCE_DIRTY_ALL = INSTANCE_DIRTY_MATRIX | INSTANCE_DIRTY_COLOR | INSTANCE_DIRTY_GEOMETRY | INSTANCE_DIRTY_ROOT | INSTANCE_DIRTY_FLAGS;
	private const FastSpriteDirtyFlags GPU_DIRTY_MASK = FastSpriteDirtyFlags.Vertex |
		FastSpriteDirtyFlags.Geometry | FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Batch |
		FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Color;
	private const CameraEvent COMMAND_EVENT = CameraEvent.AfterForwardAlpha;

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct DirtyDensityChunkJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public Int_ECSList.BurstView mDirtySlots;
		[NativeDisableUnsafePtrRestriction] public Int_ECSList.BurstView mMasks;
		[NativeDisableUnsafePtrRestriction] public FastSpriteDirtyChunkStats_ECSList.BurstView mStats;
		public int mChunkSize;
		public int mInstanceHighWater;

		public readonly void Execute(int chunkIndex)
		{
			mDirtySlots.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			int end = start + count;

			int matrixMin = int.MaxValue;
			int matrixMax = -1;
			int matrixCount = 0;
			int colorMin = int.MaxValue;
			int colorMax = -1;
			int colorCount = 0;
			int geometryMin = int.MaxValue;
			int geometryMax = -1;
			int geometryCount = 0;
			int rootMin = int.MaxValue;
			int rootMax = -1;
			int rootCount = 0;
			int flagsMin = int.MaxValue;
			int flagsMax = -1;
			int flagsCount = 0;

			for (int i = start; i < end; ++i)
			{
				int slot = mDirtySlots.mValue[i];
				if ((uint)slot >= (uint)mInstanceHighWater || (uint)slot >= (uint)mMasks.Count)
				{
					continue;
				}
				int mask = mMasks.mValue[slot];

				if ((mask & INSTANCE_DIRTY_MATRIX) != 0)
				{
					if (slot < matrixMin)
					{
						matrixMin = slot;
					}
					if (slot > matrixMax)
					{
						matrixMax = slot;
					}
					++matrixCount;
				}
				if ((mask & INSTANCE_DIRTY_COLOR) != 0)
				{
					if (slot < colorMin)
					{
						colorMin = slot;
					}
					if (slot > colorMax)
					{
						colorMax = slot;
					}
					++colorCount;
				}
				if ((mask & INSTANCE_DIRTY_GEOMETRY) != 0)
				{
					if (slot < geometryMin)
					{
						geometryMin = slot;
					}
					if (slot > geometryMax)
					{
						geometryMax = slot;
					}
					++geometryCount;
				}
				if ((mask & INSTANCE_DIRTY_ROOT) != 0)
				{
					if (slot < rootMin)
					{
						rootMin = slot;
					}
					if (slot > rootMax)
					{
						rootMax = slot;
					}
					++rootCount;
				}
				if ((mask & INSTANCE_DIRTY_FLAGS) != 0)
				{
					if (slot < flagsMin)
					{
						flagsMin = slot;
					}
					if (slot > flagsMax)
					{
						flagsMax = slot;
					}
					++flagsCount;
				}
			}

			mStats.mMatrixMin[chunkIndex] = matrixMin;
			mStats.mMatrixMax[chunkIndex] = matrixMax;
			mStats.mMatrixCount[chunkIndex] = matrixCount;
			mStats.mColorMin[chunkIndex] = colorMin;
			mStats.mColorMax[chunkIndex] = colorMax;
			mStats.mColorCount[chunkIndex] = colorCount;
			mStats.mGeometryMin[chunkIndex] = geometryMin;
			mStats.mGeometryMax[chunkIndex] = geometryMax;
			mStats.mGeometryCount[chunkIndex] = geometryCount;
			mStats.mRootMin[chunkIndex] = rootMin;
			mStats.mRootMax[chunkIndex] = rootMax;
			mStats.mRootCount[chunkIndex] = rootCount;
			mStats.mFlagsMin[chunkIndex] = flagsMin;
			mStats.mFlagsMax[chunkIndex] = flagsMax;
			mStats.mFlagsCount[chunkIndex] = flagsCount;
		}
	}

	private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
	private static readonly int GEOMETRY_ID = Shader.PropertyToID("_FastSpriteGeometry");
	private static readonly int QUAD_ASSETS_ID = Shader.PropertyToID("_FastSpriteQuadAssets");
	private static readonly int LOCAL_MATRICES_ID = Shader.PropertyToID("_FastSpriteLocalMatrices");
	private static readonly int COLORS_ID = Shader.PropertyToID("_FastSpriteColors");
	private static readonly int GEOMETRY_OFFSETS_ID = Shader.PropertyToID("_FastSpriteGeometryOffsets");
	private static readonly int ROOT_INDICES_ID = Shader.PropertyToID("_FastSpriteRootIndices");
	private static readonly int FLAGS_ID = Shader.PropertyToID("_FastSpriteFlags");
	private static readonly int ORDER_ID = Shader.PropertyToID("_FastSpriteDrawOrder");
	private static readonly int ORDER_BASE_ID = Shader.PropertyToID("_FastSpriteDrawOrderBase");
	private static readonly int ROOT_MATRICES_ID = Shader.PropertyToID("_FastSpriteRootMatrices");
	private static readonly int ROOT_MATRIX_COUNT_ID = Shader.PropertyToID("_FastSpriteRootMatrixCount");
	private static readonly int INDIRECT_ORDER_BASES_ID = Shader.PropertyToID("_FastSpriteIndirectOrderBases");
	private static readonly int INDIRECT_COMMAND_INDEX_ID = Shader.PropertyToID("_FastSpriteIndirectCommandIndex");
	private static readonly int USE_INDIRECT_ORDER_BASE_ID = Shader.PropertyToID("_FastSpriteUseIndirectOrderBase");
	private static readonly int[] VIRTUAL_TEXTURE_IDS =
	{
		Shader.PropertyToID("_FastSpriteTex0"),
		Shader.PropertyToID("_FastSpriteTex1"),
		Shader.PropertyToID("_FastSpriteTex2"),
		Shader.PropertyToID("_FastSpriteTex3"),
		Shader.PropertyToID("_FastSpriteTex4"),
		Shader.PropertyToID("_FastSpriteTex5"),
		Shader.PropertyToID("_FastSpriteTex6"),
		Shader.PropertyToID("_FastSpriteTex7"),
	};
	private static readonly int SCATTER_MATRIX_PATCHES_ID = Shader.PropertyToID("_FastSpriteMatrixPatches");
	private static readonly int SCATTER_COLOR_PATCHES_ID = Shader.PropertyToID("_FastSpriteColorPatches");
	private static readonly int SCATTER_GEOMETRY_PATCHES_ID = Shader.PropertyToID("_FastSpriteGeometryPatches");
	private static readonly int SCATTER_ROOT_PATCHES_ID = Shader.PropertyToID("_FastSpriteRootPatches");
	private static readonly int SCATTER_FLAGS_PATCHES_ID = Shader.PropertyToID("_FastSpriteFlagsPatches");
	private static readonly int SCATTER_COUNTS_ID = Shader.PropertyToID("_FastSpriteScatterCounts");

	private readonly List<FastSpriteRenderer> mDirtyRenderers = new(512);
	private int mDirectVisualEpoch = 1;
	private int mDirectVisualRendererCountPending;
	private int mDirectSpriteAssetCommitCountPending;
	private readonly FastSpriteGPUPlanSlotData_ECSList mPlanSlots = new(1024);
	private readonly FastSpriteGPUInstanceHotData_ECSList mInstanceHot = new(1024);
	private readonly Int_ECSList mOrderECS = new(4096);
	private readonly FastSpriteGPUDrawCommandData_ECSList mDrawCommands = new(128);
	// EasyECS lists are retained as SoA arenas. Logical counts avoid Clear/Add structural
	// work on every plan rebuild while columns stay allocated and cache-hot.
	private int mOrderCount;
	private int mDrawCommandCount;
	private readonly List<FastSpriteVertex> mGeometryVertices = new(64);
	private readonly List<int> mGeometryIndices = new(96);
	private readonly Int_ECSList mDirtyInstanceSlots = new(512);
	private readonly Int_ECSList mInstanceDirtyMaskBySlot = new(1024);
	private readonly Bool_ECSList mInstanceDirtyQueuedBySlot = new(1024);
	private readonly FastSpriteDirtyChunkStats_ECSList mDirtyChunkStats = new(32);
	private readonly Stack<int> mFreeInstanceSlots = new();
	private readonly FastDictionary<int, Stack<int>> mFreeGeometryBlocks = new();
	// Sprite assets are cold identity/configuration data, not a per-frame bulk-access stream,
	// so they intentionally stay outside EasyECS. Hot per-renderer plan state remains SoA.
	private readonly FastDictionary<Sprite, int> mQuadAssetIndices = new(64);
	private int[] mQuadAssetIndexBySimpleAssetID = Array.Empty<int>();
	private readonly Sprite[] mQuadAssetLookupSprites = new Sprite[QUAD_ASSET_LOOKUP_CACHE_SIZE];
	private readonly int[] mQuadAssetLookupIndices = new int[QUAD_ASSET_LOOKUP_CACHE_SIZE];
	private readonly FastDictionary<FastSpriteGPURenderStateKey, int> mRenderStateIDs = new(64);
	private int mNextRenderStateID = 1;
	private readonly FastDictionary<Material, Material> mProxyMaterials = new(8);
	private readonly HashSet<Material> mProxyMaterialsSyncedThisPlan = new();
	// Unity native-backed objects must not be created while FastSpriteRenderSystem's
	// MonoBehaviour field initializers are running. The backend itself is intentionally
	// constructor-safe/pure-managed; create the MPB lazily on the first command rebuild.
	private MaterialPropertyBlock mPropertyBlock;
	private readonly FastDictionary<Texture, int> mVirtualTextureSlots = new(VIRTUAL_TEXTURE_SLOT_COUNT);
	private readonly Texture[] mVirtualTextures = new Texture[VIRTUAL_TEXTURE_SLOT_COUNT];
	private readonly int[] mVirtualTextureRefCounts = new int[VIRTUAL_TEXTURE_SLOT_COUNT];

	private readonly FastDictionary<object, FastSpriteGPUPlanCachedSpan> mPlanSpanCache = new();
	private readonly List<object> mPlanSpanCachePruneKeys = new(64);
	private int mPlanSpanCacheBuildStamp;
	private bool mBuildingFullPlanCache;

	private int[] mCommittedCommandRenderStateIDs = Array.Empty<int>();
	private int[] mCommittedCommandOrderStarts = Array.Empty<int>();
	private int[] mCommittedCommandInstanceCounts = Array.Empty<int>();
	private int mCommittedCommandCount = -1;

	private FastSpriteGPUDrivenVertex[] mGeometryCPU = Array.Empty<FastSpriteGPUDrivenVertex>();
	private FastSpriteGPUDrivenQuadAsset[] mQuadAssetCPU = Array.Empty<FastSpriteGPUDrivenQuadAsset>();
	private FastSpriteGPUMatrixPatch[] mMatrixPatchCPU = Array.Empty<FastSpriteGPUMatrixPatch>();
	private FastSpriteGPUColorPatch[] mColorPatchCPU = Array.Empty<FastSpriteGPUColorPatch>();
	private FastSpriteGPUScalarPatch[] mGeometryPatchCPU = Array.Empty<FastSpriteGPUScalarPatch>();
	private FastSpriteGPUScalarPatch[] mRootPatchCPU = Array.Empty<FastSpriteGPUScalarPatch>();
	private FastSpriteGPUScalarPatch[] mFlagsPatchCPU = Array.Empty<FastSpriteGPUScalarPatch>();
	private readonly int[] mScatterCountsCPU = new int[5];
	private int[] mOrderFallbackCPU = Array.Empty<int>();
	private uint[] mIndirectArgsCPU = Array.Empty<uint>();
	private int[] mIndirectOrderBasesCPU = Array.Empty<int>();
	private int[] mIndirectMergedVertexCounts = Array.Empty<int>();
	private int[] mIndirectMergedOrderStarts = Array.Empty<int>();
	private int[] mIndirectMergedInstanceCounts = Array.Empty<int>();
	private FastSpriteRenderer[] mSlotOwners = Array.Empty<FastSpriteRenderer>();
	private int[] mTextureSlotCodeBySlot = Array.Empty<int>();
	private int mGeometryHighWater;
	private int mQuadAssetHighWater;
	private int mInstanceHighWater;
	private int mInstanceNextSlot;
	private int mGeometryDirtyMin = int.MaxValue;
	private int mGeometryDirtyMax = -1;
	private int mQuadAssetDirtyMin = int.MaxValue;
	private int mQuadAssetDirtyMax = -1;
	private int mLayoutRevision;
	private bool mPlanValid;
	private bool mPlanUnsupported;
	private bool mCommandDirty = true;
	private bool mOrderDirty = true;
	private bool mIndirectPlanEligible;
	private bool mIndirectDataDirty;
	private bool mUsingPersistentIndirect;
	private Material mIndirectPlanMaterial;
	private Material mBoundIndirectPlanMaterial;
	private bool mDisposed;

	private GraphicsBuffer mGeometryBuffer;
	private GraphicsBuffer mQuadAssetBuffer;
	private GraphicsBuffer mLocalMatrixBuffer;
	private GraphicsBuffer mColorBuffer;
	private GraphicsBuffer mGeometryOffsetBuffer;
	private GraphicsBuffer mRootIndexBuffer;
	private GraphicsBuffer mFlagsBuffer;
	private GraphicsBuffer mOrderBuffer;
	private GraphicsBuffer mFallbackRootBuffer;
	private GraphicsBuffer mMatrixPatchBuffer;
	private GraphicsBuffer mColorPatchBuffer;
	private GraphicsBuffer mGeometryPatchBuffer;
	private GraphicsBuffer mRootPatchBuffer;
	private GraphicsBuffer mFlagsPatchBuffer;
	private GraphicsBuffer mScatterCountBuffer;
	private GraphicsBuffer mIndirectArgsBuffer;
	private GraphicsBuffer mIndirectOrderBaseBuffer;
	private int mGeometryBufferCapacity;
	private int mQuadAssetBufferCapacity;
	private int mInstanceBufferCapacity;
	private int mOrderBufferCapacity;
	private int mInstancePatchBufferCapacity;
	private int mIndirectCommandCapacity;
	// Number of DrawProceduralIndirect commands actually recorded in the persistent CommandBuffer.
	// Unlike buffer capacity this is not rounded to a power of two. It is a session high-water so
	// ordinary order/count churn only updates indirect args and never re-records commands.
	private int mIndirectRecordedDrawCount;
	private int mIndirectMergedCommandCount;
	private Shader mShader;
	private ComputeShader mInstanceScatterShader;
	private int mInstanceScatterKernel = -1;
	private int mInstanceScatterSupportState;
	private int mRuntimeSupportState;
	private FastSpriteBackendMode mSubmissionMode = FastSpriteBackendMode.GPUDrivenIndirect;
	private CommandBuffer mCommandBuffer;
	private Camera mCommandCamera;
	private readonly List<Camera> mAttachedCommandCameras = new(4);
	private GraphicsBuffer mBoundRootBuffer;
	private int mBoundRootCount = -1;

	private FrameStats mFrameStats;

	internal FrameStats getFrameStats()
	{
		return mFrameStats;
	}
	internal bool isPlanValid()
	{
		return mPlanValid;
	}
	internal int getLayoutRevision()
	{
		return mLayoutRevision;
	}
	internal int getOrderedSlotCount()
	{
		return mOrderCount;
	}
	internal int getOrderedSlot(int index)
	{
		return (uint)index < (uint)mOrderCount ? mOrderECS.getValueColumn()[index] : -1;
	}
	internal void beginFrame()
	{
		mFrameStats = default;
		mFrameStats.mDrawCount = mPlanValid
			? (mUsingPersistentIndirect ? mIndirectRecordedDrawCount : mDrawCommandCount)
			: 0;
		mFrameStats.mInstanceCount = mPlanValid ? mOrderCount : 0;
	}

	internal void setSubmissionMode(FastSpriteBackendMode mode)
	{
		FastSpriteBackendMode concrete = mode == FastSpriteBackendMode.GPUDrivenDirect ? FastSpriteBackendMode.GPUDrivenDirect :
			mode == FastSpriteBackendMode.CompatMesh ? FastSpriteBackendMode.CompatMesh : FastSpriteBackendMode.GPUDrivenIndirect;
		if (mSubmissionMode == concrete)
		{
			return;
		}
		mSubmissionMode = concrete;
		mRuntimeSupportState = 0;
		mCommandDirty = true;
		if (concrete == FastSpriteBackendMode.CompatMesh)
		{
			detachCommandBuffer();
			invalidatePlan();
		}
	}

	internal bool isRuntimeSupported()
	{
		if (mDisposed || !Application.isPlaying || mSubmissionMode == FastSpriteBackendMode.CompatMesh)
		{
			return false;
		}
		if (mRuntimeSupportState == 0)
		{
			// Built-in pipeline only. Both GPU backends share the same StructuredBuffer shader;
			// only the draw submission differs (Indirect vs Direct Procedural).
			bool supported = SystemInfo.supportsInstancing && SystemInfo.graphicsShaderLevel >= 45 &&
				GraphicsSettings.currentRenderPipeline == null;
			mShader = supported ? Resources.Load<Shader>("FastSpriteGPUDriven") : null;
			mRuntimeSupportState = supported && mShader != null && mShader.isSupported ? 1 : -1;
		}
		return mRuntimeSupportState > 0;
	}

	internal bool isUsingIndirectSubmission()
	{
		return mUsingPersistentIndirect;
	}
	internal bool hasCommittedCommands()
	{
		return mCommandBuffer != null && mPlanValid && mDrawCommandCount > 0;
	}

	internal static bool isLikelyAndroidEmulator()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}

		// Do not depend on UnityEngine.AndroidJNIModule here. FastGUI should compile even when
		// the optional Android JNI built-in package is disabled. SystemInfo is sufficient for
		// the compatibility decision and, most importantly, reliably catches x86/x86_64
		// Android emulators running an ARM64 APK through a translation layer.
		string processor = SystemInfo.processorType ?? string.Empty;
		if (containsIgnoreCase(processor, "x86") || containsIgnoreCase(processor, "intel"))
		{
			return true;
		}

		string deviceModel = SystemInfo.deviceModel ?? string.Empty;
		string deviceName = SystemInfo.deviceName ?? string.Empty;
		string graphicsName = SystemInfo.graphicsDeviceName ?? string.Empty;
		string graphicsVendor = SystemInfo.graphicsDeviceVendor ?? string.Empty;
		string os = SystemInfo.operatingSystem ?? string.Empty;

		return containsEmulatorToken(deviceModel) ||
			containsEmulatorToken(deviceName) ||
			containsEmulatorToken(graphicsName) ||
			containsEmulatorToken(graphicsVendor) ||
			containsIgnoreCase(graphicsName, "swiftshader") ||
			containsIgnoreCase(graphicsName, "llvmpipe") ||
			containsIgnoreCase(graphicsName, "angle") ||
			containsIgnoreCase(graphicsVendor, "google") &&
				(containsIgnoreCase(graphicsName, "emulator") || containsIgnoreCase(graphicsName, "swiftshader")) ||
			containsIgnoreCase(os, "x86");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool containsEmulatorToken(string value)
	{
		return containsIgnoreCase(value, "emulator") ||
			containsIgnoreCase(value, "generic") ||
			containsIgnoreCase(value, "goldfish") ||
			containsIgnoreCase(value, "ranchu") ||
			containsIgnoreCase(value, "vbox") ||
			containsIgnoreCase(value, "virtualbox") ||
			containsIgnoreCase(value, "nox") ||
			containsIgnoreCase(value, "genymotion") ||
			containsIgnoreCase(value, "bluestacks") ||
			containsIgnoreCase(value, "memu") ||
			containsIgnoreCase(value, "ldplayer") ||
			containsIgnoreCase(value, "sdk_gphone") ||
			containsIgnoreCase(value, "android sdk built for");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool containsIgnoreCase(string value, string token)
	{
		return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	internal bool isMaterialCompatible(Material material)
	{
		if (!isRuntimeSupported())
		{
			return false;
		}
		Shader shader = material != null ? material.shader : null;
		return shader != null && shader.name == "Sprites/Default";
	}

	private int getOrCreateVirtualTextureSlot(Texture texture)
	{
		if (texture == null)
		{
			return -1;
		}
		if (mVirtualTextureSlots.TryGetValue(texture, out int existing))
		{
			return existing;
		}
		for (int i = 0; i < VIRTUAL_TEXTURE_SLOT_COUNT; ++i)
		{
			Texture current = mVirtualTextures[i];
			if (current != null)
			{
				continue;
			}
			mVirtualTextures[i] = texture;
			mVirtualTextureSlots.Add(texture, i);
			// Global texture bindings are recorded into the persistent CommandBuffer. A newly
			// occupied slot changes resource identity only once on the cold registration path.
			mCommandDirty = true;
			return i;
		}
		return -1;
	}

	private void releaseVirtualTextureSlotCode(int slotCode)
	{
		if ((uint)slotCode >= VIRTUAL_TEXTURE_SLOT_COUNT)
		{
			return;
		}
		if (mVirtualTextureRefCounts[slotCode] <= 0)
		{
			return;
		}
		if (--mVirtualTextureRefCounts[slotCode] != 0)
		{
			return;
		}
		Texture texture = mVirtualTextures[slotCode];
		if (texture != null)
		{
			mVirtualTextureSlots.Remove(texture);
		}
		mVirtualTextures[slotCode] = null;
		// The slot can now be reused by the current working set. The persistent command
		// buffer must refresh its global sampler binding only on this cold lifetime edge.
		mCommandDirty = true;
	}

	private int assignVirtualTextureSlot(int planSlot, Material material, Texture texture)
	{
		if ((uint)planSlot >= (uint)mTextureSlotCodeBySlot.Length)
		{
			return INSTANCE_TEXTURE_SLOT_FALLBACK;
		}
		int oldCode = mTextureSlotCodeBySlot[planSlot];
		bool compatible = isMaterialCompatible(material) && texture != null;
		if (compatible && (uint)oldCode < VIRTUAL_TEXTURE_SLOT_COUNT &&
			ReferenceEquals(mVirtualTextures[oldCode], texture))
		{
			return oldCode;
		}

		releaseVirtualTextureSlotCode(oldCode);
		int newCode = compatible ? getOrCreateVirtualTextureSlot(texture) : -1;
		if (newCode >= 0)
		{
			++mVirtualTextureRefCounts[newCode];
		}
		mTextureSlotCodeBySlot[planSlot] = newCode >= 0 ? newCode : INSTANCE_TEXTURE_SLOT_FALLBACK;
		return mTextureSlotCodeBySlot[planSlot];
	}

	private int getRendererTextureSlotCode(FastSpriteRenderer renderer)
	{
		int slot = renderer != null ? renderer.mGPUDrivenSlot : -1;
		return (uint)slot < (uint)mTextureSlotCodeBySlot.Length
			? mTextureSlotCodeBySlot[slot]
			: INSTANCE_TEXTURE_SLOT_FALLBACK;
	}

	private void recordVirtualTextureBindings()
	{
		if (mCommandBuffer == null)
		{
			return;
		}
		Texture fallback = Texture2D.whiteTexture;
		for (int i = 0; i < VIRTUAL_TEXTURE_SLOT_COUNT; ++i)
		{
			Texture texture = mVirtualTextures[i];
			mCommandBuffer.SetGlobalTexture(VIRTUAL_TEXTURE_IDS[i], texture != null ? texture : fallback);
		}
	}

	private bool buildMergedIndirectPlan(out Material material)
	{
		material = null;
		mIndirectMergedCommandCount = 0;
		if (!SystemInfo.supportsComputeShaders || mDrawCommandCount <= 0)
		{
			return false;
		}

		FastSpriteGPUDrawCommandDataRef firstCommand = mDrawCommands[0];
		material = firstCommand.mMaterial;
		if (material == null || !isMaterialCompatible(material))
		{
			material = null;
			return false;
		}

		if (mIndirectMergedVertexCounts.Length < mDrawCommandCount)
		{
			int capacity = Mathf.NextPowerOfTwo(Mathf.Max(mDrawCommandCount, 16));
			Array.Resize(ref mIndirectMergedVertexCounts, capacity);
			Array.Resize(ref mIndirectMergedOrderStarts, capacity);
			Array.Resize(ref mIndirectMergedInstanceCounts, capacity);
		}

		var orderStarts = mDrawCommands.getOrderStartColumn();
		var instanceCounts = mDrawCommands.getInstanceCountColumn();
		for (int i = 0; i < mDrawCommandCount; ++i)
		{
			FastSpriteGPUDrawCommandDataRef commandRef = mDrawCommands[i];
			if (!ReferenceEquals(commandRef.mMaterial, material) || commandRef.mTexture == null)
			{
				material = null;
				mIndirectMergedCommandCount = 0;
				return false;
			}

			// Avoid FastDictionary<UnityEngine.Object,...> work in the rebuilt-plan hot path. The
			// virtual table has only eight entries, so direct reference probes are cheaper and
			// also prove that every logical command texture is resident in the current working set.
			bool textureResident = false;
			for (int textureIndex = 0; textureIndex < VIRTUAL_TEXTURE_SLOT_COUNT; ++textureIndex)
			{
				if (ReferenceEquals(mVirtualTextures[textureIndex], commandRef.mTexture))
				{
					textureResident = true;
					break;
				}
			}
			if (!textureResident)
			{
				material = null;
				mIndirectMergedCommandCount = 0;
				return false;
			}

			int instanceCount = instanceCounts[i];
			if (instanceCount <= 0 || commandRef.mVertexCount <= 0)
			{
				continue;
			}
			int orderStart = orderStarts[i];
			int mergedIndex = mIndirectMergedCommandCount - 1;
			if (mergedIndex >= 0 &&
				mIndirectMergedVertexCounts[mergedIndex] == commandRef.mVertexCount &&
				mIndirectMergedOrderStarts[mergedIndex] + mIndirectMergedInstanceCounts[mergedIndex] == orderStart)
			{
				mIndirectMergedInstanceCounts[mergedIndex] += instanceCount;
				continue;
			}

			mergedIndex = mIndirectMergedCommandCount++;
			mIndirectMergedVertexCounts[mergedIndex] = commandRef.mVertexCount;
			mIndirectMergedOrderStarts[mergedIndex] = orderStart;
			mIndirectMergedInstanceCounts[mergedIndex] = instanceCount;
		}

		if (mIndirectMergedCommandCount <= 0)
		{
			material = null;
			return false;
		}

		if (mIndirectMergedCommandCount > mDrawCommandCount)
		{
			material = null;
			mIndirectMergedCommandCount = 0;
			return false;
		}
		return true;
	}

	private bool canUseInstanceScatter()
	{
		if (mInstanceScatterSupportState == 0)
		{
			if (!SystemInfo.supportsComputeShaders)
			{
				mInstanceScatterSupportState = -1;
			}
			else
			{
				if (mInstanceScatterShader == null)
				{
					mInstanceScatterShader = Resources.Load<ComputeShader>("FastSpriteGPUInstanceScatter");
				}
				if (mInstanceScatterShader == null)
				{
					mInstanceScatterSupportState = -1;
				}
				else
				{
					try
					{
						mInstanceScatterKernel = mInstanceScatterShader.FindKernel("CSMain");
						mInstanceScatterSupportState = mInstanceScatterKernel >= 0 ? 1 : -1;
					}
					catch
					{
						mInstanceScatterKernel = -1;
						mInstanceScatterSupportState = -1;
					}
				}
			}
		}
		return mInstanceScatterSupportState > 0 && mInstanceScatterShader != null && mInstanceScatterKernel >= 0;
	}

	internal void reserveRendererCapacity(int requiredCount)
	{
		if (mDisposed || requiredCount <= 0)
		{
			return;
		}
		ensureInstanceHotCapacity(requiredCount);
		ensurePlanSlotCapacity(requiredCount);
	}

	internal void registerRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || mDisposed)
		{
			return;
		}
		if (renderer.mGPUDrivenSlot < 0)
		{
			int slot = mFreeInstanceSlots.Count > 0 ? mFreeInstanceSlots.Pop() : mInstanceNextSlot++;
			ensureInstanceHotCapacity(slot + 1);
			ensurePlanSlotCapacity(slot + 1);
			renderer.mGPUDrivenSlot = slot;
			mSlotOwners[slot] = renderer;
			mInstanceHighWater = Mathf.Max(mInstanceHighWater, slot + 1);
		}
		else
		{
			ensurePlanSlotCapacity(renderer.mGPUDrivenSlot + 1);
			mSlotOwners[renderer.mGPUDrivenSlot] = renderer;
		}
		renderer.mGPUDrivenDirectVisualEpoch = 0;
		markInstanceDirty(renderer.mGPUDrivenSlot, INSTANCE_DIRTY_ALL);
		queueDirty(renderer, FastSpriteDirtyFlags.All);
	}

	internal void unregisterRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		removeDirtyRenderer(renderer);
		releaseGeometryReservation(renderer);
		int slot = renderer.mGPUDrivenSlot;
		if (slot >= 0)
		{
			if ((uint)slot < (uint)mInstanceHot.Count)
			{
				mInstanceHot.getLocalMatrixColumn()[slot] = default;
				mInstanceHot.getColorColumn()[slot] = default;
				mInstanceHot.getGeometryOffsetColumn()[slot] = -1;
				mInstanceHot.getRootIndexColumn()[slot] = -1;
				mInstanceHot.getFlagsColumn()[slot] = 0;
			}
			if ((uint)slot < (uint)mInstanceDirtyMaskBySlot.Count)
			{
				mInstanceDirtyMaskBySlot.getValueColumn()[slot] = 0;
			}
			if ((uint)slot < (uint)mInstanceDirtyQueuedBySlot.Count)
			{
				mInstanceDirtyQueuedBySlot.getValueColumn()[slot] = false;
			}
			if ((uint)slot < (uint)mSlotOwners.Length && ReferenceEquals(mSlotOwners[slot], renderer))
			{
				mSlotOwners[slot] = null;
			}
			clearPlanSlot(slot);
			mFreeInstanceSlots.Push(slot);
		}
		renderer.mGPUDrivenSlot = -1;
		renderer.mGPUDrivenDirtyFlags = FastSpriteDirtyFlags.None;
		renderer.mGPUDrivenDirtyQueued = false;
		renderer.mGPUDrivenDirectVisualEpoch = 0;
		mPlanValid = false;
		mCommandDirty = true;
	}

	internal void queueVisualDirty(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		// tryQueueGPUVisualDirty() already validated owner/batch/slot/runtime state.
		if (mDisposed)
		{
			return;
		}
		if (tryApplyVisualDirtyDirect(renderer, flags))
		{
			noteDirectVisualRenderer(renderer);
			return;
		}

		queueDirty(renderer, flags);
	}

	internal bool tryCommitSpriteAsset(FastSpriteRenderer renderer, int simpleAssetID)
	{
		if (mDisposed || renderer == null || simpleAssetID <= 0)
		{
			return false;
		}
		int slot = renderer.mGPUDrivenSlot;
		if ((uint)slot >= (uint)mInstanceHot.Count ||
			(uint)simpleAssetID >= (uint)mQuadAssetIndexBySimpleAssetID.Length)
		{
			return false;
		}
		int assetIndex = mQuadAssetIndexBySimpleAssetID[simpleAssetID];
		if (assetIndex < 0)
		{
			return false;
		}

		int binding = encodeQuadBinding(assetIndex);
		if (renderer.mGPUDrivenGeometryOffset >= 0)
		{
			releaseGeometryReservation(renderer);
		}
		renderer.mGPUDrivenGeometryOffset = binding;
		renderer.mGPUDrivenGeometryCapacity = 0;
		renderer.mGPUDrivenGeometryVertexCount = 6;

		var geometryOffsets = mInstanceHot.getGeometryOffsetColumn();
		if (geometryOffsets[slot] != binding)
		{
			geometryOffsets[slot] = binding;
			markInstanceDirty(slot, INSTANCE_DIRTY_GEOMETRY);
		}
		noteDirectVisualRenderer(renderer);
		++mDirectSpriteAssetCommitCountPending;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void noteDirectVisualRenderer(FastSpriteRenderer renderer)
	{
		if (renderer.mGPUDrivenDirectVisualEpoch == mDirectVisualEpoch)
		{
			return;
		}
		renderer.mGPUDrivenDirectVisualEpoch = mDirectVisualEpoch;
		++mDirectVisualRendererCountPending;
	}

	private bool tryApplyVisualDirtyDirect(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		int slot = renderer.mGPUDrivenSlot;
		if ((uint)slot >= (uint)mInstanceHot.Count)
		{
			return false;
		}

		if (flags == FastSpriteDirtyFlags.Color)
		{
			Color color = renderer.getColor();
			Vector4 value = new(color.r, color.g, color.b, color.a);
			var colors = mInstanceHot.getColorColumn();
			if (colors[slot] != value)
			{
				colors[slot] = value;
				markInstanceDirty(slot, INSTANCE_DIRTY_COLOR);
			}
			return true;
		}

		if (flags == FastSpriteDirtyFlags.Vertex && tryResolveHotQuadBinding(renderer, out int quadBinding))
		{
			int dirtyMask = 0;
			var geometryOffsets = mInstanceHot.getGeometryOffsetColumn();
			if (geometryOffsets[slot] != quadBinding)
			{
				geometryOffsets[slot] = quadBinding;
				dirtyMask |= INSTANCE_DIRTY_GEOMETRY;
			}

			var instanceFlags = mInstanceHot.getFlagsColumn();
			int oldFlags = instanceFlags[slot];
			int newFlags = (oldFlags & ~(INSTANCE_FLAG_FLIP_X | INSTANCE_FLAG_FLIP_Y)) |
				(renderer.getFlipX() ? INSTANCE_FLAG_FLIP_X : 0) |
				(renderer.getFlipY() ? INSTANCE_FLAG_FLIP_Y : 0);
			if (newFlags != oldFlags)
			{
				instanceFlags[slot] = newFlags;
				dirtyMask |= INSTANCE_DIRTY_FLAGS;
			}

			if (dirtyMask != 0)
			{
				markInstanceDirty(slot, dirtyMask);
			}
			return true;
		}

		return false;
	}

	internal void queueDirty(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		if (renderer == null || renderer.mGPUDrivenSlot < 0 || mDisposed)
		{
			return;
		}
		flags &= GPU_DIRTY_MASK;
		if (flags == FastSpriteDirtyFlags.None)
		{
			return;
		}
		renderer.mGPUDrivenDirtyFlags |= flags;
		if (renderer.mGPUDrivenDirtyQueued)
		{
			return;
		}
		renderer.mGPUDrivenDirtyQueued = true;
		mDirtyRenderers.Add(renderer);
	}

	internal void notifyActiveStateChanged(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		int slot = renderer.mGPUDrivenSlot;
		if ((uint)slot >= (uint)mPlanSlots.Count || (uint)slot >= (uint)mInstanceHot.Count)
		{
			return;
		}
		bool active = renderer.mRuntimeActiveState;
		mPlanSlots.getActiveColumn()[slot] = active ? 1 : 0;

		var flagsColumn = mInstanceHot.getFlagsColumn();
		int oldFlags = flagsColumn[slot];
		int newFlags = active ? (oldFlags | INSTANCE_FLAG_ACTIVE) : (oldFlags & ~INSTANCE_FLAG_ACTIVE);
		if (newFlags != oldFlags)
		{
			flagsColumn[slot] = newFlags;
			markInstanceDirty(slot, INSTANCE_DIRTY_FLAGS);
		}
	}

	internal void processDirtyRenderers(FastSpriteRenderSystem owner)
	{
		long start = System.Diagnostics.Stopwatch.GetTimestamp();
		int directVisualCount = mDirectVisualRendererCountPending;
		mDirectVisualRendererCountPending = 0;
		int directSpriteAssetCommitCount = mDirectSpriteAssetCommitCountPending;
		mDirectSpriteAssetCommitCountPending = 0;
		mFrameStats.mSpriteAssetCommitCount += directSpriteAssetCommitCount;
		int queuedCount = mDirtyRenderers.Count;
		int processedCount = 0;
		if (queuedCount == 0)
		{
			mFrameStats.mDirtyRendererCount = directVisualCount;
			mFrameStats.mHotWriteCount += directVisualCount;
			mFrameStats.mDirtyBuildMS = elapsedMS(start);
			advanceDirectVisualEpoch();
			return;
		}

		for (int i = 0; i < queuedCount; ++i)
		{
			FastSpriteRenderer renderer = mDirtyRenderers[i];
			if (renderer == null)
			{
				continue;
			}
			renderer.mGPUDrivenDirtyQueued = false;
			FastSpriteDirtyFlags flags = renderer.mGPUDrivenDirtyFlags & GPU_DIRTY_MASK;
			FastSpriteDirtyFlags originalFlags = flags;
			renderer.mGPUDrivenDirtyFlags = FastSpriteDirtyFlags.None;
			int slot = renderer.mGPUDrivenSlot;
			if (flags == FastSpriteDirtyFlags.None || slot < 0 || renderer.mSystemOwner != owner ||
				(uint)slot >= (uint)mSlotOwners.Length || !ReferenceEquals(mSlotOwners[slot], renderer))
			{
				continue;
			}
			bool alreadyCountedDirect = renderer.mGPUDrivenDirectVisualEpoch == mDirectVisualEpoch;
			if (!alreadyCountedDirect)
			{
				++processedCount;
			}

			if (flags == FastSpriteDirtyFlags.Vertex)
			{
				updateRendererGeometry(renderer);
				updateRendererBinding(renderer);
				renderer.clearDirtyFlags(originalFlags);
				continue;
			}
			if (flags == FastSpriteDirtyFlags.Transform)
			{
				updateRendererTransform(owner, renderer);
				renderer.clearDirtyFlags(originalFlags);
				continue;
			}
			if (flags == FastSpriteDirtyFlags.Color)
			{
				updateRendererColor(renderer);
				renderer.clearDirtyFlags(originalFlags);
				continue;
			}

			bool geometryDirty = (flags & (FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Geometry | FastSpriteDirtyFlags.Topology)) != 0;
			if (geometryDirty)
			{
				updateRendererGeometry(renderer);
			}
			if ((flags & FastSpriteDirtyFlags.Transform) != 0)
			{
				updateRendererTransform(owner, renderer);
			}
			if ((flags & FastSpriteDirtyFlags.Color) != 0)
			{
				updateRendererColor(renderer);
			}
			bool planStateDirty = (flags & (FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.Geometry | FastSpriteDirtyFlags.Topology)) != 0;
			if (planStateDirty)
			{
				syncPlanSlot(renderer);
			}
			if (geometryDirty || (flags & FastSpriteDirtyFlags.Batch) != 0)
			{
				updateRendererBinding(renderer);
			}
			renderer.clearDirtyFlags(originalFlags);
		}
		mDirtyRenderers.Clear();
		mFrameStats.mDirtyRendererCount = directVisualCount + processedCount;
		mFrameStats.mHotWriteCount += directVisualCount;
		mFrameStats.mDirtyBuildMS = elapsedMS(start);
		advanceDirectVisualEpoch();
	}

	private void advanceDirectVisualEpoch()
	{
		unchecked
		{
			++mDirectVisualEpoch;
		}
		if (mDirectVisualEpoch != 0)
		{
			return;
		}
		// Integer wrap is practically unreachable, but zero is reserved for never-seen renderers.
		mDirectVisualEpoch = 1;
	}

	private bool tryResolveHotQuadBinding(FastSpriteRenderer renderer, out int binding)
	{
		binding = -1;
		if (!tryResolveRendererQuadAssetIndex(renderer, out int assetIndex))
		{
			return false;
		}

		binding = encodeQuadBinding(assetIndex);
		if (renderer.mGPUDrivenGeometryOffset >= 0)
		{
			releaseGeometryReservation(renderer);
		}
		renderer.mGPUDrivenGeometryOffset = binding;
		renderer.mGPUDrivenGeometryCapacity = 0;
		renderer.mGPUDrivenGeometryVertexCount = 6;
		return true;
	}

	private bool tryResolveRendererQuadAssetIndex(FastSpriteRenderer renderer, out int assetIndex)
	{
		assetIndex = -1;
		if (renderer == null || renderer.getDrawMode() != SpriteDrawMode.Simple ||
			!renderer.tryGetSimpleAssetIDCached(out int simpleAssetID))
		{
			return false;
		}

		if ((uint)simpleAssetID < (uint)mQuadAssetIndexBySimpleAssetID.Length)
		{
			assetIndex = mQuadAssetIndexBySimpleAssetID[simpleAssetID];
			if (assetIndex >= 0)
			{
				return true;
			}
		}

		Sprite sprite = renderer.getSprite();
		if (ReferenceEquals(sprite, null))
		{
			return false;
		}
		if (!tryGetQuadAssetIndex(sprite, out assetIndex))
		{
			if (!renderer.tryGetSimpleQuadDataCached(out FastSpriteSimpleQuadData quad))
			{
				return false;
			}
			assetIndex = createQuadAsset(sprite, quad);
			if (assetIndex < 0)
			{
				return false;
			}
		}
		setSimpleAssetQuadIndex(simpleAssetID, assetIndex);
		return true;
	}

	private void setSimpleAssetQuadIndex(int simpleAssetID, int assetIndex)
	{
		if (simpleAssetID <= 0 || assetIndex < 0)
		{
			return;
		}
		if (mQuadAssetIndexBySimpleAssetID.Length <= simpleAssetID)
		{
			int oldLength = mQuadAssetIndexBySimpleAssetID.Length;
			int required = simpleAssetID + 1;
			Array.Resize(ref mQuadAssetIndexBySimpleAssetID, Mathf.NextPowerOfTwo(Mathf.Max(required, 64)));
			for (int i = oldLength; i < mQuadAssetIndexBySimpleAssetID.Length; ++i)
			{
				mQuadAssetIndexBySimpleAssetID[i] = -1;
			}
		}
		mQuadAssetIndexBySimpleAssetID[simpleAssetID] = assetIndex;
	}

	private void ensurePlanSlotCapacity(int required)
	{
		if (required <= 0)
		{
			return;
		}
		if (mPlanSlots.Count < required)
		{
			mPlanSlots.EnsureCount(required);
		}
		if (mSlotOwners.Length < required)
		{
			Array.Resize(ref mSlotOwners, Mathf.NextPowerOfTwo(Mathf.Max(required, 64)));
		}
		if (mTextureSlotCodeBySlot.Length < required)
		{
			int oldLength = mTextureSlotCodeBySlot.Length;
			Array.Resize(ref mTextureSlotCodeBySlot, Mathf.NextPowerOfTwo(Mathf.Max(required, 64)));
			for (int i = oldLength; i < mTextureSlotCodeBySlot.Length; ++i)
			{
				mTextureSlotCodeBySlot[i] = INSTANCE_TEXTURE_SLOT_FALLBACK;
			}
		}
	}

	private void clearPlanSlot(int slot)
	{
		if ((uint)slot >= (uint)mPlanSlots.Count)
		{
			return;
		}
		if ((uint)slot < (uint)mTextureSlotCodeBySlot.Length)
		{
			releaseVirtualTextureSlotCode(mTextureSlotCodeBySlot[slot]);
			mTextureSlotCodeBySlot[slot] = INSTANCE_TEXTURE_SLOT_FALLBACK;
		}
		FastSpriteGPUPlanSlotDataRef planRef = mPlanSlots[slot];
		planRef.mMaterial = null;
		planRef.mTexture = null;
		planRef.mMaterialID = 0;
		planRef.mTextureID = 0;
		planRef.mVertexCount = 0;
		mPlanSlots.getActiveColumn()[slot] = 0;
		mPlanSlots.getRenderStateIDColumn()[slot] = 0;
	}

	private int getOrCreateRenderStateID(int materialID, int textureID, int vertexCount)
	{
		FastSpriteGPURenderStateKey stateKey = new(materialID, textureID, vertexCount);
		if (mRenderStateIDs.TryGetValue(stateKey, out int stateID))
		{
			return stateID;
		}
		stateID = mNextRenderStateID++;
		mRenderStateIDs.Add(stateKey, stateID);
		return stateID;
	}

	private void syncPlanSlot(FastSpriteRenderer renderer)
	{
		int slot = renderer != null ? renderer.mGPUDrivenSlot : -1;
		if (slot < 0)
		{
			return;
		}
		ensurePlanSlotCapacity(slot + 1);
		FastSpriteBatch batch = renderer.mBatch;
		FastSpriteBatchKey key = batch != null ? batch.getKey() : default;
		int newVertexCount = renderer.mGPUDrivenGeometryVertexCount;
		FastSpriteGPUPlanSlotDataRef planRef = mPlanSlots[slot];
		bool materialChanged = planRef.mMaterialID != key.mMaterialID;
		bool textureChanged = planRef.mTextureID != key.mTextureID;
		bool vertexCountChanged = planRef.mVertexCount != newVertexCount;
		bool planStateChanged = materialChanged || textureChanged || vertexCountChanged;

		if (materialChanged || textureChanged)
		{
			planRef.mMaterial = key.mMaterial;
			planRef.mTexture = key.mTexture;
		}
		planRef.mMaterialID = key.mMaterialID;
		planRef.mTextureID = key.mTextureID;
		planRef.mVertexCount = newVertexCount;
		mPlanSlots.getActiveColumn()[slot] = renderer.mRuntimeActiveState ? 1 : 0;
		assignVirtualTextureSlot(slot, key.mMaterial, key.mTexture);

		if (planStateChanged || mPlanSlots.getRenderStateIDColumn()[slot] == 0)
		{
			int stateID;
			if (newVertexCount <= 0)
			{
				stateID = 0;
			}
			else if (!isMaterialCompatible(key.mMaterial))
			{
				stateID = -1;
			}
			else
			{
				stateID = getOrCreateRenderStateID(key.mMaterialID, key.mTextureID, newVertexCount);
			}
			mPlanSlots.getRenderStateIDColumn()[slot] = stateID;
		}
		if (planStateChanged)
		{
			unchecked
			{
				++mLayoutRevision;
			}
		}
	}

	private bool updateRendererGeometry(FastSpriteRenderer renderer)
	{
		int oldVertexCount = renderer.mGPUDrivenGeometryVertexCount;

		if (renderer.getDrawMode() == SpriteDrawMode.Simple &&
			tryResolveRendererQuadAssetIndex(renderer, out int simpleAssetIndex))
		{
			return applyQuadAssetBinding(renderer, simpleAssetIndex, oldVertexCount);
		}

		mGeometryVertices.Clear();
		mGeometryIndices.Clear();
		FastSpriteGeometryUtility.buildLocal(renderer, mGeometryVertices, mGeometryIndices);
		int vertexCount = mGeometryIndices.Count;
		bool planLayoutChanged = oldVertexCount != vertexCount;

		if (vertexCount <= 0)
		{
			renderer.mGPUDrivenGeometryVertexCount = 0;
			if (planLayoutChanged)
			{
				unchecked
				{
					++mLayoutRevision;
				}
			}
			// The reservation is intentionally retained. If the Sprite becomes drawable again
			// with a compatible topology, no geometry/instance rebinding is necessary.
			return false;
		}

		bool bindingChanged = ensureGeometryReservation(renderer, vertexCount);
		int offset = renderer.mGPUDrivenGeometryOffset;
		for (int i = 0; i < vertexCount; ++i)
		{
			int sourceIndex = mGeometryIndices[i];
			FastSpriteVertex source = (uint)sourceIndex < (uint)mGeometryVertices.Count ? mGeometryVertices[sourceIndex] : default;
			mGeometryCPU[offset + i] = new FastSpriteGPUDrivenVertex(source.mPosition, source.mUV);
		}
		renderer.mGPUDrivenGeometryVertexCount = vertexCount;
		markGeometryDirty(offset, vertexCount);
		if (planLayoutChanged)
		{
			unchecked
			{
				++mLayoutRevision;
			}
		}
		return bindingChanged;
	}

	private static int encodeQuadBinding(int assetIndex)
	{
		return -(assetIndex + QUAD_BINDING_BIAS);
	}

	private bool tryGetQuadAssetIndex(Sprite sprite, out int assetIndex)
	{
		assetIndex = -1;
		if (ReferenceEquals(sprite, null))
		{
			return false;
		}
		int referenceHash = RuntimeHelpers.GetHashCode(sprite);
		int cacheSlot = (referenceHash ^ (referenceHash >> 7) ^ (referenceHash >> 15)) & (QUAD_ASSET_LOOKUP_CACHE_SIZE - 1);
		if (ReferenceEquals(mQuadAssetLookupSprites[cacheSlot], sprite))
		{
			assetIndex = mQuadAssetLookupIndices[cacheSlot];
			return assetIndex >= 0;
		}
		if (!mQuadAssetIndices.TryGetValue(sprite, out assetIndex))
		{
			return false;
		}
		mQuadAssetLookupSprites[cacheSlot] = sprite;
		mQuadAssetLookupIndices[cacheSlot] = assetIndex;
		return true;
	}

	private int createQuadAsset(Sprite sprite, FastSpriteSimpleQuadData quad)
	{
		if (sprite == null)
		{
			return -1;
		}
		// A collision in the tiny direct cache can still land here only if the authoritative
		// dictionary also missed. Keep one defensive lookup for cold-path reentrancy.
		if (mQuadAssetIndices.TryGetValue(sprite, out int cached))
		{
			return cached;
		}

		uint packedIndices = 0;
		for (int i = 0; i < 6; ++i)
		{
			ushort index = quad.getIndex(i);
			if (index > 3)
			{
				return -1;
			}
			packedIndices |= (uint)index << (i * 2);
		}

		int assetIndex = mQuadAssetHighWater++;
		ensureQuadAssetCPUCapacity(mQuadAssetHighWater);
		Vector2 p0 = quad.mPosition0;
		Vector2 p1 = quad.mPosition1;
		Vector2 p2 = quad.mPosition2;
		Vector2 p3 = quad.mPosition3;
		Vector2 uv0 = quad.mUV0;
		Vector2 uv1 = quad.mUV1;
		Vector2 uv2 = quad.mUV2;
		Vector2 uv3 = quad.mUV3;
		mQuadAssetCPU[assetIndex] = new FastSpriteGPUDrivenQuadAsset
		{
			mPosition01 = new Vector4(p0.x, p0.y, p1.x, p1.y),
			mPosition23 = new Vector4(p2.x, p2.y, p3.x, p3.y),
			mUV01 = new Vector4(uv0.x, uv0.y, uv1.x, uv1.y),
			mUV23 = new Vector4(uv2.x, uv2.y, uv3.x, uv3.y),
			mPackedIndices = packedIndices,
		};
		mQuadAssetDirtyMin = Mathf.Min(mQuadAssetDirtyMin, assetIndex);
		mQuadAssetDirtyMax = Mathf.Max(mQuadAssetDirtyMax, assetIndex);
		mQuadAssetIndices.Add(sprite, assetIndex);
		int referenceHash = RuntimeHelpers.GetHashCode(sprite);
		int cacheSlot = (referenceHash ^ (referenceHash >> 7) ^ (referenceHash >> 15)) & (QUAD_ASSET_LOOKUP_CACHE_SIZE - 1);
		mQuadAssetLookupSprites[cacheSlot] = sprite;
		mQuadAssetLookupIndices[cacheSlot] = assetIndex;
		return assetIndex;
	}

	private bool applyQuadAssetBinding(FastSpriteRenderer renderer, int assetIndex, int oldVertexCount)
	{
		int binding = encodeQuadBinding(assetIndex);
		bool bindingChanged = renderer.mGPUDrivenGeometryOffset != binding;
		if (renderer.mGPUDrivenGeometryOffset >= 0)
		{
			releaseGeometryReservation(renderer);
		}
		renderer.mGPUDrivenGeometryOffset = binding;
		renderer.mGPUDrivenGeometryCapacity = 0;
		renderer.mGPUDrivenGeometryVertexCount = 6;
		if (oldVertexCount != 6)
		{
			unchecked
			{
				++mLayoutRevision;
			}
		}
		return bindingChanged;
	}

	private bool ensureGeometryReservation(FastSpriteRenderer renderer, int vertexCount)
	{
		if (renderer.mGPUDrivenGeometryOffset >= 0 && renderer.mGPUDrivenGeometryCapacity >= vertexCount)
		{
			return false;
		}
		releaseGeometryReservation(renderer);
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(MIN_GEOMETRY_BLOCK, vertexCount));
		if (!mFreeGeometryBlocks.TryGetValue(capacity, out Stack<int> freeBlocks))
		{
			freeBlocks = new Stack<int>();
			mFreeGeometryBlocks.Add(capacity, freeBlocks);
		}
		int offset;
		if (freeBlocks.Count > 0)
		{
			offset = freeBlocks.Pop();
		}
		else
		{
			offset = mGeometryHighWater;
			mGeometryHighWater += capacity;
			ensureGeometryCPUCapacity(mGeometryHighWater);
		}
		renderer.mGPUDrivenGeometryOffset = offset;
		renderer.mGPUDrivenGeometryCapacity = capacity;
		// Geometry reservation is an instance binding. If the backing GraphicsBuffer does not
		// reallocate, the existing commands still reference the same global buffers and remain
		// valid. uploadBuffers() marks commands dirty if a buffer really changes identity.
		return true;
	}

	private void releaseGeometryReservation(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		int offset = renderer.mGPUDrivenGeometryOffset;
		int capacity = renderer.mGPUDrivenGeometryCapacity;
		if (offset >= 0 && capacity > 0)
		{
			if (!mFreeGeometryBlocks.TryGetValue(capacity, out Stack<int> freeBlocks))
			{
				freeBlocks = new Stack<int>();
				mFreeGeometryBlocks.Add(capacity, freeBlocks);
			}
			freeBlocks.Push(offset);
		}
		renderer.mGPUDrivenGeometryOffset = -1;
		renderer.mGPUDrivenGeometryCapacity = 0;
		renderer.mGPUDrivenGeometryVertexCount = 0;
	}

	private void updateRendererTransform(FastSpriteRenderSystem owner, FastSpriteRenderer renderer)
	{
		int slot = renderer.mGPUDrivenSlot;
		if (slot < 0)
		{
			return;
		}
		ensureInstanceHotCapacity(slot + 1);
		owner.getGPUDrivenTransform(renderer, out Matrix4x4 localMatrix, out int rootIndex);
		var matrices = mInstanceHot.getLocalMatrixColumn();
		var roots = mInstanceHot.getRootIndexColumn();
		int dirtyMask = 0;
		if (matrices[slot] != localMatrix)
		{
			matrices[slot] = localMatrix;
			dirtyMask |= INSTANCE_DIRTY_MATRIX;
		}
		if (roots[slot] != rootIndex)
		{
			roots[slot] = rootIndex;
			dirtyMask |= INSTANCE_DIRTY_ROOT;
		}
		if (dirtyMask != 0)
		{
			markInstanceDirty(slot, dirtyMask);
		}
	}

	private void updateRendererColor(FastSpriteRenderer renderer)
	{
		int slot = renderer.mGPUDrivenSlot;
		if (slot < 0)
		{
			return;
		}
		ensureInstanceHotCapacity(slot + 1);
		Color color = renderer.getColor();
		Vector4 value = new(color.r, color.g, color.b, color.a);
		var colors = mInstanceHot.getColorColumn();
		if (colors[slot] == value)
		{
			return;
		}
		colors[slot] = value;
		markInstanceDirty(slot, INSTANCE_DIRTY_COLOR);
	}

	private void updateRendererBinding(FastSpriteRenderer renderer)
	{
		int slot = renderer.mGPUDrivenSlot;
		if (slot < 0)
		{
			return;
		}
		ensureInstanceHotCapacity(slot + 1);
		int geometryOffset = renderer.mGPUDrivenGeometryOffset;
		int textureSlotCode = getRendererTextureSlotCode(renderer);
		int flags = (renderer.getFlipX() ? INSTANCE_FLAG_FLIP_X : 0) |
			(renderer.getFlipY() ? INSTANCE_FLAG_FLIP_Y : 0) |
			(renderer.mRuntimeActiveState ? INSTANCE_FLAG_ACTIVE : 0) |
			(textureSlotCode << INSTANCE_TEXTURE_SLOT_SHIFT);
		var geometryOffsets = mInstanceHot.getGeometryOffsetColumn();
		var flagColumn = mInstanceHot.getFlagsColumn();
		int dirtyMask = 0;
		if (geometryOffsets[slot] != geometryOffset)
		{
			geometryOffsets[slot] = geometryOffset;
			dirtyMask |= INSTANCE_DIRTY_GEOMETRY;
		}
		if (flagColumn[slot] != flags)
		{
			flagColumn[slot] = flags;
			dirtyMask |= INSTANCE_DIRTY_FLAGS;
		}
		if (dirtyMask != 0)
		{
			markInstanceDirty(slot, dirtyMask);
		}
	}

	internal void beginPlan(int expectedInstances)
	{
		beginPlanCommon(expectedInstances);
		unchecked
		{
			++mPlanSpanCacheBuildStamp;
		}
		mBuildingFullPlanCache = true;
	}

	internal void beginCachedOrderPlan(int expectedInstances)
	{
		beginPlanCommon(expectedInstances);
		mBuildingFullPlanCache = false;
	}

	private void beginPlanCommon(int expectedInstances)
	{
		if (expectedInstances > mOrderECS.Count)
		{
			mOrderECS.EnsureCount(expectedInstances);
		}
		if (expectedInstances > mDrawCommands.Count)
		{
			mDrawCommands.EnsureCount(expectedInstances);
		}
		mOrderCount = 0;
		mDrawCommandCount = 0;
		mPlanUnsupported = false;
		mPlanValid = false;
	}

	internal bool appendSlotSpan(object spanKey, ref FastSpriteGPUPlanCachedSpan cacheHandle, Int_ECSList slotSpan, int sortingLayerID, int sortingOrder, bool retainInactiveSlots = false)
	{
		if (spanKey == null || slotSpan == null || slotSpan.Count == 0)
		{
			cacheHandle = null;
			return true;
		}

		if (!mPlanSpanCache.TryGetValue(spanKey, out FastSpriteGPUPlanCachedSpan cache))
		{
			cache = new FastSpriteGPUPlanCachedSpan();
			mPlanSpanCache.Add(spanKey, cache);
		}
		cacheHandle = cache;
		cache.beginBuild(slotSpan.Count, mLayoutRevision, mPlanSpanCacheBuildStamp);

		var spanSlots = slotSpan.getValueColumn();
		var active = mPlanSlots.getActiveColumn();
		var renderStateIDs = mPlanSlots.getRenderStateIDColumn();
		var orderSlots = mOrderECS.getValueColumn();
		int orderStart = mOrderCount;

		for (int i = 0; i < slotSpan.Count; ++i)
		{
			int slot = spanSlots[i];
			if ((uint)slot >= (uint)mPlanSlots.Count)
			{
				continue;
			}
			if (!retainInactiveSlots && active[slot] == 0)
			{
				continue;
			}
			int renderStateID = renderStateIDs[slot];
			if (renderStateID == 0)
			{
				continue;
			}
			if (renderStateID < 0)
			{
				mPlanUnsupported = true;
				return false;
			}

			orderSlots[mOrderCount++] = slot;
			cache.appendSlot(slot, renderStateID);
		}

		cache.mLastOrderStart = orderStart;
		return appendCachedRuns(cache, orderStart, sortingLayerID, sortingOrder);
	}

	internal bool appendCachedSlotSpan(object spanKey, ref FastSpriteGPUPlanCachedSpan cacheHandle, int sortingLayerID, int sortingOrder)
	{
		FastSpriteGPUPlanCachedSpan cache = cacheHandle;
		bool directHandleValid = cache != null &&
			cache.mLayoutRevision == mLayoutRevision &&
			cache.mBuildStamp == mPlanSpanCacheBuildStamp;
		if (!directHandleValid)
		{
			if (spanKey == null || !mPlanSpanCache.TryGetValue(spanKey, out cache) ||
				cache == null || cache.mLayoutRevision != mLayoutRevision ||
				cache.mBuildStamp != mPlanSpanCacheBuildStamp)
			{
				cacheHandle = null;
				return false;
			}
			cacheHandle = cache;
		}

		int orderStart = mOrderCount;
		int slotCount = cache.mSlotCount;
		bool retainedAtSameOffset = cache.mLastOrderStart == orderStart;
		if (retainedAtSameOffset)
		{
			// The authoritative cached span is structurally unchanged and the previous CPU order
			// mirror already contains these exact slots at this exact range. Advance the logical
			// cursor only; command runs are still rebuilt below so draw-boundary semantics stay
			// identical to the proven cached-order path.
			mOrderCount += slotCount;
		}
		else
		{
			var orderSlots = mOrderECS.getValueColumn();
			for (int i = 0; i < slotCount; ++i)
			{
				orderSlots[mOrderCount++] = cache.mSlots[i];
			}
		}
		cache.mLastOrderStart = orderStart;
		return appendCachedRuns(cache, orderStart, sortingLayerID, sortingOrder);
	}

	private bool appendCachedRuns(FastSpriteGPUPlanCachedSpan cache, int orderStart, int sortingLayerID, int sortingOrder)
	{
		if (cache == null)
		{
			return false;
		}
		var commandRenderStateIDs = mDrawCommands.getRenderStateIDColumn();
		var commandOrderStarts = mDrawCommands.getOrderStartColumn();
		var commandInstanceCounts = mDrawCommands.getInstanceCountColumn();
		var commandSortingLayers = mDrawCommands.getSortingLayerIDColumn();
		var commandFirstOrders = mDrawCommands.getFirstSortingOrderColumn();
		var commandLastOrders = mDrawCommands.getLastSortingOrderColumn();

		for (int runIndex = 0; runIndex < cache.mRunCount; ++runIndex)
		{
			FastSpriteGPUPlanCachedRun run = cache.mRuns[runIndex];
			if (run.mCount <= 0)
			{
				continue;
			}
			int drawIndex = mDrawCommandCount - 1;
			if (drawIndex >= 0)
			{
				bool sameState = commandRenderStateIDs[drawIndex] == run.mRenderStateID &&
					commandSortingLayers[drawIndex] == sortingLayerID;
				bool consecutiveOrder = sortingOrder <= commandLastOrders[drawIndex] + 1;
				if (sameState && consecutiveOrder)
				{
					commandInstanceCounts[drawIndex] += run.mCount;
					commandLastOrders[drawIndex] = sortingOrder;
					continue;
				}
			}

			int firstSlot = run.mFirstSlot;
			if ((uint)firstSlot >= (uint)mPlanSlots.Count)
			{
				return false;
			}
			FastSpriteGPUPlanSlotDataRef planRef = mPlanSlots[firstSlot];
			drawIndex = mDrawCommandCount++;
			FastSpriteGPUDrawCommandDataRef commandRef = mDrawCommands[drawIndex];
			commandRef.mMaterial = planRef.mMaterial;
			commandRef.mTexture = planRef.mTexture;
			commandRef.mMaterialID = planRef.mMaterialID;
			commandRef.mTextureID = planRef.mTextureID;
			commandRef.mVertexCount = planRef.mVertexCount;
			commandRenderStateIDs[drawIndex] = run.mRenderStateID;
			commandOrderStarts[drawIndex] = orderStart + run.mStart;
			commandInstanceCounts[drawIndex] = run.mCount;
			commandSortingLayers[drawIndex] = sortingLayerID;
			commandFirstOrders[drawIndex] = sortingOrder;
			commandLastOrders[drawIndex] = sortingOrder;
		}
		return true;
	}

	internal bool endPlan()
	{
		mPlanValid = !mPlanUnsupported && mOrderCount > 0 && mDrawCommandCount > 0;
		if (mPlanValid)
		{
			// Refresh proxy material properties once at the rebuilt-plan boundary rather than
			// once per draw. Existing proxy objects remain referenced by the CommandBuffer.
			if (!syncProxyMaterialsForCurrentPlan())
			{
				mPlanValid = false;
				return false;
			}
			if (mBuildingFullPlanCache)
			{
				pruneStalePlanSpanCaches();
			}
			mFrameStats.mDrawCount = mDrawCommandCount;
			mFrameStats.mInstanceCount = mOrderCount;
			mIndirectPlanEligible = buildMergedIndirectPlan(out mIndirectPlanMaterial);
			if (mIndirectPlanEligible)
			{
				mIndirectDataDirty = true;
				mFrameStats.mDrawCount = Mathf.Max(mIndirectRecordedDrawCount, mIndirectMergedCommandCount);
			}
			else if (!commandLayoutMatchesCommitted())
			{
				mCommandDirty = true;
			}
			mOrderDirty = true;
		}
		else
		{
			mIndirectPlanEligible = false;
			mIndirectPlanMaterial = null;
			mIndirectDataDirty = false;
			mIndirectMergedCommandCount = 0;
		}
		return mPlanValid;
	}

	private void pruneStalePlanSpanCaches()
	{
		if (mPlanSpanCache.Count == 0)
		{
			return;
		}
		mPlanSpanCachePruneKeys.Clear();
		foreach (KeyValuePair<object, FastSpriteGPUPlanCachedSpan> pair in mPlanSpanCache)
		{
			if (pair.Key == null || pair.Value == null || pair.Value.mBuildStamp != mPlanSpanCacheBuildStamp)
			{
				mPlanSpanCachePruneKeys.Add(pair.Key);
			}
		}
		for (int i = 0; i < mPlanSpanCachePruneKeys.Count; ++i)
		{
			object key = mPlanSpanCachePruneKeys[i];
			if (key != null)
			{
				mPlanSpanCache.Remove(key);
			}
		}
		mPlanSpanCachePruneKeys.Clear();
	}

	internal bool uploadAndBuildCommands(Camera camera, GraphicsBuffer rootBuffer, int rootCount)
	{
		if (!mPlanValid || !isRuntimeSupported() || camera == null)
		{
			return false;
		}
		long uploadStart = System.Diagnostics.Stopwatch.GetTimestamp();
		if (!uploadBuffers())
		{
			return false;
		}

		bool wantIndirect = mSubmissionMode != FastSpriteBackendMode.GPUDrivenDirect && mIndirectPlanEligible;
		if (wantIndirect)
		{
			// Keep an exact persistent command-slot high-water only while the same indirect material
			// remains active. Re-entering the indirect lane after a Direct fallback starts from the
			// current exact merged count, so transient incompatible workloads cannot permanently
			// inflate future physical draws.
			bool continuingSession = mUsingPersistentIndirect &&
				ReferenceEquals(mIndirectPlanMaterial, mBoundIndirectPlanMaterial);
			int desiredRecordedDrawCount = continuingSession
				? Mathf.Max(mIndirectRecordedDrawCount, mIndirectMergedCommandCount)
				: mIndirectMergedCommandCount;
			if (desiredRecordedDrawCount != mIndirectRecordedDrawCount)
			{
				mIndirectRecordedDrawCount = desiredRecordedDrawCount;
				mIndirectDataDirty = true;
				mCommandDirty = true;
			}

			if (!ensureIndirectCommandResources(mIndirectRecordedDrawCount))
			{
				// Allocation/API fallback keeps the validated direct path authoritative.
				wantIndirect = false;
				mIndirectPlanEligible = false;
				mIndirectPlanMaterial = null;
				mCommandDirty = true;
			}
			else if (mIndirectDataDirty)
			{
				uploadIndirectCommandData();
			}
		}
		mFrameStats.mUploadMS += elapsedMS(uploadStart);

		GraphicsBuffer actualRootBuffer = rootBuffer;
		if (actualRootBuffer == null)
		{
			ensureFallbackRootBuffer();
			actualRootBuffer = mFallbackRootBuffer;
			rootCount = 0;
		}
		if (mUsingPersistentIndirect != wantIndirect)
		{
			mCommandDirty = true;
		}
		if (wantIndirect && mUsingPersistentIndirect && !ReferenceEquals(mIndirectPlanMaterial, mBoundIndirectPlanMaterial))
		{
			mCommandDirty = true;
		}
		if (!ReferenceEquals(actualRootBuffer, mBoundRootBuffer) || mBoundRootCount != rootCount)
		{
			mCommandDirty = true;
		}
		if (camera != mCommandCamera)
		{
			mCommandDirty = true;
		}
		if (!mCommandDirty)
		{
			ensureCommandBufferAttached(camera);
			return true;
		}

		long commandStart = System.Diagnostics.Stopwatch.GetTimestamp();
		bool built = wantIndirect ? rebuildIndirectCommandBuffer(camera, actualRootBuffer, rootCount) :
									rebuildCommandBuffer(camera, actualRootBuffer, rootCount);
		if (!built)
		{
			return false;
		}
		mFrameStats.mCommandBuildMS += elapsedMS(commandStart);
		return true;
	}

	private bool ensureIndirectCommandResources(int requiredDrawCount)
	{
		if (!SystemInfo.supportsComputeShaders || requiredDrawCount <= 0)
		{
			return false;
		}
		int requiredCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredDrawCount, 1));
		if (mIndirectArgsBuffer != null && mIndirectOrderBaseBuffer != null &&
			mIndirectCommandCapacity >= requiredCapacity)
		{
			return true;
		}

		try
		{
			mIndirectArgsBuffer?.Dispose();
			mIndirectOrderBaseBuffer?.Dispose();
			mIndirectArgsBuffer = new GraphicsBuffer(
				GraphicsBuffer.Target.IndirectArguments,
				requiredCapacity * INDIRECT_ARGS_PER_DRAW,
				INDIRECT_ARG_STRIDE);
			mIndirectOrderBaseBuffer = new GraphicsBuffer(
				GraphicsBuffer.Target.Structured,
				requiredCapacity,
				ORDER_STRIDE);
			mIndirectCommandCapacity = requiredCapacity;
			if (mIndirectArgsCPU.Length < requiredCapacity * INDIRECT_ARGS_PER_DRAW)
			{
				Array.Resize(ref mIndirectArgsCPU, requiredCapacity * INDIRECT_ARGS_PER_DRAW);
			}
			if (mIndirectOrderBasesCPU.Length < requiredCapacity)
			{
				Array.Resize(ref mIndirectOrderBasesCPU, requiredCapacity);
			}
			mIndirectDataDirty = true;
			mCommandDirty = true;
			return true;
		}
		catch
		{
			mIndirectArgsBuffer?.Dispose();
			mIndirectArgsBuffer = null;
			mIndirectOrderBaseBuffer?.Dispose();
			mIndirectOrderBaseBuffer = null;
			mIndirectCommandCapacity = 0;
			mIndirectRecordedDrawCount = 0;
			return false;
		}
	}

	private void uploadIndirectCommandData()
	{
		if (mIndirectArgsBuffer == null || mIndirectOrderBaseBuffer == null ||
			mIndirectCommandCapacity <= 0 || mIndirectRecordedDrawCount <= 0)
		{
			return;
		}
		int recordedCount = Mathf.Min(mIndirectRecordedDrawCount, mIndirectCommandCapacity);
		int argsCount = recordedCount * INDIRECT_ARGS_PER_DRAW;
		Array.Clear(mIndirectArgsCPU, 0, argsCount);
		Array.Clear(mIndirectOrderBasesCPU, 0, recordedCount);

		int count = Mathf.Min(mIndirectMergedCommandCount, recordedCount);
		for (int i = 0; i < count; ++i)
		{
			int baseIndex = i * INDIRECT_ARGS_PER_DRAW;
			mIndirectArgsCPU[baseIndex] = (uint)Mathf.Max(0, mIndirectMergedVertexCounts[i]);
			mIndirectArgsCPU[baseIndex + 1] = (uint)Mathf.Max(0, mIndirectMergedInstanceCounts[i]);
			mIndirectArgsCPU[baseIndex + 2] = 0u;
			// startInstance must remain zero for OpenGL ES compatibility; orderBase lives in
			// a separate StructuredBuffer indexed by the fixed indirect command slot.
			mIndirectArgsCPU[baseIndex + 3] = 0u;
			mIndirectOrderBasesCPU[i] = mIndirectMergedOrderStarts[i];
		}
		mIndirectArgsBuffer.SetData(mIndirectArgsCPU, 0, 0, argsCount);
		mIndirectOrderBaseBuffer.SetData(mIndirectOrderBasesCPU, 0, 0, recordedCount);
		mFrameStats.mOrderUploadBytes += argsCount * INDIRECT_ARG_STRIDE +
			recordedCount * ORDER_STRIDE;
		mFrameStats.mOrderUploadCalls += 2;
		mIndirectDataDirty = false;
	}

	private bool uploadBuffers()
	{
		int geometryRequired = Mathf.NextPowerOfTwo(Mathf.Max(mGeometryHighWater, 1));
		if (mGeometryBuffer == null || mGeometryBufferCapacity < geometryRequired)
		{
			mGeometryBuffer?.Dispose();
			mGeometryBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, geometryRequired, VERTEX_STRIDE);
			mGeometryBufferCapacity = geometryRequired;
			if (mGeometryHighWater > 0)
			{
				mGeometryBuffer.SetData(mGeometryCPU, 0, 0, mGeometryHighWater);
				mFrameStats.mGeometryUploadBytes += mGeometryHighWater * VERTEX_STRIDE;
				++mFrameStats.mGeometryUploadCalls;
			}
			mGeometryDirtyMin = int.MaxValue;
			mGeometryDirtyMax = -1;
			mCommandDirty = true;
		}
		else if (mGeometryDirtyMax >= mGeometryDirtyMin)
		{
			int count = mGeometryDirtyMax - mGeometryDirtyMin + 1;
			mGeometryBuffer.SetData(mGeometryCPU, mGeometryDirtyMin, mGeometryDirtyMin, count);
			mFrameStats.mGeometryUploadBytes += count * VERTEX_STRIDE;
			++mFrameStats.mGeometryUploadCalls;
			mGeometryDirtyMin = int.MaxValue;
			mGeometryDirtyMax = -1;
		}

		int quadRequired = Mathf.NextPowerOfTwo(Mathf.Max(mQuadAssetHighWater, 1));
		if (mQuadAssetBuffer == null || mQuadAssetBufferCapacity < quadRequired)
		{
			mQuadAssetBuffer?.Dispose();
			mQuadAssetBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, quadRequired, QUAD_ASSET_STRIDE);
			mQuadAssetBufferCapacity = quadRequired;
			if (mQuadAssetHighWater > 0)
			{
				mQuadAssetBuffer.SetData(mQuadAssetCPU, 0, 0, mQuadAssetHighWater);
				mFrameStats.mGeometryUploadBytes += mQuadAssetHighWater * QUAD_ASSET_STRIDE;
				++mFrameStats.mGeometryUploadCalls;
			}
			mQuadAssetDirtyMin = int.MaxValue;
			mQuadAssetDirtyMax = -1;
			mCommandDirty = true;
		}
		else if (mQuadAssetDirtyMax >= mQuadAssetDirtyMin)
		{
			int count = mQuadAssetDirtyMax - mQuadAssetDirtyMin + 1;
			mQuadAssetBuffer.SetData(mQuadAssetCPU, mQuadAssetDirtyMin, mQuadAssetDirtyMin, count);
			mFrameStats.mGeometryUploadBytes += count * QUAD_ASSET_STRIDE;
			++mFrameStats.mGeometryUploadCalls;
			mQuadAssetDirtyMin = int.MaxValue;
			mQuadAssetDirtyMax = -1;
		}

		int instanceRequired = Mathf.NextPowerOfTwo(Mathf.Max(mInstanceHighWater, 1));
		if (mLocalMatrixBuffer == null || mInstanceBufferCapacity < instanceRequired)
		{
			disposeInstanceBuffers();
			mLocalMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceRequired, MATRIX_STRIDE);
			mColorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceRequired, COLOR_STRIDE);
			mGeometryOffsetBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceRequired, INT_STRIDE);
			mRootIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceRequired, INT_STRIDE);
			mFlagsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceRequired, INT_STRIDE);
			mInstanceBufferCapacity = instanceRequired;
			if (mInstanceHighWater > 0)
			{
				uploadInstanceRange(INSTANCE_DIRTY_ALL, 0, mInstanceHighWater);
			}
			clearInstanceDirtySlots();
			clearInstanceScatterCountsIfNeeded();
			mCommandDirty = true;
		}
		else
		{
			uploadDirtyInstances();
		}

		int orderRequired = Mathf.NextPowerOfTwo(Mathf.Max(mOrderCount, 1));
		bool orderReallocated = mOrderBuffer == null || mOrderBufferCapacity < orderRequired;
		if (orderReallocated)
		{
			mOrderBuffer?.Dispose();
			mOrderBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, orderRequired, ORDER_STRIDE);
			mOrderBufferCapacity = orderRequired;
			mCommandDirty = true;
		}

		if (orderReallocated)
		{
			mOrderDirty = true;
		}

		if (mOrderDirty && mOrderCount > 0)
		{
			int orderCount = mOrderCount;
			var orderColumn = mOrderECS.getValueColumn();
			if (Int_ECSList.IsUnsafeBackend)
			{
				ref int first = ref orderColumn[0];
				NativeArray<int> view = FastUINativeArrayBridge.createView(ref first, orderCount);
				mOrderBuffer.SetData(view, 0, 0, orderCount);
			}
			else
			{
				if (mOrderFallbackCPU.Length < orderCount)
				{
					Array.Resize(ref mOrderFallbackCPU, Mathf.NextPowerOfTwo(Mathf.Max(orderCount, 256)));
				}
				for (int i = 0; i < orderCount; ++i)
				{
					mOrderFallbackCPU[i] = orderColumn[i];
				}
				mOrderBuffer.SetData(mOrderFallbackCPU, 0, 0, orderCount);
			}
			mFrameStats.mOrderUploadBytes += orderCount * ORDER_STRIDE;
			++mFrameStats.mOrderUploadCalls;
			mOrderDirty = false;
		}
		return mGeometryBuffer != null && mQuadAssetBuffer != null && mLocalMatrixBuffer != null && mColorBuffer != null && mGeometryOffsetBuffer != null && mRootIndexBuffer != null && mFlagsBuffer != null && mOrderBuffer != null;
	}

	private void uploadDirtyInstances()
	{
		if (mDirtyInstanceSlots.Count == 0 || mLocalMatrixBuffer == null)
		{
			clearInstanceScatterCountsIfNeeded();
			return;
		}

		bool scatterSupported = canUseInstanceScatter();
		int dirtyCount = mDirtyInstanceSlots.Count;
		var dirtySlots = mDirtyInstanceSlots.getValueColumn();
		var masks = mInstanceDirtyMaskBySlot.getValueColumn();

		int matrixMin = int.MaxValue, matrixMax = -1, matrixDirtyCount = 0;
		int colorMin = int.MaxValue, colorMax = -1, colorDirtyCount = 0;
		int geometryMin = int.MaxValue, geometryMax = -1, geometryDirtyCount = 0;
		int rootMin = int.MaxValue, rootMax = -1, rootDirtyCount = 0;
		int flagsMin = int.MaxValue, flagsMax = -1, flagsDirtyCount = 0;

		bool useChunkDensity = dirtyCount >= DIRTY_DENSITY_CHUNK_MIN_COUNT &&
			BurstCompiler.IsEnabled &&
			Int_ECSList.IsUnsafeBackend &&
			FastSpriteDirtyChunkStats_ECSList.IsUnsafeBackend;
		if (useChunkDensity)
		{
			int chunkSize = DIRTY_DENSITY_CHUNK_SIZE;
			int chunkCount = (dirtyCount + chunkSize - 1) / chunkSize;
			mDirtyChunkStats.EnsureCount(chunkCount);
			mInstanceDirtyMaskBySlot.CompleteBurstJobs();
			mDirtyChunkStats.CompleteBurstJobs();

			Int_ECSList.BurstView dirtyView = mDirtyInstanceSlots.GetBurstView();
			Int_ECSList.BurstView maskView = mInstanceDirtyMaskBySlot.GetBurstView();
			FastSpriteDirtyChunkStats_ECSList.BurstView statsView = mDirtyChunkStats.GetBurstView();
			DirtyDensityChunkJob job = new()
			{
				mDirtySlots = dirtyView,
				mMasks = maskView,
				mStats = statsView,
				mChunkSize = chunkSize,
				mInstanceHighWater = mInstanceHighWater,
			};
			JobHandle handle = mDirtyInstanceSlots.ScheduleBurstChunk(job, chunkSize);
			mInstanceDirtyMaskBySlot.RegisterBurstJob(handle);
			mDirtyChunkStats.RegisterBurstJob(handle);
			mDirtyInstanceSlots.CompleteBurstJobs();
			mInstanceDirtyMaskBySlot.CompleteBurstJobs();
			mDirtyChunkStats.CompleteBurstJobs();

			var matrixMins = mDirtyChunkStats.getMatrixMinColumn();
			var matrixMaxs = mDirtyChunkStats.getMatrixMaxColumn();
			var matrixCounts = mDirtyChunkStats.getMatrixCountColumn();
			var colorMins = mDirtyChunkStats.getColorMinColumn();
			var colorMaxs = mDirtyChunkStats.getColorMaxColumn();
			var colorCounts = mDirtyChunkStats.getColorCountColumn();
			var geometryMins = mDirtyChunkStats.getGeometryMinColumn();
			var geometryMaxs = mDirtyChunkStats.getGeometryMaxColumn();
			var geometryCounts = mDirtyChunkStats.getGeometryCountColumn();
			var rootMins = mDirtyChunkStats.getRootMinColumn();
			var rootMaxs = mDirtyChunkStats.getRootMaxColumn();
			var rootCounts = mDirtyChunkStats.getRootCountColumn();
			var flagsMins = mDirtyChunkStats.getFlagsMinColumn();
			var flagsMaxs = mDirtyChunkStats.getFlagsMaxColumn();
			var flagsCounts = mDirtyChunkStats.getFlagsCountColumn();

			for (int chunkIndex = 0; chunkIndex < chunkCount; ++chunkIndex)
			{
				int count = matrixCounts[chunkIndex];
				if (count > 0)
				{
					matrixMin = Mathf.Min(matrixMin, matrixMins[chunkIndex]);
					matrixMax = Mathf.Max(matrixMax, matrixMaxs[chunkIndex]);
					matrixDirtyCount += count;
				}
				count = colorCounts[chunkIndex];
				if (count > 0)
				{
					colorMin = Mathf.Min(colorMin, colorMins[chunkIndex]);
					colorMax = Mathf.Max(colorMax, colorMaxs[chunkIndex]);
					colorDirtyCount += count;
				}
				count = geometryCounts[chunkIndex];
				if (count > 0)
				{
					geometryMin = Mathf.Min(geometryMin, geometryMins[chunkIndex]);
					geometryMax = Mathf.Max(geometryMax, geometryMaxs[chunkIndex]);
					geometryDirtyCount += count;
				}
				count = rootCounts[chunkIndex];
				if (count > 0)
				{
					rootMin = Mathf.Min(rootMin, rootMins[chunkIndex]);
					rootMax = Mathf.Max(rootMax, rootMaxs[chunkIndex]);
					rootDirtyCount += count;
				}
				count = flagsCounts[chunkIndex];
				if (count > 0)
				{
					flagsMin = Mathf.Min(flagsMin, flagsMins[chunkIndex]);
					flagsMax = Mathf.Max(flagsMax, flagsMaxs[chunkIndex]);
					flagsDirtyCount += count;
				}
			}
			++mFrameStats.mDirtyDensityChunkDispatchCount;
		}
		else
		{
			for (int i = 0; i < dirtyCount; ++i)
			{
				int slot = dirtySlots[i];
				if ((uint)slot >= (uint)mInstanceHighWater)
				{
					continue;
				}
				int mask = masks[slot];
				if ((mask & INSTANCE_DIRTY_MATRIX) != 0)
				{
					matrixMin = Mathf.Min(matrixMin, slot);
					matrixMax = Mathf.Max(matrixMax, slot);
					++matrixDirtyCount;
				}
				if ((mask & INSTANCE_DIRTY_COLOR) != 0)
				{
					colorMin = Mathf.Min(colorMin, slot);
					colorMax = Mathf.Max(colorMax, slot);
					++colorDirtyCount;
				}
				if ((mask & INSTANCE_DIRTY_GEOMETRY) != 0)
				{
					geometryMin = Mathf.Min(geometryMin, slot);
					geometryMax = Mathf.Max(geometryMax, slot);
					++geometryDirtyCount;
				}
				if ((mask & INSTANCE_DIRTY_ROOT) != 0)
				{
					rootMin = Mathf.Min(rootMin, slot);
					rootMax = Mathf.Max(rootMax, slot);
					++rootDirtyCount;
				}
				if ((mask & INSTANCE_DIRTY_FLAGS) != 0)
				{
					flagsMin = Mathf.Min(flagsMin, slot);
					flagsMax = Mathf.Max(flagsMax, slot);
					++flagsDirtyCount;
				}
			}
		}

		bool scatterMatrix = scatterSupported && shouldScatterColumn(matrixDirtyCount, matrixMin, matrixMax, MATRIX_PATCH_STRIDE, MATRIX_STRIDE);
		bool scatterColor = scatterSupported && shouldScatterColumn(colorDirtyCount, colorMin, colorMax, COLOR_PATCH_STRIDE, COLOR_STRIDE);
		bool scatterGeometry = scatterSupported && shouldScatterColumn(geometryDirtyCount, geometryMin, geometryMax, SCALAR_PATCH_STRIDE, INT_STRIDE);
		bool scatterRoot = scatterSupported && shouldScatterColumn(rootDirtyCount, rootMin, rootMax, SCALAR_PATCH_STRIDE, INT_STRIDE);
		bool scatterFlags = scatterSupported && shouldScatterColumn(flagsDirtyCount, flagsMin, flagsMax, SCALAR_PATCH_STRIDE, INT_STRIDE);
		bool anyScatter = scatterMatrix || scatterColor || scatterGeometry || scatterRoot || scatterFlags;

		int matrixPatchCount = 0;
		int colorPatchCount = 0;
		int geometryPatchCount = 0;
		int rootPatchCount = 0;
		int flagsPatchCount = 0;

		if (anyScatter)
		{
			ensureInstancePatchCPUCapacity(dirtyCount);
			var matrices = mInstanceHot.getLocalMatrixColumn();
			var colors = mInstanceHot.getColorColumn();
			var geometryOffsets = mInstanceHot.getGeometryOffsetColumn();
			var roots = mInstanceHot.getRootIndexColumn();
			var flags = mInstanceHot.getFlagsColumn();

			for (int i = 0; i < dirtyCount; ++i)
			{
				int slot = dirtySlots[i];
				if ((uint)slot >= (uint)mInstanceHighWater)
				{
					continue;
				}
				int mask = masks[slot];
				if (scatterMatrix && (mask & INSTANCE_DIRTY_MATRIX) != 0)
				{
					mMatrixPatchCPU[matrixPatchCount++] = new FastSpriteGPUMatrixPatch(slot, matrices[slot]);
				}
				if (scatterColor && (mask & INSTANCE_DIRTY_COLOR) != 0)
				{
					mColorPatchCPU[colorPatchCount++] = new FastSpriteGPUColorPatch(slot, colors[slot]);
				}
				if (scatterGeometry && (mask & INSTANCE_DIRTY_GEOMETRY) != 0)
				{
					mGeometryPatchCPU[geometryPatchCount++] = new FastSpriteGPUScalarPatch(slot, geometryOffsets[slot]);
				}
				if (scatterRoot && (mask & INSTANCE_DIRTY_ROOT) != 0)
				{
					mRootPatchCPU[rootPatchCount++] = new FastSpriteGPUScalarPatch(slot, roots[slot]);
				}
				if (scatterFlags && (mask & INSTANCE_DIRTY_FLAGS) != 0)
				{
					mFlagsPatchCPU[flagsPatchCount++] = new FastSpriteGPUScalarPatch(slot, flags[slot]);
				}
			}
		}

		if (!scatterMatrix && matrixMax >= matrixMin)
		{
			uploadInstanceRange(INSTANCE_DIRTY_MATRIX, matrixMin, matrixMax - matrixMin + 1);
		}
		if (!scatterColor && colorMax >= colorMin)
		{
			uploadInstanceRange(INSTANCE_DIRTY_COLOR, colorMin, colorMax - colorMin + 1);
		}
		if (!scatterGeometry && geometryMax >= geometryMin)
		{
			uploadInstanceRange(INSTANCE_DIRTY_GEOMETRY, geometryMin, geometryMax - geometryMin + 1);
		}
		if (!scatterRoot && rootMax >= rootMin)
		{
			uploadInstanceRange(INSTANCE_DIRTY_ROOT, rootMin, rootMax - rootMin + 1);
		}
		if (!scatterFlags && flagsMax >= flagsMin)
		{
			uploadInstanceRange(INSTANCE_DIRTY_FLAGS, flagsMin, flagsMax - flagsMin + 1);
		}

		if (anyScatter)
		{
			uploadInstancePatches(matrixPatchCount, colorPatchCount, geometryPatchCount, rootPatchCount, flagsPatchCount);
		}
		else
		{
			clearInstanceScatterCountsIfNeeded();
		}

		clearInstanceDirtySlots();
	}

	private static bool shouldScatterColumn(int dirtyCount, int minSlot, int maxSlot, int patchStride, int rangeStride)
	{
		if (dirtyCount <= 0 || maxSlot < minSlot)
		{
			return false;
		}
		long patchBytes = (long)dirtyCount * patchStride;
		long rangeBytes = (long)(maxSlot - minSlot + 1) * rangeStride;
		return patchBytes + SCATTER_MIN_SAVED_BYTES < rangeBytes;
	}

	private void ensureInstancePatchCPUCapacity(int required)
	{
		if (required <= 0)
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(required, 64));
		if (mMatrixPatchCPU.Length < required)
		{
			Array.Resize(ref mMatrixPatchCPU, capacity);
		}
		if (mColorPatchCPU.Length < required)
		{
			Array.Resize(ref mColorPatchCPU, capacity);
		}
		if (mGeometryPatchCPU.Length < required)
		{
			Array.Resize(ref mGeometryPatchCPU, capacity);
		}
		if (mRootPatchCPU.Length < required)
		{
			Array.Resize(ref mRootPatchCPU, capacity);
		}
		if (mFlagsPatchCPU.Length < required)
		{
			Array.Resize(ref mFlagsPatchCPU, capacity);
		}
	}

	private void ensureInstanceScatterBuffers(int required)
	{
		if (required <= 0 || !canUseInstanceScatter())
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(required, 64));
		if (mMatrixPatchBuffer != null && mColorPatchBuffer != null &&
			mGeometryPatchBuffer != null && mRootPatchBuffer != null && mFlagsPatchBuffer != null &&
			mScatterCountBuffer != null && mInstancePatchBufferCapacity >= capacity)
		{
			return;
		}

		mMatrixPatchBuffer?.Dispose();
		mColorPatchBuffer?.Dispose();
		mGeometryPatchBuffer?.Dispose();
		mRootPatchBuffer?.Dispose();
		mFlagsPatchBuffer?.Dispose();
		mMatrixPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, MATRIX_PATCH_STRIDE);
		mColorPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, COLOR_PATCH_STRIDE);
		mGeometryPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, SCALAR_PATCH_STRIDE);
		mRootPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, SCALAR_PATCH_STRIDE);
		mFlagsPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, SCALAR_PATCH_STRIDE);
		mInstancePatchBufferCapacity = capacity;

		if (mScatterCountBuffer == null)
		{
			mScatterCountBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 5, SCATTER_COUNT_STRIDE);
			for (int i = 0; i < mScatterCountsCPU.Length; ++i)
			{
				mScatterCountsCPU[i] = 0;
			}
			mScatterCountBuffer.SetData(mScatterCountsCPU, 0, 0, 5);
			mFrameStats.mInstanceUploadBytes += 5 * SCATTER_COUNT_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		mCommandDirty = true;
	}

	private void uploadInstancePatches(int matrixCount, int colorCount, int geometryCount, int rootCount, int flagsCount)
	{
		int maxCount = Mathf.Max(matrixCount,
			Mathf.Max(colorCount, Mathf.Max(geometryCount, Mathf.Max(rootCount, flagsCount))));
		if (maxCount <= 0)
		{
			clearInstanceScatterCountsIfNeeded();
			return;
		}

		ensureInstanceScatterBuffers(maxCount);
		if (mMatrixPatchBuffer == null || mColorPatchBuffer == null ||
			mGeometryPatchBuffer == null || mRootPatchBuffer == null || mFlagsPatchBuffer == null ||
			mScatterCountBuffer == null)
		{
			return;
		}

		if (matrixCount > 0)
		{
			mMatrixPatchBuffer.SetData(mMatrixPatchCPU, 0, 0, matrixCount);
			mFrameStats.mInstanceUploadBytes += matrixCount * MATRIX_PATCH_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if (colorCount > 0)
		{
			mColorPatchBuffer.SetData(mColorPatchCPU, 0, 0, colorCount);
			mFrameStats.mInstanceUploadBytes += colorCount * COLOR_PATCH_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if (geometryCount > 0)
		{
			mGeometryPatchBuffer.SetData(mGeometryPatchCPU, 0, 0, geometryCount);
			mFrameStats.mInstanceUploadBytes += geometryCount * SCALAR_PATCH_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if (rootCount > 0)
		{
			mRootPatchBuffer.SetData(mRootPatchCPU, 0, 0, rootCount);
			mFrameStats.mInstanceUploadBytes += rootCount * SCALAR_PATCH_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if (flagsCount > 0)
		{
			mFlagsPatchBuffer.SetData(mFlagsPatchCPU, 0, 0, flagsCount);
			mFrameStats.mInstanceUploadBytes += flagsCount * SCALAR_PATCH_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		setInstanceScatterCounts(matrixCount, colorCount, geometryCount, rootCount, flagsCount);
	}

	private void setInstanceScatterCounts(int matrixCount, int colorCount, int geometryCount, int rootCount, int flagsCount)
	{
		if (mScatterCountBuffer == null)
		{
			return;
		}
		if (mScatterCountsCPU[0] == matrixCount && mScatterCountsCPU[1] == colorCount &&
			mScatterCountsCPU[2] == geometryCount && mScatterCountsCPU[3] == rootCount &&
			mScatterCountsCPU[4] == flagsCount)
		{
			return;
		}
		mScatterCountsCPU[0] = matrixCount;
		mScatterCountsCPU[1] = colorCount;
		mScatterCountsCPU[2] = geometryCount;
		mScatterCountsCPU[3] = rootCount;
		mScatterCountsCPU[4] = flagsCount;
		mScatterCountBuffer.SetData(mScatterCountsCPU, 0, 0, 5);
		mFrameStats.mInstanceUploadBytes += 5 * SCATTER_COUNT_STRIDE;
		++mFrameStats.mInstanceUploadCalls;
	}

	private void clearInstanceScatterCountsIfNeeded()
	{
		if (mScatterCountBuffer == null)
		{
			return;
		}
		int combined = 0;
		for (int i = 0; i < mScatterCountsCPU.Length; ++i)
		{
			combined |= mScatterCountsCPU[i];
		}
		if (combined == 0)
		{
			return;
		}
		setInstanceScatterCounts(0, 0, 0, 0, 0);
	}

	private void uploadInstanceRange(int mask, int start, int count)
	{
		if (count <= 0)
		{
			return;
		}
		if ((mask & INSTANCE_DIRTY_MATRIX) != 0)
		{
			var column = mInstanceHot.getLocalMatrixColumn();
			ref Matrix4x4 first = ref column[start];
			NativeArray<Matrix4x4> view = FastUINativeArrayBridge.createView(ref first, count);
			mLocalMatrixBuffer.SetData(view, 0, start, count);
			mFrameStats.mInstanceUploadBytes += count * MATRIX_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if ((mask & INSTANCE_DIRTY_COLOR) != 0)
		{
			var column = mInstanceHot.getColorColumn();
			ref Vector4 first = ref column[start];
			NativeArray<Vector4> view = FastUINativeArrayBridge.createView(ref first, count);
			mColorBuffer.SetData(view, 0, start, count);
			mFrameStats.mInstanceUploadBytes += count * COLOR_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if ((mask & INSTANCE_DIRTY_GEOMETRY) != 0)
		{
			var column = mInstanceHot.getGeometryOffsetColumn();
			ref int first = ref column[start];
			NativeArray<int> view = FastUINativeArrayBridge.createView(ref first, count);
			mGeometryOffsetBuffer.SetData(view, 0, start, count);
			mFrameStats.mInstanceUploadBytes += count * INT_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if ((mask & INSTANCE_DIRTY_ROOT) != 0)
		{
			var column = mInstanceHot.getRootIndexColumn();
			ref int first = ref column[start];
			NativeArray<int> view = FastUINativeArrayBridge.createView(ref first, count);
			mRootIndexBuffer.SetData(view, 0, start, count);
			mFrameStats.mInstanceUploadBytes += count * INT_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
		if ((mask & INSTANCE_DIRTY_FLAGS) != 0)
		{
			var column = mInstanceHot.getFlagsColumn();
			ref int first = ref column[start];
			NativeArray<int> view = FastUINativeArrayBridge.createView(ref first, count);
			mFlagsBuffer.SetData(view, 0, start, count);
			mFrameStats.mInstanceUploadBytes += count * INT_STRIDE;
			++mFrameStats.mInstanceUploadCalls;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private MaterialPropertyBlock getPropertyBlock()
	{
		if (mPropertyBlock == null)
		{
			mPropertyBlock = new MaterialPropertyBlock();
		}
		return mPropertyBlock;
	}

	private bool rebuildCommandBuffer(Camera camera, GraphicsBuffer rootBuffer, int rootCount)
	{
		ensureCommandBufferAttached(camera);
		if (mCommandBuffer == null || mGeometryBuffer == null || mQuadAssetBuffer == null || mLocalMatrixBuffer == null || mColorBuffer == null || mGeometryOffsetBuffer == null || mRootIndexBuffer == null || mFlagsBuffer == null || mOrderBuffer == null)
		{
			return false;
		}
		mCommandBuffer.Clear();

		recordInstanceScatterCommands();

		mCommandBuffer.SetGlobalBuffer(GEOMETRY_ID, mGeometryBuffer);
		mCommandBuffer.SetGlobalBuffer(QUAD_ASSETS_ID, mQuadAssetBuffer);
		mCommandBuffer.SetGlobalBuffer(LOCAL_MATRICES_ID, mLocalMatrixBuffer);
		mCommandBuffer.SetGlobalBuffer(COLORS_ID, mColorBuffer);
		mCommandBuffer.SetGlobalBuffer(GEOMETRY_OFFSETS_ID, mGeometryOffsetBuffer);
		mCommandBuffer.SetGlobalBuffer(ROOT_INDICES_ID, mRootIndexBuffer);
		mCommandBuffer.SetGlobalBuffer(FLAGS_ID, mFlagsBuffer);
		mCommandBuffer.SetGlobalBuffer(ORDER_ID, mOrderBuffer);
		mCommandBuffer.SetGlobalBuffer(INDIRECT_ORDER_BASES_ID, mOrderBuffer);
		mCommandBuffer.SetGlobalBuffer(ROOT_MATRICES_ID, rootBuffer);
		mCommandBuffer.SetGlobalInt(ROOT_MATRIX_COUNT_ID, rootCount);
		recordVirtualTextureBindings();

		MaterialPropertyBlock propertyBlock = getPropertyBlock();
		var orderStarts = mDrawCommands.getOrderStartColumn();
		var instanceCounts = mDrawCommands.getInstanceCountColumn();
		for (int i = 0; i < mDrawCommandCount; ++i)
		{
			FastSpriteGPUDrawCommandDataRef commandRef = mDrawCommands[i];
			Material proxy = getProxyMaterial(commandRef.mMaterial);
			if (proxy == null)
			{
				return false;
			}
			propertyBlock.Clear();
			if (commandRef.mTexture != null)
			{
				propertyBlock.SetTexture(MAIN_TEX_ID, commandRef.mTexture);
			}
			propertyBlock.SetInt(USE_INDIRECT_ORDER_BASE_ID, 0);
			propertyBlock.SetInt(ORDER_BASE_ID, orderStarts[i]);
			mCommandBuffer.DrawProcedural(
				Matrix4x4.identity,
				proxy,
				0,
				MeshTopology.Triangles,
				commandRef.mVertexCount,
				instanceCounts[i],
				propertyBlock);
		}
		mBoundRootBuffer = rootBuffer;
		mBoundRootCount = rootCount;
		mUsingPersistentIndirect = false;
		mBoundIndirectPlanMaterial = null;
		captureCommittedCommandLayout();
		mCommandDirty = false;
		mFrameStats.mDrawCount = mDrawCommandCount;
		mFrameStats.mInstanceCount = mOrderCount;
		return true;
	}

	private bool rebuildIndirectCommandBuffer(Camera camera, GraphicsBuffer rootBuffer, int rootCount)
	{
		ensureCommandBufferAttached(camera);
		if (mCommandBuffer == null || mGeometryBuffer == null || mQuadAssetBuffer == null ||
			mLocalMatrixBuffer == null || mColorBuffer == null || mGeometryOffsetBuffer == null ||
			mRootIndexBuffer == null || mFlagsBuffer == null || mOrderBuffer == null ||
			mIndirectArgsBuffer == null || mIndirectOrderBaseBuffer == null ||
			mIndirectRecordedDrawCount <= 0 || mIndirectPlanMaterial == null)
		{
			return false;
		}

		Material proxy = getProxyMaterial(mIndirectPlanMaterial);
		if (proxy == null)
		{
			return false;
		}
		MaterialPropertyBlock propertyBlock = getPropertyBlock();

		mCommandBuffer.Clear();
		recordInstanceScatterCommands();
		mCommandBuffer.SetGlobalBuffer(GEOMETRY_ID, mGeometryBuffer);
		mCommandBuffer.SetGlobalBuffer(QUAD_ASSETS_ID, mQuadAssetBuffer);
		mCommandBuffer.SetGlobalBuffer(LOCAL_MATRICES_ID, mLocalMatrixBuffer);
		mCommandBuffer.SetGlobalBuffer(COLORS_ID, mColorBuffer);
		mCommandBuffer.SetGlobalBuffer(GEOMETRY_OFFSETS_ID, mGeometryOffsetBuffer);
		mCommandBuffer.SetGlobalBuffer(ROOT_INDICES_ID, mRootIndexBuffer);
		mCommandBuffer.SetGlobalBuffer(FLAGS_ID, mFlagsBuffer);
		mCommandBuffer.SetGlobalBuffer(ORDER_ID, mOrderBuffer);
		mCommandBuffer.SetGlobalBuffer(ROOT_MATRICES_ID, rootBuffer);
		mCommandBuffer.SetGlobalInt(ROOT_MATRIX_COUNT_ID, rootCount);
		mCommandBuffer.SetGlobalBuffer(INDIRECT_ORDER_BASES_ID, mIndirectOrderBaseBuffer);
		recordVirtualTextureBindings();

		for (int i = 0; i < mIndirectRecordedDrawCount; ++i)
		{
			propertyBlock.Clear();
			propertyBlock.SetInt(USE_INDIRECT_ORDER_BASE_ID, 1);
			propertyBlock.SetInt(INDIRECT_COMMAND_INDEX_ID, i);
			mCommandBuffer.DrawProceduralIndirect(
				Matrix4x4.identity,
				proxy,
				0,
				MeshTopology.Triangles,
				mIndirectArgsBuffer,
				i * INDIRECT_ARGS_BYTES_PER_DRAW,
				propertyBlock);
		}

		mBoundRootBuffer = rootBuffer;
		mBoundRootCount = rootCount;
		mUsingPersistentIndirect = true;
		mBoundIndirectPlanMaterial = mIndirectPlanMaterial;
		mCommittedCommandCount = -1;
		mCommandDirty = false;
		// Zero-instance high-water slots are still real recorded submissions on the target driver.
		// Report the exact recorded high-water, never the power-of-two GPU buffer capacity.
		mFrameStats.mDrawCount = mIndirectRecordedDrawCount;
		mFrameStats.mInstanceCount = mOrderCount;
		return true;
	}

	private void recordInstanceScatterCommands()
	{
		if (!canUseInstanceScatter() || mInstanceScatterShader == null || mInstanceScatterKernel < 0 ||
			mMatrixPatchBuffer == null || mColorPatchBuffer == null ||
			mGeometryPatchBuffer == null || mRootPatchBuffer == null || mFlagsPatchBuffer == null ||
			mScatterCountBuffer == null || mLocalMatrixBuffer == null || mColorBuffer == null ||
			mGeometryOffsetBuffer == null || mRootIndexBuffer == null || mFlagsBuffer == null)
		{
			return;
		}

		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_MATRIX_PATCHES_ID, mMatrixPatchBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_COLOR_PATCHES_ID, mColorPatchBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_GEOMETRY_PATCHES_ID, mGeometryPatchBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_ROOT_PATCHES_ID, mRootPatchBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_FLAGS_PATCHES_ID, mFlagsPatchBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, SCATTER_COUNTS_ID, mScatterCountBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, LOCAL_MATRICES_ID, mLocalMatrixBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, COLORS_ID, mColorBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, GEOMETRY_OFFSETS_ID, mGeometryOffsetBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, ROOT_INDICES_ID, mRootIndexBuffer);
		mCommandBuffer.SetComputeBufferParam(mInstanceScatterShader, mInstanceScatterKernel, FLAGS_ID, mFlagsBuffer);

		int groups = Mathf.Max(1, (mInstancePatchBufferCapacity + SCATTER_THREAD_GROUP_SIZE - 1) / SCATTER_THREAD_GROUP_SIZE);
		mCommandBuffer.DispatchCompute(mInstanceScatterShader, mInstanceScatterKernel, groups, 1, 1);
	}

	private Material getProxyMaterial(Material original)
	{
		if (original == null || !isMaterialCompatible(original))
		{
			return null;
		}
		if (!mProxyMaterials.TryGetValue(original, out Material proxy) || proxy == null)
		{
			if (mShader == null)
			{
				mShader = Resources.Load<Shader>("FastSpriteGPUDriven");
			}
			if (mShader == null)
			{
				return null;
			}
			proxy = new Material(mShader)
			{
				name = "[FastSpriteGPUDriven] " + original.name,
				hideFlags = HideFlags.HideAndDontSave,
			};
			mProxyMaterials[original] = proxy;
		}
		return proxy;
	}

	private bool syncProxyMaterialsForCurrentPlan()
	{
		mProxyMaterialsSyncedThisPlan.Clear();
		for (int i = 0; i < mDrawCommandCount; ++i)
		{
			FastSpriteGPUDrawCommandDataRef commandRef = mDrawCommands[i];
			Material source = commandRef.mMaterial;
			if (source == null || !mProxyMaterialsSyncedThisPlan.Add(source))
			{
				continue;
			}
			Material proxy = getProxyMaterial(source);
			if (proxy == null)
			{
				return false;
			}
			proxy.CopyPropertiesFromMaterial(source);
		}
		return true;
	}

	private bool commandLayoutMatchesCommitted()
	{
		if (mCommittedCommandCount != mDrawCommandCount || mDrawCommandCount <= 0)
		{
			return false;
		}
		var renderStateIDs = mDrawCommands.getRenderStateIDColumn();
		var orderStarts = mDrawCommands.getOrderStartColumn();
		var instanceCounts = mDrawCommands.getInstanceCountColumn();
		for (int i = 0; i < mDrawCommandCount; ++i)
		{
			if (mCommittedCommandRenderStateIDs[i] != renderStateIDs[i] ||
				mCommittedCommandOrderStarts[i] != orderStarts[i] ||
				mCommittedCommandInstanceCounts[i] != instanceCounts[i])
			{
				return false;
			}
		}
		return true;
	}

	private void captureCommittedCommandLayout()
	{
		int count = mDrawCommandCount;
		if (count <= 0)
		{
			mCommittedCommandCount = -1;
			return;
		}
		if (mCommittedCommandRenderStateIDs.Length < count)
		{
			int capacity = Mathf.NextPowerOfTwo(Mathf.Max(count, 32));
			Array.Resize(ref mCommittedCommandRenderStateIDs, capacity);
			Array.Resize(ref mCommittedCommandOrderStarts, capacity);
			Array.Resize(ref mCommittedCommandInstanceCounts, capacity);
		}
		var renderStateIDs = mDrawCommands.getRenderStateIDColumn();
		var orderStarts = mDrawCommands.getOrderStartColumn();
		var instanceCounts = mDrawCommands.getInstanceCountColumn();
		for (int i = 0; i < count; ++i)
		{
			mCommittedCommandRenderStateIDs[i] = renderStateIDs[i];
			mCommittedCommandOrderStarts[i] = orderStarts[i];
			mCommittedCommandInstanceCounts[i] = instanceCounts[i];
		}
		mCommittedCommandCount = count;
	}

	private void ensureCommandBufferAttached(Camera camera)
	{
		if (camera == null)
		{
			return;
		}
		if (mCommandBuffer == null)
		{
			mCommandBuffer = new CommandBuffer
			{
				name = "FastSprite GPU Driven Indirect"
			};
		}
		if (mCommandCamera != camera)
		{
			if (mCommandCamera != null)
			{
				detachCommandBufferFromCamera(mCommandCamera);
			}
			mCommandCamera = camera;
		}
		attachCommandBufferToCamera(camera);
#if UNITY_EDITOR
		attachCommandBufferToSceneViews();
#endif
	}

	private void attachCommandBufferToCamera(Camera camera)
	{
		if (camera == null || mCommandBuffer == null)
		{
			return;
		}
		for (int i = 0; i < mAttachedCommandCameras.Count; ++i)
		{
			Camera attachedCamera = mAttachedCommandCameras[i];
			if (attachedCamera == camera)
			{
				return;
			}
		}
		camera.AddCommandBuffer(COMMAND_EVENT, mCommandBuffer);
		mAttachedCommandCameras.Add(camera);
	}

	private void detachCommandBufferFromCamera(Camera camera)
	{
		if (camera == null || mCommandBuffer == null)
		{
			return;
		}
		camera.RemoveCommandBuffer(COMMAND_EVENT, mCommandBuffer);
		for (int i = mAttachedCommandCameras.Count - 1; i >= 0; --i)
		{
			Camera attachedCamera = mAttachedCommandCameras[i];
			if (attachedCamera == null || attachedCamera == camera)
			{
				mAttachedCommandCameras.RemoveAt(i);
			}
		}
	}

#if UNITY_EDITOR
	private void attachCommandBufferToSceneViews()
	{
		if (!Application.isPlaying || mCommandBuffer == null)
		{
			return;
		}
		for (int i = mAttachedCommandCameras.Count - 1; i >= 0; --i)
		{
			Camera attachedCamera = mAttachedCommandCameras[i];
			if (attachedCamera == null)
			{
				mAttachedCommandCameras.RemoveAt(i);
			}
		}
		foreach (SceneView sceneView in SceneView.sceneViews)
		{
			if (sceneView == null)
			{
				continue;
			}
			Camera sceneCamera = sceneView.camera;
			if (sceneCamera == null)
			{
				continue;
			}
			attachCommandBufferToCamera(sceneCamera);
		}
	}
#endif

	internal void detachCommandBuffer()
	{
		if (mCommandBuffer != null)
		{
			for (int i = mAttachedCommandCameras.Count - 1; i >= 0; --i)
			{
				Camera camera = mAttachedCommandCameras[i];
				if (camera != null)
				{
					camera.RemoveCommandBuffer(COMMAND_EVENT, mCommandBuffer);
				}
			}
		}
		mAttachedCommandCameras.Clear();
		mCommandCamera = null;
	}

	internal void invalidatePlan()
	{
		mPlanValid = false;
		mCommandDirty = true;
		mOrderDirty = true;
		mCommandBuffer?.Clear();
	}

	private void ensureFallbackRootBuffer()
	{
		if (mFallbackRootBuffer != null)
		{
			return;
		}
		mFallbackRootBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, 64);
		mFallbackRootBuffer.SetData(new[]
		{
			Matrix4x4.identity
		});
	}

	private void ensureGeometryCPUCapacity(int required)
	{
		if (required <= mGeometryCPU.Length)
		{
			return;
		}
		Array.Resize(ref mGeometryCPU, Mathf.NextPowerOfTwo(Mathf.Max(required, 64)));
	}

	private void ensureQuadAssetCPUCapacity(int required)
	{
		if (required <= mQuadAssetCPU.Length)
		{
			return;
		}
		Array.Resize(ref mQuadAssetCPU, Mathf.NextPowerOfTwo(Mathf.Max(required, 16)));
	}

	private void ensureInstanceHotCapacity(int required)
	{
		if (required <= 0)
		{
			return;
		}
		int oldCount = mInstanceHot.Count;
		if (oldCount < required)
		{
			mInstanceHot.EnsureCount(required);
			var geometryOffsets = mInstanceHot.getGeometryOffsetColumn();
			var roots = mInstanceHot.getRootIndexColumn();
			for (int i = oldCount; i < required; ++i)
			{
				geometryOffsets[i] = -1;
				roots[i] = -1;
			}
		}
		if (mInstanceDirtyMaskBySlot.Count < required)
		{
			mInstanceDirtyMaskBySlot.EnsureCount(required);
		}
		if (mInstanceDirtyQueuedBySlot.Count < required)
		{
			mInstanceDirtyQueuedBySlot.EnsureCount(required);
		}
	}

	private void markGeometryDirty(int start, int count)
	{
		if (count <= 0)
		{
			return;
		}
		mGeometryDirtyMin = Mathf.Min(mGeometryDirtyMin, start);
		mGeometryDirtyMax = Mathf.Max(mGeometryDirtyMax, start + count - 1);
	}

	private void markInstanceDirty(int slot, int mask)
	{
		if (slot < 0 || mask == 0)
		{
			return;
		}
		if ((uint)slot >= (uint)mInstanceDirtyMaskBySlot.Count)
		{
			ensureInstanceHotCapacity(slot + 1);
		}
		var masks = mInstanceDirtyMaskBySlot.getValueColumn();
		var queued = mInstanceDirtyQueuedBySlot.getValueColumn();
		masks[slot] |= mask;
		if (queued[slot])
		{
			return;
		}
		queued[slot] = true;
		mDirtyInstanceSlots.Add(slot);
	}

	private void clearInstanceDirtySlots()
	{
		var slots = mDirtyInstanceSlots.getValueColumn();
		var masks = mInstanceDirtyMaskBySlot.getValueColumn();
		var queued = mInstanceDirtyQueuedBySlot.getValueColumn();
		for (int i = 0; i < mDirtyInstanceSlots.Count; ++i)
		{
			int slot = slots[i];
			if ((uint)slot >= (uint)mInstanceDirtyMaskBySlot.Count)
			{
				continue;
			}
			masks[slot] = 0;
			queued[slot] = false;
		}
		mDirtyInstanceSlots.Clear();
	}

	private void removeDirtyRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || !renderer.mGPUDrivenDirtyQueued)
		{
			return;
		}
		for (int i = 0; i < mDirtyRenderers.Count; ++i)
		{
			if (!ReferenceEquals(mDirtyRenderers[i], renderer))
			{
				continue;
			}
			// Cold unregister path: leave a null tombstone; the frame consumer skips it and
			// no List.Remove/memmove is needed.
			mDirtyRenderers[i] = null;
			break;
		}
		renderer.mGPUDrivenDirtyQueued = false;
		renderer.mGPUDrivenDirtyFlags = FastSpriteDirtyFlags.None;
	}

	internal bool isPlanSlotDrawable(int slot)
	{
		if ((uint)slot >= (uint)mPlanSlots.Count)
		{
			return false;
		}
		return mPlanSlots.getActiveColumn()[slot] != 0 && mPlanSlots.getRenderStateIDColumn()[slot] > 0;
	}

	internal bool validateActiveOrder(FastSpriteRenderSystem owner, out string error)
	{
		error = null;
		if (!mPlanValid)
		{
			error = "GPU-driven plan is not valid.";
			return false;
		}
		var orderSlots = mOrderECS.getValueColumn();
		var active = mPlanSlots.getActiveColumn();
		var renderStateIDs = mPlanSlots.getRenderStateIDColumn();
		for (int i = 0; i < mOrderCount; ++i)
		{
			int slot = orderSlots[i];
			if ((uint)slot >= (uint)mSlotOwners.Length || (uint)slot >= (uint)mPlanSlots.Count || renderStateIDs[slot] <= 0)
			{
				error = "GPU-driven invalid persistent slot at order index " + i + " Slot=" + slot;
				return false;
			}
			FastSpriteRenderer renderer = mSlotOwners[slot];
			if (renderer == null || renderer.mSystemOwner != owner || renderer.mGPUDrivenSlot != slot)
			{
				error = "GPU-driven owner mismatch at order index " + i + " Slot=" + slot;
				return false;
			}
			if (active[slot] == 0 && !renderer.mRetainedWhileDisabled)
			{
				error = "GPU-driven non-retained inactive slot at order index " + i + " Slot=" + slot;
				return false;
			}
		}
		return true;
	}

	private void disposeInstanceScatterBuffers()
	{
		mMatrixPatchBuffer?.Dispose();
		mMatrixPatchBuffer = null;
		mColorPatchBuffer?.Dispose();
		mColorPatchBuffer = null;
		mGeometryPatchBuffer?.Dispose();
		mGeometryPatchBuffer = null;
		mRootPatchBuffer?.Dispose();
		mRootPatchBuffer = null;
		mFlagsPatchBuffer?.Dispose();
		mFlagsPatchBuffer = null;
		mScatterCountBuffer?.Dispose();
		mScatterCountBuffer = null;
		mInstancePatchBufferCapacity = 0;
		for (int i = 0; i < mScatterCountsCPU.Length; ++i)
		{
			mScatterCountsCPU[i] = 0;
		}
	}

	private void disposeInstanceBuffers()
	{
		mLocalMatrixBuffer?.Dispose();
		mLocalMatrixBuffer = null;
		mColorBuffer?.Dispose();
		mColorBuffer = null;
		mGeometryOffsetBuffer?.Dispose();
		mGeometryOffsetBuffer = null;
		mRootIndexBuffer?.Dispose();
		mRootIndexBuffer = null;
		mFlagsBuffer?.Dispose();
		mFlagsBuffer = null;
	}

	private static double elapsedMS(long start)
	{
		return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
	}

	public void Dispose()
	{
		if (mDisposed)
		{
			return;
		}
		mDisposed = true;
		detachCommandBuffer();
		mCommandBuffer?.Release();
		mCommandBuffer = null;
		mGeometryBuffer?.Dispose();
		mGeometryBuffer = null;
		mQuadAssetBuffer?.Dispose();
		mQuadAssetBuffer = null;
		disposeInstanceBuffers();
		disposeInstanceScatterBuffers();
		mInstanceScatterShader = null;
		mInstanceScatterKernel = -1;
		mInstanceScatterSupportState = 0;
		mOrderBuffer?.Dispose();
		mOrderBuffer = null;
		mIndirectArgsBuffer?.Dispose();
		mIndirectArgsBuffer = null;
		mIndirectOrderBaseBuffer?.Dispose();
		mIndirectOrderBaseBuffer = null;
		mIndirectCommandCapacity = 0;
		mIndirectRecordedDrawCount = 0;
		mIndirectArgsCPU = Array.Empty<uint>();
		mIndirectOrderBasesCPU = Array.Empty<int>();
		mIndirectPlanEligible = false;
		mIndirectDataDirty = false;
		mUsingPersistentIndirect = false;
		mIndirectPlanMaterial = null;
		mBoundIndirectPlanMaterial = null;
		mVirtualTextureSlots.Clear();
		Array.Clear(mVirtualTextures, 0, mVirtualTextures.Length);
		Array.Clear(mVirtualTextureRefCounts, 0, mVirtualTextureRefCounts.Length);
		mTextureSlotCodeBySlot = Array.Empty<int>();
		mFallbackRootBuffer?.Dispose();
		mFallbackRootBuffer = null;
		foreach (Material proxy in mProxyMaterials.Values)
		{
			if (proxy == null)
			{
				continue;
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(proxy);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(proxy);
			}
		}
		mProxyMaterials.Clear();
		mProxyMaterialsSyncedThisPlan.Clear();
		mPropertyBlock = null;
		mCommittedCommandRenderStateIDs = Array.Empty<int>();
		mCommittedCommandOrderStarts = Array.Empty<int>();
		mCommittedCommandInstanceCounts = Array.Empty<int>();
		mCommittedCommandCount = -1;
		mDirtyRenderers.Clear();
		mPlanSlots.Dispose();
		mInstanceHot.Dispose();
		mOrderECS.Dispose();
		mDrawCommands.Dispose();
		mOrderCount = 0;
		mDrawCommandCount = 0;
		mDirtyInstanceSlots.CompleteBurstJobs();
		mInstanceDirtyMaskBySlot.CompleteBurstJobs();
		mDirtyChunkStats.CompleteBurstJobs();
		mDirtyInstanceSlots.Dispose();
		mInstanceDirtyMaskBySlot.Dispose();
		mInstanceDirtyQueuedBySlot.Dispose();
		mDirtyChunkStats.Dispose();
		mQuadAssetIndices.Clear();
		mQuadAssetIndexBySimpleAssetID = Array.Empty<int>();
		Array.Clear(mQuadAssetLookupSprites, 0, mQuadAssetLookupSprites.Length);
		Array.Clear(mQuadAssetLookupIndices, 0, mQuadAssetLookupIndices.Length);
		mRenderStateIDs.Clear();
		mPlanSpanCache.Clear();
		mPlanSpanCachePruneKeys.Clear();
		mFreeGeometryBlocks.Clear();
		mFreeInstanceSlots.Clear();
	}
}
