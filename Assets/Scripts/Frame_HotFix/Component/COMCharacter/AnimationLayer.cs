﻿using System;

// 用于处理动画状态机层的相关逻辑
// 认为每一个状态机层都有一个对应的状态组,则会判断如果当前没有该状态组中的任意一个动作,则会添加该组中的一个默认动作
public class AnimationLayer : ClassObject
{
	protected Character mCharacter;		// 所属角色
	protected Type mDefaultState;		// 该状态组中默认的状态
	protected Type mGroup;				// 动画层的状态组
	protected int mLayerIndex;			// 动画层的下标
	public override void resetProperty()
	{
		base.resetProperty();
		mCharacter = null;
		mDefaultState = null;
		mGroup = null;
		mLayerIndex = 0;
	}
	public void update()
	{
		if (mCharacter != null && !mCharacter.hasStateGroup(mGroup))
		{
			// 如果设置了默认的动作状态,则跳转到该状态
			if (mDefaultState != null)
			{
				mCharacter.getStateMachine().addState(mDefaultState);
			}
			// 没有动作状态则直接设置为空的动作
			else
			{
				mCharacter.getAvatar().playAnimation(0, mLayerIndex);
			}
		}
	}
	public void setCharacter(Character character) { mCharacter = character; }
	public void setDefaultState(Type state) { mDefaultState = state; }
	public void setGroup(Type group) { mGroup = group; }
	public void setLayerIndex(int index) { mLayerIndex = index; }
	public Character getCharacter() { return mCharacter; }
	public Type getDefaultState() { return mDefaultState; }
	public Type getGroup() { return mGroup; }
	public int getLayerIndex() { return mLayerIndex; }
}