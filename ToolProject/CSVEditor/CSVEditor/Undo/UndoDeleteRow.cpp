#include "AllHeader.h"

UndoDeleteRow::~UndoDeleteRow()
{
	for (auto& dataList : mDatas)
	{
		for (GridData* data : dataList)
		{
			delete data;
		}
	}
}

void UndoDeleteRow::setData(const Vector<int>& row, Vector<Vector<GridData*>>&& data)
{
	mRows = row;
	mDatas = move(data);
}

void UndoDeleteRow::undo()
{
	// 恢复数据,将行插入进去
	mMainListWindow->addRow(mRows, mDatas);
	mDatas.clear();
}