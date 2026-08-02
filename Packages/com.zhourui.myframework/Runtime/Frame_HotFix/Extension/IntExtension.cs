using UnityEngine;
using static MathUtility;
using static UnityUtility;

// int的扩展方法
public static class IntExtension
{
	private static ThreadLock mGreaterPow2Lock = new();     // mGreaterPow2的线程锁
	private static int[] mGreaterPow2;                      // 预先生成的每个数字所对应的第一个比它大的2的n次方的数
	public static int abs(this int value) { return value >= 0 ? value : -value; }
	public static float sqrt(this int value) { return Mathf.Sqrt(value); }
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
	public static int clampMin(this int value, int min = 0) { return value < min ? min : value; }
	public static int clampMax(this int value, int max) { return value > max ? max : value; }
	public static float inverse(this int value)
	{
		if (value == 0)
		{
			return 0.0f;
		}
		return 1.0f / value;
	}
	public static float divide(this int value0, int value1, float defaultValue = 0)
	{
		return value1 != 0 ? (float)value0 / value1 : defaultValue;
	}
	public static float divide(this int value0, float value1, float defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static int divideInt(this int value0, int value1, int defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	// 通过加减cycle将value移入[min, max]范围（循环映射）
	// includeMax: true时max本身是合法值（value <= max），false时max被排除（value < max）
	// cycle: 每次调整的步长，如cycle=2时 value=7→5→3（每次减2）
	// 注意：如果cycle不能整除value与边界的距离，结果可能多次循环后停在区间内某个值
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
	public static bool inRange(this int value, int range0, int range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	public static bool inRange(this int value, float range0, float range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	public static bool inRangeFixed(this int value, int range0, int range1)
	{
		return value >= range0 && value <= range1;
	}
	public static bool inRangeFixed(this int value, float range0, float range1)
	{
		return value >= range0 && value <= range1;
	}
	// 计算批量处理时的总批次数
	// 例如：totalCount=10, batch=3 → 4批（3+3+3+1）
	public static int generateBatchCount(this int totalCount, int batch)
	{
		int batchCount = totalCount.divideInt(batch);
		return totalCount - batch * batchCount > 0 ? batchCount + 1 : batchCount;
	}
	public static int indexToX(this int index, int width) { return index % width; }
	public static int indexToY(this int index, int width) { return index.divideInt(width); }
	public static Vector2Int indexToIntPos(this int index, int width) { return new(index % width, index.divideInt(width)); }
	public static bool hasMask(this int value, int mask) { return (value & mask) != 0; }
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
	// 获得大于等于value的第一个2的n次方的数（value>0）
	// 策略：0~512查预计算表(mGreaterPow2)，之外分两档顺序查找（<2^15和≥2^15），比二分/顺序查找更快
	// 预计算表惰性初始化，带线程锁保护
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
	public static sbyte abs(this sbyte value) { return value >= 0 ? value : (sbyte)-value; }
	public static short abs(this short value) { return value >= 0 ? value : (short)-value; }
	public static byte clampMin(this byte value, byte min = 0) { return value < min ? min : value; }
	public static sbyte clampMin(this sbyte value, sbyte min = 0) { return value < min ? min : value; }
	public static short clampMin(this short value, short min = 0) { return value < min ? min : value; }
	public static ushort clampMin(this ushort value, ushort min = 0) { return value < min ? min : value; }
	public static uint clampMin(this uint value, uint min = 0) { return value < min ? min : value; }
	public static byte clampMax(this byte value, byte max) { return value > max ? max : value; }
	public static sbyte clampMax(this sbyte value, sbyte max) { return value > max ? max : value; }
	public static short clampMax(this short value, short max) { return value > max ? max : value; }
	public static ushort clampMax(this ushort value, ushort max) { return value > max ? max : value; }
	public static uint clampMax(this uint value, uint max) { return value > max ? max : value; }
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