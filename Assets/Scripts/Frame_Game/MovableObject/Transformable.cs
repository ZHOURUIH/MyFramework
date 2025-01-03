using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 可变换的物体,2D和3D物体都是可变换,也就是都会包含一个Transform
public class Transformable : ComponentOwner
{
	protected Transform mTransform;				// 变换组件
	protected GameObject mObject;				// 物体节点
	protected bool mNeedUpdate;					// 是否启用更新,与Active共同控制是否执行更新
	protected Action mPositionModifyCallback;
	protected Action mRotationModifyCallback;
	protected Action mScaleModifyCallback;
	protected bool mPositionDirty = true;
	protected Vector3 mPosition;				// 单独存储位置,可以在大多数时候避免访问Transform
	public Transformable()
	{
		mNeedUpdate = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mTransform = null;
		mObject = null;
		mNeedUpdate = true;
		mPositionModifyCallback = null;
		mRotationModifyCallback = null;
		mScaleModifyCallback = null;
		mPositionDirty = true;
		mPosition = Vector3.zero;
	}
	public virtual void setObject(GameObject obj)
	{
		mObject = obj;
		if (mObject != null)
		{
			mTransform = mObject.transform;
			if (mObject.name != mName)
			{
				mObject.name = mName;
			}
		}
		else
		{
			mTransform = null;
		}
	}
	public override void setActive(bool active)
	{
		if (mObject != null)
		{
			if (active == mObject.activeSelf)
			{
				return;
			}
			mObject.SetActive(active);
		}
		base.setActive(active);
	}
	// 返回第一个碰撞体,仅在当前节点中寻找,允许子类重写此函数,碰撞体可能不在当前节点,或者也不在此节点的子节点中
	public virtual Collider getCollider(bool addIfNotExist = false) 
	{
		if (addIfNotExist)
		{
			var collider = tryGetUnityComponent<Collider>();
			// 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
			if (collider == null)
			{
				collider = getOrAddUnityComponent<BoxCollider>();
			}
			return collider;
		}
		else
		{
			return tryGetUnityComponent<Collider>();
		}
	}
	public override void setName(string name)
	{
		base.setName(name);
		if (mObject != null && mObject.name != name)
		{
			mObject.name = name;
		}
	}
	public virtual bool raycastSelf(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		Collider collider = getCollider();
		if (collider == null)
		{
			hit = new();
			return false;
		}
		return collider.Raycast(ray, out hit, maxDistance);
	}
	public virtual GameObject getObject() { return mObject; }
	public void enableUnityComponent<T>(bool enable) where T : Behaviour
	{
		T com = tryGetUnityComponent<T>();
		if (com != null)
		{
			com.enabled = enable;
		}
	}
	public T tryGetUnityComponent<T>() where T : Component
	{
		mObject.TryGetComponent(out T com);
		return com;
	}
	public void tryGetUnityComponent<T>(out T com) where T : Component
	{
		mObject.TryGetComponent(out com);
	}
	public T getOrAddUnityComponent<T>() where T : Component
	{
		if (!mObject.TryGetComponent(out T com))
		{
			com = mObject.AddComponent<T>();
		}
		return com;
	}
	// 从当前节点以及所有子节点中查找指定组件
	public void getUnityComponentsInChild<T>(bool includeInactive, List<T> list) where T : Component
	{
		mObject.GetComponentsInChildren(includeInactive, list);
	}
	// 从当前以及所有子节点中查找指定组件
	public T getUnityComponentInChild<T>(bool includeInactive = true) where T : Component
	{
		return mObject.GetComponentInChildren<T>(includeInactive);
	}
	public Transform getTransform() { return mTransform; }
	public Vector3 getForward(bool ignoreY = false) { return ignoreY ? normalize(resetY(mTransform.forward)) : mTransform.forward; }
	public virtual bool isActive() { return mObject != null && mObject.activeSelf; }
	public virtual bool isActiveInHierarchy() { return mObject != null && mObject.activeInHierarchy; }
	public virtual bool isNeedUpdate() { return mNeedUpdate; }
	public virtual void setNeedUpdate(bool enable) { mNeedUpdate = enable; }
	public Vector3 getPosition() 
	{
		if (mPositionDirty)
		{
			mPositionDirty = false;
			mPosition = mTransform.localPosition;
		}
		return mPosition;
	}
	public Vector3 getRotation()
	{
		Vector3 vector3 = mTransform.localEulerAngles;
		adjustAngle180(ref vector3.z);
		return vector3;
	}
	public virtual void cloneFrom(myUIObject obj)
	{
		if (obj.GetType() != GetType())
		{
			logError("ui type is different, can not clone!");
			return;
		}
		setPosition(obj.getPosition());
		setRotation(obj.getRotationQuaternion());
		setScale(obj.getScale());
	}
	public int getChildCount() { return mTransform != null ? mTransform.childCount : 0; }
	public Vector3 getScale() { return mTransform != null ? mTransform.localScale : Vector3.zero; }
	public Vector3 getWorldPosition() { return mTransform != null ? mTransform.position : Vector3.zero; }
	public Vector3 getWorldScale()  { return mTransform != null ? mTransform.lossyScale : Vector3.zero; }
	public Quaternion getRotationQuaternion()  { return mTransform != null ? mTransform.localRotation : Quaternion.identity; }
	public void setPosition(Vector3 pos) 
	{
		if (mTransform == null || isVectorEqual(mTransform.localPosition, pos))
		{
			return;
		}
		mPositionDirty = true;
		mTransform.localPosition = pos;
		mPositionModifyCallback?.Invoke();
	}
	public void setScale(Vector3 scale)
	{
		if (mTransform == null || isVectorEqual(mTransform.localScale, scale))
		{
			return;
		}
		mTransform.localScale = scale;
		mScaleModifyCallback?.Invoke();
	}
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public void setRotation(Vector3 rot) 
	{
		if (mTransform == null || isVectorEqual(mTransform.localEulerAngles, rot))
		{
			return;
		}
		mTransform.localEulerAngles = rot;
		mRotationModifyCallback?.Invoke();
	}
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public void setRotation(Quaternion rot)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.localRotation = rot;
		mRotationModifyCallback?.Invoke();
	}
	public void setWorldPosition(Vector3 pos) 
	{
		if (mTransform == null)
		{
			return;
		}
		mPositionDirty = true;
		mTransform.position = pos;
		mPositionModifyCallback?.Invoke();
	}
	public void setPositionX(float x) { setPosition(replaceX(getPosition(), x)); }
	public void setPositionY(float y) { setPosition(replaceY(getPosition(), y)); }
	public void copyObjectTransform(GameObject obj)
	{
		Transform objTrans = obj.transform;
		FT.MOVE(this, objTrans.localPosition);
		FT.ROTATE(this, objTrans.localEulerAngles);
		FT.SCALE(this, objTrans.localScale);
		mPositionDirty = true;
	}
	public virtual bool isChildOf(IMouseEventCollect parent)
	{
		if (mTransform == null)
		{
			return false;
		}
		return parent is Transformable obj && mTransform.IsChildOf(obj.getTransform());
	}
	public virtual void setAlpha(float alpha)
	{
		using var a = new ListScope<Renderer>(out var renderers);
		getUnityComponentsInChild(true, renderers);
		foreach (Renderer renderer in renderers)
		{
			Material material = renderer.material;
			Color color = material.color;
			color.a = alpha;
			material.color = color;
		}
	}
	public virtual float getAlpha()
	{
		var renderer = tryGetUnityComponent<Renderer>();
		if (renderer == null)
		{
			return 1.0f;
		}
		return renderer.material.color.a;
	}
	public virtual bool canUpdate() { return mNeedUpdate && mObject.activeInHierarchy; }
}