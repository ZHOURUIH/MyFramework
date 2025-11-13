#include "Utility.h"

const float MathUtility::MATH_PI = 3.1415926f;
const float MathUtility::MIN_DELTA = 0.00001f;
const float MathUtility::Deg2Rad = 0.0174532924F;
const float MathUtility::Rad2Deg = 57.29578F;
const float MathUtility::TWO_PI_DEGREE = MATH_PI * Rad2Deg * 2.0f;
const float MathUtility::TWO_PI_RADIAN = MATH_PI * 2.0f;
const float MathUtility::HALF_PI_DEGREE = MATH_PI * Rad2Deg * 0.5f;
const float MathUtility::HALF_PI_RADIAN = MATH_PI * 0.5f;
const float MathUtility::PI_DEGREE = MATH_PI * Rad2Deg;
const float MathUtility::PI_RADIAN = MATH_PI;

void MathUtility::checkInt(float& value, float precision)
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
		if (value - (float)intValue < 0.5f)
		{
			if (isFloatZero(value - intValue, precision))
			{
				value = (float)intValue;
			}
		}
		// 如果原值减去整数值大于0.5f, 则表示原值可能接近于整数值+1
		else
		{
			if (isFloatZero(value - (intValue + 1), precision))
			{
				value = (float)(intValue + 1);
			}
		}
	}
	// 小于0
	else if (value < 0.0f)
	{
		// 如果原值减去整数值的结果的绝对值小于0.5f,则表示原值可能接近于整数值
		if (abs(value - (float)intValue) < 0.5f)
		{
			if (isFloatZero(value - intValue, precision))
			{
				value = (float)intValue;
			}
		}
		else
		{
			// 如果原值减去整数值的结果的绝对值大于0.5f, 则表示原值可能接近于整数值-1
			if (isFloatZero(value - (intValue - 1), precision))
			{
				value = (float)(intValue - 1);
			}
		}
	}
}

uint MathUtility::getGreaterPowerValue(uint value, uint pow)
{
	uint powValue = 1;
	FOR_I(31)
	{
		if (powValue >= value)
		{
			break;
		}
		powValue *= pow;
	}
	return powValue;
}

uint MathUtility::getGreaterPower2(uint value)
{
	uint powValue = 1;
	FOR_I(31)
	{
		if (powValue >= value)
		{
			break;
		}
		powValue <<= 1;
	}
	return powValue;
}

int MathUtility::ceil(float value)
{
	int intValue = (int)(value);
	if (value >= 0.0f && value > intValue)
	{
		++intValue;
	}
	return intValue;
}

int MathUtility::floor(float value)
{
	int intValue = (int)(value);
	if (value < 0.0f && value < intValue)
	{
		--intValue;
	}
	return intValue;
}

// 根据几率随机选择一个下标
uint MathUtility::randomHit(const myVector<float>& oddsList)
{
	float max = 0.0f;
	FOR_VECTOR(oddsList)
	{
		max += oddsList[i];
	}
	float random = randomFloat(0.0f, max);
	float curValue = 0.0f;
	uint count = oddsList.size();
	FOR_I(count)
	{
		curValue += oddsList[i];
		if (random <= curValue)
		{
			return i;
		}
	}
	return 0;
}

uint MathUtility::randomHit(const float* oddsList, uint count)
{
	float max = 0.0f;
	FOR_I(count)
	{
		max += oddsList[i];
	}
	float random = randomFloat(0.0f, max);
	float curValue = 0.0f;
	FOR_I(count)
	{
		curValue += oddsList[i];
		if (random <= curValue)
		{
			return i;
		}
	}
	return 0;
}

Vector3 MathUtility::normalize(const Vector3& value)
{
	float length = LENGTH_3(value);
	if (IS_FLOAT_ZERO(length))
	{
		return Vector3();
	}
	float inverseLength = 1.0f / length;
	return Vector3(value.x * inverseLength, value.y * inverseLength, value.z * inverseLength);
}

Vector2 MathUtility::normalize(const Vector2& value)
{
	float length = LENGTH_2(value);
	if (IS_FLOAT_ZERO(length))
	{
		return Vector2();
	}
	float inverseLength = 1.0f / length;
	return Vector2(value.x * inverseLength, value.y * inverseLength);
}

float MathUtility::getAngleBetweenVector(const Vector3& vec1, const Vector3& vec2)
{
	Vector3 curVec1 = normalize(vec1);
	Vector3 curVec2 = normalize(vec2);
	return acos(DOT_3(curVec1, curVec2));
}

float MathUtility::getAngleBetweenVector(const Vector2& vec1, const Vector2& vec2)
{
	Vector2 curVec1 = normalize(vec1);
	Vector2 curVec2 = normalize(vec2);
	return acos(DOT_2(curVec1, curVec2));
}

float MathUtility::getAngleFromVector2ToVector2(const Vector2& from, const Vector2& to, bool radian)
{
	if (IS_VECTOR2_EQUAL(from, to))
	{
		return 0.0f;
	}
	Vector3 from3 = normalize(Vector3(from.x, 0.0f, from.y));
	Vector3 to3 = normalize(Vector3(to.x, 0.0f, to.y));
	float angle = getAngleBetweenVector(from3, to3);
	Vector3 crossVec = CROSS(from3, to3);
	if (crossVec.y < 0.0f)
	{
		angle = -angle;
	}
	if (!radian)
	{
		angle = TO_DEGREE(angle);
	}
	return angle;
}

// 求Z轴顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
Vector3 MathUtility::getVectorFromAngle(float angle)
{
	CLAMP_RADIAN_180(angle);
	Vector3 temp(-sin(angle), 0.0f, cos(angle));
	// 在unity坐标系中x轴需要取反
	temp.x = -temp.x;
	return temp;
}

bool MathUtility::replaceKeywordAndCalculate(string& str, const string& keyword, int replaceValue, bool floatOrInt)
{
	// 如果最后的表达式中存在i,则需要把i替换为i具体的值,然后计算出最后的表达式的值
	bool replaced = false;
	int iPos;
	while ((iPos = (int)str.find_first_of(keyword)) != -1)
	{
		replaced = true;
		str = strReplace(str, iPos, iPos + (int)keyword.length(), intToString(replaceValue));
	}
	if (floatOrInt)
	{
		str = floatToString(calculateFloat(str));
	}
	else
	{
		str = intToString(calculateInt(str));
	}
	return replaced;
}

bool MathUtility::replaceStringKeyword(string& str, const string& keyword, int keyValue, bool floatOrInt)
{
	bool replaced = false;
	int expressionBegin = -1;
	int expressionEnd = -1;
	// 倒序寻找
	while (findSubstr(str, "\\(", &expressionBegin, 0, false))
	{
		replaced = true;
		// 找到匹配的)
		findSubstr(str, ")", &expressionEnd, 0, false);
		// expressionBegin + 1 去掉 /
		string calculateValue = str.substr(expressionBegin + 1, expressionEnd - expressionBegin + 1);
		replaceKeywordAndCalculate(calculateValue, keyword, keyValue, floatOrInt);
		// 替换掉最后一个\\()之间的内容
		str = strReplace(str, expressionBegin, expressionEnd + 1, calculateValue);
	}
	return replaced;
}

float MathUtility::powerFloat(float f, int p)
{
	CLAMP_MIN(p, 0);
	float ret = 1.0f;
	while (p--)
	{
		ret *= f;
	}
	return ret;
}

float MathUtility::calculateFloat(string str)
{
	// 判断字符串是否含有非法字符,也就是除数字,小数点,运算符以外的字符
	str = checkString(str, "0123456789.+-*/()");
	// 判断左右括号数量是否相等
	if (getCharCount(str, '(') != getCharCount(str, ')'))
	{
		return 0;
	}

	// 循环判断传入的字符串有没有括号
	while (true)
	{
		// 先判断有没有括号，如果有括号就先算括号里的,如果没有就退出while循环
		if (str.find_first_of("(") != string::npos || str.find_first_of(")") != string::npos)
		{
			int curpos = (int)str.find_last_of("(");
			string strInBracket = str.substr(curpos + 1, str.length() - curpos - 1);
			strInBracket = strInBracket.substr(0, strInBracket.find_first_of(")"));
			float ret = calculateFloat(strInBracket);
			// 如果括号中的计算结果是负数,则标记为负数
			bool isMinus = false;
			if (ret < 0)
			{
				ret = -ret;
				isMinus = true;
			}
			// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
			str = strReplace(str, curpos, curpos + (int)strInBracket.length() + 2, floatToString(ret));
			if (isMinus)
			{
				// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
				bool changeMark = false;
				for (int i = curpos - 1; i >= 0; --i)
				{
					// 找到第一个+号,则直接改为减号,然后退出遍历
					if (str[i] == '+')
					{
						str[i] = '-';
						changeMark = true;
						break;
					}
					// 找到第一个减号,如果减号的左边有数字,则直接改为+号
					// 如果减号的左边不是数字,则该减号是负号,将减号去掉,
					else if (str[i] == '-')
					{
						if (str[i - 1] >= '0' && str[i - 1] <= '9')
						{
							str[i] = '+';
						}
						else
						{
							str = strReplace(str, i, i + 1, EMPTY_STRING);
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

	myVector<float> numbers;
	myVector<char> factors;
	// 表示上一个运算符的下标+1
	int beginpos = 0;
	uint strLen = (uint)str.length();
	FOR_I(strLen)
	{
		// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
		if (i == strLen - 1)
		{
			numbers.push_back(stringToFloat(str.substr(beginpos, strLen - beginpos)));
			break;
		}
		// 找到第一个运算符
		if ((str[i] < '0' || str[i] > '9') && str[i] != '.')
		{
			if (i != 0)
			{
				numbers.push_back(stringToFloat(str.substr(beginpos, i - beginpos)));
			}
			// 如果在表达式的开始就发现了运算符,则表示第一个数是负数,那就处理为0减去这个数的绝对值
			else
			{
				numbers.push_back(0);
			}
			factors.push_back(str[i]);
			beginpos = i + 1;
		}
	}
	if (factors.size() + 1 != numbers.size())
	{
		// 计算错误,运算符与数字数量不符
		return 0;
	}
	// 现在开始计算表达式,按照运算优先级,先计算乘除和取余
	while (true)
	{
		// 表示是否还有乘除表达式
		bool hasMS = false;
		FOR_I(factors.size())
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
				numbers.erase(numbers.begin() + i + 1);
				if (numbers.size() == 0)
				{
					// 计算错误
					return 0;
				}
				numbers[i] = num3;
				// 删除第i个运算符
				factors.erase(factors.begin() + i);
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
		if (factors.size() == 0)
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
			numbers.erase(numbers.begin() + 1);
			if (numbers.size() == 0)
			{
				// 计算错误
				return 0;
			}
			numbers[0] = num3;
			// 删除第0个运算符
			factors.erase(factors.begin());
		}
	}
	if (numbers.size() == 1)
	{
		return numbers[0];
	}
	// 计算错误
	return 0;
}

int MathUtility::calculateInt(string str)
{
	// 判断字符串是否含有非法字符,也就是除数字,小数点,运算符以外的字符
	str = checkString(str, "0123456789+-*/%()");
	// 判断左右括号数量是否相等
	if (getCharCount(str, '(') != getCharCount(str, ')'))
	{
		return 0;
	}

	// 循环判断传入的字符串有没有括号
	while (true)
	{
		// 先判断有没有括号，如果有括号就先算括号里的,如果没有就退出while循环
		if (str.find_first_of("(") != string::npos || str.find_first_of(")") != string::npos)
		{
			int curpos = (int)str.find_last_of("(");
			string strInBracket = str.substr(curpos + 1, str.length() - curpos - 1);
			strInBracket = strInBracket.substr(0, strInBracket.find_first_of(")"));
			int ret = calculateInt(strInBracket);
			// 如果括号中的计算结果是负数,则标记为负数
			bool isMinus = false;
			if (ret < 0)
			{
				ret = -ret;
				isMinus = true;
			}
			// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
			str = strReplace(str, curpos, curpos + (int)strInBracket.length() + 2, intToString(ret));
			if (isMinus)
			{
				// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
				bool changeMark = false;
				for (int i = curpos - 1; i >= 0; --i)
				{
					// 找到第一个+号,则直接改为减号,然后退出遍历
					if (str[i] == '+')
					{
						str[i] = '-';
						changeMark = true;
						break;
					}
					// 找到第一个减号,如果减号的左边有数字,则直接改为+号
					// 如果减号的左边不是数字,则该减号是负号,将减号去掉,
					else if (str[i] == '-')
					{
						if (str[i - 1] >= '0' && str[i - 1] <= '9')
						{
							str[i] = '+';
						}
						else
						{
							str = strReplace(str, i, i + 1, EMPTY_STRING);
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

	myVector<int> numbers;
	myVector<char> factors;
	// 表示上一个运算符的下标+1
	int beginPos = 0;
	uint strLen = (uint)str.length();
	FOR_I(strLen)
	{
		// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
		if (i == strLen - 1)
		{
			numbers.push_back(stringToInt(str.substr(beginPos, strLen - beginPos)));
			break;
		}
		// 找到第一个运算符
		if ((str[i] < '0' || str[i] > '9') && str[i] != '.')
		{
			if (i != 0)
			{
				numbers.push_back(stringToInt(str.substr(beginPos, i - beginPos)));
			}
			// 如果在表达式的开始就发现了运算符,则表示第一个数是负数,那就处理为0减去这个数的绝对值
			else
			{
				numbers.push_back(0);
			}
			factors.push_back(str[i]);
			beginPos = i + 1;
		}
	}
	if (factors.size() + 1 != numbers.size())
	{
		// 计算错误,运算符与数字数量不符
		return 0;
	}
	// 现在开始计算表达式,按照运算优先级,先计算乘除和取余
	while (true)
	{
		// 表示是否还有乘除表达式
		bool hasMS = false;
		FOR_I(factors.size())
		{
			// 先遍历到哪个就先计算哪个
			if (factors[i] == '*' || factors[i] == '/' || factors[i] == '%')
			{
				// 第一个运算数的下标与运算符的下标是相同的
				int num1 = numbers[i];
				int num2 = numbers[i + 1];
				int num3 = 0;
				if (factors[i] == '*')
				{
					num3 = num1 * num2;
				}
				else if (factors[i] == '/')
				{
					num3 = num1 / num2;
				}
				else if (factors[i] == '%')
				{
					num3 = num1 % num2;
				}
				// 删除第i + 1个数,然后将第i个数替换为计算结果
				numbers.erase(numbers.begin() + i + 1);
				if (numbers.size() == 0)
				{
					// 计算错误
					return 0;
				}
				numbers[i] = num3;
				// 删除第i个运算符
				factors.erase(factors.begin() + i);
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
		if (factors.size() == 0)
		{
			break;
		}
		if (factors[0] == '+' || factors[0] == '-')
		{
			// 第一个运算数的下标与运算符的下标是相同的
			int num1 = numbers[0];
			int num2 = numbers[1];
			int num3 = 0;
			if (factors[0] == '+')
			{
				num3 = num1 + num2;
			}
			else if (factors[0] == '-')
			{
				num3 = num1 - num2;
			}
			// 删除第1个数,然后将第0个数替换为计算结果
			numbers.erase(numbers.begin() + 1);
			if (numbers.size() == 0)
			{
				// 计算错误
				return 0;
			}
			numbers[0] = num3;
			// 删除第0个运算符
			factors.erase(factors.begin());
		}
	}
	if (numbers.size() == 1)
	{
		return numbers[0];
	}
	// 计算错误
	return 0;
}

// 秒数转换为分数和秒数
void MathUtility::secondsToMinutesSeconds(uint seconds, uint& outMin, uint& outSec)
{
	outMin = seconds / 60;
	outSec = seconds - outMin * 60;
}

void MathUtility::secondsToHoursMinutesSeconds(uint seconds, uint& outHour, uint& outMin, uint& outSec)
{
	outHour = seconds / (60 * 60);
	outMin = (seconds - outHour * (60 * 60)) / 60;
	outSec = seconds - outHour * (60 * 60) - outMin * 60;
}

float MathUtility::HueToRGB(float v1, float v2, float vH)
{
	if (vH < 0.0f)
	{
		vH += 1.0;
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
		return v1 + (v2 - v1) * (2.0f / 3.0f - vH) * 6.0f;
	}
	return v1;
}

uint MathUtility::findPointIndex(const myVector<float>& distanceListFromStart, float curDistance, uint startIndex, uint endIndex)
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
	uint middleIndex = (uint)((endIndex - startIndex) >> 1) + startIndex;
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

// 计算两条直线的交点,返回值表示两条直线是否相交
bool MathUtility::intersectLine2(const Line2& line0, const Line2& line1, Vector2& intersect)
{
	// 计算两条线的k和b
	float k0 = 0.0f;
	float b0 = 0.0f;
	bool hasK0 = generateLineExpression(line0, k0, b0);
	float k1 = 0.0f;
	float b1 = 0.0f;
	bool hasK1 = generateLineExpression(line1, k1, b1);
	// 两条竖直的线没有交点,即使两条竖直的线重合也不计算交点
	if (!hasK0 && !hasK1)
	{
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
		if (!IS_FLOAT_EQUAL(k0, k1))
		{
			intersect.x = (b1 - b0) / (k0 - k1);
			intersect.y = k0 * intersect.x + b0;
			return true;
		}
	}
	return false;
}
// k为斜率,也就是cotan(直线与y轴的夹角)
bool MathUtility::generateLineExpression(const Line2& line, float& k, float& b)
{
	// 一条横着的线,斜率为0
	if (IS_FLOAT_ZERO(line.mStart.y - line.mEnd.y))
	{
		k = 0.0f;
		b = line.mStart.y;
	}
	// 直线是一条竖直的线,没有斜率
	else if (IS_FLOAT_ZERO(line.mStart.x - line.mEnd.x))
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

bool MathUtility::intersectLineSection(const Line2& line0, const Line2& line1, Vector2& intersect, bool checkEndPoint)
{
	// 有端点重合
	if((IS_VECTOR2_EQUAL(line0.mStart, line1.mStart) || IS_VECTOR2_EQUAL(line0.mStart, line1.mEnd) ||
		IS_VECTOR2_EQUAL(line0.mEnd, line1.mStart) || IS_VECTOR2_EQUAL(line0.mEnd, line1.mEnd)))
	{
		// 考虑端点时认为两条线段相交
		// 不考虑端点时,则两条线段不相交
		return checkEndPoint;
	}
	if (intersectLine2(line0, line1, intersect))
	{
		// 如果交点都在两条线段内,则两条线段相交
		return IS_VECTOR2_IN_RANGE(intersect, line0.mStart, line0.mEnd) && IS_VECTOR2_IN_RANGE(intersect, line1.mStart, line1.mEnd);
	}
	return false;
}

// 计算线段与三角形是否相交
bool MathUtility::intersectLineTriangle(const Line2& line, const Triangle2& triangle, TriangleIntersectResult& intersectResult, bool checkEndPoint)
{
	Vector2 intersect0;
	Vector2 intersect1;
	Vector2 intersect2;
	// 对三条边都要检测,计算出最近的一个交点
	bool result0 = intersectLineSection(line, Line2(triangle.mPoint0, triangle.mPoint1), intersect0, checkEndPoint);
	bool result1 = intersectLineSection(line, Line2(triangle.mPoint1, triangle.mPoint2), intersect1, checkEndPoint);
	bool result2 = intersectLineSection(line, Line2(triangle.mPoint2, triangle.mPoint0), intersect2, checkEndPoint);
	Vector2 point;
	Vector2 linePoint0;
	Vector2 linePoint1;
	float closestDistance = 99999.0f * 99999.0f;
	// 与第一条边相交
	if (result0)
	{
		Vector2 delta = intersect0 - line.mStart;
		float squaredLength = SQUARED_LENGTH_2(delta);
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
		Vector2 delta = intersect1 - line.mStart;
		float squaredLength = SQUARED_LENGTH_2(delta);
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
		Vector2 delta = intersect2 - line.mStart;
		float squaredLength = SQUARED_LENGTH_2(delta);
		if (squaredLength < closestDistance)
		{
			closestDistance = squaredLength;
			point = intersect2;
			linePoint0 = triangle.mPoint2;
			linePoint1 = triangle.mPoint0;
		}
	}
	intersectResult.mIntersectPoint = point;
	intersectResult.mLinePoint0 = linePoint0;
	intersectResult.mLinePoint1 = linePoint1;
	return result0 || result1 || result2;
}

bool MathUtility::intersect(const Line2& line, const Rect& rect, bool checkEndPoint)
{
	float dis = getDistanceToLine(rect.getCenter(), line);
	// 距离大于对角线的一半,则不与矩形相交
	if (dis > LENGTH_XY(rect.width, rect.height) * 0.5f)
	{
		return false;
	}
	// 直线是否与任何一条边相交
	Vector2 leftTop(rect.x, rect.y + rect.height);
	Vector2 rightTop(rect.x + rect.width, rect.y + rect.height);
	Vector2 rightBottom(rect.x + rect.width, rect.y);
	Vector2 leftBottom(rect.x, rect.y);
	Vector2 intersect;
	if (intersectLineSection(line, Line2(leftTop, rightTop), intersect, checkEndPoint))
	{
		return true;
	}
	if (intersectLineSection(line, Line2(leftBottom, rightBottom), intersect, checkEndPoint))
	{
		return true;
	}
	if (intersectLineSection(line, Line2(leftBottom, leftTop), intersect, checkEndPoint))
	{
		return true;
	}
	if (intersectLineSection(line, Line2(rightBottom, rightTop), intersect, checkEndPoint))
	{
		return true;
	}
	return false;
}

// 计算点到线的距离
float MathUtility::getDistanceToLine(const Vector2& point, const Line2& line)
{
	Vector2 pointToLine = point - getProjectPoint(point, line);
	return LENGTH_2(pointToLine);
}

// 计算点在线上的投影
Vector2 MathUtility::getProjectPoint(const Vector2& point, const Line2& line)
{
	// 计算出点到线一段的向量在线上的投影
	Vector2 projectOnLine = getProjection(point - line.mStart, line.mEnd - line.mStart);
	// 点到线垂线的交点
	return line.mStart + projectOnLine;
}

// 计算一个向量在另一个向量上的投影
Vector2 MathUtility::getProjection(const Vector2& v1, const Vector2& v2)
{
	float v1Length = LENGTH_2(v1);
	return normalize(v2) * v1Length * cos(getAngleBetweenVector(v1, v2));
}