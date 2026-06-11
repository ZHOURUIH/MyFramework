#include "AllHeader.h"

void UndoAddColumn::setData(const Vector<int>& cols)
{
	cols.clone(mColumns);
}

void UndoAddColumn::undo()
{
	// É¾³ýÁÐ
	mMainListWindow->deleteColumn(mColumns);
}