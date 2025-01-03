
// 改变物体的音量
public class CmdMovableObjectVolume : Command
{
	public KeyFrameCallback mDoingCallback;	// 变化中回调
	public KeyFrameCallback mDoneCallback;	// 变化完成时回调
	public float mStartVolume;				// 起始音量
	public float mTargetVolume;				// 目标音量
	public float mOnceLength;				// 持续时间
	public float mOffset;					// 起始时间偏移
	public int mKeyframe;					// 使用的关键帧曲线的ID
	public bool mLoop;						// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
		mStartVolume = 0.0f;
		mTargetVolume = 0.0f;
		mOnceLength = 0.0f;
		mOffset = 0.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as MovableObject;
		obj.getOrAddComponent(out COMMovableObjectVolume com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartVolume);
		com.setTarget(mTargetVolume);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mStartVolume:", mStartVolume).
				append(", mTargetVolume:", mTargetVolume).
				append(", mLoop:", mLoop);
	}
}