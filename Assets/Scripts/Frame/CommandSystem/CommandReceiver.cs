using System;

public class CommandReceiver : GameBasePooledObject
{
	protected string mName;
	public virtual void receiveCommand(Command cmd)
	{
		cmd.invokeStartCallBack();
		cmd.setState(EXECUTE_STATE.EXECUTING);
		cmd.execute();
		cmd.setState(EXECUTE_STATE.EXECUTED);
		cmd.invokeEndCallBack();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
	}
	public virtual string getName() { return mName; }
	public virtual void setName(string name) { mName = name; }
	public virtual void destroy()
	{
		// 通知命令系统有一个命令接受者已经被销毁了,需要取消命令缓冲区中的即将发给该接受者的命令
		mCommandSystem?.notifyReceiverDestroied(this);
	}
}
