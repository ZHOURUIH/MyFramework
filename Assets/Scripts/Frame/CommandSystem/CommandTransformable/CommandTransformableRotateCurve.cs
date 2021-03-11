using UnityEngine;
using System.Collections.Generic;

public class CommandTransformableRotateCurve : Command
{
	public List<Vector3> mRotList;
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mRotList = null;
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentRotateCurve component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setKeyList(mRotList);
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
			", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}