using UnityEngine;
using static MathUtility;

// float的扩展方法
public static class FloatExtension
{
	// 得到数轴上浮点数右边的第一个整数,向上取整
	public static int ceil(this float value)
	{
		int intValue = (int)value;
		if (isEqual(intValue, value))
		{
			return intValue;
		}
		if (value >= 0.0f && value > intValue)
		{
			++intValue;
		}
		return intValue;
	}
	public static bool isNaN(this float value) { return float.IsNaN(value); }
	public static float saturate(this float value) { return value.clamp(0.0f, 1.0f); }
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(this float value)
	{
		// 有时候会出现非常奇怪的现象,value显示是251,但是(int)value转换以后却是250,说明可能value实际是250.999999
		// 但是由于这种误差是非预期的,也就是说外边可能就是251*1,这种情况需要消除这种误差,使用checkInt即可消除
		value = value.checkInt();
		int intValue = (int)value;
		if (value.isEqual(intValue))
		{
			return intValue;
		}
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
	// 四舍五入
	public static int round(this float value)
	{
		if (value > 0.0f)
		{
			return (int)(value + 0.5f);
		}
		else
		{
			return (int)(value - 0.5f);
		}
	}
	// value1大于等于value0则返回1,否则返回0
	public static int step(this float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	public static float fmod(this float value0, float value1) { return value0 - value1 * (int)value0.divide(value1); }
	// 返回value的小数部分
	public static float frac(this float value) { return value - (int)value; }
	public static float abs(this float value) { return value >= 0.0f ? value : -value; }
	public static float sin(this float radian) { return Mathf.Sin(radian); }
	public static float cos(this float radian) { return Mathf.Cos(radian); }
	public static float tan(this float radian) { return Mathf.Tan(radian); }
	public static float atan(this float value) { return Mathf.Atan(value); }
	public static float asin(this float value) { return Mathf.Asin(value.clamp(-1.0f, 1.0f)); }
	public static float acos(this float value) { return Mathf.Acos(value.clamp(-1.0f, 1.0f)); }
	public static float sqrt(this float value) { return Mathf.Sqrt(value); }
	// 将一个浮点数调整保留一定的小数位,保留的最后一位四舍五入.precision表示小数点后保留几位小数
	public static float checkFloat(this float value, int precision = 4)
	{
		float helper = precision.pow10();
		return (value * helper).round().divide(helper);
	}
	// 检查浮点数是否接近整数（误差<precision），是则修正为整数值
	// 例如：checkInt(1.0000001f) → 1.0f, checkInt(0.9999999f) → 1.0f
	// 处理策略：大于0时检查与intValue或intValue+1的差距；小于0时检查与intValue或intValue-1的差距
	public static float checkInt(this float value, float precision = 0.0001f)
	{
		// 先判断是否为0
		if (value.isZero(precision))
		{
			return 0.0f;
		}
		int intValue = (int)value;
		// 大于0
		if (value > 0.0f)
		{
			// 如果原值减去整数值小于0.5f,则表示原值可能接近于整数值
			if (value - intValue < 0.5f)
			{
				if ((value - intValue).isZero(precision))
				{
					value = intValue;
				}
			}
			// 如果原值减去整数值大于0.5f, 则表示原值可能接近于整数值+1
			else
			{
				if ((value - (intValue + 1)).isZero(precision))
				{
					value = intValue + 1;
				}
			}
		}
		// 小于0
		else if (value < 0.0f)
		{
			// 如果原值减去整数值的结果的绝对值小于0.5f,则表示原值可能接近于整数值
			if (Mathf.Abs(value - intValue) < 0.5f)
			{
				if ((value - intValue).isZero(precision))
				{
					value = intValue;
				}
			}
			else
			{
				// 如果原值减去整数值的结果的绝对值大于0.5f, 则表示原值可能接近于整数值-1
				if ((value - (intValue - 1)).isZero(precision))
				{
					value = intValue - 1;
				}
			}
		}
		return value;
	}
	public static float toRadian(this float degree) { return degree * Mathf.Deg2Rad; }
	public static float toDegree(this float radian) { return radian * Mathf.Rad2Deg; }
	public static float clamp(this float value, float min, float max)
	{
		if (min > max || min.isEqual(max))
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
	public static float clampMin(this float value, float min = 0.0f) { return value < min ? min : value; }
	public static float clampMax(this float value, float max) { return value > max ? max : value; }
	public static bool isZero(this float value, float precision = 0.0001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isEqual(this float value1, float value2, float precision = 0.0001f)
	{
		return isZero(value1 - value2, precision);
	}
	// 安全倒数：value接近0时返回0而不是无穷大，避免除零错误
	public static float inverse(this float value)
	{
		if (value.isZero())
		{
			return 0.0f;
		}
		return 1.0f / value;
	}
	// 返回value0/value1的值,如果value1为0,则返回defaultValue
	public static float divide(this float value0, float value1, float defaultValue = 0.0f)
	{
		return !isZero(value1) ? value0 / value1 : defaultValue;
	}
	// 通过加减cycle将浮点value移入[min, max]范围（循环映射）
	// 逻辑与int版本相同，但使用浮点数运算
	public static float clampCycle(this float value, float min, float max, float cycle, bool includeMax = true)
	{
		while (value < min)
		{
			value += cycle;
		}
		if (includeMax)
		{
			while (value > max)
			{
				value -= cycle;
			}
		}
		else
		{
			while (value >= max)
			{
				value -= cycle;
			}
		}
		return value;
	}
	// fixedRangeOrder表示是否范围是从range0到range1,如果range0大于range1,则返回false
	public static bool inRange(this float value, float range0, float range1, float precision = 0.001f)
	{
		return value >= getMin(range0, range1) - precision && value <= getMax(range0, range1) + precision;
	}
	public static bool inRangeFixed(this float value, float range0, float range1, float precision = 0.001f)
	{
		return value >= range0 - precision && value <= range1 + precision;
	}
	// 将弧度规范化到[-π, π]范围
	public static float adjustRadian180(this float radian) { return radian.clampCycle(-PI_RADIAN, PI_RADIAN, TWO_PI_RADIAN); }
	// 将角度规范化到[-180°, 180°]范围，使用clampCycle实现周期循环
	public static float adjustAngle180(this float degree) { return degree.clampCycle(-PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	// 将弧度规范化到[0, 2π)范围
	public static float adjustRadian360(this float radian) { return radian.clampCycle(0.0f, TWO_PI_RADIAN, TWO_PI_RADIAN); }
	// 将角度规范化到[0°, 360°)范围
	public static float adjustAngle360(this float degree) { return degree.clampCycle(0.0f, TWO_PI_DEGREE, TWO_PI_DEGREE); }
	// 求Z轴顺时针旋转指定弧度后的单位向量（水平面，Y=0）
	// 先规范化弧度到[-π, π]，再用sin/cos构造方向
	public static Vector3 getVectorFromAngle(this float radian)
	{
		radian = radian.adjustRadian180();
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(radian.sin(), 0.0f, radian.cos());
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector2 getVector2FromAngle(this float radian)
	{
		radian = radian.adjustRadian180();
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(radian.sin(), radian.cos());
	}
	public static float KMHtoMS(this float kmh) { return kmh * 0.27777f; }
	public static float MStoKMH(this float ms) { return ms * 3.6f; }
	public static float MtoKM(this float m) { return m * 0.001f; }
	public static float pow(this float value, float power) { return Mathf.Pow(value, power); }
	public static float pow(this float value, int power)
	{
		float finalValue = 1.0f;
		for (int i = 0; i < power; ++i)
		{
			finalValue *= value;
		}
		return finalValue;
	}
}