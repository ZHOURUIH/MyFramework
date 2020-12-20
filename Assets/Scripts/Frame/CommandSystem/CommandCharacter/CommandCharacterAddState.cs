using UnityEngine;
using System.Collections;
using System;

public class CommandCharacterAddState : Command
{
	public StateParam mParam;
	public Type mState;
	public UINT mOutStateID;    // 用于返回添加的状态的ID
	public float mStateTime;    // 状态持续时间,小于0表示不修改默认持续时间
	public uint mStateID;
	public override void init()
	{
		base.init();
		mState = null;
		mParam = null;
		mOutStateID = null;
		mStateTime = -1.0f;
		mStateID = 0;
	}
	public override void execute()
	{
		Character character = mReceiver as Character;
		if(mState == null)
		{
			return;
		}
		PlayerState state;
		bool ret = character.getStateMachine().addState(mState, mParam, out state, mStateTime, mStateID);
		if (ret)
		{
			mOutStateID?.set(state.getID());
		}
		mResult?.set(ret);
		if(mParam != null)
		{
			mClassPool.destroyClass(mParam);
			state.setParam(null);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mState:" + mState + ", mStateTime:" + mStateTime;
	}
}