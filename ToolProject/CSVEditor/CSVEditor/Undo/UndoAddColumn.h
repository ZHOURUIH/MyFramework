#pragma once

#include "Undo.h"

class UndoAddColumn : public Undo
{
public:
	void setData(const Vector<int>& cols);
	virtual void undo();
protected:
	Vector<int> mColumns;
};