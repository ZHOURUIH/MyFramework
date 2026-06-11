#pragma once

#include "FrameDefine.h"

class Undo;
class UndoManager
{
public:
	UndoManager() = default;
	virtual ~UndoManager(){ destroy(); }
	void destroy();
	void undo();
	void redo();
	bool canUndo() const {return mUndoBuffer.size() > 0;}
	bool canRedo() const {return mRedoBuffer.size() > 0;}
	void setUndoChangeCallback(Action callback) { mUndoChangeCallback = callback; }
	void clearRedo();
	void clearUndo();
	void checkUndoRedoEnable();
	template<typename T, typename TypeCheck = IsSubClassOf<Undo, T>::mType>
	T* addUndo() 
	{
		T* undo = new T();
		addUndo(undo);
		return undo;
	}
protected:
	void addUndo(Undo* undoCollection);
	// beforeEnable是改变之前的可撤销状态,nowEnable是改变之后的可撤销状态,force是无论状态有没有改变都强制发送事件
	void undoChanged(bool beforeEnable, bool nowEnable, bool force = false);
	void redoChanged(bool beforeEnable, bool nowEnable, bool force = false);
protected:
	Vector<Undo*> mUndoBuffer;
	Vector<Undo*> mRedoBuffer;
	bool mUndoing = false;	// 是否正在撤销,如果撤销,则添加撤销操作时实际是添加重做操作
	bool mRedoing = false;	// 是否正在重做,如果不是重做,则在添加撤销操作时需要将重做列表清空
	Action mUndoChangeCallback;
};