#include "FrameHeader.h"

namespace StringUtility
{
	const char BOM[4]{ (char)0xEF, (char)0xBB, (char)0xBF, 0 };
	string ChineseSpace;
	Array<20000, string> mIntString;
	const Array<11, llong> POWER_INT_10{ 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L, 10000000L, 100000000L, 1000000000L, 10000000000L };
	const Array<6, float> INVERSE_POWER_INT_10{ 1.0f, 0.1f, 0.01f, 0.001f, 0.0001f, 0.00001f };
	const Array<19, llong> POWER_LLONG_10
	{
		1L,
		10L,
		100L,
		1000L,
		10000L,
		100000L,
		1000000L,
		10000000L,
		100000000L,
		1000000000L,
		10000000000L,
		100000000000L,
		1000000000000L,
		10000000000000L,
		100000000000000L,
		1000000000000000L,
		10000000000000000L,
		100000000000000000L,
		1000000000000000000L
	};
	const Array<10, double> INVERSE_POWER_LLONG_10{ 1.0, 0.1, 0.01, 0.001, 0.0001, 0.00001, 0.000001, 0.0000001, 0.00000001, 0.000000001 };

	byte alphabet_map[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	byte reverse_map[] =
	{
		 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 62, 255, 255, 255, 63,
		 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 255, 255, 255, 255, 255, 255,
		 255,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
		 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 255, 255, 255, 255, 255,
		 255, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 255, 255, 255, 255, 255
	};

	string removeSuffix(const string& str)
	{
		const int dotPos = (int)str.find_last_of('.');
		if (dotPos != -1)
		{
			return str.substr(0, dotPos);
		}
		return str;
	}

	void removeStartAll(string& stream, const char key)
	{
		FOR((int)stream.length())
		{
			if (stream[i] != key)
			{
				stream.erase(0, i);
				break;
			}
		}
	}

	void removeStart(string& stream, const char key)
	{
		FOR((int)stream.length())
		{
			if (stream[i] == key)
			{
				stream.erase(i, 1);
				break;
			}
		}
	}

	void removeLastAll(string& stream, const char key)
	{
		FOR_INVERSE_I((int)stream.length())
		{
			if (stream[i] != key)
			{
				stream.erase(i + 1);
				break;
			}
		}
	}

	void removeLast(string& stream, const char key)
	{
		FOR_INVERSE_I((int)stream.length())
		{
			if (stream[i] == key)
			{
				stream.erase(i);
				break;
			}
		}
	}

	int findCharCount(const string& str, const char key)
	{
		int count = 0;
		FOR((int)str.length())
		{
			if (str[i] == key)
			{
				++count;
			}
		}
		return count;
	}

	int strchar(const char* str, const char key, const int startIndex, int length)
	{
		if (length == 0)
		{
			length = strlength(str);
		}
		for (int i = startIndex; i < length; ++i)
		{
			if (str[i] == key)
			{
				return i;
			}
		}
		return -1;
	}

	string getFileName(const string& str)
	{
		string newStr = str;
		rightToLeft(newStr);
		const int dotPos = (int)newStr.find_last_of('/');
		if (dotPos != -1)
		{
			return newStr.substr(dotPos + 1, newStr.length() - 1);
		}
		return newStr;
	}

	string getFileNameNoSuffix(const string& str, const bool removePath)
	{
		string newStr = str;
		rightToLeft(newStr);
		const int dotPos = (int)newStr.find_last_of('.');
		if (removePath)
		{
			const int namePos = (int)newStr.find_last_of('/');
			if (namePos != -1)
			{
				if (dotPos != -1)
				{
					return newStr.substr(namePos + 1, dotPos - namePos - 1);
				}
				else
				{
					return newStr.substr(namePos + 1);
				}
			}
		}
		if (dotPos != -1)
		{
			return newStr.substr(0, dotPos);
		}
		return newStr;
	}

	string getFirstFolderName(const string& dir)
	{
		string temp = dir;
		rightToLeft(temp);
		const int index = (int)temp.find_first_of('/');
		if (index == -1)
		{
			return dir;
		}
		return temp.substr(0, index);
	}

	string removeFirstPath(const string& dir)
	{
		string temp = dir;
		rightToLeft(temp);
		const int index = (int)temp.find_first_of('/');
		if (index == -1)
		{
			return dir;
		}
		return temp.substr(index + 1, temp.length() - index - 1);
	}

	string getFilePath(const string& dir, bool keepSlash)
	{
		string tempDir = dir;
		rightToLeft(tempDir);
		const int pos = (int)tempDir.find_last_of('/');
		if (keepSlash)
		{
			return pos != -1 ? dir.substr(0, pos + 1) : "./";
		}
		else
		{
			return pos != -1 ? dir.substr(0, pos) : ".";
		}
	}

	string getFileSuffix(const string& fileName, bool keepDot)
	{
		const int dotPos = (int)fileName.find_last_of('.');
		if (dotPos == -1)
		{
			return fileName;
		}
		if (keepDot)
		{
			return fileName.substr(dotPos, fileName.length() - dotPos);
		}
		else
		{
			return fileName.substr(dotPos + 1, fileName.length() - dotPos - 1);
		}
	}

	string removeStartString(const string& fileName, const string& startStr)
	{
		if (startWith(fileName, startStr.c_str()))
		{
			return fileName.substr(startStr.length(), fileName.length() - startStr.length());
		}
		return fileName;
	}

	string removeEndString(const string& fileName, const string& endStr)
	{
		if (endWith(fileName, endStr.c_str()))
		{
			return fileName.substr(0, fileName.length() - endStr.length());
		}
		return fileName;
	}

	int getFirstNotNumberPos(const string& str, const int startIndex)
	{
		for (int i = startIndex; i < (int)str.length(); ++i)
		{
			if (!isNumber(str[i]))
			{
				return i;
			}
		}
		return -1;
	}

	int getFirstNumberPos(const string& str)
	{
		FOR((int)str.length())
		{
			if (isNumber(str[i]))
			{
				return i;
			}
		}
		return -1;
	}

	int getLastNotNumberPos(const string& str)
	{
		FOR_INVERSE_I((int)str.length())
		{
			if (!isNumber(str[i]))
			{
				return i;
			}
		}
		return -1;
	}

	int getLastNumber(const string& str)
	{
		const int lastPos = getLastNotNumberPos(str);
		if (lastPos == -1)
		{
			return -1;
		}
		const string numStr = str.substr(lastPos + 1, str.length() - lastPos - 1);
		if (numStr.length() == 0)
		{
			return 0;
		}
		return SToI(numStr);
	}

	int getLastChar(const char* str, const char value)
	{
		FOR_INVERSE_I(strlength(str))
		{
			if (str[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	int getLastNotChar(const string& str, const char value)
	{
		FOR_INVERSE_I(str.length())
		{
			if (str[i] != value)
			{
				return i;
			}
		}
		return -1;
	}

	void splitLine(const char* str, Vector<string>& vec, const bool removeEmpty)
	{
		if (findString(str, "\r\n"))
		{
			split(str, "\r\n", vec, removeEmpty);
		}
		else if (findString(str, "\n"))
		{
			split(str, '\n', vec, removeEmpty);
		}
		else if (findString(str, "\r"))
		{
			split(str, '\r', vec, removeEmpty);
		}
	}

	void splitLine(const char* str, string* stringBuffer, const int bufferSize, const bool removeEmpty)
	{
		if (findString(str, "\r\n"))
		{
			split(str, "\r\n", stringBuffer, bufferSize, removeEmpty);
		}
		else if (findString(str, "\n"))
		{
			split(str, '\n', stringBuffer, bufferSize, removeEmpty);
		}
		else if (findString(str, "\r"))
		{
			split(str, '\r', stringBuffer, bufferSize, removeEmpty);
		}
	}

	void split(const char* str, const char key, Vector<string>& vec, const bool removeEmpty)
	{
		const int sourceLen = strlength(str);
		if (sourceLen == 0)
		{
			return;
		}
		int startPos = 0;
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		bool ret = true;
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
				return;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + 1;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			vec.push_back(curString.str());
		}
	}

	Vector<string> split(const char* str, const char key, const bool removeEmpty)
	{
		Vector<string> vec;
		split(str, key, vec, removeEmpty);
		return vec;
	}

	int split(const char* str, const char key, string* stringBuffer, const int bufferSize, const bool removeEmpty)
	{
		const int sourceLen = strlength(str);
		if (sourceLen == 0)
		{
			return 0;
		}
		int startPos = 0;
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		int curCount = 0;
		bool ret = true;
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
				return 0;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + 1;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			if (curCount >= bufferSize)
			{
				ERROR("string buffer is too small! bufferSize:" + IToS(bufferSize));
				break;
			}
			stringBuffer[curCount++] = curString.str();
		}
		return curCount;
	}

	void split(const char* str, const char* key, Vector<string>& vec, const bool removeEmpty)
	{
		const int sourceLen = strlength(str);
		if (sourceLen == 0)
		{
			return;
		}
		const int keyLen = strlength(key);
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		int devidePos = -1;
		int startPos = 0;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			vec.push_back(curString.str());
		}
	}

	void split(const string& str, const char* key, Vector<string>& vec, const bool removeEmpty)
	{
		const int sourceLen = (int)str.length();
		if (sourceLen == 0)
		{
			return;
		}
		int startPos = 0;
		const int keyLen = strlength(key);
		constexpr int STRING_BUFFER = 1024;
		MyString<STRING_BUFFER> curString;
		int devidePos = -1;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return;
			}
			curString.copy(str.c_str() + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 放入列表
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			vec.push_back(curString.str());
		}
	}

	int split(const char* str, const char* key, string* stringBuffer, const int bufferSize, const bool removeEmpty)
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
		int curCount = 0;
		bool ret = true;
		while (ret)
		{
			ret = findString(str, key, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + IToS(STRING_BUFFER) + "字节");
				return 0;
			}
			curString.copy(str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			if (curString[0] == '\0' && removeEmpty)
			{
				continue;
			}
			if (curCount >= bufferSize)
			{
				ERROR("string buffer is too small! bufferSize:" + IToS(bufferSize));
				break;
			}
			stringBuffer[curCount++] = curString.str();
		}
		return curCount;
	}

	void replace(char* str, const int strBufferSize, const int begin, const int end, const char* reStr)
	{
		const int curLength = strlength(str);
		const int replaceLength = strlength(reStr);
		if (begin + replaceLength + curLength - end >= strBufferSize)
		{
			ERROR("buffer is too small!");
			return;
		}
		if (begin + replaceLength != end)
		{
			memmove(str + begin + replaceLength, str + end, curLength - end);
		}
		MEMCPY(str + begin, strBufferSize - begin, reStr, replaceLength);
	}

	void replace(string& str, const int begin, const int end, const string& reStr)
	{
		const int replaceLength = (int)reStr.length();
		if (end - begin == replaceLength)
		{
			MEMCPY((void*)(str.c_str() + begin), str.length() - begin, reStr.c_str(), replaceLength);
		}
		else
		{
			str = str.substr(0, begin) + reStr + str.substr(end, str.length() - end);
		}
	}

	void replaceAll(char* str, const int strBufferSize, const char* key, const char* newWord)
	{
		const int keyLength = strlength(key);
		const int newWordsLength = strlength(newWord);
		int startPos = 0;
		while (true)
		{
			int pos = 0;
			if (!findString(str, key, &pos, startPos))
			{
				break;
			}
			replace(str, strBufferSize, pos, pos + keyLength, newWord);
			startPos = pos + newWordsLength;
		}
	}

	void replaceAll(string& str, const string& key, const string& newWord)
	{
		const int keyLength = (int)key.length();
		const int newWordsLength = (int)newWord.length();
		int startPos = 0;
		while (true)
		{
			int pos = 0;
			if (!findString(str, key.c_str(), &pos, startPos))
			{
				break;
			}
			replace(str, pos, pos + keyLength, newWord);
			startPos = pos + newWordsLength;
		}
	}

	void replaceAll(string& str, const char key, const char newWord)
	{
		FOR((int)str.length())
		{
			if (str[i] == key)
			{
				str[i] = newWord;
			}
		}
	}

	void removeAll(string& str, const char value)
	{
		int strLength = (int)str.length();
		CharArrayScope tempBuffer(getGreaterPower2(strLength + 1));
		setString(tempBuffer.mArray, strLength + 1, str);
		FOR_INVERSE_I(strLength)
		{
			if (tempBuffer.mArray[i] == value)
			{
				// 移动数据
				if (i != strLength - 1)
				{
					memmove(tempBuffer.mArray + i, tempBuffer.mArray + i + 1, strLength - i - 1);
				}
				--strLength;
			}
		}
		tempBuffer.mArray[strLength] = '\0';
		str = tempBuffer.mArray;
	}

	void removeAll(string& str, const char value0, const char value1)
	{
		int strLength = (int)str.length();
		CharArrayScope tempBuffer(getGreaterPower2(strLength + 1));
		setString(tempBuffer.mArray, strLength + 1, str);
		FOR_INVERSE_I(strLength)
		{
			if (tempBuffer.mArray[i] == value0 || tempBuffer.mArray[i] == value1)
			{
				// 移动数据
				if (i != strLength - 1)
				{
					memmove(tempBuffer.mArray + i, tempBuffer.mArray + i + 1, strLength - i - 1);
				}
				--strLength;
			}
		}
		tempBuffer.mArray[strLength] = '\0';
		str = tempBuffer.mArray;
	}

	bool removeString(char* str, const int length, const char* subString)
	{
		int subPos = 0;
		if (!findString(str, subString, &subPos, 0))
		{
			return false;
		}
		// 从子字符串的位置,后面的数据覆盖前面的数据
		const int subLength = strlength(subString);
		memmove(str + subPos, str + subPos + subLength, length - subLength - subPos);
		return true;
	}

	int strlength(const char* str, const int maxLength)
	{
		FOR(maxLength)
		{
			if (str[i] == '\0')
			{
				return i;
			}
		}
		return maxLength;
	}

	int strlength(const char* str)
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

	void strcat_s(char* destBuffer, const int size, const char* source)
	{
		if (source == nullptr || destBuffer == nullptr)
		{
			return;
		}
		const int destIndex = strlength(destBuffer, size);
		if (destIndex >= size)
		{
			ERROR("strcat_s buffer is too small");
			return;
		}
		const int length = strlength(source);
		MEMCPY(destBuffer + destIndex, size - destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}

	void strcat_s(char* destBuffer, const int size, const char* source, const int length)
	{
		if (source == nullptr || destBuffer == nullptr)
		{
			return;
		}
		const int destIndex = strlength(destBuffer, size);
		if (destIndex >= size)
		{
			ERROR("strcat_s buffer is too small");
			return;
		}
		MEMCPY(destBuffer + destIndex, size - destIndex, source, length);
		destBuffer[destIndex + length] = '\0';
	}

	void strcpy_s(char* destBuffer, const int size, const char* source)
	{
		if (source == nullptr)
		{
			return;
		}
		const int length = strlength(source);
		if (length >= size)
		{
			ERROR("strcat_s buffer is too small");
			return;
		}
		MEMCPY(destBuffer, size, source, length);
		destBuffer[length] = '\0';
	}

	string IToS(const int value, const int limitLen)
	{
		MyString<16> temp;
		const int len = _itoa_s(value, temp);
		if (limitLen > len)
		{
			return zeroString(limitLen - len) + temp.str();
		}
		return temp.str();
	}

	string UIToS(const int value, const int limitLen)
	{
		MyString<16> temp;
		const int len = _uitoa_s(value, temp);
		if (limitLen > len)
		{
			return zeroString(limitLen - len) + temp.str();
		}
		return temp.str();
	}

	string ULLToS(const ullong value, const int limitLen)
	{
		MyString<32> temp;
		const int len = _ui64toa_s(value, temp);
		if (limitLen > len)
		{
			return zeroString(limitLen - len) + temp.str();
		}
		return temp.str();
	}

	string LLToS(const llong value, const int limitLen)
	{
		MyString<32> temp;
		const int len = _i64toa_s(value, temp);
		if (limitLen > len)
		{
			return zeroString(limitLen - len) + temp.str();
		}
		return temp.str();
	}

	string ULLsToS(const Vector<ullong>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = ULLToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}

	string LLsToS(const Vector<llong>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		const int count = valueList.size();
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

	string LLsToS(const llong* valueList, const int valueCount, const char* seperate)
	{
		if (valueCount == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
		const int arrayLen = 32 * getGreaterPower2(valueCount);
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<32> temp;
		FOR(valueCount)
		{
			const int len = LLToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != valueCount - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}

	string bytesToString(const Vector<byte>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		const int arrayLen = 4 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = '\0';
		const int seperateLen = strlength(seperate);
		MyString<4> temp;
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

	string SsToS(const Vector<short>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		const int arrayLen = 8 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
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

	string USsToS(const Vector<ushort>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		const int arrayLen = 8 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
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

	string IsToS(const Vector<int>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		const int arrayLen = 16 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = '\0';
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
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

	string UIsToS(const Vector<uint>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}
		// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
		const int arrayLen = 16 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = UIToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}

	string FsToS(const Vector<float>& valueList, const char* seperate)
	{
		if (valueList.size() == 0)
		{
			return "";
		}

		const int arrayLen = 16 * getGreaterPower2(valueList.size());
		CharArrayScope charArray(arrayLen);
		charArray.mArray[0] = 0;
		const int seperateLen = strlength(seperate);
		MyString<16> temp;
		const int count = valueList.size();
		FOR(count)
		{
			const int len = FToS(temp, valueList[i]);
			strcat_s(charArray.mArray, arrayLen, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(charArray.mArray, arrayLen, seperate, seperateLen);
			}
		}
		return charArray.mArray;
	}

	void FsToS(char* buffer, const int bufferSize, const Vector<float>& valueList, const char* seperate)
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
			strcat_s(buffer, bufferSize, temp.str(), len);
			if (i != count - 1)
			{
				strcat_s(buffer, bufferSize, seperate, seperateLen);
			}
		}
	}

	void SToBs(const string& str, Vector<byte>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToI(curStr));
			}
		}
	}

	int SToBs(const char* str, byte* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
		int startPos = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
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
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}

	void SToSs(const string& str, Vector<short>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(strList.size());
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToI(curStr));
			}
		}
	}

	int SToSs(const char* str, short* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
		int startPos = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}

	void SToUSs(const string& str, Vector<ushort>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToI(curStr));
			}
		}
	}

	int SToUSs(const char* str, ushort* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
		int startPos = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}

	Vector<int> SToIs(const string& str, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		Vector<int> valueList;
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToI(curStr));
			}
		}
		return valueList;
	}

	void SToIs(const string& str, Vector<int>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(strList.size());
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToI(curStr));
			}
		}
	}

	int SToIs(const char* str, int* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
		int startPos = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}

	void SToUIs(const string& str, Vector<uint>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back((uint)SToI(curStr));
			}
		}
	}

	int SToUIs(const char* str, uint* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
		int startPos = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("uint buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToI(curString.str());
		}
		return curCount;
	}

	void stringToULLongs(const char* str, Vector<ullong>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(stringToULLong(curStr));
			}
		}
	}

	int stringToULLongs(const char* str, ullong* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("ullong buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = stringToULLong(curString.str());
		}
		return curCount;
	}

	void SToLLs(const char* str, Vector<llong>& valueList, const char* seperate)
	{
		int startPos = 0;
		const int sourceLen = strlength(str);
		const int keyLen = strlength(seperate);
		MyString<32> curString;
		int devidePos = -1;
		valueList.clear();
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
			valueList.push_back(SToLL(curString.str()));
		}
	}

	Vector<llong> SToLLs(const string& str, const char* seperate)
	{
		Vector<llong> list;
		SToLLs(str, list, seperate);
		return list;
	}

	void SToLLs(const string& str, Vector<llong>& valueList, const char* seperate)
	{
		int startPos = 0;
		const int keyLen = strlength(seperate);
		const int sourceLen = (int)str.length();
		MyString<32> curString;
		int devidePos = -1;
		valueList.clear();
		bool ret = true;
		while (ret)
		{
			ret = findString(str, seperate, &devidePos, startPos);
			// 无论是否查找到,都将前面一段字符串截取出来
			devidePos = ret ? devidePos : sourceLen;
			curString.copy(str.c_str() + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
			startPos = devidePos + keyLen;
			// 转换为长整数放入列表
			if (curString[0] == '\0')
			{
				continue;
			}
			valueList.push_back(SToLL(curString.str()));
		}
	}

	int SToLLs(const char* str, llong* buffer, const int bufferSize, const char* seperate)
	{
		int curCount = 0;
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
			if (curCount >= bufferSize)
			{
				ERROR("llong buffer size is too small, bufferSize:" + IToS(bufferSize));
				break;
			}
			buffer[curCount++] = SToLL(curString.str());
		}
		return curCount;
	}

	string zeroString(const int zeroCount)
	{
		MyString<16> charArray;
		FOR(zeroCount)
		{
			charArray[i] = '0';
		}
		return charArray.str();
	}

	string floatToStringExtra(const float f, const int precision, const bool removeTailZero)
	{
		const static string zeroDot = "0.";
		string retString;
		if (!isZero(f))
		{
			const float powerValue = powerFloat(10.0f, precision);
			const float totalValue = f * powerValue + MathUtility::sign(f) * 0.5f;
			if ((int)totalValue == 0)
			{
				if (precision > 0)
				{
					MyString<16> temp;
					zeroString(temp, precision);
					retString += zeroDot + temp.str();
				}
				else
				{
					retString = "0";
				}
			}
			else
			{
				retString = IToS(abs((int)totalValue));
				const int dotPosition = (int)retString.length() - precision;
				if (dotPosition <= 0)
				{
					MyString<16> tempZero;
					zeroString(tempZero, -dotPosition);
					retString = zeroDot + tempZero.str() + retString;
				}
				else
				{
					retString.insert(dotPosition, 1, '.');
				}
				// 为负数时,确保负号始终在最前面
				if ((int)totalValue < 0)
				{
					retString = "-" + retString;
				}
			}
		}
		else
		{
			if (precision > 0)
			{
				MyString<16> tempZero;
				zeroString(tempZero, precision);
				retString = zeroDot + tempZero.str();
			}
			else
			{
				retString = "0";
			}
		}
		// 移除末尾无用的0
		if (removeTailZero && retString[retString.length() - 1] == '0')
		{
			const int dotPos = (int)retString.find_last_of('.');
			if (dotPos != -1)
			{
				const string floatPart = retString.substr(dotPos + 1, retString.length() - dotPos - 1);
				// 找到最后一个不是0的位置,然后将后面的所有0都去掉
				const int notZeroPos = (int)floatPart.find_last_not_of('0');
				// 如果小数部分全是0,则将小数点也一起去掉
				if (notZeroPos == -1)
				{
					retString = retString.substr(0, dotPos);
				}
				// 去除小数部分末尾所有0
				else if (notZeroPos != (int)floatPart.length() - 1)
				{
					retString = retString.substr(0, dotPos + 1) + floatPart.substr(0, notZeroPos + 1);
				}
			}
		}
		return retString;
	}

	string FToS(const float f)
	{
		MyString<16> temp;
		FToS(temp, f);
		return temp.str();
	}

	void SToFs(const string& str, Vector<float>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			const string& curStr = strList[i];
			if (curStr.length() > 0)
			{
				valueList.push_back(SToF(curStr));
			}
		}
	}

	bool SToV2Is(const string& str, Vector<Vector2Int>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			bool result = false;
			Vector2Int value = SToV2I(strList[i], ",", &result);
			if (!result)
			{
				return false;
			}
			valueList.push_back(value);
		}
		return true;
	}
	bool SToV2s(const string& str, Vector<Vector2>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			bool result = false;
			Vector2 value = SToV2(strList[i], ",", &result);
			if (!result)
			{
				return false;
			}
			valueList.push_back(value);
		}
		return true;
	}
	bool SToV3Is(const string& str, Vector<Vector3Int>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			bool result = false;
			Vector3Int value = SToV3I(strList[i], ",", &result);
			if (!result)
			{
				return false;
			}
			valueList.push_back(value);
		}
		return true;
	}
	bool SToV3s(const string& str, Vector<Vector3>& valueList, const char* seperate)
	{
		Vector<string> strList;
		split(str, seperate, strList);
		const int count = strList.size();
		valueList.clearAndReserve(count);
		FOR(count)
		{
			bool result = false;
			Vector3 value = SToV3(strList[i], ",", &result);
			if (!result)
			{
				return false;
			}
			valueList.push_back(value);
		}
		return true;
	}

	Vector2 SToV2(const string& str, const char* seperate, bool* result)
	{
		ArrayList<2, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { SToF(valueList[0]), SToF(valueList[1]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector2Int SToV2I(const string& str, const char* seperate, bool* result)
	{
		ArrayList<2, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { SToI(valueList[0]), SToI(valueList[1]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector2UInt SToV2UI(const string& str, const char* seperate, bool* result)
	{
		ArrayList<2, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { (uint)SToI(valueList[0]), (uint)SToI(valueList[1]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector2Short SToV2S(const string& str, const char* seperate, bool* result)
	{
		ArrayList<2, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { (short)SToI(valueList[0]), (short)SToI(valueList[1]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector2UShort SToV2US(const string& str, const char* seperate, bool* result)
	{
		ArrayList<2, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { (ushort)SToI(valueList[0]), (ushort)SToI(valueList[1]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector3 SToV3(const string& str, const char* seperate, bool* result)
	{
		ArrayList<3, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { SToF(valueList[0]), SToF(valueList[1]), SToF(valueList[2]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	Vector3Int SToV3I(const string& str, const char* seperate, bool* result)
	{
		ArrayList<3, string> valueList;
		if (splitFull(str, seperate, valueList))
		{
			if (result != nullptr)
			{
				*result = true;
			}
			return { SToI(valueList[0]), SToI(valueList[1]), SToI(valueList[2]) };
		}
		if (result != nullptr)
		{
			*result = false;
		}
		return {};
	}

	string bytesToString(const char* buffer, const int length)
	{
		const int size = getGreaterPower2(length + 1);
		CharArrayScope tempBuffer(size);
		tempBuffer.mArray[length] = '\0';
		MEMCPY(tempBuffer.mArray, size, buffer, length);
		return tempBuffer.mArray;
	}

	bool endWith(const char* oriString, const char* pattern, const bool sensitive)
	{
		const int originLength = strlength(oriString);
		const int patternLength = strlength(pattern);
		if (originLength < patternLength)
		{
			return false;
		}
		if (sensitive)
		{
			FOR(patternLength)
			{
				if (oriString[i + originLength - patternLength] != pattern[i])
				{
					return false;
				}
			}
		}
		else
		{
			FOR(patternLength)
			{
				if (toLower(oriString[i + originLength - patternLength]) != toLower(pattern[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	bool endWith(const string& oriString, const char* pattern, const bool sensitive)
	{
		const int originLength = (int)oriString.length();
		const int patternLength = strlength(pattern);
		if (originLength < patternLength)
		{
			return false;
		}
		if (sensitive)
		{
			FOR(patternLength)
			{
				if (oriString[i + originLength - patternLength] != pattern[i])
				{
					return false;
				}
			}
		}
		else
		{
			FOR(patternLength)
			{
				if (toLower(oriString[i + originLength - patternLength]) != toLower(pattern[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	bool startWith(const char* oriString, const char* pattern, const bool sensitive)
	{
		const int originLength = strlength(oriString);
		const int patternLength = strlength(pattern);
		if (originLength < patternLength)
		{
			return false;
		}
		if (sensitive)
		{
			FOR(patternLength)
			{
				if (oriString[i] != pattern[i])
				{
					return false;
				}
			}
		}
		else
		{
			FOR(patternLength)
			{
				if (toLower(oriString[i]) != toLower(pattern[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	bool startWith(const string& oriString, const char* pattern, const bool sensitive)
	{
		const int originLength = (int)oriString.length();
		const int patternLength = strlength(pattern);
		if (originLength < patternLength)
		{
			return false;
		}
		if (sensitive)
		{
			FOR(patternLength)
			{
				if (oriString[i] != pattern[i])
				{
					return false;
				}
			}
		}
		else
		{
			FOR(patternLength)
			{
				if (toLower(oriString[i]) != toLower(pattern[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	string removePreNumber(const string& str)
	{
		FOR((int)str.length())
		{
			if (!isNumber(str[i]))
			{
				return str.substr(i);
			}
		}
		return str;
	}

#ifdef WIN32
	wstring ANSIToUnicode(const char* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return L"";
		}
		const int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str, -1, nullptr, 0);
		ArrayScope<wchar_t> pUnicode(getGreaterPower2(unicodeLen + 1));
		MultiByteToWideChar(CP_ACP, 0, str, -1, (LPWSTR)pUnicode.mArray, unicodeLen);
		return pUnicode.mArray;
	}

	void ANSIToUnicode(const char* str, wchar_t* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = L'\0';
			return;
		}
		const int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str, -1, nullptr, 0);
		if (unicodeLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		MultiByteToWideChar(CP_ACP, 0, str, -1, (LPWSTR)output, unicodeLen);
	}

	void ANSIToUnicode(const string& str, wchar_t* output, const int maxLength)
	{
		if (str.length() == 0)
		{
			output[0] = L'\0';
			return;
		}
		const int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, nullptr, 0);
		if (unicodeLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, (LPWSTR)output, unicodeLen);
	}

	string UnicodeToANSI(const wchar_t* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return "";
		}
		const int iTextLen = WideCharToMultiByte(CP_ACP, 0, str, -1, nullptr, 0, nullptr, nullptr);
		CharArrayScope pElementText(getGreaterPower2(iTextLen + 1));
		WideCharToMultiByte(CP_ACP, 0, str, -1, pElementText.mArray, iTextLen, nullptr, nullptr);
		return pElementText.mArray;
	}

	void UnicodeToANSI(const wchar_t* str, char* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = '\0';
			return;
		}
		const int iTextLen = WideCharToMultiByte(CP_ACP, 0, str, -1, nullptr, 0, nullptr, nullptr);
		if (iTextLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = '\0';
			return;
		}
		WideCharToMultiByte(CP_ACP, 0, str, -1, output, iTextLen, nullptr, nullptr);
	}

	string UnicodeToUTF8(const wchar_t* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return "";
		}
		// wide char to multi char
		const int iTextLen = WideCharToMultiByte(CP_UTF8, 0, str, -1, nullptr, 0, nullptr, nullptr);
		CharArrayScope pElementText(getGreaterPower2(iTextLen + 1));
		WideCharToMultiByte(CP_UTF8, 0, str, -1, pElementText.mArray, iTextLen, nullptr, nullptr);
		return pElementText.mArray;
	}

	void UnicodeToUTF8(const wchar_t* str, char* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = '\0';
			return;
		}
		// wide char to multi char
		const int iTextLen = WideCharToMultiByte(CP_UTF8, 0, str, -1, nullptr, 0, nullptr, nullptr);
		if (iTextLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = '\0';
			return;
		}
		WideCharToMultiByte(CP_UTF8, 0, str, -1, output, iTextLen, nullptr, nullptr);
	}

	wstring UTF8ToUnicode(const char* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return L"";
		}
		const int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
		ArrayScope<wchar_t> pUnicode(getGreaterPower2(unicodeLen + 1));
		MultiByteToWideChar(CP_UTF8, 0, str, -1, (LPWSTR)pUnicode.mArray, unicodeLen);
		return pUnicode.mArray;
	}

	void UTF8ToUnicode(const char* str, wchar_t* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = L'\0';
			return;
		}
		const int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
		if (unicodeLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		MultiByteToWideChar(CP_UTF8, 0, str, -1, (LPWSTR)output, unicodeLen);
	}

	void UTF8ToUnicode(const string& str, wchar_t* output, const int maxLength)
	{
		if (str.length() == 0)
		{
			output[0] = L'\0';
			return;
		}
		const int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str.c_str(), -1, nullptr, 0);
		if (unicodeLen >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		MultiByteToWideChar(CP_UTF8, 0, str.c_str(), -1, (LPWSTR)output, unicodeLen);
	}

#elif defined LINUX
	wstring ANSIToUnicode(const char* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return L"";
		}
		LocaleGBKScope scopeGBK(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str, 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return L"";
		}
		ArrayScope<wchar_t> dBuf(getGreaterPower2(dSize), true);
		if (mbstowcs(dBuf.mArray, str, dSize) < 0)
		{
			LOG("ANSIToUnicode转换编码错误:" + string(str) + ", needSize:" + IToS(dSize));
		}
		return dBuf.mArray;
	}

	void ANSIToUnicode(const char* str, wchar_t* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = L'\0';
			return;
		}
		LocaleGBKScope scope(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str, 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return;
		}
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		if (mbstowcs(output, str, dSize) < 0)
		{
			LOG("ANSIToUnicode转换编码错误:" + string(str) + ", needSize:" + IToS(dSize));
		}
	}

	void ANSIToUnicode(const string& str, wchar_t* output, const int maxLength)
	{
		if (str.length() == 0)
		{
			output[0] = L'\0';
			return;
		}
		LocaleGBKScope scope(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str.c_str(), 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return;
		}
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		if (mbstowcs(output, str.c_str(), dSize) < 0)
		{
			LOG("ANSIToUnicode转换编码错误:" + str + ", needSize:" + IToS(dSize));
		}
	}

	string UnicodeToANSI(const wchar_t* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return "";
		}
		LocaleGBKScope scopeGBK(mSetLocaleLock);
		const int dSize = wcstombs(nullptr, str, 0) + 1;
		CharArrayScope scope(getGreaterPower2(dSize));
		if (wcstombs(scope.mArray, str, dSize) < 0)
		{
			LOG("UnicodeToANSI转换编码错误, needSize:" + IToS(dSize));
		}
		return scope.mArray;
	}

	void UnicodeToANSI(const wchar_t* str, char* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = '\0';
			return;
		}
		LocaleGBKScope scopeGBK(mSetLocaleLock);
		const int dSize = wcstombs(nullptr, str, 0) + 1;
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = '\0';
			return;
		}
		if (wcstombs(output, str, dSize) < 0)
		{
			LOG("UnicodeToANSI转换编码错误, needSize:" + IToS(dSize));
		}
	}

	string UnicodeToUTF8(const wchar_t* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return "";
		}
		LocaleUTF8Scope scopeUTF8(mSetLocaleLock);
		const int dSize = wcstombs(nullptr, str, 0) + 1;
		CharArrayScope scope(getGreaterPower2(dSize));
		if (wcstombs(scope.mArray, str, dSize) < 0)
		{
			LOG("UnicodeToUTF8转换编码错误, needSize:" + IToS(dSize));
		}
		return scope.mArray;
	}

	void UnicodeToUTF8(const wchar_t* str, char* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = '\0';
			return;
		}
		LocaleUTF8Scope scope(mSetLocaleLock);
		const int dSize = wcstombs(nullptr, str, 0) + 1;
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = '\0';
			return;
		}
		if (wcstombs(output, str, dSize) < 0)
		{
			LOG("UnicodeToUTF8转换编码错误, needSize:" + IToS(dSize));
		}
	}

	wstring UTF8ToUnicode(const char* str)
	{
		if (str == nullptr || str[0] == 0)
		{
			return L"";
		}
		LocaleUTF8Scope scopeUTF8(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str, 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return L"";
		}
		ArrayScope<wchar_t> dBuf(getGreaterPower2(dSize), true);
		if (mbstowcs(dBuf.mArray, str, dSize) < 0)
		{
			LOG("UTF8ToUnicode转换编码错误:" + string(str) + ", needSize:" + IToS(dSize));
		}
		return dBuf.mArray;
	}

	void UTF8ToUnicode(const char* str, wchar_t* output, const int maxLength)
	{
		if (str == nullptr || str[0] == 0)
		{
			output[0] = L'\0';
			return;
		}
		LocaleUTF8Scope scope(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str, 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return;
		}
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		if (mbstowcs(output, str, dSize) < 0)
		{
			LOG("UTF8ToUnicode转换编码错误:" + string(str) + ", needSize:" + IToS(dSize));
		}
	}

	void UTF8ToUnicode(const string& str, wchar_t* output, const int maxLength)
	{
		if (str.length() == 0)
		{
			output[0] = L'\0';
			return;
		}
		LocaleUTF8Scope scope(mSetLocaleLock);
		const int dSize = mbstowcs(nullptr, str.c_str(), 0) + 1;
		if (dSize == 0)
		{
			ERROR("mbstowcs 执行失败,可能当前系统不支持某些编码");
			return;
		}
		if (dSize >= maxLength)
		{
			ERROR("buffer is too small");
			output[0] = L'\0';
			return;
		}
		if (mbstowcs(output, str.c_str(), dSize) < 0)
		{
			LOG("UTF8ToUnicode转换编码错误:" + str + ", needSize:" + IToS(dSize));
		}
	}
#endif

	string ANSIToUTF8(const char* str, const bool addBOM)
	{
		const int length = strlength(str);
		const int unicodeLength = getGreaterPower2((length + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		ANSIToUnicode(str, unicodeStr.mArray, unicodeLength);
		if (addBOM)
		{
			return BOM + UnicodeToUTF8(unicodeStr.mArray);
		}
		else
		{
			return UnicodeToUTF8(unicodeStr.mArray);
		}
	}

	string ANSIToUTF8(const string& str, const bool addBOM)
	{
		const int length = (int)str.length();
		const int unicodeLength = getGreaterPower2((length + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		ANSIToUnicode(str, unicodeStr.mArray, unicodeLength);
		if (addBOM)
		{
			return BOM + UnicodeToUTF8(unicodeStr.mArray);
		}
		else
		{
			return UnicodeToUTF8(unicodeStr.mArray);
		}
	}

	void ANSIToUTF8(const char* str, char* output, const int maxLength, const bool addBOM)
	{
		const int unicodeLength = getGreaterPower2((maxLength + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		ANSIToUnicode(str, unicodeStr.mArray, unicodeLength);
		if (addBOM)
		{
			MEMCPY(output, maxLength, BOM, 3);
			UnicodeToUTF8(unicodeStr.mArray, output + 3, maxLength - 3);
		}
		else
		{
			UnicodeToUTF8(unicodeStr.mArray, output, maxLength);
		}
	}

	void ANSIToUTF8(const string& str, char* output, const int maxLength, const bool addBOM)
	{
		const int unicodeLength = getGreaterPower2((maxLength + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		ANSIToUnicode(str, unicodeStr.mArray, unicodeLength);
		if (addBOM)
		{
			MEMCPY(output, maxLength, BOM, 3);
			UnicodeToUTF8(unicodeStr.mArray, output + 3, maxLength - 3);
		}
		else
		{
			UnicodeToUTF8(unicodeStr.mArray, output, maxLength);
		}
	}

	string UTF8ToANSI(const char* str, const bool eraseBOM)
	{
		const int length = strlength(str);
		const int unicodeLength = getGreaterPower2((length + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		if (eraseBOM)
		{
			string newStr = str;
			removeBOM(newStr);
			UTF8ToUnicode(newStr, unicodeStr.mArray, unicodeLength);
		}
		else
		{
			UTF8ToUnicode(str, unicodeStr.mArray, unicodeLength);
		}
		return UnicodeToANSI(unicodeStr.mArray);
	}

	string UTF8ToANSI(const string& str, const bool eraseBOM)
	{
		const int length = (int)str.length();
		const int unicodeLength = getGreaterPower2((length + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		if (eraseBOM)
		{
			string newStr = str;
			removeBOM(newStr);
			UTF8ToUnicode(newStr, unicodeStr.mArray, unicodeLength);
		}
		else
		{
			UTF8ToUnicode(str, unicodeStr.mArray, unicodeLength);
		}
		return UnicodeToANSI(unicodeStr.mArray);
	}

	void UTF8ToANSI(const char* str, char* output, const int maxLength, const bool eraseBOM)
	{
		const int unicodeLength = getGreaterPower2((maxLength + 1) << 1);
		ArrayScope<wchar_t> unicodeStr(unicodeLength);
		if (eraseBOM)
		{
			const char* newInput = str;
			const int length = strlength(str);
			if (length >= 3 && str[0] == BOM[0] && str[1] == BOM[1] && str[2] == BOM[2])
			{
				newInput += 3;
			}
			UTF8ToUnicode(newInput, unicodeStr.mArray, unicodeLength);
		}
		else
		{
			UTF8ToUnicode(str, unicodeStr.mArray, unicodeLength);
		}
		UnicodeToANSI(unicodeStr.mArray, output, maxLength);
	}

	void removeBOM(string& str)
	{
		if (str.length() >= 3 && str[0] == BOM[0] && str[1] == BOM[1] && str[2] == BOM[2])
		{
			str.erase(0, 3);
		}
	}

	void removeBOM(char* str, int length)
	{
		if (length == 0)
		{
			length = strlength(str);
		}
		if (length >= 3 && str[0] == BOM[0] && str[1] == BOM[1] && str[2] == BOM[2])
		{
			memmove(str, str + 3, length - 3 + 1);
		}
	}

	void jsonStartArray(string& str, const int preTableCount, const bool returnLine)
	{
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "[";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	void jsonEndArray(string& str, const int preTableCount, const bool returnLine)
	{
		removeLastComma(str);
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "],";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	void jsonStartStruct(string& str, const int preTableCount, const bool returnLine)
	{
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "{";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	void jsonEndStruct(string& str, const int preTableCount, const bool returnLine)
	{
		removeLastComma(str);
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "},";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	void jsonAddPair(string& str, const string& name, const string& value, const int preTableCount, const bool returnLine)
	{
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "\"" + name + "\":\"" + value + "\",";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	void jsonAddObject(string& str, const string& name, const string& value, const int preTableCount, const bool returnLine)
	{
		FOR(preTableCount)
		{
			str += "\t";
		}
		str += "\"" + name + "\":" + value + ",";
		if (returnLine)
		{
			str += "\r\n";
		}
	}

	string toLower(const string& str)
	{
		string ret = str;
		FOR((int)ret.length())
		{
			ret[i] = toLower(ret[i]);
		}
		return ret;
	}

	string toUpper(const string& str)
	{
		string ret = str;
		FOR((int)ret.length())
		{
			ret[i] = toUpper(ret[i]);
		}
		return ret;
	}

	void rightToLeft(string& str)
	{
		FOR((int)str.length())
		{
			if (str[i] == '\\')
			{
				str[i] = '/';
			}
		}
	}
	void leftToRight(string& str)
	{
		FOR((int)str.length())
		{
			if (str[i] == '/')
			{
				str[i] = '\\';
			}
		}
	}
	bool findStringLower(const string& res, const string& sub, int* pos, const int startIndex, const bool direction)
	{
		// 全转换为小写
		return findString(toLower(res), toLower(sub).c_str(), pos, startIndex, direction);
	}

	bool findString(const string& res, const char* sub, int* pos, const int startIndex, const bool direction)
	{
		// 这里只是不再通过strlength获取字符串长度,而是直接string.length()获取,其余完全一致
		int posFind = -1;
		const int subLen = strlength(sub);
		const int searchLength = (int)res.length() - subLen + 1;
		if (searchLength <= 0)
		{
			return false;
		}
		if (direction)
		{
			for (int i = startIndex; i < searchLength; ++i)
			{
				int j = 0;
				for (; j < subLen; ++j)
				{
					if (res[i + j] != sub[j])
					{
						break;
					}
				}
				if (j == subLen)
				{
					posFind = i;
					break;
				}
			}
		}
		else
		{
			for (int i = searchLength - 1; i >= startIndex; --i)
			{
				int j = 0;
				for (; j < subLen; ++j)
				{
					if (res[i + j] != sub[j])
					{
						break;
					}
				}
				if (j == subLen)
				{
					posFind = i;
					break;
				}
			}
		}
		if (pos != nullptr)
		{
			*pos = posFind;
		}
		return posFind != -1;
	}

	bool findString(const char* res, const char* sub, int* pos, const int startIndex, const bool direction)
	{
		int posFind = -1;
		const int subLen = strlength(sub);
		const int searchLength = strlength(res) - subLen + 1;
		if (searchLength <= 0)
		{
			return false;
		}
		if (direction)
		{
			for (int i = startIndex; i < searchLength; ++i)
			{
				int j = 0;
				for (; j < subLen; ++j)
				{
					if (res[i + j] != sub[j])
					{
						break;
					}
				}
				if (j == subLen)
				{
					posFind = i;
					break;
				}
			}
		}
		else
		{
			for (int i = searchLength - 1; i >= startIndex; --i)
			{
				int j = 0;
				for (; j < subLen; ++j)
				{
					if (res[i + j] != sub[j])
					{
						break;
					}
				}
				if (j == subLen)
				{
					posFind = i;
					break;
				}
			}
		}
		if (pos != nullptr)
		{
			*pos = posFind;
		}
		return posFind != -1;
	}

	int findStringPos(const char* res, const char* dst, const int startIndex, const bool direction)
	{
		int pos = -1;
		findString(res, dst, &pos, startIndex, direction);
		return pos;
	}

	int findStringPos(const string& res, const string& dst, const int startIndex, const bool direction)
	{
		int pos = -1;
		findString(res, dst.c_str(), &pos, startIndex, direction);
		return pos;
	}

	bool checkString(const string& str, const string& valid)
	{
		FOR((int)str.length())
		{
			if (!valid.find_first_of(str[i]))
			{
				return false;
			}
		}
		return true;
	}

	bool hasNonAscii(const char* str)
	{
		FOR(strlength(str))
		{
			if ((byte)str[i] > 0x7F)
			{
				return false;
			}
		}
		return true;
	}

	string charToHexString(byte value, bool upper)
	{
		MyString<3> byteHex;
		const char* charPool = upper ? "ABCDEF" : "abcdef";
		const byte highBit = value >> 4;
		// 高字节的十六进制
		byteHex[0] = (highBit < (byte)10) ? ('0' + highBit) : charPool[highBit - 10];
		// 低字节的十六进制
		const byte lowBit = value & 0x0F;
		byteHex[1] = (lowBit < (byte)10) ? ('0' + lowBit) : charPool[lowBit - 10];
		return byteHex.str();
	}

	string bytesToHexString(const byte* data, int dataCount, bool addSpace, bool upper)
	{
		const int oneLength = addSpace ? 3 : 2;
		CharArrayScope byteData(getGreaterPower2(dataCount * oneLength + 1));
		FOR_J(dataCount)
		{
			const string byteStr = charToHexString(data[j]);
			byteData.mArray[j * oneLength + 0] = byteStr[0];
			byteData.mArray[j * oneLength + 1] = byteStr[1];
			if (oneLength >= 3)
			{
				byteData.mArray[j * oneLength + 2] = ' ';
			}
		}
		byteData.mArray[dataCount * oneLength] = '\0';
		return byteData.mArray;
	}

	bool isNumber(const string& str)
	{
		FOR(str.length())
		{
			if (!isNumber(str[i]))
			{
				return false;
			}
		}
		return true;
	}

	int getCharCount(const string& str, char key)
	{
		int count = 0;
		FOR((int)str.length())
		{
			if (str[i] == key)
			{
				++count;
			}
		}
		return count;
	}

	int getCharCount(const char* str, char key)
	{
		int count = 0;
		int index = 0;
		while (true)
		{
			if (str[index] == '\0')
			{
				break;
			}
			if (str[index] == key)
			{
				++count;
			}
			++index;
		}
		return count;
	}

	bool isPhoneNumber(const char* str)
	{
		// 手机号固定11位
		const int length = strlength(str);
		if (length != 11 || str[0] != '1')
		{
			return false;
		}
		FOR(length)
		{
			if (!isNumber(str[i]))
			{
				return false;
			}
		}
		return true;
	}

	bool hasControlChar(const string& str)
	{
		int index = 0;
		while (str[index] != '\0')
		{
			if (iscntrl((byte)str[index]))
			{
				return true;
			}
			++index;
		}
		return false;
	}

	bool hasInvisibleChar(const string& str)
	{
		// 不能有控制字符,不能有英文空格,不能有中文空格
		if (hasControlChar(str))
		{
			return true;
		}
		if (ChineseSpace.empty())
		{
			UNIFIED_UTF8(chineseSpace, 8, "　");
			ChineseSpace = chineseSpace.str();
		}
		return findString(str, ChineseSpace.c_str()) || findString(str, " ");
	}

	void initIntToString()
	{
		// double check 防止在多线程执行时多次重复初始化
		if (mIntString[0].length() == 0)
		{
			if (mIntString[0].length() > 0)
			{
				return;
			}
			FOR(mIntString.size())
			{
				if (i == 0)
				{
					mIntString[i] = "0";
					continue;
				}
				constexpr int SIZE = 16;
				MyString<SIZE> buffer;
				int index = 0;
				while (true)
				{
					// 将数字放入buffer的尾部
					if ((llong)i < POWER_INT_10[index])
					{
						break;
					}
					buffer[SIZE - 1 - index] = (int)((llong)i % POWER_INT_10[index + 1] / POWER_INT_10[index]);
					++index;
				}
				// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
				const int lengthToHead = SIZE - index;
				FOR_J(index)
				{
					buffer[j] = buffer[j + lengthToHead] + '0';
				}
				mIntString[i] = buffer.str();
			}
		}
	}

	int greaterPower2(const int value)
	{
		return getGreaterPower2(value);
	}

	string base64_encode(const byte* str, const int length)
	{
		char* encode = new char[4 * ((length + 2) / 3) + 1];
		int i, j;
		for (i = 0, j = 0; i + 3 <= length; i += 3)
		{
			// 取出第一个字符的前6位并找出对应的结果字符
			encode[j++] = alphabet_map[str[i] >> 2];
			// 将第一个字符的后2位与第二个字符的前4位进行组合并找到对应的结果字符
			encode[j++] = alphabet_map[((str[i] << 4) & 0x30) | (str[i + 1] >> 4)];
			// 将第二个字符的后4位与第三个字符的前2位组合并找出对应的结果字符
			encode[j++] = alphabet_map[((str[i + 1] << 2) & 0x3c) | (str[i + 2] >> 6)];
			// 取出第三个字符的后6位并找出结果字符
			encode[j++] = alphabet_map[str[i + 2] & 0x3f];
		}

		if (i < length)
		{
			const int tail = length - i;
			if (tail == 1)
			{
				encode[j++] = alphabet_map[str[i] >> 2];
				encode[j++] = alphabet_map[(str[i] << 4) & 0x30];
				encode[j++] = '=';
				encode[j++] = '=';
			}
			else
			{
				encode[j++] = alphabet_map[str[i] >> 2];
				encode[j++] = alphabet_map[((str[i] << 4) & 0x30) | (str[i + 1] >> 4)];
				encode[j++] = alphabet_map[(str[i + 1] << 2) & 0x3c];
				encode[j++] = '=';
			}
		}
		encode[j] = '\0';
		string temp = encode;
		delete[] encode;
		return temp;
	}

	string base64_encode(const string& str)
	{
		return base64_encode((byte*)str.c_str(), (int)str.length());
	}

	string base64_decode(const char* code, const int codeLen)
	{
		byte* dest = new byte[4 * ((codeLen + 2) / 3) + 1];
		int j = 0;
		byte quad[4] = {};
		for (int i = 0; i < codeLen; i += 4)
		{
			// 分组，每组四个分别依次转换为base64表内的十进制数
			for (int k = 0; k < 4; k++)
			{
				quad[k] = reverse_map[(int)code[i + k]];
			}

			// 取出第一个字符对应base64表的十进制数的前6位与第二个字符对应base64表的十进制数的前2位进行组合
			dest[j++] = (quad[0] << 2) | (quad[1] >> 4);

			if (quad[2] >= 64)
			{
				break;
			}
			else if (quad[3] >= 64)
			{
				// 取出第二个字符对应base64表的十进制数的后4位与第三个字符对应base64表的十进制数的前4位进行组合
				dest[j++] = (quad[1] << 4) | (quad[2] >> 2);
				break;
			}
			else
			{
				// 取出第三个字符对应base64表的十进制数的后2位与第4个字符进行组合
				dest[j++] = (quad[1] << 4) | (quad[2] >> 2);
				dest[j++] = (quad[2] << 6) | quad[3];
			}
		}
		dest[j] = '\0';
		string str = (char*)dest;
		delete[] dest;
		return str;
	}

	string base64_decode(const string& code)
	{
		return base64_decode(code.c_str(), (int)code.length());
	}

	// 工具函数：按位循环左移
	uint32_t rotate_left(const uint32_t value, const int shift)
	{
		return (value << shift) | (value >> (32 - shift));
	}
	// 工具函数：填充消息
	vector<uint8_t> pad_message(const string& message)
	{
		vector<uint8_t> padded_message(message.begin(), message.end());
		uint64_t message_length_bits = message.size() * 8;
		// 添加 '1' 位
		padded_message.push_back(0x80);
		// 填充零直到长度对 512 取余为 448
		while ((padded_message.size() * 8) % 512 != 448) 
		{
			padded_message.push_back(0);
		}
		// 添加原始消息长度（64 位，大端序）
		for (int i = 7; i >= 0; --i) 
		{
			padded_message.push_back((message_length_bits >> (i * 8)) & 0xFF);
		}
		return padded_message;
	}
	// 工具函数：将 4 个字节拼成一个 32 位整数（大端序）
	uint32_t bytes_to_uint32(const uint8_t* bytes)
	{
		return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
	}
	// 工具函数：将 32 位整数分解为 4 个字节（大端序）
	void uint32_to_bytes(const uint32_t value, uint8_t* bytes)
	{
		bytes[0] = (value >> 24) & 0xFF;
		bytes[1] = (value >> 16) & 0xFF;
		bytes[2] = (value >> 8) & 0xFF;
		bytes[3] = value & 0xFF;
	}
	// buffer的大小应该是20
	void sha1(const string& message, byte* buffer)
	{
		// 初始化哈希值
		uint32_t H0 = 0x67452301;
		uint32_t H1 = 0xEFCDAB89;
		uint32_t H2 = 0x98BADCFE;
		uint32_t H3 = 0x10325476;
		uint32_t H4 = 0xC3D2E1F0;
		// 消息填充
		vector<uint8_t> padded_message = pad_message(message);
		// 处理每个 512 位的消息块
		for (size_t i = 0; i < padded_message.size(); i += 64)
		{
			uint32_t W[80]{}; // 消息调度数组
			// 初始化 W 前 16 个字
			for (int j = 0; j < 16; ++j)
			{
				W[j] = bytes_to_uint32(&padded_message[i + j * 4]);
			}
			// 扩展 W 到 80 个字
			for (int j = 16; j < 80; ++j)
			{
				W[j] = rotate_left(W[j - 3] ^ W[j - 8] ^ W[j - 14] ^ W[j - 16], 1);
			}
			// 初始化工作变量
			uint32_t a = H0;
			uint32_t b = H1;
			uint32_t c = H2;
			uint32_t d = H3;
			uint32_t e = H4;
			// 主循环
			for (int j = 0; j < 80; ++j)
			{
				uint32_t f, k;
				if (j < 20)
				{
					f = (b & c) | (~b & d);
					k = 0x5A827999;
				}
				else if (j < 40)
				{
					f = b ^ c ^ d;
					k = 0x6ED9EBA1;
				}
				else if (j < 60)
				{
					f = (b & c) | (b & d) | (c & d);
					k = 0x8F1BBCDC;
				}
				else
				{
					f = b ^ c ^ d;
					k = 0xCA62C1D6;
				}
				uint32_t temp = rotate_left(a, 5) + f + e + k + W[j];
				e = d;
				d = c;
				c = rotate_left(b, 30);
				b = a;
				a = temp;
			}
			// 更新哈希值
			H0 += a;
			H1 += b;
			H2 += c;
			H3 += d;
			H4 += e;
		}

		// 拼接最终哈希值
		uint32_to_bytes(H0, buffer + 0);
		uint32_to_bytes(H1, buffer + 4);
		uint32_to_bytes(H2, buffer + 8);
		uint32_to_bytes(H3, buffer + 12);
		uint32_to_bytes(H4, buffer + 16);
	}
	
	// 主 SHA-1 算法
	string sha1(const string& message)
	{
		byte digest[20];
		sha1(message, digest);
		// 转换为十六进制字符串
		std::ostringstream oss;
		for (int i = 0; i < 20; ++i) 
		{
			oss << std::hex << std::setw(2) << std::setfill('0') << (int)digest[i];
		}
		return oss.str();
	}
}