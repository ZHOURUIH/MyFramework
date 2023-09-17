using System;
using static FrameUtility;

// 状态参数的基类
public class StateParam : ClassObject
{
	public float mBuffTime;     // 只用作参数存储,不会在buff中引用
	// 只会在第一次构造时调用一次,复用对象时不会再调用
	public virtual void init() { }
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffTime = 0.0f;
	}
}

// 角色状态基类
public class CharacterState : ClassObject
{
	protected CharacterData mData;				// 状态所属角色的数据
	protected OnStateLeave mOnLeave;			// 外部可设置的当前状态退出时的回调
	protected StateParam mParam;				// 此参数只能在enter中使用,执行完enter后就会回收销毁
	protected Character mCharacter;				// 状态所属角色
	protected long mID;							// 状态唯一ID
	protected float mStateMaxTime;				// 状态最大持续时间,小于0表示无限制
	protected float mStateTime;					// 该状态持续的时间,小于0表示无限制,由于需要确保状态从进入到移除的时间一定大于mStateMaxTime,所以在设置状态时间时会有一定的修正
	protected int mMutexID;						// 互斥ID,在添加状态时,会判断相同互斥ID的状态是否需要移除,互斥ID相同的状态的互斥操作对应SAME_STATE_OPERATE枚举
	protected bool mIgnoreTimeScale;			// 更新时是否忽略时间缩放
	protected bool mActive;                     // 状态是否激活
	protected bool mJustEnter;					// 是否是这一帧进入的状态,用于避免进入后的同一帧进行第一次的update而导致的时间计算错误
	protected BUFF_STATE_TYPE mBuffStateType;	// buff类型
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
	public virtual void setCharacter(Character character)
	{
		mCharacter = character;
		mData = mCharacter.getBaseData();
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
	public virtual void keyProcess(float elapsedTime)	{ }
	public void setMutexID(int mutexID)					{ mMutexID = mutexID; }
	public void setParam(StateParam param)				{ mParam = param; }
	public void setLeaveCallback(OnStateLeave callback) { mOnLeave = callback; }
	public void setActive(bool active)					{ mActive = active; }
	public void setStateMaxTime(float stateTime)		{ mStateMaxTime = stateTime; }
	public void setStateTime(float time)				{ mStateTime = time; }
	public void setID(long id)							{ mID = id; }
	public void setIgnoreTimeScale(bool ignore)			{ mIgnoreTimeScale = ignore; }
	public void setJustEnter(bool just)					{ mJustEnter = just; }
	public int getMutexID()								{ return mMutexID; }
	public BUFF_STATE_TYPE getBuffStateType()			{ return mBuffStateType; }
	public bool isActive()								{ return mActive; }
	public float getStateMaxTime()						{ return mStateMaxTime; }
	public float getStateTime()							{ return mStateTime; }
	public long getID()									{ return mID; }
	public STATE_MUTEX getMutexType()					{ return mMutexType; }
	public bool isIgnoreTimeScale()						{ return mIgnoreTimeScale; }
	public bool isJustEnter()							{ return mJustEnter; }
	public virtual int getPriority()					{ return 0; }
	public override void resetProperty()
	{
		base.resetProperty();
		mOnLeave = null;
		mCharacter = null;
		mData = null;
		mParam = null;
		mID = 0;
		mStateMaxTime = -1.0f;
		mStateTime = -1.0f;
		mMutexID = 0;
		mIgnoreTimeScale = false;
		mActive = true;
		mJustEnter = false;
		mBuffStateType = BUFF_STATE_TYPE.NONE;
		// 此处不能重置互斥类型,此字段一般在子类的构造中进行指定,一旦指定就不会改变,也就不会被重置
		// 如果重置,则在复用后互斥类型就会错误
		// mMutexType = STATE_MUTEX.COEXIST;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 要在当前状态中移除自身,可以使用removeSelf,或者直接将mStateTime设置为0,将时间设置为0最安全
	protected void removeSelf(string param = null)
	{
		if (mCharacter == null)
		{
			return;
		}
		CmdCharacterRemoveState.execute(mCharacter, this, param);
	}
}