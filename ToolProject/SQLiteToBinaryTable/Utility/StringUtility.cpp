#include "FrameHeader.h"

#define VECTOR(type, name) Vector<type> name
#define UN_VECTOR(type, name)

const char StringUtility::BOM[4]{ (char)0xEF, (char)0xBB, (char)0xBF, 0 };
Array<20000, string> StringUtility::mIntString;
const Array<11, llong> StringUtility::POWER_INT_10{ 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L, 10000000L, 100000000L, 1000000000L, 10000000000L };
const Array<19, llong> StringUtility::POWER_LLONG_10
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

byte StringUtility::alphabet_map[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
byte StringUtility::reverse_map[] =
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

string StringUtility::removeSuffix(const string& str)
{
	size_t dotPos = str.find_last_of('.');
	if (dotPos != NOT_FIND)
	{
		return str.substr(0, dotPos);
	}
	return str;
}

void StringUtility::removeStartAll(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[i] != key)
		{
			stream.erase(0, i);
			break;
		}
	}
}

void StringUtility::removeStart(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_I(streamSize)
	{
		if (stream[i] == key)
		{
			stream.erase(i, 1);
			break;
		}
	}
}

void StringUtility::removeLastAll(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_INVERSE_I(streamSize)
	{
		if (stream[i] != key)
		{
			stream.erase(i + 1);
			break;
		}
	}
}

void StringUtility::removeLast(string& stream, char key)
{
	uint streamSize = (uint)stream.length();
	FOR_INVERSE_I(streamSize)
	{
		if (stream[i] == key)
		{
			stream.erase(i);
			break;
		}
	}
}

void StringUtility::removeLastComma(string& stream)
{
	removeLast(stream, ',');
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

int StringUtility::strchar(const char* str, char key, uint startIndex, uint length)
{
	if (length == 0)
	{
		length = strlen(str);
	}
	for (uint i = startIndex; i < length; ++i)
	{
		if (str[i] == key)
		{
			return (int)i;
		}
	}
	return -1;
}

string StringUtility::getFileName(string str)
{
	rightToLeft(str);
	size_t dotPos = str.find_last_of('/');
	if (dotPos != NOT_FIND)
	{
		return str.substr(dotPos + 1, str.length() - 1);
	}
	return str;
}

string StringUtility::getFileNameNoSuffix(string str, bool removePath)
{
	rightToLeft(str);
	size_t dotPos = str.find_last_of('.');
	if (removePath)
	{
		size_t namePos = str.find_last_of('/');
		if (namePos != NOT_FIND)
		{
			if (dotPos != NOT_FIND)
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
		if (dotPos != NOT_FIND)
		{
			return str.substr(0, dotPos);
		}
	}
	return str;
}

string StringUtility::getFirstFolderName(const string& dir)
{
	string temp = dir;
	rightToLeft(temp);
	size_t index = (int)temp.find_first_of('/');
	if (index == NOT_FIND)
	{
		return dir;
	}
	return temp.substr(0, index);
}

string StringUtility::removeFirstPath(const string& dir)
{
	string temp = dir;
	rightToLeft(temp);
	size_t index = temp.find_first_of('/');
	if (index == NOT_FIND)
	{
		return dir;
	}
	return temp.substr(index + 1, temp.length() - index - 1);
}

string StringUtility::getFilePath(const string& dir)
{
	string tempDir = dir;
	rightToLeft(tempDir);
	size_t pos = tempDir.find_last_of('/');
	return pos != NOT_FIND ? dir.substr(0, pos) : "./";
}

string StringUtility::getFileSuffix(const string& fileName)
{
	size_t dotPos = fileName.find_last_of('.');
	if (dotPos == NOT_FIND)
	{
		return fileName;
	}
	return fileName.substr(dotPos + 1, fileName.length() - dotPos - 1);
}

string StringUtility::removeStartString(const string& fileName, const string& startStr)
{
	if (startWith(fileName, startStr.c_str()))
	{
		return fileName.substr(startStr.length(), fileName.length() - startStr.length());
	}
	return fileName;
}

string StringUtility::removeEndString(const string& fileName, const string& endStr)
{
	if (endWith(fileName, endStr.c_str()))
	{
		return fileName.substr(0, fileName.length() - endStr.length());
	}
	return fileName;
}

int StringUtility::getLastNotNumberPos(const string& str)
{
	uint strLen = (uint)str.length();
	FOR_INVERSE_I(strLen)
	{
		if (str[i] > '9' || str[i] < '0')
		{
			return i;
		}
	}
	return -1;
}

int StringUtility::getLastNumber(const string& str)
{
	size_t lastPos = getLastNotNumberPos(str);
	if (lastPos == NOT_FIND)
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

int StringUtility::getLastChar(const char* str, char value)
{
	uint length = strlen(str);
	FOR_INVERSE_I(length)
	{
		if (str[i] == value)
		{
			return i;
		}
	}
	return -1;
}

int StringUtility::getLastNotChar(const char* str, char value)
{
	uint length = strlen(str);
	FOR_INVERSE_I(length)
	{
		if (str[i] != value)
		{
			return i;
		}
	}
	return -1;
}

void StringUtility::splitLine(const char* str, char key, Vector<string>& vec, bool removeEmpty)
{
	split(str, key, vec, removeEmpty);
	FOR_VECTOR(vec)
	{
		removeLast(vec[i], '\r');
	}
	END(vec);
}

void StringUtility::splitLine(const char* str, char key, string* stringBuffer, uint bufferSize, bool removeEmpty)
{
	uint strCount = split(str, key, stringBuffer, bufferSize, removeEmpty);
	FOR_I(strCount)
	{
		removeLast(stringBuffer[i], '\r');
	}
}

void StringUtility::split(const char* str, char key, Vector<string>& vec, bool removeEmpty)
{
	int startPos = 0;
	uint sourceLen = strlen(str);
	constexpr int STRING_BUFFER = 1024;
	Array<STRING_BUFFER> curString{0};
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
			ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
			return;
		}
		curString.copy(str + startPos, devidePos - startPos);
		curString[devidePos - startPos] = '\0';
		startPos = devidePos + 1;
		// 放入列表
		if (curString[0] != '\0' || !removeEmpty)
		{
			vec.push_back(curString.toString());
		}
	}
}

uint StringUtility::split(const char* str, char key, string* stringBuffer, uint bufferSize, bool removeEmpty)
{
	int startPos = 0;
	int sourceLen = strlen(str);
	constexpr int STRING_BUFFER = 1024;
	Array<STRING_BUFFER> curString{ 0 };
	uint curCount = 0;
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
			ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
			return 0;
		}
		curString.copy(str + startPos, devidePos - startPos);
		curString[devidePos - startPos] = '\0';
		startPos = devidePos + 1;
		// 放入列表
		if (curString[0] == '\0' || !removeEmpty)
		{
			continue;
		}
		if (curCount >= bufferSize)
		{
			ERROR("string buffer is too small! bufferSize:" + intToString(bufferSize));
			break;
		}
		stringBuffer[curCount++] = curString.toString();
	}
	return curCount;
}

void StringUtility::split(const char* str, const char* key, Vector<string>& vec, bool removeEmpty)
{
	int startPos = 0;
	uint keyLen = strlen(key);
	uint sourceLen = strlen(str);
	constexpr int STRING_BUFFER = 1024;
	Array<STRING_BUFFER> curString{ 0 };
	int devidePos = -1;
	bool ret = true;
	while (ret)
	{
		ret = findString(str, key, &devidePos, startPos);
		// 无论是否查找到,都将前面一段字符串截取出来
		devidePos = ret ? devidePos : sourceLen;
		if (devidePos - startPos >= STRING_BUFFER)
		{
			ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
			return;
		}
		curString.copy(str + startPos, devidePos - startPos);
		curString[devidePos - startPos] = '\0';
		startPos = devidePos + keyLen;
		// 放入列表
		if (curString[0] != '\0' || !removeEmpty)
		{
			vec.push_back(curString.toString());
		}
	}
}

void StringUtility::split(const string& str, const char* key, Vector<string>& vec, bool removeEmpty)
{
	int startPos = 0;
	uint keyLen = strlen(key);
	uint sourceLen = (uint)str.length();
	constexpr int STRING_BUFFER = 1024;
	Array<STRING_BUFFER> curString{ 0 };
	int devidePos = -1;
	bool ret = true;
	while (ret)
	{
		ret = findString(str, key, &devidePos, startPos);
		// 无论是否查找到,都将前面一段字符串截取出来
		devidePos = ret ? devidePos : sourceLen;
		if (devidePos - startPos >= STRING_BUFFER)
		{
			ERROR("分隔字符串失败,缓冲区太小,当前缓冲区为" + intToString(STRING_BUFFER) + "字节");
			return;
		}
		curString.copy(str.c_str() + startPos, devidePos - startPos);
		curString[devidePos - startPos] = '\0';
		startPos = devidePos + keyLen;
		// 放入列表
		if (curString[0] != '\0' || !removeEmpty)
		{
			vec.push_back(curString.toString());
		}
	}
}

uint StringUtility::split(const char* str, const char* key, string* stringBuffer, uint bufferSize, bool removeEmpty)
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
		if (curCount >= bufferSize)
		{
			ERROR("string buffer is too small! bufferSize:" + intToString(bufferSize));
			break;
		}
		stringBuffer[curCount++] = curString.toString();
	}
	return curCount;
}

void StringUtility::replace(char* str, int strBufferSize, int begin, int end, const char* reStr)
{
	int curLength = (int)strlen(str);
	int replaceLength = (int)strlen(reStr);
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

void StringUtility::replace(string& str, int begin, int end, const string& reStr)
{
	int replaceLength = (int)reStr.length();
	if (end - begin == replaceLength)
	{
		MEMCPY((void*)(str.c_str() + begin), str.length() - begin, reStr.c_str(), replaceLength);
	}
	else
	{
		str = str.substr(0, begin) + reStr + str.substr(end, str.length() - end);
	}
}

void StringUtility::replaceAll(char* str, int strBufferSize, const char* key, const char* newWord)
{
	uint keyLength = strlen(key);
	uint newWordsLength = strlen(newWord);
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

void StringUtility::replaceAll(string& str, const string& key, const string& newWord)
{
	uint keyLength = (uint)key.length();
	uint newWordsLength = (uint)newWord.length();
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

void StringUtility::replaceAll(string& str, char key, char newWord)
{
	uint length = (uint)str.length();
	FOR_I(length)
	{
		if (str[i] == key)
		{
			str[i] = newWord;
		}
	}
}

void StringUtility::removeAll(string& str, char value)
{
	int strLength = (uint)str.length();
	char* tempBuffer = newCharArray(MathUtility::getGreaterPower2(strLength + 1));
	setString(tempBuffer, strLength + 1, str);
	FOR_INVERSE_I(strLength)
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
	deleteCharArray(tempBuffer);
}

void StringUtility::removeAll(string& str, char value0, char value1)
{
	int strLength = (uint)str.length();
	char* tempBuffer = newCharArray(MathUtility::getGreaterPower2(strLength + 1));
	setString(tempBuffer, strLength + 1, str);
	FOR_INVERSE_I(strLength)
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
	deleteCharArray(tempBuffer);
}

bool StringUtility::removeString(char* str, uint length, const char* subString)
{
	int subPos = 0;
	if (!findString(str, subString, &subPos, 0))
	{
		return false;
	}
	// 从子字符串的位置,后面的数据覆盖前面的数据
	uint subLength = strlen(subString);
	memmove(str + subPos, str + subPos + subLength, length - subLength - subPos);
	return true;
}

uint StringUtility::strlen(const char* str, uint maxLength)
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

uint StringUtility::strlen(const char* str)
{
	uint index = 0;
	while(true)
	{
		if (str[index] == '\0')
		{
			return index;
		}
		++index;
		// 当字符串长度超过1MB时,认为是错误的字符串
		if (index >= 1024 * 1024)
		{
			ERROR("字符串长度太长");
			break;
		}
	}
	return 0;
}

void StringUtility::strcat_s(char* destBuffer, uint size, const char* source)
{
	if (source == nullptr || destBuffer == nullptr)
	{
		return;
	}
	uint destIndex = strlen(destBuffer, size);
	if (destIndex >= (uint)size)
	{
		ERROR("strcat_s buffer is too small");
		return;
	}
	uint length = strlen(source);
	MEMCPY(destBuffer + destIndex, size - destIndex, source, length);
	destBuffer[destIndex + length] = '\0';
}

void StringUtility::strcat_s(char* destBuffer, uint size, const char* source, uint length)
{
	if (source == nullptr || destBuffer == nullptr)
	{
		return;
	}
	uint destIndex = strlen(destBuffer, size);
	if (destIndex >= (uint)size)
	{
		ERROR("strcat_s buffer is too small");
		return;
	}
	MEMCPY(destBuffer + destIndex, size - destIndex, source, length);
	destBuffer[destIndex + length] = '\0';
}

void StringUtility::strcpy_s(char* destBuffer, uint size, const char* source)
{
	if (source == nullptr)
	{
		return;
	}
	uint length = strlen(source);
	if (length >= size)
	{
		ERROR("strcat_s buffer is too small");
		return;
	}
	MEMCPY(destBuffer, size, source, length);
	destBuffer[length] = '\0';
}

string StringUtility::intToString(int value, uint limitLen)
{
	Array<16> temp{ 0 };
	uint len = _itoa_s(value, temp);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp.toString();
	}
	return temp.toString();
}

string StringUtility::uintToString(uint value, uint limitLen)
{
	Array<16> temp{ 0 };
	uint len = _uitoa_s(value, temp);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp.toString();
	}
	return temp.toString();
}

string StringUtility::ullongToString(ullong value, uint limitLen)
{
	Array<32> temp{ 0 };
	uint len = _ui64toa_s(value, temp);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp.toString();
	}
	return temp.toString();
}

string StringUtility::llongToString(llong value, uint limitLen)
{
	Array<32> temp{ 0 };
	uint len = _i64toa_s(value, temp);
	if (limitLen > len)
	{
		return zeroString(limitLen - len) + temp.toString();
	}
	return temp.toString();
}

string StringUtility::ullongsToString(const Vector<ullong>& valueList, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
	int arrayLen = 32 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = 0;
	uint seperateLen = strlen(seperate);
	Array<32> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = ullongToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::llongsToString(const Vector<llong>& valueList, const char* seperate)
{
	// 根据列表长度选择适当的数组长度,每个llong默认数字长度为32个字符
	int arrayLen = 32 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = 0;
	uint seperateLen = strlen(seperate);
	Array<32> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = llongToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::bytesToString(const Vector<byte>& valueList, const char* seperate)
{
	if (valueList.size() == 0)
	{
		return "";
	}
	int arrayLen = 4 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = '\0';
	uint seperateLen = strlen(seperate);
	Array<4> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = intToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::ushortsToString(const Vector<ushort>& valueList, const char* seperate)
{
	if (valueList.size() == 0)
	{
		return "";
	}
	int arrayLen = 8 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = '\0';
	uint seperateLen = strlen(seperate);
	Array<8> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = intToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::intsToString(const Vector<int>& valueList, const char* seperate)
{
	if (valueList.size() == 0)
	{
		return "";
	}
	// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
	int arrayLen = 16 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = '\0';
	uint seperateLen = strlen(seperate);
	Array<16> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = intToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::uintsToString(const Vector<uint>& valueList, const char* seperate)
{
	if (valueList.size() == 0)
	{
		return "";
	}
	// 根据列表长度选择适当的数组长度,每个int默认数字长度为16个字符
	int arrayLen = 16 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = 0;
	uint seperateLen = strlen(seperate);
	Array<16> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = uintToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

string StringUtility::floatsToString(const Vector<float>& valueList, const char* seperate)
{
	if (valueList.size() == 0)
	{
		return EMPTY;
	}

	int arrayLen = 16 * MathUtility::getGreaterPower2(valueList.size());
	char* charArray = newCharArray(arrayLen);
	charArray[0] = 0;
	uint seperateLen = strlen(seperate);
	Array<16> temp{ 0 };
	FOR_CONST(valueList)
	{
		uint len = floatToString(temp, valueList[i]);
		strcat_s(charArray, arrayLen, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(charArray, arrayLen, seperate, seperateLen);
		}
	}
	END_CONST();
	string str(charArray);
	deleteCharArray(charArray);
	return str;
}

void StringUtility::floatsToString(char* buffer, uint bufferSize, const Vector<float>& valueList, const char* seperate)
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
		strcat_s(buffer, bufferSize, temp.toString(), len);
		if (i != valueListCount - 1)
		{
			strcat_s(buffer, bufferSize, seperate, seperateLen);
		}
	}
	END_CONST();
}

void StringUtility::stringToBools(const string& str, Vector<bool>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr) != 0);
		}
	}
}

uint StringUtility::stringToBools(const char* str, bool* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString()) != 0;
	}
	return curCount;
}

void StringUtility::stringToBytes(const string& str, Vector<byte>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

uint StringUtility::stringToBytes(const char* str, byte* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString());
	}
	return curCount;
}

void StringUtility::stringToShorts(const string& str, Vector<short>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

uint StringUtility::stringToShorts(const char* str, short* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString());
	}
	return curCount;
}

void StringUtility::stringToUShorts(const string& str, Vector<ushort>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToInt(curStr));
		}
	}
}

uint StringUtility::stringToUShorts(const char* str, ushort* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString());
	}
	return curCount;
}

bool StringUtility::stringToVector2Ints(const string& str, Vector<Vector2Int>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	const uint count = strList.size();
	valueList.reserve(strList.size());
	FOR_I(count)
	{
		bool result = false;
		Vector2Int v2Value = stringToVector2Int(strList[i], ",", &result);
		if (!result)
		{
			return false;
		}
		valueList.push_back(v2Value);
	}
	return true;
}

bool StringUtility::stringToVector3Ints(const string& str, Vector<Vector3Int>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	const uint count = strList.size();
	valueList.reserve(strList.size());
	FOR_I(count)
	{
		bool result = false;
		Vector3Int v3Value = stringToVector3Int(strList[i], ",", &result);
		if (!result)
		{
			return false;
		}
		valueList.push_back(v3Value);
	}
	return true;
}

bool StringUtility::stringToVector2s(const string& str, Vector<Vector2>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	const uint count = strList.size();
	valueList.reserve(strList.size());
	FOR_I(count)
	{
		bool result = false;
		Vector2 v2Value = stringToVector2(strList[i], ",", &result);
		if (!result)
		{
			return false;
		}
		valueList.push_back(v2Value);
	}
	return true;
}

bool StringUtility::stringToVector3s(const string& str, Vector<Vector3>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	const uint count = strList.size();
	valueList.reserve(strList.size());
	FOR_I(count)
	{
		bool result = false;
		Vector3 v2Value = stringToVector3(strList[i], ",", &result);
		if (!result)
		{
			return false;
		}
		valueList.push_back(v2Value);
	}
	return true;
}

Vector2Int StringUtility::stringToVector2Int(const string& str, const char* seperate, bool* result)
{
	constexpr int INT_COUNT = sizeof(Vector2Int) / sizeof(int);
	Vector<string> valueList;
	split(str, seperate, valueList);
	if (result != nullptr)
	{
		*result = valueList.size() == INT_COUNT;
	}
	if (valueList.size() != INT_COUNT)
	{
		return {0, 0};
	}
	return { stringToInt(valueList[0]), stringToInt(valueList[1]) };
}

Vector3Int StringUtility::stringToVector3Int(const string& str, const char* seperate, bool* result)
{
	constexpr int INT_COUNT = sizeof(Vector3Int) / sizeof(int);
	Vector<string> valueList;
	split(str, seperate, valueList);
	if (result != nullptr)
	{
		*result = valueList.size() == INT_COUNT;
	}
	if (valueList.size() != INT_COUNT)
	{
		return { 0, 0, 0 };
	}
	return { stringToInt(valueList[0]), stringToInt(valueList[1]), stringToInt(valueList[2]) };
}

Vector2 StringUtility::stringToVector2(const string& str, const char* seperate, bool* result)
{
	constexpr int FLOAT_COUNT = sizeof(Vector2) / sizeof(float);
	Vector<string> valueList;
	split(str, seperate, valueList);
	if (result != nullptr)
	{
		*result = valueList.size() == FLOAT_COUNT;
	}
	if (valueList.size() != FLOAT_COUNT)
	{
		return { 0, 0 };
	}
	return { stringToFloat(valueList[0]), stringToFloat(valueList[1]) };
}

Vector3 StringUtility::stringToVector3(const string& str, const char* seperate, bool* result)
{
	constexpr int FLOAT_COUNT = sizeof(Vector3) / sizeof(float);
	Vector<string> valueList;
	split(str, seperate, valueList);
	if (result != nullptr)
	{
		*result = valueList.size() == FLOAT_COUNT;
	}
	if (valueList.size() != FLOAT_COUNT)
	{
		return { 0, 0, 0 };
	}
	return { stringToFloat(valueList[0]), stringToFloat(valueList[1]), stringToFloat(valueList[2]) };
}

void StringUtility::stringToInts(const string& str, Vector<int>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
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

uint StringUtility::stringToInts(const char* str, int* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("int buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString());
	}
	return curCount;
}

void StringUtility::stringToUInts(const string& str, Vector<uint>& valueList, const char* seperate)
{
	VECTOR(string, strList);
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back((uint)stringToInt(curStr));
		}
	}
	UN_VECTOR(string, strList);
}

uint StringUtility::stringToUInts(const char* str, uint* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("uint buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToInt(curString.toString());
	}
	return curCount;
}

void StringUtility::stringToULLongs(const char* str, Vector<ullong>& valueList, const char* seperate)
{
	Vector<string>strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToULLong(curStr));
		}
	}
}

uint StringUtility::stringToULLongs(const char* str, ullong* buffer, uint bufferSize, const char* seperate)
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
		if (curCount >= bufferSize)
		{
			ERROR("ullong buffer size is too small, bufferSize:" + intToString(bufferSize));
			break;
		}
		buffer[curCount++] = stringToULLong(curString.toString());
	}
	return curCount;
}

void StringUtility::stringToLLongs(const char* str, Vector<llong>& valueList, const char* seperate)
{
	Vector<string> strList;
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToLLong(curStr));
		}
	}
}

void StringUtility::stringToLLongs(const string& str, Vector<llong>& valueList, const char* seperate)
{
	VECTOR(string, strList);
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.clear();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToLLong(curStr));
		}
	}
	UN_VECTOR(string, strList);
}

string StringUtility::zeroString(uint zeroCount)
{
	Array<16> charArray{ 0 };
	FOR_I(zeroCount)
	{
		charArray[i] = '0';
	}
	charArray[zeroCount] = '\0';
	return charArray.toString();
}

string StringUtility::floatToStringExtra(float f, uint precision, bool removeTailZero)
{
	const static string zeroDot = "0.";
	string retString;
	if (!MathUtility::isFloatZero(f))
	{
		float powerValue = MathUtility::powerFloat(10.0f, precision);
		float totalValue = f * powerValue + MathUtility::sign(f) * 0.5f;
		if ((int)totalValue == 0)
		{
			if (precision > 0)
			{
				Array<16> temp{ 0 };
				zeroString(temp, precision);
				retString += zeroDot + temp.toString();
			}
			else
			{
				retString = "0";
			}
		}
		else
		{
			INT_STR(temp, abs((int)totalValue));
			retString = temp.toString();
			int dotPosition = (uint)retString.length() - precision;
			if (dotPosition <= 0)
			{
				Array<16> tempZero{ 0 };
				zeroString(tempZero, -dotPosition);
				retString = zeroDot + tempZero.toString() + retString;
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
			Array<16> tempZero{ 0 };
			zeroString(tempZero, precision);
			retString = zeroDot + tempZero.toString();
		}
		else
		{
			retString = "0";
		}
	}
	// 移除末尾无用的0
	if (removeTailZero && retString[retString.length() - 1] == '0')
	{
		size_t dotPos = retString.find_last_of('.');
		if (dotPos != NOT_FIND)
		{
			string floatPart = retString.substr(dotPos + 1, retString.length() - dotPos - 1);
			// 找到最后一个不是0的位置,然后将后面的所有0都去掉
			size_t notZeroPos = floatPart.find_last_not_of('0');
			// 如果小数部分全是0,则将小数点也一起去掉
			if (notZeroPos == NOT_FIND)
			{
				retString = retString.substr(0, dotPos);
			}
			// 去除小数部分末尾所有0
			else if (notZeroPos != floatPart.length() - 1)
			{
				retString = retString.substr(0, dotPos + 1) + floatPart.substr(0, notZeroPos + 1);
			}
		}
	}
	return retString;
}

string StringUtility::floatToString(float f)
{
	Array<16> temp{ 0 };
	floatToString(temp, f);
	return temp.toString();
}

void StringUtility::stringToFloats(const string& str, Vector<float>& valueList, const char* seperate)
{
	VECTOR(string, strList);
	split(str, seperate, strList);
	uint count = strList.size();
	valueList.reserve(count);
	FOR_I(count)
	{
		string& curStr = strList[i];
		if (curStr.length() > 0)
		{
			valueList.push_back(stringToFloat(curStr));
		}
	}
	UN_VECTOR(string, strList);
}

string StringUtility::bytesToString(const char* buffer, uint length)
{
	int size = MathUtility::getGreaterPower2(length + 1);
	char* tempBuffer = newCharArray(size);
	tempBuffer[length] = '\0';
	MEMCPY(tempBuffer, size, buffer, length);
	string str(tempBuffer);
	deleteCharArray(tempBuffer);
	return str;
}

bool StringUtility::endWith(const char* oriString, const char* pattern, bool sensitive)
{
	uint originLength = strlen(oriString);
	uint patternLength = strlen(pattern);
	if (originLength < patternLength)
	{
		return false;
	}
	if (sensitive)
	{
		FOR_I(patternLength)
		{
			if (oriString[i + originLength - patternLength] != pattern[i])
			{
				return false;
			}
		}
	}
	else
	{
		FOR_I(patternLength)
		{
			if (toLower(oriString[i + originLength - patternLength]) != toLower(pattern[i]))
			{
				return false;
			}
		}
	}
	return true;
}

bool StringUtility::endWith(const string& oriString, const char* pattern, bool sensitive)
{
	uint originLength = (uint)oriString.length();
	uint patternLength = strlen(pattern);
	if (originLength < patternLength)
	{
		return false;
	}
	if (sensitive)
	{
		FOR_I(patternLength)
		{
			if (oriString[i + originLength - patternLength] != pattern[i])
			{
				return false;
			}
		}
	}
	else
	{
		FOR_I(patternLength)
		{
			if (toLower(oriString[i + originLength - patternLength]) != toLower(pattern[i]))
			{
				return false;
			}
		}
	}
	return true;
}

bool StringUtility::startWith(const char* oriString, const char* pattern, bool sensitive)
{
	uint originLength = strlen(oriString);
	uint patternLength = strlen(pattern);
	if (originLength < patternLength)
	{
		return false;
	}
	if (sensitive)
	{
		FOR_I(patternLength)
		{
			if (oriString[i] != pattern[i])
			{
				return false;
			}
		}
	}
	else
	{
		FOR_I(patternLength)
		{
			if (toLower(oriString[i]) != toLower(pattern[i]))
			{
				return false;
			}
		}
	}
	return true;
}

bool StringUtility::startWith(const string& oriString, const char* pattern, bool sensitive)
{
	uint originLength = (uint)oriString.length();
	uint patternLength = strlen(pattern);
	if (originLength < patternLength)
	{
		return false;
	}
	if (sensitive)
	{
		FOR_I(patternLength)
		{
			if (oriString[i] != pattern[i])
			{
				return false;
			}
		}
	}
	else
	{
		FOR_I(patternLength)
		{
			if (toLower(oriString[i]) != toLower(pattern[i]))
			{
				return false;
			}
		}
	}
	return true;
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

#if RUN_PLATFORM == PLATFORM_WINDOWS
wstring StringUtility::ANSIToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str, -1, nullptr, 0);
	int allocSize = MathUtility::getGreaterPower2(unicodeLen + 1);
	wchar_t* pUnicode = new wchar_t[allocSize];
	MultiByteToWideChar(CP_ACP, 0, str, -1, (LPWSTR)pUnicode, unicodeLen);
	wstring ret(pUnicode);
	delete[] pUnicode;
	return ret;
}

void StringUtility::ANSIToUnicode(const char* str, wchar_t* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = L'\0';
		return;
	}
	int unicodeLen = MultiByteToWideChar(CP_ACP, 0, str, -1, nullptr, 0);
	if (unicodeLen >= (int)maxLength)
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
		return EMPTY;
	}
	int iTextLen = WideCharToMultiByte(CP_ACP, 0, str, -1, nullptr, 0, nullptr, nullptr);
	int allocSize = MathUtility::getGreaterPower2(iTextLen + 1);
	char* pElementText = newCharArray(allocSize);
	WideCharToMultiByte(CP_ACP, 0, str, -1, pElementText, iTextLen, nullptr, nullptr);
	string strText(pElementText);
	deleteCharArray(pElementText);
	return strText;
}

void StringUtility::UnicodeToANSI(const wchar_t* str, char* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = '\0';
		return;
	}
	int iTextLen = WideCharToMultiByte(CP_ACP, 0, str, -1, nullptr, 0, nullptr, nullptr);
	if (iTextLen >= (int)maxLength)
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
		return EMPTY;
	}
	// wide char to multi char
	int iTextLen = WideCharToMultiByte(CP_UTF8, 0, str, -1, nullptr, 0, nullptr, nullptr);
	int allocSize = MathUtility::getGreaterPower2(iTextLen + 1);
	char* pElementText = newCharArray(allocSize);
	WideCharToMultiByte(CP_UTF8, 0, str, -1, pElementText, iTextLen, nullptr, nullptr);
	string strText(pElementText);
	deleteCharArray(pElementText);
	return strText;
}

void StringUtility::UnicodeToUTF8(const wchar_t* str, char* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = '\0';
		return;
	}
	// wide char to multi char
	int iTextLen = WideCharToMultiByte(CP_UTF8, 0, str, -1, nullptr, 0, nullptr, nullptr);
	if (iTextLen >= (int)maxLength)
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
	int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
	int allocSize = MathUtility::getGreaterPower2(unicodeLen + 1);
	wchar_t* pUnicode = new wchar_t[allocSize];
	MultiByteToWideChar(CP_UTF8, 0, str, -1, (LPWSTR)pUnicode, unicodeLen);
	wstring rt(pUnicode);
	delete[] pUnicode;
	return rt;
}

void StringUtility::UTF8ToUnicode(const char* str, wchar_t* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = L'\0';
		return;
	}
	int unicodeLen = MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
	if (unicodeLen >= (int)maxLength)
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
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持GBK编码:") + e.what());
		return L"";
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	wchar_t* dBuf = ArrayPool::newArray<wchar_t>(allocSize, true);
	int resultLen = mbstowcs(dBuf, str, dSize);
	if (resultLen < 0)
	{
		LOG("ANSIToUnicode转换编码错误:" + string(str) + ", needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
	wstring strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}

void StringUtility::ANSIToUnicode(const char* str, wchar_t* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = L'\0';
		return;
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持GBK编码:") + e.what());
		output[0] = L'\0';
		return;
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	if (dSize >= (int)maxLength)
	{
		mSetLocaleLock.unlock();
		ERROR("buffer is too small");
		output[0] = L'\0';
		return;
	}
	int resultLen = mbstowcs(output, str, dSize);
	if (resultLen < 0)
	{
		LOG("ANSIToUnicode转换编码错误:" + string(str) + ", needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
}

string StringUtility::UnicodeToANSI(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return EMPTY;
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持GBK编码:") + e.what());
		return EMPTY;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	char* dBuf = newCharArray(allocSize);
	int resultLen = wcstombs(dBuf, str, dSize);
	if (resultLen < 0)
	{
		LOG("UnicodeToANSI转换编码错误, needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
	string strText(dBuf);
	deleteCharArray(dBuf);
	return strText;
}

void StringUtility::UnicodeToANSI(const wchar_t* str, char* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = '\0';
		return;
	}
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_GBK);
	}
	catch (const exception& e)
	{
		ERROR(string("当前系统不支持GBK编码:") + e.what());
		output[0] = '\0';
		return;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	if (dSize >= (int)maxLength)
	{
		ERROR("buffer is too small");
		output[0] = '\0';
		return;
	}
	int resultLen = wcstombs(output, str, dSize);
	if (resultLen < 0)
	{
		LOG("UnicodeToANSI转换编码错误, needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
}

string StringUtility::UnicodeToUTF8(const wchar_t* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return EMPTY;
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持UTF8编码:") + e.what());
		return EMPTY;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	char* dBuf = newCharArray(allocSize);
	int resultLen = wcstombs(dBuf, str, dSize);
	if (resultLen < 0)
	{
		LOG("UnicodeToUTF8转换编码错误, needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
	string strText(dBuf);
	deleteCharArray(dBuf);
	return strText;
}

void StringUtility::UnicodeToUTF8(const wchar_t* str, char* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		output[0] = '\0';
		return;
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持UTF8编码:") + e.what());
		output[0] = '\0';
		return;
	}
	int dSize = wcstombs(nullptr, str, 0) + 1;
	if (dSize >= (int)maxLength)
	{
		mSetLocaleLock.unlock();
		ERROR("buffer is too small");
		output[0] = '\0';
		return;
	}
	int resultLen = wcstombs(output, str, dSize);
	if (resultLen < 0)
	{
		LOG("UnicodeToUTF8转换编码错误, needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
}

wstring StringUtility::UTF8ToUnicode(const char* str)
{
	if (str == nullptr || str[0] == 0)
	{
		return L"";
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持UTF8编码:") + e.what());
		return L"";
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	int allocSize = MathUtility::getGreaterPower2(dSize);
	wchar_t* dBuf = ArrayPool::newArray<wchar_t>(allocSize, true);
	int resultLen = mbstowcs(dBuf, str, dSize);
	if (resultLen < 0)
	{
		LOG("UTF8ToUnicode转换编码错误:" + string(str) + ", needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
	wstring strText(dBuf);
	ArrayPool::deleteArray(dBuf);
	return strText;
}

void StringUtility::UTF8ToUnicode(const char* str, wchar_t* output, uint maxLength)
{
	if (str == nullptr || str[0] == 0)
	{
		maxLength = L'\0';
		return;
	}
	mSetLocaleLock.waitForUnlock(__FILE__, __LINE__);
	char* oldname = setlocale(LC_ALL, nullptr);
	try
	{
		setlocale(LC_ALL, LC_NAME_zh_CN_UTF8);
	}
	catch (const exception& e)
	{
		mSetLocaleLock.unlock();
		ERROR(string("当前系统不支持UTF8编码:") + e.what());
		maxLength = L'\0';
		return;
	}
	int dSize = mbstowcs(nullptr, str, 0) + 1;
	if (dSize >= (int)maxLength)
	{
		mSetLocaleLock.unlock();
		ERROR("buffer is too small");
		output[0] = L'\0';
		return;
	}
	int resultLen = mbstowcs(output, str, dSize);
	if (resultLen < 0)
	{
		LOG("UTF8ToUnicode转换编码错误:" + string(str) + ", needSize:" + intToString(dSize));
	}
	setlocale(LC_ALL, oldname);
	mSetLocaleLock.unlock();
}
#endif

string StringUtility::ANSIToUTF8(const char* str, bool addBOM)
{
	string utf8;
	ANSIToUTF8(str, utf8, addBOM);
	return utf8;
}

void StringUtility::ANSIToUTF8(const char* str, string& utf8, bool addBOM)
{
	uint length = strlen(str);
	uint unicodeLength = MathUtility::getGreaterPower2((length + 1) << 1);
	wchar_t* unicodeStr = new wchar_t[unicodeLength];
	ANSIToUnicode(str, unicodeStr, unicodeLength);
	utf8 = UnicodeToUTF8(unicodeStr);
	if (addBOM)
	{
		utf8 = BOM + utf8;
	}
	delete[] unicodeStr;
}

void StringUtility::ANSIToUTF8(const char* str, char* output, uint maxLength, bool addBOM)
{
	uint unicodeLength = MathUtility::getGreaterPower2((maxLength + 1) << 1);
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

void StringUtility::UTF8ToANSI(const char* str, string& ansi, bool eraseBOM)
{
	uint length = strlen(str);
	uint unicodeLength = MathUtility::getGreaterPower2((length + 1) << 1);
	wchar_t* unicodeStr = new wchar_t[unicodeLength];
	if (eraseBOM)
	{
		string newStr = str;
		removeBOM(newStr);
		UTF8ToUnicode(newStr.c_str(), unicodeStr, unicodeLength);
	}
	else
	{
		UTF8ToUnicode(str, unicodeStr, unicodeLength);
	}
	ansi = UnicodeToANSI(unicodeStr);
	delete[] unicodeStr;
}

void StringUtility::UTF8ToANSI(const char* str, char* output, uint maxLength, bool eraseBOM)
{
	uint unicodeLength = MathUtility::getGreaterPower2((maxLength + 1) << 1);
	wchar_t* unicodeStr = new wchar_t[unicodeLength];
	if (eraseBOM)
	{
		const char* newInput = str;
		uint length = strlen(str);
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
		str.erase(0, 3);
	}
}

void StringUtility::removeBOM(char* str, uint length)
{
	if (length == 0)
	{
		length = strlen(str);
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
	str += "\"" + name + "\":\"" + value + "\",";
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
	str += "\"" + name + "\":" + value + ",";
	if (returnLine)
	{
		str += "\r\n";
	}
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
bool StringUtility::findStringLower(const string& res, const string& sub, int* pos, uint startIndex, bool direction)
{
	// 全转换为小写
	return findString(toLower(res), toLower(sub).c_str(), pos, startIndex, direction);
}

bool StringUtility::findString(const string& res, const char* sub, int* pos, uint startIndex, bool direction)
{
	// 这里只是不再通过strlen获取字符串长度,而是直接string.length()获取,其余完全一致
	int posFind = -1;
	uint subLen = strlen(sub);
	int searchLength = (int)res.length() - (int)subLen + 1;
	if (searchLength <= 0)
	{
		return false;
	}
	if (direction)
	{
		for (int i = startIndex; i < searchLength; ++i)
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
		for (uint i = searchLength - 1; i >= startIndex; --i)
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

bool StringUtility::findString(const char* res, const char* sub, int* pos, uint startIndex, bool direction)
{
	int posFind = -1;
	uint subLen = strlen(sub);
	int searchLength = (int)strlen(res) - (int)subLen + 1;
	if (searchLength <= 0)
	{
		return false;
	}
	if (direction)
	{
		for (int i = startIndex; i < searchLength; ++i)
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
		for (uint i = searchLength - 1; i >= startIndex; --i)
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

int StringUtility::findStringPos(const char* res, const char* dst, uint startIndex, bool direction)
{
	int pos = -1;
	findString(res, dst, &pos, startIndex, direction);
	return pos;
}

int StringUtility::findStringPos(const string& res, const string& dst, uint startIndex, bool direction)
{
	int pos = -1;
	findString(res, dst.c_str(), &pos, startIndex, direction);
	return pos;
}

bool StringUtility::checkString(const string& str, const string& valid)
{
	uint oldStrLen = (uint)str.length();
	FOR_I(oldStrLen)
	{
		if (!valid.find_first_of(str[i]))
		{
			return false;
		}
	}
	return true;
}

bool StringUtility::checkFloatString(const string& str, const string& valid)
{
	return checkIntString(str, "." + valid);
}

bool StringUtility::checkIntString(const string& str, const string& valid)
{
	return checkString(str, "0123456789" + valid);
}

bool StringUtility::hasNonAscii(const char* str)
{
	uint len = strlen(str);
	FOR_I(len)
	{
		if (str[i] > 0x7F)
		{
			return false;
		}
	}
	return true;
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

string StringUtility::bytesToHexString(byte* data, uint dataCount, bool addSpace, bool upper)
{
	uint oneLength = addSpace ? 3 : 2;
	uint showCount = MathUtility::getGreaterPower2(dataCount * oneLength + 1);
	char* byteData = newCharArray(showCount);
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
	deleteCharArray(byteData);
	return str;
}

bool StringUtility::isNumber(const char* str, uint length)
{
	if (length == 0)
	{
		length = strlen(str);
	}
	FOR_I(length)
	{
		if (!isNumber(str[i]))
		{
			return false;
		}
	}
	return true;
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

uint StringUtility::getCharCount(const char* str, char key)
{
	uint count = 0;
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

bool StringUtility::isPhoneNumber(const char* str)
{
	// 手机号固定11位
	uint length = strlen(str);
	if (length != 11 || str[0] != '1')
	{
		return false;
	}
	FOR_I(length)
	{
		if (!isNumber(str[i]))
		{
			return false;
		}
	}
	return true;
}

void StringUtility::checkSQLString(const char*& str)
{
	// 带任何引号都不允许
	if (strchar(str, '\'') >= 0 || strchar(str, '\"') >= 0)
	{
		str = "";
	}
}

void StringUtility::sqlAddString(char* queryStr, uint size, const char* str, bool addComma)
{
	checkSQLString(str);
	strcat_t(queryStr, size, "\"", str, addComma ? "\"," : "\"");
}

void StringUtility::sqlAddStringUTF8(char* queryStr, uint size, const char* str, bool addComma)
{
	checkSQLString(str);
	if (str[0] != '\0')
	{
		char* utf8String = newCharArray(size);
		ANSIToUTF8(str, utf8String, size, false);
		strcat_t(queryStr, size, "\"", utf8String, addComma ? "\"," : "\"");
		deleteCharArray(utf8String);
	}
	else
	{
		strcat_t(queryStr, size, "\"", addComma ? "\"," : "\"");
	}
}

void StringUtility::sqlAddInt(char* queryStr, uint size, int value, bool addComma)
{
	INT_STR(temp, value);
	if (addComma)
	{
		strcat_t(queryStr, size, temp.toString(), ",");
	}
	else
	{
		strcat_s(queryStr, size, temp.toString());
	}
}

void StringUtility::sqlAddUInt(char* queryStr, uint size, uint value, bool addComma)
{
	UINT_STR(temp, value);
	if (addComma)
	{
		strcat_t(queryStr, size, temp.toString(), ",");
	}
	else
	{
		strcat_s(queryStr, size, temp.toString());
	}
}

void StringUtility::sqlAddFloat(char* queryStr, uint size, float value, bool addComma)
{
	FLOAT_STR(temp, value);
	if (addComma)
	{
		strcat_t(queryStr, size, temp.toString(), ",");
	}
	else
	{
		strcat_s(queryStr, size, temp.toString());
	}
}

void StringUtility::sqlAddULLong(char* queryStr, uint size, ullong value, bool addComma)
{
	ULLONG_STR(temp, value);
	if (addComma)
	{
		strcat_t(queryStr, size, temp.toString(), ",");
	}
	else
	{
		strcat_s(queryStr, size, temp.toString());
	}	
}

void StringUtility::sqlAddLLong(char* queryStr, uint size, llong value, bool addComma)
{
	LLONG_STR(temp, value);
	if (addComma)
	{
		strcat_t(queryStr, size, temp.toString(), ",");
	}
	else
	{
		strcat_s(queryStr, size, temp.toString());
	}
}

void StringUtility::sqlUpdateString(char* updateInfo, uint size, const char* col, const char* str, bool addComma)
{
	checkSQLString(str);
	strcat_t(updateInfo, size, col, " = \"", str, addComma ? "\"," : "\"");
}

void StringUtility::sqlUpdateStringUTF8(char* updateInfo, uint size, const char* col, const char* str, bool addComma)
{
	checkSQLString(str);
	char* utf8String = newCharArray(size);
	ANSIToUTF8(str, utf8String, size, false);
	strcat_t(updateInfo, size, col, " = \"", utf8String, addComma ? "\"," : "\"");
	deleteCharArray(utf8String);
}

void StringUtility::initIntToString()
{
	// double check 防止在多线程执行时多次重复初始化
	if (mIntString[0].length() == 0)
	{
		constexpr int SIZE = 16;
		FOR_I(mIntString.size())
		{
			if (i == 0)
			{
				mIntString[i] = "0";
				continue;
			}
			char buffer[SIZE]{ 0 };
			uint index = 0;
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
			uint lengthToHead = SIZE - index;
			FOR_J(index)
			{
				buffer[j] = buffer[j + lengthToHead] + '0';
			}
			buffer[index] = '\0';
			mIntString[i] = buffer;
		}
	}
}

uint StringUtility::greaterPower2(uint value)
{
	return MathUtility::getGreaterPower2(value);
}

uint StringUtility::base64_encode(const byte* text, uint text_len, byte* encode)
{
	uint i, j;
	for (i = 0, j = 0; i + 3 <= text_len; i += 3)
	{
		// 取出第一个字符的前6位并找出对应的结果字符
		encode[j++] = alphabet_map[text[i] >> 2];
		// 将第一个字符的后2位与第二个字符的前4位进行组合并找到对应的结果字符
		encode[j++] = alphabet_map[((text[i] << 4) & 0x30) | (text[i + 1] >> 4)];
		// 将第二个字符的后4位与第三个字符的前2位组合并找出对应的结果字符
		encode[j++] = alphabet_map[((text[i + 1] << 2) & 0x3c) | (text[i + 2] >> 6)];
		// 取出第三个字符的后6位并找出结果字符
		encode[j++] = alphabet_map[text[i + 2] & 0x3f];
	}

	if (i < text_len)
	{
		uint tail = text_len - i;
		if (tail == 1)
		{
			encode[j++] = alphabet_map[text[i] >> 2];
			encode[j++] = alphabet_map[(text[i] << 4) & 0x30];
			encode[j++] = '=';
			encode[j++] = '=';
		}
		else //tail==2
		{
			encode[j++] = alphabet_map[text[i] >> 2];
			encode[j++] = alphabet_map[((text[i] << 4) & 0x30) | (text[i + 1] >> 4)];
			encode[j++] = alphabet_map[(text[i + 1] << 2) & 0x3c];
			encode[j++] = '=';
		}
	}
	return j;
}

uint StringUtility::base64_decode(const byte* code, uint code_len, byte* dest)
{
	uint i, j = 0;
	byte quad[4];
	for (i = 0; i < code_len; i += 4)
	{
		// 分组，每组四个分别依次转换为base64表内的十进制数
		for (uint k = 0; k < 4; k++)
		{
			quad[k] = reverse_map[code[i + k]];
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
	return j;
}