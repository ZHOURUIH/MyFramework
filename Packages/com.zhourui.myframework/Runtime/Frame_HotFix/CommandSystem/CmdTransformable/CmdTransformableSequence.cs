
// 缓动序列
public class CmdTransformableSequence
{
	public static void execute(ITransformable obj, SequenceCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableSequence com);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.play(obj.tryGetUnityComponent<TweenSequence>());
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
	public static void execute(ITransformable obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.getComponent(out COMTransformableSequence com);
		if (com == null || !com.isActive())
		{
			return;
		}
		com.stop(true);
	}
}