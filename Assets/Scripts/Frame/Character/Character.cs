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
	protected Type mCharacterType;          // 角色类型
	protected OnCharacterLoaded mCharacterLoadedCallback;
	protected string mModelTag;
	protected string mModelPath;
	protected string mAnimationControllerPath;
	protected object mUserData;
	protected uint mGUID;
	public Character()
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
	public void initModelAsync(string modelPath, OnCharacterLoaded callback, object userData, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimationControllerPath = animationControllerPath;
		mCharacterLoadedCallback = callback;
		mUserData = userData;
		if (!isEmpty(modelPath))
		{
			// 模型节点也就是角色节点,并且将节点挂到角色管理器下
			mObjectPool.createObjectAsync(mModelPath, onModelLoaded, mModelTag, null);
		}
	}
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
		if(mAvatar == null || mAvatar.getAnimator() == null || isEmpty(name))
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
	public PlayerState getFirstState(Type type) { return mStateMachine.getFirstState(type); }
	public PlayerState getState(uint id) { return mStateMachine.getState(id); }
	public Dictionary<Type, List<PlayerState>> getStateList() { return mStateMachine.getStateList(); }
	public List<CacheState> getCacheStateList() { return mStateMachine.getCacheStateList(); }
	public bool hasState(Type state) { return mStateMachine.hasState(state); }
	public bool hasStateGroup(Type group) { return mStateMachine.hasStateGroup(group); }
	//--------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		mAvatar = addComponent(Typeof<CharacterComponentModel>(), true) as CharacterComponentModel;
		mStateMachine = addComponent(Typeof<CharacterStateMachine>(), true) as CharacterStateMachine;
		mDecisionTree = addComponent(Typeof<CharacterDecisionTree>()) as CharacterDecisionTree;
	}
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
		if (!isEmpty(mAnimationControllerPath))
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