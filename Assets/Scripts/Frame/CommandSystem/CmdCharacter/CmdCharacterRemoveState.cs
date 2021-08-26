using System;

// 移除角色的一个状态
public class CmdCharacterRemoveState : Command
{
	public CharacterState mState;	// 状态实例
	public Type mStateGroup;		// 状态组,如果填了状态组,则表示会移除角色上所有属于该状态组的状态,并且此处mState的值将不会生效
	public string mParam;			// 移除时要传递的参数
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
		COMCharacterStateMachine stateMachine = character.getStateMachine();
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mState:", Typeof(mState)).
				append(", mStateGroup:", mStateGroup?.ToString()).
				append(", mParam:", mParam);
	}
}