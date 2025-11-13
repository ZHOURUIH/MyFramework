
// 改变物体的音量
public class CmdMovableObjectVolume
{
	// 变化中回调
	// 变化完成时回调
	// 起始音量
	// 目标音量
	// 持续时间
	// 起始时间偏移
	// 使用的关键帧曲线的ID
	// 是否循环
	public static void execute(MovableObject obj, float startVolume, float targetVolume, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.getOrAddComponent(out COMMovableObjectVolume com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startVolume);
		com.setTarget(targetVolume);
		com.play(keyframe, loop, onceLength, offset);
	}
}