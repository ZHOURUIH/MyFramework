
// 设置摄像机的FOV
public class CmdGameCameraFOV
{
	// onceLength,变化的持续时间,如果是循环的,则表示单次的时间
	// offsetTime,时间起始偏移量
	// startFOV,起始的FOV
	// targetFOV,终止的FOV
	// keyframe,所使用的关键帧曲线ID
	// loop,是否循环
	// doingCallback,变化中的回调
	// doneCallback,变化结束时的回调
	public static void execute(GameCamera camera, float onceLength, float offsetTime, float startFOV, float targetFOV, 
								int keyframe = 0, bool loop = false, KeyFrameCallback doingCallback = null, KeyFrameCallback doneCallback = null)
	{
		if (camera == null)
		{
			return;
		}
		camera.getOrAddComponent(out COMCameraFOV com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStartFOV(startFOV);
		com.setTargetFOV(targetFOV);
		com.play(keyframe, loop, onceLength, offsetTime);
	}
}