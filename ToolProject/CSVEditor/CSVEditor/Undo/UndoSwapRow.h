#pragma once

#include "Undo.h"

class UndoSwapRow : public Undo
{
public:
	void setData(int row0, int row1);
	virtual void undo();
protected:
	int mRow0 = -1;
	int mRow1 = -1;
};