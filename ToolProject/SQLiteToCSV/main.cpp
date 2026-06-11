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

enum class SQLITE_OWNER : byte
{
	NONE,				// 不属于客户端或者服务器,仅表格辅助作用
	BOTH,				// 客户端和服务器都会用到
	CLIENT_ONLY,		// 仅客户端用
	SERVER_ONLY,		// 仅服务器用
};

struct SQLiteMember
{
	SQLITE_OWNER mOwner;
	string mTypeName;
	string mMemberName;
	string mComment;
	string mLinkTable;
};

struct SQLiteInfo
{
	Vector<SQLiteMember> mMemberList;
	SQLITE_OWNER mOwner;
	string mSQLiteName;
	string mComment;
	bool mHotFix;
	bool mClientSQLite;
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
			reader->getString(4, value);
			member.mComment = value;
			reader->getString(5, value);
			member.mLinkTable = value;
			info.mMemberList.push_back(member);
		}
		sqliteInfoList.insert(info.mSQLiteName, info);
		table.releaseReader(reader);
	}
}

void readSQLiteData(const string& path, const SQLiteInfo& sqliteInfo, Vector<Vector<string>>& dataList)
{
	SQLiteTableBase table;
	table.setTableName(StringUtility::getFileNameNoSuffix(path, true));
	table.init(path);
	SQLiteDataReader* reader = table.doSelect();
	if (reader == nullptr)
	{
		cout << "加载表格失败:" << path << endl;
		system("pause");
		return;
	}

	while (reader->read())
	{
		Vector<string> row;
		const auto& memberList = sqliteInfo.mMemberList;
		FOR_CONST_J(memberList)
		{
			string value;
			reader->getString(j, value);
			row.push_back(value);
		}
		dataList.push_back(row);
	}
	table.releaseReader(reader);
}

void appendCSVString(string& file, const string& value, bool addCommaOrReturn)
{
	string cell = value;
	// 判断是否需要加引号
	if ((cell.find(',') != -1) || (cell.find('"') != -1) ||
		(cell.find('\n') != -1) || (cell.find('\r') != -1))
	{
		string escaped;
		escaped.reserve(cell.size());
		for (char c : cell)
		{
			if (c == '"')
			{
				escaped += "\"\"";  // 替换 " 为 ""
			}
			else
			{
				escaped += c;
			}
		}
		cell = "\"" + escaped + "\"";
	}
	file += cell;
	if (addCommaOrReturn)
	{
		file += ",";
	}
	else
	{
		file += "\n";
	}
}

string getOwnerString(SQLITE_OWNER owner)
{
	if (owner == SQLITE_OWNER::NONE)
	{
		return "None";
	}
	else if (owner == SQLITE_OWNER::SERVER_ONLY)
	{
		return "Server";
	}
	else if (owner == SQLITE_OWNER::CLIENT_ONLY)
	{
		return "Client";
	}
	else if (owner == SQLITE_OWNER::BOTH)
	{
		return "Both";
	}
	return "";
}

SQLITE_OWNER getOwner(const string& owner)
{
	if (owner == "None")
	{
		return SQLITE_OWNER::NONE;
	}
	else if (owner == "Server")
	{
		return SQLITE_OWNER::SERVER_ONLY;
	}
	else if (owner == "Client")
	{
		return SQLITE_OWNER::CLIENT_ONLY;
	}
	else if (owner == "Both")
	{
		return SQLITE_OWNER::BOTH;
	}
	return SQLITE_OWNER::NONE;
}

constexpr int ROW_TABLE_NAME = 0;
constexpr int ROW_TABLE_OWNER = 1;
constexpr int ROW_COLUMN_NAME = 2;
constexpr int ROW_COLUMN_TYPE = 3;
constexpr int ROW_COLUMN_OWNER = 4;
constexpr int ROW_COLUMN_COMMENT = 5;
constexpr int ROW_COLUMN_LINK_TABLE = 6;
constexpr int HEADER_DATA_ROW = 7;

void sqliteToCSV(const string& destPath, const SQLiteInfo& sqliteInfo, Vector<Vector<string>>& dataList)
{
	string csv;
	const auto& memberList = sqliteInfo.mMemberList;
	const int colCount = memberList.size();
	for (int k = 0; k < HEADER_DATA_ROW; ++k)
	{
		// 表名
		if (k == ROW_TABLE_NAME)
		{
			appendCSVString(csv, sqliteInfo.mSQLiteName, 0 < colCount - 1);
			for (int j = 1; j < colCount; ++j)
			{
				appendCSVString(csv, "", j < colCount - 1);
			}
		}
		// 表所属
		else if (k == ROW_TABLE_OWNER)
		{
			appendCSVString(csv, getOwnerString(sqliteInfo.mOwner), 0 < colCount - 1);
			for (int j = 1; j < colCount; ++j)
			{
				appendCSVString(csv, "", j < colCount - 1);
			}
		}
		// 字段名字
		else if (k == ROW_COLUMN_NAME)
		{
			FOR_VECTOR(memberList)
			{
				appendCSVString(csv, memberList[i].mMemberName, i < memberList.size() - 1);
			}
		}
		// 字段类型
		else if (k == ROW_COLUMN_TYPE)
		{
			FOR_VECTOR(memberList)
			{
				appendCSVString(csv, memberList[i].mTypeName, i < memberList.size() - 1);
			}
		}
		// 字段所属
		else if (k == ROW_COLUMN_OWNER)
		{
			FOR_VECTOR(memberList)
			{
				appendCSVString(csv, getOwnerString(memberList[i].mOwner), i < memberList.size() - 1);
			}
		}
		// 字段注释
		else if (k == ROW_COLUMN_COMMENT)
		{
			FOR_VECTOR(memberList)
			{
				appendCSVString(csv, memberList[i].mComment, i < memberList.size() - 1);
			}
		}
		// 字段链接表格
		else if (k == ROW_COLUMN_LINK_TABLE)
		{
			FOR_VECTOR(memberList)
			{
				appendCSVString(csv, memberList[i].mLinkTable, i < memberList.size() - 1);
			}
		}
	}
	// 表数据
	FOR_VECTOR(dataList)
	{
		const auto& row = dataList[i];
		FOR_VECTOR_J(row)
		{
			appendCSVString(csv, row[j], j < row.size() - 1);
		}
	}
	FileUtility::writeFile(destPath, StringUtility::ANSIToUTF8(csv.c_str()));
}

int main()
{
	string dataBasePath = "./";
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
		const SQLiteInfo& info = sqliteInfoList[tableName];
		if (info.mClientSQLite)
		{
			cout << "保持SQLite文件:" << file << endl;
			continue;
		}
		Vector<Vector<string>> dataList;
		readSQLiteData(file, info, dataList);
		const string& destPath = StringUtility::replaceSuffix(file, ".csv");
		sqliteToCSV(destPath, info, dataList);
		cout << "生成文件:" << destPath << endl;
	}
	return 0;
}