using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;

// 用于监听延迟命令的类,确保延迟命令能够得到很好的控制
public class DelayCmdWatcher : ClassObject
{
	protected HashSet<long> mDelayCmdList;		// 布局显示和隐藏时的延迟命令列表,当命令执行时,会从列表中移除该命令
	protected CommandCallback mCmdCallback;		// 命令开始执行的回调
	public DelayCmdWatcher()
	{
		mCmdCallback = onCmdStarted;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDelayCmdList?.Clear();
		// mCmdCallback不重置
		// mCmdCallback = null;
	}
	public override void setDestroy(bool isDestroy)
	{
		base.setDestroy(isDestroy);
		if (isDestroy)
		{
			interruptAllCommand();
		}
	}
	public void addDelayCmd(Command cmd)
	{
		mDelayCmdList ??= new();
		mDelayCmdList.Add(cmd.getAssignID());
		cmd.addStartCommandCallback(mCmdCallback);
	}
	public void interruptCommand(long assignID, bool showError = true)
	{
		if (mDelayCmdList != null && mDelayCmdList.Remove(assignID))
		{
			mCommandSystem?.interruptCommand(assignID, showError);
		}
	}
	public void interruptAllCommand()
	{
		foreach (long item in mDelayCmdList.safe())
		{
			mCommandSystem?.interruptCommand(item, false);
		}
		mDelayCmdList?.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void onCmdStarted(Command cmd)
	{
		if (mDelayCmdList != null && !mDelayCmdList.Remove(cmd.getAssignID()))
		{
			logError("命令执行后移除命令失败!");
		}
	}
}