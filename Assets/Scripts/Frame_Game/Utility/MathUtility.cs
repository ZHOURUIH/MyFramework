using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 数学相关工具函数,所有与数学计算相关的函数都在这里
public class MathUtility
{
	private static ThreadLock mGreaterPow2Lock = new();
	private static int[] mGreaterPow2;										// 预先生成的每个数字所对应的第一个比它大的2的n次方的数
	public const float TWO_PI_DEGREE = Mathf.PI * Mathf.Rad2Deg * 2.0f;     // 360.0f
	public const float TWO_PI_RADIAN = Mathf.PI * 2.0f;                     // 6.28f
	public const float HALF_PI_DEGREE = Mathf.PI * Mathf.Rad2Deg * 0.5f;    // 90.0f
	public const float HALF_PI_RADIAN = Mathf.PI * 0.5f;                    // 1.57f
	public const float PI_DEGREE = Mathf.PI * Mathf.Rad2Deg;                // 180.0f
	public const float PI_RADIAN = Mathf.PI;                                // 3.14f
	public static long[] POWER_INT_10 = new long[11]{ 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L, 10000000L, 100000000L, 1000000000L, 10000000000L };
	public static float[] INVERSE_POWER_INT_10 = new float[7] { 1.0f, 0.1f, 0.01f, 0.001f, 0.0001f, 0.00001f, 0.000001f };
	public static double[] INVERSE_POWER_LLONG_10 = new double[11] { 1.0, 0.1, 0.01, 0.001, 0.0001, 0.00001, 0.000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001 };
	public static long[] POWER_LLONG_10 = new long[19]
	{
		1L,
		10L,
		100L,
		1000L,
		10000L,
		100000L,
		1000000L,
		10000000L,
		100000000L,
		1000000000L,
		10000000000L,
		100000000000L,
		1000000000000L,
		10000000000000L,
		100000000000000L,
		1000000000000000L,
		10000000000000000L,
		100000000000000000L,
		1000000000000000000L
	};
	public static float pow(float value, float power) { return Mathf.Pow(value, power); }
	public static int getGreaterPowValue(int value, int pow)
	{
		int powValue = 1;
		for(int i = 0; i < 31; ++i)
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
	public static int getGreaterPow2(int value)
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
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(float value)
	{
		int intValue = (int)(value);
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
	// 四舍五入
	public static int round(float value)
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
	public static Vector3 round(Vector3 value)
	{
		value.x = round(value.x);
		value.y = round(value.y);
		value.z = round(value.z);
		return value;
	}
	public static float abs(float value) { return value >= 0.0f ? value : -value; }
	public static float sin(float radian) { return Mathf.Sin(radian); }
	public static float cos(float radian) { return Mathf.Cos(radian); }
	public static float tan(float radian) { return Mathf.Tan(radian); }
	public static float asin(float value)
	{
		clamp(ref value, -1.0f, 1.0f);
		return Mathf.Asin(value);
	}
	public static float sqrt(float value) { return Mathf.Sqrt(value); }
	public static float sign(float value)
	{
		if (value < 0.0f)
		{
			return -1.0f;
		}
		else if (value > 0.0f)
		{
			return 1.0f;
		}
		return 0.0f;
	}
	public static int sign(int value)
	{
		if (value < 0)
		{
			return -1;
		}
		else if (value > 0)
		{
			return 1;
		}
		return 0;
	}
	public static int sign(ulong value0, ulong value1)
	{
		if (value0 < value1)
		{
			return -1;
		}
		else if (value0 > value1)
		{
			return 1;
		}
		return 0;
	}
	// value是否是2的n次方
	public static bool isPow2(int value) { return (value & (value - 1)) == 0; }
	public static float checkInt(float value, float precision = 0.0001f)
	{
		checkInt(ref value, precision);
		return value;
	}
	// 检查一个浮点数是否接近整数,精度范围为precision,如果接近整数,则将此浮点数设置为整数的值
	public static void checkInt(ref float value, float precision = 0.0001f)
	{
		// 先判断是否为0
		if (isFloatZero(value, precision))
		{
			value = 0.0f;
			return;
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
					value = (intValue + 1);
				}
			}
		}
		// 小于0
		else if (value < 0.0f)
		{
			// 如果原值减去整数值的结果的绝对值小于0.5f,则表示原值可能接近于整数值
			if (Math.Abs(value - intValue) < 0.5f)
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
					value = (intValue - 1);
				}
			}
		}
	}
	// 将向量的Y设置为0
	public static Vector3 resetY(Vector3 v) { return new(v.x, 0.0f, v.z); }
	// 将向量的X替换为指定值
	public static Vector3 replaceX(Vector3 v, float x) { return new(x, v.y, v.z); }
	// 将向量的Y替换为指定值
	public static Vector3 replaceY(Vector3 v, float y) { return new(v.x, y, v.z); }
	// 将向量的Z替换为指定值
	public static Vector3 replaceZ(Vector3 v, float z) { return new(v.x, v.y, z); }
	// vec0的2个分量是否都小于vec1的2个分量
	public static bool isVector2Less(Vector2 vec0, Vector2 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y; }
	// vec0的2个分量是否都大于vec1的2个分量
	public static bool isVector2Greater(Vector2 vec0, Vector2 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y; }
	public static bool isVectorEqual(Vector2 vec0, Vector2 vec1, float precision = 0.0001f) 
	{
		return isFloatZero(vec0.x - vec1.x, precision) && 
			   isFloatZero(vec0.y - vec1.y, precision); 
	}
	public static bool isVectorEqual(Vector3 vec0, Vector3 vec1, float precision = 0.0001f) 
	{
		return isFloatZero(vec0.x - vec1.x, precision) && 
			   isFloatZero(vec0.y - vec1.y, precision) && 
			   isFloatZero(vec0.z - vec1.z, precision); 
	}
	public static bool isVectorZero(Vector3 vec, float precision = 0.0001f) 
	{ 
		return isFloatZero(vec.x, precision) && 
			   isFloatZero(vec.y, precision) && 
			   isFloatZero(vec.z, precision); 
	}
	public static float getLength(Vector3 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	public static float getSquaredLength(Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static bool lengthLess(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthGreater(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static Vector3 normalize(Vector3 vec3)
	{
		float inverseLen = divide(1.0f, getLength(vec3));
		return new(vec3.x * inverseLen, vec3.y * inverseLen, vec3.z * inverseLen);
	}
	public static float toRadian(float degree) { return degree * Mathf.Deg2Rad; }
	public static int getMin(int a, int b) { return a < b ? a : b; }
	public static float getMin(float a, float b) { return a < b ? a : b; }
	public static int getMax(int a, int b) { return a > b ? a : b; }
	public static float getMax(float a, float b) { return a > b ? a : b; }
	public static Vector3 lerpSimple(Vector3 start, Vector3 end, float t) { return start + (end - start) * t; }
	public static void clamp(ref float value, float min, float max)
	{
		if (min > max || isFloatEqual(min, max))
		{
			value = min;
			return;
		}
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
	}
	public static void clamp(ref int value, int min, int max)
	{
		if (min > max)
		{
			return;
		}
		if (min == max)
		{
			value = min;
			return;
		}
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
	}
	public static void clampMin(ref int value, int min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref float value, float min = 0.0f)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static int clampMin(int value, int min = 0)
	{
		return value < min ? min : value;
	}
	public static void clampMax(ref int value, int max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static float clampMax(float value, float max)
	{
		return value > max ? max : value;
	}
	public static bool isFloatZero(float value, float precision = 0.0001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isFloatEqual(float value1, float value2, float precision = 0.0001f)
	{
		return isFloatZero(value1 - value2, precision);
	}
	// 返回value0/value1的值,如果value1为0,则返回defaultValue
	public static float divide(float value0, float value1, float defaultValue = 0.0f)
	{
		return !isFloatZero(value1) ? value0 / value1 : defaultValue;
	}
	public static void clampCycle(ref float value, float min, float max, float cycle, bool includeMax = true)
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
	}
	// 查找移动指定距离后当前位于哪段线段上
	public static int findPointIndex(List<KeyPoint> distanceListFromStart, float curDistance, int startIndex, int endIndex)
	{
		if (curDistance < distanceListFromStart[startIndex].mDistanceFromStart)
		{
			return startIndex - 1;
		}
		if (curDistance >= distanceListFromStart[endIndex].mDistanceFromStart)
		{
			return endIndex;
		}
		if (endIndex == startIndex || endIndex - startIndex == 1)
		{
			return startIndex;
		}
		int middleIndex = ((endIndex - startIndex) >> 1) + startIndex;
		// 当前距离比中间的距离小,则在列表前半部分查找
		KeyPoint midKey = distanceListFromStart[middleIndex];
		if (curDistance < midKey.mDistanceFromStart)
		{
			return findPointIndex(distanceListFromStart, curDistance, startIndex, middleIndex);
		}
		// 当前距离比中间的距离小,则在列表后半部分查找
		else if (curDistance > midKey.mDistanceFromStart)
		{
			return findPointIndex(distanceListFromStart, curDistance, middleIndex, endIndex);
		}
		// 距离刚好等于列表中间的值,则返回该下标
		else
		{
			return middleIndex;
		}
	}
	// 查找移动指定距离后当前位于哪段线段上
	public static int findPointIndex(List<float> distanceListFromStart, float curDistance, int startIndex, int endIndex)
	{
		if (curDistance < distanceListFromStart[startIndex])
		{
			return startIndex - 1;
		}
		if (curDistance >= distanceListFromStart[endIndex])
		{
			return endIndex;
		}
		if (endIndex == startIndex || endIndex - startIndex == 1)
		{
			return startIndex;
		}
		int middleIndex = ((endIndex - startIndex) >> 1) + startIndex;
		float midDis = distanceListFromStart[middleIndex];
		// 当前距离比中间的距离小,则在列表前半部分查找
		if (curDistance < midDis)
		{
			return findPointIndex(distanceListFromStart, curDistance, startIndex, middleIndex);
		}
		// 当前距离比中间的距离小,则在列表后半部分查找
		else if (curDistance > midDis)
		{
			return findPointIndex(distanceListFromStart, curDistance, middleIndex, endIndex);
		}
		// 距离刚好等于列表中间的值,则返回该下标
		else
		{
			return middleIndex;
		}
	}
	public static Vector2 multiVector2(Vector2 v1, Vector2 v2) { return new(v1.x * v2.x, v1.y * v2.y); }
	public static Vector2 divideVector2(Vector2 v1, Vector2 v2) { return new(divide(v1.x, v2.x), divide(v1.y, v2.y)); }
	public static Vector3 multiVector3(Vector3 v1, Vector3 v2) { return new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
	public static void adjustAngle180(ref float degree) { clampCycle(ref degree, -PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	// 使用一个四元数去旋转一个三维向量
	public static Vector3 rotateVector3(Vector3 vec, Quaternion transQuat) { return transQuat * vec; }
	public static float speedToInterval(float speed) 
	{
		return divide(0.0333f, speed); 
	}
	public static float intervalToSpeed(float interval) 
	{
		return divide(0.0333f, interval);
	}
	public static void quickSort<T>(List<T> arr, Comparison<T> comparison)
	{
		quickSort(arr, 0, arr.Count - 1, comparison);
	}
	public static bool overlapBox2(Vector2 pos0, Vector2 size0, Vector2 pos1, Vector2 size1)
	{
		Vector2 min0 = pos0 - size0 * 0.5f;
		Vector2 max0 = pos0 + size0 * 0.5f;
		Vector2 min1 = pos1 - size1 * 0.5f;
		Vector2 max1 = pos1 + size1 * 0.5f;
		return isVector2Less(min0, max1) && isVector2Greater(max0, min1) ||
			   isVector2Less(min1, max0) && isVector2Greater(max1, min0);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 可以通过comparison自己决定升序还是降序,所以不再需要额外的参数
	protected static void quickSort<T>(List<T> arr, int low, int high, Comparison<T> comparison)
	{
		if (high <= low)
		{
			return;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key小的值,如果小于key,则一直继续查找
			while (comparison(arr[++i], key) < 0 && i != high){}
			// 从右向左找到一个比key大的值,如果大于key,则一直继续查找
			while (comparison(arr[--j], key) > 0 && j != low){}
			if (i >= j)
			{
				break;
			}
			// 交换i,j对应的值
			swapIndex(arr, i, j);
		}
		// 中枢值与j对应值交换
		swapIndex(arr, low, j);
		quickSort(arr, low, j - 1, comparison);
		quickSort(arr, j + 1, high, comparison);
	}
	protected static void quickSort<T>(List<T> arr, int low, int high, bool ascend = true) where T : IComparable<T>
	{
		if (high <= low)
		{
			return;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 升序
			if(ascend)
			{
				// 从左向右找到一个比key大的值,如果小于key,则一直继续查找
				while (arr[++i].CompareTo(key) < 0 && i != high) { }
				// 从右向左找到一个比key小的值,如果大于key,则一直继续查找
				while (arr[--j].CompareTo(key) > 0 && j != low) { }
			}
			// 降序
			else
			{
				// 从左向右找到一个比key小的值,如果大于key,则一直继续查找
				while (arr[++i].CompareTo(key) > 0 && i != high) { }
				// 从右向左找到一个比key大的值,如果小于key,则一直继续查找
				while (arr[--j].CompareTo(key) < 0 && j != low) { }
			}
			if (i >= j)
			{
				break;
			}
			// 交换i,j对应的值
			swapIndex(arr, i, j);
		}
		// 中枢值与j对应值交换
		swapIndex(arr, low, j);
		quickSort(arr, low, j - 1, ascend);
		quickSort(arr, j + 1, high, ascend);
	}
	protected static void swapIndex<T>(List<T> list, int index0, int index1)
	{
		T temp0 = list[index0];
		list[index0] = list[index1];
		list[index1] = temp0;
	}
	protected static void initGreaterPow2()
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
					mGreaterPow2[i] = getGreaterPowValue(i, 2);
				}
			}
		}
	}
}