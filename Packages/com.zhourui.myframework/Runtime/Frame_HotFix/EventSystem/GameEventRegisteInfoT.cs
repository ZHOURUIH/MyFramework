using System;

// 游戏事件的注册信息,带事件类型参数
public class GameEventRegisteInfoT<T> : GameEventRegisteInfo where T : GameEvent
{
	public Action<T> mCallback;			// 监听事件的回调函数
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
	}
	public override void call(GameEvent param)
	{
		base.call(param);
		mCallback?.Invoke(param as T);
	}
}