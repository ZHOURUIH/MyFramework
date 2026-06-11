#pragma once

#include "Undo.h"

class UndoAddRow : public Undo
{
public:
	void setData(const Vector<int>& rows);
	virtual void undo();
protected:
	Vector<int> mRows;
};