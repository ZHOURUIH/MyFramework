using System;
using UnityEngine.Rendering;

public enum FastUIMaskMaterialMode : byte
{
	None,
	Reader,
	Writer,
	Pop,
}
[Serializable]
public struct FastUIMaskState : IEquatable<FastUIMaskState>
{
	public FastUIMaskMaterialMode mMode;
	public byte mDepth;
	public byte mStencilRef;
	public byte mReadMask;
	public byte mWriteMask;
	public byte mColorMask;
	public CompareFunction mCompareFunction;
	public StencilOp mStencilOp;
	public bool mUseAlphaClip;
	public readonly bool Equals(FastUIMaskState other)
	{
		return mMode == other.mMode && mDepth == other.mDepth && mStencilRef == other.mStencilRef &&
			mReadMask == other.mReadMask && mWriteMask == other.mWriteMask && mColorMask == other.mColorMask &&
			mCompareFunction == other.mCompareFunction && mStencilOp == other.mStencilOp && mUseAlphaClip == other.mUseAlphaClip;
	}
	public override bool Equals(object obj)
	{
		return obj is FastUIMaskState other && Equals(other);
	}
	public override readonly int GetHashCode()
	{
		unchecked
		{
			int hash = (int)mMode;
			hash = hash * 397 ^ mDepth;
			hash = hash * 397 ^ mStencilRef;
			hash = hash * 397 ^ mReadMask;
			hash = hash * 397 ^ mWriteMask;
			hash = hash * 397 ^ mColorMask;
			hash = hash * 397 ^ (int)mCompareFunction;
			hash = hash * 397 ^ (int)mStencilOp;
			hash = hash * 397 ^ (mUseAlphaClip ? 1 : 0);
			return hash;
		}
	}
	public static bool operator ==(FastUIMaskState left, FastUIMaskState right)
	{
		return left.Equals(right);
	}
	public static bool operator !=(FastUIMaskState left, FastUIMaskState right)
	{
		return !left.Equals(right);
	}
	public static FastUIMaskState none()
	{
		return default;
	}
	public static FastUIMaskState reader(int depth)
	{
		if (depth <= 0)
		{
			return default;
		}
		int mask = (1 << Math.Min(depth, FastMaskUtility.MAX_MASK_DEPTH)) - 1;
		return new FastUIMaskState
		{
			mMode = FastUIMaskMaterialMode.Reader,
			mDepth = (byte)depth,
			mStencilRef = (byte)mask,
			mReadMask = (byte)mask,
			mWriteMask = 0,
			mColorMask = 15,
			mCompareFunction = CompareFunction.Equal,
			mStencilOp = StencilOp.Keep,
			mUseAlphaClip = false,
		};
	}
	public static FastUIMaskState writer(int parentDepth, bool showMaskGraphic)
	{
		int parentMask = parentDepth <= 0 ? 0 : (1 << parentDepth) - 1;
		int bit = 1 << parentDepth;
		int fullMask = parentMask | bit;
		return new FastUIMaskState
		{
			mMode = FastUIMaskMaterialMode.Writer,
			mDepth = (byte)(parentDepth + 1),
			mStencilRef = (byte)fullMask,
			mReadMask = (byte)parentMask,
			mWriteMask = (byte)bit,
			mColorMask = (byte)(showMaskGraphic ? 15 : 0),
			mCompareFunction = parentDepth == 0 ? CompareFunction.Always : CompareFunction.Equal,
			mStencilOp = StencilOp.Replace,
			mUseAlphaClip = true,
		};
	}
	public static FastUIMaskState pop(int parentDepth)
	{
		int parentMask = parentDepth <= 0 ? 0 : (1 << parentDepth) - 1;
		int bit = 1 << parentDepth;
		int fullMask = parentMask | bit;
		return new FastUIMaskState
		{
			mMode = FastUIMaskMaterialMode.Pop,
			mDepth = (byte)(parentDepth + 1),
			mStencilRef = (byte)fullMask,
			mReadMask = (byte)fullMask,
			mWriteMask = (byte)bit,
			mColorMask = 0,
			mCompareFunction = CompareFunction.Equal,
			mStencilOp = StencilOp.Zero,
			mUseAlphaClip = true,
		};
	}
}
