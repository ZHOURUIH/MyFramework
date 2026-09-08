using EasyECS;
// FastUI只保留EasyECS当前没有提供，或者还需要先做FastUI实测性能门槛的List算法。
// RemoveRange/InsertRange等结构操作直接使用EasyECS生成API。
// SortFast暂时保留Direct Column路径；只有在EasyECS内置Sort达到同等性能后才移除。
public static class FastUIECSListUtility
{
	public static void SortFast(this Int_ECSList list)
	{
		int count = list.Count;
		if (count <= 1)
		{
			return;
		}
		var values = list.getValueColumn();
		for (int start = (count - 2) >> 1; start >= 0; --start)
		{
			int root = start;
			while (true)
			{
				int child = root * 2 + 1;
				if (child >= count)
				{
					break;
				}
				if (child + 1 < count && values[child] < values[child + 1])
				{
					++child;
				}
				if (values[root] >= values[child])
				{
					break;
				}
				(values[child], values[root]) = (values[root], values[child]);
				root = child;
			}
		}
		for (int end = count - 1; end > 0; --end)
		{
			(values[0], values[end]) = (values[end], values[0]);
			int root = 0;
			while (true)
			{
				int child = root * 2 + 1;
				if (child >= end)
				{
					break;
				}
				if (child + 1 < end && values[child] < values[child + 1])
				{
					++child;
				}
				if (values[root] >= values[child])
				{
					break;
				}
				(values[child], values[root]) = (values[root], values[child]);
				root = child;
			}
		}
	}
	public static void SortFast(this FastUIRangeData_ECSList list)
	{
		int count = list.Count;
		if (count <= 1)
		{
			return;
		}
		var starts = list.getStartColumn();
		var ends = list.getEndColumn();
		for (int start = (count - 2) >> 1; start >= 0; --start)
		{
			int root = start;
			while (true)
			{
				int child = root * 2 + 1;
				if (child >= count)
				{
					break;
				}
				if (child + 1 < count && starts[child] < starts[child + 1])
				{
					++child;
				}
				if (starts[root] >= starts[child])
				{
					break;
				}
				int startValue = starts[root];
				int endValue = ends[root];
				starts[root] = starts[child];
				ends[root] = ends[child];
				starts[child] = startValue;
				ends[child] = endValue;
				root = child;
			}
		}
		for (int end = count - 1; end > 0; --end)
		{
			int startValue = starts[end];
			int endValue = ends[end];
			starts[end] = starts[0];
			ends[end] = ends[0];
			starts[0] = startValue;
			ends[0] = endValue;
			int root = 0;
			while (true)
			{
				int child = root * 2 + 1;
				if (child >= end)
				{
					break;
				}
				if (child + 1 < end && starts[child] < starts[child + 1])
				{
					++child;
				}
				if (starts[root] >= starts[child])
				{
					break;
				}
				int swapStart = starts[root];
				int swapEnd = ends[root];
				starts[root] = starts[child];
				ends[root] = ends[child];
				starts[child] = swapStart;
				ends[child] = swapEnd;
				root = child;
			}
		}
	}
}
