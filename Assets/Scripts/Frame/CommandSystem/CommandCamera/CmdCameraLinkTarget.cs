using System;
using UnityEngine;

public class CmdCameraLinkTarget : Command
{
	protected Vector3 mRelativePosition;    // 相对位置
	protected float mSwitchSpeed;           // 转换器的速度
	protected bool mUseOriginRelative;      // 是否使用连接器原来的相对位置
	protected bool mUseLastSwitchSpeed;     // 是否使用当前连接器的速度
	public MovableObject mTarget;           // 摄像机要连接的对象,仅自由连接器允许连接对象为空,其他的连接器都需要一个连接对象
	public Vector3 mLookatOffset;           // 看向目标的位置偏移
	public Type mLinkerType;                // 连接器的类型
	public Type mSwitchType;                // 转换器的类型
	public bool mLookAtTarget;              // 是否始终看向目标
	public bool mImmediately;               // 是否直接将摄像机设置到当前连接器的正常位置
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mSwitchType = null;
		mLinkerType = null;
		mLookAtTarget = true;
		mLookatOffset = Vector3.zero;
		mUseOriginRelative = true;
		mRelativePosition = Vector3.zero;
		mUseLastSwitchSpeed = true;
		mSwitchSpeed = 10.0f;
		mImmediately = true;
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
		var camera = mReceiver as GameCamera;
		CameraLinker linker = camera.linkTarget(mLinkerType, mTarget);
		if (linker != null)
		{
			// 停止正在进行的摄像机运动
			FT.MOVE(camera, camera.getPosition());
			FT.ROTATE(camera, camera.getRotation());
			linker.setLookAtTarget(mLookAtTarget);
			linker.setLookAtOffset(mLookatOffset);
			// 不使用原来的相对位置,则设置新的相对位置
			if (!mUseOriginRelative)
			{
				// 只有在连接对象的命令中设置的相对位置才认为是原始位置
				linker.setOriginRelativePosition(mRelativePosition);
				if (mSwitchType == null)
				{
					linker.setRelativePosition(mRelativePosition);
				}
				else
				{
					linker.setRelativePositionWithSwitch(mRelativePosition, mSwitchType, mUseLastSwitchSpeed, mSwitchSpeed);
				}
			}
			if (mImmediately)
			{
				linker.applyRelativePosition(linker.getRelativePosition());
			}
		}
	}
}