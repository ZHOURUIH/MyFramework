#include "FrameHeader.h"

Array<513, uint> MathUtility::mGreaterPow2{ 0 };

void MathUtility::initGreaterPow2()
{
	// double check 防止在多线程执行时多次重复初始化
	if (mGreaterPow2[0] == 0)
	{
		FOR_I(mGreaterPow2.size())
		{
			if (i <= 1)
			{
				mGreaterPow2[i] = 2;
			}
			else
			{
				mGreaterPow2[i] = getGreaterPowerValue(i, 2);
			}
		}
	}
}

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

constexpr uint MathUtility::getGreaterPowerValue(uint value, uint pow)
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
	if (mGreaterPow2[0] == 0)
	{
		initGreaterPow2();
	}
	if (value < mGreaterPow2.size())
	{
		return mGreaterPow2[value];
	}
	if (isPow2(value))
	{
		return value;
	}
	// 由于2的9次方以下都可以通过查表的方式获得,所以此处直接从10次方开始
	// 分2个档位,2的15次方,这样处理更快一些,比二分查找或顺序查找都快
	constexpr int Level0 = 15;
	if (value < 1u << Level0)
	{
		for (int i = 10; i <= Level0; ++i)
		{
			if (1u << i >= value)
			{
				return 1u << i;
			}
		}
	}
	else
	{
		for (int i = Level0 + 1; i < 32; ++i)
		{
			if (1u << i >= value)
			{
				return 1u << i;
			}
		}
	}
	ERROR("无法获取大于指定数的第一个2的n次方的数:" + uintToString(value));
	return 0;
}

constexpr uint MathUtility::pow10(uint pow)
{
	uint powValue = 1;
	FOR_I(pow)
	{
		powValue *= 10;
	}
	return powValue;
}

bool MathUtility::replaceKeywordAndCalculate(string& str, const string& keyword, int replaceValue, bool floatOrInt)
{
	// 如果最后的表达式中存在i,则需要把i替换为i具体的值,然后计算出最后的表达式的值
	bool replaced = false;
	int iPos;
	while ((iPos = (int)str.find_first_of(keyword)) != -1)
	{
		replaced = true;
		replace(str, iPos, iPos + (int)keyword.length(), intToString(replaceValue));
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
	while (findString(str, "\\(", &expressionBegin, 0, false))
	{
		replaced = true;
		// 找到匹配的)
		findString(str, ")", &expressionEnd, 0, false);
		// expressionBegin + 1 去掉 /
		string calculateValue = str.substr(expressionBegin + 1, expressionEnd - expressionBegin + 1);
		replaceKeywordAndCalculate(calculateValue, keyword, keyValue, floatOrInt);
		// 替换掉最后一个\\()之间的内容
		replace(str, expressionBegin, expressionEnd + 1, calculateValue);
	}
	return replaced;
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
		size_t leftBracket = str.find_first_of('(');
		size_t rightBracket = str.find_first_of('(');
		if (leftBracket == NOT_FIND && rightBracket == NOT_FIND)
		{
			break;
		}
		float ret = calculateFloat(str.substr(leftBracket + 1, rightBracket - leftBracket - 1));
		// 如果括号中的计算结果是负数,则标记为负数
		bool isMinus = ret < 0;
		ret = abs(ret);
		// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
		replace(str, (int)leftBracket, (int)(rightBracket + 1), floatToString(ret));
		if (isMinus)
		{
			// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
			bool changeMark = false;
			for (int i = (int)leftBracket - 1; i >= 0; --i)
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
				if (str[i] == '-')
				{
					if (str[i - 1] >= '0' && str[i - 1] <= '9')
					{
						str[i] = '+';
					}
					else
					{
						str.erase(i);
					}
					changeMark = true;
					break;
				}
			}
			// 如果遍历完了还没有找到可以替换的符号,则在表达式最前面加一个负号
			if (!changeMark)
			{
				str.insert(0, 1, '-');
			}
		}
	}

	Vector<float> numbers;
	Vector<char> factors;
	// 表示上一个运算符的下标+1
	int beginPos = 0;
	uint strLen = (uint)str.length();
	FOR_I(strLen)
	{
		// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
		if (i == (int)strLen - 1)
		{
			numbers.push_back(stringToFloat(str.substr(beginPos, strLen - beginPos)));
			break;
		}
		// 找到第一个运算符
		if ((str[i] < '0' || str[i] > '9') && str[i] != '.')
		{
			if (i != 0)
			{
				numbers.push_back(stringToFloat(str.substr(beginPos, i - beginPos)));
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
				numbers.eraseAt(i + 1);
				if (numbers.size() == 0)
				{
					// 计算错误
					return 0;
				}
				numbers[i] = num3;
				// 删除第i个运算符
				factors.eraseAt(i);
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
			numbers.eraseAt(1);
			if (numbers.size() == 0)
			{
				// 计算错误
				return 0;
			}
			numbers[0] = num3;
			// 删除第0个运算符
			factors.eraseAt(0);
		}
	}
	float result = 0.0f;
	if (numbers.size() == 1)
	{
		result = numbers[0];
	}
	// 计算错误
	return result;
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
		size_t leftBracket = str.find_first_of('(');
		size_t rightBracket = str.find_first_of('(');
		if (leftBracket == NOT_FIND && rightBracket == NOT_FIND)
		{
			break;
		}
		int ret = calculateInt(str.substr(leftBracket + 1, rightBracket - leftBracket - 1));
		// 如果括号中的计算结果是负数,则标记为负数
		bool isMinus = ret < 0;
		ret = abs(ret);
		// 将括号中的计算结果替换原来的表达式,包括括号也一起替换
		replace(str, (int)leftBracket, (int)(rightBracket + 1), intToString(ret));
		if (isMinus)
		{
			// 如果括号中计算出来是负数,则将负号提取出来,将左边的第一个加减号改为相反的符号
			bool changeMark = false;
			for (int i = (int)leftBracket - 1; i >= 0; --i)
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
				if (str[i] == '-')
				{
					if (str[i - 1] >= '0' && str[i - 1] <= '9')
					{
						str[i] = '+';
					}
					else
					{
						str.erase(i);
					}
					changeMark = true;
					break;
				}
			}
			// 如果遍历完了还没有找到可以替换的符号,则在表达式最前面加一个负号
			if (!changeMark)
			{
				str.insert(0, 1, '-');
			}
		}
	}

	Vector<int> numbers;
	Vector<char> factors;
	// 表示上一个运算符的下标+1
	int beginPos = 0;
	uint strLen = (uint)str.length();
	FOR_I(strLen)
	{
		// 遍历到了最后一个字符,则直接把最后一个数字放入列表,然后退出循环
		if (i == (int)strLen - 1)
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
				numbers.eraseAt(i + 1);
				if (numbers.size() == 0)
				{
					// 计算错误
					return 0;
				}
				numbers[i] = num3;
				// 删除第i个运算符
				factors.eraseAt(i);
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
			numbers.eraseAt(1);
			if (numbers.size() == 0)
			{
				// 计算错误
				return 0;
			}
			numbers[0] = num3;
			// 删除第0个运算符
			factors.eraseAt(0);
		}
	}
	int result = 0;
	if (numbers.size() == 1)
	{
		result = numbers[0];
	}
	return result;
}

constexpr float MathUtility::HueToRGB(float v1, float v2, float vH)
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

uint MathUtility::findPointIndex(const Vector<float>& distanceListFromStart, float curDistance, uint startIndex, uint endIndex)
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