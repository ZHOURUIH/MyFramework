using static FrameBaseHotFix;

// 播放一个物体的音频
public class CmdMovableObjectPlayAudio
{
	// 音效文件名,带相对于Sound的路径,带后缀
	// 播放的音量
	// 音效ID,如果音效ID有效,则根据ID进行播放,否则根据音效文件名播放
	// 是否循环
	public static void execute(ComponentOwner obj, string soundFileName, float volume, bool loop)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMMovableObjectAudio com);
		com.setActive(true);
		com.play(soundFileName, loop, volume);
	}
	public static void execute(ComponentOwner obj)
	{
		if (obj == null)
		{
			return;
		}
		var com = obj.getComponent<COMMovableObjectAudio>();
		com?.stop();
		com?.setActive(false);
	}
}