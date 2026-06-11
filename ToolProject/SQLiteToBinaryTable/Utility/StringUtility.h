#ifndef _STRING_UTILITY_H_
#define _STRING_UTILITY_H_

#include "BinaryUtility.h"
#include "Vector2Int.h"
#include "Vector3Int.h"
#include "Vector2.h"
#include "Vector3.h"

class StringUtility : public BinaryUtility
{
public:
	static string removeSuffix(const string& str);
	// 去掉开始的所有指定字符,直到遇到不是该字符的为止
	static void removeStartAll(string& stream, char key);
	// 去掉第一个出现的指定字符
	static void removeStart(string& stream, char key);
	// 去掉最后出现的所有指定字符,从后往前,直到遇到不是该字符的为止
	static void removeLastAll(string& stream, char key);
	// 去掉最后一个出现的指定字符
	static void removeLast(string& stream, char key);
	// 去掉最后一个逗号
	static void removeLastComma(string& stream);
	// 查找str中指定key的数量
	static int findCharCount(const string& str, char key);
	static int strchar(const char* str, char key, uint startIndex = 0, uint length = 0);
	static string getFileName(string str);
	static string getFileNameNoSuffix(string str, bool removePath);
	static string getFirstFolderName(const string& dir);
	static string removeFirstPath(const string& dir);
	static string getFilePath(const string& dir);
	static string getFileSuffix(const string& fileName);
	static string removeStartString(const string& fileName, const string& startStr);
	static string removeEndString(const string& fileName, const string& endStr);
	static string replaceSuffix(const string& fileName, const string& suffix) { return getFileNameNoSuffix(fileName, false) + suffix; }
	// 获得字符串最后不是数字的下标
	static int getLastNotNumberPos(const string& str);
	// 获得字符串结尾的数字
	static int getLastNumber(const string& str);
	// 获得去除末尾数字以后的字符串
	static string getNotNumberSubString(string str) { return str.substr(0, getLastNotNumberPos(str) + 1); }
	static int getLastChar(const char* str, char value);
	static int getLastNotChar(const char* str, char value);
	static void splitLine(const char* str, char key, Vector<string>& vec, bool removeEmpty = true);
	static void splitLine(const char* str, char key, string* stringBuffer, uint bufferSize, bool removeEmpty = true);
	static void split(const char* str, char key, Vector<string>& vec, bool removeEmpty = true);
	static uint split(const char* str, char key, string* stringBuffer, uint bufferSize, bool removeEmpty = true);
	static void split(const char* str, const char* key, Vector<string>& vec, bool removeEmpty = true);
	static void split(const string& str, const char* key, Vector<string>& vec, bool removeEmpty = true);
	static uint split(const char* str, const char* key, string* stringBuffer, uint bufferSize, bool removeEmpty = true);
	// 将str中的[begin,end)替换为reStr
	template<size_t Length>
	static void replace(Array<Length>& str, int begin, int end, const char* reStr)
	{
		replace(str.toBuffer(), Length, begin, end, reStr);
	}
	static void replace(char* str, int strBufferSize, int begin, int end, const char* reStr);
	static void replace(string& str, int begin, int end, const string& reStr);
	template<size_t Length>
	static void replaceAll(Array<Length>& str, const char* key, const char* newWord)
	{
		replaceAll(str.toBuffer(), Length, key, newWord);
	}
	static void replaceAll(char* str, int strBufferSize, const char* key, const char* newWord);
	static void replaceAll(string& str, const string& key, const string& newWord);
	static void replaceAll(string& str, char key, char newWord);
	static void removeAll(string& str, char value);
	static void removeAll(string& str, char value0, char value1);
	template<size_t Length>
	static bool removeString(Array<Length>& str, const char* subString)
	{
		int subPos = 0;
		if (!findString(str.toString(), subString, &subPos, 0))
		{
			return false;
		}
		// 从子字符串的位置,后面的数据覆盖前面的数据
		uint subLength = strlen(subString);
		memmove(str.toBuffer() + subPos, str.toBuffer() + subPos + subLength, Length - subLength - subPos);
		return true;
	}
	static bool removeString(char* str, uint length, const char* subString);
	// 从一个ullong数组的字符串中移除指定的value的字符串
	template<size_t Length>
	static bool removeULLongsString(Array<Length>& valueArrayString, ullong value)
	{
		ULLONG_STR(valueString, value);
		// 如果value是在最后,则只移除value字符串
		if (endWith(valueArrayString.toString(), valueString.toString()))
		{
			return removeString(valueArrayString, valueString.toString());
		}
		// value不在最后,则移除value字符串加后面的逗号
		else
		{
			Array<32> needRemoveString{ 0 };
			strcat_t(needRemoveString, valueString.toString(), ",");
			return removeString(valueArrayString, needRemoveString.toString());
		}
	}
	// 将value添加到一个ullong数组的字符串中
	template<size_t Length>
	static void addULLongsString(Array<Length>& valueArrayString, ullong value)
	{
		ULLONG_STR(idStr, value);
		if (valueArrayString[0] != '\0')
		{
			strcat_t(valueArrayString, ",", idStr.toString());
		}
		else
		{
			valueArrayString.copy(idStr);
		}
	}
	// 从一个llong数组的字符串中移除指定的value的字符串
	template<size_t Length>
	static bool removeLLongsString(Array<Length>& valueArrayString, llong value)
	{
		LLONG_STR(valueString, value);
		// 如果value是在最后,则只移除value字符串
		if (endWith(valueArrayString.toString(), valueString.toString()))
		{
			return removeString(valueArrayString, valueString.toString());
		}
		// value不在最后,则移除value字符串加后面的逗号
		else
		{
			Array<32> needRemoveString{ 0 };
			strcat_t(needRemoveString, valueString.toString(), ",");
			return removeString(valueArrayString, needRemoveString.toString());
		}
	}
	// 将value添加到一个llong数组的字符串中
	template<size_t Length>
	static void addLLongsString(Array<Length>& valueArrayString, llong value)
	{
		LLONG_STR(idStr, value);
		if (valueArrayString[0] != '\0')
		{
			strcat_t(valueArrayString, ",", idStr.toString());
		}
		else
		{
			valueArrayString.copy(idStr);
		}
	}
	template<size_t Length>
	static uint split(const string& str, const char* key, Array<Length, string>& stringBuffer, bool removeEmpty = true, bool showError = true)
	{
		int startPos = 0;
		int keyLen = strlen(key);
		int sourceLen = (uint)str.length();
		constexpr int STRING_BUFFER = 1024;
		Array<STRING_BUFFER> curString{ 0 };
		int devidePos = -1;
		uint curCount = 0;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
				return 0;
			}
			curString.copy(str, startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' || !removeEmpty)
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("string buffer is too small! bufferSize:" + intToString(Length));
				}
				break;
			}
			stringBuffer[curCount++] = curString.toString();
		}
		return curCount;
	}
	template<size_t Length>
	static uint split(const char* str, const char* key, Array<Length, string>& stringBuffer, bool removeEmpty = true, bool showError = true)
	{
		int startPos = 0;
		int keyLen = strlen(key);
		int sourceLen = strlen(str);
		constexpr int STRING_BUFFER = 1024;
		Array<STRING_BUFFER> curString{ 0 };
		int devidePos = -1;
		uint curCount = 0;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
				return 0;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' || !removeEmpty)
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("string buffer is too small! bufferSize:" + intToString(Length));
				}
				break;
			}
			stringBuffer[curCount++] = curString.toString();
		}
		return curCount;
	}
	// 基础数据类型转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string boolToString(bool value) { return value ? "True" : "False"; }
	static uint strlen(const char* str, uint maxLength);
	static uint strlen(const char* str);
	template<size_t Length>
	static int _itoa_s(int value, Array<Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return 1;
		}

		if (mIntString[0].length() == 0)
		{
			initIntToString();
		}
		// 优先查表,但是前提是表已经有值
		if (mIntString[mIntString.size() - 1].length() > 0 && value >= 0 && value < (int)mIntString.size())
		{
			const string& str = mIntString[value];
			uint strLength = (uint)str.length();
			if (strLength + 1 >= Length)
			{
				ERROR("int value is too large:" + llongToString((llong)value));
				return 0;
			}
			charArray.copy(str, strLength);
			charArray[strLength] = '\0';
			return strLength;
		}

		int sign = 1;
		if (value < 0)
		{
			value = -value;
			sign = -1;
		}
		if ((llong)value > POWER_INT_10[POWER_INT_10.size() - 1])
		{
			ERROR("int value is too large:" + llongToString((llong)value));
			return 0;
		}
		uint index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过size - 1
			// 如果是负数,则数字个数不能超过size - 2
			if ((sign > 0 && index >= Length) ||
				(sign < 0 && index >= Length - 1))
			{
				ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if ((llong)value < POWER_INT_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)((llong)value % POWER_INT_10[index + 1] / POWER_INT_10[index]);
			++index;
		}
		// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
		if (sign > 0)
		{
			uint lengthToHead = Length - index;
			FOR_I(index)
			{
				charArray[i] = charArray[i + lengthToHead] + '0';
			}
			charArray[index] = '\0';
		}
		else
		{
			charArray[0] = '-';
			uint lengthToHead = Length - index;
			FOR_I(index)
			{
				charArray[i + 1] = charArray[i + lengthToHead] + '0';
			}
			charArray[++index] = '\0';
		}
		return index;
	}
	template<size_t Length>
	static int _uitoa_s(uint value, Array<Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return 1;
		}

		if (mIntString[0].length() == 0)
		{
			initIntToString();
		}
		// 优先查表,但是前提是表已经有值
		if (mIntString[mIntString.size() - 1].length() > 0 && value < mIntString.size())
		{
			const string& str = mIntString[value];
			uint strLength = (uint)str.length();
			if (strLength + 1 >= Length)
			{
				ERROR("uint value is too large:" + llongToString((llong)value));
				return 0;
			}
			charArray.copy(str, strLength);
			charArray[strLength] = '\0';
			return strLength;
		}

		if ((llong)value > POWER_INT_10[POWER_INT_10.size() - 1])
		{
			ERROR("uint value is too large:" + llongToString((llong)value));
			return 0;
		}
		uint index = 0;
		while (true)
		{
			// 数字个数不能超过size - 1
			if (index >= Length)
			{
				ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if ((llong)value < POWER_INT_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)((llong)value % POWER_INT_10[index + 1] / POWER_INT_10[index]);
			++index;
		}
		// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
		uint lengthToHead = Length - index;
		FOR_I(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
		return index;
	}
	template<size_t Length>
	static int _ui64toa_s(ullong value, Array<Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return 1;
		}
		if (value > (ullong)POWER_LLONG_10[POWER_LLONG_10.size() - 1])
		{
			ERROR("ullong value is too large");
			return 0;
		}
		uint index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过Length - 1
			if (index >= Length)
			{
				ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if (value < (ullong)POWER_LLONG_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)(value % POWER_LLONG_10[index + 1] / POWER_LLONG_10[index]);
			++index;
		}
		// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
		uint lengthToHead = Length - index;
		FOR_I(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
		return index;
	}
	template<size_t Length>
	static int _i64toa_s(llong value, Array<Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return 1;
		}
		int sign = 1;
		// 负数需要转换为正数才能正常转换为字符串
		if (value < 0)
		{
			value = -value;
			sign = -1;
		}
		llong maxLLong = POWER_LLONG_10[POWER_LLONG_10.size() - 1] - 1;
		if (value > maxLLong)
		{
			value = maxLLong;
		}
		uint index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过size - 1
			// 如果是负数,则数字个数不能超过size - 2
			if ((sign > 0 && index >= Length) ||
				(sign < 0 && index >= Length - 1))
			{
				ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if (value < POWER_LLONG_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)(value % POWER_LLONG_10[index + 1] / POWER_LLONG_10[index]);
			++index;
		}
		// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
		if (sign > 0)
		{
			uint lengthToHead = Length - index;
			FOR_I(index)
			{
				charArray[i] = charArray[i + lengthToHead] + '0';
			}
			charArray[index] = '\0';
		}
		else
		{
			charArray[0] = '-';
			uint lengthToHead = Length - index;
			FOR_I(index)
			{
				charArray[i + 1] = charArray[i + lengthToHead] + '0';
			}
			charArray[++index] = '\0';
		}
		return index;
	}
	// 将source拼接到destBuffer后面
	static void strcat_s(char* destBuffer, uint size, const char* source);
	static void strcat_s(char* destBuffer, uint size, const char* source, uint length);
	template<size_t Length>
	static void strcat_s(Array<Length>& destBuffer, const string& source)
	{
		uint destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source);
		destBuffer[destIndex + (uint)source.length()] = '\0';
	}
	template<size_t Length>
	static void strcat_s(Array<Length>& destBuffer, const string& source, uint length)
	{
		uint destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<size_t Length>
	static void strcat_s(Array<Length>& destBuffer, const char* source)
	{
		uint destIndex = destBuffer.length();
		uint length = strlen(source);
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<size_t Length>
	static void strcat_s(Array<Length>& destBuffer, const char* source, uint length)
	{
		uint destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<size_t Length>
	static void strcat_s(Array<Length>& destBuffer, const char** sourceArray, uint sourceCount)
	{
		uint destIndex = destBuffer.length();
		FOR_I(sourceCount)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			uint length = strlen(curSource);
			destBuffer.copy(destIndex, curSource, length);
			destIndex += length;
		}
		destBuffer[destIndex] = '\0';
	}
	template<size_t SourceLength>
	static void strcat_s(char* destBuffer, uint destSize, const Array<SourceLength, const char*>& sourceArray)
	{
		uint destIndex = strlen(destBuffer, destSize);
		FOR_I((uint)SourceLength)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			uint length = strlen(curSource);
			if (destIndex + length >= destSize)
			{
				ERROR("strcat_s buffer is too small");
				break;
			}
			MEMCPY(destBuffer + destIndex, destSize - destIndex, curSource, length);
			destIndex += length;
		}
		destBuffer[destIndex] = '\0';
	}
	template<typename... TypeList>
	static void strcat_t(char* destBuffer, uint destSize, TypeList... params)
	{
		strcat_s(destBuffer, destSize, Array<sizeof...(params), const char*>{params...});
	}
	template<size_t Length, size_t SourceLength>
	static void strcat_s(Array<Length>& destBuffer, const Array<SourceLength, const char*>& sourceArray)
	{
		uint destIndex = destBuffer.length();
		FOR_I((uint)SourceLength)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			uint length = strlen(curSource);
			destBuffer.copy(destIndex, curSource, length);
			destIndex += length;
		}
		destBuffer[destIndex] = '\0';
	}
	template<size_t Length, typename... TypeList>
	static void strcat_t(Array<Length>& destBuffer, TypeList... params)
	{
		strcat_s(destBuffer, Array<sizeof...(params), const char*>{params...});
	}
	static void strcpy_s(char* destBuffer, uint size, const char* source);
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	static string intToString(int value, uint limitLen = 0);
	template<size_t Length>
	static uint intToString(Array<Length>& charArray, int value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _itoa_s(value, charArray);
		}
		// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
		Array<16> temp{ 0 };
		uint len = _itoa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
			Array<16> zeroArray{ 0 };
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.toString(), temp.toString());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.toString(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	static string uintToString(uint value, uint limitLen = 0);
	template<size_t Length>
	static uint uintToString(Array<Length>& charArray, uint value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _itoa_s(value, charArray);
		}
		// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
		Array<16> temp{ 0 };
		uint len = _itoa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
			Array<16> zeroArray{ 0 };
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.toString(), temp.toString());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.toString(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// 返回string类型的数字字符串,速度较慢
	static string ullongToString(ullong value, uint limitLen = 0);
	template<size_t Length>
	static uint ullongToString(Array<Length>& charArray, ullong value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _ui64toa_s(value, charArray);
		}
		Array<32> temp{ 0 };
		uint len = _ui64toa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			Array<16> zeroArray{ 0 };
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.toString(), temp.toString());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.toString(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// 返回string类型的数字字符串,速度较慢
	static string llongToString(llong value, uint limitLen = 0);
	template<size_t Length>
	static uint llongToString(Array<Length>& charArray, llong value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _i64toa_s(value, charArray);
		}
		Array<32> temp{ 0 };
		uint len = _i64toa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			Array<16> zeroArray{ 0 };
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.toString(), temp.toString());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.toString(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// precision为精度,保留的小数的位数,removeZero为是否去除末尾无用的0,速度较慢
	static string floatToStringExtra(float f, uint precision = 4, bool removeTailZero = true);
	// 将浮点数转换为字符串,速度较快
	static string floatToString(float f);
	template<size_t Length>
	static uint floatToString(Array<Length>& charArray, float f)
	{
		if (f > 99999999.0f)
		{
			f = 99999999.0f;
		}
		else if (f < -99999999.0f)
		{
			f = -99999999.0f;
		}
		SPRINTF(charArray.toBuffer(), Length, "%.4f", f);
		charArray[Length - 1] = '\0';
		// 先查找小数点和结束符的位置
		int dotPos = -1;
		uint strLen = 0;
		FOR_I(Length)
		{
			if (charArray[i] == '.')
			{
				dotPos = i;
			}
			else if (charArray[i] == '\0')
			{
				strLen = i;
				break;
			}
		}
		if (dotPos >= 0)
		{
			// 从结束符往前查找
			FOR_I(strLen)
			{
				uint index = strLen - 1 - i;
				// 如果找到了小数点仍然没有找到一个不为'0'的字符,则从小数点部分截断字符串
				if (index == (uint)dotPos)
				{
					charArray[dotPos] = '\0';
					strLen = dotPos;
					break;
				}
				// 找到小数点后的从后往前的第一个不为'0'的字符,从此处截断字符串
				if (charArray[index] != '0' && index + 1 < strLen)
				{
					charArray[index + 1] = '\0';
					strLen = index + 1;
					break;
				}
			}
		}
		return strLen;
	}
	static string bytesToString(const char* buffer, uint length);
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型
	//-----------------------------------------------------------------------------------------------------------------------------
	static bool stringToBool(const string& str) { return str == "True" || str == "true"; }
	static bool stringToBool(const char* str) { return strcmp(str, "True") == 0 || strcmp(str, "true") == 0; }
	static int stringToInt(const string& str) { return atoi(str.c_str()); }
	static int stringToInt(const char* str) { return atoi(str); }
	static Vector2Int stringToVector2Int(const string& str, const char* seperate = ",", bool* result = nullptr);
	static Vector3Int stringToVector3Int(const string& str, const char* seperate = ",", bool* result = nullptr);
	static Vector2 stringToVector2(const string& str, const char* seperate = ",", bool* result = nullptr);
	static Vector3 stringToVector3(const string& str, const char* seperate = ",", bool* result = nullptr);
	static ullong stringToULLong(const string& str) { return (ullong)atoll(str.c_str()); }
	static ullong stringToULLong(const char* str) { return (ullong)atoll(str); }
	static llong stringToLLong(const string& str) { return atoll(str.c_str()); }
	static llong stringToLLong(const char* str) { return atoll(str); }
	static float stringToFloat(const string& str) { return (float)atof(str.c_str()); }
	static float stringToFloat(const char* str) { return (float)atof(str); }
	//-----------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型数组转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string ullongsToString(const Vector<ullong>& valueList, const char* seperate = ",");
	template<size_t Length>
	static string ullongsToString(const Array<Length, ullong>& valueList, uint count = (uint)-1, const char* seperate = ",")
	{
		if (count == (uint)-1)
		{
			count = valueList.size();
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		int arrayLen = 32 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		charArray[0] = 0;
		Array<32> temp{ 0 };
		FOR_I(count)
		{
			ullongToString(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(charArray, arrayLen, temp.toString(), seperate);
			}
			else
			{
				strcat_s(charArray, arrayLen, temp.toString());
			}
		}
		string str(charArray);
		deleteCharArray(charArray);
		return str;
	}
	template<size_t Length>
	static void ullongsToString(Array<Length>& buffer, const ullong* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}
		Array<32> temp{ 0 };
		FOR_I(count)
		{
			ullongToString(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(buffer, temp.toString(), seperate);
			}
			else
			{
				strcat_s(buffer, temp.toString());
			}
		}
	}
	template<size_t Length>
	static void ullongsToString(Array<Length>& buffer, const Vector<ullong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		Array<32> temp{ 0 };
		FOR_CONST(valueList)
		{
			ullongToString(temp, valueList[i]);
			if (i != valueListCount - 1)
			{
				strcat_t(buffer, temp.toString(), seperate);
			}
			else
			{
				strcat_s(buffer, temp.toString());
			}
		}
		END_CONST();
	}
	static void ullongsToString(char* buffer, uint bufferSize, const Vector<ullong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		Array<32> temp{ 0 };
		FOR_CONST(valueList)
		{
			ullongToString(temp, valueList[i]);
			if (i != valueListCount - 1)
			{
				strcat_t(buffer, bufferSize, temp.toString(), seperate);
			}
			else
			{
				strcat_s(buffer, bufferSize, temp.toString());
			}
		}
		END_CONST();
	}
	static string llongsToString(const Vector<llong>& valueList, const char* seperate = ",");
	template<size_t Length>
	static string llongsToString(const Array<Length, llong>& valueList, int count = -1, const char* seperate = ",")
	{
		if (count == -1)
		{
			count = (int)Length;
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		uint arrayLen = 32 * greaterPower2((uint)count);
		char* charArray = newCharArray(arrayLen);
		charArray[0] = 0;
		uint seperateLen = strlen(seperate);
		Array<32> temp{ 0 };
		FOR_I(count)
		{
			uint len = llongToString(temp, valueList[i]);
			strcat_s(charArray, arrayLen, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(charArray, arrayLen, seperate, seperateLen);
			}
		}
		string str(charArray);
		deleteCharArray(charArray);
		return str;
	}
	template<size_t Length>
	static void llongsToString(Array<Length>& buffer, const llong* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}
		uint seperateLen = strlen(seperate);
		Array<32> temp{ 0 };
		FOR_I(count)
		{
			uint len = llongToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void llongsToString(Array<Length>& buffer, const Vector<llong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		uint seperateLen = strlen(seperate);
		Array<32> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = llongToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static void llongsToString(char* buffer, uint bufferSize, const Vector<llong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		uint seperateLen = strlen(seperate);
		Array<32> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = llongToString(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	template<size_t Length, size_t ValueCount>
	static void llongsToString(Array<Length>& buffer, const Array<ValueCount, llong>& valueList, int count = -1, const char* seperate = ",")
	{
		if (count == -1)
		{
			count = (int)ValueCount;
		}
		uint seperateLen = strlen(seperate);
		buffer[0] = '\0';
		Array<32> temp{ 0 };
		FOR_I(count)
		{
			uint len = llongToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	// 将byte数组当成整数数组转换为字符串,并非直接将byte数组转为字符串
	static string bytesToString(const Vector<byte>& valueList, const char* seperate = ",");
	template<size_t Length>
	static void bytesToString(Array<Length>& buffer, byte* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<4> temp{ 0 };
		FOR_I(count)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void bytesToString(Array<Length>& buffer, const Vector<byte>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<4> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static void bytesToString(char* buffer, uint bufferSize, const Vector<byte>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<4> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static string ushortsToString(const Vector<ushort>& valueList, const char* seperate = ",");
	template<size_t Length>
	static void ushortsToString(Array<Length>& buffer, ushort* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<8> temp{ 0 };
		FOR_I(count)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void ushortsToString(Array<Length>& buffer, const Vector<ushort>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<8> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static void ushortsToString(char* buffer, uint bufferSize, const Vector<ushort>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<8> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static string intsToString(const Vector<int>& valueList, const char* seperate = ",");
	template<size_t Length>
	static string intsToString(const Array<Length, int>& valueList, int count, const char* seperate = ",")
	{
		if (count == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		charArray[0] = 0;
		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_I(count)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(charArray, arrayLen, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(charArray, arrayLen, seperate, seperateLen);
			}
		}
		string str(charArray);
		deleteCharArray(charArray);
		return str;
	}
	template<size_t Length>
	static string intsToString(const Array<Length, int>& valueList, const char* seperate = ",")
	{
		uint count = valueList.size();
		if (count == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		uint arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		charArray[0] = 0;
		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_I(count)
		{
			int len = intToString(temp, valueList[i]);
			strcat_s(charArray, arrayLen, temp.toString(), len);
			if (i != (int)count - 1)
			{
				strcat_s(charArray, arrayLen, seperate, seperateLen);
			}
		}
		string str(charArray);
		deleteCharArray(charArray);
		return str;
	}
	template<size_t Length>
	static void intsToString(Array<Length>& buffer, int* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_I(count)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void intsToString(Array<Length>& buffer, const Vector<int>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static void intsToString(char* buffer, uint bufferSize, const Vector<int>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = intToString(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static string uintsToString(const Vector<uint>& valueList, const char* seperate = ",");
	template<size_t Length>
	static void uintsToString(Array<Length>& buffer, uint* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_I(count)
		{
			uint len = uintToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void uintsToString(Array<Length>& buffer, const Vector<uint>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = uintToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static void uintsToString(char* buffer, uint bufferSize, const Vector<uint>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = uintToString(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	static string floatsToString(const Vector<float>& valueList, const char* seperate = ",");
	static void floatsToString(char* buffer, uint bufferSize, const Vector<float>& valueList, const char* seperate = ",");
	template<size_t Length>
	static void floatsToString(Array<Length>& buffer, float* valueList, int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_I(count)
		{
			uint len = floatToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<size_t Length>
	static void floatsToString(Array<Length>& buffer, const Vector<float>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		uint seperateLen = strlen(seperate);
		Array<16> temp{ 0 };
		FOR_CONST(valueList)
		{
			uint len = floatToString(temp, valueList[i]);
			strcat_s(buffer, temp.toString(), len);
			if (i != valueListCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
		END_CONST();
	}
	template<size_t Length>
	static void stringsToString(Array<Length>& buffer, const char** strList, int stringCount, const char* seperate = ",")
	{
		uint seperateLen = strlen(seperate);
		buffer[0] = '\0';
		FOR_I(stringCount)
		{
			strcat_s(buffer, strList[i]);
			if (i != stringCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型数组
	//-----------------------------------------------------------------------------------------------------------------------------
	static void stringToBools(const string& str, Vector<bool>& valueList, const char* seperate = ",");
	static uint stringToBools(const char* str, bool* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToBools(const char* str, Array<Length, bool>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<4> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString()) != 0;
		}
		return curCount;
	}
	static void stringToBytes(const string& str, Vector<byte>& valueList, const char* seperate = ",");
	static uint stringToBytes(const char* str, byte* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToBytes(const char* str, Array<Length, byte>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<4> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	static void stringToShorts(const string& str, Vector<short>& valueList, const char* seperate = ",");
	static uint stringToShorts(const char* str, short* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToShorts(const char* str, Array<Length, short>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<8> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	static void stringToUShorts(const string& str, Vector<ushort>& valueList, const char* seperate = ",");
	static uint stringToUShorts(const char* str, ushort* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToUShorts(const char* str, Array<Length, ushort>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<8> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	static bool stringToVector2Ints(const string& str, Vector<Vector2Int>& valueList, const char* seperate = ",");
	static bool stringToVector3Ints(const string& str, Vector<Vector3Int>& valueList, const char* seperate = ",");
	static bool stringToVector2s(const string& str, Vector<Vector2>& valueList, const char* seperate = ",");
	static bool stringToVector3s(const string& str, Vector<Vector3>& valueList, const char* seperate = ",");
	static void stringToInts(const string& str, Vector<int>& valueList, const char* seperate = ",");
	static uint stringToInts(const char* str, int* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToInts(const string& str, Array<Length, int>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = (uint)str.length();
		Array<16> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str, startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	template<size_t Length>
	static uint stringToInts(const char* str, Array<Length, int>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<16> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	static void stringToUInts(const string& str, Vector<uint>& valueList, const char* seperate = ",");
	static uint stringToUInts(const char* str, uint* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToUInts(const char* str, Array<Length, uint>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<16> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("uint buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.toString());
		}
		return curCount;
	}
	static void stringToULLongs(const char* str, Vector<ullong>& valueList, const char* seperate = ",");
	static uint stringToULLongs(const char* str, ullong* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToULLongs(const char* str, Array<Length, ullong>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("ullong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToULLong(curString.toString());
		}
		return curCount;
	}
	static void stringToLLongs(const char* str, Vector<llong>& valueList, const char* seperate = ",");
	static void stringToLLongs(const string& str, Vector<llong>& valueList, const char* seperate = ",");
	template<size_t Length>
	static uint stringToLLongs(const string& str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", bool showError = true)
	{
		uint curCount = destOffset;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = (uint)str.length();
		Array<32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str, startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					LOG("llong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToLLong(curString.toString());
		}
		return curCount;
	}
	template<size_t Length>
	static uint stringToLLongs(const char* str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", bool showError = true)
	{
		uint curCount = destOffset;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		Array<32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("llong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToLLong(curString.toString());
		}
		return curCount;
	}
	static void stringToFloats(const string& str, Vector<float>& valueList, const char* seperate = ",");
	template<size_t Length>
	static uint stringToFloats(const char* str, Array<Length, float>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = strlen(seperate);
		uint sourceLen = strlen(str);
		constexpr int BUFFER_SIZE = 32;
		Array<32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			if (curCount >= Length)
			{
				if (showError)
				{
					ERROR("float buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToFloat(curString.toString());
		}
		return curCount;
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 以string类型返回count个0
	static string zeroString(uint zeroCount);
	template<size_t Length>
	static void zeroString(Array<Length>& charArray, uint zeroCount)
	{
		if (Length <= zeroCount)
		{
			ERROR("buffer is too small");
			return;
		}
		FOR_I(zeroCount)
		{
			charArray[i] = '0';
		}
		charArray[zeroCount] = '\0';
	}
	// 判断oriString是否以pattern结尾,sensitive为是否大小写敏感
	static bool endWith(const char* oriString, const char* pattern, bool sensitive = true);
	static bool endWith(const string& oriString, const char* pattern, bool sensitive = true);
	// 判断oriString是否以pattern开头,sensitive为是否大小写敏感
	static bool startWith(const char* oriString, const char* pattern, bool sensitive = true);
	static bool startWith(const string& oriString, const char* pattern, bool sensitive = true);
	// 移除字符串首部的数字
	static string removePreNumber(const string& str);
	static wstring ANSIToUnicode(const char* str);
	static void ANSIToUnicode(const char* str, wchar_t* output, uint maxLength);
	static string UnicodeToANSI(const wchar_t* str);
	static void UnicodeToANSI(const wchar_t* str, char* output, uint maxLength);
	static string UnicodeToUTF8(const wchar_t* str);
	static void UnicodeToUTF8(const wchar_t* str, char* output, uint maxLength);
	static wstring UTF8ToUnicode(const char* str);
	static void UTF8ToUnicode(const char* str, wchar_t* output, uint maxLength);
	static string ANSIToUTF8(const char* str, bool addBOM = false);
	static void ANSIToUTF8(const char* str, string& utf8, bool addBOM = false);
	static void ANSIToUTF8(const char* str, char* output, uint maxLength, bool addBOM);
	static void UTF8ToANSI(const char* str, string& ansi, bool eraseBOM = false);
	static void UTF8ToANSI(const char* str, char* output, uint maxLength, bool eraseBOM);
	static void removeBOM(string& str);
	static void removeBOM(char* str, uint length = 0);
	// json
	static void jsonStartArray(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonEndArray(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonStartStruct(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonEndStruct(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonAddPair(string& str, const string& name, const string& value, uint preTableCount = 0, bool returnLine = false);
	static void jsonAddObject(string& str, const string& name, const string& value, uint preTableCount = 0, bool returnLine = false);
	static string toLower(const string& str);
	static string toUpper(const string& str);
	static char toLower(char value)
	{
		if (isUpper(value))
		{
			return value + 'a' - 'A';
		}
		return value;
	}
	static char toUpper(char value)
	{
		if (isLower(value))
		{
			return value - ('a' - 'A');
		}
		return value;
	}
	static void rightToLeft(string& str);
	static void leftToRight(string& str);
	// 将字符串全部转为小写再查找
	static bool findStringLower(const string& res, const string& sub, int* pos = nullptr, uint startIndex = 0, bool direction = true);
	// 可指定从后或者从头查找子字符串
	static bool findString(const string& res, const char* dst, int* pos = nullptr, uint startIndex = 0, bool direction = true);
	static bool findString(const char* res, const char* dst, int* pos = nullptr, uint startIndex = 0, bool direction = true);
	static int findStringPos(const char* res, const char* dst, uint startIndex = 0, bool direction = true);
	static int findStringPos(const string& res, const string& dst, uint startIndex = 0, bool direction = true);
	static bool checkString(const string& str, const string& valid);
	static bool checkFloatString(const string& str, const string& valid = "");
	static bool checkIntString(const string& str, const string& valid = "");
	static bool hasNonAscii(const char* str);
	static string charToHexString(byte value, bool upper = true);
	static uint getCharCount(const string& str, char key);
	static uint getCharCount(const char* str, char key);
	static bool isPhoneNumber(const char* str);
	static string bytesToHexString(byte* data, uint dataCount, bool addSpace = true, bool upper = true);
	static byte hexCharToByte(char hex)
	{
		if (isNumber(hex))
		{
			return hex - '0';
		}
		if (isUpper(hex))
		{
			return 0x0A + hex - 'A';
		}
		return 0;
	}
	static char byteToHexChar(byte value)
	{
		if (value <= 9)
		{
			return value + '0';
		}
		if (value <= 0x0F)
		{
			return value - 0x0A + 'A';
		}
		return 0;
	}
	static bool isUpper(char c) { return c >= 'A' && c <= 'Z'; }
	static bool isLower(char c) { return c >= 'a' && c <= 'z'; }
	static bool isNumber(char c) { return c >= '0' && c <= '9'; }
	static bool isNumber(const char* str, uint length = 0);
	static bool isLetter(char c) { return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'); }
	// sql语法相关字符串处理
	static void checkSQLString(const char*& str);
	// insert相关
	template<size_t Length>
	static void sqlAdd(Array<Length>& sqlStr)
	{
		// 当递归展开到最后一层时移除最后一个逗号,确保不是以逗号结尾
		uint length = sqlStr.length();
		if (length > 0 && sqlStr[length - 1] == ',')
		{
			sqlStr[length - 1] = '\0';
		}
		return;
	}
	static void sqlAddString(char* queryStr, uint size, const char* str, bool addComma);
	static void sqlAddStringUTF8(char* queryStr, uint size, const char* str, bool addComma);
	template<size_t Length>
	static void sqlAddString(Array<Length>& queryStr, const char* str, bool addComma)
	{
		checkSQLString(str);
		strcat_t(queryStr, "\"", str, addComma ? "\"," : "\"");
	}
	template<size_t Length>
	static void sqlAddStringUTF8(Array<Length>& queryStr, const char* str, bool addComma)
	{
		checkSQLString(str);
		if (str[0] != '\0')
		{
			char* utf8String = newCharArray(Length);
			ANSIToUTF8(str, utf8String, Length, false);
			strcat_t(queryStr, "\"", utf8String, addComma ? "\"," : "\"");
			deleteCharArray(utf8String);
		}
		else
		{
			strcat_t(queryStr, "\"", addComma ? "\"," : "\"");
		}
	}
	static void sqlAddInt(char* queryStr, uint size, int value, bool addComma = true);
	template<size_t Length>
	static void sqlAddInt(Array<Length>& queryStr, int value, bool addComma = true)
	{
		INT_STR(temp, value);
		if (addComma)
		{
			strcat_t(queryStr, temp.toString(), ",");
		}
		else
		{
			strcat_s(queryStr, temp.toString());
		}
	}
	static void sqlAddUInt(char* queryStr, uint size, uint value, bool addComma = true);
	template<size_t Length>
	static void sqlAddUInt(Array<Length>& queryStr, uint value, bool addComma = true)
	{
		UINT_STR(temp, value);
		if (addComma)
		{
			strcat_t(queryStr, temp.toString(), ",");
		}
		else
		{
			strcat_s(queryStr, temp.toString());
		}
	}
	static void sqlAddFloat(char* queryStr, uint size, float value, bool addComma = true);
	template<size_t Length>
	static void sqlAddFloat(Array<Length>& queryStr, float value, bool addComma = true)
	{
		FLOAT_STR(temp, value);
		if (addComma)
		{
			strcat_t(queryStr, temp.toString(), ",");
		}
		else
		{
			strcat_s(queryStr, temp.toString());
		}
	}
	static void sqlAddULLong(char* queryStr, uint size, ullong value, bool addComma = true);
	template<size_t Length>
	static void sqlAddULLong(Array<Length>& queryStr, ullong value, bool addComma = true)
	{
		ULLONG_STR(temp, value);
		if (addComma)
		{
			strcat_t(queryStr, temp.toString(), ",");
		}
		else
		{
			strcat_s(queryStr, temp.toString());
		}
	}
	static void sqlAddLLong(char* queryStr, uint size, llong value, bool addComma = true);
	template<size_t Length>
	static void sqlAddLLong(Array<Length>& queryStr, llong value, bool addComma = true)
	{
		LLONG_STR(temp, value);
		if (addComma)
		{
			strcat_t(queryStr, temp.toString(), ",");
		}
		else
		{
			strcat_s(queryStr, temp.toString());
		}
	}
	template<size_t Length>
	static void sqlAddBytes(Array<Length>& queryStr, const Vector<byte>& byteArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(byteArray.size());
		char* charArray = newCharArray(arrayLen);
		bytesToString(charArray, arrayLen, byteArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddUShorts(Array<Length>& queryStr, const Vector<ushort>& ushortArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(ushortArray.size());
		char* charArray = newCharArray(arrayLen);
		ushortsToString(charArray, arrayLen, ushortArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddInts(Array<Length>& queryStr, const Vector<int>& intArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(intArray.size());
		char* charArray = newCharArray(arrayLen);
		intsToString(charArray, arrayLen, intArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddUInts(Array<Length>& queryStr, const Vector<uint>& intArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(intArray.size());
		char* charArray = newCharArray(arrayLen);
		uintsToString(charArray, arrayLen, intArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddFloats(Array<Length>& queryStr, const Vector<float>& floatArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(floatArray.size());
		char* charArray = newCharArray(arrayLen);
		floatsToString(charArray, arrayLen, floatArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddULLongs(Array<Length>& queryStr, const Vector<ullong>& longArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(longArray.size());
		char* charArray = newCharArray(arrayLen);
		ullongsToString(charArray, arrayLen, longArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlAddLLongs(Array<Length>& queryStr, const Vector<llong>& longArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(longArray.size());
		char* charArray = newCharArray(arrayLen);
		llongsToString(charArray, arrayLen, longArray);
		sqlAddString(queryStr, charArray, addComma);
		deleteCharArray(charArray);
	}
	// where条件相关
	template<size_t Length>
	static void sqlConditionString(Array<Length>& condition, const char* col, const char* str)
	{
		checkSQLString(str);
		strcat_t(condition, col, "=\"", str, "\"");
	}
	template<size_t Length>
	static void sqlConditionString(Array<Length>& condition, const char* col, const char* str, const char* relationalOperator, const char* logicalOperator)
	{
		checkSQLString(str);
		strcat_t(condition, col, relationalOperator, "\"", str, "\"", logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionStringUTF8(Array<Length>& condition, const char* col, const char* str)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(condition, col, "=\"", utf8String, "\"");
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlConditionStringUTF8(Array<Length>& condition, const char* col, const char* str, const char* relationalOperator, const char* logicalOperator)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(condition, col, relationalOperator, "\"", utf8String, "\"", logicalOperator);
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlConditionStringLike(Array<Length>& condition, const char* col, const char* str)
	{
		checkSQLString(str);
		strcat_t(condition, col, " LIKE \"%", str, "%\"");
	}
	template<size_t Length>
	static void sqlConditionStringLike(Array<Length>& condition, const char* col, const char* str, const char* logicalOperator)
	{
		checkSQLString(str);
		strcat_t(condition, col, " LIKE \"%", str, "%\"", logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionStringLike(Array<Length>& condition, const char* col, const char* str, const char* logicalOperator, const char* prev, const char* end)
	{
		checkSQLString(str);
		strcat_t(condition, col, " LIKE \"", prev, str, end, "\"", logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionStringLikeUTF8(Array<Length>& condition, const char* col, const char* str)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(condition, col, " LIKE \"%", utf8String, "%\"");
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlConditionStringLikeUTF8(Array<Length>& condition, const char* col, const char* str, const char* logicalOperator)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(condition, col, " LIKE \"%", utf8String, "%\"", logicalOperator);
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlConditionStringLikeUTF8(Array<Length>& condition, const char* col, const char* str, const char* logicalOperator, const char* prev, const char* end)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(condition, col, " LIKE \"", prev, utf8String, end, "\"", logicalOperator);
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlConditionInt(Array<Length>& condition, const char* col, int value)
	{
		INT_STR(temp, value);
		strcat_t(condition, col, "=", temp.toString());
	}
	template<size_t Length>
	static void sqlConditionInt(Array<Length>& condition, const char* col, int value, const char* relationalOperator)
	{
		INT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString());
	}
	template<size_t Length>
	static void sqlConditionInt(Array<Length>& condition, const char* col, int value, const char* relationalOperator, const char* logicalOperator)
	{
		INT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString(), logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionUInt(Array<Length>& condition, const char* col, uint value)
	{
		UINT_STR(temp, value);
		strcat_t(condition, col, "=", temp.toString());
	}
	template<size_t Length>
	static void sqlConditionUInt(Array<Length>& condition, const char* col, uint value, const char* relationalOperator)
	{
		UINT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString());
	}
	template<size_t Length>
	static void sqlConditionUInt(Array<Length>& condition, const char* col, uint value, const char* relationalOperator, const char* logicalOperator)
	{
		UINT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString(), logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionFloat(Array<Length>& condition, const char* col, float value)
	{
		FLOAT_STR(temp, value);
		strcat_t(condition, col, "=", temp.toString());
	}
	template<size_t Length>
	static void sqlConditionFloat(Array<Length>& condition, const char* col, float value, const char* relationalOperator)
	{
		FLOAT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString());
	}
	template<size_t Length>
	static void sqlConditionFloat(Array<Length>& condition, const char* col, float value, const char* relationalOperator, const char* logicalOperator)
	{
		FLOAT_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString(), logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionULLong(Array<Length>& condition, const char* col, ullong value)
	{
		ULLONG_STR(temp, value);
		strcat_t(condition, col, "=", temp.toString());
	}
	template<size_t Length>
	static void sqlConditionULLong(Array<Length>& condition, const char* col, ullong value, const char* relationalOperator)
	{
		ULLONG_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString());
	}
	template<size_t Length>
	static void sqlConditionULLong(Array<Length>& condition, const char* col, ullong value, const char* relationalOperator, const char* logicalOperator)
	{
		ULLONG_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString(), logicalOperator);
	}
	template<size_t Length>
	static void sqlConditionLLong(Array<Length>& condition, const char* col, llong value)
	{
		LLONG_STR(temp, value);
		strcat_t(condition, col, "=", temp.toString());
	}
	template<size_t Length>
	static void sqlConditionLLong(Array<Length>& condition, const char* col, llong value, const char* relationalOperator)
	{
		LLONG_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString());
	}
	template<size_t Length>
	static void sqlConditionLLong(Array<Length>& condition, const char* col, llong value, const char* relationalOperator, const char* logicalOperator)
	{
		LLONG_STR(temp, value);
		strcat_t(condition, col, relationalOperator, temp.toString(), logicalOperator);
	}
	// update相关
	static void sqlUpdateString(char* updateInfo, uint size, const char* col, const char* str, bool addComma = true);
	static void sqlUpdateStringUTF8(char* updateInfo, uint size, const char* col, const char* str, bool addComma = true);
	template<size_t Length>
	static void sqlUpdateStringUTF8(Array<Length>& updateInfo, const char* col, const char* str, bool addComma = true)
	{
		checkSQLString(str);
		char* utf8String = newCharArray(Length);
		ANSIToUTF8(str, utf8String, Length, false);
		strcat_t(updateInfo, col, " = \"", utf8String, addComma ? "\"," : "\"");
		deleteCharArray(utf8String);
	}
	template<size_t Length>
	static void sqlUpdateString(Array<Length>& updateInfo, const char* col, const char* str, bool addComma = true)
	{
		checkSQLString(str);
		strcat_t(updateInfo, col, " = \"", str, addComma ? "\"," : "\"");
	}
	template<size_t Length>
	static void sqlUpdateString(Array<Length>& updateInfo, const char* col, const char* str, int strLength, bool addComma = true)
	{
		checkSQLString(str);
		strcat_t(updateInfo, col, " = \"");
		strcat_s(updateInfo, str, strLength);
		strcat_t(updateInfo, addComma ? "\"," : "\"");
	}
	template<size_t Length>
	static void sqlUpdateBool(Array<Length>& updateInfo, const char* col, bool value, bool addComma = true)
	{
		// bool当成int来存储
		int intValue = value ? 1 : 0;
		INT_STR(temp, intValue);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateInt(Array<Length>& updateInfo, const char* col, int value, bool addComma = true)
	{
		INT_STR(temp, value);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateUInt(Array<Length>& updateInfo, const char* col, uint value, bool addComma = true)
	{
		UINT_STR(temp, value);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateFloat(Array<Length>& updateInfo, const char* col, float value, bool addComma = true)
	{
		FLOAT_STR(temp, value);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateULLong(Array<Length>& updateInfo, const char* col, ullong value, bool addComma = true)
	{
		ULLONG_STR(temp, value);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateLLong(Array<Length>& updateInfo, const char* col, llong value, bool addComma = true)
	{
		LLONG_STR(temp, value);
		if (addComma)
		{
			strcat_t(updateInfo, col, " = ", temp.toString(), ",");
		}
		else
		{
			strcat_t(updateInfo, col, " = ", temp.toString());
		}
	}
	template<size_t Length>
	static void sqlUpdateBytes(Array<Length>& updateInfo, const char* col, const Vector<byte>& byteArray, bool addComma = true)
	{
		int arrayLen = 4 * greaterPower2(byteArray.size());
		char* charArray = newCharArray(arrayLen);
		bytesToString(charArray, arrayLen, byteArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateUShorts(Array<Length>& updateInfo, const char* col, const Vector<ushort>& ushortArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(ushortArray.size());
		char* charArray = newCharArray(arrayLen);
		ushortsToString(charArray, arrayLen, ushortArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateInts(Array<Length>& updateInfo, const char* col, const Vector<int>& intArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(intArray.size());
		char* charArray = newCharArray(arrayLen);
		intsToString(charArray, arrayLen, intArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateUInts(Array<Length>& updateInfo, const char* col, const Vector<uint>& intArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(intArray.size());
		char* charArray = newCharArray(arrayLen);
		uintsToString(charArray, arrayLen, intArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateFloats(Array<Length>& updateInfo, const char* col, const Vector<float>& floatArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(floatArray.size());
		char* charArray = newCharArray(arrayLen);
		floatsToString(charArray, arrayLen, floatArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateULLongs(Array<Length>& updateInfo, const char* col, const Vector<ullong>& longArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(longArray.size());
		char* charArray = newCharArray(arrayLen);
		ullongsToString(charArray, arrayLen, longArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	template<size_t Length>
	static void sqlUpdateLLongs(Array<Length>& updateInfo, const char* col, const Vector<llong>& longArray, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(longArray.size());
		char* charArray = newCharArray(arrayLen);
		llongsToString(charArray, arrayLen, longArray);
		updateString(updateInfo, col, charArray, addComma);
		deleteCharArray(charArray);
	}
	static uint base64_encode(const byte* text, uint text_len, byte* encode);
	static uint base64_decode(const byte* code, uint code_len, byte* plain);
protected:
	static void initIntToString();
	static uint greaterPower2(uint value);
	static char* newCharArray(uint length) { return new char[length]; }
	static void deleteCharArray(char* charArray) { delete[] charArray; }
public:
	static Array<20000, string> mIntString;
	static const Array<11, llong> POWER_INT_10;
	static const Array<19, llong> POWER_LLONG_10;
	static const char BOM[4];
	static byte alphabet_map[];
	static byte reverse_map[];
};

#endif