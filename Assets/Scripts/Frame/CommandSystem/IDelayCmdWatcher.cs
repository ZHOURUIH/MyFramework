using System;

public abstract class IDelayCmdWatcher : FrameBase
{
	public abstract void addDelayCmd(Command cmd);
	public abstract void onCmdStarted(Command cmd);
	public abstract void interruptAllCommand();
	public abstract void interruptCommand(long assignID, bool showError);
}