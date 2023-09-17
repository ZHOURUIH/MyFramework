using System;

// 移除角色的一个状态
public class CmdCharacterRemoveState
{
	// state,状态实例
	// param,移除时要传递的参数
	public static void execute(Character character, CharacterState state, string param = null)
	{
		if(state == null)
		{
			return;
		}
		// 移除指定状态时认为状态是正常结束
		character.getStateMachine().removeState(state, false, param);
	}
	// stateGroup,状态组,如果填了状态组,则表示会移除角色上所有属于该状态组的状态,并且此处mState的值将不会生效
	// param,移除时要传递的参数
	public static void execute(Character character, Type stateGroup, string param = null)
	{
		// 移除状态组时,认为状态是强行被中断
		if (stateGroup == null)
		{
			return;
		}
		COMCharacterStateMachine stateMachine = character.getStateMachine();
		stateMachine.removeStateInGroup(stateGroup, true, param);
	}
}