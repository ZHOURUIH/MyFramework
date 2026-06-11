#include "AllHeader.h"

void UndoSwapColumn::setData(int srcCol, int destCol)
{
	mSrcColumn = srcCol;
	mDestColumn = destCol;
}

void UndoSwapColumn::undo()
{
	mMainListWindow->swapColumn(mDestColumn, mSrcColumn);
}