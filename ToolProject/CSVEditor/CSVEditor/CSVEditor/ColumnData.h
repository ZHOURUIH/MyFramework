#pragma once

#include "FrameHeader.h"
#include "EditorEnum.h"

class ColumnData
{
public:
	string mName;					// 字段名
	string mType;					// 字段的数据类型
	OWNER mOwner = OWNER::NONE;		// 字段的所属
	string mComment;				// 字段的注释
	string mLinkTable;				// 字段所索引到的表格名
	string mLinkLength;				// 字段长度关联的字段名,相同关联字段名的字段需要列表长度一致,否则校验不通过
	string mFlag;					// 字段的标签,用一系列标签来标记字段的属性,标签暂时只有path
};