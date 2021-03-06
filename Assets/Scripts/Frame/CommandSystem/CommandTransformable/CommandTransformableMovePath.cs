﻿using UnityEngine;
using System.Collections.Generic;

public class CommandTransformableMovePath : Command
{
	public Dictionary<float, Vector3> mValueKeyFrame;
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public Vector3 mValueOffset;			// 位置偏移,计算出的位置会再加上这个偏移作为最终世界坐标
	public float mOffset;
	public float mSpeed;
	public bool mFullOnce;
	public bool mLoop;
	public KEY_FRAME mKeyframe;
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mDoingCallBack = null;
		mDoneCallBack = null;
		mValueOffset = Vector3.zero;
		mOffset = 0.0f;
		mSpeed = 1.0f;
		mLoop = false;
		mFullOnce = false;
		mKeyframe = KEY_FRAME.NONE;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentMovePath component);
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setValueKeyFrame(mValueKeyFrame);
		component.setSpeed(mSpeed);
		component.setValueOffset(mValueOffset);
		component.play((int)mKeyframe, mLoop, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ", mOffset:" + mOffset + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}