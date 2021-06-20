using System;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility : StringUtility
{
	private static List<AStarNode> mTempOpenList = new List<AStarNode>();
	private static Point[] mTempDirect8 = new Point[8];
	private static Point[] mTempDirect4 = new Point[4];
	private static Vector3[] mTempControlPoint = new Vector3[4];
	private static AStarNode[] mTempNodeList;
	private static int[] mGreaterPow2 = new int[513];
	private static float[] sin_tb;
	private static float[] cos_tb;
	private const int mMaxFFTCount = 1024 * 8;
	private static Complex[] mComplexList = new Complex[mMaxFFTCount];
	public const float TWO_PI_DEGREE = Mathf.PI * Mathf.Rad2Deg * 2.0f;     // 360.0f
	public const float TWO_PI_RADIAN = Mathf.PI * 2.0f;                     // 6.28f
	public const float HALF_PI_DEGREE = Mathf.PI * Mathf.Rad2Deg * 0.5f;    // 90.0f
	public const float HALF_PI_RADIAN = Mathf.PI * 0.5f;                    // 1.57f
	public const float PI_DEGREE = Mathf.PI * Mathf.Rad2Deg;                // 180.0f
	public const float PI_RADIAN = Mathf.PI;                                // 3.14f
	public static new void initUtility()
	{
		initFFTParam();
		initGreaterPow2();
	}
	public static bool hasMask(int value, int mask)
	{
		return (value & ~mask) != 0;
	}
	public static float KMHtoMS(float kmh) { return kmh * 0.27777f; }       // km/h转m/s
	public static float MStoKMH(float ms) { return ms * 3.6f; }
	public static float MtoKM(float m) { return m * 0.001f; }
	public static float pow(float value, float power)
	{
		return Mathf.Pow(value, power);
	}
	public static float pow(float value, int power)
	{
		float finalValue = 1.0f;
		for (int i = 0; i < power; ++i)
		{
			finalValue *= value;
		}
		return finalValue;
	}
	public static float pow10(int power) { return pow(10, power); }
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
		if (value < mGreaterPow2.Length)
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
		UnityUtility.logError("无法获取大于指定数的第一个2的n次方的数:" + value);
		return 0;
	}
	// 得到数轴上浮点数右边的第一个整数,向上取整
	public static int ceil(float value)
	{
		int intValue = (int)(value);
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
		int intValue = (int)(value);
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	public static int floor(double value)
	{
		int intValue = (int)(value);
		if (value < 0.0f && value < intValue)
		{
			--intValue;
		}
		return intValue;
	}
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
	// value1大于等于value0则返回1,否则返回0
	public static int step(float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	public static float fmod(float value0, float value1) { return value0 - value1 * (int)(value0 / value1); }
	// 返回value的小数部分
	public static float frac(float value) { return value - (int)value; }
	public static float abs(float value) { return value >= 0.0f ? value : -value; }
	public static int abs(int value) { return value >= 0 ? value : -value; }
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
	public static float dot(ref Vector3 v0, ref Vector3 v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	public static float dot(Vector3 v0, Vector3 v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	public static float sqrt(float value) { return Mathf.Sqrt(value); }
	public static Vector3 cross(ref Vector3 v0, ref Vector3 v1) { return Vector3.Cross(v0, v1); }
	public static Vector3 cross(Vector3 v0, Vector3 v1) { return Vector3.Cross(v0, v1); }
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
	// value是否是2的n次方
	public static bool isPow2(int value)
	{
		return (value & (value - 1)) == 0;
	}
	// 是否为偶数
	// 对于a % b的计算,如果b为2的n次方,则a % b等效于a & (b - 1)
	public static bool isEven(int value) { return (value & 1) == 0; }
	public static float getNearest(float value, float p0, float p1)
	{
		return abs(value - p0) < abs(value - p1) ? p0 : p1;
	}
	public static float getFarthest(float value, float p0, float p1)
	{
		return abs(value - p0) > abs(value - p1) ? p0 : p1;
	}
	public static float calculateFloat(string str)
	{
		// 判断字符串是否含有非法字符,也就是除数字,小数点,运算符以外的字符
		checkFloatString(str, "+-*/()");
		// 判断左右括号数量是否相等
		int leftBracketCount = 0;
		int rightBracketCount = 0;
		int newStrLen = str.Length;
		for (int i = 0; i < newStrLen; ++i)
		{
			if (str[i] == '(')
			{
				++leftBracketCount;
			}
			else if (str[i] == ')')
			{
				++rightBracketCount;
			}
		}
		if (leftBracketCount != rightBracketCount)
		{
			// 计算错误,左右括号数量不对应
			return 0;
		}

		// 循环判断传入的字符串有没有括号
		while (true)
		{
			// 先判断有没有括号，如果有括号就先算括号里的,如果没有就退出while循环
			if (str.IndexOf("(") != -1 || str.IndexOf(")") != -1)
			{
				int curpos = str.LastIndexOf("(");
				string strInBracket = str.Substring(curpos + 1, str.Length - curpos - 1);
				strInBracket = strInBracket.Substring(0, strInBracket.IndexOf(")"));
				float ret = calculateFloat(strInBracket);
				// 如果括号中的计算结果是负数,则标记为负数
				bool isMinus = false;
				if (ret < 0)
				{
					ret = -ret;
					isMinus = true;
				}
				// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
				string floatStr = (Math.Round(ret, 4)).ToString();
				str = replace(str, curpos, curpos + strInBracket.Length + 2, floatStr);
				byte[] strchar = stringToBytes(str);
				if (isMinus)
				{
					// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
					bool changeMark = false;
					for (int i = curpos - 1; i >= 0; --i)
					{
						// 找到第一个+号,则直接改为减号,然后退出遍历
						if (strchar[i] == '+')
						{
							strchar[i] = (byte)'-';
							str = bytesToString(strchar);
							changeMark = true;
							break;
						}
						// 找到第一个减号,如果减号的左边有数字,则直接改为+号
						// 如果减号的左边不是数字,则该减号是负号,将减号去掉,
						else if (strchar[i] == '-')
						{
							if (strchar[i - 1] >= '0' && strchar[i - 1] <= '9')
							{
								strchar[i] = (byte)'+';
								str = bytesToString(strchar);
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
			else
			{
				break;
			}
		}
		List<float> numbers = new List<float>();
		List<char> factors = new List<char>();
		// 表示上一个运算符的下标+1
		int beginpos = 0;
		for (int i = 0; i < str.Length; ++i)
		{
			// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
			if (i == str.Length - 1)
			{
				numbers.Add(SToF(str.Substring(beginpos, str.Length - beginpos)));
				break;
			}
			// 找到第一个运算符
			if ((str[i] < '0' || str[i] > '9') && str[i] != '.')
			{
				if (i != 0)
				{
					numbers.Add(SToF(str.Substring(beginpos, i - beginpos)));
				}
				// 如果在表达式的开始就发现了运算符,则表示第一个数是负数,那就处理为0减去这个数的绝对值
				else
				{
					numbers.Add(0);
				}
				factors.Add(str[i]);
				beginpos = i + 1;
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
						num3 = num1 / num2;
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
			// 计算错误
			return 0;
		}
		else
		{
			return numbers[0];
		}
	}
	// precision表示小数点后保留几位小数
	public static void checkFloat(ref float value, int precision = 4)
	{
		float helper = pow10(precision);
		int newValue = floor(value * helper + 0.5f);
		value = newValue / helper;
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
	// 一定几率返回true,几率范围是0.0~1.0
	public static bool randomHit(float odds)
	{
		// 几率已经大于等于1,则直接返回true
		if (odds >= 1.0f)
		{
			return true;
		}
		// 几率小于等于0,则直接返回fals
		if (odds <= 0.0f)
		{
			return false;
		}
		return randomFloat(0.0f, 1.0f) < odds;
	}
	// 根据几率随机选择一个下标
	public static int randomHit(List<ushort> oddsList)
	{
		if (oddsList == null || oddsList.Count == 0)
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
	// 根据几率随机选择一个下标
	public static int randomHit(List<int> oddsList)
	{
		if (oddsList == null || oddsList.Count == 0)
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
	// 根据几率随机选择一个下标
	public static int randomHit(List<float> oddsList)
	{
		if (oddsList == null || oddsList.Count == 0)
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
	public static float randomFloat(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}
	public static int randomInt(int min, int max)
	{
		if (min == max)
		{
			return min;
		}
		return UnityEngine.Random.Range(min, max + 1);
	}
	// 计算点p在平面on上的投影点,o为平面上一点,n为平面法线
	public static Vector3 getProjectionOnPlane(Vector3 o, Vector3 n, Vector3 p)
	{
		// o在np上的投影点即为交点
		return getProjectPoint(o, new Line3(p, p + n));
	}
	// 该算法是网上找的,计算结果与上面的函数一致
	//public static Vector3 getProjectionOnPlane(Vector3 o, Vector3 n, Vector3 p)
	//{
	//	Vector3 projection = Vector3.zero;
	//	projection.x = (n.x * n.y * o.y + n.y * n.y * p.x - n.x * n.y * p.y + n.x * n.z * o.z + n.z * n.z * p.x - n.x * n.z* p.z + n.x * n.x * o.x) / (n.x * n.x + n.y * n.y + n.z * n.z);
	//	projection.y = (n.y * n.z * o.z + n.z * n.z * p.y - n.y * n.z * p.z + n.y * n.x * o.x + n.x * n.x * p.y - n.x * n.y* p.x + n.y * n.y * o.y) / (n.x * n.x + n.y * n.y + n.z * n.z);
	//	projection.z = (n.x * o.x * n.z + n.x * n.x * p.z - n.x * p.x * n.z + n.y * o.y * n.z + n.y * n.y * p.z - n.y * p.y* n.z + n.z * n.z * o.z) / (n.x * n.x + n.y * n.y + n.z * n.z);
	//	return projection;
	//}
	// 判断两个点是否在线同一边
	public static bool isSameSidePoint(Line2 line, Vector2 point0, Vector2 point1)
	{
		int angle0 = getAngleSignFromVectorToVector2(point0 - line.mStart, line.mEnd - line.mStart);
		int angle1 = getAngleSignFromVectorToVector2(point1 - line.mStart, line.mEnd - line.mStart);
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
		v0 = normalize(v0);
		v1 = normalize(v1);
		//两个向量方向相反则为在线段上,同向则交点不在线段上
		return isVectorZero(v0 + v1);
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
			if (!intersectLineLineSection(line, new Line2(point0, point1), out intersect))
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
		Vector3 v1 = Vector3.up;
		Vector3 v2 = cross(ref v0, ref v1);
		otherPoint = point + new Vector2(v2.x, v2.z);
	}
	// 计算一条直线经过point点的平行线
	public static void generateParallel(Line3 line, Vector3 point, out Vector3 otherPoint)
	{
		Vector3 dir = line.mEnd - line.mStart;
		otherPoint = point + dir;
	}
	public static void generateParallel(Line2 line, Vector2 point, out Vector2 otherPoint)
	{
		Vector2 dir = line.mEnd - line.mStart;
		otherPoint = point + dir;
	}
	// 第index0个点和第index1个点的连线是否位于多边形内部,并且不与任何边相交
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
		float angle0 = getAngleFromVector2ToVector2(lastDir, nextDir);
		adjustRadian360(ref angle0);
		float angle1 = getAngleFromVector2ToVector2(vertice[index1] - curPoint, nextDir);
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
			Line2 line0 = new Line2(vertice[i], vertice[(i + 1) % verticeCount]);
			Line2 line1 = new Line2(vertice[index0], vertice[index1]);
			if (intersectLineSection(line0, line1, out _, false))
			{
				return false;
			}
		}
		return true;
	}
	// 计算两条线段的交点,返回值表示两条线段是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersectLineSection(Line2 line0, Line2 line1, out Vector2 intersect, bool checkEndPoint = false)
	{
		// 有端点重合
		if ((isVectorEqual(line0.mStart, line1.mStart) || isVectorEqual(line0.mStart, line1.mEnd)
			|| isVectorEqual(line0.mEnd, line1.mStart) || isVectorEqual(line0.mEnd, line1.mEnd)))
		{
			// 考虑端点时认为两条线段相交
			// 不考虑端点时,则两条线段不相交
			intersect = Vector2.zero;
			return checkEndPoint;
		}
		if (intersectLine2(line0, line1, out intersect))
		{
			// 如果交点都在两条线段内,则两条线段相交
			return inRange(intersect, line0.mStart, line0.mEnd) && inRange(intersect, line1.mStart, line1.mEnd);
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
	public static bool intersectLineIgnoreY(Line3 line0, Line3 line1, out Vector3 intersect)
	{
		bool ret = intersectLine2(line0.toLine2IgnoreY(), line1.toLine2IgnoreY(), out Vector2 point);
		intersect = new Vector3(point.x, 0.0f, point.y);
		return ret;
	}
	public static bool intersectLineIgnoreX(Line3 line0, Line3 line1, out Vector3 intersect)
	{
		bool ret = intersectLine2(line0.toLine2IgnoreX(), line1.toLine2IgnoreX(), out Vector2 point);
		intersect = new Vector3(0.0f, point.y, point.x);
		return ret;
	}
	// 计算两条直线的交点,返回值表示两条直线是否相交
	public static bool intersectLine2(Line2 line0, Line2 line1, out Vector2 intersect)
	{
		// 计算两条线的k和b
		float k0 = 0.0f;
		float b0 = 0.0f;
		bool hasK0 = generateLineExpression(line0, ref k0, ref b0);
		float k1 = 0.0f;
		float b1 = 0.0f;
		bool hasK1 = generateLineExpression(line1, ref k1, ref b1);
		// 两条竖直的线没有交点,即使两条竖直的线重合也不计算交点
		if (!hasK0 && !hasK1)
		{
			intersect = Vector2.zero;
			return false;
		}
		// 直线0为竖直的线
		else if (!hasK0)
		{
			intersect.x = line0.mStart.x;
			intersect.y = k1 * intersect.x + b1;
			return true;
		}
		// 直线1为竖直的线
		else if (!hasK1)
		{
			intersect.x = line1.mStart.x;
			intersect.y = k0 * intersect.x + b0;
			return true;
		}
		else
		{
			// 两条不重合且不平行的两条线才计算交点
			if (!isFloatEqual(k0, k1))
			{
				intersect.x = (b1 - b0) / (k0 - k1);
				intersect.y = k0 * intersect.x + b0;
				return true;
			}
		}
		intersect = Vector2.zero;
		return false;
	}
	// k为斜率,也就是cotan(直线与y轴的夹角)
	public static bool generateLineExpression(Line2 line, ref float k, ref float b)
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
			return false;
		}
		else
		{
			k = (line.mStart.y - line.mEnd.y) / (line.mStart.x - line.mEnd.x);
			b = line.mStart.y - k * line.mStart.x;
		}
		return true;
	}
	// 当忽略端点重合,如果有端点重合，则判断为不相交
	// 线段与矩形是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	public static bool intersect(Line2 line, Rect rect, bool checkEndPoint = false)
	{
		float dis = getDistanceToLine(rect.center, line);
		// 距离大于对角线的一半,则不与矩形相交
		if(dis > getLength(rect.max - rect.min) * 0.5f)
		{
			return false;
		}
		// 直线是否与任何一条边相交
		Vector2 leftTop = rect.min + new Vector2(0.0f, rect.height);
		Vector2 rightTop = rect.max;
		Vector2 rightBottom = rect.min + new Vector2(rect.width, 0.0f);
		Vector2 leftBottom = rect.min;
		if(intersectLineSection(line, new Line2(leftTop, rightTop), out _, checkEndPoint))
		{
			return true;
		}
		if (intersectLineSection(line, new Line2(leftBottom, rightBottom), out _, checkEndPoint))
		{
			return true;
		}
		if (intersectLineSection(line, new Line2(leftBottom, leftTop), out _, checkEndPoint))
		{
			return true;
		}
		if (intersectLineSection(line, new Line2(rightBottom, rightTop), out _, checkEndPoint))
		{
			return true;
		}
		return false;
	}
	// 计算线段与三角形是否相交
	public static bool intersectLineTriangle(Line2 line, Triangle2 triangle, out TriangleIntersectResult intersectResult, bool checkEndPoint = false)
	{
		// 对三条边都要检测,计算出最近的一个交点
		bool result0 = intersectLineSection(line, new Line2(triangle.mPoint0, triangle.mPoint1), out Vector2 intersect0, checkEndPoint);
		bool result1 = intersectLineSection(line, new Line2(triangle.mPoint1, triangle.mPoint2), out Vector2 intersect1, checkEndPoint);
		bool result2 = intersectLineSection(line, new Line2(triangle.mPoint2, triangle.mPoint0), out Vector2 intersect2, checkEndPoint);
		Vector2 point = Vector2.zero;
		Vector2 linePoint0 = Vector2.zero;
		Vector2 linePoint1 = Vector2.zero;
		float closestDistance = 99999.0f * 99999.0f;
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
		intersectResult = new TriangleIntersectResult();
		intersectResult.mIntersectPoint = point;
		intersectResult.mLinePoint0 = linePoint0;
		intersectResult.mLinePoint1 = linePoint1;
		return result0 || result1 || result2;
	}
	// 计算线段与三角形是否相交
	public static bool intersectLineTriangle(Line2 line, Triangle2 triangle, out Vector2 intersectPoint)
	{
		float t = 0.0f;
		float u = 0.0f;
		float v = 0.0f;
		Vector2 lineDir = normalize(line.mEnd - line.mStart);
		bool ret = intersectRayTriangle(line.mStart, lineDir, triangle.mPoint0, triangle.mPoint1, triangle.mPoint2, ref t, ref u, ref v);
		if(ret)
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
	// Determine whether a ray intersect with a triangle
	// Parameters
	// orig: origin of the ray
	// dir: direction of the ray
	// v0, v1, v2: vertices of triangle
	// t(out): weight of the intersection for the ray
	// u(out), v(out): barycentric coordinate of intersection
	public static bool intersectRayTriangle(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2, ref float t, ref float u, ref float v)
	{
		Vector3 E1 = v1 - v0;
		Vector3 E2 = v2 - v0;
		Vector3 P = cross(ref dir, ref E2);
		float determinant = dot(ref E1, ref P);
		// keep det > 0, modify T accordingly
		Vector3 T;
		if (determinant > 0)
		{
			T = orig - v0;
		}
		else
		{
			T = v0 - orig;
			determinant = -determinant;
		}
		// If determinant is near zero, ray lies in plane of triangle
		if (determinant < 0.0001f)
		{
			return false;
		}
		// Calculate u and make sure u <= 1
		u = dot(ref T, ref P);
		if (u < 0.0f || u > determinant)
		{
			return false;
		}
		Vector3 Q = cross(ref T, ref E1);
		// Calculate v and make sure u + v <= 1
		v = dot(ref dir, ref Q);
		if(v < 0.0f || u + v > determinant)
		{
			return false;
		}
		// Calculate t, scale parameters, ray intersects triangle
		t = dot(ref E2, ref Q);
		float fInvDet = 1.0f / determinant;
		t *= fInvDet;
		u *= fInvDet;
		v *= fInvDet;
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
		return lengthLess(ref centerToRightTop, circle.mRadius);
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
			if (circleIntersectLine(circle, new Line3(polygon[0], polygon[1])))
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
		for(int i = 0; i < polygonPointCount - 1; ++i)
		{
			int side = getAngleSignFromVectorToVector2(point - polygon[i], polygon[i + i] - polygon[i]);
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
	public static void adjustToNearAxis(ref Vector3 dir, bool ignoreY = false)
	{
		if (ignoreY)
		{
			dir.y = 0.0f;
		}
		float maxValue = getMax(getMax(abs(dir.x), abs(dir.y)), abs(dir.z));
		if (isFloatEqual(maxValue, abs(dir.x)))
		{
			dir = new Vector3(sign(dir.x), 0.0f, 0.0f);
		}
		else if (isFloatEqual(maxValue, abs(dir.y)))
		{
			dir = new Vector3(0.0f, sign(dir.y), 0.0f);
		}
		else if (isFloatEqual(maxValue, abs(dir.z)))
		{
			dir = new Vector3(0.0f, 0.0f, sign(dir.z));
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
	public static Quaternion getLookRotation(Vector3 forward, bool ignoreY = false)
	{
		if (ignoreY)
		{
			forward.y = 0.0f;
		}
		return Quaternion.LookRotation(forward);
	}
	public static Vector3 getDirectionFromDegreeYawPitch(float yaw, float pitch)
	{
		yaw = toRadian(yaw);
		pitch = toRadian(pitch);
		return getDirectionFromRadianYawPitch(yaw, pitch);
	}
	public static Vector3 getDirectionFromRadianYawPitch(float yaw, float pitch)
	{
		// 如果pitch为90°或者-90°,则直接返回向量,此时无论航向角为多少,向量都是竖直向下或者竖直向上
		if (isFloatZero(pitch - HALF_PI_RADIAN))
		{
			return Vector3.down;
		}
		else if (isFloatZero(pitch + HALF_PI_RADIAN))
		{
			return Vector3.up;
		}
		else
		{
			// 在unity的坐标系中航向角需要取反
			yaw = -yaw;
			Vector3 dir = new Vector3();
			dir.z = cos(yaw);
			dir.x = -sin(yaw);
			dir.y = -tan(pitch);
			dir = normalize(dir);
			return dir;
		}
	}
	public static float getVectorYaw(Vector3 vec)
	{
		vec = normalize(vec);
		float fYaw;
		// 计算航向角,航向角是向量与在X-Z平面上的投影与Z轴正方向的夹角,从上往下看是顺时针为正,逆时针为负
		Vector3 projectionXZ = new Vector3(vec.x, 0.0f, vec.z);
		float len = getLength(ref projectionXZ);
		// 如果投影的长度为0,则表示俯仰角为90°或者-90°,航向角为0
		if (isFloatZero(len))
		{
			fYaw = 0.0f;
		}
		else
		{
			projectionXZ = normalize(projectionXZ);
			fYaw = acos(Vector3.Dot(projectionXZ, Vector3.forward));
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
	// 计算向量的俯仰角,朝上时俯仰角小于0,朝下时俯仰角大于0
	public static float getVectorPitch(Vector3 vec)
	{
		vec = normalize(vec);
		return -asin(vec.y);
	}
	// 顺时针旋转为正,逆时针为负
	public static float getAngleFromVector2ToVector2(Vector2 from, Vector2 to, ANGLE radian = ANGLE.RADIAN)
	{
		if(isVectorEqual(from, to))
		{
			return 0.0f;
		}
		Vector3 from3 = normalize(new Vector3(from.x, 0.0f, from.y));
		Vector3 to3 = normalize(new Vector3(to.x, 0.0f, to.y));
		float angle = getAngleBetweenVector(from3, to3);
		Vector3 crossVec = cross(ref from3, ref to3);
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
	public static int getAngleSignFromVectorToVector3IgnoreY(Vector3 from, Vector3 to)
	{
		return getAngleSignFromVectorToVector2(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
	}
	public static int getAngleSignFromVectorToVector3IgnoreX(Vector3 from, Vector3 to)
	{
		return getAngleSignFromVectorToVector2(new Vector2(from.z, from.y), new Vector2(to.z, to.y));
	}
	public static int getAngleSignFromVectorToVector3IgnoreZ(Vector3 from, Vector3 to)
	{
		return getAngleSignFromVectorToVector2(new Vector2(from.x, from.y), new Vector2(to.x, to.y));
	}
	// 判断两个向量从from到to的角度的符号,同向或反向是为0
	public static int getAngleSignFromVectorToVector2(Vector2 from, Vector2 to)
	{
		Vector3 from3 = normalize(new Vector3(from.x, 0.0f, from.y));
		Vector3 to3 = normalize(new Vector3(to.x, 0.0f, to.y));
		// 两个向量同向或者反向是角度为0,否则角度不为0
		int angle = isVectorEqual(ref from3, ref to3) || isVectorZero(from3 + to3) ? 0 : 1;
		if (angle != 0)
		{
			Vector3 crossVec = cross(ref from3, ref to3);
			if (crossVec.y < 0.0f)
			{
				angle = -angle;
			}
		}
		return sign(angle);
	}
	// baseY为true表示将点当成X-Z平面上的点,忽略Y值,false表示将点当成X-Y平面的点
	public static float getAngleFromVector3ToVector3(Vector3 from, Vector3 to, bool baseY, ANGLE radian = ANGLE.RADIAN)
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
		Vector3 crossVec = cross(ref from, ref to);
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
	public static Vector3 getDegreeEulerFromDirection(Vector3 dir)
	{
		float yaw = 0.0f;
		float pitch = 0.0f;
		getDegreeYawPitchFromDirection(dir, ref yaw, ref pitch);
		return new Vector3(pitch, yaw, 0.0f);
	}
	public static void getDegreeYawPitchFromDirection(Vector3 dir, ref float fYaw, ref float fPitch)
	{
		getRadianYawPitchFromDirection(dir, ref fYaw, ref fPitch);
		fYaw = toDegree(fYaw);
		fPitch = toDegree(fPitch);
	}
	// fYaw是-PI到PI之间
	public static void getRadianYawPitchFromDirection(Vector3 dir, ref float fYaw, ref float fPitch)
	{
		dir = normalize(dir);
		// 首先计算俯仰角,俯仰角是向量与X-Z平面的夹角,在上面为负,在下面为正
		fPitch = getVectorPitch(dir);
		fYaw = getVectorYaw(dir);
	}
	// 给定一段圆弧,以及圆弧圆心角的百分比,计算对应的圆弧上的一个点以及该点的切线方向
	public static void getPosOnArc(Vector3 circleCenter, Vector3 startArcPos, Vector3 endArcPos, float anglePercent, ref Vector3 pos, ref Vector3 tangencyDir)
	{
		float radius = getLength(startArcPos - circleCenter);
		Vector3 relativeStart = startArcPos - circleCenter;
		Vector3 relativeEnd = endArcPos - circleCenter;
		saturate(ref anglePercent);
		// 首先判断从起始半径线段到终止半径线段的角度的正负
		float angleBetween = getAngleFromVector2ToVector2(new Vector2(relativeStart.x, relativeStart.z), new Vector2(relativeEnd.x, relativeEnd.z));
		if (isFloatZero(angleBetween))
		{
			pos = normalize(relativeStart) * radius;
			tangencyDir = normalize(rotateVector3(-pos, HALF_PI_RADIAN));
		}
		// 根据夹角的正负,判断应该顺时针还是逆时针旋转起始半径线段
		else
		{
			pos = normalize(rotateVector3(relativeStart, anglePercent * angleBetween)) * radius;
			// 计算切线,如果顺时针计算出的切线与从起始点到终止点所成的角度大于90度,则使切线反向
			tangencyDir = normalize(rotateVector3(-pos, HALF_PI_RADIAN));
			Vector3 posToEnd = relativeEnd - pos;
			if (abs(getAngleFromVector2ToVector2(new Vector2(tangencyDir.x, tangencyDir.z), new Vector2(posToEnd.x, posToEnd.z))) > HALF_PI_RADIAN)
			{
				tangencyDir = -tangencyDir;
			}
		}
		pos += circleCenter;
	}
	// 根据入射角和法线得到反射角
	public static Vector3 getReflection(Vector3 inRay, Vector3 normal)
	{
		inRay = normalize(inRay);
		return inRay - 2 * getProjection(inRay, normalize(normal));
	}
	public static Vector3 clampLength(Vector3 vec, float maxLength)
	{
		if(lengthGreater(vec, maxLength))
		{
			return normalize(vec) * maxLength;
		}
		return vec;
	}
	public static void clampLength(ref Vector3 vec, float maxLength)
	{
		if (lengthGreater(vec, maxLength))
		{
			vec = normalize(vec) * maxLength;
		}
	}
	public static Vector3 resetX(Vector3 v) { return new Vector3(0.0f, v.y, v.z); }
	public static Vector3 resetY(Vector3 v) { return new Vector3(v.x, 0.0f, v.z); }
	public static Vector3 resetZ(Vector3 v) { return new Vector3(v.x, v.y, 0.0f); }
	public static Vector3 replaceX(Vector3 v, float x) { return new Vector3(x, v.y, v.z); }
	public static Vector3 replaceY(Vector3 v, float y) { return new Vector3(v.x, y, v.z); }
	public static Vector3 replaceZ(Vector3 v, float z) { return new Vector3(v.x, v.y, z); }
	// vec0的3个分量是否都小于vec1的3个分量
	public static bool isVector3Less(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y && vec0.z < vec1.z; }
	// vec0的3个分量是否都小于vec1的3个分量
	public static bool isVector3Less(Vector3 vec0, Vector3 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y && vec0.z < vec1.z; }
	// vec0的3个分量是否都大于vec1的3个分量
	public static bool isVector3Greater(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y && vec0.z > vec1.z; }
	// vec0的3个分量是否都大于vec1的3个分量
	public static bool isVector3Greater(Vector3 vec0, Vector3 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y && vec0.z > vec1.z; }
	public static bool isVectorEqual(ref Vector2 vec0, ref Vector2 vec1, float precision = 0.0001f) { return isFloatZero(vec0.x - vec1.x, precision) && isFloatZero(vec0.y - vec1.y, precision); }
	public static bool isVectorEqual(Vector2 vec0, Vector2 vec1, float precision = 0.0001f) { return isFloatZero(vec0.x - vec1.x, precision) && isFloatZero(vec0.y - vec1.y, precision); }
	public static bool isVectorEqual(ref Vector3 vec0, ref Vector3 vec1, float precision = 0.0001f) { return isFloatZero(vec0.x - vec1.x, precision) && isFloatZero(vec0.y - vec1.y, precision) && isFloatZero(vec0.z - vec1.z, precision); }
	public static bool isVectorEqual(Vector3 vec0, Vector3 vec1, float precision = 0.0001f) { return isFloatZero(vec0.x - vec1.x, precision) && isFloatZero(vec0.y - vec1.y, precision) && isFloatZero(vec0.z - vec1.z, precision); }
	public static bool isVectorZero(ref Vector2 vec, float precision = 0.0001f) { return isFloatZero(vec.x, precision) && isFloatZero(vec.y, precision); }
	public static bool isVectorZero(Vector2 vec, float precision = 0.0001f) { return isFloatZero(vec.x, precision) && isFloatZero(vec.y, precision); }
	public static bool isVectorZero(ref Vector3 vec, float precision = 0.0001f) { return isFloatZero(vec.x, precision) && isFloatZero(vec.y, precision) && isFloatZero(vec.z, precision); }
	public static bool isVectorZero(Vector3 vec, float precision = 0.0001f) { return isFloatZero(vec.x, precision) && isFloatZero(vec.y, precision) && isFloatZero(vec.z, precision); }
	public static float getLength(ref Vector4 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w); }
	public static float getLength(Vector4 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w); }
	public static float getLength(ref Vector3 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	public static float getLength(Vector3 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	public static float getLength(ref Vector2 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	public static float getLength(Vector2 vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	public static float getSquaredLength(ref Vector4 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w; }
	public static float getSquaredLength(Vector4 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w; }
	public static float getSquaredLength(ref Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static float getSquaredLength(Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static float getSquaredLength(ref Vector2 vec) { return vec.x * vec.x + vec.y * vec.y; }
	public static float getSquaredLength(Vector2 vec) { return vec.x * vec.x + vec.y * vec.y; }
	public static bool lengthLess(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y < vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLess(ref Vector2 vec0, ref Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y < vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLess(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y < length * length; }
	public static bool lengthLess(ref Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y < length * length; }
	public static bool lengthLess(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z < vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLess(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z < vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLess(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthLess(ref Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthLessEqual(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y <= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLessEqual(ref Vector2 vec0, ref Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y <= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLessEqual(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y <= length * length; }
	public static bool lengthLessEqual(ref Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y <= length * length; }
	public static bool lengthLessEqual(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z <= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLessEqual(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z <= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLessEqual(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z <= length * length; }
	public static bool lengthLessEqual(ref Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z <= length * length; }
	public static bool lengthGreater(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static bool lengthGreater(ref Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static bool lengthGreater(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y > vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreater(ref Vector2 vec0, ref Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y > vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreater(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	public static bool lengthGreater(ref Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	public static bool lengthGreater(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z > vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreater(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z > vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqual(Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y >= length * length; }
	public static bool lengthGreaterEqual(ref Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y >= length * length; }
	public static bool lengthGreaterEqual(Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y >= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreaterEqual(ref Vector2 vec0, ref Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y >= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreaterEqual(Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z >= length * length; }
	public static bool lengthGreaterEqual(ref Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z >= length * length; }
	public static bool lengthGreaterEqual(Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z >= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqual(ref Vector3 vec0, ref Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z >= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool isQuaternionEqual(Quaternion value0, Quaternion value1)
	{
		return isFloatEqual(value0.x, value1.x) && 
			   isFloatEqual(value0.y, value1.y) && 
			   isFloatEqual(value0.z, value1.z) && 
			   isFloatEqual(value0.w, value1.w);
	}
	public static Vector3 getMatrixScale(Matrix4x4 mat)
	{
		Vector3 vec0 = new Vector3(mat.m00, mat.m01, mat.m02);
		Vector3 vec1 = new Vector3(mat.m10, mat.m11, mat.m12);
		Vector3 vec2 = new Vector3(mat.m20, mat.m21, mat.m22);
		return new Vector3(getLength(ref vec0), getLength(ref vec1), getLength(ref vec2));
	}
	// 将矩阵的缩放设置为1,并且不改变位移和旋转
	public static Matrix4x4 identityMatrix4(ref Matrix4x4 rot)
	{
		Vector3 vec0 = new Vector3(rot.m00, rot.m01, rot.m02);
		Vector3 vec1 = new Vector3(rot.m10, rot.m11, rot.m12);
		Vector3 vec2 = new Vector3(rot.m20, rot.m21, rot.m22);
		vec0 = normalize(vec0);
		vec1 = normalize(vec1);
		vec2 = normalize(vec2);
		Matrix4x4 temp = new Matrix4x4();
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
	public static Vector3 matrixToEulerAngle(ref Matrix4x4 rot)
	{
		Matrix4x4 tempMat4 = identityMatrix4(ref rot);
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
				intersectLineVector = new Vector3(1.0f, 0.0f, 0.0f);
			}
			// 矩阵中Z轴的z分量为0,则交线在世界坐标系的Z轴上,
			else if (!isFloatZero(tempMat4.m20) && isFloatZero(tempMat4.m22))
			{
				// Z轴朝向世界坐标系的X轴正方向,即Z轴的x分量大于0,应该计算X轴与世界坐标系的Z轴负方向的夹角
				if (tempMat4.m20 > 0.0f)
				{
					intersectLineVector = new Vector3(0.0f, 0.0f, -1.0f);
				}
				// Z轴朝向世界坐标系的X轴负方向,应该计算X轴与世界坐标系的Z轴正方向的夹角
				else
				{
					intersectLineVector = new Vector3(0.0f, 0.0f, 1.0f);
				}
			}
			// 矩阵中Z轴的x和z分量都不为0,取X轴正方向上的一个点
			else
			{
				intersectLineVector = new Vector3(1.0f, 0.0f, -tempMat4.m20 / tempMat4.m22);
			}
			// 然后求出矩阵中X轴与交线的夹角
			angleRoll = getAngleBetweenVector(intersectLineVector, new Vector3(tempMat4.m00, tempMat4.m01, tempMat4.m02));
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
		Vector3 zAxisInMatrix = new Vector3(nonRollMat.m20, nonRollMat.m21, nonRollMat.m22);
		float anglePitch;
		if (!isFloatZero(zAxisInMatrix.x) || !isFloatZero(zAxisInMatrix.z))
		{
			anglePitch = getAngleBetweenVector(zAxisInMatrix, new Vector3(zAxisInMatrix.x, 0.0f, zAxisInMatrix.z));
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
		return new Vector3(angleYaw, anglePitch, angleRoll);
	}
	public static float getAngleBetweenVector(Vector3 vec1, Vector3 vec2)
	{
		Vector3 curVec1 = normalize(vec1);
		Vector3 curVec2 = normalize(vec2);
		return acos(dot(ref curVec1, ref curVec2));
	}
	public static float getAngleBetweenVector(Vector2 vec1, Vector2 vec2)
	{
		Vector3 curVec1 = normalize(vec1);
		Vector3 curVec2 = normalize(vec2);
		return acos(dot(ref curVec1, ref curVec2));
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
	public static float getDistanceToLineIgnoreY(Vector3 point, Line3 line)
	{
		point.y = 0.0f;
		return getDistanceToLine(point, line);
	}
	public static float getDistanceToLineIgnoreX(Vector3 point, Line3 line)
	{
		point.x = 0.0f;
		return getDistanceToLine(point, line);
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
	// 计算一个向量在另一个向量上的投影
	public static Vector3 getProjection(Vector3 v1, Vector3 v2)
	{
		return normalize(v2) * dot(v1, v2) / getLength(v2);
	}
	public static Vector3 getProjection(Vector2 v1, Vector2 v2)
	{
		return normalize(v2) * dot(v1, v2) / getLength(v2);
	}
	public static Vector3 normalize(Vector3 vec3)
	{
		float length = getLength(ref vec3);
		if (isFloatZero(length))
		{
			return Vector3.zero;
		}
		if (isFloatEqual(length, 1.0f))
		{
			return vec3;
		}
		float inverseLen = 1.0f / length;
		return new Vector3(vec3.x * inverseLen, vec3.y * inverseLen, vec3.z * inverseLen);
	}
	public static Vector2 normalize(Vector2 vec2)
	{
		float length = getLength(ref vec2);
		if (isFloatZero(length))
		{
			return Vector2.zero;
		}
		if (isFloatEqual(length, 1.0f))
		{
			return vec2;
		}
		float inverseLen = 1.0f / length;
		return new Vector2(vec2.x * inverseLen, vec2.y * inverseLen);
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
		Matrix4x4 rot = new Matrix4x4();
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
		Matrix4x4 rot = new Matrix4x4();
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
		Matrix4x4 rot = new Matrix4x4();
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
	public static bool isReachTarget(float lastValue, float curValue, float target, float speed)
	{
		int preSign = (int)sign(target - lastValue);
		int curSign = (int)sign(target - curValue);
		int speedSign = (int)sign(speed);
		return isFloatEqual(target, curValue) || preSign != curSign && preSign == speedSign;
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
	public static float generateFactorBFromFactorA(float factorA, Vector3 point) { return (point.y - factorA * point.x * point.x) / point.x; }
	public static float generateFactorA(float factorB, Vector3 point) { return (point.y - factorB * point.x) / (point.x * point.x); }
	public static float generateTopHeight(float factorA, float factorB) { return -factorB * factorB / (4.0f * factorA); }
	public static float generateFactorBFromHeight(float topHeight, Vector3 point, bool leftOrRight = false)
	{
		if(isFloatZero(topHeight))
		{
			return 0.0f;
		}
		float a0 = -point.x * point.x / (4.0f * topHeight);
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
		return (-b0 + sqrt(delta) * sign) / (2.0f * a0);
	}
	public static Vector3 getMinVector3(ref Vector3 a, ref Vector3 b) { return new Vector3(getMin(a.x, b.x), getMin(a.y, b.y), getMin(a.z, b.z)); }
	public static Vector3 getMaxVector3(ref Vector3 a, ref Vector3 b) { return new Vector3(getMax(a.x, b.x), getMax(a.y, b.y), getMax(a.z, b.z)); }
	public static void getMinMaxVector3(ref Vector3 a, ref Vector3 min, ref Vector3 max)
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
		if (isFloatEqual(a, b))
		{
			return 0.0f;
		}
		return (value - a) / (b - a);
	}
	public static float inverseLerp(Vector2 a, Vector2 b, Vector2 value)
	{
		float length = getLength(b - a);
		if (isFloatZero(length))
		{
			return 0.0f;
		}
		return getLength(value - a) / length;
	}
	public static float inverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		float length = getLength(b - a);
		if (isFloatZero(length))
		{
			return 0.0f;
		}
		return getLength(value - a) / length;
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
	public static void perfectRotationDelta(ref float start, ref float target)
	{
		// 先都调整到-180~180的范围
		adjustAngle180(ref start);
		adjustAngle180(ref target);
		float dirDelta = target - start;
		// 如果目标方向与当前方向的差值超过180,则转换到0~360再计算
		if (abs(dirDelta) > PI_DEGREE)
		{
			adjustAngle360(ref start);
			adjustAngle360(ref target);
		}
	}
	public static void perfectRotationDelta(ref Vector3 start, ref Vector3 target)
	{
		perfectRotationDelta(ref start.x, ref target.x);
		perfectRotationDelta(ref start.y, ref target.y);
		perfectRotationDelta(ref start.z, ref target.z);
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
	public static void clampMin(ref int value, int min = 0)
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
	public static void clampMin(ref float value, float min = 0.0f)
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
	public static void clampMax(ref int value, int max)
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
	public static bool isFloatEqual(float value1, float value2, float precision = 0.0001f)
	{
		return isFloatZero(value1 - value2, precision);
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
	// fixedRangeOrder表示是否范围是从range0到range1,如果range0大于range1,则返回false
	public static bool inRange(float value, float range0, float range1, bool fixedRangeOrder = false, float precision = 0.001f)
	{
		if (fixedRangeOrder)
		{
			return value >= range0 - precision && value <= range1 + precision;
		}
		else
		{
			return value >= getMin(range0, range1) - precision && value <= getMax(range0, range1) + precision;
		}
	}
	public static bool inRange(Vector3 value, Vector3 point0, Vector3 point1, bool ignoreY = true, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, false, precision) && 
			   inRange(value.z, point0.z, point1.z, false, precision) &&
			  (ignoreY || inRange(value.y, point0.y, point1.y, false, precision));
	}
	public static bool inRange(ref Vector3 value, ref Vector3 point0, ref Vector3 point1, bool ignoreY = true, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, false, precision) && 
			   inRange(value.z, point0.z, point1.z, false, precision) &&
			  (ignoreY || inRange(value.y, point0.y, point1.y, false, precision));
	}
	public static bool inRange(Vector2 value, Vector2 point0, Vector2 point1, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, false, precision) && 
			   inRange(value.y, point0.y, point1.y, false, precision);
	}
	public static bool inRange(ref Vector2 value, ref Vector2 point0, ref Vector2 point1, float precision = 0.001f)
	{
		return inRange(value.x, point0.x, point1.x, false, precision) && 
			   inRange(value.y, point0.y, point1.y, false, precision);
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
			keyPointList.Add(new KeyPoint(keyPosList[i], distanceFromStart, distanceFromLast));
		}
	}
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
		if (curDistance < distanceListFromStart[middleIndex].mDistanceFromStart)
		{
			return findPointIndex(distanceListFromStart, curDistance, startIndex, middleIndex);
		}
		// 当前距离比中间的距离小,则在列表后半部分查找
		else if (curDistance > distanceListFromStart[middleIndex].mDistanceFromStart)
		{
			return findPointIndex(distanceListFromStart, curDistance, middleIndex, endIndex);
		}
		// 距离刚好等于列表中间的值,则返回该下标
		else
		{
			return middleIndex;
		}
	}
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
		// 当前距离比中间的距离小,则在列表前半部分查找
		if (curDistance < distanceListFromStart[middleIndex])
		{
			return findPointIndex(distanceListFromStart, curDistance, startIndex, middleIndex);
		}
		// 当前距离比中间的距离小,则在列表后半部分查找
		else if (curDistance > distanceListFromStart[middleIndex])
		{
			return findPointIndex(distanceListFromStart, curDistance, middleIndex, endIndex);
		}
		// 距离刚好等于列表中间的值,则返回该下标
		else
		{
			return middleIndex;
		}
	}
	public static Vector2 multiVector2(Vector2 v1, Vector2 v2) { return new Vector2(v1.x * v2.x, v1.y * v2.y); }
	public static Vector2 devideVector2(Vector2 v1, Vector2 v2) { return new Vector2(v1.x / v2.x, v1.y / v2.y); }
	public static Vector3 multiVector3(Vector3 v1, Vector3 v2) { return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
	public static Vector3 devideVector3(Vector3 v1, Vector3 v2) { return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z); }
	public static void swap<T>(ref T value0, ref T value1)
	{
		T temp = value0;
		value0 = value1;
		value1 = temp;
	}
	public static void adjustRadian180(ref float radian) { clampCycle(ref radian, -PI_RADIAN, PI_RADIAN, TWO_PI_RADIAN); }
	public static void adjustRadian180(ref Vector3 radian)
	{
		adjustRadian180(ref radian.x);
		adjustRadian180(ref radian.y);
		adjustRadian180(ref radian.z);
	}
	public static void adjustAngle180(ref float degree) { clampCycle(ref degree, -PI_DEGREE, PI_DEGREE, TWO_PI_DEGREE); }
	public static void adjustAngle180(ref Vector3 degree)
	{
		adjustAngle180(ref degree.x);
		adjustAngle180(ref degree.y);
		adjustAngle180(ref degree.z);
	}
	public static void adjustRadian360(ref float radian) { clampCycle(ref radian, 0.0f, TWO_PI_RADIAN, TWO_PI_RADIAN); }
	public static void adjustRadian360(ref Vector3 radian)
	{
		adjustRadian360(ref radian.x);
		adjustRadian360(ref radian.y);
		adjustRadian360(ref radian.z);
	}
	public static void adjustAngle360(ref float degree) { clampCycle(ref degree, 0.0f, TWO_PI_DEGREE, TWO_PI_DEGREE); }
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
	public static float getAngleFromVector(Vector3 vec, ANGLE radian = ANGLE.RADIAN)
	{
		vec.y = 0.0f;
		vec = normalize(vec);
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
		Vector3 tempVec = new Vector3(vec.x, 0.0f, vec.y);
		tempVec = normalize(tempVec);
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
	public static void rotateVector3(ref Vector3 vec, ref Matrix4x4 transMat3) { vec = transMat3 * vec; }
	public static Vector3 rotateVector3(Vector3 vec, Matrix4x4 transMat3) { return transMat3 * vec; }
	// 使用一个四元数去旋转一个三维向量
	public static Vector3 rotateVector3(Vector3 vec, Quaternion transQuat) { return transQuat * vec; }
	public static Vector3 rotateVector3(Vector3 vec, ref Quaternion transQuat) { return transQuat * vec; }
	public static void rotateVector3(ref Vector3 vec, Quaternion transQuat) { vec = transQuat * vec; }
	public static void rotateVector3(ref Vector3 vec, ref Quaternion transQuat) { vec = transQuat * vec; }
	// 求向量水平顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 rotateVector3(Vector3 vec, float radian)
	{
		Vector3 temp = vec;
		temp.y = 0.0f;
		float tempLength = getLength(ref temp);
		float questAngle = getAngleFromVector(temp);
		questAngle += radian;
		adjustRadian180(ref questAngle);
		temp = getVectorFromAngle(questAngle) * tempLength;
		temp.y = vec.y;
		return temp;
	}
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 getVectorFromAngle(float radian)
	{
		adjustRadian180(ref radian);
		Vector3 temp = new Vector3();
		temp.x = -sin(radian);
		temp.y = 0.0f;
		temp.z = cos(radian);
		// 在unity坐标系中x轴需要取反
		temp.x = -temp.x;
		return temp;
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
			ndb = (int)(20.0f * Mathf.Log10((float)v / 0xFFFF));
		}
		return ndb;
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	public static void getFrequencyZone(short[] pcmData, int dataCount, short[] frequencyList)
	{
		if (dataCount > mMaxFFTCount)
		{
			UnityUtility.logError("pcm data count is too many, data count : " + dataCount + ", max count : " + mMaxFFTCount);
			return;
		}
		for (int i = 0; i < dataCount; ++i)
		{
			mComplexList[i] = new Complex(pcmData[i], 0.0f);
		}
		fft(mComplexList, dataCount);
		for (int i = 0; i < dataCount; ++i)
		{
			frequencyList[i] = (short)sqrt(mComplexList[i].mReal * mComplexList[i].mReal + mComplexList[i].mImg * mComplexList[i].mImg);
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
		int i, j, k;
		float sR, sI, uR, uI;
		Complex tempComplex = new Complex();

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
			sR = cos_tb[l];
			sI = -sin_tb[l];
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
		if(count == 0)
		{
			return;
		}
		for (int k = 0; k <= count - 1; ++k)
		{
			x[k].mImg = -x[k].mImg;
		}
		fft(x, count);
		float inverseCount = 1.0f / count;
		for (int k = 0; k <= count - 1; ++k)
		{
			x[k].mReal = x[k].mReal * inverseCount;
			x[k].mImg = -x[k].mImg * inverseCount;
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
		Vector3[] temp = new Vector3[tempCount];
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
			resultList.Add(getBezier(points, loop, i / (float)(bezierDetail - 1)));
		}
	}
	public static List<Vector3> getBezierPoints(IList<Vector3> points, bool loop, int bezierDetail = 20)
	{
		if (points.Count == 1)
		{
			return new List<Vector3>(points);
		}
		List<Vector3> bezierPoints = new List<Vector3>();
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
		FrameUtility.LIST(out List<Vector3> midPoints);
		midPoints.Capacity = middleCount;
		// 生成中点       
		for (int i = 0; i < middleCount; ++i)
		{
			midPoints.Add((originPoint[i] + originPoint[(i + 1) % originCount]) * 0.5f);
		}

		// 平移中点,计算每个顶点的两个控制点
		FrameUtility.LIST(out List<Vector3> extraPoints);
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
		FrameUtility.UN_LIST(midPoints);

		int bezierCount = loop ? originCount : originCount - 1;
		float step = 1 / (float)(detail - 1);
		// 生成4控制点，产生贝塞尔曲线
		for (int i = 0; i < bezierCount; ++i)
		{
			mTempControlPoint[0] = originPoint[i];
			mTempControlPoint[1] = extraPoints[2 * i + 1];
			mTempControlPoint[2] = extraPoints[2 * (i + 1) % extraPoints.Count];
			mTempControlPoint[3] = originPoint[(i + 1) % originCount];
			for (int j = 0; j < detail; ++j)
			{
				Vector3 point = getBezier(mTempControlPoint, false, j * step);
				// 如果与上一个点重合了,则不放入列表中
				if (!isVectorEqual(curveList[curveList.Count - 1], point))
				{
					curveList.Add(point);
				}
			}
		}
		FrameUtility.UN_LIST(extraPoints);
	}
	public static uint generateGUID()
	{
		// 获得当前时间,再获取一个随机数,组成一个尽量不会重复的ID
		TimeSpan timeForm19700101 = DateTime.Now - new DateTime(1970, 1, 1);
		uint halfIntMS = (uint)((ulong)timeForm19700101.TotalMilliseconds % 0x7FFFFFFF);
		return halfIntMS + (uint)randomInt(0, 0x7FFFFFFF);
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
				S = delta / (maxRGB + minRGB);
			}
			else
			{
				S = delta / (2.0f - maxRGB - minRGB);
			}
			float inverseDelta = 1.0f / delta;
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
		return new Vector3(H, S, L);
	}

	// 色相(H),饱和度(S),亮度(L),转换为rgb
	// HSL和RGB的范围都是0-1
	public static Vector3 HSLtoRGB(Vector3 hsl)
	{
		Vector3 rgb = Vector3.zero;
		float H = hsl.x;
		float S = hsl.y;
		float L = hsl.z;
		if (S == 0.0)                       //HSL from 0 to 1
		{
			rgb.x = L;              //RGB results from 0 to 255
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
		if (isFloatZero(speed))
		{
			return 0.0f;
		}
		return 0.0333f / speed; 
	}
	public static float intervalToSpeed(float interval) 
	{
		if (isFloatZero(interval))
		{
			return 0.0f;
		}
		return 0.0333f / interval; 
	}
	// 由于使用了静态成员变量,所以不能在多线程中调用该函数
	public static bool AStar(bool[] map, Point begin, Point end, int width, List<int> foundPath, bool useDir8)
	{
		if (begin.toIndex(width) == end.toIndex(width))
		{
			return false;
		}
		int maxCount = map.Length;
		int height = maxCount / width;
		if (mTempNodeList == null || mTempNodeList.Length < maxCount)
		{
			mTempNodeList = new AStarNode[maxCount];
		}
		int count0 = mTempNodeList.Length;
		for (int i = 0; i < count0; ++i)
		{
			mTempNodeList[i].init(i);
		}
		mTempOpenList.Clear();
		AStarNode parentNode = new AStarNode(0, 0, 0, begin.toIndex(width), -1, 0);
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
			int parentNodeY = parentNode.mIndex / width;
			if (useDir8)
			{
				mTempDirect8[0] = new Point(parentNodeX - 1, parentNodeY - 1);
				mTempDirect8[1] = new Point(parentNodeX, parentNodeY - 1);
				mTempDirect8[2] = new Point(parentNodeX + 1, parentNodeY - 1);
				mTempDirect8[3] = new Point(parentNodeX - 1, parentNodeY);
				mTempDirect8[4] = new Point(parentNodeX + 1, parentNodeY);
				mTempDirect8[5] = new Point(parentNodeX - 1, parentNodeY + 1);
				mTempDirect8[6] = new Point(parentNodeX, parentNodeY + 1);
				mTempDirect8[7] = new Point(parentNodeX + 1, parentNodeY + 1);
			}
			else
			{
				mTempDirect4[0] = new Point(parentNodeX, parentNodeY - 1);
				mTempDirect4[1] = new Point(parentNodeX - 1, parentNodeY);
				mTempDirect4[2] = new Point(parentNodeX + 1, parentNodeY);
				mTempDirect4[3] = new Point(parentNodeX, parentNodeY + 1);
			}
			Point[] dirList = useDir8 ? mTempDirect8 : mTempDirect4;
			int dirCount = dirList.Length;
			// 将父节点周围可以通过的格子放进开启列表里
			for (int i = 0; i < dirCount; ++i)
			{
				int curIndex = dirList[i].toIndex(width);
				if (dirList[i].x >= 0 && dirList[i].x < width &&
					dirList[i].y >= 0 && dirList[i].y < height &&
					map[curIndex])
				{
					AStarNode curNode = mTempNodeList[curIndex];
					if (curNode.mState == NODE_STATE.CLOSE)
					{
						continue;
					}
					// 计算格子的G1值,即从当前父格到这个格子的G值
					int G1 = (dirList[i].x == parentNodeX || dirList[i].y == parentNodeY) ? parentNode.mG + 10 : parentNode.mG + 14;
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
						//计算G值, H值，F值，并设置这些节点的父节点
						curNode.mParent = parentNode.mIndex;
						curNode.mG = G1;
						curNode.mH = (abs(dirList[i].x - end.x) + abs(dirList[i].y - end.y)) * 10;
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
			if (mTempNodeList[end.toIndex(width)].mState == NODE_STATE.OPEN)
			{
				break;
			}
		}
		foundPath.Clear();
		foundPath.Add(end.toIndex(width));
		AStarNode road = mTempNodeList[end.toIndex(width)];
		while (road.mParent != -1)
		{
			foundPath.Add(road.mParent);
			road = mTempNodeList[road.mParent];
		}
		int count = foundPath.Count;
		int halfCount = count >> 1;
		for (int i = 0; i < halfCount; ++i)
		{
			int temp = foundPath[i];
			foundPath[i] = foundPath[count - i - 1];
			foundPath[count - i - 1] = temp;
		}
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
	//------------------------------------------------------------------------------------------------------------------------------------------------------
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
	protected static void initFFTParam()
	{
		sin_tb = new float[]
		{  // 精度(PI PI/2 PI/4 PI/8 PI/16 ... PI/(2^k))
			0.000000f, 1.000000f, 0.707107f, 0.382683f, 0.195090f, 0.098017f,
			0.049068f, 0.024541f, 0.012272f, 0.006136f, 0.003068f, 0.001534f,
			0.000767f, 0.000383f, 0.000192f, 0.000096f, 0.000048f, 0.000024f,
			0.000012f, 0.000006f, 0.000003f, 0.000003f, 0.000003f, 0.000003f,
			0.000003f
		};
		cos_tb = new float[]
		{  // 精度(PI PI/2 PI/4 PI/8 PI/16 ... PI/(2^k))
			-1.000000f, 0.000000f, 0.707107f, 0.923880f, 0.980785f, 0.995185f,
			0.998795f, 0.999699f, 0.999925f, 0.999981f, 0.999995f, 0.999999f,
			1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f,
			1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f, 1.000000f,
			1.000000f
		};
		for (int i = 0; i < mComplexList.Length; ++i)
		{
			mComplexList[i] = new Complex();
		}
	}
}