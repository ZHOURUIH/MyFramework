using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class StateParam : GameBase, IClassObject
{
	public virtual void resetProperty() { }
}

public class PlayerState : GameBase, IClassObject
{
	protected uint mID;
	protected Character mPlayer;
	protected CharacterBaseData mData;
	protected bool mActive;
	protected bool mIgnoreTimeScale;
	protected Animation mAnimation;
	protected Animator mAnimator;
	protected float mStateTime;		// 该状态持续的时间,小于0表示无限制
	protected List<Type> mGroup;    // 状态所属的状态组
	protected SAME_STATE_OPERATE mAllowSuperposition;   // 该状态是否允许叠加
	protected StateParam mParam;	// 此参数只能在enter中使用,执行完enter后就会回收销毁
	protected BUFF_STATE_TYPE mBuffStateType;
	protected OnStateLeave mLeaveCallback;
	public PlayerState()
	{
		mAllowSuperposition = SAME_STATE_OPERATE.SSO_COEXIST;
		mBuffStateType = BUFF_STATE_TYPE.BST_NONE;
		mActive = true;
		mIgnoreTimeScale = false;
		mStateTime = -1.0f;
		mID = generateGUID();
	}
	public void setParam(StateParam param) { mParam = param; }
	public void setLeaveCallback(OnStateLeave callback) { mLeaveCallback = callback; }
	public BUFF_STATE_TYPE getBuffStateType() { return mBuffStateType; }
	public virtual void setPlayer(Character player)
	{
		mPlayer = player;
		mData = mPlayer.getBaseData();
		mAnimation = mPlayer.getAnimation();
		mAnimator = mPlayer.getAnimator();
	}
	// 当前是否可以进入该状态
	public virtual bool canEnter(){ return true; }
	public virtual void enter() {}
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
	public virtual void fixedUpdate(float elapsedTime){}
	// isBreak表示是否是因为添加了互斥状态而退出的
	public virtual void leave(bool isBreak, string param) 
	{
		mLeaveCallback?.Invoke(this, isBreak, param);
	}
	public virtual void keyProcess(float elapsedTime){}
	public void setActive(bool active) { mActive = active; }
	public bool isActive() { return mActive; }
	public void setStateTime(float time) { mStateTime = time; }
	public float getStateTime() { return mStateTime; }
	public uint getID() { return mID; }
	public void setID(uint id) { mID = id; }
	public void setGroup(List<Type> group) { mGroup = group; }
	public List<Type> getGroup() { return mGroup; }
	public SAME_STATE_OPERATE allowSuperposition() { return mAllowSuperposition; }
	public void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public virtual int getPriority() { return 0; }
	public virtual void resetProperty()
	{
		mPlayer = null;
		mData = null;
		mActive = true;
		mAnimation = null;
		mStateTime = -1.0f;
		mParam = null;
		mIgnoreTimeScale = false;
	}
	//--------------------------------------------------------------------------------------------------------------
	protected void removeSelf(string param = EMPTY_STRING)
	{
		CommandCharacterRemoveState cmd = newCmd(out cmd, false);
		cmd.mState = this;
		cmd.mParam = param;
		pushCommand(cmd, mPlayer);
	}
}