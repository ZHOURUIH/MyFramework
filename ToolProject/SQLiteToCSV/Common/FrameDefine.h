#ifndef _FRAME_DEFINE_H_
#define _FRAME_DEFINE_H_

#include "FrameMacro.h"

// 只开放部分std的内容,避免不必要的命名冲突
using std::vector;
using std::map;
using std::set;
using std::cout;
using std::cin;
using std::endl;
using std::string;
using std::wstring;
using std::atomic_flag;
using std::make_pair;
using std::exception;
using std::move;
using std::is_same;
using std::decay;
using std::function;

//--------------------------------------------------------------------------------------------------------------------------------------------------------
// 基础数据类型简化定义
typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;
typedef unsigned long long ullong;
typedef long long llong;

// 自定义的基础头文件,因为这些头文件中可能会用到上面的宏,所以放在下面
#include "FrameEnum.h"
#include "Array.h"
#include "Vector.h"
#include "Map.h"
#include "Set.h"
#include "Stack.h"
#include "Queue.h"
#include "SafeVector.h"
#include "SafeMap.h"
#include "SafeSet.h"
#include "FrameCallback.h"

//------------------------------------------------------------------------------------------------------------------------------------------------------
class FrameDefine
{
public:
	// 常量字符串定义
	static const string MEDIA_PATH;
	static const string MAP_PATH;
	static const string CONFIG_PATH;
	static const string LOG_PATH;
	static const string EMPTY;
	static constexpr char* MYSQL_ID_COL = (char*)"ID";	// MySQL数据库每个表格的ID列的列名
	static constexpr char* SQLITE_ID_COL = (char*)"ID";	// SQLite数据库每个表格的ID列的列名
	static constexpr size_t NOT_FIND = (size_t)-1;
};

#endif