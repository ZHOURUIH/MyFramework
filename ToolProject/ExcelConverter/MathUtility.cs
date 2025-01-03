using System;
using static StringUtility;

public class MathUtility
{
	public static void clampMin(ref int value, int min)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clamp(ref int value, int min, int max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
	}
	public static bool isFloatZero(float value, float precision = 0.0001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isFloatEqual(float value1, float value2, float precision = 0.0001f)
	{
		return isFloatZero(value1 - value2, precision);
	}
	public static int ceil(float value)
	{
		int intValue = (int)value;
		if (isFloatEqual(intValue, value))
		{
			return intValue;
		}
		if (value >= 0.0f && value > intValue)
		{
			++intValue;
		}
		return intValue;
	}
	public static int generateAlignTableCount(string str, int alignWidth)
	{
		int remainChar = alignWidth - getStringWidth(str);
		clampMin(ref remainChar, 0);
		return ceil(remainChar / 4.0f);
	}
}