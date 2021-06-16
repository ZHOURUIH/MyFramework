using UnityEngine;
using System.Collections.Generic;
using System;

public class Character : MovableObject
{
	protected COMCharacterStateMachine mStateMachine;			// 状态机组件
	protected COMCharacterDecisionTree mDecisionTree;			// 决策树组件
	protected COMCharacterAvatar mAvatar;						// 模型组件
	protected CharacterBaseData mBaseData;						// 玩家数据
	protected Type mCharacterType;								// 角色类型
	protected long mGUID;										// 角色的唯一ID
	protected bool mIsMyself;									// 是否为主角实例,为了提高效率,不使用虚函数判断
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
		mBaseData = null;
		mAvatar = null;
		mStateMachine = null;
		mDecisionTree = null;
		mGUID = 0;
		mCharacterType = null;
		// mIsMyself不重置
		// mIsMyself = false;
	}
	// 异步加载模型
	public void initModelAsync(string modelPath, OnCharacterLoaded callback = null, object userData = null, string animationControllerPath = null)
	{
		mAvatar.loadModelAsync(modelPath, callback, userData, animationControllerPath);
	}
	// 同步加载模型
	public void initModel(string modelPath, string animationControllerPath = null)
	{
		mAvatar.loadModel(modelPath, animationControllerPath);
	}
	public virtual void destroyModel()
	{
		mAvatar.destroyModel();
		// 由于模型节点已经销毁,所以要重新分配一个新的节点给角色,这个节点可以自动销毁
		GameObject charNode = createGameObject(getName(), mCharacterManager.getObject());
		setObject(charNode, false);
		setDestroyObject(true);
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
	//------------------------------------------------------------------------------------------------------------------------------------------------------
	// set
	public void setCharacterType(Type type)					{ mCharacterType = type; }
	public void setID(long id)								{ mGUID = id; }
	//------------------------------------------------------------------------------------------------------------------------------------------------------
	// get
	public bool isMyself()									{ return mIsMyself; }
	public CharacterBaseData getBaseData()					{ return mBaseData; }
	public Type getType()									{ return mCharacterType; }
	public COMCharacterAvatar getAvatar()					{ return mAvatar; }
	public Animation getAnimation()							{ return mAvatar.getAnimation(); }
	public Animator getAnimator()							{ return mAvatar.getAnimator(); }
	public Rigidbody getRigidBody()							{ return mAvatar.getRigidBody(); }
	public long getGUID()									{ return mGUID; }
	public COMCharacterDecisionTree getDecisionTree()		{ return mDecisionTree; }
	public COMCharacterStateMachine getStateMachine()		{ return mStateMachine; }
	public CharacterState getFirstGroupState(Type group)	{ return mStateMachine.getFirstGroupState(group); }
	public CharacterState getFirstState(Type type)			{ return mStateMachine.getFirstState(type); }
	public CharacterState getState(uint id)					{ return mStateMachine.getState(id); }
	public SafeDeepDictionary<Type, SafeDeepList<CharacterState>> getStateList() { return mStateMachine.getStateList(); }
	public bool hasState(Type state)						{ return mStateMachine.hasState(state); }
	public bool hasStateGroup(Type group)					{ return mStateMachine.hasStateGroup(group); }
	//-------------------------------------------------------------------------------------------------------------------------------------------------------
	protected virtual CharacterBaseData createCharacterData() { return new CharacterBaseData(); }
	protected override void initComponents()
	{
		base.initComponents();
		mAvatar = addComponent(typeof(COMCharacterAvatar), true) as COMCharacterAvatar;
		mStateMachine = addComponent(typeof(COMCharacterStateMachine), true) as COMCharacterStateMachine;
		mDecisionTree = addComponent(typeof(COMCharacterDecisionTree)) as COMCharacterDecisionTree;
	}
}