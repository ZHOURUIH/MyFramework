using System.Collections.Generic;

public class CmdMovableObjectAlphaPath : Command
{
	public Dictionary<float, float> mValueKeyFrame;
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public float mValueOffset;          // 位置偏移,计算出的位置会再加上这个偏移作为最终透明
	public float mOffset;
	public float mSpeed;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mDoingCallBack = null;
		mDoneCallBack = null;
		mOffset = 0.0f;
		mSpeed = 1.0f;
		mValueOffset = 1.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as ComponentOwner;
		obj.getComponent(out COMMovableObjectAlphaPath com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(mValueKeyFrame);
		com.setSpeed(mSpeed);
		com.setValueOffset(mValueOffset);
		com.play(mLoop, mOffset, mFullOnce);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(", mOffset:", mOffset).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}