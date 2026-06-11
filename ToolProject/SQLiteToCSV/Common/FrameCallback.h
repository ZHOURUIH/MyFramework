#ifndef _FRAME_CALLBACK_H_
#define _FRAME_CALLBACK_H_

// 回调
class SQLiteDataReader;
typedef void (*SQLiteParseFunction)(void* ptr, SQLiteDataReader* reader, int index);
typedef void (*SQLiteInsertFunction)(char* str, int size, void* ptr, bool notLastOne);
typedef void (*SQLiteUpdateFunction)(char* str, int size, const char* col, void* ptr, bool notLastOne);

#endif