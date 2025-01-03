using System.Collections.Generic;

// 按一个指定的时间与透明度的列表来变化透明度
public class CmdMovableObjectAlphaPath : Command
{
	public Dictionary<float, float> mValueKeyFrame;	// 透明度与时间的关键帧列表
	public KeyFrameCallback mDoingCallBack;			// 变化中回调
	public KeyFrameCallback mDoneCallBack;			// 变化完成时回调
	public float mValueOffset;						// 位置偏移,计算出的位置会再加上这个偏移作为最终透明
	public float mOffset;							// 起始时间偏移
	public float mSpeed;							// 变化速度
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
		var obj = mReceiver as ComponentOwner;
		obj.getOrAddComponent(out COMMovableObjectAlphaPath com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(mValueKeyFrame);
		com.setSpeed(mSpeed);
		com.setValueOffset(mValueOffset);
		com.play(mLoop, mOffset);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(", mOffset:", mOffset).
				append(", mValueOffset:", mValueOffset).
				append(", mSpeed:", mSpeed).
				append(", mLoop:", mLoop);
	}
}