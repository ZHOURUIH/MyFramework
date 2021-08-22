using UnityEngine;

public class CmdTransformableRotatePhysics : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public Vector3 mStartRotation;
	public Vector3 mTargetRotation;
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
		mStartRotation = Vector3.zero;
		mTargetRotation = Vector3.zero;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (!isFloatZero(mOnceLength) && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableRotatePhysics com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setTarget(mTargetRotation);
		com.setStart(mStartRotation);
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
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce).
				Append(", mStartRotation:", mStartRotation).
				Append(", mTargetRotation:", mTargetRotation);
	}
}