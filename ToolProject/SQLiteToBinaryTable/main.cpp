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

string START_FALG = "#start";

enum class SQLITE_OWNER : byte
{
	NONE,				// 不属于客户端或者服务器,仅表格辅助作用
	BOTH,				// 客户端和服务器都会用到
	CLIENT_ONLY,		// 仅客户端用
	SERVER_ONLY,		// 仅服务器用
};

struct SQLiteMember
{
	SQLITE_OWNER mOwner = SQLITE_OWNER::NONE;
	string mTypeName;
	string mMemberName;
	string mComment;
};

struct SQLiteInfo
{
	Vector<SQLiteMember> mMemberList;
	SQLITE_OWNER mOwner = SQLITE_OWNER::NONE;
	string mSQLiteName;
	string mComment;
	bool mHotFix = false;
	bool mClientSQLite = false;
};

void parseSQLiteDescription(const string& filePath, Map<string, SQLiteInfo>& sqliteInfoList)
{
	Vector<string> files;
	FileUtility::findFiles(filePath, files, ".db");
	FOR_VECTOR(files)
	{
		SQLiteTableBase table;
		table.setTableName("Z_Description");
		table.init(files[i]);
		SQLiteDataReader* reader = table.doSelect();
		if (reader == nullptr)
		{
			cout << "加载表格失败:" << files[i] << endl;
			system("pause");
			return;
		}
		SQLiteInfo info;
		info.mSQLiteName = StringUtility::getFileNameNoSuffix(files[i], true);
		FOR_J(4)
		{
			reader->read();
			string value;
			reader->getString(2, value);
			if (j == 0)
			{
				info.mComment = value;
			}
			else if (j == 1)
			{
				info.mHotFix = StringUtility::stringToBool(value);
			}
			else if (j == 2)
			{
				if (value == "All")
				{
					info.mOwner = SQLITE_OWNER::BOTH;
				}
				else if (value == "Client")
				{
					info.mOwner = SQLITE_OWNER::CLIENT_ONLY;
				}
				else if (value == "Server")
				{
					info.mOwner = SQLITE_OWNER::SERVER_ONLY;
				}
				else if (value == "None")
				{
					info.mOwner = SQLITE_OWNER::NONE;
				}
			}
			else if (j == 3)
			{
				info.mClientSQLite = StringUtility::stringToBool(value);
			}
		}
		while (reader->read())
		{
			SQLiteMember member;
			string value;
			reader->getString(1, value);
			if (value == "All")
			{
				member.mOwner = SQLITE_OWNER::BOTH;
			}
			else if (value == "Client")
			{
				member.mOwner = SQLITE_OWNER::CLIENT_ONLY;
			}
			else if (value == "Server")
			{
				member.mOwner = SQLITE_OWNER::SERVER_ONLY;
			}
			else if (value == "None")
			{
				member.mOwner = SQLITE_OWNER::NONE;
			}
			reader->getString(2, value);
			member.mMemberName = value;
			reader->getString(3, value);
			member.mTypeName = value;
			// 枚举类型的实际基础数据类型
			int leftPos = 0;
			int rightPos = 0;
			if (StringUtility::findString(member.mTypeName, "(", &leftPos) &&
				StringUtility::findString(member.mTypeName, ")", &rightPos))
			{
				string realType = member.mTypeName.substr(leftPos + 1, rightPos - leftPos - 1);
				// 列表类型,则替换列表的元素类型
				int leftListPos = 0;
				int rightListPos = 0;
				if (StringUtility::findString(member.mTypeName, "Vector<", &leftListPos) &&
					StringUtility::findString(member.mTypeName, ">", &rightListPos))
				{
					StringUtility::replace(member.mTypeName, strlen("Vector<") + leftListPos, rightListPos, realType);
				}
				// 非列表,则直接替换类型
				else
				{
					member.mTypeName = realType;
				}
			}
			reader->getString(4, value);
			member.mComment = value;
			info.mMemberList.push_back(member);
		}
		sqliteInfoList.insert(info.mSQLiteName, info);
		table.releaseReader(reader);
	}
	END(files);
}

void copySQLiteToClient(const string& file, const string& destPath)
{
	FileContent content;
	if (!FileUtility::openBinaryFile(file, &content))
	{
		cout << "打开文件失败:" << file << endl;
		system("pause");
	}
	// 加密
	string tableName = StringUtility::getFileNameNoSuffix(file, true);
	string key = "ASLD" + tableName;
	key = FileUtility::generateFileMD5(key.c_str(), key.length()) + "23y35y9832635872349862365274732047chsudhgkshgwshfoweh238c42384fync9388v45982nc3484";
	// 只加密128分之1
	const uint bufferSize = content.mFileSize >> 7;
	FOR_I(bufferSize)
	{
		content.mBuffer[i] ^= key[i % key.length()];
	}
	string sqliteFilePath = destPath + "/" + tableName + ".bytes";
	FileUtility::writeFile(sqliteFilePath, content.mBuffer, content.mFileSize);
	cout << "加密并拷贝文件:" << sqliteFilePath << endl;
}

bool sqliteToBinary(const string& file, const SQLiteInfo& sqliteTableInfo, const string& destPath, bool isClient)
{
	string tableName = StringUtility::getFileNameNoSuffix(file, true);
	Serializer serializer;
	SQLiteTableBase table;
	table.setTableName(tableName);
	table.init(file);
	SQLiteDataReader* reader = table.doSelect();
	if (reader == nullptr)
	{
		cout << "加载表格失败:" << file << endl;
		system("pause");
		return false;
	}
	int colCount = 0;
	while (reader->read())
	{
		if (colCount == 0)
		{
			colCount = reader->getColumnCount();
		}
		const auto& memberList = sqliteTableInfo.mMemberList;
		if (colCount != memberList.size())
		{
			cout << "表格实际的字段数与描述表格中写的字段数不一致:" << file << endl;
			system("pause");
			return false;
		}
		FOR_CONST_J(memberList)
		{
			const string& typeName = memberList[j].mTypeName;
			if (isClient)
			{
				if (memberList[j].mOwner != SQLITE_OWNER::CLIENT_ONLY && memberList[j].mOwner != SQLITE_OWNER::BOTH)
				{
					continue;
				}
			}
			else
			{
				if (memberList[j].mOwner != SQLITE_OWNER::SERVER_ONLY && memberList[j].mOwner != SQLITE_OWNER::BOTH)
				{
					continue;
				}
			}
			if (typeName == "bool")
			{
				serializer.write(reader->getInt(j) != 0);
			}
			else if (typeName == "byte")
			{
				serializer.write((byte)reader->getInt(j));
			}
			else if (typeName == "char")
			{
				serializer.write((char)reader->getInt(j));
			}
			else if (typeName == "short")
			{
				serializer.write((short)reader->getInt(j));
			}
			else if (typeName == "ushort")
			{
				serializer.write((ushort)reader->getInt(j));
			}
			else if (typeName == "int")
			{
				serializer.write(reader->getInt(j));
			}
			else if (typeName == "uint")
			{
				serializer.write((uint)reader->getInt(j));
			}
			else if (typeName == "float")
			{
				serializer.write(reader->getFloat(j));
			}
			else if (typeName == "llong")
			{
				serializer.write(reader->getLLong(j));
			}
			else if (typeName == "string")
			{
				string value;
				reader->getString(j, value, false);
				serializer.writeString(value.c_str());
			}
			else if (typeName == "Vector<bool>")
			{
				string value;
				reader->getString(j, value);
				Vector<bool> bools;
				StringUtility::stringToBools(value, bools);
				serializer.writeArray(bools);
			}
			else if (typeName == "Vector<byte>")
			{
				string value;
				reader->getString(j, value);
				Vector<byte> bytes;
				StringUtility::stringToBytes(value, bytes);
				serializer.writeArray(bytes);
			}
			else if (typeName == "Vector<short>")
			{
				string value;
				reader->getString(j, value);
				Vector<short> shorts;
				StringUtility::stringToShorts(value, shorts);
				serializer.writeArray(shorts);
			}
			else if (typeName == "Vector<ushort>")
			{
				string value;
				reader->getString(j, value);
				Vector<ushort> ushorts;
				StringUtility::stringToUShorts(value, ushorts);
				serializer.writeArray(ushorts);
			}
			else if (typeName == "Vector<int>")
			{
				string value;
				reader->getString(j, value);
				Vector<int> ints;
				StringUtility::stringToInts(value, ints);
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<uint>")
			{
				string value;
				reader->getString(j, value);
				Vector<uint> uints;
				StringUtility::stringToUInts(value, uints);
				serializer.writeArray(uints);
			}
			else if (typeName == "Vector<float>")
			{
				string value;
				reader->getString(j, value);
				Vector<float> floats;
				StringUtility::stringToFloats(value, floats);
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<Vector2Int>")
			{
				string value;
				reader->getString(j, value);
				Vector<Vector2Int> ints;
				if (!StringUtility::stringToVector2Ints(value, ints, "|"))
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector<Vector2Int>,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<Vector3Int>")
			{
				string value;
				reader->getString(j, value);
				Vector<Vector3Int> ints;
				if (!StringUtility::stringToVector3Ints(value, ints, "|"))
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector<Vector3Int>,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.writeArray(ints);
			}
			else if (typeName == "Vector<Vector2>")
			{
				string value;
				reader->getString(j, value);
				Vector<Vector2> floats;
				if (!StringUtility::stringToVector2s(value, floats, "|"))
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector<Vector2>,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<Vector3>")
			{
				string value;
				reader->getString(j, value);
				Vector<Vector3> floats;
				if (!StringUtility::stringToVector3s(value, floats, "|"))
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector<Vector3>,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.writeArray(floats);
			}
			else if (typeName == "Vector<string>")
			{
				string value;
				reader->getString(j, value, false);
				Vector<string> strings;
				StringUtility::split(value, ",", strings);
				serializer.writeArray(strings);
			}
			else if (typeName == "Vector2Short")
			{
				string value;
				reader->getString(j, value);
				Vector<short> shorts;
				StringUtility::stringToShorts(value, shorts);
				if (shorts.size() != 2)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector2Short,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector2Short(shorts[0], shorts[1]));
			}
			else if (typeName == "Vector2UShort")
			{
				string value;
				reader->getString(j, value);
				Vector<ushort> ushorts;
				StringUtility::stringToUShorts(value, ushorts);
				if (ushorts.size() != 2)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector2Short,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector2UShort(ushorts[0], ushorts[1]));
			}
			else if (typeName == "Vector2Int")
			{
				string value;
				reader->getString(j, value);
				Vector<int> ints;
				StringUtility::stringToInts(value, ints);
				if (ints.size() != 2)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector2Int,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector2Int(ints[0], ints[1]));
			}
			else if (typeName == "Vector2UInt")
			{
				string value;
				reader->getString(j, value);
				Vector<uint> uints;
				StringUtility::stringToUInts(value, uints);
				if (uints.size() != 2)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector2UInt,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector2UInt(uints[0], uints[1]));
			}
			else if (typeName == "Vector2")
			{
				string value;
				reader->getString(j, value);
				Vector<float> values;
				StringUtility::stringToFloats(value, values);
				if (values.size() != 2)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector2,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector2(values[0], values[1]));
				}
			else if (typeName == "Vector3")
			{
				string value;
				reader->getString(j, value);
				Vector<float> values;
				StringUtility::stringToFloats(value, values);
				if (values.size() != 3)
				{
					llong id = reader->getLLong(0);
					cout << "字段内容错误,类型Vector3,字段名" << memberList[j].mMemberName << ",表格:" << tableName + ",ID:" + StringUtility::llongToString(id) << endl;
					system("pause");
					return false;
				}
				serializer.write(Vector3(values[0], values[1], values[2]));
			}
			else
			{
				cout << "无法识别的字段类型:" << typeName << ",表格:" << tableName << endl;
				system("pause");
				return false;
			}
		}
		END_CONST();
	}
	table.releaseReader(reader);
	// 重新计算密钥
	string key = "ASLD" + tableName;
	key = FileUtility::generateFileMD5(key.c_str(), key.length()) + "23y35y983";
	char* buffer = serializer.getWriteableBuffer();
	uint bufferSize = serializer.getBufferSize();
	FOR_I(bufferSize)
	{
		buffer[i] = (buffer[i] - ((i << 1) & 0xFF)) ^ key[i % key.length()];
	}

	string fullPath = destPath + "/" + tableName + ".bytes";
	serializer.writeToFile(fullPath);
	cout << "生成文件:" << fullPath << endl;
	return true;
}

string dataBasePath;
string clientDestSQLitePath;
string serverDestPath;
bool parseConfig(const string& path)
{ 
	Vector<string> lines;
	if (!FileUtility::openTxtFileLines(path, lines, true))
	{
		cout << "找不到SQLiteToBinaryTableConfig.txt" << endl;
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
		if (paramName == "DataBasePath")
		{
			dataBasePath = paramValue;
		}
		else if (StringUtility::startWith(paramName, "ClientDestSQLitePath"))
		{
			clientDestSQLitePath = paramValue;
		}
		else if (paramName == "ServerDestPath")
		{
			serverDestPath = paramValue;
		}
	}
	if (dataBasePath.empty())
	{
		ERROR("参数解析错误,找不到DataBasePath");
		return 0;
	}
	if (clientDestSQLitePath.empty())
	{
		ERROR("参数解析错误,找不到ClientDestSQLitePath");
		return 0;
	}
	if (serverDestPath.empty())
	{
		ERROR("参数解析错误,找不到ServerDestPath");
		return 0;
	}
	return true;
}

int main()
{
	// 解析配置文件
	parseConfig("./SQLiteToBinaryTableConfig.txt");

	// 不能删除之前生成的文件,因为所有的csv和db文件生成的bytes是在一起的,所以不能删除,只能覆盖

	// 解析表头
	Map<string, SQLiteInfo> sqliteInfoList;
	parseSQLiteDescription(dataBasePath, sqliteInfoList);
	if (sqliteInfoList.size() == 0)
	{
		return 0;
	}

	Vector<string> files;
	FileUtility::findFiles(dataBasePath, files, ".db");
	for(const string& file : files)
	{
		string tableName = StringUtility::getFileNameNoSuffix(file, true);
		const SQLiteInfo& sqliteTableInfo = sqliteInfoList[tableName];
		// 转换给客户端,这种情况下客户端只会使用表格原始文件,只需要拷贝并加密即可
		if (sqliteTableInfo.mOwner == SQLITE_OWNER::CLIENT_ONLY || sqliteTableInfo.mOwner == SQLITE_OWNER::BOTH)
		{
			copySQLiteToClient(file, clientDestSQLitePath);
		}
		// 转换给服务器
		if (sqliteTableInfo.mOwner == SQLITE_OWNER::SERVER_ONLY || sqliteTableInfo.mOwner == SQLITE_OWNER::BOTH)
		{
			sqliteToBinary(file, sqliteTableInfo, serverDestPath, false);
		}
	}
	return 0;
}