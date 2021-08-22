using System;

public class CmdWindowLum : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public float mOnceLength;
	public float mOffset;
	public float mStartLum;
	public float mTargetLum;
	public bool mFullOnce;
	public bool mLoop;
	public int mKeyframe;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartLum = 0.0f;
		mTargetLum = 0.0f;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
#if UNITY_EDITOR
		if (!isFloatZero(mOnceLength) && !obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
		}
#endif
		obj.getComponent(out COMWindowLum com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartLum);
		com.setTarget(mTargetLum);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartLum:", mStartLum).
				Append(", mTargetLum:", mTargetLum).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}