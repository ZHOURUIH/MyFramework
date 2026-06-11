#pragma once

#include "Undo.h"

class GridData;
class ColumnData;
class UndoDeleteColumn : public Undo
{
public:
	~UndoDeleteColumn();
	void setData(const Vector<int>& cols, Vector<Vector<GridData*>>&& data, Vector<ColumnData*>&& headerDatas);
	virtual void undo();
protected:
	Vector<int> mColumns;
	Vector<Vector<GridData*>> mDatas;
	Vector<ColumnData*> mHeaderDatas;
};