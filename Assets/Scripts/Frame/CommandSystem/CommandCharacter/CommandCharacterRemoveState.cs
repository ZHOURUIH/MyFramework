using System;

public class CommandCharacterRemoveState : Command
{
	public PlayerState mState;
	public Type mStateGroup;
	public string mParam;
	public override void resetProperty()
	{
		base.resetProperty();
		mState = null;
		mStateGroup = null;
		mParam = null;
	}
	public override void execute()
	{
		var character = mReceiver as Character;
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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mState:", mState?.GetType()).
				Append(", mStateGroup:", mStateGroup?.ToString()).
				Append(", mParam:", mParam);
	}
}