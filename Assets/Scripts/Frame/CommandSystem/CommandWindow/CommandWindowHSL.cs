using UnityEngine;

public class CommandWindowHSL : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public Vector3 mStartHSL;
	public Vector3 mTargetHSL;
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
		mStartHSL = Vector3.zero;
		mTargetHSL = Vector3.zero;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
		obj.getComponent(out WindowComponentHSL component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setStartHSL(mStartHSL);
		component.setTargetHSL(mTargetHSL);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartHSL:" + mStartHSL
			+ ", mTargetHSL:" + mTargetHSL + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}
