using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;

public class WindowObjectBase : ILocalizationCollection
{
	protected HashSet<myUIObject> mLocalizationObjectList;  // 需要本地化的文本对象
	protected LayoutScript mScript;							// 所属的布局脚本
	protected bool mDestroied;								// 是否已经销毁过了,用于检测重复销毁的
	public WindowObjectBase(LayoutScript script)
	{
		mScript = script;
	}
	public virtual void assignWindow(myUIObject parent, myUIObject template, string name) { }
	// 需要主动调用
	public virtual void init() { mDestroied = false; }
	// 需要主动调用
	public virtual void reset() { }
	public virtual void destroy()
	{
		if (mDestroied)
		{
			logWarning("WindowObject重复销毁对象:" + GetType());
		}
		mDestroied = true;

		mLocalizationManager?.unregisteLocalization(mLocalizationObjectList);
		mLocalizationObjectList?.Clear();
	}
	public void addLocalizationObject(myUIObject obj)
	{
		mLocalizationObjectList ??= new();
		mLocalizationObjectList.Add(obj);
	}
	public virtual bool isActive() { return false; }
	public virtual void setActive(bool active) { }
	public virtual void setParent(myUIObject parent, bool refreshDepth = true) { }
	public virtual void setAsLastSibling(bool refreshDepth = true) { }
	public virtual void setAsFirstSibling(bool refreshDepth = true) { }
	//------------------------------------------------------------------------------------------------------------------------------
	protected T0 newObject<T0>(out T0 obj, myUIObject parent, string name, bool showError) where T0 : myUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, -1, showError);
	}
	protected T0 newObject<T0>(out T0 obj, myUIObject parent, string name) where T0 : myUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, -1, true);
	}
	protected T0 newObject<T0>(out T0 obj, myUIObject parent, string name, int active) where T0 : myUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, active, true);
	}
	protected T newObject<T>(out T obj, myUIObject parent, string name, int active, bool showError) where T : myUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, active, showError);
	}
}