﻿using System.Collections.Generic;

public class WindowPool<T> where T : myUIObject, new()
{
	protected Stack<T> mUnusedList = new();         // 未使用的窗口列表
	protected List<T> mInusedList = new();          // 正在使用的窗口列表,因为需要保证物体的存储顺序,所以使用List
	protected UIObjectCallback mDestroyCallback;	// 窗口销毁的回调,用于让外部定义窗口的销毁方式
	protected LayoutScript mScript;					// 所属布局脚本
	protected myUIObject mParent;					// 创建出的窗口的默认父节点
	protected T mTemplate;							// 窗口模板
	protected bool mAutoRefreshDepth;				// 在添加窗口后是否刷新窗口的深度,只在需要移动到最后一个子节点时才会生效
	protected bool mMoveToLast;						// 新添加的窗口是否需要移动到父节点的最后一个子节点
	public WindowPool(LayoutScript script)
	{
		mScript = script;
	}
	public void init(myUIObject parent, T template, bool autoRefreshDepth, bool moveToLast = true)
	{
		mParent = parent;
		mTemplate = template;
		mAutoRefreshDepth = autoRefreshDepth;
		mMoveToLast = moveToLast;
	}
	public T newWindow(string name = null)
	{
		return newWindow(mParent, name);
	}
	// 新创建的窗口会自动移动到父节点的最后一个子节点的位置
	public T newWindow(myUIObject parent, string name = null)
	{
		name ??= mTemplate.getName();
		T window = null;
		// 从未使用列表中获取
		if (mUnusedList.Count > 0)
		{
			window = mUnusedList.Pop();
			window.setParent(parent, false);
		}
		// 未使用列表中没有就创建新窗口
		if (window == null)
		{
			mScript.cloneObject(out window, parent, mTemplate, name, true);
		}
		mInusedList.Add(window);
		window.setActive(true);
		window.setName(name);
		if (mMoveToLast)
		{
			window.setAsLastSibling(mAutoRefreshDepth);
		}
		return window;
	}
	public void unuseAll()
	{
		foreach (T item in mInusedList)
		{
			if (mDestroyCallback != null)
			{
				mDestroyCallback(item);
			}
			else
			{
				item.setActive(false);
			}
			mUnusedList.Push(item);
		}
		mInusedList.Clear();
	}
	public bool unuseWindow(T window)
	{
		if (window == null || !mInusedList.Remove(window))
		{
			return false;
		}
		mUnusedList.Push(window);
		if (mDestroyCallback != null)
		{
			mDestroyCallback(window);
		}
		else
		{
			window.setActive(false);
		}
		return true;
	}
	public List<T> getWindowList() { return mInusedList; }
	public void setDestroyCallback(UIObjectCallback callback) { mDestroyCallback = callback; }
}