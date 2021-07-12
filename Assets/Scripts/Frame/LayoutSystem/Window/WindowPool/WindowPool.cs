using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnDestroyWindow(myUIObject window);

// 因为此处可以确定只有主工程的类,所以可以使用new T()
public class WindowPool<T> where T : myUIObject, new()
{
	protected List<T> mInusedList;
	protected Stack<T> mUnusedList;
	protected T mTemplate;
	protected LayoutScript mScript;
	protected OnDestroyWindow mDestroyCallback;
	public WindowPool(LayoutScript script)
	{
		mScript = script;
		mInusedList = new List<T>();
		mUnusedList = new Stack<T>();
	}
	public void setTemplate(T template) { mTemplate = template; }
	public T newWindow(myUIObject parent, string name = null, bool notifyLayout = true, bool sortChild = true)
	{
		if (name == null)
		{
			name = mTemplate.getName();
		}
		T window = null;
		// 从未使用列表中获取
		if (mUnusedList.Count > 0)
		{
			window = mUnusedList.Pop();
		}
		// 未使用列表中没有就创建新窗口
		if (window == null)
		{
			if (mTemplate != null)
			{
				window = mScript.cloneObject(parent, mTemplate, name, true);
			}
			else
			{
				window = mScript.createObject<T>(name, true);
			}
		}
		mInusedList.Add(window);
		window.setActive(true);
		window.setName(name);
		window.setParent(parent, false, false);
		window.setAsLastSibling(sortChild, notifyLayout);
		return window;
	}
	public void unuseAll()
	{
		int count = mInusedList.Count;
		for(int i = 0; i < count; ++i)
		{
			T item = mInusedList[i];
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
	public void unuseWindow(T window)
	{
		if (window == null)
		{
			return;
		}
		if(!mInusedList.Contains(window))
		{
			return;
		}
		mInusedList.Remove(window);
		mUnusedList.Push(window);
		if (mDestroyCallback != null)
		{
			mDestroyCallback(window);
		}
		else
		{
			window.setActive(false);
		}
	}
	public List<T> getWindowList() { return mInusedList; }
	public void setDestroyCallback(OnDestroyWindow callback) { mDestroyCallback = callback; }
}