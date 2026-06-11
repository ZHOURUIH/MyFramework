#pragma once

#include "Undo.h"

class UndoSetCellData : public Undo
{
public:
	void setData(const HashMap<Vector2Int, string>& data, bool isHeader);
	virtual void undo();
protected:
	HashMap<Vector2Int, string> mData;
	bool mIsHeader = false;
};