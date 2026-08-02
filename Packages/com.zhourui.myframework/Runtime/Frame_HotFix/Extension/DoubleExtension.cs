
// double的扩展方法
public static class DoubleExtension
{
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(this double value)
	{
		int intValue = (int)value;
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
	public static long round(this double value)
	{
		if (value > 0.0)
		{
			return (long)(value + 0.5);
		}
		else
		{
			return (long)(value - 0.5);
		}
	}
	public static double abs(this double value) { return value >= 0.0 ? value : -value; }
	public static double clampMin(this double value, double min = 0.0) { return value < min ? min : value; }
	public static double clampMax(this double value, double max) { return value > max ? max : value; }
	public static bool isZero(this double value, double precision = 0.00000001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isEqual(this double value1, double value2, double precision = 0.00000001f)
	{
		return isZero(value1 - value2, precision);
	}
	public static double inverse(this double value)
	{
		if (value.isZero())
		{
			return 0.0;
		}
		return 1.0 / value;
	}
	public static double divide(this double value0, double value1, double defaultValue = 0.0f)
	{
		return !isZero(value1) ? value0 / value1 : defaultValue;
	}
}