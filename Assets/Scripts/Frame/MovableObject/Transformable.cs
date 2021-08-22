using System;
using UnityEngine;

public class Transformable : ComponentOwner
{
	protected Transform mTransform;     // 变换组件
	protected GameObject mObject;       // 物体节点
	protected bool mEnable;             // 是否启用更新,与Active共同控制是否执行更新
	public Transformable()
	{
		mEnable = true;
	}
	public override void destroy()
	{
		base.destroy();
		mTransform = null;
		mObject = null;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mEnable = true;
		mObject = null;
		mTransform = null;
	}
	public virtual void setObject(GameObject obj)
	{
		mObject = obj;
		mTransform = mObject?.transform;
		if (mObject != null && mObject.name != mName)
		{
			mObject.name = mName;
		}
	}
	public override void setActive(bool active)
	{
		if(mObject != null)
		{
			if (active == mObject.activeSelf)
			{
				return;
			}
			mObject.SetActive(active);
		}
		base.setActive(active);
	}
	public void resetActive()
	{
		// 重新禁用再启用,可以重置状态
		mObject.SetActive(false);
		mObject.SetActive(true);
	}
	public void enableAllColliders(bool enable)
	{
		var colliders = mObject.GetComponents<Collider>();
		int count = colliders.Length;
		for (int i = 0; i < count; ++i)
		{
			colliders[i].enabled = enable;
		}
	}
	// 返回第一个碰撞体,当前节点找不到,则会在子节点中寻找
	public Collider getColliderInChild() { return getUnityComponentInChild<Collider>(true); }
	// 返回第一个碰撞体,仅在当前节点中寻找
	public Collider getCollider() { return getUnityComponent<Collider>(); }
	public override void setName(string name)
	{
		base.setName(name);
		if (mObject != null && mObject.name != name)
		{
			mObject.name = name;
		}
	}
	public virtual bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		Collider collider = getCollider();
		if (collider == null)
		{
			hit = new RaycastHit();
			return false;
		}
		return collider.Raycast(ray, out hit, maxDistance);
	}
	public virtual GameObject getObject() { return mObject; }
	public T addUnityComponent<T>() where T : Component
	{
		if (mObject == null)
		{
			return null;
		}
		T com = mObject.GetComponent<T>();
		if (com != null)
		{
			return com;
		}
		return mObject.AddComponent<T>();
	}
	public bool isUnityComponentEnabled<T>() where T : Behaviour
	{
		T com = getUnityComponent<T>(false);
		return com != null && com.enabled;
	}
	public void enableUnityComponent<T>(bool enable) where T : Behaviour
	{
		T com = getUnityComponent<T>();
		if (com != null)
		{
			com.enabled = enable;
		}
	}
	public T getUnityComponent<T>(bool addIfNotExist = true) where T : Component
	{
		T com = mObject.GetComponent<T>();
		if (com == null && addIfNotExist)
		{
			com = mObject.AddComponent<T>();
		}
		return com;
	}
	public void getUnityComponent<T>(out T com, bool addIfNotExist = true) where T : Component
	{
		com = mObject.GetComponent<T>();
		if (com == null && addIfNotExist)
		{
			com = mObject.AddComponent<T>();
		}
	}
	// 从当前节点以及所有子节点中查找指定组件
	public T[] getUnityComponentsInChild<T>(bool includeInactive = true) where T : Component
	{
		return mObject.GetComponentsInChildren<T>(includeInactive);
	}
	// 从指定的子节点中查找指定组件
	public T getUnityComponentInChild<T>(string childName) where T : Component
	{
		GameObject child = getGameObject(childName, mObject);
		return child.GetComponent<T>();
	}
	// 从当前以及所有子节点中查找指定组件
	public T getUnityComponentInChild<T>(bool includeInactive = true) where T : Component
	{
		return mObject.GetComponentInChildren<T>(includeInactive);
	}
	public GameObject getGameObject() { return mObject; }
	public Transform getTransform() { return mTransform; }
	public Vector3 getLeft(bool ignoreY = false)
	{
		Vector3 left = localToWorldDirection(Vector3.left);
		return ignoreY ? normalize(resetY(left)) : left;
	}
	public Vector3 getRight(bool ignoreY = false)
	{
		Vector3 right = localToWorldDirection(Vector3.right);
		return ignoreY ? normalize(resetY(right)) : right;
	}
	public Vector3 getBack(bool ignoreY = false)
	{
		Vector3 back = localToWorldDirection(Vector3.back);
		return ignoreY ? normalize(resetY(back)) : back;
	}
	public Vector3 getForward(bool ignoreY = false)
	{
		Vector3 forward = localToWorldDirection(Vector3.forward);
		return ignoreY ? normalize(resetY(forward)) : forward;
	}
	public virtual bool isActive() { return mObject.activeSelf; }
	public virtual bool isActiveInHierarchy() { return mObject.activeInHierarchy; }
	public string getLayer() { return LayerMask.LayerToName(mObject.layer); }
	public int getLayerInt() { return mObject.layer; }
	public virtual bool isEnable() { return mEnable; }
	public virtual void setEnable(bool enable) { mEnable = enable; }
	public virtual Vector3 getPosition() { return mTransform.localPosition; }
	public virtual Vector3 getRotation()
	{
		Vector3 vector3 = mTransform.localEulerAngles;
		adjustAngle180(ref vector3.z);
		return vector3;
	}
	public virtual void cloneFrom(myUIObject obj)
	{
		if (Typeof(obj) != Typeof(this))
		{
			logError("ui type is different, can not clone!");
			return;
		}
		setPosition(obj.getPosition());
		setRotation(obj.getRotation());
		setScale(obj.getScale());
	}
	public int getSiblingIndex() { return mTransform.GetSiblingIndex(); }
	public int getChildCount() { return mTransform.childCount; }
	public GameObject getChild(int index) { return mTransform.GetChild(index).gameObject; }
	public virtual Vector3 getScale() { return mTransform.localScale; }
	public virtual Vector3 getWorldPosition() { return mTransform.position; }
	public virtual Vector3 getWorldScale() { return mTransform.lossyScale; }
	public virtual Vector3 getWorldRotation() { return mTransform.rotation.eulerAngles; }
	public Vector3 getRotationRadian()
	{
		Vector3 vector3 = toRadian(mTransform.localEulerAngles);
		adjustRadian180(ref vector3.z);
		return vector3;
	}
	public Quaternion getQuaternionRotation() { return mTransform.localRotation; }
	public Quaternion getWorldQuaternionRotation() { return mTransform.rotation; }
	public virtual void setPosition(Vector3 pos) { mTransform.localPosition = pos; }
	public virtual void setScale(Vector3 scale) { mTransform.localScale = scale; }
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public virtual void setRotation(Vector3 rot) { mTransform.localEulerAngles = rot; }
	public virtual void setWorldPosition(Vector3 pos) { mTransform.position = pos; }
	public virtual void setWorldRotation(Vector3 rot) { mTransform.eulerAngles = rot; }
	public virtual void setWorldScale(Vector3 scale)
	{
		if (mTransform.parent != null)
		{
			mTransform.localScale = devideVector3(scale, mTransform.parent.lossyScale);
		}
		else
		{
			mTransform.localScale = scale;
		}
	}
	public Vector3 localToWorld(Vector3 point) { return localToWorld(mTransform, point); }
	public Vector3 worldToLocal(Vector3 point) { return worldToLocal(mTransform, point); }
	public Vector3 localToWorldDirection(Vector3 direction) { return localToWorldDirection(mTransform, direction); }
	public Vector3 worldToLocalDirection(Vector3 direction) { return worldToLocalDirection(mTransform, direction); }
	public void setPositionX(float x) { setPosition(replaceX(getPosition(), x)); }
	public void setPositionY(float y) { setPosition(replaceY(getPosition(), y)); }
	public void setPositionZ(float z) { setPosition(replaceZ(getPosition(), z)); }
	public void setRotationX(float rotX) { setRotation(replaceX(mTransform.localEulerAngles, rotX)); }
	public void setRotationY(float rotY) { setRotation(replaceY(mTransform.localEulerAngles, rotY)); }
	public void setRotationZ(float rotZ) { setRotation(replaceZ(mTransform.localEulerAngles, rotZ)); }
	public void setScaleX(float x) { setScale(replaceX(mTransform.localScale, x)); }
	public virtual void move(Vector3 moveDelta, Space space = Space.Self)
	{
		if (space == Space.Self)
		{
			moveDelta = rotateVector3(moveDelta, getQuaternionRotation());
		}
		setPosition(getPosition() + moveDelta);
	}
	public void rotate(Vector3 rotation) { mTransform.Rotate(rotation, Space.Self); }
	public void rotateWorld(Vector3 rotation) { mTransform.Rotate(rotation, Space.World); }
	// angle为角度制
	public void rotateAround(Vector3 axis, float angle) { mTransform.Rotate(axis, angle, Space.Self); }
	public void rotateAroundWorld(Vector3 axis, float angle) { mTransform.Rotate(axis, angle, Space.World); }
	public void lookAt(Vector3 direction) 
	{
		if (isVectorZero(direction))
		{
			return;
		}
		setRotation(getLookAtRotation(direction)); 
	}
	public void lookAtPoint(Vector3 point) { setRotation(getLookAtRotation(point - getPosition())); }
	public void yawPitch(float yaw, float pitch)
	{
		Vector3 curRot = getRotation();
		curRot.x += pitch;
		curRot.y += yaw;
		setRotation(curRot);
	}
	public void resetTransform()
	{
		mTransform.localPosition = Vector3.zero;
		mTransform.localEulerAngles = Vector3.zero;
		mTransform.localScale = Vector3.one;
	}
	public void setParent(GameObject parent, bool resetTrans = true)
	{
		if (parent == null)
		{
			mTransform.parent = null;
		}
		else
		{
			mTransform.parent = parent.transform;
		}
		if (resetTrans)
		{
			resetTransform();
		}
	}
	public void copyObjectTransform(GameObject obj)
	{
		Transform objTrans = obj.transform;
		FT.MOVE(this, objTrans.localPosition);
		FT.ROTATE(this, objTrans.localEulerAngles);
		FT.SCALE(this, objTrans.localScale);
	}
	public virtual bool isChildOf(IMouseEventCollect parent)
	{
		var obj = parent as Transformable;
		if (obj == null)
		{
			return false;
		}
		return mTransform.IsChildOf(obj.getTransform());
	}
	public virtual void setAlpha(float alpha)
	{
		var renderers = getUnityComponentsInChild<Renderer>();
		int count = renderers.Length;
		for (int i = 0; i < count; ++i)
		{
			Material material = renderers[i].material;
			Color color = material.color;
			color.a = alpha;
			material.color = color;
		}
	}
	public virtual float getAlpha()
	{
		return getUnityComponent<Renderer>().material.color.a;
	}
	public bool canUpdate() { return mObject.activeInHierarchy && mEnable; }
}