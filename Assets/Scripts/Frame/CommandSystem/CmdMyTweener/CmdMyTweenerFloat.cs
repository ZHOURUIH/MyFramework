using System;

public class CmdMyTweenerFloat : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public float mStartAlpha;
	public float mTargetAlpha;
	public float mOnceLength;
	public float mOffset;
	public int mKeyframeID;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mKeyframeID = KEY_CURVE.NONE;
		mStartAlpha = 1.0f;
		mTargetAlpha = 1.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		obj.getComponent(out COMMyTweenerFloat com);
		com.setDoingCallback(mTremblingCallBack);
		com.setDoneCallback(mTrembleDoneCallBack);
		com.setActive(true);
		com.setStart(mStartAlpha);
		com.setTarget(mTargetAlpha);
		com.play(mKeyframeID, mLoop, mOnceLength, mOffset, mFullOnce);
	}
}