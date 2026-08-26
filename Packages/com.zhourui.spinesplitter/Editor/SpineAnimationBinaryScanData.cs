using System.Collections.Generic;

public enum SpineSourceDataFormat
{
    Binary,
    Json,
}

// Spine源文件中单个动画的数据范围。二进制源记录真实偏移，JSON源使用序列化后的动画payload估算范围。
public sealed class SpineAnimationBinaryRange
{
    public int mIndex;
    public string mName;
    public long mStartPosition;
    public long mEndPosition;
    public long mLength
    {
        get
        {
            return mEndPosition - mStartPosition;
        }
    }
}

// Spine扫描结果,同时兼容二进制与JSON源资源。
public sealed class SpineBinaryScanResult
{
    public SpineSourceDataFormat mSourceFormat = SpineSourceDataFormat.Binary;
    public long mSkeletonHash;
    public string mVersion;
    public string[] mStrings;
    public long mFileLength;
    public long mAnimationCountPosition;
    public long mAnimationDataPosition;
    public int mSlotCount;
    public int[] mRequiredAnimationIndices = new int[0];
    public readonly List<SpineAnimationBinaryRange> mAnimations = new List<SpineAnimationBinaryRange>();
}
