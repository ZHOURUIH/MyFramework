using System.Collections.Generic;

// 以指定的路径渐变窗口透明度
public class CmdWindowAlphaPath : Command
{
	public Dictionary<float, float> mValueKeyFrame;	// 透明度和时间的关键帧列表
	public KeyFrameCallback mDoingCallBack;			// 变化中回调
	public KeyFrameCallback mDoneCallBack;			// 变化完成时回调
	public float mValueOffset;						// 透明偏移,计算出的值会再加上这个偏移作为最终透明
	public float mOffset;							// 起始时间偏移
	public float mSpeed;							// 所使用的关键帧ID
	public bool mLoop;								// 是否循环
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
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
#if UNITY_EDITOR
		if (mValueKeyFrame != null && !obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
		}
#endif
		obj.getComponent(out COMWindowAlphaPath com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(mValueKeyFrame);
		com.setSpeed(mSpeed);
		com.setValueOffset(mValueOffset);
		com.play(mLoop, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(", mOffset:", mOffset).
				append(", mLoop:", mLoop);
	}
}