#ifndef _MATH_UTILITY_H_
#define _MATH_UTILITY_H_

#include "StringUtility.h"

struct Line2
{
	Vector2 mStart;
	Vector2 mEnd;
	Line2(const Vector2& start, const Vector2& end)
	{
		mStart = start;
		mEnd = end;
	}
};

struct Line3
{
	Vector3 mStart;
	Vector3 mEnd;
	Line3(const Vector3& start, const Vector3& end)
	{
		mStart = start;
		mEnd = end;
	}
	Line2 toLine2IgnoreY()
	{
		return Line2(Vector2(mStart.x, mStart.z), Vector2(mEnd.x, mEnd.z));
	}
	Line2 toLine2IgnoreX()
	{
		return Line2(Vector2(mStart.z, mStart.y), Vector2(mEnd.z, mEnd.y));
	}
};

struct Triangle2
{
	Vector2 mPoint0;
	Vector2 mPoint1;
	Vector2 mPoint2;
	Triangle2() {}
	Triangle2(const Vector2& point0, const Vector2& point1, const Vector2& point2)
	{
		mPoint0 = point0;
		mPoint1 = point1;
		mPoint2 = point2;
	}
};

struct Rect
{
	float x;
	float y;
	float width;
	float height;
	Rect(const Vector2& min, const Vector2& size)
	{
		x = min.x;
		y = min.y;
		width = size.x;
		height = size.y;
	}
	Vector2 getSize() const { return Vector2(width, height); }
	Vector2 getMin() const { return Vector2(x, y); }
	Vector2 getMax() const { return Vector2(x + width, y + height); }
	Vector2 getCenter() const { return Vector2(x + width * 0.5f, y + height * 0.5f); }
};

struct TriangleIntersect
{
	Vector2 mIntersectPoint; // 交点
	Vector2 mLinePoint0;     // 交点所在的三角形的一条边的起点
	Vector2 mLinePoint1;     // 交点所在的三角形的一条边的终点
};

class MathUtility : public StringUtility
{
public:
	static void initMathUtility();
	static void checkInt(float& value, float precision = MIN_DELTA); // 判断传入的参数是否已经接近于整数,如果接近于整数,则设置为整数
	static bool isFloatZero(float value, float precision = MIN_DELTA) { return value >= -precision && value <= precision; }
	static bool isFloatEqual(float value1, float value2, float precision = MIN_DELTA) { return isFloatZero(value1 - value2, precision); }
	// 是否为偶数
	static bool isEven(int value) { return (value & 1) == 0; }
	// 是否为2的n次幂
	static bool isPow2(int value) { return (value & (value - 1)) == 0; }
	// 得到比value大的第一个pow的n次方的数
	static uint getGreaterPowerValue(uint value, uint pow);
	// 得到比value大的第一个2的n次方的数
	static uint getGreaterPower2(uint value);
	static uint pow2(uint pow) { return 1 << pow; }
	static uint pow10(uint pow);
	// 得到数轴上浮点数右边的第一个整数,向上取整
	static int ceil(float value)
	{
		int intValue = (int)(value);
		// 当差值极小时,认为两个值相等
		if (value >= 0.0f && value > intValue + 0.0001f)
		{
			++intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数右边的第一个整数,向上取整
	static int ceil(const double& value)
	{
		int intValue = (int)(value);
		if (value >= 0.0f && value > (double)intValue + 0.0001)
		{
			++intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	static int floor(float value)
	{
		int intValue = (int)(value);
		if (value < 0.0f && value < intValue - 0.0001f)
		{
			--intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	static int floor(const double& value)
	{
		int intValue = (int)(value);
		if (value < 0.0f && value < (double)intValue - 0.0001)
		{
			--intValue;
		}
		return intValue;
	}
	static void saturate(float& value) { clamp(value, 0.0f, 1.0f); }
	// value1大于等于value0则返回1,否则返回0
	static int step(float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	static float fmod(float value0, float value1) { return value0 - value1 * (int)(value0 / value1); }
	// 返回value的小数部分
	static float frac(float value) { return value - (int)value; }
	// 帧换算成秒
	static float frameToSecond(int frame) { return frame * 0.0333f; }
	static float getSquaredLength(const Vector2& vec) { return vec.x * vec.x + vec.y * vec.y; }
	static float getSquaredLength(const Vector3& vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	static float getLength(float x, float y) { return sqrt(x * x + y * y); }
	static float getLength(float x, float y, float z) { return sqrt(x * x + y * y + z * z); }
	static float getLength(const Vector2& vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	static float getLength(const Vector3& vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	static Vector3 normalize(const Vector3& value);
	static Vector2 normalize(const Vector2& value);
	// 此处计算结果与unity中的Vector3.Cross()一致
	static Vector3 cross(const Vector3& v0, const Vector3& v1) { return Vector3(v1.y * v0.z - v0.y * v1.z, v1.x * v0.z - v0.x * v1.z, v1.x * v0.y - v0.x * v1.y); }
	static float dot(const Vector3& v0, const Vector3& v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	static float dot(const Vector2& v0, const Vector2& v1) { return v0.x * v1.x + v0.y * v1.y; }
	static float toDegree(float radian) { return radian * Rad2Deg; }
	static float toRadian(float degree) { return degree * Deg2Rad; }
	static bool lengthLess(const Vector3& vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	static bool lengthGreater(const Vector3& vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	static bool isVectorEqual(const Vector3& vec0, const Vector3& vec1) { return isFloatEqual(vec0.x, vec1.x) && isFloatEqual(vec0.y, vec1.y) && isFloatEqual(vec0.z, vec1.z); }
	static bool isVectorEqual(const Vector2& vec0, const Vector2& vec1) { return isFloatEqual(vec0.x, vec1.x) && isFloatEqual(vec0.y, vec1.y); }
	static void ceil(Vector2& value)
	{
		value.x = (float)ceil(value.x);
		value.y = (float)ceil(value.y);
	}
	static void ceil(Vector3& value)
	{
		value.x = (float)ceil(value.x);
		value.y = (float)ceil(value.y);
		value.z = (float)ceil(value.z);
	}
	template<typename T>
	static void clamp(T& value, T minValue, T maxValue)
	{
		if (value > maxValue)
		{
			value = maxValue;
		}
		else if (value < minValue)
		{
			value = minValue;
		}
	}
	template<typename T>
	static void clampMin(T& value, T minValue = (T)0)
	{
		if (value < minValue)
		{
			value = minValue;
		}
	}
	template<typename T>
	static void clampMax(T& value, T maxValue)
	{
		if (value > maxValue)
		{
			value = maxValue;
		}
	}
	// 根据一定几率随机返回true或false,probability范围为0到1
	static bool randomHit(float probability)
	{
		if (probability <= 0.0f)
		{
			return false;
		}
		if (probability >= 1.0f)
		{
			return true;
		}
		return randomFloat(0.0f, 1.0f) < probability;
	}
	// 根据一定几率随机返回true或false,实际几率为probability除以scale
	// 一般scale应该为100,1000或者10000,以确保随机几率正常,不能超过30000
	static bool randomHit(uint probability, uint scale)
	{
		if (probability <= 0)
		{
			return false;
		}
		if (probability >= scale)
		{
			return true;
		}
		return randomInt(0, scale - 1) < probability;
	}
	// 根据几率随机选择一个下标
	static uint randomHit(const myVector<float>& oddsList);
	template<typename T>
	static T randomHit(const myMap<T, float>& oddsList, const T& defaultValue)
	{
		float max = 0.0f;
		FOREACH_CONST(iter0, oddsList)
		{
			max += iter0->second;
		}
		float random = randomFloat(0.0f, max);
		float curValue = 0.0f;
		FOREACH_CONST(iter1, oddsList)
		{
			curValue += iter1->second;
			if (random <= curValue)
			{
				return iter1->first;
			}
		}
		return defaultValue;
	}
	static uint randomHit(const float* oddsList, uint count);
	static float randomFloat(float minFloat, float maxFloat) 
	{
		if (minFloat >= maxFloat)
		{
			return minFloat;
		}
		return ((rand() % (10000 + 1)) * 0.0001f) * (maxFloat - minFloat) + minFloat; 
	}
	static uint randomInt(uint minInt, uint maxInt)
	{
		if (minInt >= maxInt)
		{
			return minInt;
		}
		return rand() % (maxInt - minInt + 1) + minInt;
	}
	// randomCount表示要随机的个数,0表示全部都随机顺序
	static void randomOrder(int* list, uint count, uint randomCount = 0);
	static void randomSelect(uint count, uint selectCount, myVector<int>& selectIndexes);
	static void randomSelect(uint count, uint selectCount, mySet<int>& selectIndexes);
	static void clampCycle(float& value, float min, float max, float cycle)
	{
		while (value < min)
		{
			value += cycle;
		}
		while (value > max)
		{
			value -= cycle;
		}
	}
	static void clampCycle(float& value, float min, float max) { clampCycle(value, min, max, max - min); }
	static void clampRadian180(float& radianAngle) { clampCycle(radianAngle, -MATH_PI, MATH_PI); }
	static void clampDegree180(float& radianAngle) { clampCycle(radianAngle, -180.0f, 180.0f); }
	static void clampRadian360(float& radianAngle) { clampCycle(radianAngle, 0.0f, MATH_PI * 2.0f); }
	static void clampDegree360(float& radianAngle) { clampCycle(radianAngle, 0.0f, 360.0f); }
	static bool inFixedRange(int value, int range0, int range1) { return value >= range0 && value <= range1; }
	static bool inFixedRange(float value, float range0, float range1) { return value >= range0 && value <= range1; }
	static bool inRange(int value, int range0, int range1) { return value >= getMin(range0, range1) && value <= getMax(range0, range1); }
	static bool inRange(float value, float range0, float range1) { return value >= getMin(range0, range1) && value <= getMax(range0, range1); }
	static bool inRange(const Vector2& value, const Vector2& range0, const Vector2& range1) { return inRange(value.x, range0.x, range1.x) && inRange(value.y, range0.y, range1.y); }
	template<typename T>
	static T getMin(T a, T b) { return a < b ? a : b; }
	template<typename T>
	static T getMax(T a, T b) { return a > b ? a : b; }
	template<typename T>
	static T getMax(T a, T b, T c) { return getMax(a, getMax(b, c)); }
	template<typename T>
	static T lerpSimple(const T& start, const T& end, float t) { return start + (end - start) * t; }
	template<typename T>
	static T lerp(const T& start, const T& end, float t, T minAbsDelta = 0.0f)
	{
		clamp(t, 0.0f, 1.0f);
		T value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		if (abs(value - end) <= minAbsDelta)
		{
			value = end;
		}
		return value;
	}
	static float inverseLerp(float a, float b, float value)
	{
		return (value - a) / (b - a);
	}
	// 此处应该替换为使用四元数进行旋转
	static void rotateVector3(Vector3& vec, float radian);
	static float getAngleBetweenVector(const Vector3& vec1, const Vector3& vec2);
	static float getAngleBetweenVector(const Vector2& vec1, const Vector2& vec2);
	static float getAngleFromVector2ToVector2(const Vector2& from, const Vector2& to, bool radian = true);
	// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	static Vector3 getVectorFromAngle(float angle);
	// 将表达式str中的keyword替换为replaceValue,然后计算str的值,返回值表示str中是否有被替换的值,str只能是算术表达式
	static bool replaceKeywordAndCalculate(string& str, const string& keyword, int replaceValue, bool floatOrInt);
	// 将表达式str中的所有\\()包含的部分中的keyword替换为keyValue,并且计算包含的表达式,返回值表示str中是否有被替换的部分,str可以是任意表达式
	static bool replaceStringKeyword(string& str, const string& keyword, int keyValue, bool floatOrInt);
	static float powerFloat(float f, int p);
	static float calculateFloat(string str);	// 以浮点数的计算法则计算一个表达式,只支持加减乘除和括号
	static int calculateInt(string str);		// 以整数的计算法则计算一个表达式,支持取余,加减乘除和括号
	// 秒数转换为分数和秒数
	static void secondsToMinutesSeconds(uint seconds, uint& outMin, uint& outSec)
	{
		outMin = seconds / 60;
		outSec = seconds % 60;
	}
	static void secondsToHoursMinutesSeconds(uint seconds, uint& outHour, uint& outMin, uint& outSec)
	{
		outHour = seconds / (60 * 60);
		outMin = (seconds % (60 * 60)) / 60;
		outSec = seconds % 60;
	}
	static float speedToInterval(float speed) { return 0.0333f / speed; }
	static float intervalToSpeed(float interval) { return 0.0333f / interval; }
	template<typename T>
	static void swap(T& value0, T& value1)
	{
		T temp = value0;
		value0 = value1;
		value1 = temp;
	}
	template<typename T>
	static T sign(T value)
	{
		if (value > (T)0)
		{
			return (T)1;
		}
		if (value < (T)0)
		{
			return (T)-1;
		}
		return (T)0;
	}
	// 四舍五入
	static int round(float value)
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
	static void round(Vector3& value)
	{
		value.x = (float)round(value.x);
		value.y = (float)round(value.y);
		value.z = (float)round(value.z);
	}
	static float HueToRGB(float v1, float v2, float vH);
	static uint findPointIndex(const myVector<float>& distanceListFromStart, float curDistance, uint startIndex, uint endIndex);
	// 计算两条直线的交点,返回值表示两条直线是否相交
	static bool intersectLine2(const Line2& line0, const Line2& line1, Vector2& intersect);
	// k为斜率,也就是cotan(直线与y轴的夹角)
	static bool generateLineExpression(const Line2& line, float& k, float& b);
	// 计算两条线段的交点,返回值表示两条线段是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	static bool intersectLineSection(const Line2& line0, const Line2& line1, Vector2& intersect, bool checkEndPoint = false);
	// 计算线段与三角形是否相交
	static bool intersectLineTriangle(const Line2& line, const Triangle2& triangle, TriangleIntersect& intersectResult, bool checkEndPoint = false);
	// 当忽略端点重合,如果有端点重合，则判断为不相交
	// 线段与矩形是否相交,checkEndPoint为是否判断两条线段的端点,为false表示即使端点重合也不认为线段相交
	static bool intersect(const Line2& line, const Rect& rect, bool checkEndPoint = false);
	// 计算点到线的距离
	static float getDistanceToLine(const Vector2& point, const Line2& line);
	// 计算点在线上的投影
	static Vector2 getProjectPoint(const Vector2& point, const Line2& line);
	// 计算一个向量在另一个向量上的投影
	static Vector2 getProjection(const Vector2& v1, const Vector2& v2);
	template<typename T>
	static bool binarySearch(myVector<T>& list, int start, int end, const T& value)
	{
		// 没有可查找的值
		if (start >= end)
		{
			return list[start] == value;
		}
		int middle = (start + end) >> 1;
		const T& middleValue = list[middle];
		// 中间值就是要查找的值
		if (middleValue == value)
		{
			return true;
		}
		// 要查找的值比中间值小,查找左边
		if (value < middleValue)
		{
			return binarySearch(list, start, middle - 1, value);
		}
		// 要查找的值比中间值大,查找右边
		else
		{
			return binarySearch(list, middle + 1, end, value);
		}
	}
	template<typename T>
	static void quickSort(myVector<T>& arr, int (*compare)(T&, T&), bool ascend = true)
	{
		quickSort(arr.data(), 0, arr.size() - 1, compare);
	}
	template<typename T>
	static void quickSort(T* arr, int count, int (*compare)(T&, T&), bool ascend = true)
	{
		quickSort(arr, 0, count - 1, compare);
	}
protected:
	template<typename T>
	static void quickSort(T* arr, int low, int high, int (*compare)(T&, T&), bool ascend = true)
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
			if (compare != NULL)
			{
				if (ascend)
				{
					// 升序
					// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
					// 然后将找到的两个值交换,确保小的在左边,大的在右边
					while (compare(arr[++i], key) < 0 && i != high) {}
					while (compare(arr[--j], key) > 0 && j != low) {}
				}
				else
				{
					// 降序
					// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
					// 然后将找到的两个值交换,确保大的在左边,小的在右边
					while (compare(arr[++i], key) > 0 && i != high) {}
					while (compare(arr[--j], key) < 0 && j != low) {}
				}
			}
			else
			{
				if (ascend)
				{
					// 升序
					// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
					// 然后将找到的两个值交换,确保小的在左边,大的在右边
					while (arr[++i] < key && i != high) {}
					while (arr[--j] > key && j != low) {}
				}
				else
				{
					// 降序
					// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
					// 然后将找到的两个值交换,确保大的在左边,小的在右边
					while (arr[++i] > key && i != high) {}
					while (arr[--j] < key && j != low) {}
				}
			}
			if (i >= j)
			{
				break;
			}
			// 交换i,j对应的值
			T temp0 = arr[i];
			arr[i] = arr[j];
			arr[j] = temp0;
		}
		// 中枢值与j对应值交换
		arr[low] = arr[j];
		arr[j] = key;
		quickSort(arr, low, j - 1, compare, ascend);
		quickSort(arr, j + 1, high, compare, ascend);
	}
protected:
	static void initGreaterPow2();
public:
	static array<uint, 513> mGreaterPow2;		// 存储比数值数值大的第一个2的次方数,比如获得比5大的第一个数,则是mGreaterPow2[5]
	static const float MATH_PI;
	static const float MIN_DELTA;
	static const float Deg2Rad;
	static const float Rad2Deg;
	static const float TWO_PI_DEGREE;
	static const float TWO_PI_RADIAN;
	static const float HALF_PI_DEGREE;
	static const float HALF_PI_RADIAN;
	static const float PI_DEGREE;
	static const float PI_RADIAN;
};

#endif