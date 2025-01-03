#ifndef _SERVER_CALLBACK_H_
#define _SERVER_CALLBACK_H_

class SQLiteDataReader;

// 线程回调
typedef bool(*CustomThreadCallback)(void* args);
typedef void (*SQLiteParseFunction)(void* ptr, SQLiteDataReader* reader, int index);
typedef void (*SQLiteInsertFunction)(char* str, int size, void* ptr, bool notLastOne);
typedef void (*SQLiteUpdateFunction)(char* str, int size, const char* col, void* ptr, bool notLastOne);

#endif