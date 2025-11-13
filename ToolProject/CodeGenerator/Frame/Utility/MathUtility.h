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

struct TriangleIntersectResult
{
	Vector2 mIntersectPoint; // 交点
	Vector2 mLinePoint0;     // 交点所在的三角形的一条边的起点
	Vector2 mLinePoint1;     // 交点所在的三角形的一条边的终点
};

#define IS_FLOAT_ZERO(value) (value >= -MathUtility::MIN_DELTA && value <= MathUtility::MIN_DELTA)
#define IS_FLOAT_EQUAL(value1, value2) (IS_FLOAT_ZERO(value1 - value2))
#define IS_VECTOR3_EQUAL(vec0, vec1) (IS_FLOAT_ZERO(vec0.x - vec1.x) && IS_FLOAT_ZERO(vec0.y - vec1.y) && IS_FLOAT_ZERO(vec0.z - vec1.z))
#define IS_VECTOR2_EQUAL(vec0, vec1) (IS_FLOAT_ZERO(vec0.x - vec1.x) && IS_FLOAT_ZERO(vec0.y - vec1.y))
#define LENGTH_XY(x, y) sqrt(x * x + y * y)
#define LENGTH_XYZ(x, y, z) sqrt(x * x + y * y + z * z)
#define LENGTH_2(vec) sqrt(vec.x * vec.x + vec.y * vec.y)
#define LENGTH_3(vec) sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z)
#define SQUARED_LENGTH_XY(x, y) (x * x + y * y)
#define SQUARED_LENGTH_XYZ(x, y, z) (x * x + y * y + z * z)
#define SQUARED_LENGTH_2(vec) (vec.x * vec.x + vec.y * vec.y)
#define SQUARED_LENGTH_3(vec) (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z)
#define LENGTH_LESS_3(vec, length) (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length)
#define LENGTH_GREATER_3(vec, length) (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length* length)
#define DOT_3(v0, v1) (v0.x * v1.x + v0.y * v1.y + v0.z * v1.z)
#define DOT_2(v0, v1) (v0.x * v1.x + v0.y * v1.y)
#define CROSS(v0, v1) Vector3(v1.y * v0.z - v0.y * v1.z, v1.x * v0.z - v0.x * v1.z, v1.x * v0.y - v0.x * v1.y)
#define CLAMP(value, minValue, maxValue)\
{\
	if (value > maxValue){value = maxValue;}\
	else if (value < minValue){value = minValue;}\
}
#define CLAMP_MIN(value, minValue) if (value < minValue){value = minValue;}
#define CLAMP_MAX(value, maxValue) if (value > maxValue){value = maxValue;}
#define CLAMP_CYCLE(value, minValue, maxValue, cycle)\
		while (value < minValue)\
		{\
			value += cycle;\
		}\
		while (value > maxValue)\
		{\
			value -= cycle;\
		}
#define CLAMP_ANGLE(angle, minValue, maxValue, pi) CLAMP_CYCLE(angle, minValue, maxValue, pi * 2.0f)
#define CLAMP_RADIAN_180(radian) CLAMP_ANGLE(radian, -MathUtility::MATH_PI, MathUtility::MATH_PI, MathUtility::MATH_PI)
#define CLAMP_DEGREE_180(degree) CLAMP_ANGLE(degree, -180.0f, 180.0f, 180.0f);
#define CLAMP_RADIAN_360(radian) CLAMP_ANGLE(radian, 0.0f, MathUtility::MATH_PI * 2.0f, MathUtility::MATH_PI)
#define CLAMP_DEGREE_360(degree) CLAMP_ANGLE(degree, 0.0f, 360.0f, 180.0f)
#define TO_DEGREE(radian) (radian * MathUtility::Rad2Deg)
#define TO_RADIAN(degree) (degree * MathUtility::Deg2Rad)
#define SATURATE(value) CLAMP(value, 0.0f, 1.0f)
// 判断value是否在minRange和maxRange之间,并且minRange和maxRange的顺序不固定
#define IS_IN_RANGE(value, minRange, maxRange) (value >= MIN(minRange, maxRange) && value <= MAX(minRange, maxRange))
// 判断value是否在minRange和maxRange之间,并且minRange和maxRange的顺序固定
#define IS_IN_RANGE_FIXED(value, minRange, maxRange) (value >= minRange && value <= maxRange)
#define IS_VECTOR2_IN_RANGE(value, minRange, maxRange) (IS_IN_RANGE(value.x, minRange.x, maxRange.x) && IS_IN_RANGE(value.y, minRange.y, maxRange.y))
#define IS_VECTOR2_IN_RANGE_FIXED(value, minRange, maxRange) (IS_IN_RANGE_FIXED(value.x, minRange.x, maxRange.x) && IS_IN_RANGE_FIXED(value.y, minRange.y, maxRange.y))
#define MIN(value0, value1) (value0 < value1 ? value0 : value1)
#define MAX(value0, value1) (value0 > value1 ? value0 : value1)
#define INVERSE_LERP(a, b, value) ((value - a) / (b - a))
#define LERP_SIMPLE(start, end, t) (start + (end - start) * t)
#define ABS(value) value = value > 0 ? value : -value;
#define SIGN(sign, value)\
		if (value > 0)		{sign = 1;}\
		else if (value < 0)	{sign = -1;}\
		else				{sign = 0;}
#define SWAP(value0, value1)\
		auto& temp = value0;\
		value0 = value1;\
		value1 = temp;
#define CEIL(value) ((int)(value) >= 0.0f && value > (int)(value)) ? (int)(value) + 1 : (int)(value)
#define CEIL_2(vec)\
		vec.x = (float)(CEIL(vec.x));\
		vec.y = (float)(CEIL(vec.y));
#define CEIL_3(vec)\
		vec.x = (float)(CEIL(vec.x));\
		vec.y = (float)(CEIL(vec.y));\
		vec.z = (float)(CEIL(vec.z));

class MathUtility : public StringUtility
{
public:
	static void checkInt(float& value, float precision = MIN_DELTA); // 判断传入的参数是否已经接近于整数,如果接近于整数,则设置为整数
	static bool isFloatZero(float value, float precision = MIN_DELTA){return value >= -precision && value <= precision;}
	static bool isFloatEqual(float value1, float value2, float precision = MIN_DELTA){return isFloatZero(value1 - value2, precision);}
	// 得到比value大的第一个pow的n次方的数
	static uint getGreaterPowerValue(uint value, uint pow);
	// 得到比value大的第一个2的n次方的数
	static uint getGreaterPower2(uint value);
	// 得到数轴上浮点数右边的第一个整数,向上取整
	static int ceil(float value);
	// 得到数轴上浮点数左边的第一个整数,向下取整
	static int floor(float value);
	static void saturate(float& value) { clamp(value, 0.0f, 1.0f); }
	// value1大于等于value0则返回1,否则返回0
	static int step(float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	static float fmod(float value0, float value1) { return value0 - value1 * (int)(value0 / value1); }
	// 返回value的小数部分
	static float frac(float value) { return value - (int)value; }
	static float getLength(const Vector2& vec) { return sqrt(vec.x * vec.x + vec.y * vec.y); }
	static float getLength(const Vector3& vec) { return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z); }
	static Vector3 normalize(const Vector3& value);
	static Vector2 normalize(const Vector2& value);
	// 此处计算结果与unity中的Vector3.Cross()一致
	static Vector3 cross(const Vector3& v0, const Vector3& v1) { return Vector3(v1.y * v0.z - v0.y * v1.z, v1.x * v0.z - v0.x * v1.z, v1.x * v0.y - v0.x * v1.y); }
	static float dot(const Vector3& v0, const Vector3& v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	static float dot(const Vector2& v0, const Vector2& v1) { return v0.x * v1.x + v0.y * v1.y; }
	static float toDegree(float radian){return radian * Rad2Deg;}
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
	static void clampMin(T& value, T minValue)
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
	// 根据几率随机选择一个下标
	static uint randomHit(const myVector<float>& oddsList);
	static uint randomHit(const float* oddsList, uint count);
	static float randomFloat(float minFloat, float maxFloat)
	{
		float percent = (rand() % (10000 + 1)) * 0.0001f;
		return percent * (maxFloat - minFloat) + minFloat;
	}
	static uint randomInt(uint minInt, uint maxInt)
	{
		if (minInt >= maxInt)
		{
			return minInt;
		}
		return rand() % (maxInt - minInt + 1) + minInt;
	}
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
	static void clampCycle(float& value, float min, float max)
	{
		clampCycle(value, min, max, max - min);
	}
	static void clampRadian180(float& radianAngle)
	{
		clampCycle(radianAngle, -MATH_PI, MATH_PI);
	}
	static void clampDegree180(float& radianAngle)
	{
		clampCycle(radianAngle, -180.0f, 180.0f);
	}
	static void clampRadian360(float& radianAngle)
	{
		clampCycle(radianAngle, 0.0f, MATH_PI * 2.0f);
	}
	static void clampDegree360(float& radianAngle)
	{
		clampCycle(radianAngle, 0.0f, 360.0f);
	}
	static bool isInRange(int value, int range0, int range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	static bool isInRange(float value, float range0, float range1)
	{
		return value >= getMin(range0, range1) && value <= getMax(range0, range1);
	}
	template<typename T>
	static T getMin(T a, T b)
	{
		return a < b ? a : b;
	}
	template<typename T>
	static T getMax(T a, T b)
	{
		return a > b ? a : b;
	}
	template<typename T>
	static float lerpSimple(const T& start, const T& end, float t)
	{
		return start + (end - start) * t;
	}
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
	static void secondsToMinutesSeconds(uint seconds, uint& outMin, uint& outSec);
	static void secondsToHoursMinutesSeconds(uint seconds, uint& outHour, uint& outMin, uint& outSec);
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
		else if (value < (T)0)
		{
			return (T)-1;
		}
		else
		{
			return (T)0;
		}
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
	static bool intersectLineTriangle(const Line2& line, const Triangle2& triangle, TriangleIntersectResult& intersectResult, bool checkEndPoint = false);
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
public:
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