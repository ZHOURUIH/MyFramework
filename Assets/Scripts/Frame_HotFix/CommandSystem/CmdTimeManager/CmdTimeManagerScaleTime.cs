using static FrameBaseHotFix;

// 渐变时间缩放
public class CmdTimeManagerScaleTime
{
	// 变化中回调
	// 变化完成时回调
	// 起始缩放值
	// 目标缩放值
	// 单次所需时间
	// 起始时间偏移
	// 使用的关键帧曲线ID
	// 是否循环
	public static void execute(float startScale, float targetScale, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		mTimeManager.getOrAddComponent(out COMTimeScale com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startScale);
		com.setTarget(targetScale);
		com.play(keyframe, loop, onceLength, offset);
	}
	public static void execute(float targetScale)
	{
		mTimeManager.getOrAddComponent(out COMTimeScale com);
		com.setStart(targetScale);
		com.setTarget(targetScale);
		com.play(0, false, 0.0f, 0.0f);
	}
}