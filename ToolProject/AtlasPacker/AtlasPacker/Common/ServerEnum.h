#ifndef _SERVER_ENUM_H_
#define _SERVER_ENUM_H_

// 服务器配置文件浮点数参数定义
enum SERVER_DEFINE_FLOAT
{
	SDF_HEART_BEAT_TIME_OUT,	// 心跳超时时间
	SDF_SOCKET_PORT,			// socket端口号
	SDF_BACK_LOG,				// 连接请求队列的最大长度
	SDF_SHOW_COMMAND_DEBUG_INFO,// 是否显示命令调试信息
	SDF_OUTPUT_NET_LOG,			// 是否显示网络日志信息
	SDF_MAX,
};
// 服务器配置文件字符串参数定义
enum SERVER_DEFINE_STRING
{
	SDS_DOMAIN_NAME,			// 连接的服务器域名
	SDS_MAX,
};

enum PARSE_RESULT
{
	PR_SUCCESS,
	PR_NOT_ENOUGH,
	PR_ERROR,
};

#endif