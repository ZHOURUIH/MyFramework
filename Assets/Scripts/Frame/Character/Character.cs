using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface ICharacterMyself
{ }

public class Character : MovableObject
{
	protected Dictionary<string, float> mAnimationLenghtList;
	protected CharacterStateMachine mStateMachine;
	protected CharacterDecisionTree mDecisionTree;
	protected CharacterComponentModel mAvatar;
	protected CharacterBaseData	mBaseData;	//玩家数据
	protected Rigidbody mRigidBody;
	protected Type mCharacterType;			// 角色类型
	protected uint mGUID;
	protected string mModelTag;
	protected string mModelPath;
	protected string mAnimationControllerPath;
	protected OnCharacterLoaded mCharacterLoadedCallback;
	protected object mUserData;
	public Character(string name)
		:base(name)
	{
		mAnimationLenghtList = new Dictionary<string, float>();
	}
	protected virtual CharacterBaseData createCharacterData(){return new CharacterBaseData();}
	public void setCharacterType(Type type) { mCharacterType = type; }
	public void setID(uint id){mGUID = id;}
	public override void init()
	{
		mBaseData = createCharacterData();
		mBaseData.mName = mName;
		mBaseData.mGUID = mGUID;
		base.init();
	}
	public override void initComponents()
	{
		base.initComponents();
		mAvatar = addComponent<CharacterComponentModel>(true);
		mStateMachine = addComponent<CharacterStateMachine>(true);
		mDecisionTree = addComponent<CharacterDecisionTree>();
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
	}
	public override void update(float elapsedTime)
	{
		// 先更新自己的所有组件
		base.update(elapsedTime);
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
	}
	public void initModelAsync(string modelPath, OnCharacterLoaded callback, object userData, string animationControllerPath = EMPTY_STRING)
	{
		mModelPath = modelPath;
		mAnimationControllerPath = animationControllerPath;
		mCharacterLoadedCallback = callback;
		mUserData = userData;
		if (modelPath.Length != 0)
		{
			// 模型节点也就是角色节点,并且将节点挂到角色管理器下
			mObjectPool.createObjectAsync(mModelPath, onModelLoaded, mModelTag, null);
		}
	}
	public void initModel(string modelPath, string animationControllerPath = EMPTY_STRING)
	{
		mModelPath = modelPath;
		mAnimationControllerPath = animationControllerPath;
		mCharacterLoadedCallback = null;
		mUserData = null;
		if (modelPath.Length != 0)
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
		if(mAvatar == null || mAvatar.getAnimator() == null || string.IsNullOrEmpty(name))
		{
			return 0.0f;
		}
		if(mAnimationLenghtList.ContainsKey(name))
		{
			return mAnimationLenghtList[name];
		}
		float length = getAnimationLength(mAvatar.getAnimator(), name);
		mAnimationLenghtList.Add(name, length);
		return length;
	}
	public virtual void notifyComponentChanged(GameComponent component) {}
	public virtual void notifyStateChanged(PlayerState state) { }
	public CharacterBaseData getBaseData() { return mBaseData; }
	public Type getType() { return mCharacterType; }
	public CharacterComponentModel getAvatar() { return mAvatar; }
	public Animation getAnimation() { return mAvatar.getAnimation(); }
	public Animator getAnimator() { return mAvatar.getAnimator(); }
	public Rigidbody getRigidBody() { return mRigidBody; }
	public uint getGUID() { return mGUID; }
	public CharacterDecisionTree getDecisionTree() { return mDecisionTree; }
	public CharacterStateMachine getStateMachine() { return mStateMachine; }
	public T getState<T>(uint id) where T : PlayerState { return mStateMachine.getState<T>(id); }
	public List<PlayerState> getState<T>() where T : PlayerState { return mStateMachine.getState<T>(); }
	public T getFirstState<T>() where T : PlayerState { return mStateMachine.getFirstState<T>(); }
	public Dictionary<Type, List<PlayerState>> getStateList() { return mStateMachine.getStateList(); }
	public List<CacheState> getCacheStateList() { return mStateMachine.getCacheStateList(); }
	public bool hasState<T>() where T : PlayerState { return mStateMachine.hasState<T>(); }
	public bool hasState(Type state) { return mStateMachine.hasState(state); }
	public bool hasStateGroup<T>() where T : StateGroup { return mStateMachine.hasStateGroup<T>(); }
	//--------------------------------------------------------------------------------------------------------------
	protected void onModelLoaded(GameObject go, object userData)
	{
		notifyModelLoaded(go);
		afterModelLoaded();
	}
	protected virtual void notifyModelLoaded(GameObject go)
	{
		Vector3 lastPosition = getPosition();
		Vector3 lastRotation = getRotation();
		Vector3 lastScale = getScale();
		setObject(go, true);
		// 将外部节点设置为角色节点后,角色在销毁时就不能自动销毁节点,否则会出错
		setDestroyObject(false);
		setParent(mCharacterManager.getObject());
		mAvatar.setModel(go, mModelPath);
		mRigidBody = go.GetComponent<Rigidbody>();
		if (mAnimationControllerPath.Length != 0)
		{
			mAvatar.getAnimator().runtimeAnimatorController = mResourceManager.loadResource<RuntimeAnimatorController>(mAnimationControllerPath, true);
		}
		OT.MOVE(this,lastPosition);
		OT.ROTATE(this, lastRotation);
		OT.SCALE(this, lastScale);
	}
	protected void afterModelLoaded()
	{
		mCharacterLoadedCallback?.Invoke(this, mUserData);
		mCharacterLoadedCallback = null;
		mUserData = null;
	}
}