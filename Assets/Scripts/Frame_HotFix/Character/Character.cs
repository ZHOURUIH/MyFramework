using UnityEngine;
using System;
using static FrameBaseHotFix;

// 所有角色的基类,提供基础的角色行为
public class Character : MovableObject
{
	protected COMCharacterStateMachine mStateMachine;	// 状态机组件
	protected COMCharacterAnimation mCOMAnimation;		// 动作逻辑处理的组件
	protected COMCharacterAvatar mAvatar;				// 模型组件
	protected Type mCharacterType;						// 角色类型
	protected long mGUID;								// 角色的唯一ID
	protected bool mIsMyself;							// 是否为主角实例,为了提高效率,不使用虚函数判断
	public override void resetProperty()
	{
		base.resetProperty();
		mCOMAnimation = null;
		mAvatar = null;
		mStateMachine = null;
		mGUID = 0;
		mCharacterType = null;
		// mIsMyself不重置
		// mIsMyself = false;
	}
	// 异步加载模型
	public void initModelAsync(string modelPath, CharacterCallback callback = null, string animationControllerPath = null)
	{
		mAvatar?.loadModelAsync(modelPath, callback, animationControllerPath);
	}
	// 同步加载模型
	public void initModel(string modelPath, string animationControllerPath = null)
	{
		mAvatar?.loadModel(modelPath, animationControllerPath);
	}
	public virtual void destroyModel()
	{
		mAvatar?.destroyModel();
		// 如果销毁模型后角色节点为空了,则需要创建一个新的角色节点
		if (mObject == null)
		{
			selfCreateObject(getName(), mCharacterManager.getObject());
		}
	}
	// 参数是动作名,不是状态机节点名
	public virtual float getAnimationLength(string name) { return mAvatar?.getAnimationLength(name) ?? 0.0f; }
	public virtual void notifyModelLoaded() { }
	//------------------------------------------------------------------------------------------------------------------------------
	// set
	public void setCharacterType(Type type)					{ mCharacterType = type; }
	public void setID(long id)								{ mGUID = id; }
	//------------------------------------------------------------------------------------------------------------------------------
	// get
	public bool isMyself()									{ return mIsMyself; }
	public Type getType()									{ return mCharacterType; }
	public COMCharacterAvatar getAvatar()					{ return mAvatar; }
	public COMCharacterAnimation getCOMAnimation()			{ return mCOMAnimation; }
	public Animator getAnimator()							{ return mAvatar?.getAnimator(); }
	public Rigidbody getRigidBody()							{ return mAvatar?.getRigidBody(); }
	public long getGUID()									{ return mGUID; }
	public COMCharacterStateMachine getStateMachine()
	{
		if (isDestroy())
		{
			return null;
		}
		if (mStateMachine == null)
		{
			addComponent(out mStateMachine, true);
		}
		return mStateMachine;
	}
	public CharacterState getFirstGroupState(Type group)	{ return getStateMachine()?.getFirstGroupState(group); }
	public CharacterState getFirstGroupState<T>()			{ return getStateMachine()?.getFirstGroupState(typeof(T)); }
	public CharacterState getFirstState(Type type)			{ return getStateMachine()?.getFirstState(type); }
	public T getFirstState<T>() where T : CharacterState { return getStateMachine()?.getFirstState(typeof(T)) as T; }
	public CharacterState getState(long instanceID)			{ return getStateMachine()?.getState(instanceID); }
	public SafeDictionary<Type, SafeList<CharacterState>> getStateList() { return getStateMachine()?.getStateList(); }
	public CharacterState addState(Type type, StateParam param = null, float stateTime = -1.0f, long id = 0)
	{
		return mStateMachine?.addState(type, param, stateTime, id); 
	}
	public T addState<T>(StateParam param = null, float stateTime = -1.0f, long id = 0) where T : CharacterState 
	{
		return getStateMachine()?.addState<T>(param, stateTime, id) as T; 
	}
	public bool hasState(Type state)						{ return mStateMachine != null && mStateMachine.hasState(state); }
	public bool hasState<T>() where T : CharacterState		{ return mStateMachine != null && mStateMachine.hasState(typeof(T)); }
	public bool hasStateGroup(Type group)					{ return mStateMachine != null && mStateMachine.hasStateGroup(group); }
	public bool hasStateGroup<T>() where T : StateGroup		{ return mStateMachine != null && mStateMachine.hasStateGroup(typeof(T)); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mAvatar, true);
		addInitComponent(out mStateMachine, true);
		addInitComponent(out mCOMAnimation, true);
	}
}