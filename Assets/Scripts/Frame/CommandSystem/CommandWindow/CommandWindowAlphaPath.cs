using System.Collections.Generic;

public class CommandWindowAlphaPath : Command
{
	public Dictionary<float, float> mValueKeyFrame;
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public float mValueOffset;			// 透明偏移,计算出的值会再加上这个偏移作为最终透明
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
		mLoop = false;
		mFullOnce = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
		obj.getComponent(out WindowComponentAlphaPath component);
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setValueKeyFrame(mValueKeyFrame);
		component.setSpeed(mSpeed);
		component.setValueOffset(mValueOffset);
		component.play(mLoop, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(", mOffset:", mOffset).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}