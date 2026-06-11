#pragma once

#include "Undo.h"

class GridData;
class ColumnData;
class UndoDeleteRow : public Undo
{
public:
	~UndoDeleteRow();
	void setData(const Vector<int>& row, Vector<Vector<GridData*>>&& data);
	virtual void undo();
protected:
	Vector<int> mRows;
	Vector<Vector<GridData*>> mDatas;
};