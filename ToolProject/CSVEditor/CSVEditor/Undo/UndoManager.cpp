#include "AllHeader.h"

void UndoManager::destroy()
{
	clearUndo();
	clearRedo();
}

void UndoManager::addUndo(Undo* undoCollection)
{
	// 正在进行撤销操作,则加入的应该是重做操作
	if (mUndoing)
	{
		const bool beforeRedoEnable = canRedo();
		mRedoBuffer.push_back(undoCollection);
		redoChanged(beforeRedoEnable, canRedo());
	}
	// 没有正在撤销,则加入撤销操作
	else
	{
		const bool beforeUndoEnable = canUndo();
		// 如果加入的撤销操作已经大于最大数量了,则删除最先加入的撤销操作
		if (mUndoBuffer.size() >= EditorDefine::MAX_UNDO_COUNT)
		{
			delete *mUndoBuffer.begin();
			mUndoBuffer.erase(mUndoBuffer.begin());
		}
		mUndoBuffer.push_back(undoCollection);
		undoChanged(beforeUndoEnable, canUndo());

		// 如果不是在重做时添加撤销操作,则需要清空重做列表
		if (!mRedoing)
		{
			clearRedo();
		}
	}
}

void UndoManager::undo()
{
	mUndoing = true;
	if (canUndo())
	{
		// 执行了撤销操作后就删除该操作
		const bool beforeUndoEnable = canUndo();
		mUndoBuffer[mUndoBuffer.size() - 1]->undo();
		delete *(mUndoBuffer.begin() + mUndoBuffer.size() - 1);
		mUndoBuffer.erase(mUndoBuffer.begin() + mUndoBuffer.size() - 1);
		undoChanged(beforeUndoEnable, canUndo());
	}
	mUndoing = false;
}

void UndoManager::redo()
{
	mRedoing = true;
	if (canRedo())
	{
		// 执行了重做操作后就删除该操作
		const bool beforeRedoEnable = canRedo();
		mRedoBuffer[mRedoBuffer.size() - 1]->undo();
		delete *(mRedoBuffer.begin() + mRedoBuffer.size() - 1);
		mRedoBuffer.erase(mRedoBuffer.begin() + mRedoBuffer.size() - 1);
		redoChanged(beforeRedoEnable, canRedo());
	}
	mRedoing = false;
}

void UndoManager::clearRedo()
{
	bool beforeRedoEnable = canRedo();
	for (Undo* item : mRedoBuffer)
	{
		delete item;
	}
	mRedoBuffer.clear();
	redoChanged(beforeRedoEnable, canRedo());
}

void UndoManager::clearUndo()
{
	const bool beforeUndoEnable = canUndo();
	for (Undo* item : mUndoBuffer)
	{
		delete item;
	}
	mUndoBuffer.clear();
	undoChanged(beforeUndoEnable, canUndo());
}

void UndoManager::checkUndoRedoEnable()
{
	undoChanged(false, canUndo(), true);
	redoChanged(false, canRedo(), true);
}

void UndoManager::undoChanged(bool beforeEnable, bool nowEnable, bool force)
{
	if (beforeEnable != nowEnable || force)
	{
		CALL(mUndoChangeCallback);
	}
}

void UndoManager::redoChanged(bool beforeEnable, bool nowEnable, bool force)
{
	if (beforeEnable != nowEnable || force)
	{
		CALL(mUndoChangeCallback);
	}
}