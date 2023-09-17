using UnityEngine;
using System;
using static FrameBase;
using static FrameUtility;

// 所有角色的基类,提供基础的角色行为
public class Character : MovableObject
{
	protected COMCharacterStateMachine mStateMachine;			// 状态机组件
	protected COMCharacterAnimation mCOMAnimation;				// 动作逻辑处理的组件
	protected COMCharacterAvatar mAvatar;						// 模型组件
	protected CharacterData mBaseData;							// 玩家数据
	protected Type mCharacterType;								// 角色类型
	protected long mGUID;										// 角色的唯一ID
	protected bool mIsMyself;									// 是否为主角实例,为了提高效率,不使用虚函数判断
	protected bool mCharacterSelfObject;						// 当前的mObject是否为当前类中所创建的节点
	public Character()
	{}
	public override void init()
	{
		mBaseData = createCharacterData();
		mBaseData.mName = mName;
		mBaseData.mGUID = mGUID;
		base.init();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCOMAnimation = null;
		mBaseData = null;
		mAvatar = null;
		mStateMachine = null;
		mGUID = 0;
		mCharacterType = null;
		mCharacterSelfObject = false;
		// mIsMyself不重置
		// mIsMyself = false;
	}
	public override void destroy()
	{
		// 只有当节点是当前类创建的才需要销毁
		if (mCharacterSelfObject)
		{
			mGameObjectPool.destroyObject(mObject);
		}
		base.destroy();
	}
	public override void setObject(GameObject obj)
	{
		// 将角色节点设置为空,也就意味着角色节点将不再是最初创建的节点了,也就不应该在最后销毁时将此节点销毁
		if (obj == null)
		{
			mCharacterSelfObject = false;
		}
		base.setObject(obj);
	}
	// 异步加载模型
	public void initModelAsync(string modelPath, OnCharacterLoaded callback = null, object userData = null, string animationControllerPath = null)
	{
		mAvatar?.loadModelAsync(modelPath, callback, userData, animationControllerPath);
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
			createCharacterNode();
		}
	}
	public void createCharacterNode()
	{
		mCharacterSelfObject = true;
		setObject(mGameObjectPool.newObject(getName(), mCharacterManager.getObject()));
	}
	// 参数是动作名,不是状态机节点名
	public virtual float getAnimationLength(string name)
	{
		if (mAvatar == null)
		{
			return 0.0f;
		}
		return mAvatar.getAnimationLength(name);
	}
	public virtual void notifyModelLoaded() { }
	//------------------------------------------------------------------------------------------------------------------------------
	// set
	public void setCharacterType(Type type)					{ mCharacterType = type; }
	public void setID(long id)								{ mGUID = id; }
	//------------------------------------------------------------------------------------------------------------------------------
	// get
	public bool isMyself()									{ return mIsMyself; }
	public CharacterData getBaseData()						{ return mBaseData; }
	public Type getType()									{ return mCharacterType; }
	public COMCharacterAvatar getAvatar()					{ return mAvatar; }
	public COMCharacterAnimation getCOMAnimation()			{ return mCOMAnimation; }
	public Animation getAnimation()							{ return mAvatar?.getAnimation(); }
	public Animator getAnimator()							{ return mAvatar?.getAnimator(); }
	public Rigidbody getRigidBody()							{ return mAvatar?.getRigidBody(); }
	public long getGUID()									{ return mGUID; }
	public COMCharacterStateMachine getStateMachine()
	{
		if (mStateMachine == null)
		{
			addComponent(out mStateMachine, true);
		}
		return mStateMachine;
	}
	public CharacterState getFirstGroupState(Type group)	{ return getStateMachine()?.getFirstGroupState(group); }
	public CharacterState getFirstState(Type type)			{ return getStateMachine()?.getFirstState(type); }
	public CharacterState getState(long instanceID)			{ return getStateMachine()?.getState(instanceID); }
	public SafeDictionary<Type, SafeList<CharacterState>> getStateList() { return getStateMachine()?.getStateList(); }
	public bool hasState(Type state)						{ return mStateMachine != null && mStateMachine.hasState(state); }
	public bool hasStateGroup(Type group)					{ return mStateMachine != null && mStateMachine.hasStateGroup(group); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual CharacterData createCharacterData()
	{
		CLASS(out mBaseData);
		return mBaseData;
	}
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mAvatar, true);
		addInitComponent(out mStateMachine, true);
		addInitComponent(out mCOMAnimation, true);
	}
}