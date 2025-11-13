using System.Collections.Generic;
using static FrameBaseUtility;
using static MathUtility;
using static UnityUtility;

public class WindowPool<T> where T : myUGUIObject, new()
{
	protected List<T> mUnusedList = new();			// 未使用的窗口列表
	protected List<T> mInusedList = new();          // 正在使用的窗口列表,因为需要保证物体的存储顺序,所以使用List
	protected UGUIObjectCallback mDestroyCallback;	// 窗口销毁的回调,用于让外部定义窗口的销毁方式
	protected LayoutScript mScript;					// 所属布局脚本
	protected myUGUIObject mParent;					// 创建出的窗口的默认父节点
	protected T mTemplate;							// 窗口模板
	protected bool mAutoRefreshDepth;				// 在添加窗口后是否刷新窗口的深度,只在需要移动到最后一个子节点时才会生效
	protected bool mMoveToLast;                     // 新添加的窗口是否需要移动到父节点的最后一个子节点
	public WindowPool(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.getScript();
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
		}
	}
	public void assignTemplate(myUGUIObject parent, string name)
	{
		mScript.newObject(out mTemplate, parent, name);
	}
	public void assignTemplate(T template)
	{
		mTemplate = template;
	}
	public void init(bool autoRefreshDepth, bool moveToLast = true)
	{
		mParent = mTemplate.getParent();
		mAutoRefreshDepth = autoRefreshDepth;
		mMoveToLast = moveToLast;
		mTemplate.setActive(false);
	}
	public T newWindow(string name = null)
	{
		return newWindow(mParent, name);
	}
	// 新创建的窗口会自动移动到父节点的最后一个子节点的位置
	public T newWindow(myUGUIObject parent, string name = null)
	{
		name ??= mTemplate.getName();
		T window = null;
		// 从未使用列表中获取
		if (mUnusedList.Count > 0)
		{
			window = mUnusedList.popBack();
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
			mUnusedList.Add(item);
		}
		mInusedList.Clear();
	}
	public bool unuseWindow(T window)
	{
		if (window == null)
		{
			return false;
		}
		if (isEditor() && mUnusedList.Contains(window))
		{
			logError("重复回收窗口,name:" + window.getName());
			return false;
		}
		if (!mInusedList.Remove(window))
		{
			logError("要回收的窗口不属于当前对象池, name:" + window.getName());
			return false;
		}
		mUnusedList.Add(window);
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
	public void unuseIndex(int index)
	{
		unuseRange(index, 1);
	}
	// 回收一定下标范围的对象,count小于0表示回收从startIndex到结尾的所有对象
	public void unuseRange(int startIndex, int count = -1)
	{
		int usedCount = mInusedList.Count;
		if (count < 0)
		{
			count = usedCount - startIndex;
		}
		else
		{
			clampMax(ref count, usedCount - startIndex);
		}
		for (int i = 0; i < count; ++i)
		{
			mUnusedList.add(mInusedList[startIndex + i]).setActive(false);
		}
		mInusedList.RemoveRange(startIndex, count);
	}
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mInusedList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			newWindow();
		}
	}
	public List<T> getWindowList() { return mInusedList; }
	public void setDestroyCallback(UGUIObjectCallback callback) { mDestroyCallback = callback; }
}