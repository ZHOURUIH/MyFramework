using System;

// Spine Skeleton Hash兼容工具。二进制资源通常是Int64字符串，JSON资源可能是Base64样式字符串。
public static class SpineSkeletonHashUtility
{
    public static long getStableHash(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0L;
        }
        if (long.TryParse(value, out long numericHash))
        {
            return numericHash;
        }
        unchecked
        {
            const ulong offsetBasis = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offsetBasis;
            for (int i = 0; i < value.Length; ++i)
            {
                hash ^= value[i];
                hash *= prime;
            }
            return (long)hash;
        }
    }
}
