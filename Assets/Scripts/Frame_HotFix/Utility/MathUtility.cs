using System;
using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static UnityUtility;
using static FrameUtility;

// 数学相关工具函数,所有与数学计算相关的函数都在这里
public class MathUtility
{
	private static List<AStarNode> mTempOpenList = new();	// 避免GC
	private static Point[] mTempDirect4 = new Point[4];						// 避免GC
	private static Point[] mTempDirect6 = new Point[6];						// 避免GC
	private static Point[] mTempDirect8 = new Point[8];						// 避免GC
	private static AStarNode[] mTempNodeList;                               // 避免GC
	private static ThreadLock mGreaterPow2Lock = new();
	private static int[] mGreaterPow2;										// 预先生成的每个数字所对应的第一个比它大的2的n次方的数
	private static float[] mSinList;										// PI/(2^n)的sin值,其中n是下标
	private static float[] mCosList;										// PI/(2^n)的cos值,其中n是下标
	private const int MAX_FFT_COUNT = 1024 * 8;								// 计算频域时数据的最大数量
	private static Complex[] mComplexList = new Complex[MAX_FFT_COUNT];     // 避免GC
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
	public static int generateBatchCount(int totalCount, int batch)
	{
		int batchCount = divideInt(totalCount, batch);
		return totalCount - batch * batchCount > 0 ? batchCount + 1 : batchCount;
	}
	public static int indexToX(int index, int width) { return index % width; }
	public static int indexToY(int index, int width) { return divideInt(index, width); }
	public static Vector2Int indexToIntPos(int index, int width) { return new(index % width, divideInt(index, width)); }
	public static int intPosToIndex(Vector2Int pos, int width) { return pos.x + pos.y * width; }
	public static int intPosToIndex(int x, int y, int width) { return x + y * width; }
	public static bool hasMask(int value, int mask) { return (value & mask) != 0; }
	public static float KMHtoMS(float kmh) { return kmh * 0.27777f; }
	public static float MStoKMH(float ms) { return ms * 3.6f; }
	public static float MtoKM(float m) { return m * 0.001f; }
	public static float pow(float value, float power) { return Mathf.Pow(value, power); }
	public static float pow(float value, int power)
	{
		float finalValue = 1.0f;
		for (int i = 0; i < power; ++i)
		{
			finalValue *= value;
		}
		return finalValue;
	}
	public static float inversePow10(int pow) { return INVERSE_POWER_INT_10[pow]; }
	public static int pow10(int pow) { return (int)POWER_INT_10[pow]; }
	public static double inversePow10Long(int pow) { return INVERSE_POWER_LLONG_10[pow]; }
	public static long pow10Long(int pow) { return POWER_LLONG_10[pow]; }
	public static float pow2(int power) { return 1 << power; }
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
	// 得到数轴上浮点数右边的第一个整数,向上取整
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
	public static void ceil(ref Vector2 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
	}
	public static Vector2 ceil(Vector2 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
		return value;
	}
	public static void ceil(ref Vector3 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
		value.z = ceil(value.z);
	}
	public static Vector3 ceil(Vector3 value)
	{
		value.x = ceil(value.x);
		value.y = ceil(value.y);
		value.z = ceil(value.z);
		return value;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(float value)
	{
		// 有时候会出现非常奇怪的现象,value显示是251,但是(int)value转换以后却是250,说明可能value实际是250.999999
		// 但是由于这种误差是非预期的,也就是说外边可能就是251*1,这种情况需要消除这种误差,使用checkInt即可消除
		checkInt(ref value);
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
	public static int floor(double value)
	{
		int intValue = (int)value;
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
	public static bool isNaN(Vector3 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z); }
	public static bool isNaN(Vector2 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y); }
	public static bool isNaN(float value) { return float.IsNaN(value); }
	public static void saturate(ref float value) { clamp(ref value, 0.0f, 1.0f); }
	public static float saturate(float value)
	{
		clamp(ref value, 0.0f, 1.0f);
		return value;
	}
	public static void saturate(ref Vector3 value)
	{
		value.x = saturate(value.x);
		value.y = saturate(value.y);
		value.z = saturate(value.z);
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
	public static long round(double value)
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
	public static void round(ref Vector3 value)
	{
		value.x = round(value.x);
		value.y = round(value.y);
		value.z = round(value.z);
	}
	public static Vector3 round(Vector3 value)
	{
		value.x = round(value.x);
		value.y = round(value.y);
		value.z = round(value.z);
		return value;
	}
	public static Vector3 floor(Vector3 value)
	{
		value.x = floor(value.x);
		value.y = floor(value.y);
		value.z = floor(value.z);
		return value;
	}
	// value1大于等于value0则返回1,否则返回0
	public static int step(float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	public static float fmod(float value0, float value1) { return value0 - value1 * (int)(divide(value0, value1)); }
	// 返回value的小数部分
	public static float frac(float value) { return value - (int)value; }
	public static float abs(float value) { return value >= 0.0f ? value : -value; }
	public static double abs(double value) { return value >= 0.0 ? value : -value; }
	public static sbyte abs(sbyte value) { return value >= 0 ? value : (sbyte)-value; }
	public static short abs(short value) { return value >= 0 ? value : (short)-value; }
	public static int abs(int value) { return value >= 0 ? value : -value; }
	public static long abs(long value) { return value >= 0 ? value : -value; }
	public static float sin(float radian) { return Mathf.Sin(radian); }
	public static float cos(float radian) { return Mathf.Cos(radian); }
	public static float tan(float radian) { return Mathf.Tan(radian); }
	public static float atan(float value) { return Mathf.Atan(value); }
	public static float asin(float value)
	{
		clamp(ref value, -1.0f, 1.0f);
		return Mathf.Asin(value);
	}
	public static float acos(float value)
	{
		clamp(ref value, -1.0f, 1.0f);
		return Mathf.Acos(value);
	}
	public static float atan2(float y, float x) { return Mathf.Atan2(y, x); }
	public static float dot(Vector3 v0, Vector3 v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	public static float dot(Vector2 v0, Vector2 v1) { return v0.x * v1.x + v0.y * v1.y; }
	public static float sqrt(float value) { return Mathf.Sqrt(value); }
	public static Vector3 cross(Vector3 v0, Vector3 v1) { return Vector3.Cross(v0, v1); }
	// 计算二维空间的向量叉积,大于0表示point在线段的左边,小于0表示point在线段的右边,等于0表示三点共线
	public static float crossProduct(Vector2 start, Vector2 end, Vector2 point) { return (end.x - start.x) * (point.y - start.y) - (end.y - start.y) * (point.x - start.x); }
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
	public static int sign(sbyte value)
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
	public static int sign(uint value0, uint value1)
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
	public static int sign(long value)
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
	// 如果是钝角,则获取对应的锐角,弧度制,但是如果大于180度,则可能返回错误的值
	public static float toAcuteAngleRadian(float angle)
	{
		return angle >= HALF_PI_RADIAN ? PI_RADIAN - angle : angle;
	}
	// 如果是钝角,则获取对应的锐角,角度制,但是如果大于180度,则可能返回错误的值
	public static float toAcuteAngleDegree(float angle)
	{
		return angle >= HALF_PI_DEGREE ? PI_DEGREE - angle : angle;
	}
	// value是否是2的n次方
	public static bool isPow2(int value) { return (value & (value - 1)) == 0; }
	// 是否为偶数
	// 对于a % b的计算,如果b为2的n次方,则a % b等效于a & (b - 1)
	public static bool isEven(int value) { return (value & 1) == 0; }
	public static float getNearest(float value, float p0, float p1) { return abs(value - p0) < abs(value - p1) ? p0 : p1; }
	public static float getFarthest(float value, float p0, float p1) { return abs(value - p0) > abs(value - p1) ? p0 : p1; }
	public static int getCharCount(string str, char c)
	{
		int count = 0;
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (str[i] == c)
			{
				++count;
			}
		}
		return count;
	}
	public static float calculateFloat(string str)
	{
		// 判断字符串是否含有非法字符,也就是除数字,小数点,运算符以外的字符
		checkFloatString(str, "+-*/()");
		// 计算错误,左右括号数量不对应
		if (getCharCount(str, '(') != getCharCount(str, ')'))
		{
			return 0;
		}

		// 循环判断传入的字符串有没有括号
		while (true)
		{
			// 先判断有没有括号，如果有括号就先算括号里的,如果没有就退出while循环
			if (str.IndexOf('(') == -1 && str.IndexOf(')') == -1)
			{
				break;
			}
			int curPos = str.LastIndexOf('(');
			string strInBracket = str.removeStartCount(curPos + 1).rangeToFirst(')');
			float ret = calculateFloat(strInBracket);
			// 如果括号中的计算结果是负数,则标记为负数
			bool isMinus = ret < 0;
			ret = abs(ret);
			// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
			str = str.replace(curPos, curPos + strInBracket.Length + 2, Math.Round(ret, 4).ToString());
			if (isMinus)
			{
				// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
				bool changeMark = false;
				for (int i = curPos - 1; i >= 0; --i)
				{
					// 找到第一个+号,则直接改为减号,然后退出遍历
					if (str[i] == '+')
					{
						str = str.replace(i, i + 1, "-");
						changeMark = true;
						break;
					}
					// 找到第一个减号,如果减号的左边有数字,则直接改为+号
					// 如果减号的左边不是数字,则该减号是负号,将减号去掉,
					else if (str[i] == '-')
					{
						if (isNumeric(str[i - 1]))
						{
							str = str.replace(i, i + 1, "+");
						}
						else
						{
							str = str.Remove(i, 1);
						}
						changeMark = true;
						break;
					}
				}
				// 如果遍历完了还没有找到可以替换的符号,则在表达式最前面加一个负号
				if (!changeMark)
				{
					str = "-" + str;
				}
			}
		}
		List<float> numbers = new();
		List<char> factors = new();
		// 表示上一个运算符的下标+1
		int beginPos = 0;
		for (int i = 0; i < str.Length; ++i)
		{
			// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
			if (i == str.Length - 1)
			{
				numbers.Add(SToF(str.removeStartCount(beginPos)));
				break;
			}
			// 找到第一个运算符
			if (!isNumeric(str[i]) && str[i] != '.')
			{
				if (i != 0)
				{
					numbers.Add(SToF(str.range(beginPos, i)));
				}
				// 如果在表达式的开始就发现了运算符,则表示第一个数是负数,那就处理为0减去这个数的绝对值
				else
				{
					numbers.Add(0);
				}
				factors.Add(str[i]);
				beginPos = i + 1;
			}
		}
		if (factors.Count + 1 != numbers.Count)
		{
			// 计算错误,运算符与数字数量不符
			return 0;
		}
		// 现在开始计算表达式,按照运算优先级,先计算乘除和取余
		while (true)
		{
			// 表示是否还有乘除表达式
			bool hasMS = false;
			for (int i = 0; i < factors.Count; ++i)
			{
				// 先遍历到哪个就先计算哪个
				if (factors[i] == '*' || factors[i] == '/')
				{
					// 第一个运算数的下标与运算符的下标是相同的
					float num1 = numbers[i];
					float num2 = numbers[i + 1];
					float num3 = 0.0f;
					if (factors[i] == '*')
					{
						num3 = num1 * num2;
					}
					else if (factors[i] == '/')
					{
						num3 = divide(num1, num2);
					}
					// 删除第i + 1个数,然后将第i个数替换为计算结果
					numbers.RemoveAt(i + 1);
					if (numbers.Count == 0)
					{
						// 计算错误
						return 0;
					}
					numbers[i] = num3;
					// 删除第i个运算符
					factors.RemoveAt(i);
					hasMS = true;
					break;
				}
			}
			if (!hasMS)
			{
				break;
			}
		}
		// 再计算加减法
		while (true)
		{
			if (factors.Count == 0)
			{
				break;
			}
			if (factors[0] == '+' || factors[0] == '-')
			{
				// 第一个运算数的下标与运算符的下标是相同的
				float num1 = numbers[0];
				float num2 = numbers[1];
				float num3 = 0.0f;
				if (factors[0] == '+')
				{
					num3 = num1 + num2;
				}
				else if (factors[0] == '-')
				{
					num3 = num1 - num2;
				}
				// 删除第1个数,然后将第0个数替换为计算结果
				numbers.RemoveAt(1);
				if (numbers.Count == 0)
				{
					// 计算错误
					return 0;
				}
				numbers[0] = num3;
				// 删除第0个运算符
				factors.RemoveAt(0);
			}
		}
		if (numbers.Count != 1)
		{
			return numbers[0];
		}
		// 计算错误
		return 0;
	}
	// 将一个浮点数调整保留一定的小数位,保留的最后一位四舍五入.precision表示小数点后保留几位小数
	public static void checkFloat(ref float value, int precision = 4)
	{
		float helper = pow10(precision);
		value = divide(round(value * helper), helper);
	}
	public static void checkFloat(ref Vector3 value, int precision = 4)
	{
		checkFloat(ref value.x, precision);
		checkFloat(ref value.y, precision);
		checkFloat(ref value.z, precision);
	}
	public static void checkInt(ref Vector3 vec, float precision = 0.0001f)
	{
		checkInt(ref vec.x, precision);
		checkInt(ref vec.y, precision);
		checkInt(ref vec.z, precision);
	}
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
					value = intValue + 1;
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
					value = intValue - 1;
				}
			}
		}
	}
	public static void randomSelect(List<int> oddsList, int selectCount, List<int> selectIndexes)
	{
		int allCount = oddsList.Count;
		int max = 0;
		for (int i = 0; i < allCount; ++i)
		{
			max += oddsList[i];
		}
		selectIndexes.Clear();
		if (selectCount >= allCount)
		{
			selectIndexes.Capacity = allCount;
			for (int i = 0; i < allCount; ++i)
			{
				selectIndexes.Add(i);
			}
			return;
		}

		selectIndexes.Capacity = selectCount;
		for (int i = 0; i < selectCount; ++i)
		{
			int random = randomInt(0, max);
			int curValue = 0;
			int count = oddsList.Count;
			for (int j = 0; j < count; ++j)
			{
				// 已经被选中的下标就需要排除掉
				if (selectIndexes.Contains(j))
				{
					continue;
				}
				curValue += oddsList[j];
				if (random <= curValue)
				{
					selectIndexes.Add(j);
					// 选出一个就去除此下标的权重
					max -= oddsList[j];
					break;
				}
			}
		}
	}
	public static int randomSelect(List<int> oddsList, int selectCount, Span<int> selectIndexes)
	{
		int allCount = oddsList.Count;
		int max = 0;
		for (int i = 0; i < allCount; ++i)
		{
			max += oddsList[i];
		}
		selectIndexes.Clear();
		if (selectCount >= allCount)
		{
			for (int i = 0; i < allCount; ++i)
			{
				selectIndexes[i] = i;
			}
			return allCount;
		}

		int curCount = 0;
		for (int i = 0; i < selectCount; ++i)
		{
			int random = randomInt(0, max);
			int curValue = 0;
			int count = oddsList.Count;
			for (int j = 0; j < count; ++j)
			{
				// 已经被选中的下标就需要排除掉
				bool contains = false;
				for (int k = 0; k < curCount; ++k)
				{
					if (selectIndexes[k] == j)
					{
						contains = true;
						break;
					}
				}
				if (contains)
				{
					continue;
				}
				curValue += oddsList[j];
				if (random <= curValue)
				{
					selectIndexes[curCount++] = j;
					// 选出一个就去除此下标的权重
					max -= oddsList[j];
					break;
				}
			}
		}
		return curCount;
	}
	public static void randomSelect(int allCount, int selectCount, List<int> selectIndexes)
	{
		selectIndexes.Clear();
		if (selectCount >= allCount)
		{
			for (int i = 0; i < allCount; ++i)
			{
				selectIndexes.Add(i);
			}
			return;
		}
		Span<int> helpList = stackalloc int[allCount];
		for (int i = 0; i < allCount; ++i)
		{
			helpList[i] = i;
		}	
		for (int i = 0; i < selectCount; ++i)
		{
			int index = randomInt(i, allCount - 1);
			selectIndexes.Add(helpList[index]);
			// 选中的下标跟第一个交换,再次随机
			if (index != i)
			{
				int temp = helpList[index];
				helpList[index] = helpList[i];
				helpList[i] = temp;
			}
		}
	}
	// 非线程安全,根据一定几率随机返回true或false,实际几率为probability除以scale
	// 一般scale应该尽量大一些,比如1万,10万,100万,以确保随机几率正常
	public static bool randomHit(int probability, int scale)
	{
		if (probability == 0)
		{
			return false;
		}
		if (probability >= scale)
		{
			return true;
		}
		return randomInt(0, scale - 1) < probability;
	}
	// 一定几率返回true,几率范围是0.0~1.0
	public static bool randomHit(float odds)
	{
		// 几率已经大于等于1,则直接返回true
		if (odds >= 1.0f)
		{
			return true;
		}
		// 几率小于等于0,则直接返回false
		if (odds <= 0.0f)
		{
			return false;
		}
		return randomFloat(0.0f, 1.0f) < odds;
	}
	// 根据几率随机选择一个下标,oddsList中的元素是权重,几率就是权重除以所有权重的和
	public static int randomHit(List<ushort> oddsList)
	{
		if (oddsList.isEmpty())
		{
			return 0;
		}
		int max = 0;
		int count = oddsList.Count;
		for (int i = 0; i < count; ++i)
		{
			max += oddsList[i];
		}
		int random = randomInt(0, max);
		int curValue = 0;
		for (int i = 0; i < count; ++i)
		{
			curValue += oddsList[i];
			if (random <= curValue && oddsList[i] > 0)
			{
				return i;
			}
		}
		return 0;
	}
	// 根据几率随机选择一个下标,oddsList中的元素是权重,几率就是权重除以所有权重的和
	public static int randomHit(List<int> oddsList)
	{
		if (oddsList.isEmpty())
		{
			return 0;
		}
		int max = 0;
		int count = oddsList.Count;
		for (int i = 0; i < count; ++i)
		{
			max += oddsList[i];
		}
		int random = randomInt(0, max);
		int curValue = 0;
		for (int i = 0; i < count; ++i)
		{
			curValue += oddsList[i];
			if (random <= curValue && oddsList[i] > 0)
			{
				return i;
			}
		}
		return 0;
	}
	// 根据几率随机选择一个下标,oddsList中的元素是权重,几率就是权重除以所有权重的和
	public static int randomHit(List<float> oddsList)
	{
		if (oddsList.isEmpty())
		{
			return 0;
		}
		float max = 0.0f;
		int count = oddsList.Count;
		for (int i = 0; i < count; ++i)
		{
			max += oddsList[i];
		}
		float random = randomFloat(0.0f, max);
		float curValue = 0.0f;
		for (int i = 0; i < count; ++i)
		{
			curValue += oddsList[i];
			if (random <= curValue && oddsList[i] > 0.0f)
			{
				return i;
			}
		}
		return 0;
	}
	// 根据几率随机选择一个下标,oddsList中的元素是权重,几率就是权重除以所有权重的和
	public static int randomHit(Span<float> oddsList, int count)
	{
		if (oddsList.isEmptySpan() || count == 0)
		{
			return 0;
		}
		float max = 0.0f;
		for (int i = 0; i < count; ++i)
		{
			max += oddsList[i];
		}
		float random = randomFloat(0.0f, max);
		float curValue = 0.0f;
		for (int i = 0; i < count; ++i)
		{
			curValue += oddsList[i];
			if (random <= curValue && oddsList[i] > 0.0f)
			{
				return i;
			}
		}
		return 0;
	}
	// 获取一个min和max之间的随机浮点数
	public static float randomFloat(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}
	// 获取一个min和max之间的随机整数,包含max
	public static int randomInt(int min, int max)
	{
		if (min == max)
		{
			return min;
		}
		return UnityEngine.Random.Range(min, max + 1);
	}
	// 计算射线与平面的交点,ray为射线，normal为平面法线，point为平面上一点
	public static Vector3 intersectRayPlane(Ray ray, Vector3 normal, Vector3 point)
	{
		// 先计算出射线起点在平面上的投影
		Vector3 originProjectPoint = getProjectionOnPlane(point, normal, ray.origin);
		// 计算射线与法线的夹角,弧度制,算出来的角度可能是一个锐角也可能是钝角，与射线的方向和法线方向有关
		float angle = getAngleBetweenVector(normal, ray.direction);
		// 射线起点到起点的投影点的距离
		float originToProjectDistance = getLength(originProjectPoint - ray.origin);
		// 根据夹角计算出射线交点到射线起始点的距离,如果夹角是钝角，则还是取剩下的锐角的部分
		// 因为射线只要不与平面平行，就肯定会有一个小于90°的角度
		float distance = divide(originToProjectDistance, abs(cos(angle)));
		// 根据距离计算出交点
		return ray.origin + ray.direction * distance;
	}
	// 计算两个向量所在平面的法线,unity是左手坐标系
	public static Vector3 generateNormal(Vector3 vec0, Vector3 vec1)
	{
		return cross(vec0, vec1);
	}
	// 计算点point在平面planePoint-normal上的投影点,planePoint为平面上一点,normal为平面法线
	// planePoint在normal-point上的投影点即为交点
	public static Vector3 getProjectionOnPlane(Vector3 planePoint, Vector3 normal, Vector3 point)
	{
		return getProjectPoint(planePoint, new(point, point + normal));
	}
	// 判断一个点在平面的正面还是反面,-1表示反面,也就是在法线负方向,0表示在平面上,1表示在正面
	public static int getPointInPlaneSide(Vector3 planePoint, Vector3 normal, Vector3 point)
	{
		Vector3 dir = normalize(point - planePoint);
		float dotResult = dot(dir, normal);
		// 在平面上
		if (isFloatZero(dotResult))
		{
			return 0;
		}
		return (int)sign(dotResult);
	}
	// 判断两个点是否在线同一边
	public static bool isSameSidePoint(Line2 line, Vector2 point0, Vector2 point1)
	{
		int angle0 = getAngleSignVector2ToVector2(point0 - line.mStart, line.mEnd - line.mStart);
		int angle1 = getAngleSignVector2ToVector2(point1 - line.mStart, line.mEnd - line.mStart);
		return sign(angle0) == sign(angle1);
	}
	// 任意一个点是否在一个线段上
	public static bool isPointInSection(Vector2 point, Line2 line)
	{
		Vector2 v0 = point - line.mStart;
		Vector2 v1 = point - line.mEnd;
		// 交点与端点重合
		if (isVectorZero(v0) || isVectorZero(v1))
		{
			return true;
		}
		normalize(ref v0);
		normalize(ref v1);
		// 两个向量方向相反则为在线段上,同向则交点不在线段上
		return isVectorZero(v0 + v1);
	}
	// 三个点是否在同一条直线上
	public static bool isPointsInSameLine2(Vector2 point0, Vector2 point1, Vector2 point2)
	{
		Vector2 v0 = point0 - point1;
		Vector2 v1 = point0 - point2;
		// 交点与端点重合
		if (isVectorZero(v0) || isVectorZero(v1))
		{
			return true;
		}
		normalize(ref v0);
		normalize(ref v1);
		// 两个向量方向相反或者相同,就在同一条直线上
		return isVectorZero(v0 + v1) || isVectorEqual(v0, v1);
	}
	// 三个点是否在同一条直线上
	public static bool isPointsInSameLine3(Vector3 point0, Vector3 point1, Vector3 point2)
	{
		Vector3 v0 = point0 - point1;
		Vector3 v1 = point0 - point2;
		// 交点与端点重合
		if (isVectorZero(v0) || isVectorZero(v1))
		{
			return true;
		}
		normalize(ref v0);
		normalize(ref v1);
		// 两个向量方向相反或者相同,就在同一条直线上
		return isVectorZero(v0 + v1) || isVectorEqual(v0, v1);
	}
	// 直线与圆的相交检测
	public static bool intersectCircle(Vector2 center, float radius, Line2 line, ref PolygonIntersectResult result)
	{
		// 计算圆心到直线的距离,如果距离大于半径,则直线和圆不相交
		// 圆心在直线上的投影
		Vector2 projectPoint = getProjectPoint(center, line.toLine3());
		float squaredRadius = radius * radius;
		float pointToLineSquaredDistance = getSquaredLength(center - projectPoint);
		// 直线与圆相切
		if (isFloatEqual(pointToLineSquaredDistance, squaredRadius))
		{
			result.mIntersectPoint0 = projectPoint;
			result.mIntersectPoint1 = projectPoint;
			result.mLine0.mStart = result.mIntersectPoint0;
			generatePerpendicular(center, projectPoint, out result.mLine0.mEnd);
			result.mLine1.mStart = result.mLine0.mStart;
			result.mLine1.mEnd = result.mLine0.mEnd;
			return true;
		}
		// 直线与圆相交
		else if (pointToLineSquaredDistance < squaredRadius)
		{
			// 根据直线的方向和交点到圆心投影点的距离计算两个相交点
			Vector2 normalizedLineDir = normalize(line.mStart - line.mEnd);
			float distanceFromIntersectToProjection = sqrt(squaredRadius - pointToLineSquaredDistance);
			result.mIntersectPoint0 = projectPoint + normalizedLineDir * distanceFromIntersectToProjection;
			result.mIntersectPoint1 = projectPoint - normalizedLineDir * distanceFromIntersectToProjection;
			result.mLine0.mStart = result.mIntersectPoint0;
			generatePerpendicular(center, result.mIntersectPoint0, out result.mLine0.mEnd);
			result.mLine1.mStart = result.mIntersectPoint1;
			generatePerpendicular(center, result.mIntersectPoint1, out result.mLine1.mEnd);
			return true;
		}
		return false;
	}
	// 计算多边形和直线的交点,points为形状的顶点列表,只支持凸多边形,不支持凹多边形,因为凹多边形跟直线相交的交点数量可能不止2个
	// transform表示多边形的变换,包含平移,旋转,缩放
	// 忽略Z轴
	public static bool intersectPolygon(List<Vector2> points, Transform transform, Line2 line, ref PolygonIntersectResult result)
	{
		int pointCount = points.Count;
		if (pointCount < 3)
		{
			return false;
		}
		Vector2 intersect;
		int intersectCount = 0;
		for (int i = 0; i < pointCount; ++i)
		{
			// 首先对每条边进行变换
			Vector3 point0 = points[i];
			Vector3 point1 = points[(i + 1) % pointCount];
			point0 = rotateVector3(point0, transform.localRotation);
			point0 = multiVector3(point0, transform.localScale);
			point0 += transform.localPosition;

			point1 = rotateVector3(point1, transform.localRotation);
			point1 = multiVector3(point1, transform.localScale);
			point1 += transform.localPosition;
			// 与每一条边进行相交检测
			if (!intersectLineLineSection(line, new(point0, point1), out intersect))
			{
				continue;
			}
			if (intersectCount == 0)
			{
				result.mIntersectPoint0 = intersect;
				result.mLine0.mStart = point0;
				result.mLine0.mEnd = point1;
				++intersectCount;
			}
			else if (intersectCount == 1)
			{
				// 判断是否与第一个点是同一个点,因为当相交点为其中一个顶点时,会计算出重合的相交点
				if (!isVectorEqual(result.mIntersectPoint0, intersect))
				{
					result.mIntersectPoint1 = intersect;
					result.mLine1.mStart = point0;
					result.mLine1.mEnd = point1;
					++intersectCount;
					break;
				}
			}
		}
		// 如果最终只有一个交点,则将另外一个交点也设置为该值
		if (intersectCount == 1)
		{
			result.mIntersectPoint1 = result.mIntersectPoint0;
			result.mLine1.mStart = result.mLine0.mStart;
			result.mLine1.mEnd = result.mLine0.mEnd;
		}
		return intersectCount > 0;
	}
	// 计算一条直线的在一个点point上的垂线,otherPoint表示计算出的垂线上的一个点
	public static void generatePerpendicular(Vector2 linePoint0, Vector2 point, out Vector2 otherPoint)
	{
		// 转换为3维向量,通过与Y轴的叉乘得到垂直的向量
		Vector3 v0 = normalize(new Vector3(linePoint0.x, 0.0f, linePoint0.y) - new Vector3(point.x, 0.0f, point.y));
		Vector3 v2 = cross(v0, Vector3.up);
		otherPoint = point + new Vector2(v2.x, v2.z);
	}
	// 计算一条直线经过point点的平行线
	public static void generateParallel(Line3 line, Vector3 point, out Vector3 otherPoint)
	{
		otherPoint = point + line.mEnd - line.mStart;
	}
	// 计算一条直线经过point点的平行线
	public static void generateParallel(Line2 line, Vector2 point, out Vector2 otherPoint)
	{
		otherPoint = point + line.mEnd - line.mStart;
	}
	// 第index0个点和第index1个点的连线是否位于多边形内部,并且不与任何边相交,要求点列表是以逆时针来排列的
	public static bool canConnectPoint(List<Vector2> vertice, int index0, int index1)
	{
		int verticeCount = vertice.Count;
		if (verticeCount < 4)
		{
			return true;
		}
		Vector2 lastPoint = vertice[(index0 - 1 + vertice.Count) % vertice.Count];
		Vector2 nextPoint = vertice[(index0 + 1) % vertice.Count];
		Vector2 curPoint = vertice[index0];
		// 当前点的相邻两个点的从下一个点的连线到上一个点的连线的角度
		Vector2 lastDir = lastPoint - curPoint;
		Vector2 nextDir = nextPoint - curPoint;
		float angle0 = getAngleVector2ToVector2(lastDir, nextDir);
		adjustRadian360(ref angle0);
		float angle1 = getAngleVector2ToVector2(vertice[index1] - curPoint, nextDir);
		adjustRadian360(ref angle1);
		// 法线向上时,点是按逆时针来排列,夹角小于邻边角时才有效
		bool validAngle = angle1 <= angle0;
		// 夹角需要在一定范围内
		if (!validAngle)
		{
			return false;
		}
		// 判断是否与其他边相交
		for (int i = 0; i < verticeCount; ++i)
		{
			// 先判断是否有端点重合,端点重合时认为不相交
			if (i == index0 || i == index1 || (i + 1) % verticeCount == index0 || (i + 1) % verticeCount == index1)
			{
				continue;
			}
			if (intersectLineSection(vertice[i], vertice[(i + 1) % verticeCount], vertice[index0], vertice[index1], out _))
			{
				return false;
			}
		}
		return true;
	}
	// 计算两条线段的交点,返回值表示两条线段是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersectLineSectionIgnoreY(Line3 line0, Line3 line1, out Vector3 intersect, bool checkEndPoint = false)
	{
		Vector2 start0 = new(line0.mStart.x, line0.mStart.z);
		Vector2 end0 = new(line0.mEnd.x, line0.mEnd.z);
		Vector2 start1 = new(line1.mStart.x, line1.mStart.z);
		Vector2 end1 = new(line1.mEnd.x, line1.mEnd.z);
		bool result = intersectLineSection(start0, end0, start1, end1, out Vector2 intersect2, checkEndPoint);
		intersect = new(intersect2.x, 0.0f, intersect2.y);
		return result;
	}
	// 计算两条线段的交点,返回值表示两条线段是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersectLineSection(Vector2 start0, Vector2 end0, Vector2 start1, Vector2 end1, out Vector2 intersection, bool checkEndPoint = false, float precision = 0.0001f)
	{
		// 有端点重合
		if (isVectorEqual(start0, start1) ||
			isVectorEqual(start0, end1) ||
			isVectorEqual(end0, start1) ||
			isVectorEqual(end0, end1))
		{
			// 考虑端点时认为两条线段相交
			// 不考虑端点时,则两条线段不相交
			intersection = Vector2.zero;
			return checkEndPoint;
		}
		intersection = Vector2.zero;
		Vector2 d1 = end0 - start0;
		Vector2 d2 = end1 - start1;
		// 计算两个线段方向向量的叉积
		float cross = d1.x * d2.y - d1.y * d2.x;
		// 如果叉积为0，则平行或共线，无交点
		if (abs(cross) < precision)
		{
			return false;
		}
		// 计算 t 和 u 的值
		Vector2 pToQ = start1 - start0;
		float t = (pToQ.x * d2.y - pToQ.y * d2.x) / cross;
		float u = (pToQ.x * d1.y - pToQ.y * d1.x) / cross;
		// 检查 t 和 u 是否都在 [0, 1] 范围内
		if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
		{
			intersection = start0 + t * d1;
			return true;
		}
		return false;
	}
	// 计算直线和线段的交点,返回值表示是否相交
	public static bool intersectLineLineSection(Line2 line, Line2 section, out Vector2 intersect)
	{
		if (intersectLine2(line, section, out intersect))
		{
			// 如果交点在线段内,则线段和直线相交
			return inRange(intersect, section.mStart, section.mEnd);
		}
		return false;
	}
	// 在忽略Y轴的情况下,判断两条直线是否相交,如果相交,则计算交点
	public static bool intersectLineIgnoreY(Line3 line0, Line3 line1, out Vector3 intersect)
	{
		bool ret = intersectLine2(line0.toLine2IgnoreY(), line1.toLine2IgnoreY(), out Vector2 point);
		intersect = new(point.x, 0.0f, point.y);
		return ret;
	}
	// 在忽略X轴的情况下,判断两条直线是否相交,如果相交,则计算交点
	public static bool intersectLineIgnoreX(Line3 line0, Line3 line1, out Vector3 intersect)
	{
		bool ret = intersectLine2(line0.toLine2IgnoreX(), line1.toLine2IgnoreX(), out Vector2 point);
		intersect = new(0.0f, point.y, point.x);
		return ret;
	}
	// 计算两条直线的交点,返回值表示两条直线是否相交
	public static bool intersectLine2(Line2 line0, Line2 line1, out Vector2 intersect)
	{
		// 两条竖直的线没有交点,即使两条竖直的线重合也不计算交点
		if (!line0.mHasK && !line1.mHasK)
		{
			intersect = Vector2.zero;
			return false;
		}
		// 直线0为竖直的线
		else if (!line0.mHasK)
		{
			intersect.x = line0.mStart.x;
			intersect.y = line1.mK * intersect.x + line1.mB;
			return true;
		}
		// 直线1为竖直的线
		else if (!line1.mHasK)
		{
			intersect.x = line1.mStart.x;
			intersect.y = line0.mK * intersect.x + line0.mB;
			return true;
		}
		else
		{
			// 两条不重合且不平行的两条线才计算交点
			if (!isFloatEqual(line0.mK, line1.mK))
			{
				intersect.x = divide(line1.mB - line0.mB, line0.mK - line1.mK);
				intersect.y = line0.mK * intersect.x + line0.mB;
				return true;
			}
		}
		intersect = Vector2.zero;
		return false;
	}
	// k为斜率,也就是cotan(直线与y轴的夹角)
	public static bool generateLineExpressionIgnoreY(Line3 line, out float k, out float b)
	{
		// 一条横着的线,斜率为0
		if (isFloatZero(line.mStart.z - line.mEnd.z))
		{
			k = 0.0f;
			b = line.mStart.z;
		}
		// 直线是一条竖直的线,没有斜率
		else if (isFloatZero(line.mStart.x - line.mEnd.x))
		{
			k = 0.0f;
			b = 0.0f;
			return false;
		}
		else
		{
			k = divide(line.mStart.z - line.mEnd.z, line.mStart.x - line.mEnd.x);
			b = line.mStart.z - k * line.mStart.x;
		}
		return true;
	}
	// k为斜率,也就是cotan(直线与y轴的夹角)
	public static bool generateLineExpression(Line2 line, out float k, out float b)
	{
		// 一条横着的线,斜率为0
		if (isFloatZero(line.mStart.y - line.mEnd.y))
		{
			k = 0.0f;
			b = line.mStart.y;
		}
		// 直线是一条竖直的线,没有斜率
		else if (isFloatZero(line.mStart.x - line.mEnd.x))
		{
			k = 0.0f;
			b = 0.0f;
			return false;
		}
		else
		{
			k = divide(line.mStart.y - line.mEnd.y, line.mStart.x - line.mEnd.x);
			b = line.mStart.y - k * line.mStart.x;
		}
		return true;
	}
	// 当忽略端点重合,如果有端点重合，则判断为不相交
	// 线段与矩形是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersectIgnoreY(Line3 line, Rect3 rect3, float precision = 0.0001f, bool checkEndPoint = false)
	{
		return intersect(line.toLine2IgnoreY(), rect3.toRect(), precision, checkEndPoint);
	}
	// 当忽略端点重合,如果有端点重合，则判断为不相交
	// 线段与矩形是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersect(Line2 line, Rect rect, float precision = 0.0001f, bool checkEndPoint = false)
	{
		// 距离大于对角线的一半,则不与矩形相交
		if (getDistanceToLine(rect.center, line) > getLength(rect.max - rect.min) * 0.5f)
		{
			return false;
		}
		// 直线是否与任何一条边相交
		Vector2 leftTop = rect.min + new Vector2(0.0f, rect.height);
		Vector2 rightTop = rect.max;
		Vector2 rightBottom = rect.min + new Vector2(rect.width, 0.0f);
		Vector2 leftBottom = rect.min;
		return intersectLineSection(line.mStart, line.mEnd, leftTop, rightTop, out _, checkEndPoint, precision) ||
			   intersectLineSection(line.mStart, line.mEnd, leftBottom, rightBottom, out _, checkEndPoint, precision) ||
			   intersectLineSection(line.mStart, line.mEnd, leftBottom, leftTop, out _, checkEndPoint, precision) ||
			   intersectLineSection(line.mStart, line.mEnd, rightBottom, rightTop, out _, checkEndPoint, precision);
	}
	// 计算线段与三角形是否相交
	public static bool intersectLineTriangleIgnoreY(Line3 line, Triangle3 triangle, out TriangleIntersectResult3 intersectResult, bool checkEndPoint = false)
	{
		line.mStart = resetY(line.mStart);
		line.mEnd = resetY(line.mEnd);
		// 对三条边都要检测,计算出最近的一个交点
		bool result0 = intersectLineSectionIgnoreY(line, new(triangle.mPoint0, triangle.mPoint1), out Vector3 intersect0, checkEndPoint);
		bool result1 = intersectLineSectionIgnoreY(line, new(triangle.mPoint1, triangle.mPoint2), out Vector3 intersect1, checkEndPoint);
		bool result2 = intersectLineSectionIgnoreY(line, new(triangle.mPoint2, triangle.mPoint0), out Vector3 intersect2, checkEndPoint);
		Vector3 point = Vector3.zero;
		Vector3 linePoint0 = Vector3.zero;
		Vector3 linePoint1 = Vector3.zero;
		float closestDistance = float.MaxValue;
		// 与第一条边相交
		if (result0)
		{
			float squaredLength = getSquaredLength(intersect0 - line.mStart);
			if (squaredLength < closestDistance)
			{
				closestDistance = squaredLength;
				point = intersect0;
				linePoint0 = triangle.mPoint0;
				linePoint1 = triangle.mPoint1;
			}
		}
		// 与第二条边相交
		if (result1)
		{
			float squaredLength = getSquaredLength(intersect1 - line.mStart);
			if (squaredLength < closestDistance)
			{
				closestDistance = squaredLength;
				point = intersect1;
				linePoint0 = triangle.mPoint1;
				linePoint1 = triangle.mPoint2;
			}
		}
		// 与第三条边相交
		if (result2)
		{
			float squaredLength = getSquaredLength(intersect2 - line.mStart);
			if (squaredLength < closestDistance)
			{
				point = intersect2;
				linePoint0 = triangle.mPoint2;
				linePoint1 = triangle.mPoint0;
			}
		}
		intersectResult = new();
		intersectResult.mIntersectPoint = point;
		intersectResult.mLinePoint0 = linePoint0;
		intersectResult.mLinePoint1 = linePoint1;
		return result0 || result1 || result2;
	}
	// 计算线段与三角形是否相交
	public static bool intersectLineTriangle(Line2 line, Triangle2 triangle, out TriangleIntersectResult intersectResult, float precision = 0.0001f, bool checkEndPoint = false)
	{
		// 对三条边都要检测,计算出最近的一个交点
		Vector2 start0 = line.mStart;
		Vector2 end0 = line.mEnd;
		bool result0 = intersectLineSection(start0, end0, triangle.mPoint0, triangle.mPoint1, out Vector2 intersect0, checkEndPoint, precision);
		bool result1 = intersectLineSection(start0, end0, triangle.mPoint1, triangle.mPoint2, out Vector2 intersect1, checkEndPoint, precision);
		bool result2 = intersectLineSection(start0, end0, triangle.mPoint2, triangle.mPoint0, out Vector2 intersect2, checkEndPoint, precision);
		Vector2 point = Vector2.zero;
		Vector2 linePoint0 = Vector2.zero;
		Vector2 linePoint1 = Vector2.zero;
		float closestDistance = float.MaxValue;
		// 与第一条边相交
		if (result0)
		{
			float squaredLength = getSquaredLength(intersect0 - line.mStart);
			if(squaredLength < closestDistance)
			{
				closestDistance = squaredLength;
				point = intersect0;
				linePoint0 = triangle.mPoint0;
				linePoint1 = triangle.mPoint1;
			}
		}
		// 与第二条边相交
		if (result1)
		{
			float squaredLength = getSquaredLength(intersect1 - line.mStart);
			if (squaredLength < closestDistance)
			{
				closestDistance = squaredLength;
				point = intersect1;
				linePoint0 = triangle.mPoint1;
				linePoint1 = triangle.mPoint2;
			}
		}
		// 与第三条边相交
		if (result2)
		{
			float squaredLength = getSquaredLength(intersect2 - line.mStart);
			if (squaredLength < closestDistance)
			{
				point = intersect2;
				linePoint0 = triangle.mPoint2;
				linePoint1 = triangle.mPoint0;
			}
		}
		intersectResult = new();
		intersectResult.mIntersectPoint = point;
		intersectResult.mLinePoint0 = linePoint0;
		intersectResult.mLinePoint1 = linePoint1;
		return result0 || result1 || result2;
	}
	// 计算线段与三角形是否相交
	public static bool intersectLineTriangle(Line2 line, Triangle2 triangle, out Vector2 intersectPoint)
	{
		Vector2 lineDir = normalize(line.mEnd - line.mStart);
		bool ret = intersectRayTriangle(line.mStart, lineDir, triangle.toTriangle3(), out float t, out _, out _);
		if (ret)
		{
			intersectPoint = line.mStart + lineDir * t;
			// 如果交点超出了线段范围,则不相交
			float lineLength = getLength(line.mEnd - line.mStart);
			if(t <= 0.0f || t >= lineLength)
			{
				ret = false;
			}
		}
		else
		{
			intersectPoint = Vector2.zero;
		}
		return ret;
	}
	public static bool intersectRayRect(Vector3 orig, Vector3 dir, Rect3 rect, out float t)
	{
		// 当法线朝向屏幕外时,右边的方向
		Vector3 right = 0.5f * rect.mWidth * cross(rect.mNormal, rect.mUp);
		Vector3 top = 0.5f * rect.mHeight * rect.mUp;
		Vector3 rightTop = right + top + rect.mCenter;
		Vector3 leftTop = -right + top + rect.mCenter;
		Vector3 rightBottom = right - top + rect.mCenter;
		Vector3 leftBottom = -right - top + rect.mCenter;
		if (intersectRayTriangle(orig, dir, new(rightTop, rightBottom, leftBottom), out t, out _, out _))
		{
			return true;
		}
		if (intersectRayTriangle(orig, dir, new(rightBottom, leftBottom, leftTop), out t, out _, out _))
		{
			return true;
		}
		return false;
	}
	// Determine whether a ray intersect with a triangle
	// Parameters
	// orig: origin of the ray
	// dir: direction of the ray
	// v0, v1, v2: vertices of triangle
	// t(out): weight of the intersection for the ray
	// u(out), v(out): barycentric coordinate of intersection
	public static bool intersectRayTriangle(Vector3 orig, Vector3 dir, Triangle3 triangle, out float t, out float u, out float v)
	{
		t = 0.0f;
		u = 0.0f;
		v = 0.0f;
		Vector3 E1 = triangle.mPoint1 - triangle.mPoint0;
		Vector3 E2 = triangle.mPoint2 - triangle.mPoint0;
		Vector3 P = cross(dir, E2);
		float determinant = dot(E1, P);
		// keep det > 0, modify T accordingly
		Vector3 T;
		if (determinant > 0)
		{
			T = orig - triangle.mPoint0;
		}
		else
		{
			T = triangle.mPoint0 - orig;
			determinant = -determinant;
		}
		// If determinant is near zero, ray lies in plane of triangle
		if (determinant < 0.0001f)
		{
			return false;
		}
		// Calculate u and make sure u <= 1
		u = dot(T, P);
		if (u < 0.0f || u > determinant)
		{
			return false;
		}
		Vector3 Q = cross(T, E1);
		// Calculate v and make sure u + v <= 1
		v = dot(dir, Q);
		if(v < 0.0f || u + v > determinant)
		{
			return false;
		}
		// Calculate t, scale parameters, ray intersects triangle
		t = dot(E2, Q);
		float fInvDet = divide(1.0f, determinant);
		t *= fInvDet;
		u *= fInvDet;
		v *= fInvDet;
		return true;
	}
	// 二维平面上一个点是否在一个多边形内,多边形的顺时针点列表,并且只能是凸多边形
	public static bool isPointInPolygon(IList<Vector2> pointList, Vector2 point)
	{
		int count = pointList.Count;
		for (int i = 0; i < count; ++i)
		{
			Vector2 v1 = pointList[i];
			Vector2 v2 = pointList[(i + 1) % count];
			if (crossProduct(v1, v2, point) > 0.0f)
			{
				return false;
			}
		}
		return true;
	}
	// 判断pos是否在扇形区域内,会忽略pos和center的y轴
	public static bool inFanShape(Vector3 center, float radius, float radian, Vector3 pos)
	{
		Vector3 relative = pos - center;
		if (lengthGreater(relative, radius))
		{
			return false;
		}
		return getAngleBetweenVector(Vector3.forward, relative) < radian * 0.5f;
	}
	// circle0是否与circle1相交
	public static bool circleOverlap(Circle3 circle0, Circle3 circle1, bool ignoreY)
	{
		if(ignoreY)
		{
			circle0.mCenter.y = 0.0f;
			circle1.mCenter.y = 0.0f;
		}
		return lengthLess(circle0.mCenter - circle1.mCenter, circle0.mRadius + circle1.mRadius);
	}
	// circle0是否包含circle1
	public static bool circleContains(Circle3 circle0, Circle3 circle1, bool ignoreY)
	{
		// 如果第一个圆的半径小于第二个圆,则第一个圆不可能包含第二个圆
		if(circle0.mRadius < circle1.mRadius)
		{
			return false;
		}
		if (ignoreY)
		{
			circle0.mCenter.y = 0.0f;
			circle1.mCenter.y = 0.0f;
		}
		return lengthLess(circle0.mCenter - circle1.mCenter, circle0.mRadius - circle1.mRadius);
	}
	// 判断圆形和矩形是否相交,rotation为角度制
	public static bool circleIntersectRectangle(Circle3 circle, Vector3 rectanglePosition, Vector3 size, Vector3 rectangleRotation, bool ignoreY = true)
	{
		Vector3 circleCenter = circle.mCenter;
		if (ignoreY)
		{
			circleCenter.y = 0.0f;
			rectanglePosition.y = 0.0f;
			size.y = 0.0f;
		}
		// 将圆形转换到以矩形中心为原点的坐标系
		circleCenter -= rectanglePosition;
		circleCenter = rotateVector3(circleCenter, toRadian(-rectangleRotation.y));
		// 然后把圆心映射到第一象限,因为在转换以后的坐标系中,4个象限都是对称的,所以只需要判断一个象限即可
		circleCenter.x = abs(circleCenter.x);
		circleCenter.y = abs(circleCenter.y);
		circleCenter.z = abs(circleCenter.z);
		// 矩形在第一象限上的顶点
		Vector3 rightTopPoint = size * 0.5f;
		// 相减后获得右上角顶点到圆心的向量
		Vector3 centerToRightTop = circleCenter - rightTopPoint;
		// 将小于0的分量设置为0,保证如果圆心到矩形边的垂点在矩形范围内时该向量与矩形的某条边垂直
		clampMin(ref centerToRightTop.x);
		clampMin(ref centerToRightTop.y);
		clampMin(ref centerToRightTop.z);
		return lengthLess(centerToRightTop, circle.mRadius);
	}
	// 判断圆是否与线段相交,仅限2D平面,且Z轴为0
	public static bool circleIntersectLine(Circle3 circle, Line3 line)
	{
		// 圆心在线段所在直线的投影
		Vector3 projectPoint = getProjectPoint(circle.mCenter, line);
		// 计算圆心到线段的距离
		float distance = getLength(projectPoint - circle.mCenter);
		if(distance > circle.mRadius)
		{
			return false;
		}
		// 如果投影在线段两个端点之间,则相交
		if(inRange(projectPoint, line.mStart, line.mEnd, false))
		{
			return true;
		}
		// 计算相交部分的长度的一半,如果线段任一端点到垂线的距离小于该长度,则线段与圆相交
		float halfSquaredLength = circle.mRadius * circle.mRadius - distance * distance;
		if (getSquaredLength(line.mStart - projectPoint) <= halfSquaredLength ||
			getSquaredLength(line.mEnd - projectPoint) <= halfSquaredLength)
		{
			return true;
		}
		return false;
	}
	// 圆是否与凸多边形相交,仅限2D平面,且Z轴为0
	public static bool circleIntersectPolygon(Circle3 circle, List<Vector3> polygon)
	{
		// 如果所有的顶点都在圆外部,且没有边与圆相交,且圆心不在多边形内部,则不相交
		int polygonPointCount = polygon.Count;
		for(int i = 0; i < polygonPointCount; ++i)
		{
			// 有顶点在圆内,则相交
			if (lengthLessEqual(polygon[i] - circle.mCenter, circle.mRadius))
			{
				return true;
			}
			// 有边与圆相交,则相交
			if (circleIntersectLine(circle, new(polygon[0], polygon[1])))
			{
				return true;
			}
		}
		// 圆心在多边形内,则相交
		if(isPointInPolygon(circle.mCenter, polygon))
		{
			return true;
		}
		return false;
	}
	// 一个点是否在一个凸多边形内,仅限2D平面,且Z轴为0
	public static bool isPointInPolygon(Vector3 point, List<Vector3> polygon)
	{
		int pointSide = -999;
		// 如果点都在所有边的同一侧,则点在多边形内
		int polygonPointCount = polygon.Count;
		for(int i = 0; i < polygonPointCount; ++i)
		{
			int nextPointIndex = i < polygonPointCount - 1 ? i + 1 : 0;
			int side = getAngleSignVector2ToVector2(point - polygon[i], polygon[nextPointIndex] - polygon[i]);
			// 如果位于边所在直线上,则忽略
			if (side == 0)
			{
				continue;
			}
			if (pointSide < -1)
			{
				pointSide = side;
				continue;
			}
			// 发现一个不同侧的则直接返回不在多边形内
			if(pointSide >= -1 && pointSide != side)
			{
				return false;
			}
		}
		return true;
	}
	// 将一个向量调整到距离最近的坐标轴
	public static void adjustToNearAxis(ref Vector3 dir, bool ignoreY = false)
	{
		if (ignoreY)
		{
			dir.y = 0.0f;
		}
		float maxValue = getMax(getMax(abs(dir.x), abs(dir.y)), abs(dir.z));
		if (isFloatEqual(maxValue, abs(dir.x)))
		{
			dir = new(sign(dir.x), 0.0f, 0.0f);
		}
		else if (isFloatEqual(maxValue, abs(dir.y)))
		{
			dir = new(0.0f, sign(dir.y), 0.0f);
		}
		else if (isFloatEqual(maxValue, abs(dir.z)))
		{
			dir = new(0.0f, 0.0f, sign(dir.z));
		}
	}
	// 获取指定方向的旋转值,角度制
	public static Vector3 getLookAtRotation(Vector3 forward, bool ignoreY = false)
	{
		if (ignoreY)
		{
			forward.y = 0.0f;
		}
		return Quaternion.LookRotation(normalize(forward)).eulerAngles;
	}
	// 获取指定方向的四元数,角度制
	public static Quaternion getLookAtQuaternion(Vector3 forward, bool ignoreY = false)
	{
		if (ignoreY)
		{
			forward.y = 0.0f;
		}
		return Quaternion.LookRotation(normalize(forward));
	}
	// 获得指定朝向的四元数
	public static Quaternion getLookRotation(Vector3 forward, bool ignoreY = false)
	{
		if (ignoreY)
		{
			forward.y = 0.0f;
		}
		return Quaternion.LookRotation(forward);
	}
	// 根据航向角和俯仰角计算向量,航向角和俯仰角是角度制的
	public static Vector3 getDirectionFromDegreeYawPitch(float yaw, float pitch)
	{
		return getDirectionFromRadianYawPitch(toRadian(yaw), toRadian(pitch));
	}
	// 根据航向角和俯仰角计算向量,航向角和俯仰角是弧度制的
	public static Vector3 getDirectionFromRadianYawPitch(float yaw, float pitch)
	{
		// 如果pitch为90°或者-90°,则直接返回向量,此时无论航向角为多少,向量都是竖直向下或者竖直向上
		if (isFloatZero(pitch - HALF_PI_RADIAN))
		{
			return Vector3.down;
		}
		if (isFloatZero(pitch + HALF_PI_RADIAN))
		{
			return Vector3.up;
		}
		// 在unity的坐标系中航向角需要取反
		yaw = -yaw;
		return normalize(new Vector3(-sin(yaw), -tan(pitch), cos(yaw)));
	}
	// 计算向量的航向角,弧度制
	public static float getVectorYaw(Vector3 vec)
	{
		normalize(ref vec);
		float fYaw;
		// 计算航向角,航向角是向量与在X-Z平面上的投影与Z轴正方向的夹角,从上往下看是顺时针为正,逆时针为负
		Vector3 projectionXZ = new(vec.x, 0.0f, vec.z);
		float len = getLength(projectionXZ);
		// 如果投影的长度为0,则表示俯仰角为90°或者-90°,航向角为0
		if (isFloatZero(len))
		{
			fYaw = 0.0f;
		}
		else
		{
			normalize(ref projectionXZ);
			fYaw = acos(dot(projectionXZ, Vector3.forward));
			// 判断航向角的正负,如果x为正,则航向角为负,如果x为,则航向角为正
			if (vec.x > 0.0f)
			{
				fYaw = -fYaw;
			}
		}
		// 在unity的坐标系中航向角需要取反
		fYaw = -fYaw;
		return fYaw;
	}
	// 计算向量的俯仰角,朝上时俯仰角小于0,朝下时俯仰角大于0,弧度制
	public static float getVectorPitch(Vector3 vec)
	{
		return -asin(normalize(vec).y);
	}
	// 设置一个向量的俯仰角,pitch是弧度制的
	public static Vector3 setVectorPitch(Vector3 vec, float pitch)
	{
		float length = getLength(vec);
		Vector3 normal = generateNormal(vec, replaceY(vec, vec.y - 1.0f));
		return setLength(rotateVector3(resetY(vec), Quaternion.AngleAxis(toDegree(pitch), normal)), length);
	}
	// 设置一个向量的俯仰角,pitch是弧度制的
	public static void setVectorPitch(ref Vector3 vec, float pitch)
	{
		float length = getLength(vec);
		Vector3 normal = generateNormal(vec, replaceY(vec, vec.y - 1.0f));
		vec = setLength(rotateVector3(resetY(vec), Quaternion.AngleAxis(toDegree(pitch), normal)), length);
	}
	// 顺时针旋转为正,逆时针为负
	public static float getAngleVector2ToVector2(Vector2 from, Vector2 to, ANGLE radian = ANGLE.RADIAN)
	{
		if(isVectorEqual(from, to))
		{
			return 0.0f;
		}
		Vector3 from3 = normalize(new Vector3(from.x, 0.0f, from.y));
		Vector3 to3 = normalize(new Vector3(to.x, 0.0f, to.y));
		float angle = getAngleBetweenVector(from3, to3);
		Vector3 crossVec = cross(from3, to3);
		if (crossVec.y < 0.0f)
		{
			angle = -angle;
		}
		if (radian == ANGLE.DEGREE)
		{
			angle = toDegree(angle);
		}
		return angle;
	}
	// 在忽略Y轴的情况下,判断从from到to的角度的符号,同向或反向时为0
	public static int getAngleSignVector3ToVector3IgnoreY(Vector3 from, Vector3 to)
	{
		return getAngleSignVector2ToVector2(new(from.x, from.z), new(to.x, to.z));
	}
	// 在忽略X轴的情况下,判断从from到to的角度的符号,同向或反向时为0
	public static int getAngleSignVector3ToVector3IgnoreX(Vector3 from, Vector3 to)
	{
		return getAngleSignVector2ToVector2(new(from.z, from.y), new(to.z, to.y));
	}
	// 在忽略Z轴的情况下,判断从from到to的角度的符号,同向或反向时为0
	public static int getAngleSignVector3ToVector3IgnoreZ(Vector3 from, Vector3 to)
	{
		return getAngleSignVector2ToVector2(from, to);
	}
	// 判断两个向量从from到to的角度的符号,同向或反向时为0
	public static int getAngleSignVector2ToVector2(Vector2 from, Vector2 to)
	{
		Vector3 from3 = normalize(new Vector3(from.x, 0.0f, from.y));
		Vector3 to3 = normalize(new Vector3(to.x, 0.0f, to.y));
		// 两个向量同向或者反向时角度为0,否则角度不为0
		int angle = isVectorEqual(from3, to3) || isVectorZero(from3 + to3) ? 0 : 1;
		if (angle != 0)
		{
			Vector3 crossVec = cross(from3, to3);
			if (crossVec.y < 0.0f)
			{
				angle = -angle;
			}
		}
		return sign(angle);
	}
	// 计算从from到to的角度，根据法线normal决定角度的正负
	public static float getAngleVectorToVector(Vector3 from, Vector3 to, Vector3 normal, ANGLE radian = ANGLE.RADIAN)
	{
		if(isVectorEqual(from, to))
		{
			return 0.0f;
		}
		float angle = getAngleBetweenVector(from, to);
		Vector3 crossVec = cross(from, to);
		if (!isVectorEqual(normalize(crossVec), normalize(normal)))
		{
			angle = -angle;
		}
		if (radian == ANGLE.DEGREE)
		{
			angle = toDegree(angle);
		}
		return angle;
	}
	// baseY为true表示将点当成X-Z平面上的点,忽略Y值,false表示将点当成X-Y平面的点
	public static float getAngleVectorToVector(Vector3 from, Vector3 to, bool baseY, ANGLE radian = ANGLE.RADIAN)
	{
		if (baseY)
		{
			from.y = 0.0f;
			to.y = 0.0f;
		}
		if(isVectorEqual(from, to))
		{
			return 0.0f;
		}
		float angle = getAngleBetweenVector(from, to);
		Vector3 crossVec = cross(from, to);
		if (baseY)
		{
			if (crossVec.y < 0.0f)
			{
				angle = -angle;
			}
		}
		else
		{
			if (crossVec.z > 0.0f)
			{
				angle = -angle;
			}
		}
		if (radian == ANGLE.DEGREE)
		{
			angle = toDegree(angle);
		}
		return angle;
	}
	// 计算向量的航向角和俯仰角,计算结果为角度制
	public static Vector3 getDegreeEulerFromDirection(Vector3 dir)
	{
		getDegreeYawPitchFromDirection(dir, out float yaw, out float pitch);
		return new(pitch, yaw, 0.0f);
	}
	// 计算向量的航向角和俯仰角,计算结果为角度制
	public static void getDegreeYawPitchFromDirection(Vector3 dir, out float fYaw, out float fPitch)
	{
		getRadianYawPitchFromDirection(dir, out fYaw, out fPitch);
		fYaw = toDegree(fYaw);
		fPitch = toDegree(fPitch);
	}
	// 计算向量的航向角和俯仰角,fYaw是-PI到PI之间
	public static void getRadianYawPitchFromDirection(Vector3 dir, out float fYaw, out float fPitch)
	{
		normalize(ref dir);
		// 首先计算俯仰角,俯仰角是向量与X-Z平面的夹角,在上面为负,在下面为正
		fPitch = getVectorPitch(dir);
		fYaw = getVectorYaw(dir);
	}
	// 给定一段圆弧,以及圆弧圆心角的百分比,计算对应的圆弧上的一个点以及该点的切线方向
	public static void getPosOnArc(Vector3 circleCenter, Vector3 startArcPos, Vector3 endArcPos, float anglePercent, out Vector3 pos, out Vector3 tangencyDir)
	{
		float radius = getLength(startArcPos - circleCenter);
		Vector3 start = startArcPos - circleCenter;
		Vector3 end = endArcPos - circleCenter;
		saturate(ref anglePercent);
		// 首先判断从起始半径线段到终止半径线段的角度的正负
		float angleBetween = getAngleVector2ToVector2(new(start.x, start.z), new(end.x, end.z));
		if (isFloatZero(angleBetween))
		{
			pos = normalize(start) * radius;
			tangencyDir = normalize(rotateVector3(-pos, HALF_PI_RADIAN));
		}
		// 根据夹角的正负,判断应该顺时针还是逆时针旋转起始半径线段
		else
		{
			pos = normalize(rotateVector3(start, anglePercent * angleBetween)) * radius;
			// 计算切线,如果顺时针计算出的切线与从起始点到终止点所成的角度大于90度,则使切线反向
			tangencyDir = normalize(rotateVector3(-pos, HALF_PI_RADIAN));
			Vector3 posToEnd = end - pos;
			if (abs(getAngleVector2ToVector2(new(tangencyDir.x, tangencyDir.z), new(posToEnd.x, posToEnd.z))) > HALF_PI_RADIAN)
			{
				tangencyDir = -tangencyDir;
			}
		}
		pos += circleCenter;
	}
	// 根据入射角和法线得到反射角
	public static Vector3 getReflection(Vector3 inRay, Vector3 normal)
	{
		normalize(ref inRay);
		return inRay - 2 * getProjection(inRay, normalize(normal));
	}
	// 将vec的长度限定到maxLength,如果长度未超过,则不作修改
	public static Vector3 clampLength(Vector3 vec, float maxLength)
	{
		if(lengthGreater(vec, maxLength))
		{
			return normalize(vec) * maxLength;
		}
		return vec;
	}
	// 将vec的长度限定到maxLength,如果长度未超过,则不作修改
	public static void clampLength(ref Vector3 vec, float maxLength)
	{
		if (lengthGreater(vec, maxLength))
		{
			vec = normalize(vec) * maxLength;
		}
	}
	// 将向量的X设置为0
	public static Vector3 resetX(Vector3 v) { return new(0.0f, v.y, v.z); }
	// 将向量的Y设置为0
	public static Vector3 resetY(Vector3 v) { return new(v.x, 0.0f, v.z); }
	// 将向量的Z设置为0
	public static Vector3 resetZ(Vector3 v) { return new(v.x, v.y, 0.0f); }
	// 将向量的X替换为指定值
	public static Vector3 replaceX(Vector3 v, float x) { return new(x, v.y, v.z); }
	// 将向量的Y替换为指定值
	public static Vector3 replaceY(Vector3 v, float y) { return new(v.x, y, v.z); }
	// 将向量的Z替换为指定值
	public static Vector3 replaceZ(Vector3 v, float z) { return new(v.x, v.y, z); }
	// vec0的3个分量是否都小于vec1的3个分量
	public static bool isVector3Less(Vector3 vec0, Vector3 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y && vec0.z < vec1.z; }
	// vec0的3个分量是否都大于vec1的3个分量
	public static bool isVector3Greater(Vector3 vec0, Vector3 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y && vec0.z > vec1.z; }
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
	public static bool isQuatertionEqual(Quaternion q0, Quaternion q1, float precision = 0.0001f) 
	{ 
		return isFloatZero(q0.x - q1.x, precision) && 
			   isFloatZero(q0.y - q1.y, precision) && 
			   isFloatZero(q0.z - q1.z, precision) && 
			   isFloatZero(q0.w - q1.w, precision); 
	}
	public static bool isVectorZero(Vector2 vec, float precision = 0.0001f) 
	{
		return isFloatZero(vec.x, precision) && 
			   isFloatZero(vec.y, precision); 
	}
	public static bool isVectorZero(Vector3 vec, float precision = 0.0001f) 
	{ 
		return isFloatZero(vec.x, precision) && 
			   isFloatZero(vec.y, precision) && 
			   isFloatZero(vec.z, precision); 
	}
	public static bool isQuaternionEqual(Quaternion value0, Quaternion value1, float precision = 0.0001f)
	{
		return isFloatEqual(value0.x, value1.x, precision) &&
			   isFloatEqual(value0.y, value1.y, precision) &&
			   isFloatEqual(value0.z, value1.z, precision) &&
			   isFloatEqual(value0.w, value1.w, precision);
	}
	public static float getLength(Vector4 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w); }
	public static float getLength(Vector3 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	public static float getLengthIgnoreY(Vector3 vec) { return sqrt(vec.x * vec.x + vec.z * vec.z); }
	public static float getLength(Vector2 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	public static float getSquaredLength(Vector4 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w; }
	public static float getSquaredLength(Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static float getSquaredLengthIgnoreY(Vector3 vec) { return vec.x * vec.x + vec.z * vec.z; }
	public static float getSquaredLength(Vector2 vec) { return vec.x * vec.x + vec.y * vec.y; }
	public static bool lengthLess(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y < vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLess(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y < length * length; }
	public static bool lengthLess(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z < vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLess(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthLessIgnoreY(Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z < length * length; }
	public static bool lengthLess(Vector4 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w < length * length; }
	public static bool lengthLessEqual(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y <= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLessEqual(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y <= length * length; }
	public static bool lengthLessEqual(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z <= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLessEqual(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z <= length * length; }
	public static bool lengthLessEqualIgnoreY(Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z <= length * length; }
	public static bool lengthGreater(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static bool lengthGreater(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y > vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreater(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	public static bool lengthGreaterIgnoreY(Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z > length * length; }
	public static bool lengthGreater(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z > vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqual(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y >= length * length; }
	public static bool lengthGreaterEqual(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y >= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreaterEqual(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z >= length * length; }
	public static bool lengthGreaterEqual(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z >= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqualIgnoreY(Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z >= length * length; }
	public static Vector3 setLength(Vector3 vec, float length) 
	{
		float scale = divide(1.0f, getLength(vec)) * length;
		return new(vec.x * scale, vec.y * scale, vec.z * scale);
	}
	public static void setLength(ref Vector3 vec, float length) 
	{
		float scale = divide(1.0f, getLength(vec)) * length;
		vec.x *= scale;
		vec.y *= scale;
		vec.z *= scale;
	}
	// 将矩阵的缩放设置为1,并且不改变位移和旋转
	public static Matrix4x4 identityMatrix4(Matrix4x4 rot)
	{
		Vector3 vec0 = normalize(new Vector3(rot.m00, rot.m01, rot.m02));
		Vector3 vec1 = normalize(new Vector3(rot.m10, rot.m11, rot.m12));
		Vector3 vec2 = normalize(new Vector3(rot.m20, rot.m21, rot.m22));
		Matrix4x4 temp = new();
		temp.m00 = vec0.x;
		temp.m01 = vec0.y;
		temp.m02 = vec0.z;
		temp.m10 = vec1.x;
		temp.m11 = vec1.y;
		temp.m12 = vec1.z;
		temp.m20 = vec2.x;
		temp.m21 = vec2.y;
		temp.m22 = vec2.z;
		return temp;
	}
	public static Vector3 matrixToEulerAngle(Matrix4x4 rot)
	{
		Matrix4x4 tempMat4 = identityMatrix4(rot);
		// 计算滚动角
		// 首先求出矩阵中X-Y平面与世界坐标系水平面的交线
		// 交线为X = -rot[2][2] / rot[2][0] * Z,然后随意构造出一个向量
		Vector3 intersectLineVector;
		float angleRoll;
		if (!isFloatZero(tempMat4.m20) || !isFloatZero(tempMat4.m22))
		{
			// 矩阵中Z轴的x分量为0,则交线在世界坐标系的X轴上,取X轴正方向上的一个点
			if (isFloatZero(tempMat4.m20) && !isFloatZero(tempMat4.m22))
			{
				intersectLineVector = new(1.0f, 0.0f, 0.0f);
			}
			// 矩阵中Z轴的z分量为0,则交线在世界坐标系的Z轴上,
			else if (!isFloatZero(tempMat4.m20) && isFloatZero(tempMat4.m22))
			{
				// Z轴朝向世界坐标系的X轴正方向,即Z轴的x分量大于0,应该计算X轴与世界坐标系的Z轴负方向的夹角
				if (tempMat4.m20 > 0.0f)
				{
					intersectLineVector = new(0.0f, 0.0f, -1.0f);
				}
				// Z轴朝向世界坐标系的X轴负方向,应该计算X轴与世界坐标系的Z轴正方向的夹角
				else
				{
					intersectLineVector = new(0.0f, 0.0f, 1.0f);
				}
			}
			// 矩阵中Z轴的x和z分量都不为0,取X轴正方向上的一个点
			else
			{
				intersectLineVector = new(1.0f, 0.0f, -divide(tempMat4.m20, tempMat4.m22));
			}
			// 然后求出矩阵中X轴与交线的夹角
			angleRoll = getAngleBetweenVector(intersectLineVector, new(tempMat4.m00, tempMat4.m01, tempMat4.m02));
			// 如果X轴的y分量大于0,则滚动角为负
			if (tempMat4.m01 > 0.0f)
			{
				angleRoll = -angleRoll;
			}
		}
		// 如果Z轴的x和z分量都为0,则俯仰角为90°或者-90°,此处不计算
		else
		{
			// 此时X-Y平面与水平面相平行,计算X轴与世界坐标系中X轴的夹角,X轴的z分量小于0时,滚动角为负
			angleRoll = getAngleBetweenVector(new Vector3(tempMat4.m00, tempMat4.m01, tempMat4.m02), new Vector3(1.0f, 0.0f, 0.0f));
			if (tempMat4.m02 < 0.0f)
			{
				angleRoll = -angleRoll;
			}
		}

		// 计算出滚动角后,将矩阵中的滚动角归0
		Matrix4x4 nonRollMat = rot;
		if (!isFloatZero(angleRoll))
		{
			nonRollMat *= getRollMatrix3(-angleRoll);
		}

		// 然后计算俯仰角
		// Z轴与Z轴在水平面上的投影的夹角
		Vector3 zAxisInMatrix = new(nonRollMat.m20, nonRollMat.m21, nonRollMat.m22);
		float anglePitch;
		if (!isFloatZero(zAxisInMatrix.x) || !isFloatZero(zAxisInMatrix.z))
		{
			anglePitch = getAngleBetweenVector(zAxisInMatrix, new(zAxisInMatrix.x, 0.0f, zAxisInMatrix.z));
			// Z轴的y分量小于0,则俯仰角为负
			if (nonRollMat.m21 < 0.0f)
			{
				anglePitch = -anglePitch;
				// 如果Y轴的y分量小于0,则说明俯仰角的绝对值已经大于90°了
				if (nonRollMat.m11 < 0.0f)
				{
					anglePitch = -PI_RADIAN - anglePitch;
				}
			}
			else
			{
				if (nonRollMat.m11 < 0.0f)
				{
					anglePitch = PI_RADIAN - anglePitch;
				}
			}
		}
		// 如果在水平面上的投影为0,俯仰角为90°或-90°
		else
		{
			if (nonRollMat.m21 > 0.0f)
			{
				anglePitch = HALF_PI_RADIAN;
			}
			else
			{
				anglePitch = -HALF_PI_RADIAN;
			}
		}

		// 然后计算航向角
		// X轴与世界坐标系中的X轴的夹角
		float angleYaw = getAngleBetweenVector(new Vector3(nonRollMat.m00, nonRollMat.m01, nonRollMat.m02), Vector3.right);
		// X轴的z分量小于0,则航向角为负
		if (nonRollMat.m02 < 0.0f)
		{
			angleYaw = -angleYaw;
		}
		return new(angleYaw, anglePitch, angleRoll);
	}
	// 返回值是弧度值的角度
	public static float getAngleBetweenVector(Vector3 vec1, Vector3 vec2)
	{
		return acos(dot(normalize(vec1), normalize(vec2)));
	}
	// 返回值是弧度值的角度
	public static float getAngleBetweenVector(Vector2 vec1, Vector2 vec2)
	{
		return acos(dot(normalize((Vector3)vec1), normalize((Vector3)vec2)));
	}
	// 计算点到线的距离
	public static float getDistanceToLine(Vector3 point, Line3 line)
	{
		return getLength(point - getProjectPoint(point, line));
	}
	// 计算点到线的距离
	public static float getDistanceToLine(Vector2 point, Line2 line)
	{
		return getLength(point - getProjectPoint(point, line));
	}
	// 计算点在线上的投影
	public static Vector3 getProjectPoint(Vector3 point, Line3 line)
	{
		// 计算出点到线一段的向量在线上的投影
		Vector3 projectOnLine = getProjection(point - line.mStart, line.mEnd - line.mStart);
		// 点到线垂线的交点
		return line.mStart + projectOnLine;
	}
	// 计算点在线上的投影
	public static Vector2 getProjectPoint(Vector2 point, Line2 line)
	{
		// 计算出点到线一段的向量在线上的投影
		Vector2 projectOnLine = getProjection(point - line.mStart, line.mEnd - line.mStart);
		// 点到线垂线的交点
		return line.mStart + projectOnLine;
	}
	// 点在线上的投影是否在线段范围内
	public static bool isPointProjectOnLine(Vector3 point, Line3 line)
	{
		Vector3 point0 = line.mStart;
		Vector3 point1 = line.mEnd;
		if (lengthGreater(point - line.mStart, point - line.mEnd))
		{
			point0 = line.mEnd;
			point1 = line.mStart;
		}
		return getAngleBetweenVector(point - point0, point1 - point0) <= HALF_PI_RADIAN;
	}
	// 点在线上的投影是否在线段范围内
	public static bool isPointProjectOnLine(Vector2 point, Line2 line)
	{
		Vector2 point0 = line.mStart;
		Vector2 point1 = line.mEnd;
		if (lengthGreater(point - line.mStart, point - line.mEnd))
		{
			point0 = line.mEnd;
			point1 = line.mStart;
		}
		return getAngleBetweenVector(point - point0, point1 - point0) <= HALF_PI_RADIAN;
	}
	// 计算向量v1在向量v2上的投影
	public static Vector3 getProjection(Vector3 v1, Vector3 v2)
	{
		return divide(normalize(v2) * dot(v1, v2), getLength(v2));
	}
	public static Vector2 getProjection(Vector2 v1, Vector2 v2)
	{
		float inverseLen = divide(1.0f, getLength(v2));
		return (v1.x * v2.x + v1.y * v2.y) * inverseLen * inverseLen * v2;
	}
	public static Vector3 normalize(Vector3 vec3)
	{
		float inverseLen = divide(1.0f, getLength(vec3));
		return new(vec3.x * inverseLen, vec3.y * inverseLen, vec3.z * inverseLen);
	}
	public static void normalize(ref Vector3 vec3)
	{
		float inverseLen = divide(1.0f, getLength(vec3));
		vec3.x *= inverseLen;
		vec3.y *= inverseLen;
		vec3.z *= inverseLen;
	}
	public static Vector2 normalize(Vector2 vec2)
	{
		float inverseLen = divide(1.0f, getLength(vec2));
		return new(vec2.x * inverseLen, vec2.y * inverseLen);
	}
	public static void normalize(ref Vector2 vec2)
	{
		float inverseLen = divide(1.0f, getLength(vec2));
		vec2.x *= inverseLen;
		vec2.y *= inverseLen;
	}
	public static Matrix4x4 eulerAngleToMatrix3(Vector3 angle)
	{
		// 分别计算三个分量的旋转矩阵,然后相乘得出最后的结果
		return getYawMatrix3(angle.x) * getPitchMatrix3(angle.y) * getRollMatrix3(angle.z);
	}
	public static Matrix4x4 getYawMatrix3(float angle)
	{
		float cosY = cos(angle);
		float sinY = sin(angle);
		Matrix4x4 rot = new();
		rot.m00 = cosY;
		rot.m01 = 0.0f;
		rot.m02 = sinY;
		rot.m10 = 0.0f;
		rot.m11 = 1.0f;
		rot.m12 = 0.0f;
		rot.m20 = -sinY;
		rot.m21 = 0.0f;
		rot.m22 = cosY;
		return rot;
	}
	public static Matrix4x4 getPitchMatrix3(float angle)
	{
		float cosZ = cos(angle);
		float sinZ = sin(angle);
		Matrix4x4 rot = new();
		rot.m00 = 1.0f;
		rot.m01 = 0.0f;
		rot.m02 = 0.0f;
		rot.m10 = 0.0f;
		rot.m11 = cosZ;
		rot.m12 = -sinZ;
		rot.m20 = 0.0f;
		rot.m21 = sinZ;
		rot.m22 = cosZ;
		return rot;
	}
	public static Matrix4x4 getRollMatrix3(float angle)
	{
		float cosX = cos(angle);
		float sinX = sin(angle);
		Matrix4x4 rot = new();
		rot.m00 = cosX;
		rot.m01 = -sinX;
		rot.m02 = 0.0f;
		rot.m10 = sinX;
		rot.m11 = cosX;
		rot.m12 = 0.0f;
		rot.m20 = 0.0f;
		rot.m21 = 0.0f;
		rot.m22 = 1.0f;
		return rot;
	}
	// 检测curValue变化delta以后是否到达了指定值target,会同时修改curValue的值,返回true表示已经到达
	public static bool checkReachTarget(ref float curValue, float delta, float target)
	{
		// 当前已经到达目标,则不需要计算
		if (isFloatEqual(curValue, target))
		{
			return true;
		}
		float newValue = curValue + delta;
		// 加上delta以后等于了target,或者超过了target,则是到达目标
		if (isFloatEqual(target, newValue) || (int)sign(target - newValue) != (int)sign(target - curValue))
		{
			curValue = target;
			return true;
		}
		curValue = newValue;
		return false;
	}
	public static float toRadian(float degree) { return degree * Mathf.Deg2Rad; }
	public static void toRadian(ref float degree) { degree *= Mathf.Deg2Rad; }
	public static Vector3 toRadian(Vector3 degree) { return degree * Mathf.Deg2Rad; }
	public static void toRadian(ref Vector3 degree) { degree *= Mathf.Deg2Rad; }
	public static float toDegree(float radian) { return radian * Mathf.Rad2Deg; }
	public static void toDegree(ref float radian) { radian *= Mathf.Rad2Deg; }
	public static Vector3 toDegree(Vector3 radian) { return radian * Mathf.Rad2Deg; }
	public static void toDegree(ref Vector3 radian) { radian *= Mathf.Rad2Deg; }
	public static float getQuaternionYaw(Quaternion q) { return q.eulerAngles.y; }
	public static float getQuaternionPitch(Quaternion q) { return q.eulerAngles.z; }
	public static float getQuaternionRoll(Quaternion q) { return q.eulerAngles.x; }
	// 根据公式y = a * x * x + b * x + c
	public static float generateFactorBFromFactorA(float factorA, Vector3 point) { return divide(point.y - factorA * point.x * point.x, point.x); }
	public static float generateFactorA(float factorB, Vector3 point) { return divide(point.y - factorB * point.x, point.x * point.x); }
	public static float generateTopHeight(float factorA, float factorB) { return -divide(factorB * factorB, 4.0f * factorA); }
	public static float generateFactorBFromHeight(float topHeight, Vector3 point, bool leftOrRight = false)
	{
		if(isFloatZero(topHeight))
		{
			return 0.0f;
		}
		// 设另外一个点为px,py,最高点的y坐标为h
		// 最高点处的x坐标表示为x = -b / (2 * a)
		// py = a * px * px + b * px
		// h = -b * b / (4 * a)
		// 联立以上两个方程可以得知
		// 0 = -px * px / (4 * h) * b * b  + px * b - py
		// 将此方程看作是b的一元二次方程，可解得b的值
		float a0 = -divide(point.x * point.x, 4.0f * topHeight);
		if(isFloatZero(a0))
		{
			return 0.0f;
		}
		float b0 = point.x;
		float c0 = -point.y;
		float delta = b0 * b0 - 4.0f * a0 * c0;
		if (delta < 0.0f)
		{
			return 0.0f;
		}
		int sign = leftOrRight ? 1 : -1;
		return divide(-b0 + sqrt(delta) * sign, 2.0f * a0);
	}
	// 根据抛物线起点,顶点相对于起点的高度和抛物线上的一个点(都是世界坐标系下的点),计算出抛物线的公式
	// 对世界坐标系下的点需要经过一次坐标系转换,坐标系以origin为原点,以从origin到otherPoint的方向为X轴
	// X坐标值就是忽略高度后目标点到起点的距离,Y坐标是高度差,Z坐标为0
	// 计算出的抛物线是过原点的,开口向下的,顶点在X轴正方向的,且对称轴在point和原点之间
	public static void generateParabola(float topHeight, Vector3 origin, Vector3 otherPoint, out float factorA, out float factorB)
	{
		Vector3 newPos = new(getLength(resetY(otherPoint - origin)), otherPoint.y - origin.y);
		factorB = generateFactorBFromHeight(topHeight, newPos, false);
		factorA = generateFactorA(factorB, newPos);
	}
	public static Vector3 getMinVector3(Vector3 a, Vector3 b) { return new(getMin(a.x, b.x), getMin(a.y, b.y), getMin(a.z, b.z)); }
	public static Vector3 getMaxVector3(Vector3 a, Vector3 b) { return new(getMax(a.x, b.x), getMax(a.y, b.y), getMax(a.z, b.z)); }
	public static void getMinMaxVector3(Vector3 a, ref Vector3 min, ref Vector3 max)
	{
		min.x = getMin(a.x, min.x);
		min.y = getMin(a.y, min.y);
		min.z = getMin(a.z, min.z);
		max.x = getMax(a.x, max.x);
		max.y = getMax(a.y, max.y);
		max.z = getMax(a.z, max.z);
	}
	public static int getMin(int a, int b) { return a < b ? a : b; }
	public static float getMin(float a, float b) { return a < b ? a : b; }
	public static uint getMin(uint a, uint b) { return a < b ? a : b; }
	public static long getMin(long a, long b) { return a < b ? a : b; }
	public static int getMax(int a, int b) { return a > b ? a : b; }
	public static float getMax(float a, float b) { return a > b ? a : b; }
	public static long getMax(long a, long b) { return a > b ? a : b; }
	public static uint getMax(uint a, uint b) { return a > b ? a : b; }
	public static float inverseLerp(float a, float b, float value)
	{
		return divide(value - a, b - a);
	}
	public static float inverseLerp(Vector2 a, Vector2 b, Vector2 value)
	{
		return divide(getLength(value - a), getLength(b - a));
	}
	public static float inverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		return divide(getLength(value - a), getLength(b - a));
	}
	public static float lerpSimple(float start, float end, float t) { return start + (end - start) * t; }
	public static Vector3 lerpSimple(Vector3 start, Vector3 end, float t) { return start + (end - start) * t; }
	public static Color lerpSimple(Color start, Color end, float t) { return start + (end - start) * t; }
	public static float lerp(float start, float end, float t, float minRange = 0.0f)
	{
		saturate(ref t);
		float value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		if (abs(value - end) <= minRange)
		{
			value = end;
		}
		return value;
	}
	public static Vector3 lerp(Vector3 start, Vector3 end, float t, float minRange = 0.0f)
	{
		saturate(ref t);
		Vector3 value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		if (lengthLess(value - end, minRange))
		{
			value = end;
		}
		return value;
	}
	public static Vector4 lerp(Vector4 start, Vector4 end, float t, float minRange = 0.0f)
	{
		saturate(ref t);
		Vector4 value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		if (lengthLess(value - end, minRange))
		{
			value = end;
		}
		return value;
	}
	public static Quaternion lerp(Quaternion start, Quaternion end, float t)
	{
		saturate(ref t);
		return Quaternion.Lerp(start, end, t);
	}
	public static Color lerp(Color start, Color end, float t, float minRange = 0.0f)
	{
		saturate(ref t);
		Color value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		Color curDelta = value - end;
		if (getSquaredLength(new Vector4(curDelta.r, curDelta.g, curDelta.b, curDelta.a)) <= minRange * minRange)
		{
			value = end;
		}
		return value;
	}
	// 当要旋转到指定角度时,可以调整当前角度和目标角度,使中间的夹角不会超过180°
	public static void perfectRotationDeltaDegree(ref float start, ref float target)
	{
		// 先都调整到-180~180的范围
		adjustAngle180(ref start);
		adjustAngle180(ref target);
		// 如果目标方向与当前方向的差值超过180,则转换到0~360再计算
		if (abs(target - start) > PI_DEGREE)
		{
			adjustAngle360(ref start);
			adjustAngle360(ref target);
		}
	}
	// 当要旋转到指定角度时,可以调整当前角度和目标角度,使中间的夹角不会超过180°
	public static void perfectRotationDeltaRadian(ref float start, ref float target)
	{
		// 先都调整到-180~180的范围
		adjustRadian180(ref start);
		adjustRadian180(ref target);
		// 如果目标方向与当前方向的差值超过180,则转换到0~360再计算
		if (abs(target - start) > PI_RADIAN)
		{
			adjustRadian360(ref start);
			adjustRadian360(ref target);
		}
	}
	public static void perfectRotationDeltaDegree(ref Vector3 start, ref Vector3 target)
	{
		perfectRotationDeltaDegree(ref start.x, ref target.x);
		perfectRotationDeltaDegree(ref start.y, ref target.y);
		perfectRotationDeltaDegree(ref start.z, ref target.z);
	}
	public static void perfectRotationDeltaRadian(ref Vector3 start, ref Vector3 target)
	{
		perfectRotationDeltaRadian(ref start.x, ref target.x);
		perfectRotationDeltaRadian(ref start.y, ref target.y);
		perfectRotationDeltaRadian(ref start.z, ref target.z);
	}
	// 判断target是否在v0和v1之间,会自动寻找v0和v1的小于180°的夹角
	public static bool isVector2BetweenVectors(Vector2 target, Vector2 v0, Vector2 v1)
	{
		float angle0 = getAngleFromVector2(v0);
		float angle1 = getAngleFromVector2(v1);
		float angle = getAngleFromVector2(target);
		if (abs(angle1 - angle0) > PI_RADIAN)
		{
			adjustRadian360(ref angle0);
			adjustRadian360(ref angle1);
			adjustRadian360(ref angle);
		}
		return angle >= angle0 && angle <= angle1;
	}
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
	public static void clamp(ref long value, long min, long max)
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
	public static float clamp(float value, float min, float max)
	{
		if (min > max || isFloatEqual(min, max))
		{
			return min;
		}
		if (value < min)
		{
			return min;
		}
		else if (value > max)
		{
			return max;
		}
		return value;
	}
	public static int clamp(int value, int min, int max)
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
			return min;
		}
		else if (value > max)
		{
			return max;
		}
		return value;
	}
	public static long clamp(long value, long min, long max)
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
			return min;
		}
		else if (value > max)
		{
			return max;
		}
		return value;
	}
	public static void clampMin(ref byte value, byte min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref sbyte value, sbyte min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref short value, short min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref ushort value, ushort min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref int value, int min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref uint value, uint min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref long value, long min = 0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static void clampMin(ref ulong value, ulong min = 0)
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
	public static void clampMin(ref double value, double min = 0.0)
	{
		if (value < min)
		{
			value = min;
		}
	}
	public static float clampMin(float value, float min = 0.0f)
	{
		return value < min ? min : value;
	}
	public static int clampMin(int value, int min = 0)
	{
		return value < min ? min : value;
	}
	public static long clampMin(long value, int min = 0)
	{
		return value < min ? min : value;
	}
	public static void clampMax(ref byte value, byte max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref sbyte value, sbyte max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref short value, short max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref ushort value, ushort max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref int value, int max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref uint value, uint max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref long value, long max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref ulong value, ulong max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static void clampMax(ref float value, float max)
	{
		if (value > max)
		{
			value = max;
		}
	}
	public static int clampMax(int value, int max)
	{
		return value > max ? max : value;
	}
	public static long clampMax(long value, long max)
	{
		return value > max ? max : value;
	}
	public static float clampMax(float value, float max)
	{
		return value > max ? max : value;
	}
	public static bool isFloatZero(float value, float precision = 0.0001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isZero(double value, double precision = 0.00000001f)
	{
		return value >= -precision && value <= precision;
	}
	public static bool isFloatEqual(float value1, float value2, float precision = 0.0001f)
	{
		return isFloatZero(value1 - value2, precision);
	}
	public static bool isDoubleEqual(double value1, double value2, double precision = 0.00000001f)
	{
		return isZero(value1 - value2, precision);
	}
	// 返回value0/value1的值,如果value1为0,则返回defaultValue
	public static float divide(float value0, float value1, float defaultValue = 0.0f)
	{
		return !isFloatZero(value1) ? value0 / value1 : defaultValue;
	}
	public static int divideInt(int value0, int value1, int defaultValue = 0)
	{
		return value1 != 0 ? value0 / value1 : defaultValue;
	}
	public static double divide(double value0, double value1, double defaultValue = 0.0f)
	{
		return !isZero(value1) ? value0 / value1 : defaultValue;
	}
	public static void clampCycle(ref int value, int min, int max, int cycle, bool includeMax = true)
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
	public static int clampCycle(int value, int min, int max, int cycle, bool includeMax = true)
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
	public static float clampCycle(float value, float min, float max, float cycle, bool includeMax = true)
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
	public static bool inRange(float value, float range0, float range1, float precision = 0.001f)
	{
		return value >= getMin(range0, range1) - precision && value <= getMax(range0, range1) + precision;
	}
	public static bool inRangeFixed(float value, float range0, float range1, float precision = 0.001f)
	{
		return value >= range0 - precision && value <= range1 + precision;
	}
	public static bool inRange(int value, int range0, int range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	public static bool inRangeFixed(int value, int range0, int range1)
	{
		return value >= range0 && value <= range1;
	}
	public static bool inRange(Vector3 value, Vector3 point0, Vector3 point1, bool ignoreY = true, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, precision) && 
			   inRange(value.z, point0.z, point1.z, precision) &&
			  (ignoreY || inRange(value.y, point0.y, point1.y, precision));
	}
	public static bool inRange(Vector2 value, Vector2 point0, Vector2 point1, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, precision) && 
			   inRange(value.y, point0.y, point1.y, precision);
	}
	// 计算路线的总长度
	public static float generatePathLength(List<Vector3> path)
	{
		float length = 0.0f;
		int count = path.Count - 1;
		for (int i = 0; i < count; ++i)
		{
			length += getLength(path[i] - path[i + 1]);
		}
		return length;
	}
	// 根据一个路线的点列表，计算每个点到起点的曲线距离列表，返回整个路线的长度
	public static void generateDistanceList(List<Vector3> keyPosList, List<KeyPoint> keyPointList)
	{
		keyPointList.Clear();
		float distanceFromStart = 0.0f;
		int count = keyPosList.Count;
		for (int i = 0; i < count; ++i)
		{
			float distanceFromLast = 0.0f;
			if (i > 0)
			{
				distanceFromLast = getLength(keyPosList[i] - keyPosList[i - 1]);
				distanceFromStart += distanceFromLast;
			}
			keyPointList.Add(new(keyPosList[i], distanceFromStart, distanceFromLast));
		}
	}
	// 根据一个路线的点列表，计算每个点到起点的曲线距离列表，返回整个路线的长度
	public static void generateDistanceList(Span<Vector3> keyPosList, List<KeyPoint> keyPointList)
	{
		keyPointList.Clear();
		float distanceFromStart = 0.0f;
		int count = keyPosList.Length;
		for (int i = 0; i < count; ++i)
		{
			float distanceFromLast = 0.0f;
			if (i > 0)
			{
				distanceFromLast = getLength(keyPosList[i] - keyPosList[i - 1]);
				distanceFromStart += distanceFromLast;
			}
			keyPointList.Add(new(keyPosList[i], distanceFromStart, distanceFromLast));
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
	public static Vector3 divideVector3(Vector3 v1, Vector3 v2) { return new(divide(v1.x, v2.x), divide(v1.y, v2.y), divide(v1.z, v2.z)); }
	public static Vector2 divide(Vector2 v1, float scale) { return new(divide(v1.x, scale), divide(v1.y, scale)); }
	public static Vector3 divide(Vector3 v1, float scale) { return new(divide(v1.x, scale), divide(v1.y, scale), divide(v1.z, scale)); }
	public static void swap<T>(ref T value0, ref T value1)
	{
		T temp = value0;
		value0 = value1;
		value1 = temp;
	}
	public static void adjustRadian180(ref float radian) { clampCycle(ref radian, -PI_RADIAN, PI_RADIAN, TWO_PI_RADIAN); }
	public static float adjustRadian180(float radian) { return clampCycle(radian, -PI_RADIAN, PI_RADIAN, TWO_PI_RADIAN); }
	public static Vector3 adjustRadian180(Vector3 radian)
	{
		adjustRadian180(ref radian.x);
		adjustRadian180(ref radian.y);
		adjustRadian180(ref radian.z);
		return radian;
	}
	public static void adjustRadian180(ref Vector3 radian)
	{
		adjustRadian180(ref radian.x);
		adjustRadian180(ref radian.y);
		adjustRadian180(ref radian.z);
	}
	public static void adjustAngle180(ref float degree) { clampCycle(ref degree, -PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	public static float adjustAngle180(float degree) { return clampCycle(degree, -PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	public static Vector3 adjustAngle180(Vector3 degree)
	{
		adjustAngle180(ref degree.x);
		adjustAngle180(ref degree.y);
		adjustAngle180(ref degree.z);
		return degree;
	}
	public static void adjustAngle180(ref Vector3 degree)
	{
		adjustAngle180(ref degree.x);
		adjustAngle180(ref degree.y);
		adjustAngle180(ref degree.z);
	}
	public static float adjustRadian360(float radian) { return clampCycle(radian, 0.0f, TWO_PI_RADIAN, TWO_PI_RADIAN); }
	public static void adjustRadian360(ref float radian) { clampCycle(ref radian, 0.0f, TWO_PI_RADIAN, TWO_PI_RADIAN); }
	public static Vector3 adjustRadian360(Vector3 radian)
	{
		adjustRadian360(ref radian.x);
		adjustRadian360(ref radian.y);
		adjustRadian360(ref radian.z);
		return radian;
	}
	public static void adjustRadian360(ref Vector3 radian)
	{
		adjustRadian360(ref radian.x);
		adjustRadian360(ref radian.y);
		adjustRadian360(ref radian.z);
	}
	public static void adjustAngle360(ref float degree) { clampCycle(ref degree, 0.0f, TWO_PI_DEGREE, TWO_PI_DEGREE); }
	public static float adjustAngle360(float degree) { return clampCycle(degree, 0.0f, TWO_PI_DEGREE, TWO_PI_DEGREE); }
	public static Vector3 adjustAngle360(Vector3 degree)
	{
		adjustAngle360(ref degree.x);
		adjustAngle360(ref degree.y);
		adjustAngle360(ref degree.z);
		return degree;
	}
	public static void adjustAngle360(ref Vector3 degree)
	{
		adjustAngle360(ref degree.x);
		adjustAngle360(ref degree.y);
		adjustAngle360(ref degree.z);
	}
	// 求从z轴到指定向量的水平方向上的顺时针角度,角度范围是-MATH_PI 到 MATH_PI
	public static float getAngleFromQuaternion(Quaternion from, Quaternion to, ANGLE angleType = ANGLE.RADIAN)
	{
		float angle = Quaternion.Angle(from, to);
		if (angleType == ANGLE.RADIAN)
		{
			angle = toRadian(angle);
		}
		return angle;
	}
	// 求从z轴到指定向量的水平方向上的顺时针角度,角度范围是-MATH_PI 到 MATH_PI
	public static float getAngleFromVector3(Vector3 vec, ANGLE radian = ANGLE.RADIAN)
	{
		vec.y = 0.0f;
		normalize(ref vec);
		float angle = acos(vec.z);
		if (vec.x > 0.0f)
		{
			angle = -angle;
		}
		adjustRadian180(ref angle);
		// 在unity的坐标系中航向角需要取反
		angle = -angle;
		if (radian == ANGLE.DEGREE)
		{
			angle = toDegree(angle);
		}
		return angle;
	}
	public static float getAngleFromVector2(Vector2 vec)
	{
		Vector3 tempVec = normalize(new Vector3(vec.x, 0.0f, vec.y));
		float angle = acos(tempVec.z);
		if (tempVec.x > 0.0f)
		{
			angle = -angle;
		}
		adjustRadian180(ref angle);
		// 在unity的坐标系中航向角需要取反
		angle = -angle;
		return angle;
	}
	public static Vector3 rotateVector3(Vector3 vec, Matrix4x4 transMat3) { return transMat3 * vec; }
	public static void rotateVector3(ref Vector3 vec, Matrix4x4 transMat3) { vec = transMat3 * vec; }
	// 使用一个四元数去旋转一个三维向量
	public static Vector3 rotateVector3(Vector3 vec, Quaternion transQuat) { return transQuat * vec; }
	public static void rotateVector3(ref Vector3 vec, Quaternion transQuat) { vec = transQuat * vec; }
	// 求向量水平顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 rotateVector3(Vector3 vec, float radian)
	{
		return rotateVector3(vec, Quaternion.AngleAxis(toDegree(radian), Vector3.up));
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 getVectorFromAngle(float radian)
	{
		adjustRadian180(ref radian);
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(sin(radian), 0.0f, cos(radian));
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector2 getVector2FromAngle(float radian)
	{
		adjustRadian180(ref radian);
		// 在unity坐标系是右手坐标系,所以x轴不需要添加负号
		return new(sin(radian), cos(radian));
	}
	public static int pcm_db_count(short[] ptr, int size)
	{
		long v = 0;
		for (int i = 0; i < size; ++i)
		{
			v += abs(ptr[i]);
		}
		v /= size;
		int ndb = -96;
		if (v != 0)
		{
			ndb = (int)(20.0f * Mathf.Log10(v * (1.0f / 0xFFFF)));
		}
		return ndb;
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	public static void getFrequencyZone(short[] pcmData, int dataCount, short[] frequencyList)
	{
		if (dataCount > MAX_FFT_COUNT)
		{
			logError("pcm data count is too many, data count : " + dataCount + ", max count : " + MAX_FFT_COUNT);
			return;
		}
		for (int i = 0; i < dataCount; ++i)
		{
			mComplexList[i] = new(pcmData[i], 0.0f);
		}
		fft(mComplexList, dataCount);
		for (int i = 0; i < dataCount; ++i)
		{
			frequencyList[i] = (short)sqrt(mComplexList[i].mReal * mComplexList[i].mReal + mComplexList[i].mImg * mComplexList[i].mImg);
		}
	}
	public static void secondToMinuteSecond(int seconds, out int outMin, out int outSec)
	{
		outMin = seconds / 60;
		outSec = seconds - outMin * 60;
	}
	public static void secondToHourMinuteSecond(int seconds, out int outHour, out int outMin, out int outSec)
	{
		outHour = seconds / 3600;
		outMin = (seconds - outHour * 3600) / 60;
		outSec = seconds - outHour * 3600 - outMin * 60;
	}
	public static void minuteToHourMinute(int minute, out int outHour, out int outMinute)
	{
		outHour = minute / 60;
		outMinute = minute - outHour * 60;
	}
	// 帧换算成秒
	public static float frameToSecond(float frame) { return frame * 0.0333f; }
	// 递归计算贝塞尔曲线的点
	public static Vector3 getBezier(IList<Vector3> points, bool loop, float t)
	{
		int pointCount = points.Count;
		if (pointCount == 2)
		{
			return lerp(points[0], points[1], t);
		}
		int tempCount = loop ? pointCount : pointCount - 1;
		Span<Vector3> temp = stackalloc Vector3[tempCount];
		for (int i = 0; i < tempCount; ++i)
		{
			temp[i] = lerp(points[i], points[(i + 1) % pointCount], t);
		}
		return getBezier(temp, loop, t);
	}
	public static Vector3 getBezier(Span<Vector3> points, bool loop, float t)
	{
		int pointCount = points.Length;
		if (pointCount == 2)
		{
			return lerp(points[0], points[1], t);
		}
		int tempCount = loop ? pointCount : pointCount - 1;
		Span<Vector3> temp = stackalloc Vector3[tempCount];
		for (int i = 0; i < tempCount; ++i)
		{
			temp[i] = lerp(points[i], points[(i + 1) % pointCount], t);
		}
		return getBezier(temp, loop, t);
	}
	public static void getBezierPoints(IList<Vector3> points, List<Vector3> resultList, bool loop, int bezierDetail = 20)
	{
		resultList.Clear();
		if (points.Count == 1 || bezierDetail <= 1)
		{
			return;
		}
		for (int i = 0; i < bezierDetail; ++i)
		{
			resultList.Add(getBezier(points, loop, divide(i, bezierDetail - 1)));
		}
	}
	public static void getBezierPoints(IList<Vector3> points, Span<Vector3> resultList, bool loop, int bezierDetail = 20)
	{
		resultList.Clear();
		if (points.Count == 1 || bezierDetail <= 1)
		{
			return;
		}
		if (resultList.Length < bezierDetail)
		{
			logError("resultList count not enough, cur:" + resultList.Length + ", need:" + bezierDetail);
			return;
		}
		for (int i = 0; i < bezierDetail; ++i)
		{
			resultList[i] = getBezier(points, loop, divide(i, bezierDetail - 1));
		}
	}
	public static List<Vector3> getBezierPoints(IList<Vector3> points, bool loop, int bezierDetail = 20)
	{
		if (points.Count == 1)
		{
			return new(points);
		}
		List<Vector3> bezierPoints = new();
		getBezierPoints(points, bezierPoints, loop, bezierDetail);
		return bezierPoints;
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	// 得到经过所有点的平滑曲线的点列表,detail是曲线平滑度,越大越平滑,scale是曲线接近折线的程度,越小越接近于折线
	public static void getCurvePoints(IList<Vector3> originPoint, List<Vector3> curveList, bool loop, int detail = 10, float scale = 0.6f)
	{
		curveList.Clear();
		if (originPoint.Count == 1)
		{
			curveList.Add(originPoint[0]);
			return;
		}
		int originCount = originPoint.Count;
		int middleCount = loop ? originCount : originCount - 1;
		using var a = new ListScope2<Vector3>(out var midPoints, out var extraPoints);
		midPoints.Capacity = middleCount;
		// 生成中点       
		for (int i = 0; i < middleCount; ++i)
		{
			midPoints.Add((originPoint[i] + originPoint[(i + 1) % originCount]) * 0.5f);
		}

		// 平移中点,计算每个顶点的两个控制点
		extraPoints.Capacity = 2 * originCount;
		for (int i = 0; i < originCount; ++i)
		{
			if (!loop)
			{
				if (i == 0)
				{
					extraPoints[0] = originPoint[0];
					extraPoints[1] = originPoint[0];
				}
				else if (i == originCount - 1)
				{
					extraPoints[i * 2 + 0] = originPoint[originCount - 1];
					extraPoints[i * 2 + 1] = originPoint[originCount - 1];
				}
				else
				{
					Vector3 midinmid = (midPoints[i] + midPoints[i - 1]) * 0.5f;
					// 朝 originPoint[i]方向收缩
					extraPoints[2 * i + 0] = originPoint[i] + (midPoints[i - 1] - midinmid) * scale;
					// 朝 originPoint[i]方向收缩
					extraPoints[2 * i + 1] = originPoint[i] + (midPoints[i] - midinmid) * scale;
				}
			}
			else
			{
				int backi = (i + originCount - 1) % originCount;
				Vector3 midinmid = (midPoints[i] + midPoints[backi]) * 0.5f;
				// 朝 originPoint[i]方向收缩
				extraPoints[2 * i + 0] = originPoint[i] + (midPoints[backi] - midinmid) * scale;
				// 朝 originPoint[i]方向收缩
				extraPoints[2 * i + 1] = originPoint[i] + (midPoints[i] - midinmid) * scale;
			}
		}

		int bezierCount = loop ? originCount : originCount - 1;
		float step = divide(1, detail - 1);
		// 生成4控制点，产生贝塞尔曲线
		Span<Vector3> tempControlPoint = stackalloc Vector3[4];
		for (int i = 0; i < bezierCount; ++i)
		{
			tempControlPoint[0] = originPoint[i];
			tempControlPoint[1] = extraPoints[2 * i + 1];
			tempControlPoint[2] = extraPoints[2 * (i + 1) % extraPoints.Count];
			tempControlPoint[3] = originPoint[(i + 1) % originCount];
			for (int j = 0; j < detail; ++j)
			{
				Vector3 point = getBezier(tempControlPoint, false, j * step);
				// 如果与上一个点重合了,则不放入列表中
				curveList.addIf(point, !isVectorEqual(curveList[^1], point));
			}
		}
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	// 得到经过所有点的平滑曲线的点列表,detail是曲线平滑度,越大越平滑,scale是曲线接近折线的程度,越小越接近于折线
	public static int getCurvePoints(IList<Vector3> originPoint, Span<Vector3> curveList, bool loop, int detail = 10, float scale = 0.6f)
	{
		curveList.Clear();
		int hasCount = 0;
		if (originPoint.Count == 1)
		{
			curveList[hasCount++] = originPoint[0];
			return hasCount;
		}
		int originCount = originPoint.Count;
		int middleCount = loop ? originCount : originCount - 1;
		using var a = new ListScope2<Vector3>(out var midPoints, out var extraPoints);
		midPoints.Capacity = middleCount;
		// 生成中点       
		for (int i = 0; i < middleCount; ++i)
		{
			midPoints.Add((originPoint[i] + originPoint[(i + 1) % originCount]) * 0.5f);
		}

		// 平移中点,计算每个顶点的两个控制点
		extraPoints.Capacity = 2 * originCount;
		for (int i = 0; i < originCount; ++i)
		{
			if (!loop)
			{
				if (i == 0)
				{
					extraPoints[0] = originPoint[0];
					extraPoints[1] = originPoint[0];
				}
				else if (i == originCount - 1)
				{
					extraPoints[i * 2 + 0] = originPoint[originCount - 1];
					extraPoints[i * 2 + 1] = originPoint[originCount - 1];
				}
				else
				{
					Vector3 midinmid = (midPoints[i] + midPoints[i - 1]) * 0.5f;
					// 朝 originPoint[i]方向收缩
					extraPoints[2 * i + 0] = originPoint[i] + (midPoints[i - 1] - midinmid) * scale;
					// 朝 originPoint[i]方向收缩
					extraPoints[2 * i + 1] = originPoint[i] + (midPoints[i] - midinmid) * scale;
				}
			}
			else
			{
				int backi = (i + originCount - 1) % originCount;
				Vector3 midinmid = (midPoints[i] + midPoints[backi]) * 0.5f;
				// 朝 originPoint[i]方向收缩
				extraPoints[2 * i + 0] = originPoint[i] + (midPoints[backi] - midinmid) * scale;
				// 朝 originPoint[i]方向收缩
				extraPoints[2 * i + 1] = originPoint[i] + (midPoints[i] - midinmid) * scale;
			}
		}

		int bezierCount = loop ? originCount : originCount - 1;
		float step = divide(1, detail - 1);
		// 生成4控制点，产生贝塞尔曲线
		Span<Vector3> tempControlPoint = stackalloc Vector3[4];
		for (int i = 0; i < bezierCount; ++i)
		{
			tempControlPoint[0] = originPoint[i];
			tempControlPoint[1] = extraPoints[2 * i + 1];
			tempControlPoint[2] = extraPoints[2 * (i + 1) % extraPoints.Count];
			tempControlPoint[3] = originPoint[(i + 1) % originCount];
			for (int j = 0; j < detail; ++j)
			{
				Vector3 point = getBezier(tempControlPoint, false, j * step);
				// 如果与上一个点重合了,则不放入列表中
				if (!isVectorEqual(curveList[hasCount - 1], point))
				{
					curveList[hasCount++] = point;
				}
			}
		}
		return hasCount;
	}
	public static uint generateGUID()
	{
		// 获得当前时间,再获取一个随机数,组成一个尽量不会重复的ID
		TimeSpan timeForm19700101 = DateTime.Now - new DateTime(1970, 1, 1);
		uint halfIntMS = (uint)((ulong)timeForm19700101.TotalMilliseconds % 0x7FFFFFFF);
		return halfIntMS + (uint)randomInt(0, 0x7FFFFFFF);
	}
	// rgb转换为色相(H),饱和度(S),亮度(L)
	// HSL和RGB的范围都是0-1
	public static Vector3 RGBtoHSL(Vector3 rgb)
	{
		float minRGB = getMin(getMin(rgb.x, rgb.y), rgb.z);
		float maxRGB = getMax(getMax(rgb.x, rgb.y), rgb.z);
		float delta = maxRGB - minRGB;
		float H = 0.0f;
		float S = 0.0f;
		float L = (maxRGB + minRGB) * 0.5f;
		// 如果三个分量的最大和最小相等,则说明该颜色是灰色的,灰色的色相和饱和度都为0
		if (delta > 0.0f)
		{
			if (L < 0.5f)
			{
				S = divide(delta, maxRGB + minRGB);
			}
			else
			{
				S = divide(delta, 2.0f - maxRGB - minRGB);
			}
			float inverseDelta = divide(1.0f, delta);
			float halfDelta = delta * 0.5f;
			float delR = ((maxRGB - rgb.x) * 0.1667f + halfDelta) * inverseDelta;
			float delG = ((maxRGB - rgb.y) * 0.1667f + halfDelta) * inverseDelta;
			float delB = ((maxRGB - rgb.z) * 0.1667f + halfDelta) * inverseDelta;
			if (isFloatEqual(rgb.x, maxRGB))
			{
				H = delB - delG;
			}
			else if (isFloatEqual(rgb.y, maxRGB))
			{
				H = 0.333f + delR - delB;
			}
			else if (isFloatEqual(rgb.z, maxRGB))
			{
				H = 0.6667f + delG - delR;
			}
			if (H < 0.0f)
			{
				H += 1.0f;
			}
			else if (H > 1.0f)
			{
				H -= 1.0f;
			}
		}
		return new(H, S, L);
	}

	// 色相(H),饱和度(S),亮度(L),转换为rgb
	// HSL和RGB的范围都是0-1
	public static Vector3 HSLtoRGB(Vector3 hsl)
	{
		Vector3 rgb = Vector3.zero;
		float H = hsl.x;
		float S = hsl.y;
		float L = hsl.z;
		if (S == 0.0)                       // HSL from 0 to 1
		{
			rgb.x = L;              // RGB results from 0 to 255
			rgb.y = L;
			rgb.z = L;
		}
		else
		{
			float var2;
			if (L < 0.5f)
			{
				var2 = L * (1.0f + S);
			}
			else
			{
				var2 = L + S - (S * L);
			}

			float var1 = 2.0f * L - var2;
			rgb.x = HueToRGB(var1, var2, H + 0.333f);
			rgb.y = HueToRGB(var1, var2, H);
			rgb.z = HueToRGB(var1, var2, H - 0.333f);
		}
		return rgb;
	}
	public static void randomOrder(List<int> list)
	{
		for (int i = list.Count - 1; i > 0; --i)
		{
			// 随机数生成器，范围[0, i]  
			int rand = randomInt(0, i);
			int temp = list[i];
			list[i] = list[rand];
			list[rand] = temp;
		}
	}
	public static float speedToInterval(float speed) 
	{
		return divide(0.0333f, speed); 
	}
	public static float intervalToSpeed(float interval) 
	{
		return divide(0.0333f, interval);
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	public static bool AStar4(List<bool> map, int begin, int end, int width, List<int> foundPath)
	{
		return AStar4(map, Point.fromIndex(begin, width), Point.fromIndex(end, width), width, foundPath);
	}
	public static bool AStar4(List<bool> map, Point begin, Point end, int width, List<int> foundPath)
	{
		foundPath?.Clear();
		if (!checkAStar(map, begin, end, width))
		{
			return false;
		}
		int beginIndex = begin.toIndex(width);
		int endIndex = end.toIndex(width);
		int maxCount = map.Count;
		int height = divideInt(maxCount, width);
		if (mTempNodeList.count() < maxCount)
		{
			mTempNodeList = new AStarNode[maxCount];
		}
		int count0 = mTempNodeList.Length;
		for (int i = 0; i < count0; ++i)
		{
			mTempNodeList[i].init(i);
		}
		mTempOpenList.Clear();
		AStarNode parentNode = new(0, 0, 0, beginIndex, -1, 0);
		mTempOpenList.Add(parentNode);
		while (true)
		{
			// 找出F值最低的格子
			int minFPosInOpenList = 0;
			// 具有最小F值的下标
			int minFIndex = mTempOpenList[minFPosInOpenList].mIndex;
			int len = mTempOpenList.Count;
			for (int i = 1; i < len; ++i)
			{
				AStarNode temp = mTempOpenList[i];
				if (temp.mF < mTempNodeList[minFIndex].mF)
				{
					minFIndex = temp.mIndex;
					minFPosInOpenList = i;
				}
			}
			mTempOpenList.RemoveAt(minFPosInOpenList);
			// 添加进关闭列表里
			mTempNodeList[minFIndex].mState = NODE_STATE.CLOSE;
			parentNode = mTempNodeList[minFIndex];
			// 父节点的坐标
			int parentNodeX = parentNode.mIndex % width;
			int parentNodeY = divideInt(parentNode.mIndex, width);
			mTempDirect4[0] = new(parentNodeX, parentNodeY - 1);
			mTempDirect4[1] = new(parentNodeX - 1, parentNodeY);
			mTempDirect4[2] = new(parentNodeX + 1, parentNodeY);
			mTempDirect4[3] = new(parentNodeX, parentNodeY + 1);
			int dirCount = mTempDirect4.Length;
			// 将父节点周围可以通过的格子放进开启列表里
			for (int i = 0; i < dirCount; ++i)
			{
				Point point = mTempDirect4[i];
				int curIndex = point.toIndex(width);
				if (point.x >= 0 && point.x < width &&
					point.y >= 0 && point.y < height &&
					map[curIndex])
				{
					AStarNode curNode = mTempNodeList[curIndex];
					if (curNode.mState == NODE_STATE.CLOSE)
					{
						continue;
					}
					// 计算格子的G1值,即从当前父格到这个格子的G值再加上父格子自己的G值
					int G1 = parentNode.mG + 10;
					// 如果它已经在开启列表里面了
					if (curNode.mState == NODE_STATE.OPEN)
					{
						// 检查G值来判定，如果通过这一格到达那里，路径是否更好
						// 如果新的G值更小一些
						if (G1 < curNode.mG)
						{
							int pos = -1;
							int tempCount = mTempOpenList.Count;
							for (int j = 0; j < tempCount; ++j)
							{
								if (mTempOpenList[j].mIndex == curNode.mIndex)
								{
									pos = j;
									break;
								}
							}
							// 将当前点的父节点设为F值最小的节点,重新计算当前节点的数值
							curNode.mParent = parentNode.mIndex;
							curNode.mG = G1;
							curNode.mF = curNode.mG + curNode.mH;
							// 修改开启列表里的节点
							mTempOpenList[pos] = curNode;
						}
					}
					// 如果不在开启列表里,则加入开启列表
					else
					{
						// 计算G值, H值，F值，并设置这些节点的父节点
						curNode.mParent = parentNode.mIndex;
						curNode.mG = G1;
						curNode.mH = (abs(point.x - end.x) + abs(point.y - end.y)) * 10;
						curNode.mF = curNode.mG + curNode.mH;
						curNode.mState = NODE_STATE.OPEN;
						mTempOpenList.Add(curNode);
					}
					mTempNodeList[curIndex] = curNode;
				}
			}
			// 开启列表已空，表示没有路可以走
			if (mTempOpenList.Count == 0)
			{
				return false;
			}
			// 终点被添加进开启列表里，找到路了
			if (mTempNodeList[endIndex].mState == NODE_STATE.OPEN)
			{
				break;
			}
		}
		postAStar(mTempNodeList, endIndex, foundPath);
		return true;
	}
	public static bool AStar8(List<bool> map, int begin, int end, int width, List<int> foundPath)
	{
		return AStar8(map, Point.fromIndex(begin, width), Point.fromIndex(end, width), width, foundPath);
	}
	public static bool AStar8(List<bool> map, Point begin, Point end, int width, List<int> foundPath)
	{
		foundPath?.Clear();
		if (!checkAStar(map, begin, end, width))
		{
			return false;
		}
		int beginIndex = begin.toIndex(width);
		int endIndex = end.toIndex(width);
		int maxCount = map.Count;
		int height = divideInt(maxCount, width);
		if (mTempNodeList.count() < maxCount)
		{
			mTempNodeList = new AStarNode[maxCount];
		}
		int count0 = mTempNodeList.Length;
		for (int i = 0; i < count0; ++i)
		{
			mTempNodeList[i].init(i);
		}
		mTempOpenList.Clear();
		AStarNode parentNode = new(0, 0, 0, beginIndex, -1, 0);
		mTempOpenList.Add(parentNode);
		while (true)
		{
			// 找出F值最低的格子
			int minFPosInOpenList = 0;
			// 具有最小F值的下标
			int minFIndex = mTempOpenList[minFPosInOpenList].mIndex;
			int len = mTempOpenList.Count;
			for (int i = 1; i < len; ++i)
			{
				AStarNode temp = mTempOpenList[i];
				if (temp.mF < mTempNodeList[minFIndex].mF)
				{
					minFIndex = temp.mIndex;
					minFPosInOpenList = i;
				}
			}
			mTempOpenList.RemoveAt(minFPosInOpenList);
			// 添加进关闭列表里
			mTempNodeList[minFIndex].mState = NODE_STATE.CLOSE;
			parentNode = mTempNodeList[minFIndex];
			// 父节点的坐标
			int parentNodeX = parentNode.mIndex % width;
			int parentNodeY = divideInt(parentNode.mIndex, width);
			mTempDirect8[0] = new(parentNodeX - 1, parentNodeY - 1);
			mTempDirect8[1] = new(parentNodeX, parentNodeY - 1);
			mTempDirect8[2] = new(parentNodeX + 1, parentNodeY - 1);
			mTempDirect8[3] = new(parentNodeX - 1, parentNodeY);
			mTempDirect8[4] = new(parentNodeX + 1, parentNodeY);
			mTempDirect8[5] = new(parentNodeX - 1, parentNodeY + 1);
			mTempDirect8[6] = new(parentNodeX, parentNodeY + 1);
			mTempDirect8[7] = new(parentNodeX + 1, parentNodeY + 1);
			int dirCount = mTempDirect8.Length;
			// 将父节点周围可以通过的格子放进开启列表里
			for (int i = 0; i < dirCount; ++i)
			{
				Point point = mTempDirect8[i];
				int curIndex = point.toIndex(width);
				if (point.x >= 0 && point.x < width &&
					point.y >= 0 && point.y < height &&
					map[curIndex])
				{
					AStarNode curNode = mTempNodeList[curIndex];
					if (curNode.mState == NODE_STATE.CLOSE)
					{
						continue;
					}
					// 计算格子的G1值,即从当前父格到这个格子的G值再加上父格子自己的G值
					int G1 = parentNode.mG + ((point.x == parentNodeX || point.y == parentNodeY) ? parentNode.mG + 10 : parentNode.mG + 14);
					// 如果它已经在开启列表里面了
					if (curNode.mState == NODE_STATE.OPEN)
					{
						// 检查G值来判定，如果通过这一格到达那里，路径是否更好
						// 如果新的G值更小一些
						if (G1 < curNode.mG)
						{
							int pos = -1;
							int tempCount = mTempOpenList.Count;
							for (int j = 0; j < tempCount; ++j)
							{
								if (mTempOpenList[j].mIndex == curNode.mIndex)
								{
									pos = j;
									break;
								}
							}
							// 将当前点的父节点设为F值最小的节点,重新计算当前节点的数值
							curNode.mParent = parentNode.mIndex;
							curNode.mG = G1;
							curNode.mF = curNode.mG + curNode.mH;
							// 修改开启列表里的节点
							mTempOpenList[pos] = curNode;
						}
					}
					// 如果不在开启列表里,则加入开启列表
					else
					{
						// 计算G值, H值，F值，并设置这些节点的父节点
						curNode.mParent = parentNode.mIndex;
						curNode.mG = G1;
						curNode.mH = (abs(point.x - end.x) + abs(point.y - end.y)) * 10;
						curNode.mF = curNode.mG + curNode.mH;
						curNode.mState = NODE_STATE.OPEN;
						mTempOpenList.Add(curNode);
					}
					mTempNodeList[curIndex] = curNode;
				}
			}
			// 开启列表已空，表示没有路可以走
			if (mTempOpenList.Count == 0)
			{
				return false;
			}
			// 终点被添加进开启列表里，找到路了
			if (mTempNodeList[endIndex].mState == NODE_STATE.OPEN)
			{
				break;
			}
		}
		postAStar(mTempNodeList, endIndex, foundPath);
		return true;
	}
	public static bool AStar6(List<bool> map, int begin, int end, int width, List<int> foundPath)
	{
		return AStar6(map, Point.fromIndex(begin, width), Point.fromIndex(end, width), width, foundPath);
	}
	public static bool AStar6(List<bool> map, Point begin, Point end, int width, List<int> foundPath)
	{
		foundPath?.Clear();
		if (!checkAStar(map, begin, end, width))
		{
			return false;
		}
		int beginIndex = begin.toIndex(width);
		int endIndex = end.toIndex(width);
		int maxCount = map.Count;
		int height = divideInt(maxCount, width);
		if (mTempNodeList.count() < maxCount)
		{
			mTempNodeList = new AStarNode[maxCount];
		}
		int count0 = mTempNodeList.Length;
		for (int i = 0; i < count0; ++i)
		{
			mTempNodeList[i].init(i);
		}
		mTempOpenList.Clear();
		AStarNode parentNode = new(0, 0, 0, beginIndex, -1, 0);
		mTempOpenList.Add(parentNode);
		while (true)
		{
			// 找出F值最低的格子
			int minFPosInOpenList = 0;
			// 具有最小F值的下标
			int minFIndex = mTempOpenList[minFPosInOpenList].mIndex;
			int len = mTempOpenList.Count;
			for (int i = 1; i < len; ++i)
			{
				AStarNode temp = mTempOpenList[i];
				if (temp.mF < mTempNodeList[minFIndex].mF)
				{
					minFIndex = temp.mIndex;
					minFPosInOpenList = i;
				}
			}
			mTempOpenList.RemoveAt(minFPosInOpenList);
			// 添加进关闭列表里
			mTempNodeList[minFIndex].mState = NODE_STATE.CLOSE;
			parentNode = mTempNodeList[minFIndex];
			// 父节点的坐标
			int parentNodeX = parentNode.mIndex % width;
			int parentNodeY = divideInt(parentNode.mIndex, width);
			// 这里依赖于不同行相同x坐标的格子之间偶数行的格子始终比奇数行的要靠左一些
			// 偶数行
			if ((parentNodeY & 1) == 0)
			{
				mTempDirect6[0] = new(parentNodeX - 1, parentNodeY - 1);
				mTempDirect6[1] = new(parentNodeX, parentNodeY - 1);
				mTempDirect6[2] = new(parentNodeX + 1, parentNodeY);
				mTempDirect6[3] = new(parentNodeX, parentNodeY + 1);
				mTempDirect6[4] = new(parentNodeX - 1, parentNodeY + 1);
				mTempDirect6[5] = new(parentNodeX - 1, parentNodeY);
			}
			// 奇数行
			else
			{
				mTempDirect6[0] = new(parentNodeX, parentNodeY - 1);
				mTempDirect6[1] = new(parentNodeX + 1, parentNodeY - 1);
				mTempDirect6[2] = new(parentNodeX + 1, parentNodeY);
				mTempDirect6[3] = new(parentNodeX + 1, parentNodeY + 1);
				mTempDirect6[4] = new(parentNodeX, parentNodeY + 1);
				mTempDirect6[5] = new(parentNodeX - 1, parentNodeY);
			}
			int dirCount = mTempDirect6.Length;
			// 将父节点周围可以通过的格子放进开启列表里
			for (int i = 0; i < dirCount; ++i)
			{
				Point point = mTempDirect6[i];
				int curIndex = point.toIndex(width);
				if (point.x >= 0 && point.x < width &&
					point.y >= 0 && point.y < height &&
					map[curIndex])
				{
					AStarNode curNode = mTempNodeList[curIndex];
					if (curNode.mState == NODE_STATE.CLOSE)
					{
						continue;
					}
					// 计算格子的G1值,即从当前父格到这个格子的G值再加上父格子自己的G值
					// 六边形到周围相邻六边形的距离都是一样的
					int G1 = parentNode.mG + 10;
					// 如果它已经在开启列表里面了
					if (curNode.mState == NODE_STATE.OPEN)
					{
						// 检查G值来判定，如果通过这一格到达那里，路径是否更好
						// 如果新的G值更小一些
						if (G1 < curNode.mG)
						{
							int pos = -1;
							int tempCount = mTempOpenList.Count;
							for (int j = 0; j < tempCount; ++j)
							{
								if (mTempOpenList[j].mIndex == curNode.mIndex)
								{
									pos = j;
									break;
								}
							}
							// 将当前点的父节点设为F值最小的节点,重新计算当前节点的数值
							curNode.mParent = parentNode.mIndex;
							curNode.mG = G1;
							curNode.mF = curNode.mG + curNode.mH;
							// 修改开启列表里的节点
							mTempOpenList[pos] = curNode;
						}
					}
					// 如果不在开启列表里,则加入开启列表
					else
					{
						// 偶数行和奇数行到终点的预估距离还是有点差别
						int disX = abs(point.x - end.x);
						int disY = abs(point.y - end.y);
						float disFX;
						// 同是奇数行或者同是偶数行
						if ((point.y & 1) == (end.y & 1))
						{
							disFX = disX;
						}
						else
						{
							// 当前点是偶数行,目标点是奇数行
							if ((point.y & 1) == 0)
							{
								// 奇偶行将下标转换为浮点数,会发现X轴的下标实际上相差0.5
								disFX = disX + 0.5f;
							}
							// 当前点是奇数行,目标点是偶数行
							else
							{
								// 奇偶行将下标转换为浮点数,会发现X轴的下标实际上相差0.5
								disFX = disX - 0.5f;
							}
						}
						// 斜着走一格,相当于横向走0.5格和纵向走1格
						// 所以当横向距离大于纵向距离的2倍时,需要先斜着走到同一行,然后再走剩下的横向距离
						// 斜着走的距离就是纵向的距离
						if (disFX > disY * 0.5f)
						{
							// 等效于disFY + disFX - disFY * 0.5f
							curNode.mH = (int)(disFX + disY * 0.5f) * 10;
						}
						// 当横向距离小于等于纵向距离的2倍时,只需要走到同一行即可
						else
						{
							curNode.mH = disY * 10;
						}
						// 计算G值, H值，F值，并设置这些节点的父节点
						curNode.mParent = parentNode.mIndex;
						curNode.mG = G1;
						curNode.mF = curNode.mG + curNode.mH;
						curNode.mState = NODE_STATE.OPEN;
						mTempOpenList.Add(curNode);
					}
					mTempNodeList[curIndex] = curNode;
				}
			}
			// 开启列表已空，表示没有路可以走
			if (mTempOpenList.Count == 0)
			{
				return false;
			}
			// 终点被添加进开启列表里，找到路了
			if (mTempNodeList[endIndex].mState == NODE_STATE.OPEN)
			{
				break;
			}
		}
		postAStar(mTempNodeList, endIndex, foundPath);
		return true;
	}
	public static void quickSort<T>(List<T> arr, Comparison<T> comparison)
	{
		quickSort(arr, 0, arr.Count - 1, comparison);
	}
	public static void quickSort<T>(List<T> arr, bool ascend = true) where T : IComparable<T>
	{
		quickSort(arr, 0, arr.Count - 1, ascend);
	}
	public static bool overlapBox3(Vector3 pos0, Vector3 size0, Vector3 pos1, Vector3 size1)
	{
		Vector3 min0 = pos0 - size0 * 0.5f;
		Vector3 max0 = pos0 + size0 * 0.5f;
		Vector3 min1 = pos1 - size1 * 0.5f;
		Vector3 max1 = pos1 + size1 * 0.5f;
		return isVector3Less(min0, max1) && isVector3Greater(max0, min1);
	}
	public static bool overlapBox2(Vector2 pos0, Vector2 size0, Vector2 pos1, Vector2 size1)
	{
		Vector2 min0 = pos0 - size0 * 0.5f;
		Vector2 max0 = pos0 + size0 * 0.5f;
		Vector2 min1 = pos1 - size1 * 0.5f;
		Vector2 max1 = pos1 + size1 * 0.5f;
		return isVector2Less(min0, max1) && isVector2Greater(max0, min1);
	}
	// 将凹多边形分割为多个凸多边形,暂未经过验证
	public static void dividePolygonToTriangle(List<Vector2> originPoints, List<ConvexPolygon> polygonList)
	{
		using var a = new ListScope<Vector2>(out var tempVertices, originPoints);
		bool order = isClockwise(tempVertices);
		// 将多边形拆分成多个凸多边形
		int startIndex = -1;
		int curIndex = 0;
		int maxLoopCount = 10000;
		while (maxLoopCount-- > 0)
		{
			// 已经遍历到了最后一个点还是没找到可以分割的点,则剩下的就是一个凸多边形,至此全部拆分完毕
			if (curIndex == startIndex)
			{
				polygonList.addClass().mPoints.addRange(tempVertices);
				break;
			}
			// 已经遍历到了最后一个点,仍然不是凹陷点,则剩下的就是凸多边形,否则就继续循环
			if (startIndex == -1 && nextIndex(tempVertices.count(), curIndex) == 0 && !isConcavePoint(tempVertices, curIndex, order))
			{
				polygonList.addClass().mPoints.addRange(tempVertices);
				break;
			}
			// 寻找第一个凹陷点
			if (startIndex < 0)
			{
				if (isConcavePoint(tempVertices, curIndex, order))
				{
					startIndex = curIndex;
				}
			}
			else
			{
				// 找到了一个凹陷点
				if (isConcavePoint(tempVertices, curIndex, order))
				{
					// 找到了第二个凹陷点
					// 如果当前点与前一个凹陷点之间没有其他的点,则以此凹陷点为起点
					if (nextIndex(tempVertices.Count, curIndex) == startIndex || prevIndex(tempVertices.Count, curIndex) == startIndex)
					{
						startIndex = curIndex;
					}
					// 分割当前凹陷点与前一个凹陷点之间的顶点
					else
					{
						cutOffPolygon(tempVertices, polygonList.addClass().mPoints, ref startIndex, ref curIndex);
					}
				}
				// 如果不是凹陷点,但是当前点与找到的前一个凹陷点的夹角已经超过了180度,则以此来分割
				else
				{
					int startNext = nextIndex(tempVertices.Count, startIndex);
					if (!isConvexVertice(tempVertices, startNext, startIndex, curIndex, order))
					{
						curIndex = prevIndex(tempVertices.Count, curIndex);
						cutOffPolygon(tempVertices, polygonList.addClass().mPoints, ref startIndex, ref curIndex);
					}
				}
			}
			// 下标需要循环
			curIndex = nextIndex(tempVertices.Count, curIndex);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 一个多边形中的三个点的夹角是否大于180
	protected static bool isConvexVertice(List<Vector2> points, int index0, int index1, int index2, bool isClockwise)
	{
		if (index0 == index2)
		{
			return true;
		}
		// 计算叉积，判断该点是否为凹陷点
		float crossProduct = cross(points[index0], points[index1], points[index2]);
		// 根据整体方向判断凹陷点：如果多边形是顺时针，凹陷点的叉积应该是负的；如果是逆时针，应该是正的
		if (isClockwise)
		{
			return crossProduct > 0;  // 顺时针时，凹陷点应该是正叉积
		}
		else
		{
			return crossProduct < 0;  // 逆时针时，凹陷点应该是负叉积
		}
	}
	// 计算叉积，返回值表示两个向量的旋转方向
	protected static float cross(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p2.x - p1.x) * (p3.y - p2.y) - (p2.y - p1.y) * (p3.x - p2.x);
	}

	// 判断多边形的整体方向（顺时针还是逆时针）
	protected static bool isClockwise(List<Vector2> polygon)
	{
		float sum = 0;
		int n = polygon.Count;
		for (int i = 0; i < n; i++)
		{
			Vector2 p1 = polygon[i];
			Vector2 p2 = polygon[(i + 1) % n];
			sum += (p2.x - p1.x) * (p2.y + p1.y);
		}
		return sum > 0;  // 如果和大于0，则是顺时针，否则逆时针
	}
	// 判断给定多边形的一个点是否是凹陷点
	protected static bool isConcavePoint(List<Vector2> polygon, int index, bool isClockwise)
	{
		int n = polygon.Count;
		// 获取相邻的两个点
		Vector2 p0 = polygon[(index - 1 + n) % n];  // 上一个点
		Vector2 p1 = polygon[index];                 // 当前点
		Vector2 p2 = polygon[(index + 1) % n];       // 下一个点

		// 计算叉积，判断该点是否为凹陷点
		float crossProduct = cross(p0, p1, p2);
		// 根据整体方向判断凹陷点：如果多边形是顺时针，凹陷点的叉积应该是负的；如果是逆时针，应该是正的
		if (isClockwise)
		{
			return crossProduct > 0;  // 顺时针时，凹陷点应该是正叉积
		}
		else
		{
			return crossProduct < 0;  // 逆时针时，凹陷点应该是负叉积
		}
	}
	protected static void cutOffPolygon(List<Vector2> origin, List<Vector2> dest, ref int start, ref int end)
	{
		if (start < end)
		{
			for (int i = start; i <= end; ++i)
			{
				dest.add(origin[i]);
			}
			// 去除已经分割下的顶点时需要保留第一个和最后一个顶点
			origin.RemoveRange(start + 1, end - (start + 1));
			end = start;
			// 重新寻找凹陷点
			start = -1;
		}
		else if (start > end)
		{
			// 拆分成两部分,start->0,0->end
			for (int i = start; i < origin.count(); ++i)
			{
				dest.add(origin[i]);
			}
			if (end == 0)
			{
				origin.RemoveRange(start + 1, clampMin(origin.count() - 1 - (start + 1)));
			}
			else
			{
				origin.RemoveRange(start + 1, origin.count() - (start + 1));
			}

			for (int i = 0; i <= end; ++i)
			{
				dest.add(origin[i]);
			}
			origin.RemoveRange(0, end);
			start = 0;
			end = 0;
		}
	}
	protected static int prevIndex(int verticeCount, int index)
	{
		return (index - 1 + verticeCount) % verticeCount;
	}
	protected static int nextIndex(int verticeCount, int index)
	{
		return (index + 1) % verticeCount;
	}
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
	protected static void initFFTParam()
	{
		// 精度(PI PI/2 PI/4 PI/8 PI/16 ... PI/(2^k))
		mSinList = new float[]
		{
			0.000000f, 1.000000f, 0.707107f, 0.382683f, 0.195090f, 0.098017f,
			0.049068f, 0.024541f, 0.012272f, 0.006136f, 0.003068f, 0.001534f,
			0.000767f, 0.000383f, 0.000192f, 0.000096f, 0.000048f, 0.000024f,
			0.000012f, 0.000006f, 0.000003f, 0.000003f, 0.000003f, 0.000003f,
			0.000003f
		};
		// 精度(PI PI/2 PI/4 PI/8 PI/16 ... PI/(2^k))
		mCosList = new float[]
		{
			-1.000000f, 0.000000f, 0.707107f, 0.923880f, 0.980785f, 0.995185f,
			0.998795f, 0.999699f, 0.999925f, 0.999981f, 0.999995f, 0.999999f,
			1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f,
			1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f,
			1.000000f
		};
		for (int i = 0; i < mComplexList.Length; ++i)
		{
			mComplexList[i] = new();
		}
	}
	/*
	* FFT Algorithm
	* === Inputs ===
	* x : complex numbers
	* N : nodes of FFT. @N should be power of 2, that is 2^(*)
	* === Output ===
	* the @x contains the result of FFT algorithm, so the original data
	* in @x is destroyed, please store them before using FFT.
	*/
	protected static void fft(Complex[] x, int count)
	{
		if (mSinList == null)
		{
			initFFTParam();
		}
		int i, j, k;
		float sR, sI, uR, uI;
		Complex tempComplex = new();

		/*
		* bit reversal sorting
		*/
		int l = count >> 1;
		j = l;
		int forCount = count - 2;
		for (i = 1; i <= forCount; ++i)
		{
			if (i < j)
			{
				tempComplex = x[j];
				x[j] = x[i];
				x[i] = tempComplex;
			}
			k = l;
			while (k <= j)
			{
				j -= k;
				k >>= 1;
			}
			j += k;
		}

		/*
		* For Loops
		*/
		int dftForCount = count - 1;
		int le = 1;
		int halfLe;
		int M = (int)(Mathf.Log(count) / Mathf.Log(2));
		int ip;
		for (l = 0; l < M; ++l)
		{
			// 在le左移1位之前保存值,也就是左移以后的值的一半
			halfLe = le;
			le <<= 1;
			uR = 1;
			uI = 0;
			sR = mCosList[l];
			sI = -mSinList[l];
			for (j = 0; j < halfLe; ++j)
			{
				/* loop for each sub DFT */
				for (i = j; i <= dftForCount; i += le)
				{
					/* loop for each butterfly */
					ip = i + halfLe;
					tempComplex.mReal = x[ip].mReal * uR - x[ip].mImg * uI;
					tempComplex.mImg = x[ip].mReal * uI + x[ip].mImg * uR;
					x[ip] = x[i] - tempComplex;
					x[i] += tempComplex;
				}
				tempComplex.mReal = uR;
				uR = tempComplex.mReal * sR - uI * sI;
				uI = tempComplex.mReal * sI + uI * sR;
			}
		}
	}
	/*
	* Inverse FFT Algorithm
	* === Inputs ===
	* x : complex numbers
	* N : nodes of FFT. @N should be power of 2, that is 2^(*)
	* === Output ===
	* the @x contains the result of FFT algorithm, so the original data
	* in @x is destroyed, please store them before using FFT.
	*/
	protected static void ifft(Complex[] x, int count)
	{
		if (count == 0)
		{
			return;
		}
		for (int k = 0; k <= count - 1; ++k)
		{
			x[k].mImg = -x[k].mImg;
		}
		fft(x, count);
		float inverseCount = divide(1.0f, count);
		for (int k = 0; k <= count - 1; ++k)
		{
			x[k].mReal = x[k].mReal * inverseCount;
			x[k].mImg = -x[k].mImg * inverseCount;
		}
	}
	protected static float HueToRGB(float v1, float v2, float vH)
	{
		if (vH < 0.0f)
		{
			vH += 1.0f;
		}
		if (vH > 1.0f)
		{
			vH -= 1.0f;
		}
		if (6.0f * vH < 1.0f)
		{
			return v1 + (v2 - v1) * 6.0f * vH;
		}
		else if (2.0f * vH < 1.0f)
		{
			return v2;
		}
		else if (3.0f * vH < 2.0f)
		{
			return v1 + (v2 - v1) * (0.667f - vH) * 6.0f;
		}
		else
		{
			return v1;
		}
	}
	protected static bool checkAStar(List<bool> map, Point begin, Point end, int width)
	{
		int beginIndex = begin.toIndex(width);
		int endIndex = end.toIndex(width);
		// 起点与终点在同一点,不需要寻路
		if (beginIndex == endIndex)
		{
			return true;
		}
		if (beginIndex < 0 || beginIndex >= map.Count)
		{
			logError("起点下标错误,x:" + begin.x + ", y:" + begin.y + ", width:" + width + ", map length:" + map.Count);
			return false;
		}
		if (endIndex < 0 || endIndex >= map.Count)
		{
			logError("目标点下标错误,x:" + end.x + ", y:" + end.y + ", width:" + width + ", map length:" + map.Count);
			return false;
		}
		// 起点或者终点不可行走,也不能找到路径
		if (!map[beginIndex] || !map[endIndex])
		{
			return false;
		}
		return true;
	}
	protected static void postAStar(AStarNode[] nodeList, int endIndex, List<int> foundPath)
	{
		if (foundPath == null)
		{
			return;
		}
		foundPath.Add(endIndex);
		AStarNode road = nodeList[endIndex];
		while (road.mParent != -1)
		{
			foundPath.Add(road.mParent);
			road = nodeList[road.mParent];
		}
		int count = foundPath.Count;
		int halfCount = count >> 1;
		for (int i = 0; i < halfCount; ++i)
		{
			int temp = foundPath[i];
			foundPath[i] = foundPath[count - i - 1];
			foundPath[count - i - 1] = temp;
		}
	}
}