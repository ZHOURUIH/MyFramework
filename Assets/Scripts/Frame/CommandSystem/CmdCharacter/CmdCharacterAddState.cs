using System;

// 给角色添加一个状态
public class CmdCharacterAddState
{
	// 状态所需参数
	// 状态类型
	// 用于返回添加的状态的ID
	// 状态持续时间,小于0表示不修改默认持续时间
	// 状态ID,可不填
	public static CharacterState execute(Character character, Type stateType, StateParam param  = null, float stateTime = -1.0f, long stateID = 0)
	{
		if (stateType == null)
		{
			return null;
		}
		COMCharacterStateMachine stateMachine = character.getStateMachine();
		if (stateMachine == null)
		{
			return null;
		}
		CharacterState state = stateMachine.addState(stateType, param, stateTime, stateID);
		state?.setParam(null);
		return state;
	}
}