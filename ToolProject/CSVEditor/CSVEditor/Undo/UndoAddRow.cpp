#include "AllHeader.h"

void UndoAddRow::setData(const Vector<int>& rows)
{
	mRows = rows;
}

void UndoAddRow::undo()
{
	// É¾³ýÐÐ
	mMainListWindow->deleteRow(mRows);
}