#include "Utility.h"

const char StringUtility::BOM[4]{ (char)0xEF, (char)0xBB, (char)0xBF, 0 };
myVector<int> StringUtility::mTempIntList;

string StringUtility::removeStartString(const string& fileName, const string& startStr)
{
	if (startWith(fileName, startStr.c_str()))
	{
		return fileName.substr(startStr.length(), fileName.length() - startStr.length());
	}
	return fileName;
}

string StringUtility::removeSuffix(const string& str)
{
	int dotPos = (int)str.find_last_of('.');
	if (dotPos != -1)
	{
		return str.substr(0, dotPos);
	}
	return str;
}

void StringUtility::removeStartAll(string& stream, char key)
{
	int firstNotPos = -1;
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[i] != key)
		{
			firstNotPos = i;
			break;
		}
	}
	if (firstNotPos > 0)
	{
		stream.erase(0, firstNotPos);
	}
}

void StringUtility::removeStart(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[streamSize - i - 1] == key)
		{
			stream.erase(streamSize - i - 1, 1);
			break;
		}
	}
}

void StringUtility::removeLastAll(string& stream, char key)
{
	int lastNotPos = -1;
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[streamSize - i - 1] != key)
		{
			lastNotPos = streamSize - i - 1;
			break;
		}
	}
	if (lastNotPos != -1 && lastNotPos != streamSize - 1)
	{
		stream.erase(lastNotPos + 1);
	}
}

void StringUtility::removeLast(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[streamSize - i - 1] == key)
		{
			stream.erase(streamSize - i - 1, 1);
			break;
		}
	}
}

void StringUtility::removeLastComma(string& stream)
{
	removeLast(stream, ',');
}

void StringUtility::removeEnd(string& str, char key)
{
	if (str[str.length() - 1] == key)
	{
		str.erase(str.length() - 1, 1);
	}
}

int StringUtility::findCharCount(const string& str, char key)
{
	int count = 0;
	uint length = (uint)str.length();
	FOR_I(length)
	{
		if (str[i] == key)
		{
			++count;
		}
	}
	return count;
}

int StringUtility::findFirstNonEmptyChar(const string& str)
{
	// 不能有控制字符,不能有英文空格,不能有中文空格
	int index = 0;
	while (str[index] != '\0')
	{
		if (!iscntrl(str[index]) && str[index] != ' ')
		{
			return index;
		}
		++index;
	}
	return -1;
}

string StringUtility::getFileName(string str)
{
	rightToLeft(str);
	int dotPos = (int)str.find_last_of('/');
	if (dotPos != -1)
	{
		return str.substr(dotPos + 1, str.length() - 1);
	}
	return str;
}

string StringUtility::getFileNameNoSuffix(string str, bool removePath)
{
	rightToLeft(str);
	int dotPos = (int)str.find_last_of('.');
	if (removePath)
	{
		int namePos = (int)str.find_last_of('/');
		if (namePos != -1)
		{
			if (dotPos != -1)
			{
				return str.substr(namePos + 1, dotPos - namePos - 1);
			}
			else
			{
				return str.substr(namePos + 1);
			}
		}
	}
	else
	{
		if (dotPos != -1)
		{
			return str.substr(0, dotPos);
		}
	}
	return str;
}

string StringUtility::getFilePath(const string& dir)
{
	if (dir.length() == 0)
	{
		return dir;
	}
	string tempDir = dir;
	rightToLeft(tempDir);
	if (tempDir[tempDir.length() - 1] == '/')
	{
		tempDir.erase(tempDir.length() - 1);
	}
	int pos = (int)tempDir.find_last_of('/');
	return pos != -1 ? dir.substr(0, pos) : "./";
}

string StringUtility::getFileSuffix(const string& fileName)
{
	int dotPos = (int)fileName.find_last_of('.');
	if (dotPos != -1)
	{
		return fileName.substr(dotPos + 1, fileName.length() - dotPos - 1);
	}
	return fileName;
}

string StringUtility::replaceSuffix(const string& fileName, const string& suffix)
{
	return getFileNameNoSuffix(fileName, false) + suffix;
}

int StringUtility::getLastNotNumberPos(const string& str)
{
	uint strLen = (uint)str.length();
	FOR_I(strLen)
	{
		if (str[strLen - i - 1] > '9' || str[strLen - i - 1] < '0')
		{
			return strLen - i - 1;
		}
	}
	return -1;
}

int StringUtility::getLastNumber(const string& str)
{
	int lastPos = getLastNotNumberPos(str);
	if (lastPos == -1)
	{
		return -1;
	}
	string numStr = str.substr(lastPos + 1, str.length() - lastPos - 1);
	if (numStr.length() == 0)
	{
		return 0;
	}
	return stringToInt(numStr);
}

string StringUtility::getNotNumberSubString(string str)
{
	return str.substr(0, getLastNotNumberPos(str) + 1);
}

void StringUtility::replace(char* str, int strBufferSize, int begin, int end, const char* reStr)
{
	uint curLength = (uint)strlen(str);
	uint replaceLength = (uint)strlen(reStr);
	int lengthAfterReplace = begin + replaceLength + curLength - end;
	if (lengthAfterReplace >= strBufferSize)
	{
		ERROR("buffer is too small!");
		return;
	}
	memmove(str + begin + replaceLength, str + end, curLength - end);
	memcpy(str + begin, reStr, replaceLength);
}

void StringUtility::replace(string& str, int begin, int end, const string& reStr)
{
	if (begin < 0 || end < 0)
	{
		return;
	}
	string sub1 = str.substr(0, begin);
	string sub2 = str.substr(end, str.length() - end);
	str = sub1 + reStr + sub2;
}

void StringUtility::replaceAll(char* str, int strBufferSize, const char* key, const char* newWords)
{
	uint keyLength = (uint)strlen(key);
	uint newWordsLength = (uint)strlen(newWords);
	int startPos = 0;
	while (true)
	{
		int pos = 0;
		if (!findString(str, key, &pos, startPos))
		{
			break;
		}
		replace(str, strBufferSize, pos, pos + keyLength, newWords);
		startPos = pos + newWordsLength;
	}
}

void StringUtility::replaceAll(string& str, const string& key, const string& newWords)
{
	uint keyLength = (uint)key.length();
	uint newWordsLength = (uint)newWords.length();
	int startPos = 0;
	while (true)
	{
		int pos = 0;
		if (!findString(str.c_str(), key.c_str(), &pos, startPos))
		{
			break;
		}
		replace(str, pos, pos + keyLength, newWords);
		startPos = pos + newWordsLength;
	}
}

void StringUtility::removeAll(string& str, char value)
{
	int strLength = (int)str.length();
	char* tempBuffer = ArrayPool::newArray<char>(MathUtility::getGreaterPower2(strLength + 1));
	memcpy(tempBuffer, str.c_str(), strLength);
	tempBuffer[strLength] = '\0';
	for (int i = strLength; i >= 0; --i)
	{
		if (tempBuffer[i] == value)
		{
			// 移动数据
			if (i != strLength - 1)
			{
				memmove(tempBuffer + i, tempBuffer + i + 1, strLength - i - 1);
			}
			--strLength;
		}
	}
	tempBuffer[strLength] = '\0';
	str.assign(tempBuffer);
}

void StringUtility::removeAll(string& str, char value0, char value1)
{
	int strLength = (int)str.length();
	char* tempBuffer = ArrayPool::newArray<char>(MathUtility::getGreaterPower2(strLength + 1));
	memcpy(tempBuffer, str.c_str(), strLength);
	tempBuffer[strLength] = '\0';
	for (int i = strLength; i >= 0; --i)
	{
		if (tempBuffer[i] == value0 || tempBuffer[i] == value1)
		{
			// 移动数据
			if (i != strLength - 1)
			{
				memmove(tempBuffer + i, tempBuffer + i + 1, strLength - i - 1);
			}
			--strLength;
		}
	}
	tempBuffer[strLength] = '\0';
	str.assign(tempBuffer);
}

void StringUtility::split(const char* str, const char* key, myVector<string>& vec, bool removeEmpty)
{
	int startPos = 0;
	int keyLen = (int)strlen(key);
	int sourceLen = (int)strlen(str);
	const int STRING_BUFFER = 1024;
	char curString[STRING_BUFFER];
	int devidePos = -1;
	while (true)
	{
		bool ret = findString(str, key, &devidePos, startPos);
		// 无论是否查找到,都将前面一段字符串截取出来
		if (ret)
		{
			if (devidePos - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节, 字符串:" + str + ", key:" + key);
				return;
			}
			MEMCPY(curString, STRING_BUFFER, str + startPos, devidePos - startPos);
			curString[devidePos - startPos] = '\0';
		}
		else
		{
			if (sourceLen - startPos >= STRING_BUFFER)
			{
				ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节, 字符串:" + str + ", key:" + key);
				return;
			}
			MEMCPY(curString, STRING_BUFFER, str + startPos, sourceLen - startPos);
			curString[sourceLen - startPos] = '\0';
		}
		// 放入列表
		if (curString[0] != '\0' || !removeEmpty)
		{
			vec.push_back(curString);
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
}

uint StringUtility::split(const char* str, const char* key, string* stringBuffer, uint bufferSize, bool removeEmpty)
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
			if (curCount >= bufferSize)
			{
				ERROR("string buffer is too small! bufferSize:" + intToString(bufferSize));
			}
			stringBuffer[curCount++] = curString;
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

string StringUtility::strReplace(const string& str, uint begin, uint end, const string& reStr)
{
	string sub1 = str.substr(0, begin);
	string sub2 = str.substr(end, str.length() - end);
	return sub1 + reStr + sub2;
}

void StringUtility::strReplaceAll(string& str, const char* key, const string& newWords)
{
	int keyLen = (int)strlen(key);
	int startPos = 0;
	while (true)
	{
		int pos = -1;
		if (!findSubstr(str, key, &pos, startPos))
		{
			break;
		}
		str = strReplace(str, pos, pos + keyLen, newWords);
		startPos = pos + (int)newWords.length();
	}
}

void StringUtility::_itoa_s(int value, char* charArray, uint size)
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
		if ((sign > 0 && index >= size) ||
			(sign < 0 && index >= size - 1))
		{
			ERROR("buffer is too small!");
			break;
		}
		// 将数字放入numberArray的尾部
		if ((ullong)value < power[index])
		{
			break;
		}
		charArray[size - 1 - index] = (int)((ullong)value % power[index + 1] / power[index]);
		++index;
	}
	// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
	if (sign > 0)
	{
		uint lengthToHead = size - index;
		FOR_I(index)
		{
			charArray[i] = charArray[i + lengthToHead] + '0';
		}
		charArray[index] = '\0';
	}
	else
	{
		uint lengthToHead = size - index;
		FOR_I(index)
		{
			charArray[i + 1] = charArray[i + lengthToHead] + '0';
		}
		charArray[0] = '-';
		charArray[index + 1] = '\0';
	}
}

void StringUtility::_i64toa_s(const ullong& value, char* charArray, uint size)
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
		// 如果是正数,则数字个数不能超过size - 1
		if (index >= size)
		{
			ERROR("buffer is too small!");
			break;
		}
		// 将数字放入numberArray的尾部
		if (value < power[index])
		{
			break;
		}
		charArray[size - 1 - index] = (int)(value % power[index + 1] / power[index]);
		++index;
	}
	// 将数字从数组末尾移动到开头,并且将数字转换为数字字符
	uint lengthToHead = size - index;
	FOR_I(index)
	{
		charArray[i] = charArray[i + lengthToHead] + '0';
	}
	charArray[index] = '\0';
}

void StringUtility::strcat_s(char* destBuffer, uint size, const char* source)
{
	FOR_I((uint)size)
	{
		// 找到字符串的末尾
		if (destBuffer[i] == '\0')
		{
			uint index = 0;
			while (true)
			{
				if (index + i >= (uint)size)
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

void StringUtility::strcpy_s(char* destBuffer, uint size, const char* source)
{
	uint index = 0;
	while (true)
	{
		if (index >= size)
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

string StringUtility::intToString(int value, uint limitLen)
{
	array<char, 16> temp;
	_itoa_s(value, temp);
	uint len = (uint)strlen(temp._Elems);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp._Elems;
	}
	return temp._Elems;
}

void StringUtility::intToString(char* charArray, uint size, int value, uint limitLen)
{
	if (limitLen == 0)
	{
		_itoa_s(value, charArray, size);
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
				STRCAT2_N(charArray, size, zeroArray._Elems, temp._Elems);
				return;
			}
		}
		strcpy_s(charArray, size, temp._Elems);
	}
}

string StringUtility::ullongToString(const ullong& value, uint limitLen)
{
	array<char, 32> temp;
	_i64toa_s(value, temp);
	uint len = (uint)strlen(temp._Elems);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp._Elems;
	}
	return temp._Elems;
}

void StringUtility::ullongToString(char* charArray, uint size, const ullong& value, uint limitLen)
{
	if (limitLen == 0)
	{
		_i64toa_s(value, charArray, size);
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
				STRCAT2_N(charArray, size, zeroArray._Elems, temp._Elems);
				return;
			}
		}
		strcpy_s(charArray, size, temp._Elems);
	}
}

string StringUtility::ullongArrayToString(ullong* valueList, uint llongCount, uint limitLen, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
	int arrayLen = 32 * MathUtility::getGreaterPower2(llongCount);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 32> temp;
	FOR_I(llongCount)
	{
		ullongToString(temp, valueList[i], limitLen);
		if (i != llongCount - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::ullongArrayToString(char* buffer, uint bufferSize, ullong* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 32> temp;
	FOR_I(count)
	{
		ullongToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

string StringUtility::uintArrayToString(uint* valueList, uint uintCount, uint limitLen, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个uint默认数字长度为16个字符
	int arrayLen = 16 * MathUtility::getGreaterPower2(uintCount);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 16> temp;
	FOR_I(uintCount)
	{
		intToString(temp, valueList[i], limitLen);
		if (i != uintCount - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::uintArrayToString(char* buffer, uint bufferSize, uint* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 16> temp;
	FOR_I(count)
	{
		intToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

string StringUtility::byteArrayToString(byte* valueList, uint intCount, uint limitLen, const char* seperate)
{
	int arrayLen = 4 * MathUtility::getGreaterPower2(intCount);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 4> temp;
	FOR_I(intCount)
	{
		intToString(temp, valueList[i], limitLen);
		if (i != intCount - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::byteArrayToString(char* buffer, uint bufferSize, byte* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 4> temp;
	FOR_I(count)
	{
		intToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

string StringUtility::ushortArrayToString(ushort* valueList, uint intCount, uint limitLen, const char* seperate)
{
	int arrayLen = 8 * MathUtility::getGreaterPower2(intCount);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 8> temp;
	FOR_I(intCount)
	{
		intToString(temp, valueList[i], limitLen);
		if (i != intCount - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::ushortArrayToString(char* buffer, uint bufferSize, ushort* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 8> temp;
	FOR_I(count)
	{
		intToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

string StringUtility::intArrayToString(int* valueList, uint intCount, uint limitLen, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
	int arrayLen = 16 * MathUtility::getGreaterPower2(intCount);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 16> temp;
	FOR_I(intCount)
	{
		intToString(temp, valueList[i], limitLen);
		if (i != intCount - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::intArrayToString(char* buffer, uint bufferSize, int* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 16> temp;
	FOR_I(count)
	{
		intToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

void StringUtility::stringToByteArray(const string& str, myVector<byte>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	FOR_VECTOR(strList)
	{
		valueList.push_back(stringToInt(strList[i]));
	}
}

uint StringUtility::stringToByteArray(const char* str, byte* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToInt(curString._Elems);
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

void StringUtility::stringToUShortArray(const string& str, myVector<ushort>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	FOR_VECTOR(strList)
	{
		valueList.push_back(stringToInt(strList[i]));
	}
}

uint StringUtility::stringToUShortArray(const char* str, ushort* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToInt(curString._Elems);
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

void StringUtility::stringToIntArray(const string& str, myVector<int>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	FOR_VECTOR(strList)
	{
		valueList.push_back(stringToInt(strList[i]));
	}
}

uint StringUtility::stringToIntArray(const char* str, int* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToInt(curString._Elems);
			if (curCount >= bufferSize)
			{
				ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

void StringUtility::stringToUIntArray(const string& str, myVector<uint>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	FOR_VECTOR(strList)
	{
		valueList.push_back((uint)stringToInt(strList[i]));
	}
}

uint StringUtility::stringToUIntArray(const char* str, uint* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToInt(curString._Elems);
			if (curCount >= bufferSize)
			{
				ERROR("uint buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

void StringUtility::stringToULLongArray(const string& str, myVector<ullong>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	FOR_VECTOR(strList)
	{
		valueList.push_back(stringToULLong(strList[i]));
	}
}

uint StringUtility::stringToULLongArray(const char* str, ullong* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToULLong(curString._Elems);
			if (curCount > bufferSize)
			{
				ERROR("ullong buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

string StringUtility::zeroString(uint count)
{
	array<char, 16> charArray;
	FOR_I(count)
	{
		charArray[i] = '0';
	}
	charArray[count] = '\0';
	return charArray._Elems;
}

void StringUtility::zeroString(char* charArray, uint size, uint count)
{
	if (size < count)
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

string StringUtility::floatToStringExtra(float f, uint precision, bool removeTailZero)
{
	const static string zeroDot = "0.";
	string retString;
	do
	{
		if (!IS_FLOAT_ZERO(f))
		{
			float powerValue = MathUtility::powerFloat(10.0f, precision);
			float totalValue = f * powerValue + MathUtility::sign(f) * 0.5f;
			if ((int)totalValue == 0)
			{
				if (precision > 0)
				{
					array<char, 16> temp;
					zeroString(temp, precision);
					retString += zeroDot + temp._Elems;
				}
				else
				{
					retString = "0";
				}
			}
			else
			{
				array<char, 16> temp;
				intToString(temp, abs((int)totalValue));
				retString = temp._Elems;
				int dotPosition = (int)strlen(retString.c_str()) - precision;
				if (dotPosition <= 0)
				{
					array<char, 16> tempZero;
					zeroString(tempZero, -dotPosition);
					retString = zeroDot + tempZero._Elems + retString;
				}
				else
				{
					retString.insert(dotPosition, ".");
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
				array<char, 16> tempZero;
				zeroString(tempZero, precision);
				retString = zeroDot + tempZero._Elems;
			}
			else
			{
				retString = "0";
			}
		}
	} while (false);
	// 移除末尾无用的0
	if (removeTailZero && retString[retString.length() - 1] == '0')
	{
		int dotPos = (int)retString.find_last_of('.');
		if (dotPos != -1)
		{
			string floatPart = retString.substr(dotPos + 1, retString.length() - dotPos - 1);
			// 找到最后一个不是0的位置,然后将后面的所有0都去掉
			int notZeroPos = (int)floatPart.find_last_not_of('0');
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

string StringUtility::floatToString(float f)
{
	array<char, 16> temp;
	floatToString(temp, f);
	return temp._Elems;
}

void StringUtility::floatToString(char* charArray, uint size, float f) 
{
	SPRINTF(charArray, size, "%f", f);
	// 先查找小数点和结束符的位置
	int dotPos = -1;
	uint strLen = 0;
	FOR_I(size)
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

string StringUtility::floatArrayToString(float* valueList, uint count, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个float默认数字长度为16个字符
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	charArray[0] = 0;
	array<char, 16> temp;
	FOR_I(count)
	{
		floatToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(charArray, arrayLen, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(charArray, arrayLen, temp._Elems);
		}
	}
	string str(charArray);
	ArrayPool::deleteArray(charArray);
	return str;
}

void StringUtility::floatArrayToString(char* buffer, uint bufferSize, float* valueList, uint count, const char* seperate)
{
	buffer[0] = '\0';
	array<char, 16> temp;
	FOR_I(count)
	{
		floatToString(temp, valueList[i]);
		if (i != count - 1)
		{
			STR_APPEND2_N(buffer, bufferSize, temp._Elems, seperate);
		}
		else
		{
			STR_APPEND1_N(buffer, bufferSize, temp._Elems);
		}
	}
}

void StringUtility::stringToFloatArray(const string& str, myVector<float>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	uint count = strList.size();
	FOR_I(count)
	{
		if (strList[i].length() > 0)
		{
			valueList.push_back(stringToFloat(strList[i]));
		}
	}
}

uint StringUtility::stringToFloatArray(const char* str, float* buffer, uint bufferSize, const char* seperate)
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
			buffer[curCount++] = stringToFloat(curString._Elems);
			if (curCount > bufferSize)
			{
				ERROR("float buffer size is too small, bufferSize:" + intToString(bufferSize));
				break;
			}
		}
		if (!ret)
		{
			break;
		}
		startPos = devidePos + keyLen;
	}
	return curCount;
}

string StringUtility::vector2ToString(const Vector2& vec, const char* seperate)
{
	return floatToString(vec.x) + seperate + floatToString(vec.y);
}

void StringUtility::vector2ToString(char* buffer, uint bufferSize, const Vector2& vec, const char* seperate)
{
	buffer[0] = '\0';
	FLOAT_TO_STRING(xStr, vec.x);
	FLOAT_TO_STRING(yStr, vec.y);
	STR_APPEND3_N(buffer, bufferSize, xStr, seperate, yStr);
}

Vector2 StringUtility::stringToVector2(const string& str, const char* seperate)
{
	Vector2 value;
	array<string, 2> valueList;
	if (split(str.c_str(), seperate, valueList) == valueList.size())
	{
		value.x = stringToFloat(valueList[0]);
		value.y = stringToFloat(valueList[1]);
	}
	return value;
}

Vector2Int StringUtility::stringToVector2Int(const string& str, const char* seperate)
{
	Vector2Int value;
	array<string, 2> valueList;
	if (split(str.c_str(), seperate, valueList) == valueList.size())
	{
		value.x = stringToInt(valueList[0]);
		value.y = stringToInt(valueList[1]);
	}
	return value;
}

Vector2UShort StringUtility::stringToVector2UShort(const string& str, const char* seperate)
{
	Vector2UShort value;
	array<string, 2> valueList;
	if (split(str.c_str(), seperate, valueList) == valueList.size())
	{
		value.x = stringToInt(valueList[0]);
		value.y = stringToInt(valueList[1]);
	}
	return value;
}

Vector2Short StringUtility::stringToVector2Short(const string& str, const char* seperate)
{
	Vector2Short value;
	array<string, 2> valueList;
	if (split(str.c_str(), seperate, valueList) == valueList.size())
	{
		value.x = stringToInt(valueList[0]);
		value.y = stringToInt(valueList[1]);
	}
	return value;
}

string StringUtility::vector3ToString(const Vector3& vec, const char* seperate)
{
	return floatToString(vec.x) + seperate + floatToString(vec.y) + seperate + floatToString(vec.z);
}

void StringUtility::vector3ToString(char* buffer, uint bufferSize, const Vector3& vec, const char* seperate)
{
	buffer[0] = '\0';
	FLOAT_TO_STRING(xStr, vec.x);
	FLOAT_TO_STRING(yStr, vec.y);
	FLOAT_TO_STRING(zStr, vec.z);
	STR_APPEND5_N(buffer, bufferSize, xStr, seperate, yStr, seperate, zStr);
}

string StringUtility::vector2IntToString(const Vector2Int& value, const char* seperate)
{
	return intToString(value.x) + seperate + intToString(value.y);
}

string StringUtility::vector2UShortToString(const Vector2UShort& value, const char* seperate)
{
	return intToString(value.x) + seperate + intToString(value.y);
}

void StringUtility::vector2IntToString(char* buffer, uint bufferSize, const Vector2Int& value, const char* seperate)
{
	buffer[0] = '\0';
	INT_TO_STRING(xStr, value.x);
	INT_TO_STRING(yStr, value.y);
	STR_APPEND3_N(buffer, bufferSize, xStr, seperate, yStr);
}

void StringUtility::vector2UShortToString(char* buffer, uint bufferSize, const Vector2UShort& value, const char* seperate)
{
	buffer[0] = '\0';
	INT_TO_STRING(xStr, value.x);
	INT_TO_STRING(yStr, value.y);
	STR_APPEND3_N(buffer, bufferSize, xStr, seperate, yStr);
}

Vector3 StringUtility::stringToVector3(const string& str, const char* seperate)
{
	Vector3 value;
	array<string, 3> valueList;
	if (split(str.c_str(), seperate, valueList) == valueList.size())
	{
		value.x = stringToFloat(valueList[0]);
		value.y = stringToFloat(valueList[1]);
		value.z = stringToFloat(valueList[2]);
	}
	return value;
}

string StringUtility::bytesToString(const char* buffer, uint length)
{
	int size = MathUtility::getGreaterPower2(length + 1);
	char* tempBuffer = ArrayPool::newArray<char>(size);
	tempBuffer[length] = '\0';
	memcpy(tempBuffer, buffer, length);
	string str(tempBuffer);
	ArrayPool::deleteArray(tempBuffer);
	return str;
}

bool StringUtility::endWith(const string& oriString, const string& pattern, bool sensitive)
{
	if (oriString.length() < pattern.length())
	{
		return false;
	}
	string endString = oriString.substr(oriString.length() - pattern.length(), pattern.length());
	if (sensitive)
	{
		return endString == pattern;
	}
	else
	{
		return toLower(endString) == toLower(pattern);
	}
}

bool StringUtility::startWith(const string& oriString, const string& pattern, bool sensitive)
{
	if (oriString.length() < pattern.length())
	{
		return false;
	}
	string startString = oriString.substr(0, pattern.length());
	if (sensitive)
	{
		return startString == pattern;
	}
	else
	{
		return toLower(startString) == toLower(pattern);
	}
}

string StringUtility::removePreNumber(const string& str)
{
	uint length = (uint)str.length();
	FOR_I(length)
	{
		if (str[i] < '0' || str[i] > '9')
		{
			return str.substr(i);
		}
	}
	return str;
}

string StringUtility::stringArrayToString(string* strList, uint stringCount, const char* seperate)
{
	string str;
	FOR_I(stringCount)
	{
		str += strList[i];
		if (i != stringCount - 1)
		{
			str += seperate;
		}
	}
	return str;
}

#if RUN_PLATFORM == PLATFORM_WINDOWS
wstring StringUtility::ANSIToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	const int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str, -1, nullptr, 0);
	wchar_t* pUnicode = new wchar_t[MathUtility::getGreaterPower2(unicodeLen + 1)];
	MultiByteToWideChar(CP_ACP, 0, str, -1, (LPWSTR)pUnicode, unicodeLen);
	wstring finalStr = pUnicode;
	delete[] pUnicode;
	return finalStr;
}

void StringUtility::ANSIToUnicode(const char* str, wchar_t* output, const int maxLength)
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

string StringUtility::UnicodeToANSI(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return FrameDefine::EMPTY_STRING;
	}
	const int iTextLen = WideCharToMultiByte(CP_ACP, 0, str, -1, nullptr, 0, nullptr, nullptr);
	char* pElementText = new char[MathUtility::getGreaterPower2(iTextLen + 1)];
	WideCharToMultiByte(CP_ACP, 0, str, -1, pElementText, iTextLen, nullptr, nullptr);
	string finalStr = pElementText;
	delete[] pElementText;
	return finalStr;
}

void StringUtility::UnicodeToANSI(const wchar_t* str, char* output, const int maxLength)
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

string StringUtility::UnicodeToUTF8(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return FrameDefine::EMPTY_STRING;
	}
	// wide char to multi char
	const int iTextLen = WideCharToMultiByte(CP_UTF8, 0, str, -1, nullptr, 0, nullptr, nullptr);
	char* pElementText = new char[MathUtility::getGreaterPower2(iTextLen + 1)];
	WideCharToMultiByte(CP_UTF8, 0, str, -1, pElementText, iTextLen, nullptr, nullptr);
	string finalStr = pElementText;
	delete[] pElementText;
	return finalStr;
}

void StringUtility::UnicodeToUTF8(const wchar_t* str, char* output, const int maxLength)
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

wstring StringUtility::UTF8ToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	const int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
	wchar_t* pUnicode = new wchar_t[MathUtility::getGreaterPower2(unicodeLen + 1)];
	MultiByteToWideChar(CP_UTF8, 0, str, -1, (LPWSTR)pUnicode, unicodeLen);
	wstring finalStr = pUnicode;
	delete[] pUnicode;
	return finalStr;
}

void StringUtility::UTF8ToUnicode(const char* str, wchar_t* output, const int maxLength)
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

#elif RUN_PLATFORM == PLATFORM_LINUX
wstring StringUtility::ANSIToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (exception)
	{
		ERROR("当前系统不支持GBK编码");
		return L"";
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	wchar_t* dBuf = ArrayPool::newArray<wchar_t>(allocSize);
	mbstowcs(dBuf, str, dSize);
	setlocale(LC_ALL, oldname);
	wstring strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}

string StringUtility::UnicodeToANSI(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return "";
	}
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (exception)
	{
		ERROR("当前系统不支持GBK编码");
		return EMPTY_STRING;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	char* dBuf = ArrayPool::newArray<char>(allocSize);
	wcstombs(dBuf, str, dSize);
	setlocale(LC_ALL, oldname);
	string strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}
string StringUtility::UnicodeToUTF8(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return "";
	}
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (exception)
	{
		ERROR("当前系统不支持UTF8编码");
		return EMPTY_STRING;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	char* dBuf = ArrayPool::newArray<char>(allocSize);
	wcstombs(dBuf, str, dSize);
	setlocale(LC_ALL, oldname);
	string strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}

wstring StringUtility::UTF8ToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (exception)
	{
		ERROR("当前系统不支持UTF8编码");
		return L"";
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	wchar_t* dBuf = ArrayPool::newArray<wchar_t>(allocSize);
	mbstowcs(dBuf, str, dSize);
	setlocale(LC_ALL, oldname);
	wstring strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}
#endif

string StringUtility::ANSIToUTF8(const char* str, bool addBOM)
{
	wstring unicodeStr = ANSIToUnicode(str);
	string utf8Str = UnicodeToUTF8(unicodeStr.c_str());
	if (addBOM)
	{
		utf8Str = BOM + utf8Str;
	}
	return utf8Str;
}

void StringUtility::ANSIToUTF8(const char* str, char* output, const int maxLength, const bool addBOM)
{
	const int unicodeLength = MathUtility::getGreaterPower2((maxLength + 1) << 1);
	wchar_t* unicodeStr = new wchar_t[unicodeLength];
	ANSIToUnicode(str, unicodeStr, unicodeLength);
	if (addBOM)
	{
		MEMCPY(output, maxLength, BOM, 3);
		UnicodeToUTF8(unicodeStr, output + 3, maxLength - 3);
	}
	else
	{
		UnicodeToUTF8(unicodeStr, output, maxLength);
	}
	delete[] unicodeStr;
}

string StringUtility::UTF8ToANSI(const char* str, bool eraseBOM)
{
	wstring unicodeStr;
	if (eraseBOM)
	{
		string newStr = str;
		removeBOM(newStr);
		unicodeStr = UTF8ToUnicode(newStr.c_str());
	}
	else
	{
		unicodeStr = UTF8ToUnicode(str);
	}
	return UnicodeToANSI(unicodeStr.c_str());
}

void StringUtility::UTF8ToANSI(const char* str, char* output, const int maxLength, const bool eraseBOM)
{
	const int unicodeLength = MathUtility::getGreaterPower2((maxLength + 1) << 1);
	wchar_t* unicodeStr = new wchar_t[unicodeLength];
	if (eraseBOM)
	{
		const char* newInput = str;
		const int length = strlength(str);
		if (length >= 3 && str[0] == BOM[0] && str[1] == BOM[1] && str[2] == BOM[2])
		{
			newInput += 3;
		}
		UTF8ToUnicode(newInput, unicodeStr, unicodeLength);
	}
	else
	{
		UTF8ToUnicode(str, unicodeStr, unicodeLength);
	}
	UnicodeToANSI(unicodeStr, output, maxLength);
	delete[] unicodeStr;
}

void StringUtility::removeBOM(string& str)
{
	if (str.length() >= 3 && str[0] == BOM[0] && str[1] == BOM[1] && str[2] == BOM[2])
	{
		str = str.erase(0, 3);
	}
}

void StringUtility::removeBOM(char* str, int length)
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

void StringUtility::jsonStartArray(string& str, uint preTableCount, bool returnLine)
{
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "[";
	if (returnLine)
	{
		str += "\r\n";
	}
}

void StringUtility::jsonEndArray(string& str, uint preTableCount, bool returnLine)
{
	removeLastComma(str);
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "],";
	if (returnLine)
	{
		str += "\r\n";
	}
}

void StringUtility::jsonStartStruct(string& str, uint preTableCount, bool returnLine)
{
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "{";
	if (returnLine)
	{
		str += "\r\n";
	}
}

void StringUtility::jsonEndStruct(string& str, uint preTableCount, bool returnLine)
{
	removeLastComma(str);
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "},";
	if (returnLine)
	{
		str += "\r\n";
	}
}

void StringUtility::jsonAddPair(string& str, const string& name, const string& value, uint preTableCount, bool returnLine)
{
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "\"" + name + "\": \"" + value + "\",";
	if (returnLine)
	{
		str += "\r\n";
	}
}

void StringUtility::jsonAddObject(string& str, const string& name, const string& value, uint preTableCount, bool returnLine)
{
	FOR_I(preTableCount)
	{
		str += "\t";
	}
	str += "\"" + name + "\": " + value + ",";
	if (returnLine)
	{
		str += "\r\n";
	}
}

char StringUtility::toLower(char str)
{
	if (str >= 'A' && str <= 'Z')
	{
		return str + 'a' - 'A';
	}
	return str;
}

char StringUtility::toUpper(char str)
{
	if (str >= 'a' && str <= 'z')
	{
		return str - ('a' - 'A');
	}
	return str;
}

string StringUtility::toLower(const string& str)
{
	string ret = str;
	uint size = (uint)ret.length();
	FOR_I(size)
	{
		ret[i] = toLower(ret[i]);
	}
	return ret;
}

string StringUtility::toUpper(const string& str)
{
	string ret = str;
	uint size = (uint)ret.length();
	FOR_I(size)
	{
		ret[i] = toUpper(ret[i]);
	}
	return ret;
}

void StringUtility::rightToLeft(string& str)
{
	uint pathLength = (uint)str.length();
	FOR_I(pathLength)
	{
		if (str[i] == '\\')
		{
			str[i] = '/';
		}
	}
}
void StringUtility::leftToRight(string& str)
{
	uint pathLength = (uint)str.length();
	FOR_I(pathLength)
	{
		if (str[i] == '/')
		{
			str[i] = '\\';
		}
	}
}
bool StringUtility::findSubstrLower(const string& res, const string& sub, int* pos, uint startIndex, bool direction)
{
	// 全转换为小写
	string lowerRes = toLower(res);
	string lowerSub = toLower(sub);
	return findSubstr(lowerRes, lowerSub, pos, startIndex, direction);
}

bool StringUtility::findSubstr(const string& res, const string& sub, int* pos, uint startIndex, bool direction)
{
	int posFind = -1;
	uint subLen = (uint)sub.length();
	int searchLength = (int)res.length() - subLen;
	if (searchLength < 0)
	{
		return false;
	}
	if (direction)
	{
		for (int i = startIndex; i <= searchLength; ++i)
		{
			uint j = 0;
			for (j = 0; j < subLen; ++j)
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
		for (uint i = searchLength; i >= startIndex; --i)
		{
			uint j = 0;
			for (j = 0; j < subLen; ++j)
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

bool StringUtility::findString(const char* str, const char* key, int* pos, uint startPos)
{
	CLAMP_MIN(startPos, 0);
	uint length = (uint)strlen(str);
	uint keyLen = (uint)strlen(key);
	for (uint i = startPos; i < length; ++i)
	{
		bool find = true;
		FOR_J(keyLen)
		{
			if (str[i + j] != key[j])
			{
				find = false;
				break;
			}
		}
		if (find)
		{
			if (pos != nullptr)
			{
				*pos = i;
			}
			return true;
		}
	}
	if (pos != nullptr)
	{
		*pos = -1;
	}
	return false;
}

string StringUtility::checkString(const string& str, const string& valid)
{
	string newString = "";
	uint validCount = (uint)valid.length();
	uint oldStrLen = (uint)str.length();
	FOR_I(oldStrLen)
	{
		bool keep = false;
		FOR_J(validCount)
		{
			if (str[i] == valid[j])
			{
				keep = true;
				break;
			}
		}
		if (keep)
		{
			newString += str[i];
		}
	}
	return newString;
}

string StringUtility::checkFloatString(const string& str, const string& valid)
{
	return checkIntString(str, "." + valid);
}

string StringUtility::checkIntString(const string& str, const string& valid)
{
	return checkString(str, "0123456789" + valid);
}

string StringUtility::charToHexString(byte value, bool upper)
{
	char byteHex[3]{ 0 };
	const char* charPool = upper ? "ABCDEF" : "abcdef";
	byte highBit = value >> 4;
	// 高字节的十六进制
	byteHex[0] = (highBit < (byte)10) ? ('0' + highBit) : charPool[highBit - 10];
	// 低字节的十六进制
	byte lowBit = value & 0x0F;
	byteHex[1] = (lowBit < (byte)10) ? ('0' + lowBit) : charPool[lowBit - 10];
	return byteHex;
}

string StringUtility::charArrayToHexString(byte* data, uint dataCount, bool addSpace, bool upper)
{
	uint oneLength = addSpace ? 3 : 2;
	uint showCount = MathUtility::getGreaterPower2(dataCount * oneLength + 1);
	char* byteData = ArrayPool::newArray<char>(showCount);
	FOR_J(dataCount)
	{
		string byteStr = charToHexString(data[j]);
		byteData[j * oneLength + 0] = byteStr[0];
		byteData[j * oneLength + 1] = byteStr[1];
		if (oneLength >= 3)
		{
			byteData[j * oneLength + 2] = ' ';
		}
	}
	byteData[dataCount * oneLength] = '\0';
	string str(byteData);
	ArrayPool::deleteArray(byteData);
	return str;
}

uint StringUtility::getCharCount(const string& str, char key)
{
	uint count = 0;
	uint length = (uint)str.length();
	FOR_I(length)
	{
		if (str[i] == key)
		{
			++count;
		}
	}
	return count;
}

uint StringUtility::getStringWidth(const string& str)
{
	return (uint)str.length() + getCharCount(str, '\t') * 3;
}

uint StringUtility::generateAlignTableCount(const string& str, int alignWidth)
{
	int remainChar = alignWidth - getStringWidth(str);
	MathUtility::clampMin(remainChar, 0);
	return (uint)ceil(remainChar / 4.0f);
}

void StringUtility::appendWithAlign(string& oriStr, const string& appendStr, int alignWidth)
{
	uint tabCount = generateAlignTableCount(oriStr, alignWidth);
	FOR_I(tabCount)
	{
		oriStr += '\t';
	}
	oriStr += appendStr;
}

void StringUtility::appendValueString(char* queryStr, uint size, const char* str, bool toUTF8, bool addComma)
{
	if (toUTF8)
	{
		if (addComma)
		{
			STR_APPEND3_N(queryStr, size, "\"", ANSIToUTF8(str).c_str(), "\",");
		}
		else
		{
			STR_APPEND3_N(queryStr, size, "\"", ANSIToUTF8(str).c_str(), "\"");
		}
	}
	else
	{
		if (addComma)
		{
			STR_APPEND3_N(queryStr, size, "\"", str, "\",");
		}
		else
		{
			STR_APPEND3_N(queryStr, size, "\"", str, "\"");
		}
	}
}

void StringUtility::appendValueInt(char* queryStr, uint size, int value, bool addComma)
{
	array<char, 16> temp;
	intToString(temp, value);
	if (addComma)
	{
		STR_APPEND2_N(queryStr, size, temp._Elems, ",");
	}
	else
	{
		STR_APPEND1_N(queryStr, size, temp._Elems);
	}
}

void StringUtility::appendValueFloat(char* queryStr, uint size, float value, bool addComma)
{
	array<char, 16> temp;
	floatToString(temp, value);
	if (addComma)
	{
		STR_APPEND2_N(queryStr, size, temp._Elems, ",");
	}
	else
	{
		STR_APPEND1_N(queryStr, size, temp._Elems);
	}
}

void StringUtility::appendValueLLong(char* queryStr, uint size, const ullong& value, bool addComma)
{
	array<char, 32> temp;
	ullongToString(temp, value);
	if (addComma)
	{
		STR_APPEND2_N(queryStr, size, temp._Elems, ",");
	}
	else
	{
		STR_APPEND1_N(queryStr, size, temp._Elems);
	}
}

void StringUtility::appendValueByteArray(char* queryStr, uint size, byte* ushortArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	byteArrayToString(charArray, arrayLen, ushortArray, count);
	appendValueString(queryStr, size, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendValueUShortArray(char* queryStr, uint size, ushort* ushortArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	ushortArrayToString(charArray, arrayLen, ushortArray, count);
	appendValueString(queryStr, size, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendValueIntArray(char* queryStr, uint size, int* intArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	intArrayToString(charArray, arrayLen, intArray, count);
	appendValueString(queryStr, size, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendValueFloatArray(char* queryStr, uint size, float* floatArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	floatArrayToString(charArray, arrayLen, floatArray, count);
	appendValueString(queryStr, size, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendValueULLongArray(char* queryStr, uint size, ullong* longArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	ullongArrayToString(charArray, arrayLen, longArray, count);
	appendValueString(queryStr, size, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendValueVector2Int(char* queryStr, uint size, const Vector2Int& value, bool addComma)
{
	array<char, 32> temp;
	vector2IntToString(temp, value);
	appendValueString(queryStr, size, temp._Elems, false, addComma);
}

void StringUtility::appendValueVector2UShort(char* queryStr, uint size, const Vector2UShort& value, bool addComma)
{
	array<char, 16> temp;
	vector2UShortToString(temp, value);
	appendValueString(queryStr, size, temp._Elems, false, addComma);
}

void StringUtility::appendConditionString(char* condition, uint size, const char* col, const char* str, bool toUTF8, const char* operate)
{
	if (toUTF8)
	{
		if (operate == nullptr)
		{
			STR_APPEND5_N(condition, size, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"");
		}
		else
		{
			STR_APPEND6_N(condition, size, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"", operate);
		}
	}
	else
	{
		if (operate == nullptr)
		{
			STR_APPEND5_N(condition, size, col, " = ", "\"", str, "\"");
		}
		else
		{
			STR_APPEND6_N(condition, size, col, " = ", "\"", str, "\"", operate);
		}
	}
}

void StringUtility::appendConditionInt(char* condition, uint size, const char* col, int value, const char* operate)
{
	array<char, 16> temp;
	intToString(temp, value);
	if (operate == nullptr)
	{
		STR_APPEND3_N(condition, size, col, " = ", temp._Elems);
	}
	else
	{
		STR_APPEND4_N(condition, size, col, " = ", temp._Elems, operate);
	}
}

void StringUtility::appendConditionFloat(char* condition, uint size, const char* col, float value, const char* operate)
{
	array<char, 16> temp;
	floatToString(temp, value);
	if (operate == nullptr)
	{
		STR_APPEND3_N(condition, size, col, " = ", temp._Elems);
	}
	else
	{
		STR_APPEND4_N(condition, size, col, " = ", temp._Elems, operate);
	}
}

void StringUtility::appendConditionULLong(char* condition, uint size, const char* col, const ullong& value, const char* operate)
{
	array<char, 32> temp;
	ullongToString(temp, value);
	if (operate == nullptr)
	{
		STR_APPEND3_N(condition, size, col, " = ", temp._Elems);
	}
	else
	{
		STR_APPEND4_N(condition, size, col, " = ", temp._Elems, operate);
	}
}

void StringUtility::appendUpdateString(char* updateInfo, uint size, const char* col, const char* str, bool toUTF8, bool addComma)
{
	if (toUTF8)
	{
		if (addComma)
		{
			STR_APPEND5_N(updateInfo, size, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\",");
		}
		else
		{
			STR_APPEND5_N(updateInfo, size, col, " = ", "\"", ANSIToUTF8(str).c_str(), "\"");
		}
	}
	else
	{
		if (addComma)
		{
			STR_APPEND5_N(updateInfo, size, col, " = ", "\"", str, "\",");
		}
		else
		{
			STR_APPEND5_N(updateInfo, size, col, " = ", "\"", str, "\"");
		}
	}
}

void StringUtility::appendUpdateInt(char* updateInfo, uint size, const char* col, int value, bool addComma)
{
	array<char, 16> temp;
	intToString(temp, value);
	if (addComma)
	{
		STR_APPEND4_N(updateInfo, size, col, " = ", temp._Elems, ",");
	}
	else
	{
		STR_APPEND3_N(updateInfo, size, col, " = ", temp._Elems);
	}
}

void StringUtility::appendUpdateFloat(char* updateInfo, uint size, const char* col, float value, bool addComma)
{
	array<char, 16> temp;
	floatToString(temp, value);
	if (addComma)
	{
		STR_APPEND4_N(updateInfo, size, col, " = ", temp._Elems, ",");
	}
	else
	{
		STR_APPEND3_N(updateInfo, size, col, " = ", temp._Elems);
	}
}

void StringUtility::appendUpdateULLong(char* updateInfo, uint size, const char* col, const ullong& value, bool addComma)
{
	array<char, 32> temp;
	ullongToString(temp, value);
	if (addComma)
	{
		STR_APPEND4_N(updateInfo, size, col, " = ", temp._Elems, ",");
	}
	else
	{
		STR_APPEND3_N(updateInfo, size, col, " = ", temp._Elems);
	}
}

void StringUtility::appendUpdateByteArray(char* updateInfo, uint size, const char* col, byte* ushortArray, uint count, bool addComma)
{
	int arrayLen = 4 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	byteArrayToString(charArray, arrayLen, ushortArray, count);
	appendUpdateString(updateInfo, size, col, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendUpdateUShortArray(char* updateInfo, uint size, const char* col, ushort* ushortArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	ushortArrayToString(charArray, arrayLen, ushortArray, count);
	appendUpdateString(updateInfo, size, col, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendUpdateIntArray(char* updateInfo, uint size, const char* col, int* intArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	intArrayToString(charArray, arrayLen, intArray, count);
	appendUpdateString(updateInfo, size, col, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendUpdateFloatArray(char* updateInfo, uint size, const char* col, float* floatArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	floatArrayToString(charArray, arrayLen, floatArray, count);
	appendUpdateString(updateInfo, size, col, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendUpdateULLongArray(char* updateInfo, uint size, const char* col, ullong* longArray, uint count, bool addComma)
{
	int arrayLen = 16 * MathUtility::getGreaterPower2(count);
	char* charArray = ArrayPool::newArray<char>(arrayLen);
	ullongArrayToString(charArray, arrayLen, longArray, count);
	appendUpdateString(updateInfo, size, col, charArray, false, addComma);
	ArrayPool::deleteArray(charArray);
}

void StringUtility::appendUpdateVector2Int(char* updateInfo, uint size, const char* col, const Vector2Int& value, bool addComma)
{
	array<char, 32> temp;
	vector2IntToString(temp, value);
	appendUpdateString(updateInfo, size, col, temp._Elems, false, addComma);
}

void StringUtility::appendUpdateVector2UShort(char* updateInfo, uint size, const char* col, const Vector2UShort& value, bool addComma)
{
	array<char, 16> temp;
	vector2UShortToString(temp, value);
	appendUpdateString(updateInfo, size, col, temp._Elems, false, addComma);
}

void StringUtility::stringToFloats(const string& str, myVector<float>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToFloat(curStr));
		}
	}
}

void StringUtility::stringToBytes(const string& str, myVector<byte>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

int StringUtility::stringToBytes(const char* str, byte* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
	const int sourceLen = strlength(str);
	const int keyLen = strlength(seperate);
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.str());
	}
	return curCount;
}

void StringUtility::stringToUShorts(const string& str, myVector<ushort>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

int StringUtility::stringToUShorts(const char* str, ushort* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.str());
	}
	return curCount;
}

void StringUtility::stringToInts(const string& str, myVector<int>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(strList.size());
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

int StringUtility::stringToInts(const char* str, int* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
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
		// 转换为整数放入列表
		if (curString[0] == '\0')
		{
			continue;
		}
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.str());
	}
	return curCount;
}

void StringUtility::stringToUInts(const string& str, myVector<uint>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str.c_str(), seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back((uint)stringToInt(curStr));
		}
	}
}

int StringUtility::stringToUInts(const char* str, uint* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
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
		if (curCount >= bufferSize)
		{
			ERROR("uint buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.str());
	}
	return curCount;
}

void StringUtility::strcat_s(char* destBuffer, const int size, const char* source)
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

void StringUtility::strcat_s(char* destBuffer, const int size, const char* source, const int length)
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

void StringUtility::strcpy_s(char* destBuffer, const int size, const char* source)
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

string StringUtility::zeroString(const int zeroCount)
{
	Array<16> charArray{ 0 };
	FOR_I(zeroCount)
	{
		charArray[i] = '0';
	}
	charArray[zeroCount] = '\0';
	return charArray.str();
}

void StringUtility::stringToULLongs(const char* str, myVector<ullong>& valueList, const char* seperate)
{
	myVector<string> strList;
	split(str, seperate, strList);
	const int count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		const string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToULLong(curStr));
		}
	}
}

int StringUtility::stringToULLongs(const char* str, ullong* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
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
		if (curCount >= bufferSize)
		{
			ERROR("ullong buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToULLong(curString.str());
	}
	return curCount;
}

void StringUtility::stringToLLongs(const char* str, myVector<llong>& valueList, const char* seperate)
{
	int startPos = 0;
	const int sourceLen = strlength(str);
	const int keyLen = strlength(seperate);
	Array<32> curString{ 0 };
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
		valueList.push_back(stringToLLong(curString.str()));
	}
}

void StringUtility::stringToLLongs(const string& str, myVector<llong>& valueList, const char* seperate)
{
	int startPos = 0;
	const int keyLen = strlength(seperate);
	const int sourceLen = (int)str.length();
	Array<32> curString{ 0 };
	int devidePos = -1;
	valueList.clear();
	bool ret = true;
	while (ret)
	{
		ret = findString(str.c_str(), seperate, &devidePos, startPos);
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
		valueList.push_back(stringToLLong(curString.str()));
	}
}

int StringUtility::stringToLLongs(const char* str, llong* buffer, const int bufferSize, const char* seperate)
{
	int curCount = 0;
	int startPos = 0;
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
		if (curCount >= bufferSize)
		{
			ERROR("llong buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToLLong(curString.str());
	}
	return curCount;
}