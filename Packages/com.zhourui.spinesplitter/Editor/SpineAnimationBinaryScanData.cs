using System.Collections.Generic;

// Spine二进制文件中单个动画的原始字节范围,起始位置包含动画名称,结束位置指向动画数据之后。
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

// Spine二进制扫描结果,保存Skeleton Hash、共享字符串表、动画区位置以及每个动画的原始字节范围。
public sealed class SpineBinaryScanResult
{
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
