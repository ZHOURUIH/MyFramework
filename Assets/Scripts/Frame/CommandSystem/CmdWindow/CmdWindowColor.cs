using UnityEngine;

public class CmdWindowColor : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public Color mStartColor;
	public Color mTargetColor;
	public float mOnceLength;
	public float mOffset;
	public int mKeyframe;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
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
#if UNITY_EDITOR
		if (!isFloatZero(mOnceLength) && !obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
		}
#endif
		obj.getComponent(out COMWindowColor com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartColor);
		com.setTarget(mTargetColor);
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
				Append(", mStartColor:", mStartColor).
				Append(", mTargetColor:", mTargetColor).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}