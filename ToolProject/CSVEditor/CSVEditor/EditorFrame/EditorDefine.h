#pragma once

#include "FrameHeader.h"

class EditorDefine
{
public:
	static constexpr int ROW_TABLE_NAME = 0;
	static constexpr int ROW_TABLE_OWNER = 1;
	static constexpr int ROW_COLUMN_NAME = 2;
	static constexpr int ROW_COLUMN_TYPE = 3;
	static constexpr int ROW_COLUMN_OWNER = 4;
	static constexpr int ROW_COLUMN_COMMENT = 5;
	static constexpr int ROW_COLUMN_LINK_TABLE = 6;
	static constexpr int ROW_COLUMN_LINK_LENGTH = 7;
	static constexpr int ROW_COLUMN_FLAG = 8;
	static constexpr int ROW_COLUMN_FILTER = 9;
	static constexpr int HEADER_ROW = 10;
	static constexpr int HEADER_DATA_ROW = 9;
	static constexpr int MAX_UNDO_COUNT = 100;
};