#pragma once

#include "FrameMacro.h"

// 只开放部分std的内容,避免不必要的命名冲突
using std::vector;
using std::map;
using std::stack;
using std::queue;
using std::set;
using std::endl;
using std::string;
using std::wstring;
using std::atomic_flag;
using std::make_pair;
using std::exception;
using std::move;
using std::forward;
using std::is_same;
using std::conjunction;
using std::decay;
using std::function;
using std::atomic;
using std::enable_if_t;
using std::tuple;
using std::remove_cv;
using std::remove_reference;
using std::initializer_list;
using std::array;
using std::is_enum;
using std::to_string;
using std::pair;
using std::unordered_map;

//--------------------------------------------------------------------------------------------------------------------------------------------------------
// 基础数据类型简化定义
typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;
typedef unsigned long long ullong;
typedef long long llong;