using System;
using System.Collections.Generic;
using UnityEngine;

// 摄像机的连接器,用于第三人称摄像机的跟随逻辑
public class CameraLinker : GameComponent
{
	protected Dictionary<Type, CameraLinkerSwitch> mSwitchList; // 转换器列表
	protected CameraLinkerSwitch mCurSwitch;// 当前转换器
	protected MovableObject mLinkObject;	// 摄像机跟随的物体
	protected GameCamera mCamera;			// 所属摄像机
	protected Vector3 mRelativePosition;    // 相对位置
	protected Vector3 mLookAtOffset;        // 焦点的偏移,实际摄像机的焦点是物体的位置加上偏移
	protected bool mLateUpdate;				// 是否在LateUpdate中更新连接器
	protected bool mLookAtTarget;           // 是否在摄像机运动过程中一直看向目标位置
	public CameraLinker()
	{
		mLookAtOffset = new Vector3(0.0f, 2.0f, 0.0f);
		mLateUpdate = true;
		mSwitchList = new Dictionary<Type, CameraLinkerSwitch>();
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		initSwitch();
		mCamera = mComponentOwner as GameCamera;
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
		mLateUpdate = true;
		mLookAtTarget = false;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mLinkObject == null)
		{
			return;
		}
		if (!mLateUpdate)
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
		if (mLateUpdate)
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
			//让摄像机朝向
			Vector3 cameraDir = mLinkObject.getWorldPosition() + mLookAtOffset - mCamera.getPosition();
			Vector3 angles = getLookAtRotation(cameraDir);
			mCamera.setRotation(angles);
		}
	}
	public override void destroy()
	{
		base.destroy();
		destroySwitch();
	}
	public void setLinkObject(MovableObject obj) { mLinkObject = obj; }
	public MovableObject getLinkObject() { return mLinkObject; }
	public Vector3 getRelativePosition() { return mRelativePosition; }
	public virtual void setRelativePosition(Vector3 pos, Type switchType = null, 
											bool useDefaultSwitchSpeed = true, 
											float switchSpeed = 1.0f)
	{
		// 如果不使用转换器,则直接设置位置
		if (switchType == null)
		{
			mRelativePosition = pos;
			mCurSwitch = null;
			return;
		}
		// 如果使用转换器,则查找相应的转换器,设置参数
		mCurSwitch = getSwitch(switchType);
		// 找不到则直接设置位置
		if (mCurSwitch == null)
		{
			mRelativePosition = pos;
			return;
		}
		// 如果不使用默认速度,其实是转换器当前的速度,则设置新的速度
		if(useDefaultSwitchSpeed)
		{
			switchSpeed = mCurSwitch.getSwitchSpeed();
		}
		mCurSwitch.init(mRelativePosition, pos, switchSpeed);
	}
	// 由转换器调用,通知连接器转换已经完成
	public virtual void notifyFinishSwitching(CameraLinkerSwitch fixedSwitch) { mCurSwitch = null; }
	public CameraLinkerSwitch getSwitch(Type type)
	{
		mSwitchList.TryGetValue(type, out CameraLinkerSwitch linkerSwitch);
		return linkerSwitch;
	}
	public void setLookAtOffset(Vector3 offset) { mLookAtOffset = offset; }
	public Vector3 getLookAtOffset() { return mLookAtOffset; }
	public void setLookAtTarget(bool lookat) { mLookAtTarget = lookat; }
	public bool isLookAtTarget() { return mLookAtTarget; }
	public virtual Vector3 getNormalRelativePosition()
	{
		return rotateVector3(mRelativePosition, toRadian(mLinkObject.getRotation().y));
	}
	//------------------------------------------------------------------------------------------------------------------------------------------------
	protected virtual void updateLinker(float elapsedTime) { }
	protected void initSwitch()
	{
		addSwitch(Typeof<CameraLinkerSwitchLinear>());
		addSwitch(Typeof<CameraLinkerSwitchCircle>());
		addSwitch(Typeof<CameraLinkerSwitchAroundTarget>());
	}
	protected void addSwitch(Type classType)
	{
		var lineSwitch = createInstance<CameraLinkerSwitch>(classType);
		lineSwitch.initType(this);
		mSwitchList.Add(classType, lineSwitch);
	}
	protected void destroySwitch()
	{
		foreach(var item in mSwitchList)
		{
			item.Value.destroy();
		}
		mSwitchList.Clear();
		mCurSwitch = null;
	}
}