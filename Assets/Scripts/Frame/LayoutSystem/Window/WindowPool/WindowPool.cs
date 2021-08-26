using System;
using System.Collections.Generic;
using UnityEngine;

// 窗口对象池,因为此处可以确定只有主工程的类,所以可以使用new T()
public class WindowPool<T> where T : myUIObject, new()
{
	protected Stack<T> mUnusedList;				// 未使用的窗口列表
	protected List<T> mInusedList;				// 正在使用的窗口列表,因为需要保证物体的存储顺序,所以使用List
	protected OnDestroyWindow mDestroyCallback;	// 窗口销毁的回调,用于让外部定义窗口的销毁方式
	protected LayoutScript mScript;				// 所属布局脚本
	protected T mTemplate;						// 窗口模板
	public WindowPool(LayoutScript script)
	{
		mScript = script;
		mInusedList = new List<T>();
		mUnusedList = new Stack<T>();
	}
	public void setTemplate(T template) { mTemplate = template; }
	public T newWindow(myUIObject parent, string name = null, bool notifyLayout = true)
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
		window.setAsLastSibling(false, notifyLayout);
		return window;
	}
	public void unuseAll()
	{
		foreach(var item in mInusedList)
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
		if (window == null)
		{
			return false;
		}
		if (!mInusedList.Contains(window))
		{
			return false;
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
		return true;
	}
	public List<T> getWindowList() { return mInusedList; }
	public void setDestroyCallback(OnDestroyWindow callback) { mDestroyCallback = callback; }
}