using System;

public class StateParam : FrameBase
{ }

public class CharacterState : FrameBase
{
	protected CharacterBaseData mData;          // 状态所属角色的数据
	protected OnStateLeave mOnLeave;			// 外部可设置的当前状态退出时的回调
	protected StateParam mParam;				// 此参数只能在enter中使用,执行完enter后就会回收销毁
	protected Character mPlayer;                // 状态所属角色
	protected float mStateMaxTime;              // 状态最大持续时间,小于0表示无限制
	protected float mStateTime;                 // 该状态持续的时间,小于0表示无限制
	protected uint mMutexID;                    // 互斥ID,在添加状态时,会判断相同互斥ID的状态是否需要移除,互斥ID相同的状态的互斥操作对应SAME_STATE_OPERATE枚举
	protected uint mID;                         // 状态唯一ID
	protected bool mIgnoreTimeScale;            // 更新时是否忽略时间缩放
	protected bool mActive;                     // 状态是否激活
	protected BUFF_STATE_TYPE mBuffStateType;   // buff类型
	protected STATE_MUTEX mMutexType;			// 该状态是否允许叠加
	public CharacterState()
	{
		mMutexType = STATE_MUTEX.COEXIST;
		mBuffStateType = BUFF_STATE_TYPE.NONE;
		mActive = true;
		mStateMaxTime = -1.0f;
		mStateTime = -1.0f;
	}
	public virtual void destroy() { }
	public void setMutexID(uint mutexID) { mMutexID = mutexID; }
	public uint getMutexID() { return mMutexID; }
	public void setParam(StateParam param) { mParam = param; }
	public void setLeaveCallback(OnStateLeave callback) { mOnLeave = callback; }
	public BUFF_STATE_TYPE getBuffStateType() { return mBuffStateType; }
	public virtual void setPlayer(Character player)
	{
		mPlayer = player;
		mData = mPlayer.getBaseData();
	}
	// 当前是否可以进入该状态
	public virtual bool canEnter() { return true; }
	public virtual void enter() { }
	// 一般在子类的update最后再调用该父类的update,确保在移除状态后不会再执行update
	public virtual void update(float elapsedTime)
	{
		if (mStateTime >= 0.0f)
		{
			mStateTime -= elapsedTime;
			if (mStateTime <= 0.0f)
			{
				mStateTime = -1.0f;
				removeSelf();
			}
		}
	}
	public virtual void fixedUpdate(float elapsedTime) { }
	// isBreak表示是否是因为添加了互斥状态而退出的
	public virtual void leave(bool isBreak, string param)
	{
		mOnLeave?.Invoke(this, isBreak, param);
	}
	public virtual void keyProcess(float elapsedTime) { }
	public void setActive(bool active) { mActive = active; }
	public bool isActive() { return mActive; }
	public void setStateMaxTime(float stateTime) { mStateMaxTime = stateTime; }
	public void setStateTime(float time) { mStateTime = time; }
	public float getStateMaxTime() { return mStateMaxTime; }
	public float getStateTime() { return mStateTime; }
	public uint getID() { return mID; }
	public void setID(uint id) { mID = id; }
	public STATE_MUTEX getMutexType() { return mMutexType; }
	public void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public virtual int getPriority() { return 0; }
	public override void resetProperty()
	{
		base.resetProperty();
		mOnLeave = null;
		mPlayer = null;
		mData = null;
		mParam = null;
		mStateMaxTime = -1.0f;
		mStateTime = -1.0f;
		mActive = true;
		mMutexID = 0;
		mID = 0;
		mBuffStateType = 0;
		// 此处不能重置互斥类型,此字段一般在子类的构造中进行指定,一旦指定就不会改变,也就不会被重置
		// 如果重置,则在复用后互斥类型就会错误
		//mMutexType = STATE_MUTEX.COEXIST;
		mIgnoreTimeScale = false;
	}
	//--------------------------------------------------------------------------------------------------------------
	// 要在当前状态中移除自身,可以使用removeSelf,或者直接将mStateTime设置为0,将时间设置为0最安全
	protected void removeSelf(string param = null)
	{
		CMD(out CmdCharacterRemoveState cmd, false);
		cmd.mState = this;
		cmd.mParam = param;
		pushCommand(cmd, mPlayer);
	}
}