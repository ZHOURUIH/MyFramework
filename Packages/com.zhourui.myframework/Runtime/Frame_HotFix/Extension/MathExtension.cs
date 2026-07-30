using UnityEngine;
using static MathUtility;
using static UnityUtility;

// 一些数学方法的扩展方法
public static class MathExtension
{
	private static ThreadLock mGreaterPow2Lock = new();		// mGreaterPow2的线程锁
	private static int[] mGreaterPow2;						// 预先生成的每个数字所对应的第一个比它大的2的n次方的数
	// 得到数轴上浮点数右边的第一个整数,向上取整
	public static int ceil(this float value)
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
	public static Vector2 ceil(this Vector2 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
		return value;
	}
	public static Vector3 ceil(this Vector3 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
		value.z = ceil(value.z);
		return value;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(this float value)
	{
		// 有时候会出现非常奇怪的现象,value显示是251,但是(int)value转换以后却是250,说明可能value实际是250.999999
		// 但是由于这种误差是非预期的,也就是说外边可能就是251*1,这种情况需要消除这种误差,使用checkInt即可消除
		value = value.checkInt();
		int intValue = (int)value;
		if (isFloatEqual(intValue, value))
		{
			return intValue;
		}
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
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
	public static bool isNaN(this Vector3 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z); }
	public static bool isNaN(this Vector2 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y); }
	public static bool isNaN(this float value) { return float.IsNaN(value); }
	public static float saturate(this float value){ return value.clamp(0.0f, 1.0f); }
	public static Vector3 saturate(this Vector3 value)
	{
		value.x = saturate(value.x);
		value.y = saturate(value.y);
		value.z = saturate(value.z);
		return value;
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
	public static Vector3 round(this Vector2 value)
	{
		value.x = round(value.x);
		value.y = round(value.y);
		return value;
	}
	public static Vector3 floor(this Vector2 value)
	{
		value.x = floor(value.x);
		value.y = floor(value.y);
		return value;
	}
	public static Vector3 round(this Vector3 value)
	{
		value.x = round(value.x);
		value.y = round(value.y);
		value.z = round(value.z);
		return value;
	}
	public static Vector3 floor(this Vector3 value)
	{
		value.x = floor(value.x);
		value.y = floor(value.y);
		value.z = floor(value.z);
		return value;
	}
	// value1大于等于value0则返回1,否则返回0
	public static int step(this float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	public static float fmod(this float value0, float value1) { return value0 - value1 * (int)(value0.divide(value1)); }
	// 返回value的小数部分
	public static float frac(this float value) { return value - (int)value; }
	public static float abs(this float value) { return value >= 0.0f ? value : -value; }
	public static Vector3 abs(this Vector2 value) { return new(value.x.abs(), value.y.abs()); }
	public static Vector2Int abs(this Vector2Int value) { return new(value.x.abs(), value.y.abs()); }
	public static Vector3 abs(this Vector3 value) { return new(value.x.abs(), value.y.abs(), value.z.abs()); }
	public static Vector3Int abs(this Vector3Int value) { return new(value.x.abs(), value.y.abs(), value.z.abs()); }
	public static Vector4 abs(this Vector4 value) { return new(value.x.abs(), value.y.abs(), value.z.abs(), value.w.abs()); }
	public static double abs(this double value) { return value >= 0.0 ? value : -value; }
	public static sbyte abs(this sbyte value) { return value >= 0 ? value : (sbyte)-value; }
	public static short abs(this short value) { return value >= 0 ? value : (short)-value; }
	public static int abs(this int value) { return value >= 0 ? value : -value; }
	public static long abs(this long value) { return value >= 0 ? value : -value; }
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
		float helper = pow10(precision);
		value = divide(round(value * helper), helper);
		return value;
	}
	public static Vector3 checkFloat(this Vector3 value, int precision = 4)
	{
		value.x = value.x.checkFloat(precision);
		value.y = value.y.checkFloat(precision);
		value.z = value.z.checkFloat(precision);
		return value;
	}
	public static Vector3 checkInt(this Vector3 vec, float precision = 0.0001f)
	{
		vec.x = vec.x.checkInt(precision);
		vec.y = vec.y.checkInt(precision);
		vec.z = vec.z.checkInt(precision);
		return vec;
	}
	// 检查一个浮点数是否接近整数,精度范围为precision,如果接近整数,则将此浮点数设置为整数的值
	public static float checkInt(this float value, float precision = 0.0001f)
	{
		// 先判断是否为0
		if (value.isFloatZero(precision))
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
				if (isFloatZero(value - intValue, precision))
				{
					value = intValue;
				}
			}
			// 如果原值减去整数值大于0.5f, 则表示原值可能接近于整数值+1
			else
			{
				if (isFloatZero(value - (intValue + 1), precision))
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
				if (isFloatZero(value - intValue, precision))
				{
					value = intValue;
				}
			}
			else
			{
				// 如果原值减去整数值的结果的绝对值大于0.5f, 则表示原值可能接近于整数值-1
				if (isFloatZero(value - (intValue - 1), precision))
				{
					value = intValue - 1;
				}
			}
		}
		return value;
	}
	// 将vec的长度限定到maxLength,如果长度未超过,则不作修改
	public static Vector3 clampLength(this Vector3 vec, float maxLength)
	{
		if (vec.lengthGreater(maxLength))
		{
			return normalize(vec) * maxLength;
		}
		return vec;
	}
	// 将向量的X设置为0
	public static Vector3 resetX(this Vector3 v) { return new(0.0f, v.y, v.z); }
	// 将向量的Y设置为0
	public static Vector3 resetY(this Vector3 v) { return new(v.x, 0.0f, v.z); }
	// 将向量的Z设置为0
	public static Vector3 resetZ(this Vector3 v) { return new(v.x, v.y, 0.0f); }
	// 将向量的X替换为指定值
	public static Vector3 replaceX(this Vector3 v, float x) { return new(x, v.y, v.z); }
	// 将向量的Y替换为指定值
	public static Vector3 replaceY(this Vector3 v, float y) { return new(v.x, y, v.z); }
	// 将向量的Z替换为指定值
	public static Vector3 replaceZ(this Vector3 v, float z) { return new(v.x, v.y, z); }
	// 将向量的X设置为0
	public static Vector2 resetX(this Vector2 v) { return new(0.0f, v.y); }
	// 将向量的Y设置为0
	public static Vector2 resetY(this Vector2 v) { return new(v.x, 0.0f); }
	// 将向量的X替换为指定值
	public static Vector2 replaceX(this Vector2 v, float x) { return new(x, v.y); }
	// 将向量的Y替换为指定值
	public static Vector2 replaceY(this Vector2 v, float y) { return new(v.x, y); }
	public static bool isVectorZero(this Vector2 vec, float precision = 0.0001f)
	{
		return vec.x.isFloatZero(precision) &&
			   vec.y.isFloatZero(precision);
	}
	public static bool isVectorZero(this Vector3 vec, float precision = 0.0001f)
	{
		return vec.x.isFloatZero(precision) &&
			   vec.y.isFloatZero(precision) &&
			   vec.z.isFloatZero(precision);
	}
	public static float getLength(this Vector4 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w); }
	public static float getLength(this Vector3 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	public static float getLengthIgnoreY(this Vector3 vec) { return sqrt(vec.x * vec.x + vec.z * vec.z); }
	public static float getLength(this Vector2 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	public static float getSquaredLength(this Vector4 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w; }
	public static float getSquaredLength(this Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static float getSquaredLengthIgnoreY(this Vector3 vec) { return vec.x * vec.x + vec.z * vec.z; }
	public static float getSquaredLength(this Vector2 vec) { return vec.x * vec.x + vec.y * vec.y; }
	public static bool lengthLess(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y < vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLess(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y < length * length; }
	public static bool lengthLess(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z < vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLess(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthLessIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z < length * length; }
	public static bool lengthLess(this Vector4 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w < length * length; }
	public static bool lengthLessEqual(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y <= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLessEqual(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y <= length * length; }
	public static bool lengthLessEqual(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z <= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLessEqual(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z <= length * length; }
	public static bool lengthLessEqualIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z <= length * length; }
	public static bool lengthGreater(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static bool lengthGreater(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y > vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreater(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	public static bool lengthGreaterIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z > length * length; }
	public static bool lengthGreater(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z > vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqual(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y >= length * length; }
	public static bool lengthGreaterEqual(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y >= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreaterEqual(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z >= length * length; }
	public static bool lengthGreaterEqual(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z >= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqualIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z >= length * length; }
	public static Vector3 setLength(this Vector3 vec, float length)
	{
		float scale = 1.0f.divide(getLength(vec)) * length;
		return new(vec.x * scale, vec.y * scale, vec.z * scale);
	}
	public static Vector2 setLength(this Vector2 vec, float length)
	{
		float scale = 1.0f.divide(getLength(vec)) * length;
		return new(vec.x * scale, vec.y * scale);
	}
	// vec0的3个分量是否都小于vec1的3个分量
	public static bool isVector3Less(this Vector3 vec0, Vector3 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y && vec0.z < vec1.z; }
	// vec0的3个分量是否都大于vec1的3个分量
	public static bool isVector3Greater(this Vector3 vec0, Vector3 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y && vec0.z > vec1.z; }
	// vec0的2个分量是否都小于vec1的2个分量
	public static bool isVector2Less(this Vector2 vec0, Vector2 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y; }
	// vec0的2个分量是否都大于vec1的2个分量
	public static bool isVector2Greater(this Vector2 vec0, Vector2 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y; }
	public static bool isVectorEqual(this Vector2 vec0, Vector2 vec1, float precision = 0.0001f)
	{
		return isFloatZero(vec0.x - vec1.x, precision) &&
			   isFloatZero(vec0.y - vec1.y, precision);
	}
	public static bool isVectorEqual(this Vector3 vec0, Vector3 vec1, float precision = 0.0001f)
	{
		return isFloatZero(vec0.x - vec1.x, precision) &&
			   isFloatZero(vec0.y - vec1.y, precision) &&
			   isFloatZero(vec0.z - vec1.z, precision);
	}
	public static bool isQuaternionEqual(this Quaternion value0, Quaternion value1, float precision = 0.0001f)
	{
		return isFloatEqual(value0.x, value1.x, precision) &&
			   isFloatEqual(value0.y, value1.y, precision) &&
			   isFloatEqual(value0.z, value1.z, precision) &&
			   isFloatEqual(value0.w, value1.w, precision);
	}
	public static Vector3 normalize(this Vector3 vec3)
	{
		float inverseLen = 1.0f.divide(getLength(vec3));
		return new(vec3.x * inverseLen, vec3.y * inverseLen, vec3.z * inverseLen);
	}
	public static Vector2 normalize(this Vector2 vec2)
	{
		float inverseLen = 1.0f.divide(getLength(vec2));
		return new(vec2.x * inverseLen, vec2.y * inverseLen);
	}
	public static float toRadian(this float degree) { return degree * Mathf.Deg2Rad; }
	public static Vector3 toRadian(this Vector3 degree) { return degree * Mathf.Deg2Rad; }
	public static float toDegree(this float radian) { return radian * Mathf.Rad2Deg; }
	public static Vector3 toDegree(this Vector3 radian) { return radian * Mathf.Rad2Deg; }
	public static float getQuaternionYaw(this Quaternion q) { return q.eulerAngles.y; }
	public static float getQuaternionPitch(this Quaternion q) { return q.eulerAngles.z; }
	public static float getQuaternionRoll(this Quaternion q) { return q.eulerAngles.x; }
	public static float clamp(this float value, float min, float max)
	{
		if (min > max || min.isFloatEqual(max))
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
	public static int clamp(this int value, int min, int max)
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
	public static byte clampMin(this byte value, byte min = 0) { return value < min ? min : value; }
	public static sbyte clampMin(this sbyte value, sbyte min = 0) { return value < min ? min : value; }
	public static short clampMin(this short value, short min = 0) { return value < min ? min : value; }
	public static ushort clampMin(this ushort value, ushort min = 0) { return value < min ? min : value; }
	public static int clampMin(this int value, int min = 0) { return value < min ? min : value; }
	public static uint clampMin(this uint value, uint min = 0) { return value < min ? min : value; }
	public static long clampMin(this long value, long min = 0) { return value < min ? min : value; }
	public static ulong clampMin(this ulong value, ulong min = 0) { return value < min ? min : value; }
	public static float clampMin(this float value, float min = 0.0f) { return value < min ? min : value; }
	public static double clampMin(this double value, double min = 0.0) { return value < min ? min : value; }
	public static Vector2 clampMin(this Vector2 value, float min = 0.0f) 
	{
		value.x = value.x.clampMin(min);
		value.y = value.y.clampMin(min);
		return value;
	}
	public static Vector2Int clampMin(this Vector2Int value, int min = 0)
	{
		value.x = value.x.clampMin(min);
		value.y = value.y.clampMin(min);
		return value;
	}
	public static Vector3 clampMin(this Vector3 value, float min = 0.0f)
	{
		value.x = value.x.clampMin(min);
		value.y = value.y.clampMin(min);
		value.z = value.z.clampMin(min);
		return value;
	}
	public static Vector3Int clampMin(this Vector3Int value, int min = 0)
	{
		value.x = value.x.clampMin(min);
		value.y = value.y.clampMin(min);
		value.z = value.z.clampMin(min);
		return value;
	}
	public static Vector4 clampMin(this Vector4 value, float min = 0.0f)
	{
		value.x = value.x.clampMin(min);
		value.y = value.y.clampMin(min);
		value.z = value.z.clampMin(min);
		value.w = value.w.clampMin(min);
		return value;
	}
	public static byte clampMax(this byte value, byte max) { return value > max ? max : value; }
	public static sbyte clampMax(this sbyte value, sbyte max) { return value > max ? max : value; }
	public static short clampMax(this short value, short max) { return value > max ? max : value; }
	public static ushort clampMax(this ushort value, ushort max) { return value > max ? max : value; }
	public static int clampMax(this int value, int max) { return value > max ? max : value; }
	public static uint clampMax(this uint value, uint max) { return value > max ? max : value; }
	public static long clampMax(this long value, long max) { return value > max ? max : value; }
	public static ulong clampMax(this ulong value, ulong max) { return value > max ? max : value; }
	public static float clampMax(this float value, float max) { return value > max ? max : value; }
	public static double clampMax(this double value, double max) { return value > max ? max : value; }
	public static Vector2 clampMax(this Vector2 value, float min = 0.0f)
	{
		value.x = value.x.clampMax(min);
		value.y = value.y.clampMax(min);
		return value;
	}
	public static Vector2Int clampMax(this Vector2Int value, int min = 0)
	{
		value.x = value.x.clampMax(min);
		value.y = value.y.clampMax(min);
		return value;
	}
	public static Vector3 clampMax(this Vector3 value, float min = 0.0f)
	{
		value.x = value.x.clampMax(min);
		value.y = value.y.clampMax(min);
		value.z = value.z.clampMax(min);
		return value;
	}
	public static Vector3Int clampMax(this Vector3Int value, int min = 0)
	{
		value.x = value.x.clampMax(min);
		value.y = value.y.clampMax(min);
		value.z = value.z.clampMax(min);
		return value;
	}
	public static Vector4 clampMax(this Vector4 value, float min = 0.0f)
	{
		value.x = value.x.clampMax(min);
		value.y = value.y.clampMax(min);
		value.z = value.z.clampMax(min);
		value.w = value.w.clampMax(min);
		return value;
	}
	public static bool isFloatZero(this float value, float precision = 0.0001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isZero(this double value, double precision = 0.00000001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isFloatEqual(this float value1, float value2, float precision = 0.0001f)
	{
		return isFloatZero(value1 - value2, precision);
	}
	public static bool isDoubleEqual(this double value1, double value2, double precision = 0.00000001f)
	{
		return isZero(value1 - value2, precision);
	}
	// 返回value0/value1的值,如果value1为0,则返回defaultValue
	public static float divide(this float value0, float value1, float defaultValue = 0.0f)
	{
		return !isFloatZero(value1) ? value0 / value1 : defaultValue;
	}
	public static float divide(this int value0, int value1, int defaultValue = 0)
	{
		return value1 != 0 ? (float)value0 / value1 : defaultValue;
	}
	public static float divide(this int value0, float value1, int defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static int divideInt(this int value0, int value1, int defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static long divideLong(this long value0, long value1, long defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static double divide(this double value0, double value1, double defaultValue = 0.0f)
	{
		return !isZero(value1) ? value0 / value1 : defaultValue;
	}
	public static int clampCycle(this int value, int min, int max, int cycle, bool includeMax = true)
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
	public static bool inRange(this int value, int range0, int range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	public static bool inRangeFixed(this int value, int range0, int range1)
	{
		return value >= range0 && value <= range1;
	}
	public static bool inRange(this Vector3 value, Vector3 point0, Vector3 point1, bool ignoreY = true, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, precision) &&
			   inRange(value.z, point0.z, point1.z, precision) &&
			  (ignoreY || inRange(value.y, point0.y, point1.y, precision));
	}
	public static bool inRange(this Vector2 value, Vector2 point0, Vector2 point1, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, precision) &&
			   inRange(value.y, point0.y, point1.y, precision);
	}
	public static Vector2 multiVector2(this Vector2 v1, Vector2 v2) { return new(v1.x * v2.x, v1.y * v2.y); }
	public static Vector2 divideVector2(this Vector2 v1, Vector2 v2) { return new(divide(v1.x, v2.x), divide(v1.y, v2.y)); }
	public static Vector3 multiVector3(this Vector3 v1, Vector3 v2) { return new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
	public static Vector3 divideVector3(this Vector3 v1, Vector3 v2) { return new(divide(v1.x, v2.x), divide(v1.y, v2.y), divide(v1.z, v2.z)); }
	public static Vector2 divide(this Vector2 v1, float scale) { return new(divide(v1.x, scale), divide(v1.y, scale)); }
	public static Vector3 divide(this Vector3 v1, float scale) { return new(divide(v1.x, scale), divide(v1.y, scale), divide(v1.z, scale)); }
	public static float adjustRadian180(this float radian) { return clampCycle(radian, -PI_RADIAN, PI_RADIAN, TWO_PI_RADIAN); }
	public static Vector3 adjustRadian180(this Vector3 radian)
	{
		radian.x = adjustRadian180(radian.x);
		radian.y = adjustRadian180(radian.y);
		radian.z = adjustRadian180(radian.z);
		return radian;
	}
	public static float adjustAngle180(this float degree) { return clampCycle(degree, -PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	public static Vector3 adjustAngle180(this Vector3 degree)
	{
		degree.x = adjustAngle180(degree.x);
		degree.y = adjustAngle180(degree.y);
		degree.z = adjustAngle180(degree.z);
		return degree;
	}
	public static float adjustRadian360(this float radian) { return clampCycle(radian, 0.0f, TWO_PI_RADIAN, TWO_PI_RADIAN); }
	public static Vector3 adjustRadian360(this Vector3 radian)
	{
		radian.x = adjustRadian360(radian.x);
		radian.y = adjustRadian360(radian.y);
		radian.z = adjustRadian360(radian.z);
		return radian;
	}
	public static float adjustAngle360(this float degree) { return clampCycle(degree, 0.0f, TWO_PI_DEGREE, TWO_PI_DEGREE); }
	public static Vector3 adjustAngle360(this Vector3 degree)
	{
		degree.x = adjustAngle360(degree.x);
		degree.y = adjustAngle360(degree.y);
		degree.z = adjustAngle360(degree.z);
		return degree;
	}
	// 求从z轴到指定向量的水平方向上的顺时针角度,角度范围是-MATH_PI 到 MATH_PI
	public static float getAngleFromVector3(this Vector3 vec, ANGLE radian = ANGLE.RADIAN)
	{
		vec.y = 0.0f;
		vec = vec.normalize();
		float angle = acos(vec.z);
		if (vec.x > 0.0f)
		{
			angle = -angle;
		}
		// 在unity的坐标系中航向角需要取反
		angle = -angle.adjustRadian180();
		if (radian == ANGLE.DEGREE)
		{
			angle = toDegree(angle);
		}
		return angle;
	}
	public static float getAngleFromVector2(this Vector2 vec)
	{
		Vector3 tempVec = normalize(new Vector3(vec.x, 0.0f, vec.y));
		float angle = acos(tempVec.z);
		if (tempVec.x > 0.0f)
		{
			angle = -angle;
		}
		// 在unity的坐标系中航向角需要取反
		angle = -angle.adjustRadian180();
		return angle;
	}
	public static Vector3 rotateVector3(this Vector3 vec, Matrix4x4 transMat3) { return transMat3 * vec; }
	// 使用一个四元数去旋转一个三维向量
	public static Vector3 rotateVector3(this Vector3 vec, Quaternion transQuat) { return transQuat * vec; }
	// 求向量水平顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 rotateVector3(this Vector3 vec, float radian)
	{
		return vec.rotateVector3(Quaternion.AngleAxis(toDegree(radian), Vector3.up));
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 getVectorFromAngle(this float radian)
	{
		radian = radian.adjustRadian180();
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(sin(radian), 0.0f, cos(radian));
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector2 getVector2FromAngle(this float radian)
	{
		radian = radian.adjustRadian180();
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(sin(radian), cos(radian));
	}
	public static float dot(this Vector3 v0, Vector3 v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	public static float dot(this Vector2 v0, Vector2 v1) { return v0.x * v1.x + v0.y * v1.y; }
	public static Vector3 cross(this Vector3 v0, Vector3 v1) { return Vector3.Cross(v0, v1); }
	public static int generateBatchCount(this int totalCount, int batch)
	{
		int batchCount = totalCount.divideInt(batch);
		return totalCount - batch * batchCount > 0 ? batchCount + 1 : batchCount;
	}
	public static int indexToX(this int index, int width) { return index % width; }
	public static int indexToY(this int index, int width) { return index.divideInt(width); }
	public static Vector2Int indexToIntPos(this int index, int width) { return new(index % width, index.divideInt(width)); }
	public static int intPosToIndex(this Vector2Int pos, int width) { return pos.x + pos.y * width; }
	public static bool hasMask(this int value, int mask) { return (value & mask) != 0; }
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
	public static float inversePow10(this int pow) { return INVERSE_POWER_INT_10[pow]; }
	public static int pow10(this int pow) { return (int)POWER_INT_10[pow]; }
	public static double inversePow10Long(this int pow) { return INVERSE_POWER_LLONG_10[pow]; }
	public static long pow10Long(this int pow) { return POWER_LLONG_10[pow]; }
	public static float pow2(this int power) { return 1 << power; }
	public static int getGreaterPowValue(this int value, int pow)
	{
		int powValue = 1;
		for (int i = 0; i < 31; ++i)
		{
			if (powValue >= value)
			{
				break;
			}
			powValue *= pow;
		}
		return powValue;
	}
	// 获得大于value的第一个2的n次方的数,value需要大于0
	public static int getGreaterPow2(this int value)
	{
		if (mGreaterPow2 == null)
		{
			initGreaterPow2();
		}
		if (mGreaterPow2 != null && value < mGreaterPow2.Length)
		{
			return mGreaterPow2[value];
		}
		if (isPow2(value))
		{
			return value;
		}
		// 由于2的9次方以下都可以通过查表的方式获得,所以此处直接从10次方开始
		// 分2个档位,2的15次方,这样处理更快一些,比二分查找或顺序查找都快
		const int Level0 = 15;
		if (value < 1 << Level0)
		{
			for (int i = 10; i <= Level0; ++i)
			{
				if (1 << i >= value)
				{
					return 1 << i;
				}
			}
		}
		else
		{
			for (int i = Level0 + 1; i < 32; ++i)
			{
				if (1 << i >= value)
				{
					return 1 << i;
				}
			}
		}
		logError("无法获取大于指定数的第一个2的n次方的数:" + value);
		return 0;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void initGreaterPow2()
	{
		using (new ThreadLockScope(mGreaterPow2Lock))
		{
			if (mGreaterPow2 != null)
			{
				return;
			}
			mGreaterPow2 = new int[513];
			for (int i = 0; i < mGreaterPow2.Length; ++i)
			{
				if (i <= 1)
				{
					mGreaterPow2[i] = 2;
				}
				else
				{
					mGreaterPow2[i] = i.getGreaterPowValue(2);
				}
			}
		}
	}
}