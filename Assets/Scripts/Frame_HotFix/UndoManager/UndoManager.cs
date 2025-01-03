using System;
using System.Collections.Generic;
using static FrameUtility;

// 撤销重做管理
public class UndoManager : FrameSystem
{
	protected List<Undo> mUndoList = new();		// 撤销操作列表
	protected List<Undo> mRedoList = new();		// 重做操作列表
	protected Action mUndoRedoChangeCallback;	// 撤销或者重做有改变
	protected int mMaxUndo = 10;				// 最多可存储的撤销操作数量
	protected bool mUndoing;					// 当前是否正在执行撤销操作
	protected bool mRedoing;                    // 当前是否正在执行撤销操作
	public override void resetProperty()
	{
		base.resetProperty();
		mUndoList.Clear();
		mRedoList.Clear();
		mUndoRedoChangeCallback = null;
		mMaxUndo = 10;
		mUndoing = false;
		mRedoing = false;
	}
	public void clearAll()
	{
		bool lastCanUndo = canUndo();
		bool lastCanRedo = canRedo();
		UN_CLASS_LIST(mUndoList);
		clearRedo();
		mUndoing = false;
		mRedoing = false;
		bool curCanUndo = canUndo();
		bool curCanRedo = canRedo();
		if (curCanUndo != lastCanUndo || curCanRedo != lastCanRedo)
		{
			mUndoRedoChangeCallback?.Invoke();
		}
	}
	public void clearRedo()
	{
		UN_CLASS_LIST(mRedoList);
	}
	public bool canUndo() { return mUndoList.Count > 0; }
	public bool canRedo() { return mRedoList.Count > 0; }
	public void setMaxUndoCount(int count) { mMaxUndo = count; }
	public int getMaxUndoCount() { return mMaxUndo; }
	public void addUndoRedoChangeCallback(Action callback) { mUndoRedoChangeCallback += callback; }
	public void removeUndoRedoChangeCallback(Action callback) { mUndoRedoChangeCallback -= callback; }
	public void addUndo(Undo undo)
	{
		bool lastCanUndo = canUndo();
		bool lastCanRedo = canRedo();
		// 正在进行撤销操作,则加入的应该是重做操作
		if (mUndoing)
		{
			mRedoList.Add(undo);
		}
		// 没有正在撤销,则加入撤销操作
		else
		{
			// 如果加入的撤销操作已经大于最大数量了,则删除最先加入的撤销操作
			if (mUndoList.Count >= mMaxUndo)
			{
				UN_CLASS(mUndoList.removeAt(0));
			}
			mUndoList.Add(undo);

			// 如果不是在重做时添加撤销操作,则需要清空重做列表
			if (!mRedoing)
			{
				clearRedo();
			}
		}
		if (canUndo() != lastCanUndo || canRedo() != lastCanRedo)
		{
			mUndoRedoChangeCallback?.Invoke();
		}
	}
	public void undo()
	{
		bool lastCanUndo = canUndo();
		mUndoing = true;
		if (canUndo())
		{
			mUndoList[^1].undo();
			UN_CLASS(mUndoList.popBack());
		}
		mUndoing = false;
		if (canUndo() != lastCanUndo)
		{
			mUndoRedoChangeCallback?.Invoke();
		}
	}
	public void redo()
	{
		bool lastCanRedo = canRedo();
		mRedoing = true;
		if (canRedo())
		{
			// 执行了重做操作后就删除该操作
			mRedoList[^1].undo();
			UN_CLASS(mRedoList.popBack());
		}
		mRedoing = false;
		if (canRedo() != lastCanRedo)
		{
			mUndoRedoChangeCallback?.Invoke();
		}
	}
}