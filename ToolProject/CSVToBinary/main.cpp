#include "SQLiteTableBase.h"
#include "Serializer.h"
#include "FileContent.h"
#include "Vector2.h"
#include "Vector2Int.h"
#include "Vector3Int.h"
#include "Vector2UInt.h"
#include "Vector2UShort.h"
#include "Vector2Short.h"
#include "Vector3.h"

enum class OWNER : byte
{
	NONE,				// 不属于客户端或者服务器,仅表格辅助作用
	BOTH,				// 客户端和服务器都会用到
	CLIENT_ONLY,		// 仅客户端用
	SERVER_ONLY,		// 仅服务器用
};

constexpr int ROW_TABLE_NAME = 0;
constexpr int ROW_TABLE_OWNER = 1;
constexpr int ROW_COLUMN_NAME = 2;
constexpr int ROW_COLUMN_TYPE = 3;
constexpr int ROW_COLUMN_OWNER = 4;
constexpr int ROW_COLUMN_COMMENT = 5;
constexpr int ROW_COLUMN_LINK_TABLE = 6;
constexpr int ROW_COLUMN_LINK_LENGTH = 7;
constexpr int ROW_COLUMN_FLAG = 8;
constexpr int HEADER_DATA_ROW = 9;

struct CSVColumn
{
	OWNER mOwner = OWNER::NONE;
	string mType;
	string mColumnName;
	string mComment;
	string mLinkTable;
	string mLinkLength;
	string mFlag;
};

struct CSVHeader
{
	Vector<CSVColumn> mColumnDataList;
	OWNER mOwner = OWNER::NONE;
	string mTableName;
};

OWNER getOwner(const string& owner)
{
	if (owner == "None")
	{
		return OWNER::NONE;
	}
	else if (owner == "Server")
	{
		return OWNER::SERVER_ONLY;
	}
	else if (owner == "Client")
	{
		return OWNER::CLIENT_ONLY;
	}
	else if (owner == "Both")
	{
		return OWNER::BOTH;
	}
	return OWNER::NONE;
}

void parseCSVLines(const string& fullContent, Vector<Vector<string>>& result)
{
	Vector<string> row;
	string field;
	bool inQuotes = false;
	for (int i = 0; i < (int)fullContent.size(); ++i)
	{
		const char c = fullContent[i];
		if (c == '"')
		{
			if (inQuotes && i + 1 < fullContent.size() && fullContent[i + 1] == '"')
			{
				// 转义的双引号 ""
				field.push_back('"');
				++i;
			}
			else
			{
				// 进入或退出引号
				inQuotes = !inQuotes;
			}
		}
		else if (c == ',' && !inQuotes)
		{
			// 逗号分隔列
			row.push_back(field);
			field.clear();
		}
		else if ((c == '\n' || c == '\r') && !inQuotes)
		{
			// 遇到换行，完成一行,即使空行也要放进去
			row.push_back(field);
			field.clear();
			result.push_back(row);
			row.clear();
			// 处理 \r\n 的情况：如果当前是 \r 且下一个是 \n，就跳过 \n
			if (c == '\r' && i + 1 < fullContent.size() && fullContent[i + 1] == '\n')
			{
				++i;
			}
		}
		else
		{
			// 普通字符
			field.push_back(c);
		}
	}

	// 最后一行如果没加进去，要补上
	if (!field.empty() || row.size() != 0)
	{
		row.push_back(field);
		result.push_back(row);
	}
}

void parseCSV(const string& filePath, CSVHeader& header, Vector<Vector<string>>& dataList)
{
	Vector<Vector<string>> originData;
	parseCSVLines(FileUtility::openTxtFile(filePath, false), originData);
	FOR_I(HEADER_DATA_ROW)
	{
		const Vector<string>& line = originData[i];
		// 表名
		if (i == ROW_TABLE_NAME)
		{
			header.mTableName = line[0];
		}
		// 表所属
		else if (i == ROW_TABLE_OWNER)
		{
			header.mOwner = getOwner(line[0]);
		}
		// 字段名
		else if (i == ROW_COLUMN_NAME)
		{
			FOR_VECTOR_J(line)
			{
				CSVColumn column;
				column.mColumnName = line[j];
				header.mColumnDataList.push_back(column);
			}
		}
		// 字段类型
		else if (i == ROW_COLUMN_TYPE)
		{
			FOR_VECTOR_J(line)
			{
				string type = line[j];
				// 枚举类型的实际基础数据类型
				int leftPos = 0;
				int rightPos = 0;
				if (StringUtility::findString(type, "(", &leftPos) &&
					StringUtility::findString(type, ")", &rightPos))
				{
					string realType = type.substr(leftPos + 1, rightPos - leftPos - 1);
					// 列表类型,则替换列表的元素类型
					int leftListPos = 0;
					int rightListPos = 0;
					if (StringUtility::findString(type, "Vector<", &leftListPos) &&
						StringUtility::findString(type, ">", &rightListPos))
					{
						StringUtility::replace(type, (int)strlen("Vector<") + leftListPos, rightListPos, realType);
					}
					// 非列表,则直接替换类型
					else
					{
						type = realType;
					}
				}
				header.mColumnDataList[j].mType = type;
			}
		}
		// 字段所属
		else if (i == ROW_COLUMN_OWNER)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j].mOwner = getOwner(line[j]);
			}
		}
		// 字段注释
		else if (i == ROW_COLUMN_COMMENT)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j].mComment = line[j];
			}
		}
		// 链接表格
		else if (i == ROW_COLUMN_LINK_TABLE)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j].mLinkTable = line[j];
			}
		}
		// 长度链接
		else if (i == ROW_COLUMN_LINK_LENGTH)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j].mLinkLength = line[j];
			}
		}
		// 字段标签
		else if (i == ROW_COLUMN_FLAG)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j].mFlag = line[j];
			}
		}
	}
	for (int i = HEADER_DATA_ROW; i < (int)originData.size(); ++i)
	{
		dataList.push_back(originData[i]);
	}
}

void csvToBinary(const string& destPath, const CSVHeader& header, const Vector<Vector<string>>& dataList, bool targetIsClient)
{
	const string tableName = StringUtility::getFileNameNoSuffix(destPath, true);
	Serializer serializer;
	FOR_VECTOR(dataList)
	{
		const auto& line = dataList[i];
		if (line.size() != header.mColumnDataList.size())
		{
			cout << "数据字段数量与表头数量不匹配,数据有" + StringUtility::intToString(line.size()) + "列,表头有" + StringUtility::intToString(header.mColumnDataList.size()) + "列,表格:" << tableName << endl;
			system("pause");
			return;
		}
		FOR_VECTOR_J(line)
		{
			const CSVColumn& column = header.mColumnDataList[j];
			const string& typeName = column.mType;
			if (targetIsClient)
			{
				if (column.mOwner != OWNER::CLIENT_ONLY && column.mOwner != OWNER::BOTH)
				{
					continue;
				}
			}
			else
			{
				if (column.mOwner != OWNER::SERVER_ONLY && column.mOwner != OWNER::BOTH)
				{
					continue;
				}
			}
			const string& value = line[j];
			if (typeName == "bool")
			{
				serializer.write(StringUtility::stringToInt(value) != 0);
			}
			else if (typeName == "byte")
			{
				serializer.write((byte)StringUtility::stringToInt(value));
			}
			else if (typeName == "char")
			{
				serializer.write((char)StringUtility::stringToInt(value));
			}
			else if (typeName == "short")
			{
				serializer.write((short)StringUtility::stringToInt(value));
			}
			else if (typeName == "ushort")
			{
				serializer.write((ushort)StringUtility::stringToInt(value));
			}
			else if (typeName == "int")
			{
				serializer.write(StringUtility::stringToInt(value));
			}
			else if (typeName == "uint")
			{
				serializer.write((uint)StringUtility::stringToInt(value));
			}
			else if (typeName == "float")
			{
				serializer.write((float)StringUtility::stringToFloat(value));
			}
			else if (typeName == "llong")
			{
				serializer.write((llong)StringUtility::stringToLLong(value));
			}
			else if (typeName == "string")
			{
				serializer.writeString(value.c_str());
			}
			else if (typeName == "Vector<bool>")
			{
				Vector<bool> bools;
				StringUtility::stringToBools(value, bools);
				serializer.writeArray(bools);
			}
			else if (typeName == "Vector<byte>")
			{
				Vector<byte> bytes;
				StringUtility::stringToBytes(value, bytes);
				serializer.writeArray(bytes);
			}
			else if (typeName == "Vector<short>")
			{
				Vector<short> shorts;
				StringUtility::stringToShorts(value, shorts);
				serializer.writeArray(shorts);
			}
			else if (typeName == "Vector<ushort>")
			{
				Vector<ushort> ushorts;
				StringUtility::stringToUShorts(value, ushorts);
				serializer.writeArray(ushorts);
			}
			else if (typeName == "Vector<int>")
			{
				Vector<int> ints;
				StringUtility::stringToInts(value, ints);
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<uint>")
			{
				Vector<uint> uints;
				StringUtility::stringToUInts(value, uints);
				serializer.writeArray(uints);
			}
			else if (typeName == "Vector<llong>")
			{
				Vector<llong> llongs;
				StringUtility::stringToLLongs(value, llongs);
				serializer.writeArray(llongs);
			}
			else if (typeName == "Vector<float>")
			{
				Vector<float> floats;
				StringUtility::stringToFloats(value, floats);
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<Vector2Int>")
			{
				Vector<Vector2Int> ints;
				if (!StringUtility::stringToVector2Ints(value, ints, "|"))
				{
					cout << "字段内容错误,类型Vector<Vector2Int>,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<Vector3Int>")
			{
				Vector<Vector3Int> ints;
				if (!StringUtility::stringToVector3Ints(value, ints, "|"))
				{
					cout << "字段内容错误,类型Vector<Vector3Int>,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<Vector2>")
			{
				Vector<Vector2> floats;
				if (!StringUtility::stringToVector2s(value, floats, "|"))
				{
					cout << "字段内容错误,类型Vector<Vector2>,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<Vector3>")
			{
				Vector<Vector3> floats;
				if (!StringUtility::stringToVector3s(value, floats, "|"))
				{
					cout << "字段内容错误,类型Vector<Vector3>,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<string>")
			{
				Vector<string> strings;
				StringUtility::split(value, ",", strings);
				serializer.writeArray(strings);
			}
			else if (typeName == "Vector2")
			{
				Vector<float> values;
				StringUtility::stringToFloats(value, values);
				if (values.size() != 2)
				{
					cout << "字段内容错误,类型Vector2,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.write(Vector2(values[0], values[1]));
			}
			else if (typeName == "Vector2Int")
			{
				Vector<int> values;
				StringUtility::stringToInts(value, values);
				if (values.size() != 2)
				{
					cout << "字段内容错误,类型Vector2Int,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.write(Vector2Int(values[0], values[1]));
			}
			else if (typeName == "Vector3")
			{
				Vector<float> values;
				StringUtility::stringToFloats(value, values);
				if (values.size() != 3)
				{
					cout << "字段内容错误,类型Vector3,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.write(Vector3(values[0], values[1], values[2]));
			}
			else if (typeName == "Vector3Int")
			{
				Vector<int> values;
				StringUtility::stringToInts(value, values);
				if (values.size() != 3)
				{
					cout << "字段内容错误,类型Vector3Int,字段名" << column.mColumnName << ",表格:" << tableName + ",ID:" + line[0] << endl;
					system("pause");
					return;
				}
				serializer.write(Vector3Int(values[0], values[1], values[2]));
			}
			else
			{
				cout << "无法识别的字段类型:" << typeName << ",表格:" << tableName << ",字段名:" << column.mColumnName << endl;
				system("pause");
				return;
			}
		}
	}
	// 重新计算密钥
	string key = "ASLD" + tableName;
	key = FileUtility::generateFileMD5(key.c_str(), (int)key.length()) + "23y35y983";
	char* buffer = serializer.getWriteableBuffer();
	uint bufferSize = serializer.getBufferSize();
	FOR_I(bufferSize)
	{
		buffer[i] = (buffer[i] - ((i << 1) & 0xFF)) ^ key[i % key.length()];
	}

	serializer.writeToFile(destPath);
	cout << "生成文件:" << destPath << endl;
}

string csvPath;
string clientDestPath;
string serverDestPath;
bool parseConfig(const string& filePath)
{
	Vector<string> lines;
	if (!FileUtility::openTxtFileLines(filePath, lines, true))
	{
		cout << "找不到" + filePath << endl;
		system("pause");
		return false;
	}
	for (const string& line : lines)
	{
		Vector<string> params;
		StringUtility::split(line, "=", params);
		if (params.size() != 2)
		{
			continue;
		}
		const string& paramName = params[0];
		const string& paramValue = params[1];
		if (paramName == "CSVPath")
		{
			csvPath = paramValue;
		}
		else if (paramName == "ClientDestPath")
		{
			clientDestPath = paramValue;
		}
		else if (paramName == "ServerDestPath")
		{
			serverDestPath = paramValue;
		}
	}
	if (csvPath.empty())
	{
		ERROR("参数解析错误,找不到CSVPath");
		return false;
	}
	if (clientDestPath.empty())
	{
		ERROR("参数解析错误,找不到ClientDestPath");
		return false;
	}
	if (serverDestPath.empty())
	{
		ERROR("参数解析错误,找不到ServerDestPath");
		return false;
	}
	return true;
}

int main()
{
	if (!parseConfig("./CSVToBinaryConfig.txt"))
	{
		return 0;
	}
	// 删除之前生成的文件
	Vector<string> tempBytesFiles;
	FileUtility::findFiles(serverDestPath, tempBytesFiles, ".bytes");
	for (const string& file : tempBytesFiles)
	{
		FileUtility::deleteFile(file);
	}
	FileUtility::findFiles(clientDestPath, tempBytesFiles, ".bytes");
	for (const string& file : tempBytesFiles)
	{
		FileUtility::deleteFile(file);
	}

	Vector<string> files;
	FileUtility::findFiles(csvPath, files, ".csv");
	for(const string& file : files)
	{
		CSVHeader header;
		Vector<Vector<string>> dataList;
		parseCSV(file, header, dataList);
		const string fileName = StringUtility::getFileNameNoSuffix(file, true);
		if (header.mOwner == OWNER::BOTH || header.mOwner == OWNER::CLIENT_ONLY)
		{
			csvToBinary(clientDestPath + "/" + fileName + ".bytes", header, dataList, true);
		}
		if (header.mOwner == OWNER::BOTH || header.mOwner == OWNER::SERVER_ONLY)
		{
			csvToBinary(serverDestPath + "/" + fileName + ".bytes", header, dataList, false);
		}
	}
	return 0;
}