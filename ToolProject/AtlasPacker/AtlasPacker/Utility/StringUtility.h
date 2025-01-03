#ifndef _STRING_UTILITY_H_
#define _STRING_UTILITY_H_

#include "BinaryUtility.h"

class StringUtility : public BinaryUtility
{
public:
	static void initStringUtility();
	static string removeSuffix(const string& str);
	// 去掉最后一个逗号
	static void removeLastComma(string& stream);
	static string getFolderName(string str);
	static string getFileName(string str);
	static string getFileNameNoSuffix(string str, bool removePath);
	static string getFirstFolderName(const string& dir);
	static string removeFirstPath(const string& dir);
	static string replaceFolderName(const string& fileName, const string& newFolderName);
	static string getFilePath(const string& dir);
	static string getFileSuffix(const string& fileName);
	static string removeStartString(const string& fileName, const string& startStr);
	static string replaceSuffix(const string& fileName, const string& suffix) { return getFileNameNoSuffix(fileName, false) + suffix; }
	// 获得字符串最后不是数字的下标
	static int getLastNotNumberPos(const string& str);
	// 获得字符串结尾的数字
	static int getLastNumber(const string& str);
	// 获得去除末尾数字以后的字符串
	static string getNotNumberSubString(string str) { return str.substr(0, getLastNotNumberPos(str) + 1); }
	static int getLastChar(const char* str, char value);
	static int getLastNotChar(const char* str, char value);
	static void split(const char* str, const char* key, myVector<string>& vec, bool removeEmpty = true);
	static uint split(const char* str, const char* key, string* stringBuffer, uint bufferSize, bool removeEmpty = true);
	// 将str中的[begin,end)替换为reStr
	template<size_t Length>
	static void replace(array<char, Length>& str, int begin, int end, const char* reStr)
	{
		replace(str.data(), Length, begin, end, reStr);
	}
	static void replace(char* str, int strBufferSize, int begin, int end, const char* reStr);
	static void replace(string& str, int begin, int end, const string& reStr);
	template<size_t Length>
	static void replaceAll(array<char, Length>& str, const char* key, const char* newWords)
	{
		replaceAll(str.data(), Length, key, newWords);
	}
	static void replaceAll(char* str, int strBufferSize, const char* key, const char* newWords);
	static void replaceAll(string& str, const string& key, const string& newWords);
	static void removeAll(string& str, char value);
	static void removeAll(string& str, char value0, char value1);
	template<size_t Length>
	static void removeString(array<char, Length>& str, const char* subString)
	{
		int subPos = 0;
		if (!findString(str.data(), subString, &subPos, 0))
		{
			return;
		}
		// 从子字符串的位置,后面的数据覆盖前面的数据
		uint subLength = (uint)strlen(subString);
		memmove(str.data() + subPos, str.data() + subPos + subLength, Length - subLength - subPos);
	}
	// 从一个ullong数组的字符串中移除指定的value的字符串
	template<size_t Length>
	static void removeULLongsString(array<char, Length>& valueArrayString, ullong value)
	{
		ULLONG_STR(valueString, value);
		// 如果value是在最后,则只移除value字符串
		if (endWith(valueArrayString.data(), valueString))
		{
			removeString(valueArrayString, valueString);
		}
		// value不在最后,则移除value字符串加后面的逗号
		else
		{
			array<char, 32> needRemoveString{ 0 };
			STR_APPEND2(needRemoveString, valueString, ",");
			removeString(valueArrayString, needRemoveString.data());
		}
	}
	// 将value添加到一个ullong数组的字符串中
	template<size_t Length>
	static void addULLongsString(array<char, Length>& valueArrayString, ullong value)
	{
		ULLONG_CHARS(idStr, value);
		if (valueArrayString[0] != '\0')
		{
			STR_APPEND2(valueArrayString, ",", idStr.data());
		}
		else
		{
			MEMCPY(valueArrayString.data(), valueArrayString.size(), idStr.data(), idStr.size());
		}
	}
	template<size_t Length>
	static uint split(const char* str, const char* key, array<string, Length>& stringBuffer, bool removeEmpty = true, bool showError = true)
	{
		int startPos = 0;
		int keyLen = (uint)strlen(key);
		int sourceLen = (uint)strlen(str);
		const int STRING_BUFFER = 1024;
		char curString[STRING_BUFFER];
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
				//ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
				return 0;
			}
			MEMCPY(curString, STRING_BUFFER, str + startPos, devidePos - startPos);
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
					//ERROR("string buffer is too small! bufferSize:" + intToString(Length));
				}
				break;
			}
			stringBuffer[curCount++] = curString;
		}
		return curCount;
	}
	// 基础数据类型转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string boolToString(bool value) { return value ? "True" : "False"; }
	static void _itoa_s(int value, char* charArray, uint size);
	template<size_t Length>
	static void _itoa_s(int value, array<char, Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return;
		}

		// 优先查表
		if (value >= 0 && value < (int)mIntString.size())
		{
			const string& str = mIntString[value];
			uint strLength = (uint)str.length();
			if (strLength + 1 >= Length)
			{
				//ERROR("int value is too large:" + ullongToString((ullong)value));
				return;
			}
			copyArray(charArray, (char*)str.c_str(), strLength);
			charArray[strLength] = '\0';
			return;
		}

		int sign = 1;
		if (value < 0)
		{
			value = -value;
			sign = -1;
		}
		if ((ullong)value > POWER_INT_10[POWER_INT_10.size() - 1])
		{
			//ERROR("int value is too large:" + ullongToString((ullong)value));
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
				//ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if ((ullong)value < POWER_INT_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)((ullong)value % POWER_INT_10[index + 1] / POWER_INT_10[index]);
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
	static void _i64toa_s(ullong value, char* charArray, uint size);
	template<size_t Length>
	static void _i64toa_s(ullong value, array<char, Length>& charArray)
	{
		if (value == 0)
		{
			charArray[0] = '0';
			charArray[1] = '\0';
			return;
		}
		if (value > POWER_ULLONG_10[POWER_ULLONG_10.size() - 1])
		{
			//ERROR("long long value is too large");
			return;
		}
		uint index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过Length - 1
			if (index >= Length)
			{
				//ERROR("buffer is too small!");
				break;
			}
			// 将数字放入numberArray的尾部
			if (value < POWER_ULLONG_10[index])
			{
				break;
			}
			charArray[Length - 1 - index] = (int)(value % POWER_ULLONG_10[index + 1] / POWER_ULLONG_10[index]);
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
	// 将source拼接到destBuffer后面
	static void strcat_s(char* destBuffer, uint size, const char* source);
	// 将sourceArray中的所有字符串依次拼接在destBuffer后面
	static void strcat_s(char* destBuffer, uint size, const char** sourceArray, uint sourceCount);
	template<size_t Length>
	static void strcat_s(array<char, Length>& destBuffer, const char* source)
	{
		uint destIndex = (uint)-1;
		FOR_I(Length)
		{
			// 找到字符串的末尾
			if (destBuffer[i] == '\0')
			{
				destIndex = i;
				break;
			}
		}
		if (destIndex == (uint)-1)
		{
			return;
		}
		uint index = 0;
		while (true)
		{
			if (destIndex >= Length)
			{
				//ERROR("strcat_s buffer is too small");
				break;
			}
			destBuffer[destIndex] = source[index];
			if (source[index] == '\0')
			{
				break;
			}
			++index;
			++destIndex;
		}
	}
	template<size_t Length>
	static void strcat_s(array<char, Length>& destBuffer, const char** sourceArray, uint sourceCount)
	{
		uint destIndex = (uint)-1;
		FOR_I(Length)
		{
			// 找到字符串的末尾
			if (destBuffer[i] == '\0')
			{
				destIndex = i;
				break;
			}
		}
		if (destIndex == (uint)-1)
		{
			return;
		}
		FOR_I(sourceCount)
		{
			uint index = 0;
			const char* curSource = sourceArray[i];
			if (curSource == NULL)
			{
				continue;
			}
			while (true)
			{
				if (destIndex >= Length)
				{
					//ERROR("strcat_s buffer is too small");
					break;
				}
				destBuffer[destIndex] = curSource[index];
				if (curSource[index] == '\0')
				{
					break;
				}
				++index;
				++destIndex;
			}
		}
	}
	// 字符串拼接,并且指定拼接source多长的内容
	template<size_t Length>
	static void strcat_s(array<char, Length>& destBuffer, const char* source, int sourceLength)
	{
		if (source == NULL || sourceLength <= 0)
		{
			return;
		}
		uint destIndex = (uint)-1;
		FOR_I(Length)
		{
			// 找到字符串的末尾
			if (destBuffer[i] == '\0')
			{
				destIndex = i;
				break;
			}
		}
		if (destIndex == (uint)-1)
		{
			return;
		}
		int index = 0;
		while (true)
		{
			if (destIndex >= Length)
			{
				//ERROR("strcat_s buffer is too small");
				break;
			}
			if (sourceLength >= 0 && index >= sourceLength)
			{
				break;
			}
			destBuffer[destIndex] = source[index];
			if (source[index] == '\0')
			{
				break;
			}
			++index;
			++destIndex;
		}
	}
	static void strcpy_s(char* destBuffer, uint size, const char* source);
	template<size_t Length>
	static void strcpy_s(array<char, Length>& destBuffer, const char* source)
	{
		if (source == NULL)
		{
			return;
		}
		uint index = 0;
		while (true)
		{
			if (index >= Length)
			{
				//ERROR("strcat_s buffer is too small");
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
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	static string intToString(int value, uint limitLen = 0);
	// 传入存放字符串的数组,速度较快
	static void intToString(char* charArray, uint size, int value, uint limitLen = 0);
	template<size_t Length>
	static void intToString(array<char, Length>& charArray, int value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			_itoa_s(value, charArray);
			return;
		}
		// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
		array<char, 16> temp{ 0 };
		_itoa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > 0)
		{
			uint len = (uint)strlen(temp.data());
			if (limitLen > len)
			{
				// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
				array<char, 16> zeroArray{ 0 };
				zeroString(zeroArray, limitLen - len);
				STRCAT2(charArray, zeroArray.data(), temp.data());
				return;
			}
		}
		strcpy_s(charArray, temp.data());
	}
	// 返回string类型的数字字符串,速度较慢
	static string ullongToString(ullong value, uint limitLen = 0);
	// 传入存放字符串的数组,速度较快
	static void ullongToString(char* charArray, uint size, ullong value, uint limitLen = 0);
	template<size_t Length>
	static void ullongToString(array<char, Length>& charArray, ullong value, uint limitLen = 0)
	{
		if (limitLen == 0)
		{
			_i64toa_s(value, charArray);
			return;
		}
		array<char, 32> temp{ 0 };
		_i64toa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > 0)
		{
			uint len = (uint)strlen(temp.data());
			if (limitLen > len)
			{
				array<char, 16> zeroArray{ 0 };
				zeroString(zeroArray, limitLen - len);
				STRCAT2(charArray, zeroArray.data(), temp.data());
				return;
			}
		}
		strcpy_s(charArray, temp.data());
	}
	// precision为精度,保留的小数的位数,removeZero为是否去除末尾无用的0,速度较慢
	static string floatToStringExtra(float f, uint precision = 4, bool removeTailZero = true);
	// 将浮点数转换为字符串,速度较快
	static string floatToString(float f);
	// 将浮点数转换为字符串并存储在charArry中,速度最快
	static void floatToString(char* charArray, uint size, float f);
	template<size_t Length>
	static void floatToString(array<char, Length>& charArray, float f)
	{
		SPRINTF(charArray.data(), Length, "%f", f);
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
	template<size_t Length>
	static void vector2ToString(array<char, Length>& buffer, const Vector2& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_STR(xStr, vec.x);
		FLOAT_STR(yStr, vec.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	static string vector3ToString(const Vector3& vec, const char* seperate = ",");
	static void vector3ToString(char* buffer, uint bufferSize, const Vector3& vec, const char* seperate = ",");
	template<size_t Length>
	static void vector3ToString(array<char, Length>& buffer, const Vector3& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_STR(xStr, vec.x);
		FLOAT_STR(yStr, vec.y);
		FLOAT_STR(zStr, vec.z);
		STR_APPEND5(buffer, xStr, seperate, yStr, seperate, zStr);
	}	
	static string vector2IntToString(const Vector2Int& value, const char* seperate = ",");
	static void vector2IntToString(char* buffer, uint bufferSize, const Vector2Int& value, const char* seperate = ",");
	template<size_t Length>
	static void vector2IntToString(array<char, Length>& buffer, const Vector2Int& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	static string vector2UShortToString(const Vector2UShort& value, const char* seperate = ",");
	static void vector2UShortToString(char* buffer, uint bufferSize, const Vector2UShort& value, const char* seperate = ",");
	template<size_t Length>
	static void vector2UShortToString(array<char, Length>& buffer, const Vector2UShort& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		STR_APPEND3(buffer, xStr, seperate, yStr);
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型
	//-----------------------------------------------------------------------------------------------------------------------------
	static bool stringToBool(const string& str) { return str == "True" || str == "true"; }
	static bool stringToBool(const char* str) { return strcmp(str, "True") == 0 || strcmp(str, "true") == 0; }
	static int stringToInt(const string& str) { return atoi(str.c_str()); }
	static int stringToInt(const char* str) { return atoi(str); }
	static ullong stringToULLong(const string& str) { return atoll(str.c_str()); }
	static ullong stringToULLong(const char* str) { return atoll(str); }
	static float stringToFloat(const string& str) { return (float)atof(str.c_str()); }
	static float stringToFloat(const char* str) { return (float)atof(str); }
	static Vector2 stringToVector2(const string& str, const char* seperate = ",");
	static Vector2Int stringToVector2Int(const string& str, const char* seperate = ",");
	static Vector2UShort stringToVector2UShort(const string& str, const char* seperate = ",");
	static Vector3 stringToVector3(const string& str, const char* seperate = ",");
	//-----------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型数组转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	static string ullongsToString(ullong* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void ullongsToString(char* buffer, uint bufferSize, const ullong* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void ullongsToString(array<char, Length>& buffer, const ullong* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 32> temp{ 0 };
		FOR_I(count)
		{
			ullongToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string uintsToString(uint* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void uintsToString(char* buffer, uint bufferSize, uint* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void uintsToString(array<char, Length>& buffer, uint* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 16> temp{ 0 };
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string bytesToString(byte* valueList, uint count, uint limitLen = 0, const char* seperate = ",");// 将byte数组当成整数数组转换为字符串,并非直接将byte数组转为字符串
	static void bytesToString(char* buffer, uint bufferSize, byte* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void bytesToString(array<char, Length>& buffer, byte* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 4> temp{ 0 };
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string ushortsToString(ushort* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void ushortsToString(char* buffer, uint bufferSize, ushort* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void ushortsToString(array<char, Length>& buffer, ushort* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 8> temp{ 0 };
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string intsToString(int* valueList, uint count, uint limitLen = 0, const char* seperate = ",");
	static void intsToString(char* buffer, uint bufferSize, int* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void intsToString(array<char, Length>& buffer, int* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 16> temp{ 0 };
		FOR_I(count)
		{
			intToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string floatsToString(float* valueList, uint count, const char* seperate = ",");
	static void floatsToString(char* buffer, uint bufferSize, float* valueList, uint count, const char* seperate = ",");
	template<size_t Length>
	static void floatsToString(array<char, Length>& buffer, float* valueList, uint count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == NULL || count == 0)
		{
			return;
		}
		array<char, 16> temp{ 0 };
		FOR_I(count)
		{
			floatToString(temp, valueList[i]);
			if (i != count - 1)
			{
				STR_APPEND2(buffer, temp.data(), seperate);
			}
			else
			{
				STR_APPEND1(buffer, temp.data());
			}
		}
	}
	static string stringsToString(string* strList, uint stringCount, const char* seperate = ",");
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型数组
	//-----------------------------------------------------------------------------------------------------------------------------
	static void stringToBytes(const string& str, myVector<byte>& valueList, const char* seperate = ",");
	static uint stringToBytes(const char* str, byte* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToBytes(const char* str, array<byte, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		array<char, 4> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.data());
		}
		return curCount;
	}
	static void stringToUShorts(const string& str, myVector<ushort>& valueList, const char* seperate = ",");
	static uint stringToUShorts(const char* str, ushort* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToUShorts(const char* str, array<ushort, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		array<char, 8> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.data());
		}
		return curCount;
	}
	static void stringToInts(const string& str, myVector<int>& valueList, const char* seperate = ",");
	static uint stringToInts(const char* str, int* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToInts(const char* str, array<int, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		array<char, 16> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("int buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.data());
		}
		return curCount;
	}
	static void stringToUInts(const string& str, myVector<uint>& valueList, const char* seperate = ",");
	static uint stringToUInts(const char* str, uint* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToUInts(const char* str, array<uint, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		array<char, 16> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("uint buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToInt(curString.data());
		}
		return curCount;
	}
	static void stringToULLongs(const char* str, myVector<ullong>& valueList, const char* seperate = ",");
	static uint stringToULLongs(const char* str, ullong* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToULLongs(const char* str, array<ullong, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		array<char, 32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("ullong buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToULLong(curString.data());
		}
		return curCount;
	}
	static void stringToFloats(const string& str, myVector<float>& valueList, const char* seperate = ",");
	static uint stringToFloats(const char* str, float* buffer, uint bufferSize, const char* seperate = ",");
	template<size_t Length>
	static uint stringToFloats(const char* str, array<float, Length>& buffer, const char* seperate = ",", bool showError = true)
	{
		uint curCount = 0;
		uint startPos = 0;
		uint keyLen = (uint)strlen(seperate);
		uint sourceLen = (uint)strlen(str);
		const int BUFFER_SIZE = 32;
		array<char, 32> curString{ 0 };
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			MEMCPY(curString.data(), curString.size(), str + startPos, devidePos - startPos);
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
					//ERROR("float buffer size is too small, bufferSize:" + intToString(Length));
				}
				break;
			}
			buffer[curCount++] = stringToFloat(curString.data());
		}
		return curCount;
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 以string类型返回count个0
	static string zeroString(uint zeroCount);
	// 需要传入存放字符串的数组
	static void zeroString(char* charArray, uint size, uint zeroCount);
	template<size_t Length>
	static void zeroString(array<char, Length>& charArray, uint zeroCount)
	{
		if (Length <= zeroCount)
		{
			//ERROR("buffer is too small");
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
	static bool endWith(const string& oriString, const char* pattern, bool sensitive = true)
	{
		return endWith(oriString.c_str(), pattern, sensitive);
	}
	// 判断oriString是否以pattern开头,sensitive为是否大小写敏感
	static bool startWith(const char* oriString, const char* pattern, bool sensitive = true);
	static bool startWith(const string& oriString, const char* pattern, bool sensitive = true)
	{
		return startWith(oriString.c_str(), pattern, sensitive);
	}
	// 移除字符串首部的数字
	static string removePreNumber(const string& str);
	static wstring ANSIToUnicode(const char* str);
	static string UnicodeToANSI(const wchar_t* str);
	static string UnicodeToUTF8(const wchar_t* str);
	static wstring UTF8ToUnicode(const char* str);
	static string ANSIToUTF8(const char* str, bool addBOM = false);
	static string UTF8ToANSI(const char* str, bool eraseBOM = false);
	static void removeBOM(string& str);
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
		if (value >= 'A' && value <= 'Z')
		{
			return value + 'a' - 'A';
		}
		return value;
	}
	static char toUpper(char value)
	{
		if (value >= 'a' && value <= 'z')
		{
			return value - ('a' - 'A');
		}
		return value;
	}
	static void rightToLeft(string& str);
	static void leftToRight(string& str);
	// 将字符串全部转为小写再查找
	static bool findStringLower(const string& res, const string& sub, int* pos = NULL, uint startIndex = 0, bool direction = true);
	// 可指定从后或者从头查找子字符串
	static bool findString(const string& res, const string& dst, int* pos = NULL, uint startIndex = 0, bool direction = true);
	static bool findString(const char* res, const char* dst, int* pos = NULL, uint startIndex = 0, bool direction = true);
	static int findStringPos(const char* res, const char* dst, uint startIndex = 0, bool direction = true);
	static int findStringPos(const string& res, const string& dst, uint startIndex = 0, bool direction = true);
	static bool checkString(const string& str, const string& valid);
	static bool checkFloatString(const string& str, const string& valid = "");
	static bool checkIntString(const string& str, const string& valid = "");
	static string charToHexString(byte value, bool upper = true);
	static uint getCharCount(const string& str, char key);
	static uint getCharCount(const char* str, char key);
	static string charArrayToHexString(byte* data, uint dataCount, bool addSpace = true, bool upper = true);
	static byte hexCharToByte(char hex)
	{
		if (hex >= '0' && hex <= '9')
		{
			return hex - '0';
		}
		if (hex >= 'A' && hex <= 'F')
		{
			return 0x0A + hex - 'A';
		}
		return 0;
	}
	static char byteToHexChar(byte value)
	{
		if (value >= 0 && value <= 9)
		{
			return value + '0';
		}
		if (value >= 0x0A && value <= 0x0F)
		{
			return value - 0x0A + 'A';
		}
		return 0;
	}
	// sql语法相关字符串处理
	// insert相关
	static void appendValueString(char* queryStr, uint size, const char* str, bool toUTF8, bool addComma = true);
	template<size_t Length>
	static void appendValueString(array<char, Length>& queryStr, const char* str, bool toUTF8, bool addComma = true)
	{
		// 由于希望尽量不使用string对象,而且ANSIToUTF8的返回值在当前语句结束后就会立即销毁
		// const char* utf8Str = ANSIToUTF8(str).c_str();中的utf8Str无法在下一句代码中使用
		// 所以考虑到代码量和执行效率,使用if区分
		const char* end = addComma ? "\"," : "\"";
		if (toUTF8)
		{
			STR_APPEND3(queryStr, "\"", ANSIToUTF8(str).c_str(), end);
		}
		else
		{
			STR_APPEND3(queryStr, "\"", str, end);
		}
	}
	static void appendValueInt(char* queryStr, uint size, int value, bool addComma = true);
	template<size_t Length>
	static void appendValueInt(array<char, Length>& queryStr, int value, bool addComma = true)
	{
		INT_STR(temp, value);
		STR_APPEND2(queryStr, temp, addComma ? "," : NULL);
	}
	static void appendValueFloat(char* queryStr, uint size, float value, bool addComma = true);
	template<size_t Length>
	static void appendValueFloat(array<char, Length>& queryStr, float value, bool addComma = true)
	{
		FLOAT_STR(temp, value);
		STR_APPEND2(queryStr, temp, addComma ? "," : NULL);
	}
	static void appendValueULLong(char* queryStr, uint size, ullong value, bool addComma = true);
	template<size_t Length>
	static void appendValueULLong(array<char, Length>& queryStr, ullong value, bool addComma = true)
	{
		ULLONG_STR(temp, value);
		STR_APPEND2(queryStr, temp, addComma ? "," : NULL);
	}
	static void appendValueBytes(char* queryStr, uint size, byte* ushortArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueBytes(array<char, Length>& queryStr, byte* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		bytesToString(charArray, arrayLen, ushortArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueUShorts(char* queryStr, uint size, ushort* ushortArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueUShorts(array<char, Length>& queryStr, ushort* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		ushortsToString(charArray, arrayLen, ushortArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueInts(char* queryStr, uint size, int* intArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueInts(array<char, Length>& queryStr, int* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		intsToString(charArray, arrayLen, intArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueUInts(char* queryStr, uint size, uint* intArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueUInts(array<char, Length>& queryStr, uint* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		uintsToString(charArray, arrayLen, intArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueFloats(char* queryStr, uint size, float* floatArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueFloats(array<char, Length>& queryStr, float* floatArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		floatsToString(charArray, arrayLen, floatArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueULLongs(char* queryStr, uint size, ullong* longArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendValueULLongs(array<char, Length>& queryStr, ullong* longArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		ullongsToString(charArray, arrayLen, longArray, count);
		appendValueString(queryStr, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendValueVector2Int(char* queryStr, uint size, const Vector2Int& value, bool addComma = true);
	template<size_t Length>
	static void appendValueVector2Int(array<char, Length>& queryStr, const Vector2Int& value, bool addComma = true)
	{
		array<char, 32> temp{ 0 };
		vector2IntToString(temp, value);
		appendValueString(queryStr, temp.data(), false, addComma);
	}
	static void appendValueVector2UShort(char* queryStr, uint size, const Vector2UShort& value, bool addComma = true);
	template<size_t Length>
	static void appendValueVector2UShort(array<char, Length>& queryStr, const Vector2UShort& value, bool addComma = true)
	{
		array<char, 16> temp{ 0 };
		vector2UShortToString(temp, value);
		appendValueString(queryStr, temp.data(), false, addComma);
	}
	// where条件相关
	static void appendConditionString(char* condition, uint size, const char* col, const char* str, bool toUTF8, const char* relationalOperator = "=", const char* logicalOperator = NULL);
	template<size_t Length>
	static void appendConditionString(array<char, Length>& condition, const char* col, const char* str, bool toUTF8, const char* relationalOperator = "=", const char* logicalOperator = NULL)
	{
		if (toUTF8)
		{
			STR_APPEND6(condition, col, relationalOperator, "\"", ANSIToUTF8(str).c_str(), "\"", logicalOperator);
		}
		else
		{
			STR_APPEND6(condition, col, relationalOperator, "\"", str, "\"", logicalOperator);
		}
	}
	template<size_t Length>
	static void appendConditionStringLike(array<char, Length>& condition, const char* col, const char* str, bool toUTF8, const char* logicalOperator = NULL, const char* prev = "%", const char* end = "%")
	{
		if (toUTF8)
		{
			STR_APPEND7(condition, col, " LIKE \"", prev, ANSIToUTF8(str).c_str(), end, "\"", logicalOperator);
		}
		else
		{
			STR_APPEND7(condition, col, " LIKE \"", prev, str, end, "\"", logicalOperator);
		}
	}
	static void appendConditionInt(char* condition, uint size, const char* col, int value, const char* relationalOperator = "=", const char* logicalOperator = NULL);
	template<size_t Length>
	static void appendConditionInt(array<char, Length>& condition, const char* col, int value, const char* relationalOperator = "=", const char* logicalOperator = NULL)
	{
		INT_STR(temp, value);
		STR_APPEND4(condition, col, relationalOperator, temp, logicalOperator);
	}
	static void appendConditionFloat(char* condition, uint size, const char* col, float value, const char* relationalOperator = "=", const char* logicalOperator = NULL);
	template<size_t Length>
	static void appendConditionFloat(array<char, Length>& condition, const char* col, float value, const char* relationalOperator = "=", const char* logicalOperator = NULL)
	{
		FLOAT_STR(temp, value);
		STR_APPEND4(condition, col, relationalOperator, temp, logicalOperator);
	}
	static void appendConditionULLong(char* condition, uint size, const char* col, ullong value, const char* relationalOperator = "=", const char* logicalOperator = NULL);
	template<size_t Length>
	static void appendConditionULLong(array<char, Length>& condition, const char* col, ullong value, const char* relationalOperator = "=", const char* logicalOperator = NULL)
	{
		ULLONG_STR(temp, value);
		STR_APPEND4(condition, col, relationalOperator, temp, logicalOperator);
	}
	// update相关
	static void appendUpdateString(char* updateInfo, uint size, const char* col, const char* str, bool toUTF8, bool addComma = true);
	template<size_t Length>
	static void appendUpdateString(array<char, Length>& updateInfo, const char* col, const char* str, bool toUTF8, bool addComma = true)
	{
		const char* end = addComma ? "\"," : "\"";
		if (toUTF8)
		{
			STR_APPEND4(updateInfo, col, " = \"", ANSIToUTF8(str).c_str(), end);
		}
		else
		{
			STR_APPEND4(updateInfo, col, " = \"", str, end);
		}
	}
	template<size_t Length>
	static void appendUpdateString(array<char, Length>& updateInfo, const char* col, const char* str, int strLength, bool toUTF8, bool addComma = true)
	{
		if (toUTF8)
		{
			//ERROR("appendUpdateString指定长度的字符串不支持转换为UTF8格式");
			return;
		}
		STR_APPEND2(updateInfo, col, " = \"");
		strcat_s(updateInfo, str, strLength);
		strcat_s(updateInfo, addComma ? "\"," : "\"");
	}
	static void appendUpdateInt(char* updateInfo, uint size, const char* col, int value, bool addComma = true);
	template<size_t Length>
	static void appendUpdateInt(array<char, Length>& updateInfo, const char* col, int value, bool addComma = true)
	{
		INT_STR(temp, value);
		STR_APPEND4(updateInfo, col, " = ", temp, addComma ? "," : NULL);
	}
	static void appendUpdateFloat(char* updateInfo, uint size, const char* col, float value, bool addComma = true);
	template<size_t Length>
	static void appendUpdateFloat(array<char, Length>& updateInfo, const char* col, float value, bool addComma = true)
	{
		FLOAT_STR(temp, value);
		STR_APPEND4(updateInfo, col, " = ", temp, addComma ? "," : NULL);
	}
	static void appendUpdateULLong(char* updateInfo, uint size, const char* col, ullong value, bool addComma = true);
	template<size_t Length>
	static void appendUpdateULLong(array<char, Length>& updateInfo, const char* col, ullong value, bool addComma = true)
	{
		ULLONG_STR(temp, value);
		STR_APPEND4(updateInfo, col, " = ", temp, addComma ? "," : NULL);
	}
	static void appendUpdateBytes(char* updateInfo, uint size, const char* col, byte* ushortArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateBytes(array<char, Length>& updateInfo, const char* col, byte* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 4 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		bytesToString(charArray, arrayLen, ushortArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateUShorts(char* updateInfo, uint size, const char* col, ushort* ushortArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateUShorts(array<char, Length>& updateInfo, const char* col, ushort* ushortArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		ushortsToString(charArray, arrayLen, ushortArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateInts(char* updateInfo, uint size, const char* col, int* intArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateInts(array<char, Length>& updateInfo, const char* col, int* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		intsToString(charArray, arrayLen, intArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateUInts(char* updateInfo, uint size, const char* col, uint* intArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateUInts(array<char, Length>& updateInfo, const char* col, uint* intArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		uintsToString(charArray, arrayLen, intArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateFloats(char* updateInfo, uint size, const char* col, float* floatArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateFloats(array<char, Length>& updateInfo, const char* col, float* floatArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		floatsToString(charArray, arrayLen, floatArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateULLongs(char* updateInfo, uint size, const char* col, ullong* longArray, uint count, bool addComma = true);
	template<size_t Length>
	static void appendUpdateULLongs(array<char, Length>& updateInfo, const char* col, ullong* longArray, uint count, bool addComma = true)
	{
		int arrayLen = 16 * greaterPower2(count);
		char* charArray = newCharArray(arrayLen);
		ullongsToString(charArray, arrayLen, longArray, count);
		appendUpdateString(updateInfo, col, charArray, false, addComma);
		deleteCharArray(charArray);
	}
	static void appendUpdateVector2Int(char* updateInfo, uint size, const char* col, const Vector2Int& value, bool addComma = true);
	template<size_t Length>
	static void appendUpdateVector2Int(array<char, Length>& updateInfo, const char* col, const Vector2Int& value, bool addComma = true)
	{
		array<char, 32> temp{ 0 };
		vector2IntToString(temp, value);
		appendUpdateString(updateInfo, col, temp.data(), false, addComma);
	}
	static void appendUpdateVector2UShort(char* updateInfo, uint size, const char* col, const Vector2UShort& value, bool addComma = true);
	template<size_t Length>
	static void appendUpdateVector2UShort(array<char, Length>& updateInfo, const char* col, const Vector2UShort& value, bool addComma = true)
	{
		array<char, 16> temp{ 0 };
		vector2UShortToString(temp, value);
		appendUpdateString(updateInfo, col, temp.data(), false, addComma);
	}
protected:
	static uint greaterPower2(uint value);
	static char* newCharArray(uint length);
	static void deleteCharArray(char* charArray);
protected:
	static array<string, 4097> mIntString;
	static const array<ullong, 11> POWER_INT_10;
	static const array<ullong, 20> POWER_ULLONG_10;
	static const char BOM[4];
};

#endif