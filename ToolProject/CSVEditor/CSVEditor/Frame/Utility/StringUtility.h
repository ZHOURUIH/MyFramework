#pragma once

#include "BinaryUtility.h"
#include "Vector.h"
#include "Set.h"
#include "ArrayList.h"
#include "MyString.h"

namespace StringUtility
{
	extern Array<20000, string> mIntString;
	extern const Array<11, llong> POWER_INT_10;
	extern const Array<6, float> INVERSE_POWER_INT_10;
	extern const Array<19, llong> POWER_LLONG_10;
	extern const Array<10, double> INVERSE_POWER_LLONG_10;
	extern const char BOM[4];
	extern string ChineseSpace;
	extern byte alphabet_map[];
	extern byte reverse_map[];
	//------------------------------------------------------------------------------------------------------------------------------
	// private
	void initIntToString();
	// 为了避免在头文件中包含MathUtility,在此封装一下要调的函数
	int greaterPower2(int value);
	//------------------------------------------------------------------------------------------------------------------------------
	// public
	string removeSuffix(const string& str);
	// 去掉开始的所有指定字符,直到遇到不是该字符的为止
	void removeStartAll(string& stream, char key);
	// 去掉第一个出现的指定字符
	void removeStart(string& stream, char key);
	// 去掉最后出现的所有指定字符,从后往前,直到遇到不是该字符的为止
	void removeLastAll(string& stream, char key);
	// 去掉最后一个出现的指定字符
	void removeLast(string& stream, char key);
	// 去掉最后一个逗号
	inline void removeLastComma(string& stream) { removeLast(stream, ','); }
	int strlength(const char* str, int maxLength);
	int strlength(const char* str);
	// 查找str中指定key的数量
	int findCharCount(const string& str, char key);
	int strchar(const char* str, char key, int startIndex = 0, int length = 0);
	string getFileName(const string& str);
	string getFileNameNoSuffix(const string& str, bool removePath);
	string getFirstFolderName(const string& dir);
	string removeFirstPath(const string& dir);
	string getFilePath(const string& dir, bool keepSlash);
	string getFileSuffix(const string& fileName, bool keepDot);
	string removeStartString(const string& fileName, const string& startStr);
	string removeEndString(const string& fileName, const string& endStr);
	inline string replaceSuffix(const string& fileName, const string& suffix) { return getFileNameNoSuffix(fileName, false) + suffix; }
	int getFirstNotNumberPos(const string& str, int startIndex);
	int getFirstNumberPos(const string& str);
	// 获得字符串最后不是数字的下标
	int getLastNotNumberPos(const string& str);
	// 获得字符串结尾的数字
	int getLastNumber(const string& str);
	// 获得去除末尾数字以后的字符串
	inline string getNotNumberSubString(const string& str) { return str.substr(0, getLastNotNumberPos(str) + 1); }
	int getLastChar(const char* str, char value);
	int getLastNotChar(const string& str, char value);
	void splitLine(const char* str, Vector<string>& vec, bool removeEmpty = true);
	void splitLine(const char* str, string* stringBuffer, int bufferSize, bool removeEmpty = true);
	void split(const char* str, char key, Vector<string>& vec, bool removeEmpty = true);
	Vector<string> split(const char* str, char key, bool removeEmpty = true);
	int split(const char* str, char key, string* stringBuffer, int bufferSize, bool removeEmpty = true);
	void split(const char* str, const char* key, Vector<string>& vec, bool removeEmpty = true);
	void split(const string& str, const char* key, Vector<string>& vec, bool removeEmpty = true);
	int split(const char* str, const char* key, string* stringBuffer, int bufferSize, bool removeEmpty = true);
	// 将字符串全部转为小写再查找
	bool findStringLower(const string& res, const string& sub, int* pos = nullptr, int startIndex = 0, bool direction = true);
	// 可指定从后或者从头查找子字符串
	bool findString(const string& res, const char* dst, int* pos = nullptr, int startIndex = 0, bool direction = true);
	bool findString(const char* res, const char* dst, int* pos = nullptr, int startIndex = 0, bool direction = true);
	int findStringPos(const char* res, const char* dst, int startIndex = 0, bool direction = true);
	int findStringPos(const string& res, const string& dst, int startIndex = 0, bool direction = true);
	// 将str中的[begin,end)替换为reStr
	template<int Length>
	inline void replace(MyString<Length>& str, const int begin, const int end, const char* reStr)
	{
		replace(str.toBuffer(), Length, begin, end, reStr);
	}
	void replace(char* str, int strBufferSize, int begin, int end, const char* reStr);
	void replace(string& str, int begin, int end, const string& reStr);
	template<int Length>
	inline void replaceAll(MyString<Length>& str, const char* key, const char* newWord)
	{
		replaceAll(str.toBuffer(), Length, key, newWord);
	}
	void replaceAll(char* str, int strBufferSize, const char* key, const char* newWord);
	void replaceAll(string& str, const string& key, const string& newWord);
	void replaceAll(string& str, char key, char newWord);
	void removeAll(string& str, char value);
	void removeAll(string& str, char value0, char value1);
	template<int Length>
	inline bool removeString(MyString<Length>& str, const char* subString)
	{
		int subPos = 0;
		if (!findString(str.str(), subString, &subPos, 0))
		{
			return false;
		}
		// 从子字符串的位置,后面的数据覆盖前面的数据
		int subLength = strlength(subString);
		memmove(str.toBuffer() + subPos, str.toBuffer() + subPos + subLength, Length - subLength - subPos);
		return true;
	}
	bool removeString(char* str, int length, const char* subString);
	// 判断oriString是否以pattern结尾,sensitive为是否大小写敏感
	bool endWith(const char* oriString, const char* pattern, bool sensitive = true);
	bool endWith(const string& oriString, const char* pattern, bool sensitive = true);
	// 判断oriString是否以pattern开头,sensitive为是否大小写敏感
	bool startWith(const char* oriString, const char* pattern, bool sensitive = true);
	bool startWith(const string& oriString, const char* pattern, bool sensitive = true);
	// 将source拼接到destBuffer后面
	void strcat_s(char* destBuffer, int size, const char* source);
	void strcat_s(char* destBuffer, int size, const char* source, int length);
	template<int Length>
	inline void strcat_s(MyString<Length>& destBuffer, const string& source)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source);
		destBuffer[destIndex + (int)source.length()] = '\0';
	}
	template<int Length>
	inline void strcat_s(MyString<Length>& destBuffer, const string& source, const int length)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	inline void strcat_s(MyString<Length>& destBuffer, const char* source)
	{
		const int destIndex = destBuffer.length();
		const int length = strlength(source);
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	inline void strcat_s(MyString<Length>& destBuffer, const char* source, const int length)
	{
		const int destIndex = destBuffer.length();
		destBuffer.copy(destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}
	template<int Length>
	inline void strcat_s(MyString<Length>& destBuffer, const char** sourceArray, const int sourceCount)
	{
		int destIndex = destBuffer.length();
		FOR(sourceCount)
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
	inline void strcat_s(char* destBuffer, const int destSize, const Array<SourceLength, const char*>& sourceArray)
	{
		int destIndex = strlength(destBuffer, destSize);
		for (const char* curSource : sourceArray)
		{
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
	inline void strcat_t(char* destBuffer, const int destSize, TypeList&&... params)
	{
		strcat_s(destBuffer, destSize, Array<sizeof...(params), const char*>{ forward<TypeList>(params)... });
	}
	template<int Length, int SourceLength>
	inline void strcat_s(MyString<Length>& destBuffer, const Array<SourceLength, const char*>& sourceArray)
	{
		int destIndex = destBuffer.length();
		for (const char* curSource : sourceArray)
		{
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
	inline void strcat_t(MyString<Length>& destBuffer, TypeList&&... params)
	{
		strcat_s(destBuffer, Array<sizeof...(params), const char*>{ forward<TypeList>(params)... });
	}
	void strcpy_s(char* destBuffer, int size, const char* source);
	// 以string类型返回count个0
	string zeroString(int zeroCount);
	template<int Length>
	inline void zeroString(MyString<Length>& charArray, const int zeroCount)
	{
		if ((int)Length <= zeroCount)
		{
			ERROR("buffer is too small");
			return;
		}
		FOR(zeroCount)
		{
			charArray[i] = '0';
		}
		charArray[zeroCount] = '\0';
	}
	// 基础数据类型转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	inline string BToS(const bool value) { return value ? "True" : "False"; }
	// 返回string类型的数字字符串,速度较慢
	string LLToS(llong value, int limitLen = 0);
	template<int Length>
	inline int _i64toa_s(llong value, MyString<Length>& charArray)
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
		const llong maxLLong = POWER_LLONG_10[POWER_LLONG_10.size() - 1] - 1;
		if (value > maxLLong)
		{
			value = maxLLong;
		}
		int index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过size - 1
			// 如果是负数,则数字个数不能超过size - 2
			if ((sign > 0 && index >= (int)Length) ||
				(sign < 0 && index >= (int)Length - 1))
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
			const int lengthToHead = Length - index;
			FOR(index)
			{
				charArray[i] = charArray[i + lengthToHead] + '0';
			}
			charArray[index] = '\0';
		}
		else
		{
			charArray[0] = '-';
			const int lengthToHead = Length - index;
			FOR(index)
			{
				charArray[i + 1] = charArray[i + lengthToHead] + '0';
			}
			charArray[++index] = '\0';
		}
		return index;
	}
	template<int Length>
	inline int LLToS(MyString<Length>& charArray, const llong value, const int limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _i64toa_s(value, charArray);
		}
		MyString<32> temp;
		const int len = _i64toa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			MyString<16> zeroArray;
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.str(), temp.str());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.str(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	template<int Length>
	inline int _itoa_s(int value, MyString<Length>& charArray)
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
			const int strLength = (int)str.length();
			if (strLength + 1 >= (int)Length)
			{
				ERROR("int value is too large:" + LLToS((llong)value));
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
			ERROR("int value is too large:" + LLToS((llong)value));
			return 0;
		}
		int index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过size - 1
			// 如果是负数,则数字个数不能超过size - 2
			if ((sign > 0 && index >= (int)Length) ||
				(sign < 0 && index >= (int)Length - 1))
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
			const int lengthToHead = Length - index;
			FOR(index)
			{
				charArray[i] = charArray[i + lengthToHead] + '0';
			}
			charArray[index] = '\0';
		}
		else
		{
			charArray[0] = '-';
			const int lengthToHead = Length - index;
			FOR(index)
			{
				charArray[i + 1] = charArray[i + lengthToHead] + '0';
			}
			charArray[++index] = '\0';
		}
		return index;
	}
	// 这里有问题,应该是
	template<int Length>
	inline int _uitoa_s(const uint value, MyString<Length>& charArray)
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
		if (mIntString[mIntString.size() - 1].length() > 0 && value < (uint)mIntString.size())
		{
			const string& str = mIntString[value];
			const int strLength = (int)str.length();
			if (strLength + 1 >= (int)Length)
			{
				ERROR("uint value is too large:" + LLToS((llong)value));
				return 0;
			}
			charArray.copy(str, strLength);
			charArray[strLength] = '\0';
			return strLength;
		}

		if ((llong)value > POWER_INT_10[POWER_INT_10.size() - 1])
		{
			ERROR("uint value is too large:" + LLToS((llong)value));
			return 0;
		}
		int index = 0;
		while (true)
		{
			// 数字个数不能超过size - 1
			if (index >= (int)Length)
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
		const int lengthToHead = Length - index;
		FOR(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
		return index;
	}
	template<int Length>
	inline int _ui64toa_s(const ullong value, MyString<Length>& charArray)
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
		int index = 0;
		while (true)
		{
			// 如果是正数,则数字个数不能超过Length - 1
			if (index >= (int)Length)
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
		const int lengthToHead = Length - index;
		FOR(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
		return index;
	}
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	string UIToS(int value, int limitLen = 0);
	template<int Length>
	inline int UIToS(MyString<Length>& charArray, const int value, const int limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _itoa_s(value, charArray);
		}
		// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
		MyString<16> temp;
		const int len = _itoa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
			MyString<16> zeroArray;
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.str(), temp.str());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.str(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// 返回string类型的数字字符串,速度较慢,limitLen是字符串的最小长度,如果整数的位数不足最小长度,则会在前面加0
	string IToS(const int value, int limitLen = 0);
	template<int Length>
	inline int IToS(MyString<Length>& charArray, const int value, const int limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _itoa_s(value, charArray);
		}
		// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
		MyString<16> temp;
		const int len = _itoa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			// 因为当前函数设计为线程安全,所以只能使用栈内存中的数组
			MyString<16> zeroArray;
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.str(), temp.str());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.str(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// 返回string类型的数字字符串,速度较慢
	string ULLToS(ullong value, int limitLen = 0);
	template<int Length>
	inline int ULLToS(MyString<Length>& charArray, const ullong value, const int limitLen = 0)
	{
		if (limitLen == 0)
		{
			return _ui64toa_s(value, charArray);
		}
		MyString<32> temp;
		const int len = _ui64toa_s(value, temp);
		// 判断是否需要在前面补0
		if (limitLen > len)
		{
			MyString<16> zeroArray;
			zeroString(zeroArray, limitLen - len);
			strcat_t(charArray, zeroArray.str(), temp.str());
			charArray[limitLen] = '\0';
			return limitLen;
		}
		else
		{
			charArray.copy(temp.str(), len);
			charArray[len] = '\0';
			return len;
		}
	}
	// precision为精度,保留的小数的位数,removeZero为是否去除末尾无用的0,速度较慢
	string floatToStringExtra(float f, int precision = 4, bool removeTailZero = true);
	// 将浮点数转换为字符串,速度较快
	string FToS(float f);
	template<int Length>
	inline int FToS(MyString<Length>& charArray, float f)
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
		int strLen = 0;
		FOR(Length)
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
			FOR(strLen)
			{
				const int index = strLen - 1 - i;
				// 如果找到了小数点仍然没有找到一个不为'0'的字符,则从小数点部分截断字符串
				if (index == dotPos)
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
	string bytesToString(const char* buffer, int length);
	inline string V2ToS(const Vector2& vec, const char* seperate = ",") { return FToS(vec.x) + seperate + FToS(vec.y); }
	template<int Length>
	inline void V2ToS(MyString<Length>& buffer, const Vector2& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_STR(xStr, vec.x);
		FLOAT_STR(yStr, vec.y);
		strcat_t(buffer, xStr.str(), seperate, yStr.str());
	}
	inline string V3ToS(const Vector3& vec, const char* seperate = ",") { return FToS(vec.x) + seperate + FToS(vec.y) + seperate + FToS(vec.z); }
	template<int Length>
	inline void V3ToS(MyString<Length>& buffer, const Vector3& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		FLOAT_STR(xStr, vec.x);
		FLOAT_STR(yStr, vec.y);
		FLOAT_STR(zStr, vec.z);
		strcat_t(buffer, xStr.str(), seperate, yStr.str(), seperate, zStr.str());
	}
	inline string V3IToS(const Vector3Int& vec, const char* seperate = ",") { return IToS(vec.x) + seperate + IToS(vec.y) + seperate + IToS(vec.z); }
	template<int Length>
	inline void V3IToS(MyString<Length>& buffer, const Vector3Int& vec, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, vec.x);
		INT_STR(yStr, vec.y);
		INT_STR(zStr, vec.z);
		strcat_t(buffer, xStr.str(), seperate, yStr.str(), seperate, zStr.str());
	}
	inline string V2IToS(const Vector2Int& value, const char* seperate = ",") { return IToS(value.x) + seperate + IToS(value.y); }
	inline string V2UIToS(const Vector2UInt& value, const char* seperate = ",") { return UIToS(value.x) + seperate + UIToS(value.y); }
	template<int Length>
	inline void V2IToS(MyString<Length>& buffer, const Vector2Int& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		strcat_t(buffer, xStr.str(), seperate, yStr.str());
	}
	template<int Length>
	inline void V2UIToS(MyString<Length>& buffer, const Vector2UInt& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		strcat_t(buffer, xStr.str(), seperate, yStr.str());
	}
	inline string V2SToS(const Vector2Short& value, const char* seperate = ",") { return IToS(value.x) + seperate + IToS(value.y); }
	template<int Length>
	inline void V2SToS(MyString<Length>& buffer, const Vector2Short& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		strcat_t(buffer, xStr.str(), seperate, yStr.str());
	}
	inline string V2USToS(const Vector2UShort& value, const char* seperate = ",") { return IToS(value.x) + seperate + IToS(value.y); }
	template<int Length>
	inline void V2USToS(MyString<Length>& buffer, const Vector2UShort& value, const char* seperate = ",")
	{
		buffer[0] = '\0';
		INT_STR(xStr, value.x);
		INT_STR(yStr, value.y);
		strcat_t(buffer, xStr.str(), seperate, yStr.str());
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型
	//-----------------------------------------------------------------------------------------------------------------------------
	inline bool SToB(const string& str) { return str == "True" || str == "true"; }
	inline bool SToB(const char* str) { return strcmp(str, "True") == 0 || strcmp(str, "true") == 0; }
	inline int SToI(const string& str) { return atoi(str.c_str()); }
	inline int SToI(const char* str) { return atoi(str); }
	inline ullong stringToULLong(const string& str) { return (ullong)atoll(str.c_str()); }
	inline ullong stringToULLong(const char* str) { return (ullong)atoll(str); }
	inline llong SToLL(const string& str) { return atoll(str.c_str()); }
	inline llong SToLL(const char* str) { return atoll(str); }
	inline float SToF(const string& str) { return (float)atof(str.c_str()); }
	inline float SToF(const char* str) { return (float)atof(str); }
	Vector2 SToV2(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector2Int SToV2I(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector2UInt SToV2UI(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector2Short SToV2S(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector2UShort SToV2US(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector3 SToV3(const string& str, const char* seperate = ",", bool* result = nullptr);
	Vector3Int SToV3I(const string& str, const char* seperate = ",", bool* result = nullptr);
	//-----------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型数组转换为字符串
	//-----------------------------------------------------------------------------------------------------------------------------
	string ULLsToS(const Vector<ullong>& valueList, const char* seperate = ",");
	template<int Length>
	inline string ULLsToS(const Array<Length, ullong>& valueList, const int count = -1, const char* seperate = ",")
	{
		if (count == -1)
		{
			count = valueList.size();
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		MyString<32> temp;
		FOR(count)
		{
			ULLToS(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(charArray.mArray, arrayLen, temp.str(), seperate);
			}
			else
			{
				strcat_s(charArray.mArray, arrayLen, temp.str());
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline void ULLsToS(MyString<Length>& buffer, const ullong* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}
		MyString<32> temp;
		FOR(count)
		{
			ULLToS(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(buffer, temp.str(), seperate);
			}
			else
			{
				strcat_s(buffer, temp.str());
			}
		}
	}
	template<int Length>
	inline void ULLsToS(MyString<Length>& buffer, const Vector<ullong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		MyString<32> temp;
		const int count = valueList.size();
		FOR(count)
		{
			ULLToS(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(buffer, temp.str(), seperate);
			}
			else
			{
				strcat_s(buffer, temp.str());
			}
		}
	}
	inline void ULLsToS(char* buffer, const int bufferSize, const Vector<ullong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		MyString<32> temp;
		const int count = valueList.size();
		FOR(count)
		{
			ULLToS(temp, valueList[i]);
			if (i != count - 1)
			{
				strcat_t(buffer, bufferSize, temp.str(), seperate);
			}
			else
			{
				strcat_s(buffer, bufferSize, temp.str());
			}
		}
	}
	string LLsToS(const Vector<llong>& valueList, const char* seperate = ",");
	string LLsToS(const llong* valueList, int valueCount, const char* seperate = ",");
	template<int Length>
	inline string LLsToS(const Array<Length, llong>& valueList, int count = -1, const char* seperate = ",")
	{
		if (count == -1)
		{
			count = (int)Length;
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline string LLsToS(const ArrayList<Length, llong>& valueList, const char* seperate = ",")
	{
		const int count = valueList.size();
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline void LLsToS(MyString<Length>& buffer, const llong* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void LLsToS(MyString<Length>& buffer, const Vector<llong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void LLsToS(char* buffer, const int bufferSize, const Vector<llong>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	template<int Length, int ValueCount>
	inline void LLsToS(MyString<Length>& buffer, const Array<ValueCount, llong>& valueList, int count = -1, const char* seperate = ",")
	{
		if (count == -1)
		{
			count = (int)ValueCount;
		}
		const int seperateLen = strlength(seperate);
		buffer[0] = '\0';
		MyString<32> temp;
		FOR(count)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	// 将byte数组当成整数数组转换为字符串,并非直接将byte数组转为字符串
	string bytesToString(const Vector<byte>& valueList, const char* seperate = ",");
	template<int Length>
	inline void bytesToString(MyString<Length>& buffer, byte* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<4> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void bytesToString(MyString<Length>& buffer, const Vector<byte>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<4> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void bytesToString(char* buffer, const int bufferSize, const Vector<byte>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<4> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	string SsToS(const Vector<short>& valueList, const char* seperate = ",");
	template<int Length>
	inline void SsToS(MyString<Length>& buffer, short* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void SsToS(MyString<Length>& buffer, const Vector<short>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void SsToS(char* buffer, const int bufferSize, const Vector<short>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	string USsToS(const Vector<ushort>& valueList, const char* seperate = ",");
	template<int Length>
	inline string USsToS(const ArrayList<Length, ushort>& valueList, const char* seperate = ",")
	{
		if (valueList.size() == 0)
		{
			return FrameDefine::EMPTY;
		}
		const int arrayLen = 8 * greaterPower2(valueList.size());
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = '\0';
		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline void USsToS(MyString<Length>& buffer, ushort* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void USsToS(MyString<Length>& buffer, const Vector<ushort>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void USsToS(char* buffer, const int bufferSize, const Vector<ushort>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<8> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	string IsToS(const Vector<int>& valueList, const char* seperate = ",");
	template<int Length>
	inline string IsToS(const Array<Length, int>& valueList, const int count, const char* seperate = ",")
	{
		if (count == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		const int arrayLen = 16 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline string IsToS(const Array<Length, int>& valueList, const char* seperate = ",")
	{
		const int count = valueList.size();
		if (count == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		const int arrayLen = 16 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline string IsToS(const ArrayList<Length, int>& valueList, const char* seperate = ",")
	{
		const int count = valueList.size();
		if (count == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		const int arrayLen = 16 * greaterPower2(count);
		CharArrayScopeThread charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}
	template<int Length>
	inline void IsToS(MyString<Length>& buffer, int* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void IsToS(MyString<Length>& buffer, const Vector<int>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void IsToS(char* buffer, int bufferSize, const Vector<int>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void IsToS(char* buffer, int bufferSize, const ArrayList<Length, int>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = IToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	string UIsToS(const Vector<uint>& valueList, const char* seperate = ",");
	template<int Length>
	inline void UIsToS(MyString<Length>& buffer, uint* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = UIToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void UIsToS(MyString<Length>& buffer, const Vector<uint>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = UIToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline void UIsToS(char* buffer, int bufferSize, const Vector<uint>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = UIToS(temp, valueList[i]);
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}
	string FsToS(const Vector<float>& valueList, const char* seperate = ",");
	void FsToS(char* buffer, int bufferSize, const Vector<float>& valueList, const char* seperate = ",");
	template<int Length>
	inline void FsToS(MyString<Length>& buffer, float* valueList, const int count, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList == nullptr || count == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		FOR(count)
		{
			const int len = FToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void FsToS(MyString<Length>& buffer, const Vector<float>& valueList, const char* seperate = ",")
	{
		buffer[0] = '\0';
		if (valueList.size() == 0)
		{
			return;
		}

		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = FToS(temp, valueList[i]);
			strcat_s(buffer, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	template<int Length>
	inline void stringsToString(MyString<Length>& buffer, const char** strList, const int stringCount, const char* seperate = ",")
	{
		const int seperateLen = strlength(seperate);
		buffer[0] = '\0';
		FOR(stringCount)
		{
			strcat_s(buffer, strList[i]);
			if (i != stringCount - 1)
			{
				strcat_s(buffer, seperate, seperateLen);
			}
		}
	}
	inline string stringsToString(const Vector<string>& strList, const char* seperate = ",")
	{
		string totalStr;
		FOR_VECTOR(strList)
		{
			totalStr += strList[i];
			if (i != strList.size() - 1)
			{
				totalStr += seperate;
			}
		}
		return totalStr;
	}
	inline string stringsToString(const Set<string>& strList, const char* seperate = ",")
	{
		int index = 0;
		string totalStr;
		for (const string& str : strList)
		{
			totalStr += str;
			if (index++ != strList.size() - 1)
			{
				totalStr += seperate;
			}
		}
		return totalStr;
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 字符串转换为基础数据类型数组
	//-----------------------------------------------------------------------------------------------------------------------------
	void SToBs(const string& str, Vector<byte>& valueList, const char* seperate = ",");
	int SToBs(const char* str, byte* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToBs(const char* str, Array<Length, byte>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int keyLen = strlength(seperate);
		const int sourceLen = strlength(str);
		MyString<4> curString;
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
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	void SToSs(const string& str, Vector<short>& valueList, const char* seperate = ",");
	int SToSs(const char* str, short* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToSs(const string& str, Array<Length, short>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
		MyString<16> curString;
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
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	void SToUSs(const string& str, Vector<ushort>& valueList, const char* seperate = ",");
	template<int Length>
	inline void SToUSs(const string& str, ArrayList<Length, ushort>& valueList, const char* seperate = ",")
	{
		Vector<string> strList;
		split(str, seperate, strList);
		valueList.clear();
		FOR(strList.size())
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.add(SToI(curStr));
			}
		}
	}
	int SToUSs(const char* str, ushort* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToUSs(const char* str, Array<Length, ushort>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<8> curString;
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
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	Vector<int> SToIs(const string& str, const char* seperate = ",");
	void SToIs(const string& str, Vector<int>& valueList, const char* seperate = ",");
	int SToIs(const char* str, int* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToIs(const string& str, Array<Length, int>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
		MyString<16> curString;
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
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	template<int Length>
	inline void SToIs(const string& str, ArrayList<Length, int>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
		MyString<16> curString;
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
			if (!buffer.add(SToI(curString.str())))
			{
				if (showError)
				{
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
		}
	}
	template<int Length>
	inline int SToIs(const char* str, Array<Length, int>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<16> curString;
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
					ERROR("int buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	void SToUIs(const string& str, Vector<uint>& valueList, const char* seperate = ",");
	int SToUIs(const char* str, uint* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToUIs(const char* str, Array<Length, uint>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<16> curString;
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
					ERROR("uint buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}
	void stringToULLongs(const char* str, Vector<ullong>& valueList, const char* seperate = ",");
	int stringToULLongs(const char* str, ullong* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int stringToULLongs(const char* str, Array<Length, ullong>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
					ERROR("ullong buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = stringToULLong(curString.str());
		}
		return curCount;
	}
	void SToLLs(const char* str, Vector<llong>& valueList, const char* seperate = ",");
	void SToLLs(const string& str, Vector<llong>& valueList, const char* seperate = ",");
	Vector<llong> SToLLs(const string& str, const char* seperate = ",");
	int SToLLs(const char* str, llong* buffer, int bufferSize, const char* seperate = ",");
	template<int Length>
	inline int SToLLs(const string& str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = destOffset;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
					LOG("llong buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToLL(curString.str());
		}
		return curCount;
	}
	template<int Length>
	inline void SToLLs(const string& str, ArrayList<Length, llong>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		const int sourceLen = (int)str.length();
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
			if (!buffer.add(SToLL(curString.str())))
			{
				if (showError)
				{
					LOG("llong buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
		}
	}
	template<int Length>
	inline int SToLLs(const char* str, Array<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = destOffset;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
					ERROR("llong buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToLL(curString.str());
		}
		return curCount;
	}
	template<int Length>
	inline void SToLLs(const char* str, ArrayList<Length, llong>& buffer, int destOffset = 0, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
			if (!buffer.add(SToLL(curString.str())))
			{
				if (showError)
				{
					ERROR("llong buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
		}
	}
	void SToFs(const string& str, Vector<float>& valueList, const char* seperate = ",");
	template<int Length>
	inline int SToFs(const char* str, Array<Length, float>& buffer, const char* seperate = ",", const bool showError = true)
	{
		int startPos = 0;
		int curCount = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<32> curString;
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
					ERROR("float buffer size is too small, bufferSize:" + IToS(Length));
				}
				break;
			}
			buffer[curCount++] = SToF(curString.str());
		}
		return curCount;
	}
	bool SToV2Is(const string& str, Vector<Vector2Int>& valueList, const char* seperate = ",");
	bool SToV2s(const string& str, Vector<Vector2>& valueList, const char* seperate = ",");
	bool SToV3Is(const string& str, Vector<Vector3Int>& valueList, const char* seperate = ",");
	bool SToV3s(const string& str, Vector<Vector3>& valueList, const char* seperate = ",");
	// 从一个ullong数组的字符串中移除指定的value的字符串
	template<int Length>
	inline bool removeULLongsString(MyString<Length>& valueArrayString, const ullong value)
	{
		ULLONG_STR(valueString, value);
		// 如果value是在最后,则只移除value字符串
		if (endWith(valueArrayString.str(), valueString.str()))
		{
			return removeString(valueArrayString, valueString.str());
		}
		// value不在最后,则移除value字符串加后面的逗号
		else
		{
			MyString<32> needRemoveString;
			strcat_t(needRemoveString, valueString.str(), ",");
			return removeString(valueArrayString, needRemoveString.str());
		}
	}
	// 将value添加到一个ullong数组的字符串中
	template<int Length>
	inline void addULLongsString(MyString<Length>& valueArrayString, const ullong value)
	{
		ULLONG_STR(idStr, value);
		if (valueArrayString[0] != '\0')
		{
			strcat_t(valueArrayString, ",", idStr.str());
		}
		else
		{
			valueArrayString.copy(idStr);
		}
	}
	// 从一个llong数组的字符串中移除指定的value的字符串
	template<int Length>
	inline bool removeLLongsString(MyString<Length>& valueArrayString, const llong value)
	{
		LLONG_STR(valueString, value);
		// 如果value是在最后,则只移除value字符串
		if (endWith(valueArrayString.str(), valueString.str()))
		{
			return removeString(valueArrayString, valueString.str());
		}
		// value不在最后,则移除value字符串加后面的逗号
		else
		{
			MyString<32> needRemoveString;
			strcat_t(needRemoveString, valueString.str(), ",");
			return removeString(valueArrayString, needRemoveString.str());
		}
	}
	// 将value添加到一个llong数组的字符串中
	template<int Length>
	inline void addLLongsString(MyString<Length>& valueArrayString, const llong value)
	{
		LLONG_STR(idStr, value);
		if (valueArrayString[0] != '\0')
		{
			strcat_t(valueArrayString, ",", idStr.str());
		}
		else
		{
			valueArrayString.copy(idStr);
		}
	}
	template<int Length>
	inline int split(const string& str, const char* key, ArrayList<Length, string>& stringBuffer, const bool removeEmpty = true, const bool showError = true)
	{
		const int sourceLen = (int)str.length();
		if (sourceLen == 0)
		{
			return 0;
		}
		const int keyLen = strlength(key);
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		int devidePos = -1;
		int startPos = 0;
		bool ret = true;
		int elementCount = 0;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return elementCount;
			}
			curString.copy(str, startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			++elementCount;
			if (!stringBuffer.add(curString.str()))
			{
				if (showError)
				{
					ERROR("string buffer is too small! bufferSize:" + IToS(Length));
				}
				break;
			}
		}
		return elementCount;
	}
	template<int Length>
	inline int split(const char* str, const char* key, ArrayList<Length, string>& stringBuffer, const bool removeEmpty = true, const bool showError = true)
	{
		const int sourceLen = strlength(str);
		if (sourceLen == 0)
		{
			return 0;
		}
		const int keyLen = strlength(key);
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		int devidePos = -1;
		int startPos = 0;
		bool ret = true;
		int elementCount = 0;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return elementCount;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			++elementCount;
			if (!stringBuffer.add(curString.str()))
			{
				if (showError)
				{
					ERROR("string buffer is too small! bufferSize:" + IToS(Length));
				}
				break;
			}
		}
		return elementCount;
	}
	template<int Length>
	inline int split(const char* str, const char key, ArrayList<Length, string>& stringBuffer, const bool removeEmpty = true, const bool showError = true)
	{
		const int sourceLen = strlength(str);
		if (sourceLen == 0)
		{
			return 0;
		}
		int startPos = 0;
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		bool ret = true;
		int elementCount = 0;
		while (ret)
		{
			int devidePos = strchar(str, key, startPos, sourceLen);
			// 无论是否查找到,都将前面一段字符串截取出来
			if (devidePos == -1)
			{
				devidePos = sourceLen;
				ret = false;
			}
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return elementCount;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + 1;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			++elementCount;
			if (!stringBuffer.add(curString.str()))
			{
				if (showError)
				{
					ERROR("string buffer is too small! bufferSize:" + IToS(Length));
				}
				break;
			}
		}
		return elementCount;
	}

	template<int Length>
	inline bool splitFull(const char* str, const char* key, ArrayList<Length, string>& stringBuffer, const bool removeEmpty = true, const bool showError = true)
	{
		const int elementCount = split(str, key, stringBuffer, removeEmpty, showError);
		return elementCount == stringBuffer.maxSize();
	}
	template<int Length>
	bool splitFull(const string& str, const char* key, ArrayList<Length, string>& stringBuffer, const bool removeEmpty = true, const bool showError = true)
	{
		const int elementCount = split(str, key, stringBuffer, removeEmpty, showError);
		return elementCount == stringBuffer.maxSize();
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	// 移除字符串首部的数字
	string removePreNumber(const string& str);
	wstring ANSIToUnicode(const char* str);
	void ANSIToUnicode(const char* str, wchar_t* output, int maxLength);
	void ANSIToUnicode(const string& str, wchar_t* output, int maxLength);
	string UnicodeToANSI(const wchar_t* str);
	void UnicodeToANSI(const wchar_t* str, char* output, int maxLength);
	string UnicodeToUTF8(const wchar_t* str);
	void UnicodeToUTF8(const wchar_t* str, char* output, int maxLength);
	wstring UTF8ToUnicode(const char* str);
	void UTF8ToUnicode(const char* str, wchar_t* output, int maxLength);
	void UTF8ToUnicode(const string& str, wchar_t* output, int maxLength);
	string ANSIToUTF8(const char* str, bool addBOM = false);
	string ANSIToUTF8(const string& str, bool addBOM = false);
	void ANSIToUTF8(const char* str, char* output, int maxLength, bool addBOM = false);
	void ANSIToUTF8(const string& str, char* output, int maxLength, bool addBOM = false);
	// 根据不同的平台选择不同的实现方式,windows下进行转换,linux直接返回原字符串
#ifdef WIN32
	inline string ANSIToUTF8Auto(const char* str, bool addBOM = false) { return ANSIToUTF8(str, addBOM); }
	inline string ANSIToUTF8Auto(const string& str, bool addBOM = false) { return ANSIToUTF8(str, addBOM); }
#elif defined LINUX
	inline string ANSIToUTF8Auto(const char* str, bool addBOM = false) { return str; }
	inline const string& ANSIToUTF8Auto(const string& str, bool addBOM = false) { return str; }
#endif
	string UTF8ToANSI(const char* str, bool eraseBOM = false);
	string UTF8ToANSI(const string& str, bool eraseBOM = false);
	void UTF8ToANSI(const char* str, char* output, int maxLength, bool eraseBOM = false);
#ifdef WIN32
	inline string UTF8ToANSIAuto(const char* str, bool eraseBOM = false) { return UTF8ToANSI(str, eraseBOM); }
	inline string UTF8ToANSIAuto(const string& str, bool eraseBOM = false) { return UTF8ToANSI(str, eraseBOM); }
#elif defined LINUX
	inline string UTF8ToANSIAuto(const char* str, bool eraseBOM = false) { return str; }
	inline const string& UTF8ToANSIAuto(const string& str, bool eraseBOM = false) { return str; }
#endif
	void removeBOM(string& str);
	void removeBOM(char* str, int length = 0);
	// json
	void jsonStartArray(string& str, int preTableCount = 0, bool returnLine = false);
	void jsonEndArray(string& str, int preTableCount = 0, bool returnLine = false);
	void jsonStartStruct(string& str, int preTableCount = 0, bool returnLine = false);
	void jsonEndStruct(string& str, int preTableCount = 0, bool returnLine = false);
	void jsonAddPair(string& str, const string& name, const string& value, int preTableCount = 0, bool returnLine = false);
	void jsonAddObject(string& str, const string& name, const string& value, int preTableCount = 0, bool returnLine = false);
	string toLower(const string& str);
	string toUpper(const string& str);
	inline bool isUpper(char c) { return c >= 'A' && c <= 'Z'; }
	inline bool isLower(char c) { return c >= 'a' && c <= 'z'; }
	inline bool isNumber(char c) { return c >= '0' && c <= '9'; }
	bool isNumber(const string& str);
	inline bool isLetter(char c) { return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'); }
	inline char toLower(const char value)
	{
		if (isUpper(value))
		{
			return value + 'a' - 'A';
		}
		return value;
	}
	inline char toUpper(const char value)
	{
		if (isLower(value))
		{
			return value - ('a' - 'A');
		}
		return value;
	}
	void rightToLeft(string& str);
	void leftToRight(string& str);
	bool checkString(const string& str, const string& valid);
	inline bool checkIntString(const string& str, const string& valid = "") { return checkString(str, "0123456789" + valid); }
	inline bool checkFloatString(const string& str, const string& valid = "") { return checkIntString(str, "." + valid); }
	bool hasNonAscii(const char* str);
	string charToHexString(byte value, bool upper = true);
	int getCharCount(const string& str, char key);
	int getCharCount(const char* str, char key);
	bool isPhoneNumber(const char* str);
	string bytesToHexString(const byte* data, int dataCount, bool addSpace = true, bool upper = true);
	inline byte hexCharToByte(const char hex)
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
	inline char byteToHexChar(const byte value)
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
	// 字符串中是否包含控制字符
	bool hasControlChar(const string& str);
	// 字符串中是否包含控制字符,英文空格,中文空格
	bool hasInvisibleChar(const string& str);
	// base64编码
	string base64_encode(const byte* str, int length);
	string base64_encode(const string& str);
	// base64解码
	string base64_decode(const string& code);
	string base64_decode(const char* code, int codeLen);
	// 计算一个字符串的SHA-1值
	uint32_t rotate_left(uint32_t value, int shift);
	vector<uint8_t> pad_message(const string& message);
	uint32_t bytes_to_uint32(const uint8_t* bytes);
	void uint32_to_bytes(uint32_t value, uint8_t* bytes);
	void sha1(const string& message, byte* buffer);
	string sha1(const string& str);
}

using StringUtility::ANSIToUTF8;
using StringUtility::ANSIToUTF8Auto;
using StringUtility::UTF8ToANSI;
using StringUtility::UTF8ToANSIAuto;
using StringUtility::ANSIToUnicode;
using StringUtility::IToS;
using StringUtility::LLToS;
using StringUtility::V3ToS;
using StringUtility::V3IToS;
using StringUtility::V2ToS;
using StringUtility::LLsToS;
using StringUtility::IsToS;
using StringUtility::strcat_t;
using StringUtility::SToLLs;
using StringUtility::SToIs;
using StringUtility::SToLL;
using StringUtility::split;
using StringUtility::splitLine;
using StringUtility::splitFull;
using StringUtility::SToI;
using StringUtility::FToS;
using StringUtility::SToF;
using StringUtility::getFileName;
using StringUtility::removeAll;
using StringUtility::SToV2I;
using StringUtility::SToV2UI;
using StringUtility::SToV2US;
using StringUtility::strlength;
using StringUtility::UIToS;
using StringUtility::endWith;
using StringUtility::hexCharToByte;
using StringUtility::SToV2S;
using StringUtility::isNumber;
using StringUtility::hasInvisibleChar;
using StringUtility::replaceAll;
using StringUtility::startWith;
using StringUtility::findString;
using StringUtility::SToUSs;
using StringUtility::getFileNameNoSuffix;
using StringUtility::getLastNotChar;
using StringUtility::ULLToS;
using StringUtility::USsToS;
using StringUtility::UIsToS;
using StringUtility::FsToS;
using StringUtility::ULLsToS;
using StringUtility::replace;
using StringUtility::strchar;
using StringUtility::bytesToHexString;
using StringUtility::getFilePath;
using StringUtility::SToBs;
using StringUtility::SToUIs;
using StringUtility::SToFs;
using StringUtility::SToV2;
using StringUtility::SToV3;
using StringUtility::SToV3I;
using StringUtility::SToSs;
using StringUtility::SsToS;
using StringUtility::removeStartString;
using StringUtility::removeEndString;
using StringUtility::rightToLeft;
using StringUtility::toUpper;
using StringUtility::checkString;
using StringUtility::getCharCount;
using StringUtility::POWER_INT_10;
using StringUtility::INVERSE_POWER_INT_10;
using StringUtility::POWER_LLONG_10;
using StringUtility::INVERSE_POWER_LLONG_10;
using StringUtility::V2IToS;
using StringUtility::V2UIToS;
using StringUtility::byteToHexChar;
using StringUtility::removeLLongsString;
using StringUtility::stringsToString;
using StringUtility::BToS;
using StringUtility::SToB;
using StringUtility::SToV2s;
using StringUtility::SToV2Is;
using StringUtility::SToV3s;
using StringUtility::SToV3Is;
using StringUtility::base64_encode;
using StringUtility::sha1;
using StringUtility::getFileSuffix;
using StringUtility::getFirstNotNumberPos;
using StringUtility::getFirstNumberPos;