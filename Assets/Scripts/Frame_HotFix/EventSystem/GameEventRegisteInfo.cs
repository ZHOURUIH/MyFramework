using System;

// 游戏事件的注册信息
public class GameEventRegisteInfo : ClassObject
{
	public Type mEventType;				// 事件类型
	public long mCharacterID;			// 事件所属的玩家ID
	public IEventListener mListener;    // 监听者
	public Action mBaseCallback;		// 事件触发时最基础的不带参数的回调
	public override void resetProperty()
	{
		base.resetProperty();
		mEventType = null;
		mCharacterID = 0;
		mListener = null;
		mBaseCallback = null;
	}
	public virtual void call(GameEvent param)
	{
		mBaseCallback?.Invoke();
	}
}