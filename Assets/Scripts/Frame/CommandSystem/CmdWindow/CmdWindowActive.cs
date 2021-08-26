using UnityEngine;

// 设置窗口显示或隐藏
public class CmdWindowActive : Command
{
	public bool mActive;		// 显示或隐藏
	public override void resetProperty()
	{
		base.resetProperty();
		mActive = true;
	}
	public override void execute()
	{
		var uiObjcet = mReceiver as myUIObject;
		uiObjcet.setActive(mActive);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mActive:", mActive);
	}
}