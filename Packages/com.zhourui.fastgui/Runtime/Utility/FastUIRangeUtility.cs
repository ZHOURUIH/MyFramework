using UnityEngine;

// RenderRange相关无状态工具。
// 负责Range合并、追加和边界处理，不持有Canvas运行时状态。

public static class FastUIRangeUtility
{
	public static void addRange(FastUIRangeData_ECSList ranges, int start, int count, int maxCount)
	{
		if (ranges == null || count <= 0 || maxCount <= 0)
		{
			return;
		}
		int rangeStart = Mathf.Clamp(start, 0, maxCount);
		int rangeEnd = Mathf.Clamp(start + count, rangeStart, maxCount);
		if (rangeEnd <= rangeStart)
		{
			return;
		}
		if (ranges.Count > 0)
		{
			FastUIRangeDataRef last = ranges[^1];
			// ：addRange不再假设调用顺序单调递增。
			// 半开区间只有真正重叠或相邻时才能与最后一个Range即时合并；
			// 逆序加入且彼此远离的Range必须分别保留，之后交给SortFast+merge统一处理。
			if (rangeStart <= last.mEnd && rangeEnd >= last.mStart)
			{
				last.mStart = Mathf.Min(last.mStart, rangeStart);
				last.mEnd = Mathf.Max(last.mEnd, rangeEnd);
				return;
			}
		}
		ranges.Add(new FastUIRangeData(rangeStart, rangeEnd));
	}
	public static void copy(FastUIRangeData_ECSList source, FastUIRangeData_ECSList target)
	{
		target.Clear();
		int count = source.Count;
		if (count <= 0)
		{
			return;
		}
		var starts = source.getStartColumn();
		var ends = source.getEndColumn();
		for (int i = 0; i < count; ++i)
		{
			target.Add(new FastUIRangeData(starts[i], ends[i]));
		}
	}
	public static bool isSame(FastUIRangeData_ECSList first, FastUIRangeData_ECSList second)
	{
		int count = first.Count;
		if (count != second.Count)
		{
			return false;
		}
		var firstStarts = first.getStartColumn();
		var firstEnds = first.getEndColumn();
		var secondStarts = second.getStartColumn();
		var secondEnds = second.getEndColumn();
		for (int i = 0; i < count; ++i)
		{
			if (firstStarts[i] != secondStarts[i] || firstEnds[i] != secondEnds[i])
			{
				return false;
			}
		}
		return true;
	}
	public static void merge(FastUIRangeData_ECSList ranges)
	{
		int count = ranges.Count;
		if (count <= 1)
		{
			return;
		}
		ranges.SortFast();
		var starts = ranges.getStartColumn();
		var ends = ranges.getEndColumn();
		int writeIndex = 0;
		for (int readIndex = 1; readIndex < count; ++readIndex)
		{
			if (starts[readIndex] <= ends[writeIndex])
			{
				if (ends[readIndex] > ends[writeIndex])
				{
					ends[writeIndex] = ends[readIndex];
				}
				continue;
			}
			++writeIndex;
			starts[writeIndex] = starts[readIndex];
			ends[writeIndex] = ends[readIndex];
		}
		int mergedCount = writeIndex + 1;
		if (ranges.Count > mergedCount)
		{
			ranges.RemoveRange(mergedCount, ranges.Count - mergedCount);
		}
	}
}
