#pragma once

#include "Undo.h"

class UndoSwapColumn : public Undo
{
public:
	void setData(int srcCol, int destCol);
	virtual void undo();
protected:
	int mSrcColumn = -1;
	int mDestColumn = -1;
};