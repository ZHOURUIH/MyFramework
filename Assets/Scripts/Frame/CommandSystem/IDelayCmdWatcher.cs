using System;

public interface IDelayCmdWatcher
{
	void addDelayCmd(Command cmd);
	void onCmdStarted(Command cmd);
	void interruptAllCommand();
	void interruptCommand(int assignID, bool showError);
}