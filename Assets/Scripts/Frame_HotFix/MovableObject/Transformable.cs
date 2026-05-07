using UnityEngine;
using System;
using System.Collections.Generic;
using static MathUtility;
using static UnityUtility;

// 可变换的物体,2D和3D物体都是可变换,也就是都会包含一个Transform
public class Transformable : ComponentOwner, ITransformable
{
	protected Transform mTransform;						// 变换组件
	protected GameObject mObject;						// 物体节点
	protected List<Action> mPositionModifyCallback;		// 使用注册回调的方式来代替虚函数重写
	protected List<Action> mRotationModifyCallback;		// 使用注册回调的方式来代替虚函数重写
	protected List<Action> mScaleModifyCallback;		// 使用注册回调的方式来代替虚函数重写
	protected List<Action> mWorldScaleModifyCallback;   // 使用注册回调的方式来代替虚函数重写
	protected Vector3 mLastWorldScale;                  // 上一次设置的世界缩放值
	protected bool mPositionDirty = true;				// 位置是否有更改
	protected bool mNeedUpdate = true;					// 是否启用更新,与Active共同控制是否执行更新
	protected bool mActive;								// 缓存的GameObject.activeSelf状态,用于避免频繁访问activeSelf属性
	protected Vector3 mPosition;						// 单独存储位置,可以在大多数时候避免访问Transform
	public override void resetProperty()
	{
		base.resetProperty();
		mTransform = null;
		mObject = null;
		mPositionModifyCallback?.Clear();
		mRotationModifyCallback?.Clear();
		mScaleModifyCallback?.Clear();
		mWorldScaleModifyCallback?.Clear();
		mLastWorldScale = Vector3.zero;
		mPositionDirty = true;
		mNeedUpdate = true;
		mActive = false;
		mPosition = Vector3.zero;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 检测世界缩放值是否有变化
		if (!mWorldScaleModifyCallback.isEmpty())
		{
			Vector3 worldScale = getWorldScale();
			if (!isVectorEqual(mLastWorldScale, worldScale))
			{
				foreach (Action item in mWorldScaleModifyCallback)
				{
					item?.Invoke();
				}
				mLastWorldScale = worldScale;
			}
		}
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
			mActive = mObject.activeSelf;
			mLastWorldScale = getWorldScale();
		}
		else
		{
			mTransform = null;
			mActive = false;
		}
	}
	public override bool setActive(bool active)
	{
		if (active == mActive)
		{
			return active;
		}
		if (mObject != null)
		{
			using var a = new ProfilerScope("active object");
			mObject.SetActive(active);
		}
		mActive = active;
		return base.setActive(active);
	}
	public void resetActive()
	{
		// 重新禁用再启用,可以重置状态
		mObject.SetActive(false);
		mObject.SetActive(true);
		mActive = true;
	}
	public void enableAllColliders(bool enable)
	{
		foreach (var collider in mObject.GetComponents<Collider>())
		{
			collider.enabled = enable;
		}
	}
	// 返回第一个碰撞体,当前节点找不到,则会在子节点中寻找
	public Collider getColliderInChild() { return getUnityComponentInChild<Collider>(true); }
	// 返回第一个碰撞体,仅在当前节点中寻找,允许子类重写此函数,碰撞体可能不在当前节点,或者也不在此节点的子节点中
	public virtual Collider getCollider(bool addIfNotExist = false)
	{
		var collider = tryGetUnityComponent<Collider>();
		// 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
		if (addIfNotExist && collider == null)
		{
			collider = getOrAddUnityComponent<BoxCollider>();
		}
		return collider;
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
	public GameObject getGameObject() { return mObject; }
	public int getGameObjectInstanceID() { return getGameObjectID(mObject); }
	public bool isUnityComponentEnabled<T>() where T : Behaviour
	{
		T com = tryGetUnityComponent<T>();
		return com != null && com.enabled;
	}
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
		if (mObject == null)
		{
			return null;
		}
		mObject.TryGetComponent(out T com);
		return com;
	}
	public bool tryGetUnityComponent<T>(out T com) where T : Component
	{
		if (mObject == null)
		{
			com = null;
			return false;
		}
		return mObject.TryGetComponent(out com);
	}
	public T getOrAddUnityComponent<T>() where T : Component
	{
		FrameBaseUtility.getOrAddComponent(mObject, out T com);
		return com;
	}
	// 从当前节点以及所有子节点中查找指定组件
	public void getUnityComponentsInChild<T>(bool includeInactive, List<T> list) where T : Component
	{
		if (mObject == null)
		{
			return;
		}
		mObject.GetComponentsInChildren(includeInactive, list);
	}
	// 从当前节点以及所有子节点中查找指定组件
	public T[] getUnityComponentsInChild<T>(bool includeInactive) where T : Component
	{
		if (mObject == null)
		{
			return null;
		}
		return mObject.GetComponentsInChildren<T>(includeInactive);
	}
	// 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
	public void getUnityComponentsInChild<T>(List<T> list) where T : Component
	{
		if (mObject == null)
		{
			return;
		}
		mObject.GetComponentsInChildren(list);
	}
	// 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
	public T[] getUnityComponentsInChild<T>() where T : Component
	{
		if (mObject == null)
		{
			return null;
		}
		return mObject.GetComponentsInChildren<T>();
	}
	// 从指定的子节点中查找指定组件
	public T getUnityComponentInChild<T>(string childName) where T : Component
	{
		GameObject go = FrameBaseUtility.getGameObject(childName, mObject);
		if (go == null)
		{
			return null;
		}
		go.TryGetComponent(out T com);
		return com;
	}
	// 从当前以及所有子节点中查找指定组件
	public T getUnityComponentInChild<T>(bool includeInactive = true) where T : Component
	{
		if (mObject == null)
		{
			return null;
		}
		return mObject.GetComponentInChildren<T>(includeInactive);
	}
	public GameObject getUnityObject() { return mObject; }
	public Transform getTransform() { return mTransform; }
	public Vector3 getLeft(bool ignoreY = false) { return ignoreY ? normalize(resetY(-mTransform.right)) : -mTransform.right; }
	public Vector3 getRight(bool ignoreY = false) { return ignoreY ? normalize(resetY(mTransform.right)) : mTransform.right; }
	public Vector3 getBack(bool ignoreY = false) { return ignoreY ? normalize(resetY(-mTransform.forward)) : -mTransform.forward; }
	public Vector3 getForward(bool ignoreY = false) { return ignoreY ? normalize(resetY(mTransform.forward)) : mTransform.forward; }
	public virtual bool isActive() { return mActive; }
	public virtual bool isActiveInHierarchy() { return mObject != null && mObject.activeInHierarchy; }
	public string getLayerName() { return LayerMask.LayerToName(mObject.layer); }
	public int getLayer() { return mObject.layer; }
	public virtual bool isNeedUpdate() { return mNeedUpdate; }
	public virtual void setNeedUpdate(bool enable) { mNeedUpdate = enable; }
	public void addPositionModifyCallback(Action callback) 
	{
		mPositionModifyCallback ??= new();
		mPositionModifyCallback.add(callback); 
	}
	public void removePositionModifyCallback(Action callback) { mPositionModifyCallback.Remove(callback); }
	public void addRotationModifyCallback(Action callback) 
	{
		mRotationModifyCallback ??= new();
		mRotationModifyCallback.add(callback); 
	}
	public void removeRotationModifyCallback(Action callback) { mRotationModifyCallback.Remove(callback); }
	public void addScaleModifyCallback(Action callback) 
	{
		mScaleModifyCallback ??= new();
		mScaleModifyCallback.add(callback); 
	}
	public void removeWorldScaleModifyCallback(Action callback) { mWorldScaleModifyCallback.Remove(callback); }
	public void addWorldScaleModifyCallback(Action callback) 
	{
		mWorldScaleModifyCallback ??= new();
		mWorldScaleModifyCallback.add(callback); 
	}
	public void removeScaleModifyCallback(Action callback) { mScaleModifyCallback.Remove(callback); }
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
	public int getSiblingIndex() { return mTransform != null ? mTransform.GetSiblingIndex() : 0; }
	public int getChildCount() { return mTransform != null ? mTransform.childCount : 0; }
	public GameObject getChild(int index) { return mTransform != null ? mTransform.GetChild(index).gameObject : null; }
	public Vector3 getScale() { return mTransform != null ? mTransform.localScale : Vector3.zero; }
	public Vector3 getWorldPosition() { return mTransform != null ? mTransform.position : Vector3.zero; }
	public Vector3 getWorldScale() { return mTransform != null ? mTransform.lossyScale : Vector3.zero; }
	public Vector3 getWorldRotation() { return mTransform != null ? mTransform.rotation.eulerAngles : Vector3.zero; }
	public Vector3 getRotationRadian()
	{
		if (mTransform == null)
		{
			return Vector3.zero;
		}
		Vector3 vector3 = toRadian(mTransform.localEulerAngles);
		adjustRadian180(ref vector3.z);
		return vector3;
	}
	public Quaternion getRotationQuaternion() { return mTransform != null ? mTransform.localRotation : Quaternion.identity; }
	public Quaternion getWorldQuaternionRotation() { return mTransform != null ? mTransform.rotation : Quaternion.identity; }
	public void setPosition(Vector3 pos)
	{
		if (mTransform == null)
		{
			return;
		}
		if (isVectorEqual(mTransform.localPosition, pos))
		{
			return;
		}
		mPositionDirty = true;
		mTransform.localPosition = pos;
		foreach (Action item in mPositionModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public void setScale(float scale)
	{
		setScale(new Vector3(scale, scale, scale));
	}
	public void setScale(Vector3 scale)
	{
		if (mTransform == null || isVectorEqual(mTransform.localScale, scale))
		{
			return;
		}
		mTransform.localScale = scale;
		foreach (Action item in mScaleModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public void setRotation(Vector3 rot)
	{
		if (mTransform == null || isVectorEqual(mTransform.localEulerAngles, rot))
		{
			return;
		}
		mTransform.localEulerAngles = rot;
		foreach (Action item in mRotationModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public void setRotation(Quaternion rot)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.localRotation = rot;
		foreach (Action item in mRotationModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public void setWorldPosition(Vector3 pos)
	{
		if (mTransform == null)
		{
			return;
		}
		mPositionDirty = true;
		mTransform.position = pos;
		foreach (Action item in mPositionModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public void setWorldRotation(Vector3 rot)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.eulerAngles = rot;
		foreach (Action item in mRotationModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public void setWorldRotation(Quaternion rot)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.rotation = rot;
		foreach (Action item in mRotationModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public void setWorldScale(Vector3 scale)
	{
		if (mTransform == null)
		{
			return;
		}
		if (mTransform.parent != null)
		{
			mTransform.localScale = divideVector3(scale, mTransform.parent.lossyScale);
		}
		else
		{
			mTransform.localScale = scale;
		}
		foreach (Action item in mScaleModifyCallback.safe())
		{
			item?.Invoke();
		}
		foreach (Action item in mWorldScaleModifyCallback.safe())
		{
			item?.Invoke();
		}
	}
	public Vector3 localToWorld(Vector3 point) { return UnityUtility.localToWorld(mTransform, point); }
	public Vector3 worldToLocal(Vector3 point) { return UnityUtility.worldToLocal(mTransform, point); }
	public Vector3 localToWorldDirection(Vector3 direction) { return UnityUtility.localToWorldDirection(mTransform, direction); }
	public Vector3 worldToLocalDirection(Vector3 direction) { return UnityUtility.worldToLocalDirection(mTransform, direction); }
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
			moveDelta = rotateVector3(moveDelta, getRotationQuaternion());
		}
		setPosition(getPosition() + moveDelta);
	}
	public void rotate(Vector3 rotation)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.Rotate(rotation, Space.Self);
	}
	public void rotateWorld(Vector3 rotation)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.Rotate(rotation, Space.World);
	}
	// 绕本地坐标系下某个轴原地旋转,angle为角度制
	public void rotateAround(Vector3 axis, float angle)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.Rotate(axis, angle, Space.Self);
	}
	// 绕世界某条直线旋转
	public void rotateAround(Vector3 point, Vector3 axis, float angle)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.RotateAround(point, axis, angle);
	}
	public void rotateAroundWorld(Vector3 axis, float angle)
	{
		if (mTransform == null)
		{
			return;
		}
		mTransform.Rotate(axis, angle, Space.World);
	}
	public void lookAt(Vector3 direction)
	{
		if (isVectorZero(direction))
		{
			return;
		}
		setRotation(getLookAtQuaternion(direction));
	}
	public void lookAtPoint(Vector3 point)
	{
		if (!isVectorEqual(point, getPosition()))
		{
			setRotation(getLookAtQuaternion(point - getPosition()));
		}
	}
	public void yawPitch(float yaw, float pitch)
	{
		Vector3 curRot = getRotation();
		curRot.x += pitch;
		curRot.y += yaw;
		setRotation(curRot);
	}
	public void resetTransform()
	{
		if (mTransform == null)
		{
			return;
		}
		mPositionDirty = true;
		mTransform.localPosition = Vector3.zero;
		mTransform.localEulerAngles = Vector3.zero;
		mTransform.localScale = Vector3.one;
	}
	public void setLayer(int layer)
	{
		if (mObject == null)
		{
			return;
		}
		mObject.layer = layer;
	}
	public void setParent(GameObject parent, bool resetTrans = true)
	{
		if (mTransform == null)
		{
			return;
		}
		Transform parentTrans = parent != null ? parent.transform : null;
		if (parentTrans != mTransform.parent)
		{
			mTransform.SetParent(parentTrans);
			mPositionDirty = true;
			if (resetTrans)
			{
				resetTransform();
			}
		}
	}
	public void copyObjectTransform(GameObject obj)
	{
		Transform objTrans = obj.transform;
		this.MOVE(objTrans.localPosition);
		this.ROTATE(objTrans.localEulerAngles);
		this.SCALE(objTrans.localScale);
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
	public bool canUpdate() { return mNeedUpdate && mActive; }
}