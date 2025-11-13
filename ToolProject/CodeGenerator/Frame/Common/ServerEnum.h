#ifndef _SERVER_ENUM_H_
#define _SERVER_ENUM_H_

// 服务器配置文件浮点数参数定义
enum class FRAME_DEFINE_FLOAT : byte
{
	HEART_BEAT_TIME_OUT,	// 心跳超时时间
	SOCKET_PORT,			// socket端口号
	BACK_LOG,				// 连接请求队列的最大长度
	SHOW_COMMAND_DEBUG_INFO,// 是否显示命令调试信息
	OUTPUT_NET_LOG,			// 是否显示网络日志信息
	MAX,
};
// 服务器配置文件字符串参数定义
enum class FRAME_DEFINE_STRING : byte
{
	DOMAIN_NAME,			// 连接的服务器域名
	MAX,
};

enum class PARSE_RESULT : byte
{
	SUCCESS,
	NOT_ENOUGH,
	FAILED,
};

// 播放状态
enum class PLAY_STATE : byte
{
	PLAY_NONE,
	PLAY,
	PAUSE,
	STOP,
};

enum class EXECUTE_STATE : byte
{
	NOT_EXECUTE,
	EXECUTING,
	EXECUTED,
};

// SQLite数据类型
enum class SQLITE_DATATYPE : byte
{
	SQLITE_DATATYPE_INTEGER = SQLITE_INTEGER,
	SQLITE_DATATYPE_FLOAT = SQLITE_FLOAT,
	SQLITE_DATATYPE_TEXT = SQLITE_TEXT,
	SQLITE_DATATYPE_BLOB = SQLITE_BLOB,
	SQLITE_DATATYPE_NULL = SQLITE_NULL,
};

#endif