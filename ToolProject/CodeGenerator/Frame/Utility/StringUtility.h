#ifndef _STRING_UTILITY_H_
#define _STRING_UTILITY_H_

#include "BinaryUtility.h"

class StringUtility : public BinaryUtility
{
public:
	static string removeStartString(const string& fileName, const string& startStr);
	static string removeSuffix(const string& str);
	static int strlength(const char* str, const int maxLength)
	{
		FOR_I(maxLength)
		{
			if (str[i] == '\0')
			{
				return i;
			}
		}
		return maxLength;
	}
	static int strlength(const char* str)
	{
		int index = 0;
		while (true)
		{
			if (str[index] == '\0')
			{
				return index;
			}
			// 当字符串长度超过1MB时,认为是错误的字符串
			if (++index >= 1024 * 1024)
			{
				ERROR("字符串长度太长");
				break;
			}
		}
		return 0;
	}
	// 去掉从开始出现的连续指定字符
	static void removeStartAll(string& stream, char key);
	// 去掉第一个出现的指定字符
	static void removeStart(string& stream, char key);
	// 去掉最后出现的连续指定字符
	static void removeLastAll(string& stream, char key);
	// 去掉最后一个出现的指定字符
	static void removeLast(string& stream, char key);
	// 去掉最后一个逗号
	static void removeLastComma(string& stream);
	// 如果以key结尾,则移除此字符
	static void removeEnd(string& str, char key);
	// 查找str中指定key的数量
	static int findCharCount(const string& str, char key);
	static int findFirstNonEmptyChar(const string& str);
	static string getFileName(string str);
	static string getFileNameNoSuffix(string str, bool removePath);
	static string getFilePath(const string& dir);
	static string getFileSuffix(const string& fileName);
	static string replaceSuffix(const string& fileName, const string& suffix);
	// 获得字符串最后不是数字的下标
	static int getLastNotNumberPos(const string& str);
	// 获得字符串结尾的数字
	static int getLastNumber(const string& str);
	// 获得去除末尾数字以后的字符串
	static string getNotNumberSubString(string str);
	// 将str中的[begin,end)替换为reStr
	template<uint Length>
	static void replace(array<char, Length>& str, int begin, int end, const char* reStr)
	{
		replace(str.data(), Length, begin, end, reStr);
	}
	static void replace(char* str, int strBufferSize, int begin, int end, const char* reStr);
	static void replace(string& str, int begin, int end, const string& reStr);
	template<uint Length>
	static void replaceAll(array<char, Length>& str, const char* key, const char* newWords)
	{
		replaceAll(str.data(), Length, key, newWords);
	}
	static void replaceAll(char* str, int strBufferSize, const char* key, const char* newWords);
	static void replaceAll(string& str, const string& key, const string& newWords);
	static void removeAll(string& str, char value);
	static void removeAll(string& str, char value0, char value1);
	static void split(const char* str, const char* key, myVector<string>& vec, bool removeEmpty = true);
	static uint split(const char* str, const char* key, string* stringBuffer, uint bufferSize, bool removeEmpty = true);
	template<uint Length>
	static uint split(const char* str, const char* key, array<string, Length>& stringBuffer, bool removeEmpty = true, bool showError = true)
	{
		int startPos = 0;
		int keyLen = (int)strlen(key);
		int sourceLen = (int)strlen(str);
		const int STRING_BUFFER = 1024;
		char curString[STRING_BUFFER];
		int devidePos = -1;
		uint curCount = 0;
		while (true)
		{
			bool ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				if (devidePos - startPos >= STRING_BUFFER)
				{
					ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节, 字符串:" + str + ", key:" + key);
					return 0;
				}
				MEMCPY(curString, STRING_BUFFER, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				if (sourceLen - startPos >= STRING_BUFFER)
				{
					ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节, 字符串:" + str + ", key:" + key);
					return 0;
				}
				MEMCPY(curString, STRING_BUFFER, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 放入列表
			if (curString[0] != '\0' || !removeEmpty)
			{
				if (curCount >= Length)
				{
					if (showError)
					{
						ERROR("string buffer is too small! bufferSize:" + intToString(Length));
					}
					else
					{
						break;
					}
				}
				stringBuffer[curCount++] = string(curString);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	// 基础数据类型转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string boolToString(bool value) { return value ? "true" : "false"; }
	static void _itoa_s(int value, char* charArray, uint size);
	template<uint Length>
	static void _itoa_s(int value, array<char, Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return;
		}
		int sign = 1;
		if (value < 0)
		{
			value = -value;
			sign = -1;
		}
		static array<ullong, 11> power{ 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000, 10000000000 };
		if ((ullong)value > power[power.size() - 1])
		{
			ERROR("int value is too large:" + ullongToString((ullong)value));
			return;
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
			if ((ullong)value < power[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)((ullong)value % power[index + 1] / power[index]);
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
			uint lengthToHead = Length - index;
			FOR_I(index)
			{
				charArray[i + 1] = charArray[i + lengthToHead] + '0';
			}
			charArray[0] = '-';
			charArray[index + 1] = '\0';
		}
	}
	static void _i64toa_s(const ullong& value, char* charArray, uint size);
	template<uint Length>
	static void _i64toa_s(const ullong& value, array<char, Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return;
		}
		static array<ullong, 20> power =
		{
			1ul,
			10ul,
			100ul,
			1000ul,
			10000ul,
			100000ul,
			1000000ul,
			10000000ul,
			100000000ul,
			1000000000ul,
			10000000000ul,
			100000000000ul,
			1000000000000ul,
			10000000000000ul,
			100000000000000ul,
			1000000000000000ul,
			10000000000000000ul,
			100000000000000000ul,
			1000000000000000000ul,
			10000000000000000000ul
		};
		if (value > power[power.size() - 1])
		{
			ERROR("long long value is too large");
			return;
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
			if (value < power[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)(value % power[index + 1] / power[index]);
			++index;
		}
		// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
		uint lengthToHead = Length - index;
		FOR_I(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
	}
	static void strcat_s(char* destBuffer, uint size, const char* source);
	template<uint Length>
	static void strcat_s(array<char, Length>& destBuffer, const char* source)
	{
		FOR_I(Length)
		{
			// 找到字符串的末尾
			if (destBuffer[i] == '\0')
			{
				uint index = 0;
				while (true)
				{
					if (index + i >= Length)
					{
						ERROR("strcat_s buffer is too small");
						break;
					}
					destBuffer[i + index] = source[index];
					if (source[index] == '\0')
					{
						break;
					}
					++index;
				}
				break;
			}
		}
	}
	static void strcpy_s(char* destBuffer, uint size, const char* source);
	template<uint Length>
	static void strcpy_s(array<char, Length>& destBuffer, const char* source)
	{
		uint index = 0;
		while (true)
		{
			if (index >= Length)
			{
				ERROR("strcat_s buffer is too small");
				break;
			}
			destBuffer[index] = source[index];
			if (source[index] == '\0')
			{
				break;
			}
			++index;
		}
	}
	// 将source拼接到destBuffer后面
	static void strcat_s(char* destBuffer, int size, const char* source);
	static void strcat_s(char* destBuffer, int size, const char* source, int length);
	template<int Length>
	static void strcat_s(Array<Length>& destBuffer, const string& source)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source);
		destBuffer[destIndex + (int)source.length()] = '\0';
	}
	template<int Length>
	static void strcat_s(Array<Length>& destBuffer, const string& source, const int length)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	static void strcat_s(Array<Length>& destBuffer, const char* source)
	{
		const int destIndex = destBuffer.length();
		const int length = strlength(source);
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	static void strcat_s(Array<Length>& destBuffer, const char* source, const int length)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	static void strcat_s(Array<Length>& destBuffer, const char** sourceArray, const int sourceCount)
	{
		int destIndex = destBuffer.length();
		FOR_I(sourceCount)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			const int length = strlength(curSource);
			destBuffer.copy(destIndex, curSource, length);
			destIndex += length;
		}
		destBuffer[destIndex] = '\0';
	}
	template<int SourceLength>
	static void strcat_s(char* destBuffer, const int destSize, const Array<SourceLength, const char*>& sourceArray)
	{
		int destIndex = strlength(destBuffer, destSize);
		FOR_I(SourceLength)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			const int length = strlength(curSource);
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
	static void strcat_t(char* destBuffer, const int destSize, TypeList&&... params)
	{
		strcat_s(destBuffer, destSize, Array<sizeof...(params), const char*>{ forward<TypeList>(params)... });
	}
	template<int Length, int SourceLength>
	static void strcat_s(Array<Length>& destBuffer, const Array<SourceLength, const char*>& sourceArray)
	{
		int destIndex = destBuffer.length();
		FOR_I(SourceLength)
		{
			const char* curSource = sourceArray[i];
			if (curSource == nullptr)
			{
				continue;
			}
			const int length = strlength(curSource);
			destBuffer.copy(destIndex, curSource, length);
			destIndex += length;
		}
		destBuffer[destIndex] = '\0';
	}
	template<int Length, typename... TypeList>
	static void strcat_t(Array<Length>& destBuffer, TypeList&&... params)
	{
		strcat_s(destBuffer, Array<sizeof...(params), const char*>{ forward<TypeList>(params)... });
	}
	static void strcpy_s(char* destBuffer, int size, const char* source);
	// 以string类型返回count个0
	static string zeroString(int zeroCount);
	template<int Length>
	static void zeroString(Array<Length>& charArray, const int zeroCount)
	{
		if ((int)Length <= zeroCount)
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
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	static string intToString(int value, uint limitLen = 0);
	// 传入存放字符串的数组,速度较快
	static void intToString(char* charArray, uint size, int value, uint limitLen = 0);
	template<uint Length>
	static void intToString(array<char, Length>& charArray, int value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			_itoa_s(value, charArray);
		}
		else
		{
			// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
			array<char, 16> temp;
			_itoa_s(value, temp);
			// 判断是否需要在前面补0
			if (limitLen > 0)
			{
				uint len = (uint)strlen(temp._Elems);
				if (limitLen > len)
				{
					// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
					array<char, 16> zeroArray;
					zeroString(zeroArray, limitLen - len);
					STRCAT2(charArray, zeroArray._Elems, temp._Elems);
					return;
				}
			}
			strcpy_s(charArray, temp._Elems);
		}
	}
	// 返回string类型的数字字符串,速度较慢
	static string ullongToString(const ullong& value, uint limitLen = 0);
	// 传入存放字符串的数组,速度较快
	static void ullongToString(char* charArray, uint size, const ullong& value, uint limitLen = 0);
	template<uint Length>
	static void ullongToString(array<char, Length>& charArray, const ullong& value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			_i64toa_s(value, charArray);
		}
		else
		{
			array<char, 32> temp;
			_i64toa_s(value, temp);
			// 判断是否需要在前面补0
			if (limitLen > 0)
			{
				uint len = (uint)strlen(temp._Elems);
				if (limitLen > len)
				{
					array<char, 16> zeroArray;
					zeroString(zeroArray, limitLen - len);
					STRCAT2(charArray, zeroArray._Elems, temp._Elems);
					return;
				}
			}
			strcpy_s(charArray, temp._Elems);
		}
	}
	// precision为精度,保留的小数的位数,removeZero为是否去除末尾无用的0,速度较慢
	static string floatToStringExtra(float f, uint precision = 4, bool removeTailZero = true);
	// 将浮点数转换为字符串,速度较快
	static string floatToString(float f);
	// 将浮点数转换为字符串并存储在charArry中,速度最快
	static void floatToString(char* charArray, uint size, float f);
	template<uint Length>
	static void floatToString(array<char, Length>& charArray, float f)
	{
		SPRINTF(charArray._Elems, Length, "%f", f);
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
			bool findEnd = false;
			FOR_I(strLen)
			{
				uint index = strLen - 1 - i;
				// 如果找到了小数点仍然没有找到一个不为'0'的字符,则从小数点部分截断字符串
				if (index == (uint)dotPos)
				{
					charArray[dotPos] = '\0';
					break;
				}
				// 找到小数点后的从后往前的第一个不为'0'的字符,从此处截断字符串
				if (charArray[index] != '0' && index + 1 < strLen)
				{
					charArray[index + 1] = '\0';
					break;
				}
			}
		}
	}
	static string bytesToString(const char* buffer, uint length);
	static string vector2ToString(const Vector2& vec, const char* seperate = ",");
	static void vector2ToString(char* buffer, uint bufferSize, const Vector2& vec, const char* seperate = ",");
	template<uint Length>
	static void vector2ToString(array<char, Length>& buffer, const Vector2& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_TO_STRING(xStr, vec.x);
		FLOAT_TO_STRING(yStr, vec.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	static string vector3ToString(const Vector3& vec, const char* seperate = ",");
	static void vector3ToString(char* buffer, uint bufferSize, const Vector3& vec, const char* seperate = ",");
	template<uint Length>
	static void vector3ToString(array<char, Length>& buffer, const Vector3& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_TO_STRING(xStr, vec.x);
		FLOAT_TO_STRING(yStr, vec.y);
		FLOAT_TO_STRING(zStr, vec.z);
		STR_APPEND5(buffer, xStr, seperate, yStr, seperate, zStr);
	}	
	static string vector2IntToString(const Vector2Int& value, const char* seperate = ",");
	static void vector2IntToString(char* buffer, uint bufferSize, const Vector2Int& value, const char* seperate = ",");
	template<uint Length>
	static void vector2IntToString(array<char, Length>& buffer, const Vector2Int& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_TO_STRING(xStr, value.x);
		INT_TO_STRING(yStr, value.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	static string vector2UShortToString(const Vector2UShort& value, const char* seperate = ",");
	static void vector2UShortToString(char* buffer, uint bufferSize, const Vector2UShort& value, const char* seperate = ",");
	template<uint Length>
	static void vector2UShortToString(array<char, Length>& buffer, const Vector2UShort& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_TO_STRING(xStr, value.x);
		INT_TO_STRING(yStr, value.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	static void stringToBytes(const string& str, myVector<byte>& valueList, const char* seperate = ",");
	static int stringToBytes(const char* str, byte* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToBytes(const char* str, Array<Length, byte>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int keyLen = strlength(seperate);
		const int sourceLen = strlength(str);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.str());
		}
		return curCount;
	}
	static void stringToUShorts(const string& str, myVector<ushort>& valueList, const char* seperate = ",");
	static int stringToUShorts(const char* str, ushort* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToUShorts(const char* str, Array<Length, ushort>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.str());
		}
		return curCount;
	}
	static void stringToInts(const string& str, myVector<int>& valueList, const char* seperate = ",");
	static int stringToInts(const char* str, int* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToInts(const string& str, Array<Length, int>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.str());
		}
		return curCount;
	}
	static void stringToUInts(const string& str, myVector<uint>& valueList, const char* seperate = ",");
	static int stringToUInts(const char* str, uint* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToUInts(const char* str, Array<Length, uint>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("uint buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.str());
		}
		return curCount;
	}
	static void stringToULLongs(const char* str, myVector<ullong>& valueList, const char* seperate = ",");
	static int stringToULLongs(const char* str, ullong* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToULLongs(const char* str, Array<Length, ullong>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("ullong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToULLong(curString.str());
		}
		return curCount;
	}
	static void stringToLLongs(const char* str, myVector<llong>& valueList, const char* seperate = ",");
	static void stringToLLongs(const string& str, myVector<llong>& valueList, const char* seperate = ",");
	static int stringToLLongs(const char* str, llong* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	static int stringToLLongs(const string& str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = destOffset;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					LOG("llong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToLLong(curString.str());
		}
		return curCount;
	}
	template<int Length>
	static int stringToLLongs(const char* str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = destOffset;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("llong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToLLong(curString.str());
		}
		return curCount;
	}
	static void stringToFloats(const string& str, myVector<float>& valueList, const char* seperate = ",");
	template<int Length>
	static int stringToFloats(const char* str, Array<Length, float>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= (int)Length)
			{
				if (showError)
				{
					ERROR("float buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToFloat(curString.str());
		}
		return curCount;
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型
	//-----------------------------------------------------------------------------------------------------------------------------
	static bool stringToBool(const string& str) { return str == "True" || str == "true"; }
	static bool stringToBool(const char* str) { return str == "True" || str == "true"; }
	static int stringToInt(const string& str) { return atoi(str.c_str()); }
	static int stringToInt(const char* str) { return atoi(str); }
	static ullong stringToULLong(const string& str) { return atoll(str.c_str()); }
	static ullong stringToULLong(const char* str) { return atoll(str); }
	static llong stringToLLong(const string& str) { return atoll(str.c_str()); }
	static llong stringToLLong(const char* str) { return atoll(str); }
	static float stringToFloat(const string& str) { return (float)atof(str.c_str()); }
	static float stringToFloat(const char* str) { return (float)atof(str); }
	static Vector2 stringToVector2(const string& str, const char* seperate = ",");
	static Vector2Int stringToVector2Int(const string& str, const char* seperate = ",");
	static Vector2UShort stringToVector2UShort(const string& str, const char* seperate = ",");
	static Vector2Short stringToVector2Short(const string& str, const char* seperate = ",");
	static Vector3 stringToVector3(const string& str, const char* seperate = ",");
	//-----------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型数组转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string ullongArrayToString(ullong* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void ullongArrayToString(char* buffer, uint bufferSize, ullong* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void ullongArrayToString(array<char, Length>& buffer, ullong* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 32> temp;
		FOR_I(count)
		{
			ullongToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string uintArrayToString(uint* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void uintArrayToString(char* buffer, uint bufferSize, uint* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void uintArrayToString(array<char, Length>& buffer, uint* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 16> temp;
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string byteArrayToString(byte* valueList, uint count, uint limitLen = 0, const char* seperate = ",");// 将byte数组当成整数数组转换为字符串,并非直接将byte数组转为字符串
	static void byteArrayToString(char* buffer, uint bufferSize, byte* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void byteArrayToString(array<char, Length>& buffer, byte* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 4> temp;
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string ushortArrayToString(ushort* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void ushortArrayToString(char* buffer, uint bufferSize, ushort* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void ushortArrayToString(array<char, Length>& buffer, ushort* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 8> temp;
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string intArrayToString(int* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void intArrayToString(char* buffer, uint bufferSize, int* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void intArrayToString(array<char, Length>& buffer, int* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 16> temp;
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string floatArrayToString(float* valueList, uint count, const char* seperate = ",");
	static void floatArrayToString(char* buffer, uint bufferSize, float* valueList, uint count, const char* seperate = ",");
	template<uint Length>
	static void floatArrayToString(array<char, Length>& buffer, float* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		array<char, 16> temp;
		FOR_I(count)
		{
			floatToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp._Elems, seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp._Elems);
			}
		}
	}
	static string stringArrayToString(string* strList, uint stringCount, const char* seperate = ",");
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型数组
	//-----------------------------------------------------------------------------------------------------------------------------
	static void stringToByteArray(const string& str, myVector<byte>& valueList, const char* seperate = ",");
	static uint stringToByteArray(const char* str, byte* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToByteArray(const char* str, array<byte, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 4;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount >= Length)
				{
					if (showError)
					{
						ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToInt(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	static void stringToUShortArray(const string& str, myVector<ushort>& valueList, const char* seperate = ",");
	static uint stringToUShortArray(const char* str, ushort* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToUShortArray(const char* str, array<ushort, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 8;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount >= Length)
				{
					if (showError)
					{
						ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToInt(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	static void stringToIntArray(const string& str, myVector<int>& valueList, const char* seperate = ",");
	static uint stringToIntArray(const char* str, int* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToIntArray(const char* str, array<int, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 16;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount >= Length)
				{
					if (showError)
					{
						ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToInt(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	static void stringToUIntArray(const string& str, myVector<uint>& valueList, const char* seperate = ",");
	static uint stringToUIntArray(const char* str, uint* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToUIntArray(const char* str, array<uint, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 16;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为长整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount >= Length)
				{
					if (showError)
					{
						ERROR("uint buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToInt(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	static void stringToULLongArray(const string& str, myVector<ullong>& valueList, const char* seperate = ",");
	static uint stringToULLongArray(const char* str, ullong* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToULLongArray(const char* str, array<ullong, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 32;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为长整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount > Length)
				{
					if (showError)
					{
						ERROR("ullong buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToULLong(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	static void stringToFloatArray(const string& str, myVector<float>& valueList, const char* seperate = ",");
	static uint stringToFloatArray(const char* str, float* buffer, uint bufferSize, const char* seperate = ",");
	template<uint Length>
	static uint stringToFloatArray(const char* str, array<float, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 32;
		array<char, BUFFER_SIZE> curString;
		int devidePos = -1;
		while (true)
		{
			bool ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (ret)
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, devidePos - startPos);
				curString[devidePos - startPos] = '\0';
			}
			else
			{
				MEMCPY(curString._Elems, BUFFER_SIZE, str + startPos, sourceLen - startPos);
				curString[sourceLen - startPos] = '\0';
			}
			// 转换为长整数放入列表
			if (curString[0] != '\0')
			{
				if (curCount > Length)
				{
					if (showError)
					{
						ERROR("float buffer size is too small, bufferSize:" + intToString(Length));
					}
					break;
				}
				buffer[curCount++] = stringToFloat(curString._Elems);
			}
			if (!ret)
			{
				break;
			}
			startPos = devidePos + keyLen;
		}
		return curCount;
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 将str中的[begin,end)替换为reStr
	static string strReplace(const string& str, uint begin, uint end, const string& reStr);
	static void strReplaceAll(string& str, const char* key, const string& newWords);
	// 以string类型返回count个0
	static string zeroString(uint count);
	// 需要传入存放字符串的数组
	static void zeroString(char* charArray, uint size, uint count);
	template<uint Length>
	static void zeroString(array<char, Length>& charArray, uint count)
	{
		if (Length <= count)
		{
			ERROR("buffer is too small");
			return;
		}
		FOR_I(count)
		{
			charArray[i] = '0';
		}
		charArray[count] = '\0';
	}
	// 判断oriString是否以pattern结尾,sensitive为是否大小写敏感
	static bool endWith(const string& oriString, const string& pattern, bool sensitive = true);
	// 判断oriString是否以pattern开头,sensitive为是否大小写敏感
	static bool startWith(const string& oriString, const string& pattern, bool sensitive = true);
	// 移除字符串首部的数字
	static string removePreNumber(const string& str);
	static wstring ANSIToUnicode(const char* str);
	static void ANSIToUnicode(const char* str, wchar_t* output, int maxLength);
	static string UnicodeToANSI(const wchar_t* str);
	static void UnicodeToANSI(const wchar_t* str, char* output, int maxLength);
	static string UnicodeToUTF8(const wchar_t* str);
	static void UnicodeToUTF8(const wchar_t* str, char* output, int maxLength);
	static wstring UTF8ToUnicode(const char* str);
	static void UTF8ToUnicode(const char* str, wchar_t* output, int maxLength);
	static string ANSIToUTF8(const char* str, bool addBOM = false);
	static void ANSIToUTF8(const char* str, char* output, int maxLength, bool addBOM = false);
	static string UTF8ToANSI(const char* str, bool eraseBOM = false);
	static void UTF8ToANSI(const char* str, char* output, int maxLength, bool eraseBOM = false);
	static void removeBOM(string& str);
	static void removeBOM(char* str, int length = 0);
	// json
	static void jsonStartArray(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonEndArray(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonStartStruct(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonEndStruct(string& str, uint preTableCount = 0, bool returnLine = false);
	static void jsonAddPair(string& str, const string& name, const string& value, uint preTableCount = 0, bool returnLine = false);
	static void jsonAddObject(string& str, const string& name, const string& value, uint preTableCount = 0, bool returnLine = false);
	static char toLower(char str);
	static char toUpper(char str);
	static string toLower(const string& str);
	static string toUpper(const string& str);
	static bool isUpper(char value) { return value >= 'A' && value <= 'Z'; }
	static bool isLower(char value) { return value >= 'a' && value <= 'z'; }
	static bool isNumber(char value) { return value >= '0' && value <= '9'; }
	static void rightToLeft(string& str);
	static void leftToRight(string& str);
	// 将字符串全部转为小写再查找
	static bool findSubstrLower(const string& res, const string& sub, int* pos = nullptr, uint startIndex = 0, bool direction = true);
	static bool findSubstr(const string& res, const string& dst, int* pos = nullptr, uint startIndex = 0, bool direction = true);
	static bool findString(const char* str, const char* key, int* pos = nullptr, uint startPos = 0);
	static string checkString(const string& str, const string& valid);
	static string checkFloatString(const string& str, const string& valid = "");
	static string checkIntString(const string& str, const string& valid = "");
	static string charToHexString(byte value, bool upper = true);
	static uint getCharCount(const string& str, char key);
	// 计算字符串显示所需的宽度,一个制表符按4个字符宽度,只考虑ASCII字符
	static uint getStringWidth(const string& str);
	// str计算按照alignWidth个字符宽度对齐时,需要补充的制表符的个数
	static uint generateAlignTableCount(const string& str, int alignWidth);
	// 在oriStr后面拼接上appendStr,并且使拼接后appendStr的起始下标为alignWidth
	static void appendWithAlign(string& oriStr, const string& appendStr, int alignWidth);
	static string charArrayToHexString(byte* data, uint dataCount, bool addSpace = true, bool upper = true);
	// sql语法相关字符串处理
	// insert相关
	static void appendValueString(char* queryStr, uint size, const char* str, bool toUTF8, bool addComma = true);
	template<uint Length>
	static void appendValueString(array<char, Length>& queryStr, const char* str, bool toUTF8, bool addComma = true)
	{
		if (toUTF8)
		{
			if (addComma)
			{
				STR_APPEND3(queryStr, "\"", ANSIToUTF8(str).c_str(), "\",");
			}
			else
			{
				STR_APPEND3(queryStr, "\"", ANSIToUTF8(str).c_str(), "\"");
			}
		}
		else
		{
			if (addComma)
			{
				STR_APPEND3(queryStr, "\"", str, "\",");
			}
			else
			{
				STR_APPEND3(queryStr, "\"", str, "\"");
			}
		}
	}
	static void appendValueInt(char* queryStr, uint size, int value, bool addComma = true);
	template<uint Length>
	static void appendValueInt(array<char, Length>& queryStr, int value, bool addComma = true)
	{
		array<char, 16> temp;
		intToString(temp, value);
		if (addComma)
		{
			STR_APPEND2(queryStr, temp._Elems, ",");
		}
		else
		{
			STR_APPEND1(queryStr, temp._Elems);
		}
	}
	static void appendValueFloat(char* queryStr, uint size, float value, bool addComma = true);
	template<uint Length>
	static void appendValueFloat(array<char, Length>& queryStr, float value, bool addComma = true)
	{
		array<char, 16> temp;
		floatToString(temp, value);
		if (addComma)
		{
			STR_APPEND2(queryStr, temp._Elems, ",");
		}
		else
		{
			STR_APPEND1(queryStr, temp._Elems);
		}
	}
	static void appendValueLLong(char* queryStr, uint size, const ullong& value, bool addComma = true);
	template<uint Length>
	static void appendValueLLong(array<char, Length>& queryStr, const ullong& value, bool addComma = true)
	{
		array<char, 32> temp;
		ullongToString(temp, value);
		if (addComma)
		{
			STR_APPEND2(queryStr, temp._Elems, ",");
		}
		else
		{
			STR_APPEND1(queryStr, temp._Elems);
		}
	}
	static void appendValueByteArray(char* queryStr, uint size, byte* ushortArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendValueByteArray(array<char, Length>& queryStr, byte* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		byteArrayToString(charArray, arrayLen, ushortArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendValueUShortArray(char* queryStr, uint size, ushort* ushortArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendValueUShortArray(array<char, Length>& queryStr, ushort* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		ushortArrayToString(charArray, arrayLen, ushortArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendValueIntArray(char* queryStr, uint size, int* intArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendValueIntArray(array<char, Length>& queryStr, int* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		intArrayToString(charArray, arrayLen, intArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendValueFloatArray(char* queryStr, uint size, float* floatArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendValueFloatArray(array<char, Length>& queryStr, float* floatArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		floatArrayToString(charArray, arrayLen, floatArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendValueULLongArray(char* queryStr, uint size, ullong* longArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendValueULLongArray(array<char, Length>& queryStr, ullong* longArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		ullongArrayToString(charArray, arrayLen, longArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendValueVector2Int(char* queryStr, uint size, const Vector2Int& value, bool addComma = true);
	template<uint Length>
	static void appendValueVector2Int(array<char, Length>& queryStr, const Vector2Int& value, bool addComma = true)
	{
		array<char, 32> temp;
		vector2IntToString(temp, value);
		appendValueString(queryStr, temp._Elems, false, addComma);
	}
	static void appendValueVector2UShort(char* queryStr, uint size, const Vector2UShort& value, bool addComma = true);
	template<uint Length>
	static void appendValueVector2UShort(array<char, Length>& queryStr, const Vector2UShort& value, bool addComma = true)
	{
		array<char, 16> temp;
		vector2UShortToString(temp, value);
		appendValueString(queryStr, temp._Elems, false, addComma);
	}
	// where条件相关
	static void appendConditionString(char* condition, uint size, const char* col, const char* str, bool toUTF8, const char* operate);
	template<uint Length>
	static void appendConditionString(array<char, Length>& condition, const char* col, const char* str, bool toUTF8, const char* operate)
	{
		if (toUTF8)
		{
			if (operate == NULL)
			{
				STR_APPEND5(condition, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"");
			}
			else
			{
				STR_APPEND6(condition, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"", operate);
			}
		}
		else
		{
			if (operate == NULL)
			{
				STR_APPEND5(condition, col, " = ", "\"", str, "\"");
			}
			else
			{
				STR_APPEND6(condition, col, " = ", "\"", str, "\"", operate);
			}
		}
	}
	static void appendConditionInt(char* condition, uint size, const char* col, int value, const char* operate);
	template<uint Length>
	static void appendConditionInt(array<char, Length>& condition, const char* col, int value, const char* operate)
	{
		array<char, 16> temp;
		intToString(temp, value);
		if (operate == NULL)
		{
			STR_APPEND3(condition, col, " = ", temp._Elems);
		}
		else
		{
			STR_APPEND4(condition, col, " = ", temp._Elems, operate);
		}
	}
	static void appendConditionFloat(char* condition, uint size, const char* col, float value, const char* operate);
	template<uint Length>
	static void appendConditionFloat(array<char, Length>& condition, const char* col, float value, const char* operate)
	{
		array<char, 16> temp;
		floatToString(temp, value);
		if (operate == NULL)
		{
			STR_APPEND3(condition, col, " = ", temp._Elems);
		}
		else
		{
			STR_APPEND4(condition, col, " = ", temp._Elems, operate);
		}
	}
	static void appendConditionULLong(char* condition, uint size, const char* col, const ullong& value, const char* operate);
	template<uint Length>
	static void appendConditionULLong(array<char, Length>& condition, const char* col, const ullong& value, const char* operate)
	{
		array<char, 32> temp;
		ullongToString(temp, value);
		if (operate == NULL)
		{
			STR_APPEND3(condition, col, " = ", temp._Elems);
		}
		else
		{
			STR_APPEND4(condition, col, " = ", temp._Elems, operate);
		}
	}
	// update相关
	static void appendUpdateString(char* updateInfo, uint size, const char* col, const char* str, bool toUTF8, bool addComma = true);
	template<uint Length>
	static void appendUpdateString(array<char, Length>& updateInfo, const char* col, const char* str, bool toUTF8, bool addComma = true)
	{
		if (toUTF8)
		{
			if (addComma)
			{
				STR_APPEND5(updateInfo, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\",");
			}
			else
			{
				STR_APPEND5(updateInfo, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"");
			}
		}
		else
		{
			if (addComma)
			{
				STR_APPEND5(updateInfo, col, " = ", "\"", str, "\",");
			}
			else
			{
				STR_APPEND5(updateInfo, col, " = ", "\"", str, "\"");
			}
		}
	}
	static void appendUpdateInt(char* updateInfo, uint size, const char* col, int value, bool addComma = true);
	template<uint Length>
	static void appendUpdateInt(array<char, Length>& updateInfo, const char* col, int value, bool addComma = true)
	{
		array<char, 16> temp;
		intToString(temp, value);
		if (addComma)
		{
			STR_APPEND4(updateInfo, col, " = ", temp._Elems, ",");
		}
		else
		{
			STR_APPEND3(updateInfo, col, " = ", temp._Elems);
		}
	}
	static void appendUpdateFloat(char* updateInfo, uint size, const char* col, float value, bool addComma = true);
	template<uint Length>
	static void appendUpdateFloat(array<char, Length>& updateInfo, const char* col, float value, bool addComma = true)
	{
		array<char, 16> temp;
		floatToString(temp, value);
		if (addComma)
		{
			STR_APPEND4(updateInfo, col, " = ", temp._Elems, ",");
		}
		else
		{
			STR_APPEND3(updateInfo, col, " = ", temp._Elems);
		}
	}
	static void appendUpdateULLong(char* updateInfo, uint size, const char* col, const ullong& value, bool addComma = true);
	template<uint Length>
	static void appendUpdateULLong(array<char, Length>& updateInfo, const char* col, const ullong& value, bool addComma = true)
	{
		array<char, 32> temp;
		ullongToString(temp, value);
		if (addComma)
		{
			STR_APPEND4(updateInfo, col, " = ", temp._Elems, ",");
		}
		else
		{
			STR_APPEND3(updateInfo, col, " = ", temp._Elems);
		}
	}
	static void appendUpdateByteArray(char* updateInfo, uint size, const char* col, byte* ushortArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendUpdateByteArray(array<char, Length>& updateInfo, const char* col, byte* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 4 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		byteArrayToString(charArray, arrayLen, ushortArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendUpdateUShortArray(char* updateInfo, uint size, const char* col, ushort* ushortArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendUpdateUShortArray(array<char, Length>& updateInfo, const char* col, ushort* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		ushortArrayToString(charArray, arrayLen, ushortArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendUpdateIntArray(char* updateInfo, uint size, const char* col, int* intArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendUpdateIntArray(array<char, Length>& updateInfo, const char* col, int* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		intArrayToString(charArray, arrayLen, intArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendUpdateFloatArray(char* updateInfo, uint size, const char* col, float* floatArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendUpdateFloatArray(array<char, Length>& updateInfo, const char* col, float* floatArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		floatArrayToString(charArray, arrayLen, floatArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendUpdateULLongArray(char* updateInfo, uint size, const char* col, ullong* longArray, uint count, bool addComma = true);
	template<uint Length>
	static void appendUpdateULLongArray(array<char, Length>& updateInfo, const char* col, ullong* longArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * MathUtility::getGreaterPower2(count);
		char* charArray = ArrayPool::newArray<char>(arrayLen);
		ullongArrayToString(charArray, arrayLen, longArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		ArrayPool::deleteArray(charArray);
	}
	static void appendUpdateVector2Int(char* updateInfo, uint size, const char* col, const Vector2Int& value, bool addComma = true);
	template<uint Length>
	static void appendUpdateVector2Int(array<char, Length>& updateInfo, const char* col, const Vector2Int& value, bool addComma = true)
	{
		array<char, 32> temp;
		vector2IntToString(temp, value);
		appendUpdateString(updateInfo, col, temp._Elems, false, addComma);
	}
	static void appendUpdateVector2UShort(char* updateInfo, uint size, const char* col, const Vector2UShort& value, bool addComma = true);
	template<uint Length>
	static void appendUpdateVector2UShort(array<char, Length>& updateInfo, const char* col, const Vector2UShort& value, bool addComma = true)
	{
		array<char, 16> temp;
		vector2UShortToString(temp, value);
		appendUpdateString(updateInfo, col, temp._Elems, false, addComma);
	}
protected:
	static const char BOM[4];
	static myVector<int> mTempIntList;
};

#endif