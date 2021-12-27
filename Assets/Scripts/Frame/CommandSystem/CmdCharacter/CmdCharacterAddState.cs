using System;

// 给角色添加一个状态
public class CmdCharacterAddState : Command
{
	public StateParam mParam;	// 状态所需参数
	public Type mStateType;		// 状态类型
	public LONG mOutStateID;	// 用于返回添加的状态的ID
	public float mStateTime;	// 状态持续时间,小于0表示不修改默认持续时间
	public long mStateID;		// 状态ID,可不填
	public override void resetProperty()
	{
		base.resetProperty();
		mStateType = null;
		mParam = null;
		mOutStateID = null;
		mStateTime = -1.0f;
		mStateID = 0;
	}
	public override void execute()
	{
		var character = mReceiver as Character;
		if (mStateType == null)
		{
			return;
		}
		bool ret = character.getStateMachine().addState(mStateType, mParam, out CharacterState state, mStateTime, mStateID);
		if (ret)
		{
			mOutStateID?.set(state.getID());
		}
		mResult?.set(ret);
		if(mParam != null)
		{
			UN_CLASS(mParam);
			state.setParam(null);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mStateType:", mStateType).
				append(", mStateTime:", mStateTime);
	}
}