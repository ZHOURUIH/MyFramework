using System;

public class CommandCharacterRemoveState : Command
{
	public PlayerState mState;
	public Type mStateGroup;
	public string mParam;
	public override void init()
	{
		base.init();
		mState = null;
		mStateGroup = null;
		mParam = null;
	}
	public override void execute()
	{
		Character character = mReceiver as Character;
		CharacterStateMachine stateMachine = character.getStateMachine();
		// 移除状态组时,认为状态是强行被中断
		if (mStateGroup != null)
		{
			stateMachine.removeStateInGroup(mStateGroup, true, mParam);
			return;
		}
		// 移除指定状态时认为状态是正常结束
		if(mState != null)
		{
			stateMachine.removeState(mState, false, mParam);
		}
	}
	public override string showDebugInfo()
	{
		string group = EMPTY;
		if(mStateGroup != null)
		{
			group = ", mStateGroup:" + mStateGroup;
		}
		return base.showDebugInfo() + ": mState:" + mState + group;
	}
}