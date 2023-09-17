using System;

// 组件基类,只是使用了MonoBehaviour的组件思想
public abstract class GameComponent : ClassObject
{
	protected ComponentOwner mComponentOwner;   // 该组件的拥有者
	protected Type mType;						// 组件类型,因为有些组件可能在热更工厂中,无法通过GetType获取正确的类型,所以保存一个
	protected bool mIgnoreTimeScale;			// 更新时是否忽略时间缩放
	protected bool mDefaultActive;				// 默认的启用状态
	protected bool mActive;                     // 是否激活组件
	public GameComponent()
	{
		mActive = true;
	}
	public virtual void init(ComponentOwner owner) { mComponentOwner = owner; }
	public virtual void update(float elapsedTime){}
	public virtual void fixedUpdate(float elapsedTime){}
	public virtual void lateUpdate(float elapsedTime){}
	public virtual void destroy()
	{
		mComponentOwner = null;
	}
	public bool isActive() { return mActive; }
	public override void resetProperty() 
	{
		base.resetProperty();
		mComponentOwner = null;
		mType = null;
		mIgnoreTimeScale = false;
		mDefaultActive = false;
		mActive = true;
	}
	public virtual void setActive(bool active) 
	{
		mActive = active;
		if (mActive)
		{
			mComponentOwner.notifyComponentStart(this);
		}
	}
	public void setDefaultActive(bool active) { mDefaultActive = active; }
	public void setType(Type type) { mType = type; }
	public virtual void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	// 获得成员变量
	public ComponentOwner getOwner() { return mComponentOwner; }
	public bool isComponentActive() { return mActive; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public bool isDefaultActive() { return mDefaultActive; }
	public Type getType() { return mType; }
	// 通知
	public virtual void notifyOwnerActive(bool active) { }
}