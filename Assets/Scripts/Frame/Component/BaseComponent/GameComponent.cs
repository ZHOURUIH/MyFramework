using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class GameComponent : GameBase
{
	protected ComponentOwner mComponentOwner;	// 该组件的拥有者
	protected bool mIgnoreTimeScale;			// 更新时是否忽略时间缩放
	protected bool mDefaultActive;				// 默认的启用状态
	protected bool mActive;                     // 是否激活组件
	public GameComponent()
	{
		mActive = true;
		mIgnoreTimeScale = false;
		mDefaultActive = false;
	}
	public virtual void init(ComponentOwner owner) { mComponentOwner = owner; }
	public virtual void update(float elapsedTime){}
	public virtual void fixedUpdate(float elapsedTime){}
	public virtual void lateUpdate(float elapsedTime){}
	public virtual void destroy()
	{
		mComponentOwner?.notifyComponentDestroied(this);
	}
	// 拷贝当前组件的所有属性到目标组件中,返回值表示当前组件是否已经链接了预设
	public bool isActive() { return mActive; }
	public virtual void resetProperty() { }
	public virtual void setActive(bool active) { mActive = active; }
	public void setDefaultActive(bool active) { mDefaultActive = active; }
	public virtual void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	// 获得成员变量
	public ComponentOwner getOwner() { return mComponentOwner; }
	public bool isComponentActive() { return mActive; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public bool isDefaultActive() { return mDefaultActive; }
	// 通知
	public virtual void notifyOwnerActive(bool active) { }
	public virtual void notifyOwnerDestroy() { }
	//------------------------------------------------------------------------------------------------------------------------
}
