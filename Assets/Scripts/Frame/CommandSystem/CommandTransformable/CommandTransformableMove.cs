﻿using UnityEngine;

public class CommandTransformableMove : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public Vector3 mStartPos;
	public Vector3 mTargetPos;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mStartPos = Vector3.zero;
		mTargetPos = Vector3.zero;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentMove component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setTargetPos(mTargetPos);
		component.setStartPos(mStartPos);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + 
			", mStartPos:" + mStartPos + ", mTargetPos:" + mTargetPos + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}