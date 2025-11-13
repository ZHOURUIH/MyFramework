#ifndef _GAME_DEFINE_H_
#define _GAME_DEFINE_H_

#include "Utility.h"

// SQLite表格所属的端
enum class SQLITE_OWNER : byte
{
	NONE,				// 不属于客户端或者服务器,仅表格辅助作用
	BOTH,				// 客户端和服务器都会用到
	CLIENT_ONLY,		// 仅客户端用
	SERVER_ONLY,		// 仅服务器用
};

struct PacketMember
{
	string mTypeName;
	string mMemberName;
	string mMemberNameNoPrefix;
	string mComment;
	int mIndex = 0;
	bool mOptional = false;
};

// 表示消息中的结构体定义
struct PacketStruct
{
	myVector<PacketMember> mMemberList;			// 结构体成员
	string mStructName;							// 结构体名字
	string mComment;							// 结构体注释
};

// 消息中的所有信息定义
struct PacketInfo
{
	myVector<PacketMember> mMemberList;
	string mPacketName;							// 消息包名
	string mComment;							// 消息注释
	bool mShowInfo = false;						// 是否显示调试信息
	bool mUDP = false;							// 是否通过UDP传输
};

struct SQLiteMember
{
	SQLITE_OWNER mOwner = SQLITE_OWNER::NONE;
	string mTypeName;
	string mMemberName;
	string mComment;
	string mEnumRealType;
};

struct SQLiteInfo
{
	myVector<SQLiteMember> mMemberList;
	SQLITE_OWNER mOwner = SQLITE_OWNER::NONE;
	string mSQLiteName;
	string mComment;
	bool mClientSQLite = false;
};

struct MySQLMember
{
	string mTypeName;
	string mMemberName;
	string mComment;
	bool mUTF8 = false;
};

struct MySQLInfo
{
	myVector<MySQLMember> mMemberList;	// 不带ID字段的字段信息列表
	myVector<string> mIndexList;		// 索引列表
	string mMySQLClassName;
	string mMySQLTableName;
	string mComment;
	void init(const string& className, const string& tableName, const string& comment)
	{
		mMemberList.clear();
		mIndexList.clear();
		mMySQLClassName = className;
		mMySQLTableName = tableName;
		mComment = comment;
	}
};

#endif