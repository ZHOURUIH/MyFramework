
// long的扩展方法
public static class LongExtension
{
	public static long abs(this long value) { return value >= 0 ? value : -value; }
	public static long clamp(this long value, long min, long max)
	{
		if (min > max)
		{
			return value;
		}
		if (min == max)
		{
			return min;
		}
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
		return value;
	}
	public static long clampMin(this long value, long min = 0) { return value < min ? min : value; }
	public static long clampMax(this long value, long max) { return value > max ? max : value; }
	public static float divide(this long value0, float value1, float defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static float divide(this long value0, long value1, float defaultValue = 0)
	{
		return value1 != 0 ? value0 / (float)value1 : defaultValue;
	}
	public static long divideLong(this long value0, long value1, long defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static ulong clampMin(this ulong value, ulong min = 0) { return value < min ? min : value; }
	public static ulong clampMax(this ulong value, ulong max) { return value > max ? max : value; }
}