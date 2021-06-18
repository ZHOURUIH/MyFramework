using System;
using System.Collections.Generic;
using UnityEngine;

// 摄像机的连接器,用于第三人称摄像机的跟随逻辑
public class CameraLinker : GameComponent
{
	protected Dictionary<Type, CameraLinkerSwitch> mSwitchList; // 转换器列表
	protected CameraLinkerSwitch mCurSwitch;                    // 当前转换器
	protected MovableObject mLinkObject;                        // 摄像机跟随的物体
	protected GameCamera mCamera;                               // 所属摄像机
	protected Vector3 mRelativePosition;                        // 相对位置
	protected Vector3 mOriginRelativePosition;                  // 原始的相对位置,因为在设置mRelativePosition后可能会由于连接器或者其他原因对其进行动态修改,所以需要记录一个原始的位置
	protected Vector3 mLookAtOffset;                            // 焦点的偏移,实际摄像机的焦点是物体的位置加上偏移
	protected LINKER_UPDATE mUpdateMoment;                      // 连接器更新时机
	protected bool mUseTargetYaw;                               // 是否使用目标物体的旋转来旋转摄像机的位置,给子类使用
	protected bool mLookAtTarget;                               // 是否在摄像机运动过程中一直看向目标位置
	public CameraLinker()
	{
		mLookAtOffset = new Vector3(0.0f, 2.0f, 0.0f);
		mUpdateMoment = LINKER_UPDATE.LATE_UPDATE;
		mSwitchList = new Dictionary<Type, CameraLinkerSwitch>();
		mUseTargetYaw = true;
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
		mLookAtOffset = new Vector3(0.0f, 2.0f, 0.0f);
		mUpdateMoment = LINKER_UPDATE.LATE_UPDATE;
		mLookAtTarget = false;
		mUseTargetYaw = true;
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
	public virtual void applyRelativePosition(Vector3 relative)
	{
		if (mLinkObject == null)
		{
			return;
		}
		Vector3 newPos = mLinkObject.getWorldPosition() + relative;
		mCamera.setPosition(newPos);
		if (mLookAtTarget)
		{
			// 让摄像机朝向目标
			mCamera.lookAt(mLinkObject.getWorldPosition() + mLookAtOffset - mCamera.getPosition());
		}
	}
	public void setUseTargetYaw(bool use) { mUseTargetYaw = use; }
	public bool isUseTargetYaw() { return mUseTargetYaw; }
	public void setUpdateMoment(LINKER_UPDATE moment) { mUpdateMoment = moment; }
	public void setLinkObject(MovableObject obj) { mLinkObject = obj; }
	public MovableObject getLinkObject() { return mLinkObject; }
	public Vector3 getOriginRelativePosition() { return mOriginRelativePosition; }
	public void setOriginRelativePosition(Vector3 relative) { mOriginRelativePosition = relative; }
	public Vector3 getRelativePosition() { return mRelativePosition; }
	public virtual void setRelativePosition(Vector3 pos) { mRelativePosition = pos; }
	public virtual void setRelativePositionWithSwitch(Vector3 pos, Type switchType, bool useDefaultSwitchSpeed = true, float switchSpeed = 1.0f)
	{
		setRelativePosition(pos);
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
		mCurSwitch.init(mRelativePosition, pos, switchSpeed);
	}
	// 由转换器调用,通知连接器转换已经完成
	public void notifyFinishSwitching() { mCurSwitch = null; }
	public CameraLinkerSwitch getSwitch(Type type)
	{
		mSwitchList.TryGetValue(type, out CameraLinkerSwitch linkerSwitch);
		return linkerSwitch;
	}
	public void setLookAtOffset(Vector3 offset) { mLookAtOffset = offset; }
	public Vector3 getLookAtOffset() { return mLookAtOffset; }
	public void setLookAtTarget(bool lookat) { mLookAtTarget = lookat; }
	public bool isLookAtTarget() { return mLookAtTarget; }
	//------------------------------------------------------------------------------------------------------------------------------------------------
	protected virtual void updateLinker(float elapsedTime) { }
	protected void initSwitch()
	{
		addSwitch(typeof(CameraLinkerSwitchLinear));
		addSwitch(typeof(CameraLinkerSwitchCircle));
		addSwitch(typeof(CameraLinkerSwitchAroundTarget));
	}
	protected void addSwitch(Type classType)
	{
		var lineSwitch = createInstance<CameraLinkerSwitch>(classType);
		lineSwitch.setLinker(this);
		mSwitchList.Add(classType, lineSwitch);
	}
	protected void destroySwitch()
	{
		foreach (var item in mSwitchList)
		{
			item.Value.destroy();
		}
		mSwitchList.Clear();
		mCurSwitch = null;
	}
}