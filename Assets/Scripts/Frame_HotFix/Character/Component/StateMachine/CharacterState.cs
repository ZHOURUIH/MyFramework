using System.Collections.Generic;
using static FrameUtility;
using static CSharpUtility;
using static FrameBase;

// 角色状态基类
public class CharacterState : ClassObject
{
	protected Dictionary<IEventListener, List<CharacterStateCallback>> mWillRemoveCallbackList;  // 即将销毁此状态时的回调列表,不一定所有状态都需要这个,需要时才创建
	protected OnStateLeave mOnLeave;			// 外部可设置的当前状态退出时的回调
	private StateParam mParam;					// 此参数只能在enter中使用,执行完enter后就会回收销毁,为了避免类型转换问题,设置为私有的
	protected Character mCharacter;				// 状态所属角色
	protected long mID;							// 状态唯一ID
	protected float mStateMaxTime;				// 状态最大持续时间,小于0表示无限制,实际只是一个数值备份
	protected float mStateTime;					// 该状态持续的时间,小于0表示无限制,由于需要确保状态从进入到移除的时间一定大于mStateMaxTime,所以在设置状态时间时会有一定的修正
	protected int mMutexID;						// 互斥ID,在添加状态时,会判断相同互斥ID的状态是否需要移除,互斥ID相同的状态的互斥操作对应SAME_STATE_OPERATE枚举
	protected bool mIgnoreTimeScale;			// 更新时是否忽略时间缩放
	protected bool mActive;                     // 状态是否激活
	protected bool mJustEnter;                  // 是否是这一帧进入的状态,用于避免进入后的同一帧进行第一次的update而导致的时间计算错误
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
	public override void destroy()
	{
		base.destroy();
		foreach (var item in mWillRemoveCallbackList.safe().Values)
		{
			UN_LIST(item);
		}
		mWillRemoveCallbackList?.Clear();
	}
	public virtual void setCharacter(Character character) { mCharacter = character; }
	// 当前是否可以进入该状态
	public virtual bool canEnter() { return true; }
	public virtual void enter() { }
	// 一般在子类的update最后再调用该父类的update,确保在移除状态后不会再执行update
	public virtual void update(float elapsedTime)
	{
		if (tickTimerOnce(ref mStateTime, elapsedTime))
		{
			removeSelf();
		}
	}
	public virtual void fixedUpdate(float elapsedTime) { }
	// isBreak表示是否是因为添加了互斥状态而退出的,willDestroy表示是否销毁了角色
	public virtual void leave(bool isBreak, bool willDestroy, string param)
	{
		mOnLeave?.Invoke(this, isBreak, willDestroy, param);
		mEventSystem?.unlistenEvent(this);
	}
	public virtual void addSameState(CharacterState newState) { }
	public void setMutexID(int mutexID)					{ mMutexID = mutexID; }
	public virtual void setParam(StateParam param)		{ mParam = param; }
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
	public Character getCharacter()						{ return mCharacter; }
	public virtual int getPriority()					{ return 0; }
	public StateParam getParam()						{ return mParam; }
	public override void resetProperty()
	{
		base.resetProperty();
		mWillRemoveCallbackList = null;
		mOnLeave = null;
		mCharacter = null;
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
	// 添加即将移除此状态的回调监听
	public void addWillRemoveCallback(IEventListener listener, CharacterStateCallback callback) 
	{
		mWillRemoveCallbackList ??= new();
		mWillRemoveCallbackList.tryGetOrAddListPersist(listener).Add(callback); 
	}
	// 移除回调监听,只能移除此监听者的所有监听
	public void removeWillRemoveCallback(IEventListener listener) 
	{
		if (mWillRemoveCallbackList != null && mWillRemoveCallbackList.Remove(listener, out var list))
		{
			UN_LIST(ref list);
		}
	}
	public void callWillRemoveCallback()
	{
		foreach (var item in mWillRemoveCallbackList.safe().Values)
		{
			foreach (CharacterStateCallback callback in item)
			{
				callback(this);
			}
			UN_LIST(item);
		}
		mWillRemoveCallbackList?.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 要在当前状态中移除自身,可以使用removeSelf,或者直接将mStateTime设置为0,将时间设置为0最安全
	protected void removeSelf(string param = null)
	{
		mCharacter?.getStateMachine().removeState(this, false, param);
	}
}