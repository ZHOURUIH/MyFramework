using System;

public class CmdCharacterAddState : Command
{
	public StateParam mParam;
	public Type mStateType;
	public UINT mOutStateID;    // 用于返回添加的状态的ID
	public float mStateTime;    // 状态持续时间,小于0表示不修改默认持续时间
	public uint mStateID;
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
		builder.Append(": mStateType:", mStateType).
				Append(", mStateTime:", mStateTime);
	}
}