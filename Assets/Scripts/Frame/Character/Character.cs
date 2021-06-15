using UnityEngine;
using System.Collections.Generic;
using System;

public class Character : MovableObject
{
	protected Dictionary<string, float> mAnimationLenghtList;
	protected COMCharacterStateMachine mStateMachine;
	protected COMCharacterDecisionTree mDecisionTree;
	protected CreateObjectCallback mModelLoadCallback;
	protected OnCharacterLoaded mCharacterLoadedCallback;
	protected COMCharacterAvatar mAvatar;
	protected CharacterBaseData mBaseData;  //玩家数据
	protected Rigidbody mRigidBody;
	protected Type mCharacterType;          // 角色类型
	protected string mModelPath;
	protected string mAnimationControllerPath;
	protected object mUserData;
	protected long mGUID;
	protected int mModelTag;
	protected bool mIsMyself;               // 是否为主角实例,为了提高效率,不使用虚函数判断
	public Character()
	{
		mAnimationLenghtList = new Dictionary<string, float>();
		mModelLoadCallback = onModelLoaded;
	}
	protected virtual CharacterBaseData createCharacterData() { return new CharacterBaseData(); }
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
		mRigidBody = null;
		mStateMachine = null;
		mDecisionTree = null;
		mGUID = 0;
		mAnimationLenghtList.Clear();
		mCharacterType = null;
		mCharacterLoadedCallback = null;
		mModelPath = null;
		mAnimationControllerPath = null;
		mUserData = null;
		mModelTag = 0;
		// mModelLoadCallback,mIsMyself不重置
		// mModelLoadCallback = null;
		// mIsMyself = false;
	}
	public void setCharacterType(Type type) { mCharacterType = type; }
	public bool isMyself() { return mIsMyself; }
	public void setID(long id) { mGUID = id; }
	// 异步加载模型
	public void initModelAsync(string modelPath, OnCharacterLoaded callback = null, object userData = null, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimationControllerPath = animationControllerPath;
		mCharacterLoadedCallback = callback;
		mUserData = userData;
		if (!isEmpty(modelPath))
		{
			// 模型节点也就是角色节点,并且将节点挂到角色管理器下
			mObjectPool.createObjectAsync(mModelPath, mModelLoadCallback, mModelTag);
		}
	}
	// 同步加载模型
	public void initModel(string modelPath, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimationControllerPath = animationControllerPath;
		mCharacterLoadedCallback = null;
		mUserData = null;
		if (!isEmpty(modelPath))
		{
			// 模型节点也就是角色节点,并且将节点挂到角色管理器下
			onModelLoaded(mObjectPool.createObject(mModelPath, mModelTag), null);
			afterModelLoaded();
		}
	}
	public virtual void destroyModel()
	{
		mRigidBody = null;
		mAvatar.destroyModel();
		// 由于模型节点已经销毁,所以要重新分配一个新的节点给角色,这个节点可以自动销毁
		GameObject charNode = createGameObject(getName(), mCharacterManager.getObject());
		setObject(charNode, false);
		setDestroyObject(true);
	}
	// 参数是动作名,不是状态机节点名
	public virtual float getAnimationLength(string name)
	{
		if (mAvatar == null || mAvatar.getAnimator() == null || isEmpty(name))
		{
			return 0.0f;
		}
		if (mAnimationLenghtList.TryGetValue(name, out float length))
		{
			return length;
		}
		length = getAnimationLength(mAvatar.getAnimator(), name);
		mAnimationLenghtList.Add(name, length);
		return length;
	}
	public virtual void notifyComponentChanged(GameComponent com) { }
	public CharacterBaseData getBaseData() { return mBaseData; }
	public Type getType() { return mCharacterType; }
	public COMCharacterAvatar getAvatar() { return mAvatar; }
	public Animation getAnimation() { return mAvatar.getAnimation(); }
	public Animator getAnimator() { return mAvatar.getAnimator(); }
	public Rigidbody getRigidBody() { return mRigidBody; }
	public long getGUID() { return mGUID; }
	public COMCharacterDecisionTree getDecisionTree() { return mDecisionTree; }
	public COMCharacterStateMachine getStateMachine() { return mStateMachine; }
	public CharacterState getFirstGroupState(Type group) { return mStateMachine.getFirstGroupState(group); }
	public CharacterState getFirstState(Type type) { return mStateMachine.getFirstState(type); }
	public CharacterState getState(uint id) { return mStateMachine.getState(id); }
	public SafeDeepDictionary<Type, SafeDeepList<CharacterState>> getStateList() { return mStateMachine.getStateList(); }
	public bool hasState(Type state) { return mStateMachine.hasState(state); }
	public bool hasStateGroup(Type group) { return mStateMachine.hasStateGroup(group); }
	//-------------------------------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		mAvatar = addComponent(typeof(COMCharacterAvatar), true) as COMCharacterAvatar;
		mStateMachine = addComponent(typeof(COMCharacterStateMachine), true) as COMCharacterStateMachine;
		mDecisionTree = addComponent(typeof(COMCharacterDecisionTree)) as COMCharacterDecisionTree;
	}
	protected void onModelLoaded(GameObject go, object userData)
	{
		notifyModelLoaded(go);
		afterModelLoaded();
	}
	protected virtual void notifyModelLoaded(GameObject go)
	{
		mAvatar.notifyModelLoaded(go, mAnimationControllerPath);
		mRigidBody = go.GetComponent<Rigidbody>();
	}
	protected void afterModelLoaded()
	{
		mCharacterLoadedCallback?.Invoke(this, mUserData);
		mCharacterLoadedCallback = null;
		mUserData = null;
	}
}