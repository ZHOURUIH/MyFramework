#include "AllHeader.h"

UndoDeleteColumn::~UndoDeleteColumn()
{
	for (const Vector<GridData*>& list : mDatas)
	{
		for (GridData* data : list)
		{
			delete data;
		}
	}
	for (ColumnData* data : mHeaderDatas)
	{
		delete data;
	}
}

void UndoDeleteColumn::setData(const Vector<int>& cols, Vector<Vector<GridData*>>&& data, Vector<ColumnData*>&& headerDatas)
{
	cols.clone(mColumns);
	mDatas = move(data);
	mHeaderDatas = move(headerDatas);
}

void UndoDeleteColumn::undo()
{
	// 恢复数据,将列插入进去
	mMainListWindow->addColumn(mColumns, move(mDatas), mHeaderDatas);
	mHeaderDatas.clear();
}