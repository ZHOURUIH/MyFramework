#ifndef _MATH_UTILITY_H_
#define _MATH_UTILITY_H_

#include "StringUtility.h"

class MathUtility : public StringUtility
{
public:
	static void checkInt(float& value, float precision = MIN_DELTA); // 判断传入的参数是否已经接近于整数,如果接近于整数,则设置为整数
	static constexpr bool isFloatZero(float value, float precision = MIN_DELTA) { return value >= -precision && value <= precision; }
	static constexpr bool isFloatEqual(float value1, float value2, float precision = MIN_DELTA) { return isFloatZero(value1 - value2, precision); }
	// 是否为偶数
	// 对于a % b的计算,如果b为2的n次方,则a % b等效于a & (b - 1)
	static constexpr bool isEven(int value) { return (value & 1) == 0; }
	// 是否为2的n次幂
	static constexpr bool isPow2(int value) { return (value & (value - 1)) == 0; }
	static constexpr uint generateBatchCount(ullong count, ullong batch)
	{
		ullong batchCount = count / batch;
		return (uint)(batch * batchCount != count ? batchCount + 1 : batchCount);
	}
	// 得到比value大的第一个pow的n次方的数
	static constexpr uint getGreaterPowerValue(uint value, uint pow);
	// 得到比value大的第一个2的n次方的数
	static uint getGreaterPower2(uint value);
	static constexpr uint pow2(uint pow) { return 1 << pow; }
	static constexpr uint pow10(uint pow);
	// 得到数轴上浮点数右边的第一个整数,向上取整
	static constexpr int ceil(float value)
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
	static constexpr int ceil(double value)
	{
		int intValue = (int)(value);
		if (value >= 0.0f && value > (double)intValue + 0.000001)
		{
			++intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	static constexpr int floor(float value)
	{
		int intValue = (int)(value);
		if (value < 0.0f && value < intValue - 0.0001f)
		{
			--intValue;
		}
		return intValue;
	}
	// 得到数轴上浮点数左边的第一个整数,向下取整
	static constexpr int floor(double value)
	{
		int intValue = (int)(value);
		if (value < 0.0f && value < (double)intValue - 0.000001)
		{
			--intValue;
		}
		return intValue;
	}
	static constexpr int abs(int value) { return value >= 0 ? value : -value; }
	static constexpr float abs(float value) { return value >= 0.0f ? value : -value; }
	static constexpr void saturate(float& value) { clamp(value, 0.0f, 1.0f); }
	// value1大于等于value0则返回1,否则返回0
	static constexpr int step(float value0, float value1) { return value1 >= value0 ? 1 : 0; }
	// 得到value0除以value1的余数
	static constexpr float fmod(float value0, float value1) { return value0 - value1 * (int)(value0 / value1); }
	// 返回value的小数部分
	static constexpr float frac(float value) { return value - (int)value; }
	// 帧换算成秒
	static constexpr float frameToSecond(int frame) { return frame * 0.0333f; }
	
	static float getLength(float x, float y) { return sqrt(x * x + y * y); }
	static float getLength(float x, float y, float z) { return sqrt(x * x + y * y + z * z); }
	static constexpr float toDegree(float radian) { return radian * Rad2Deg; }
	static constexpr float toRadian(float degree) { return degree * Deg2Rad; }
	template<typename T>
	static constexpr void clamp(T& value, T minValue, T maxValue)
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
	static constexpr void clampMin(T& value, T minValue = (T)0)
	{
		if (value < minValue)
		{
			value = minValue;
		}
	}
	template<typename T>
	static constexpr void clampMax(T& value, T maxValue)
	{
		if (value > maxValue)
		{
			value = maxValue;
		}
	}
	static constexpr void clampCycle(float& value, float min, float max, float cycle)
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
	static constexpr void clampCycle(float& value, float min, float max) { clampCycle(value, min, max, max - min); }
	static constexpr void clampRadian180(float& radianAngle) { clampCycle(radianAngle, -MATH_PI, MATH_PI); }
	static constexpr void clampDegree180(float& radianAngle) { clampCycle(radianAngle, -180.0f, 180.0f); }
	static constexpr void clampRadian360(float& radianAngle) { clampCycle(radianAngle, 0.0f, MATH_PI * 2.0f); }
	static constexpr void clampDegree360(float& radianAngle) { clampCycle(radianAngle, 0.0f, 360.0f); }
	static constexpr bool inFixedRange(int value, int range0, int range1) { return value >= range0 && value <= range1; }
	static constexpr bool inFixedRange(float value, float range0, float range1) { return value >= range0 && value <= range1; }
	static constexpr bool inRange(int value, int range0, int range1) { return value >= getMin(range0, range1) && value <= getMax(range0, range1); }
	static constexpr bool inRange(float value, float range0, float range1) { return value >= getMin(range0, range1) && value <= getMax(range0, range1); }
	template<typename T>
	static constexpr T getMin(T a, T b) { return a < b ? a : b; }
	template<typename T>
	static constexpr T getMax(T a, T b) { return a > b ? a : b; }
	template<typename T>
	static constexpr T lerpSimple(const T& start, const T& end, float t) { return start + (end - start) * t; }
	static constexpr float lerp(float start, float end, float t, float minAbsDelta = 0.0f)
	{
		clamp(t, 0.0f, 1.0f);
		float value = start + (end - start) * t;
		// 如果值已经在end的一定范围内了,则直接设置为end
		if (abs(value - end) <= minAbsDelta)
		{
			value = end;
		}
		return value;
	}
	static constexpr int lerp(int start, int end, float t)
	{
		clamp(t, 0.0f, 1.0f);
		return start + (int)((end - start) * t);
	}
	static constexpr float inverseLerp(float a, float b, float value) { return (value - a) / (b - a); }
	// 将表达式str中的keyword替换为replaceValue,然后计算str的值,返回值表示str中是否有被替换的值,str只能是算术表达式
	static bool replaceKeywordAndCalculate(string& str, const string& keyword, int replaceValue, bool floatOrInt);
	// 将表达式str中的所有\\()包含的部分中的keyword替换为keyValue,并且计算包含的表达式,返回值表示str中是否有被替换的部分,str可以是任意表达式
	static bool replaceStringKeyword(string& str, const string& keyword, int keyValue, bool floatOrInt);
	static constexpr float powerFloat(float f, int p)
	{
		clampMin(p);
		float ret = 1.0f;
		while (p--)
		{
			ret *= f;
		}
		return ret;
	}
	static float calculateFloat(string str);	// 以浮点数的计算法则计算一个表达式,只支持加减乘除和括号
	static int calculateInt(string str);		// 以整数的计算法则计算一个表达式,支持取余,加减乘除和括号
	// 秒数转换为分数和秒数
	static constexpr void secondsToMinutesSeconds(uint seconds, uint& outMin, uint& outSec)
	{
		outMin = seconds / 60;
		outSec = seconds % 60;
	}
	static constexpr void secondsToHoursMinutesSeconds(uint seconds, uint& outHour, uint& outMin, uint& outSec)
	{
		outHour = seconds / (60 * 60);
		outMin = (seconds % (60 * 60)) / 60;
		outSec = seconds % 60;
	}
	static constexpr float speedToInterval(float speed) { return 0.0333f / speed; }
	static constexpr float intervalToSpeed(float interval) { return 0.0333f / interval; }
	template<typename T>
	static constexpr void swap(T& value0, T& value1)
	{
		T temp = value0;
		value0 = value1;
		value1 = temp;
	}
	template<typename T>
	static constexpr T sign(T value)
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
	static constexpr int round(float value) { return value > 0.0f ? (int)(value + 0.5f) : (int)(value - 0.5f); }
	static constexpr void checkLLong(llong& value) { clamp(value, 0LL, POWER_LLONG_10[POWER_LLONG_10.size() - 1] - 1); }
	static constexpr float HueToRGB(float v1, float v2, float vH);
	static uint findPointIndex(const Vector<float>& distanceListFromStart, float curDistance, uint startIndex, uint endIndex);
	template<typename T>
	static bool binarySearch(Vector<T>& list, int start, int end, const T& value)
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
	// 递归版本的快速排序,效率比std::sort高
	template<typename T>
	static void quickSort(Vector<T>& arr, bool ascend = true)
	{
		if (ascend)
		{
			quickSortAscend(arr.data(), 0, arr.size() - 1);
		}
		else
		{
			quickSortDecend(arr.data(), 0, arr.size() - 1);
		}
	}
	template<typename T>
	static void quickSort(T* arr, int count, bool ascend = true)
	{
		if (ascend)
		{
			quickSortAscend(arr, 0, count - 1);
		}
		else
		{
			quickSortDecend(arr, 0, count - 1);
		}
	}
	template<typename T>
	static void quickSort(Vector<T>& arr, int (*compare)(T&, T&), bool ascend = true)
	{
		if (ascend)
		{
			quickSortAscend(arr.data(), 0, arr.size() - 1, compare);
		}
		else
		{
			quickSortDecend(arr.data(), 0, arr.size() - 1, compare);
		}
	}
	template<typename T>
	static void quickSort(T* arr, int count, int (*compare)(T&, T&), bool ascend = true)
	{
		if (ascend)
		{
			quickSortAscend(arr, 0, count - 1, compare);
		}
		else
		{
			quickSortDecend(arr, 0, count - 1, compare);
		}
	}
	template<typename T>
	static void quickSortNonRecursive(Vector<T>& arr, bool ascend = true)
	{
		quickSortNonRecursive(arr.data(), arr.size(), ascend);
	}
	// 非递归版本的快速排序,但是执行了大量的额外逻辑,所以效率比较低,比std::sort的效率低
	template<typename T>
	static void quickSortNonRecursive(T* arr, int count, bool ascend = true)
	{
		Vector<int> startList;
		Vector<int> endList;
		Vector<int> lastStartList;
		Vector<int> lastEndList;
		lastStartList.push_back(0);
		lastEndList.push_back(count - 1);
		auto* curStartList = &lastStartList;
		auto* curEndList = &lastEndList;
		auto* lastStartPtr = &startList;
		auto* lastEndPtr = &endList;
		if (ascend)
		{
			while (true)
			{
				lastStartPtr->clear();
				lastEndPtr->clear();
				// 遍历每一个分段数据,将分段数据进行排序,然后返回获得中枢值,根据中枢值计算出下一次要分段的数据
				auto& temp = *curStartList;
				FOR_VECTOR(temp)
				{
					int start = (*curStartList)[i];
					int end = (*curEndList)[i];
					int middle = quickSortNonRecursiveAscend(arr, start, end);
					if (middle < 0)
					{
						continue;
					}
					lastStartPtr->push_back(start, middle + 1);
					lastEndPtr->push_back(middle - 1, end);
				}
				END(temp);
				if (startList.size() == 0)
				{
					break;
				}
				swap(curStartList, lastStartPtr);
				swap(curEndList, lastEndPtr);
			}
		}
		else
		{
			while (true)
			{
				lastStartPtr->clear();
				lastEndPtr->clear();
				// 遍历每一个分段数据,将分段数据进行排序,然后返回获得中枢值,根据中枢值计算出下一次要分段的数据
				auto& temp = *curStartList;
				FOR_VECTOR(temp)
				{
					int start = (*curStartList)[i];
					int end = (*curEndList)[i];
					int middle = quickSortNonRecursiveDecend(arr, start, end);
					if (middle < 0)
					{
						continue;
					}
					lastStartPtr->push_back(start, middle + 1);
					lastEndPtr->push_back(middle - 1, end);
				}
				END(temp);
				if (startList.size() == 0)
				{
					break;
				}
				swap(curStartList, lastStartPtr);
				swap(curEndList, lastEndPtr);
			}
		}
	}
	template<typename T>
	static void quickSortNonRecursive(Vector<T>& arr, int (*compare)(T&, T&), bool ascend = true)
	{
		quickSort(arr.data(), arr.size(), compare, ascend);
	}
	template<typename T>
	static void quickSortNonRecursive(T* arr, int count, int (*compare)(T&, T&), bool ascend = true)
	{
		Vector<int> startList;
		Vector<int> endList;
		Vector<int> lastStartList;
		Vector<int> lastEndList;
		lastStartList.push_back(0);
		lastEndList.push_back(count - 1);
		auto* curStartList = &lastStartList;
		auto* curEndList = &lastEndList;
		auto* lastStartPtr = &startList;
		auto* lastEndPtr = &endList;
		if (ascend)
		{
			while (true)
			{
				lastStartPtr->clear();
				lastEndPtr->clear();
				// 遍历每一个分段数据,将分段数据进行排序,然后返回获得中枢值,根据中枢值计算出下一次要分段的数据
				auto& temp = *curStartList;
				FOR_VECTOR(temp)
				{
					int start = (*curStartList)[i];
					int end = (*curEndList)[i];
					int middle = quickSortNonRecursiveAscend(arr, start, end, compare);
					if (middle < 0)
					{
						continue;
					}
					lastStartPtr->push_back(start, middle + 1);
					lastEndPtr->push_back(middle - 1, end);
				}
				END(temp);
				if (startList.size() == 0)
				{
					break;
				}
				swap(curStartList, lastStartPtr);
				swap(curEndList, lastEndPtr);
			}
		}
		else
		{
			while (true)
			{
				lastStartPtr->clear();
				lastEndPtr->clear();
				// 遍历每一个分段数据,将分段数据进行排序,然后返回获得中枢值,根据中枢值计算出下一次要分段的数据
				auto& temp = *curStartList;
				FOR_VECTOR(temp)
				{
					int start = (*curStartList)[i];
					int end = (*curEndList)[i];
					int middle = quickSortNonRecursiveDecend(arr, start, end, compare);
					if (middle < 0)
					{
						continue;
					}
					lastStartPtr->push_back(start, middle + 1);
					lastEndPtr->push_back(middle - 1, end);
				}
				END(temp);
				if (startList.size() == 0)
				{
					break;
				}
				swap(curStartList, lastStartPtr);
				swap(curEndList, lastEndPtr);
			}
		}
	}
private:
	// 不使用函数指针的方法,但是要求T重载了比较运算符
	// 递归版本的快速排序返回值是中枢值
	template<typename T>
	static int quickSortNonRecursiveAscend(T* arr, int low, int high)
	{
		if (high <= low)
		{
			return -1;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		// 升序
		while (true)
		{
			// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
			// 然后将找到的两个值交换,确保小的在左边,大的在右边
			while (arr[++i] < key && i != high) {}
			while (arr[--j] > key && j != low) {}
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
		return j;
	}
	template<typename T>
	static int quickSortNonRecursiveDecend(T* arr, int low, int high)
	{
		if (high <= low)
		{
			return -1;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
			// 然后将找到的两个值交换,确保大的在左边,小的在右边
			while (arr[++i] > key && i != high) {}
			while (arr[--j] < key && j != low) {}
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
		return j;
	}
	// 使用函数指针的方式,但是此方式效率较低,因为调用函数指针的效率较低,无法进行内联
	template<typename T>
	static int quickSortNonRecursiveAscend(T* arr, int low, int high, int (*compare)(T&, T&))
	{
		if (compare == nullptr)
		{
			return -1;
		}
		if (high <= low)
		{
			return -1;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
			// 然后将找到的两个值交换,确保小的在左边,大的在右边
			while (compare(arr[++i], key) < 0 && i != high) {}
			while (compare(arr[--j], key) > 0 && j != low) {}
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
		return j;
	}
	// 使用函数指针的方式,但是此方式效率较低,因为调用函数指针的效率较低,无法进行内联
	template<typename T>
	static int quickSortNonRecursiveDecend(T* arr, int low, int high, int (*compare)(T&, T&))
	{
		if (compare == nullptr)
		{
			return -1;
		}
		if (high <= low)
		{
			return -1;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
			// 然后将找到的两个值交换,确保大的在左边,小的在右边
			while (compare(arr[++i], key) > 0 && i != high) {}
			while (compare(arr[--j], key) < 0 && j != low) {}
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
		return j;
	}
	// 不使用函数指针的方法,但是要求T重载了比较运算符
	template<typename T>
	static void quickSortAscend(T* arr, int low, int high)
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
			// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
			// 然后将找到的两个值交换,确保小的在左边,大的在右边
			while (arr[++i] < key && i != high) {}
			while (arr[--j] > key && j != low) {}
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
		quickSortAscend(arr, low, j - 1);
		quickSortAscend(arr, j + 1, high);
	}
	// 不使用函数指针的方法,但是要求T重载了比较运算符
	template<typename T>
	static void quickSortDecend(T* arr, int low, int high)
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
			// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
			// 然后将找到的两个值交换,确保大的在左边,小的在右边
			while (arr[++i] > key && i != high) {}
			while (arr[--j] < key && j != low) {}
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
		quickSortDecend(arr, low, j - 1);
		quickSortDecend(arr, j + 1, high);
	}
	// 使用函数指针的方式,但是此方式效率较低,因为调用函数指针的效率较低,无法进行内联
	template<typename T>
	static void quickSortAscend(T* arr, int low, int high, int (*compare)(T&, T&))
	{
		if (compare == nullptr)
		{
			return;
		}
		if (high <= low)
		{
			return;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key大的值,从右向左找到一个比key小的值
			// 然后将找到的两个值交换,确保小的在左边,大的在右边
			while (compare(arr[++i], key) < 0 && i != high) {}
			while (compare(arr[--j], key) > 0 && j != low) {}
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
		quickSortAscend(arr, low, j - 1, compare);
		quickSortAscend(arr, j + 1, high, compare);
	}
	// 使用函数指针的方式,但是此方式效率较低,因为调用函数指针的效率较低,无法进行内联
	template<typename T>
	static void quickSortDecend(T* arr, int low, int high, int (*compare)(T&, T&))
	{
		if (compare == nullptr)
		{
			return;
		}
		if (high <= low)
		{
			return;
		}
		int i = low;
		int j = high + 1;
		T key = arr[low];
		while (true)
		{
			// 从左向右找到一个比key小的值,从右向左找到一个比key大的值
			// 然后将找到的两个值交换,确保大的在左边,小的在右边
			while (compare(arr[++i], key) > 0 && i != high) {}
			while (compare(arr[--j], key) < 0 && j != low) {}
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
		quickSortDecend(arr, low, j - 1, compare);
		quickSortDecend(arr, j + 1, high, compare);
	}
protected:
	static void initGreaterPow2();
protected:
	static Array<513, uint> mGreaterPow2;		// 存储比数值数值大的第一个2的次方数,比如获得比5大的第一个数,则是mGreaterPow2[5]
	static constexpr float MATH_PI = 3.1415926f;
	static constexpr float MIN_DELTA = 0.00001f;
	static constexpr float Deg2Rad = 0.0174532924f;
	static constexpr float Rad2Deg = 57.29578f;
	static constexpr float TWO_PI_DEGREE = MATH_PI * Rad2Deg * 2.0f;
	static constexpr float TWO_PI_RADIAN = MATH_PI * 2.0f;
	static constexpr float HALF_PI_DEGREE = MATH_PI * Rad2Deg * 0.5f;
	static constexpr float HALF_PI_RADIAN = MATH_PI * 0.5f;
	static constexpr float PI_DEGREE = MATH_PI * Rad2Deg;
	static constexpr float PI_RADIAN = MATH_PI;
};

#endif