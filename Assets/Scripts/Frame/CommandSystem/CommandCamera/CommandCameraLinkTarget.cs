using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandCameraLinkTarget : Command
{
	protected Vector3 mRelativePosition;    // 相对位置
	protected float mSwitchSpeed;           // 转换器的速度
	protected bool mUseOriginRelative;      // 是否使用连接器原来的相对位置
	protected bool mUseLastSwitchSpeed;     // 是否使用当前连接器的速度
	public CAMERA_LINKER_SWITCH mSwitchType;
	public MovableObject mTarget;
	public CameraLinker mLinker;
	public Vector3 mLookatOffset;           // 看向目标的位置偏移
	public bool mLookAtTarget;              // 是否始终看向目标
	public bool mAutoProcessKey;            // 是否在断开连接器后可以使用按键控制摄像机
	public bool mImmediately;				// 是否直接将摄像机设置到当前连接器的正常位置
	public override void init()
	{
		base.init();
		mTarget = null;
		mSwitchType = CAMERA_LINKER_SWITCH.CLS_NONE;
		mLinker = null;
		mLookAtTarget = true;
		mLookatOffset = Vector3.zero;
		mUseOriginRelative = true;
		mRelativePosition = Vector3.zero;
		mUseLastSwitchSpeed = true;
		mSwitchSpeed = 10.0f;
		mAutoProcessKey = false;
		mImmediately = false;
	}
	public void setRelativePosition(Vector3 relative)
	{
		mUseOriginRelative = false;
		mRelativePosition = relative;
	}
	public void setSwitchSpeed(float speed)
	{
		mUseLastSwitchSpeed = false;
		mSwitchSpeed = speed;
	}
	public override void execute()
	{
		GameCamera camera = mReceiver as GameCamera;
		camera.linkTarget(mLinker, mTarget);
		if (mTarget != null)
		{
			// 停止正在进行的摄像机运动
			OT.MOVE(camera, camera.getPosition());
			OT.ROTATE(camera, camera.getRotation());
			mLinker.setLookAtTarget(mLookAtTarget);
			mLinker.setLookAtOffset(mLookatOffset);
			// 不使用原来的相对位置,则设置新的相对位置
			if (!mUseOriginRelative)
			{
				mLinker.setRelativePosition(mRelativePosition, mSwitchType, mUseLastSwitchSpeed, mSwitchSpeed);
			}
			if (mImmediately)
			{
				mLinker.applyRelativePosition(mLinker.getNormalRelativePosition());
			}
		}
	}
}