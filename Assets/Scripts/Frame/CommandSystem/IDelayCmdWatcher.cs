using System;

public interface IDelayCmdWatcher
{
	void addDelayCmd(Command cmd);
	void onCmdStarted(Command cmd);
	void interruptAllCommand();
	void interruptCommand(ulong assignID, bool showError);
}