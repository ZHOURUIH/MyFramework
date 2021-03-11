using UnityEngine;

public class CommandWindowColor : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public Color mStartColor;
	public Color mTargetColor;
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
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartColor = Color.white;
		mTargetColor = Color.white;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
		obj.getComponent(out WindowComponentColor component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setStart(mStartColor);
		component.setTarget(mTargetColor);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartColor:" + 
			mStartColor + ", mTargetColor:" + mTargetColor + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}
