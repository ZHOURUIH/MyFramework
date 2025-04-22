using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static UnityUtility;
using static FrameUtility;
using static BinaryUtility;
using static FrameBaseUtility;

// 摄像机的连接器,用于摄像机的跟随逻辑
public class CameraLinker : GameComponent
{
	protected Dictionary<Type, CameraLinkerSwitch> mSwitchList = new(); // 转换器列表
	protected CameraLinkerSwitch mCurSwitch;							// 当前转换器
	protected MovableObject mLinkObject;								// 摄像机跟随的物体
	protected GameCamera mCamera;										// 所属摄像机
	protected Vector3 mRelativePosition;								// 相对位置
	protected Vector3 mOriginRelativePosition;							// 原始的相对位置,因为在设置mRelativePosition后可能会由于连接器或者其他原因对其进行动态修改,所以需要记录一个原始的位置
	protected Vector3 mLookAtOffset;									// 焦点的偏移,实际摄像机的焦点是物体的位置加上偏移
	protected float mMinRelativePitch;									// 如果外部改变了mRelativePosition的值,则mRelativePosition的俯仰角不能小于mMinRelativePitch
	protected float mMaxRelativePitch;									// 如果外部改变了mRelativePosition的值,则mRelativePosition的俯仰角不能大于mMaxRelativePitch
	protected bool mUseTargetYaw;										// 是否使用目标物体的旋转来旋转摄像机的相对位置,给子类使用
	protected bool mLookAtTarget;										// 是否在摄像机运动过程中一直看向目标位置
	protected CAMERA_COLLISION mCameraCollision;						// 摄像机碰撞检测类型.需要mLookAtTarget为true
	protected LINKER_UPDATE mUpdateMoment;								// 连接器更新时机
	protected int mIgnoreLayer = -1;									// 摄像机跟随过滤空气墙
	protected Collider[] mTempResults;									// 碰撞检测的临时对象,用于避免频繁GC
	public CameraLinker()
	{
		mLookAtOffset = new(0.0f, 2.0f, 0.0f);
		mMinRelativePitch = toRadian(0.0f);
		mMaxRelativePitch = toRadian(85.0f);
		mUpdateMoment = LINKER_UPDATE.LATE_UPDATE;
		mCameraCollision = CAMERA_COLLISION.NONE;
		mTempResults = new Collider[8];
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		initSwitch();
		mCamera = mComponentOwner as GameCamera;
	}
	public override void destroy()
	{
		base.destroy();
		destroySwitch();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSwitchList.Clear();
		mCurSwitch = null;
		mLinkObject = null;
		mCamera = null;
		mRelativePosition = Vector3.zero;
		mOriginRelativePosition = Vector3.zero;
		mLookAtOffset = new(0.0f, 2.0f, 0.0f);
		mMinRelativePitch = toRadian(0.0f);
		mMaxRelativePitch = toRadian(85.0f);
		mUseTargetYaw = false;
		mLookAtTarget = false;
		mCameraCollision = CAMERA_COLLISION.NONE;
		mUpdateMoment = LINKER_UPDATE.LATE_UPDATE; 
		mIgnoreLayer = -1;
		memset(mTempResults, null);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mLinkObject == null)
		{
			return;
		}
		if (mUpdateMoment == LINKER_UPDATE.UPDATE)
		{
			mCurSwitch?.update(elapsedTime);
			updateLinker(elapsedTime);
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		if (mLinkObject == null)
		{
			return;
		}
		if (mUpdateMoment == LINKER_UPDATE.LATE_UPDATE)
		{
			mCurSwitch?.update(elapsedTime);
			updateLinker(elapsedTime);
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		if (mLinkObject == null)
		{
			return;
		}
		if (mUpdateMoment == LINKER_UPDATE.FIXED_UPDATE)
		{
			mCurSwitch?.update(elapsedTime);
			updateLinker(elapsedTime);
		}
	}
	// 当使用此连接器时调用
	public virtual void onLinked() { }
	// 断开此连接器时调用
	public virtual void onUnlink() { }
	public virtual void applyRelativePosition(Vector3 relative)
	{
		if (mLinkObject?.getTransform() == null)
		{
			return;
		}
		Vector3 newPos = mLinkObject.getWorldPosition() + relative;
		mCamera.setPosition(newPos);

		if (mLookAtTarget)
		{
			Vector3 lookAtPos = mLinkObject.getWorldPosition() + mLookAtOffset;
			// 检查目标与摄像机之间是否有阻挡视线的模型
			if (mCameraCollision == CAMERA_COLLISION.CHECK_MODEL_BETWEEN)
			{
				checkCameraRayIntersect();
			}
			else if (mCameraCollision == CAMERA_COLLISION.CHECK_MODEL_INTERSECT)
			{
				var box = mCamera.getOrAddUnityComponent<BoxCollider>();
				// 只需要一个较小的碰撞体即可
				if (!isVectorEqual(box.size, new(0.1f, 0.1f, 0.1f)))
				{
					box.size = new(0.1f, 0.1f, 0.1f);
				}
				// 判断是否与任意模型相交,与模型相交才会拉近
				if (overlapAllBox(box, mTempResults) > 0)
				{
					checkCameraRayIntersect();
				}
			}

			// 让摄像机朝向目标
			mCamera.lookAtPoint(lookAtPos);
		}
	}
	public void setMinRelativePitch(float minPitch) { mMinRelativePitch = minPitch; }
	public void setMaxRelativePitch(float maxPitch) { mMaxRelativePitch = maxPitch; }
	public void setUseTargetYaw(bool use) { mUseTargetYaw = use; }
	public bool isUseTargetYaw() { return mUseTargetYaw; }
	public void setUpdateMoment(LINKER_UPDATE moment) { mUpdateMoment = moment; }
	public void setLinkObject(MovableObject obj) { mLinkObject = obj; }
	public MovableObject getLinkObject() { return mLinkObject; }
	public Vector3 getOriginRelativePosition() { return mOriginRelativePosition; }
	public void setOriginRelativePosition(Vector3 relative) { mOriginRelativePosition = relative; }
	public Vector3 getRelativePosition() { return mRelativePosition; }
	// 水平旋转相对位置
	public void rotateRelativePositionHorizontal(float deltaDegree)
	{
		Vector3 relative = rotateVector3(mRelativePosition, Quaternion.AngleAxis(deltaDegree, Vector3.up));
		setRelativePosition(relative);
	}
	// 竖直方向上旋转相对位置
	public void rotateRelativePositionVertical(float deltaDegree)
	{
		Vector3 normal = generateNormal(mRelativePosition, replaceY(mRelativePosition, mRelativePosition.y - 1.0f));
		Vector3 relative = rotateVector3(mRelativePosition, Quaternion.AngleAxis(deltaDegree, normal));
		setRelativePosition(relative);
	}
	public virtual void setRelativePosition(Vector3 relative)
	{
		float curPitch = -getVectorPitch(relative);
		if (curPitch < mMinRelativePitch || curPitch > mMaxRelativePitch)
		{
			setVectorPitch(ref relative, -clamp(curPitch, mMinRelativePitch, mMaxRelativePitch));
		}
		mRelativePosition = relative;
	}
	public virtual void setRelativePositionWithSwitch(Vector3 relative, Type switchType, bool useDefaultSwitchSpeed = true, float switchSpeed = 1.0f)
	{
		setRelativePosition(relative);
		// 如果不使用转换器,则直接设置位置
		if (switchType == null)
		{
			mCurSwitch = null;
			return;
		}
		// 如果使用转换器,则查找相应的转换器,设置参数
		mCurSwitch = getSwitch(switchType);
		// 找不到则直接设置位置
		if (mCurSwitch == null)
		{
			return;
		}
		// 如果不使用默认速度,其实是转换器当前的速度,则设置新的速度
		if (useDefaultSwitchSpeed)
		{
			switchSpeed = mCurSwitch.getSwitchSpeed();
		}
		mCurSwitch.init(mRelativePosition, relative, switchSpeed);
	}
	// 由转换器调用,通知连接器转换已经完成
	public void notifyFinishSwitching() { mCurSwitch = null; }
	public CameraLinkerSwitch getSwitch(Type type) { return mSwitchList.get(type); }
	public void setLookAtOffset(Vector3 offset) { mLookAtOffset = offset; }
	public virtual void setLookAtTarget(bool lookat) { mLookAtTarget = lookat; }
	public void setCameraCollision(CAMERA_COLLISION type) { mCameraCollision = type; }
	public Vector3 getLookAtOffset() { return mLookAtOffset; }
	public bool isLookAtTarget() { return mLookAtTarget; }
	public CAMERA_COLLISION getCameraCollision() { return mCameraCollision; }
	public void setIgnoreLayerMask(int ignoreLayerMask){ mIgnoreLayer = ignoreLayerMask; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void updateLinker(float elapsedTime) { }
	protected void initSwitch()
	{
		addSwitch<CameraLinkerSwitchLinear>();
		addSwitch<CameraLinkerSwitchCircle>();
		addSwitch<CameraLinkerSwitchAroundTarget>();
	}
	protected void addSwitch<T>() where T : CameraLinkerSwitch, new()
	{
		mSwitchList.add(typeof(T), CLASS<T>()).setLinker(this);
	}
	protected void destroySwitch()
	{
		UN_CLASS_LIST(mSwitchList);
		mCurSwitch = null;
	}
	// 检测摄像机与看向的目标之间是否有模型遮挡,如果有,则将摄像机拉近到最近的射线交点处
	protected void checkCameraRayIntersect()
	{
		float nearestDis = float.MaxValue;
		Vector3 nearestPoint = Vector3.zero;
		Transform targetTrans = mLinkObject.getTransform();
		Vector3 lookAtPos = mLinkObject.getWorldPosition() + mLookAtOffset;
		Vector3 newPos = mCamera.getPosition();
		Vector3 originPos = lookAtPos + setLength(newPos - lookAtPos, 0.1f);
		if (isEditor())
		{
			Debug.DrawLine(originPos, newPos, Color.blue);
		}
		Transform avatarTrans = null;
		GameObject modelGo = (mLinkObject as Character)?.getAvatar()?.getModel();
		if (modelGo != null)
		{
			avatarTrans = modelGo.transform;
		}
		using var a = new ArrayScope<RaycastHit>(out var hitRet, 2);
		int hitCount = raycastAll(new Ray(originPos, newPos - originPos), hitRet, getLength(originPos - newPos), mIgnoreLayer);
		for (int i = 0; i < hitCount; ++i)
		{
			// 需要排除目标的所有子节点,包括目标为角色是的模型节点,因为角色的模型不一定挂接到角色节点下
			RaycastHit hit = hitRet[i];
			Transform hitTrans = hit.transform;
			if (hitTrans == targetTrans || hitTrans.IsChildOf(targetTrans))
			{
				continue;
			}
			if (avatarTrans != null && (hitTrans == avatarTrans || hitTrans.IsChildOf(avatarTrans)))
			{
				continue;
			}

			// 只判断模型即可,并且需要找到离角色最近的一个交点
			if (hitTrans.TryGetComponent<MeshFilter>(out var meshFilter) && 
				meshFilter.sharedMesh != null &&
				hit.distance < nearestDis)
			{
				nearestDis = hit.distance;
				nearestPoint = hit.point;
			}
		}
		if (nearestDis < float.MaxValue)
		{
			mCamera.setPosition(nearestPoint + setLength(lookAtPos - newPos, 0.2f));
		}
	}
}