using System;
using UnityEngine;

public abstract class Transformable : ComponentOwner
{
	protected bool mEnable;				// 是否启用更新,与Active共同控制是否执行更新
	public Transformable()
	{
		mEnable = true;
	}
	public abstract bool isActive();
	public virtual bool isEnable() { return mEnable; }
	public virtual void setEnable(bool enable) { mEnable = enable; }
	public abstract Vector3 getPosition();
	public abstract Vector3 getRotation();
	public abstract Vector3 getScale();
	public abstract Vector3 getWorldPosition();
	public abstract Vector3 getWorldRotation();
	public abstract Vector3 getWorldScale();
	public abstract void setPosition(Vector3 pos);
	public abstract void setRotation(Vector3 rot);
	public abstract void setScale(Vector3 scale);
	public abstract void setWorldPosition(Vector3 pos);
	public abstract void setWorldRotation(Vector3 rot);
	public abstract void setWorldScale(Vector3 scale);
	public abstract Vector3 localToWorld(Vector3 point);
	public abstract Vector3 worldToLocal(Vector3 point);
	public abstract Vector3 localToWorldDirection(Vector3 direction);
	public abstract Vector3 worldToLocalDirection(Vector3 direction);
}