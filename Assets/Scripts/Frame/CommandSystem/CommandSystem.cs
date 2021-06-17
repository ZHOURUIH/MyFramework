using UnityEngine;
using System.Collections.Generic;
using System;

public class CommandSystem : FrameSystem
{
	protected List<Command> mCommandBufferProcess;	// 用于处理的命令列表
	protected List<Command> mCommandBufferInput;	// 用于放入命令的命令列表,收集一帧中各个线程的命令
	protected List<Command> mExecuteList;           // 即将在这一帧执行的命令
	protected ThreadLock mBufferLock;
	public CommandSystem()
	{
		mCommandBufferProcess = new List<Command>();
		mCommandBufferInput = new List<Command>();
		mExecuteList = new List<Command>();
		mBufferLock = new ThreadLock();
	}
	public override void destroy()
	{
		mCommandBufferInput.Clear();
		mCommandBufferProcess.Clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 同步命令输入列表到命令处理列表中
		syncCommandBuffer();
		// 执行之前需要先清空列表
		mExecuteList.Clear();
		// 开始处理命令处理列表
		int count = mCommandBufferProcess.Count;
		for (int i = 0; i < count; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (!cmd.isIgnoreTimeScale())
			{
				cmd.setDelayTime(cmd.getDelayTime() - elapsedTime);
			}
			else
			{
				cmd.setDelayTime(cmd.getDelayTime() - Time.unscaledDeltaTime);
			}
			if (cmd.getDelayTime() <= 0.0f)
			{
				// 命令的延迟执行时间到了,则执行命令
				mExecuteList.Add(cmd);
				mCommandBufferProcess.RemoveAt(i);
				--i;
				--count;
			}
		}
		int executeCount = mExecuteList.Count;
		for (int i = 0; i < executeCount; ++i)
		{
			Command cmd = mExecuteList[i];
			cmd.setDelayCommand(false);
			if (cmd.getReceiver() != null)
			{
				pushCommand(cmd, cmd.getReceiver());
			}
		}
		// 执行完后清空列表
		mExecuteList.Clear();
	}
	// 在主线程中创建命令
	public Command newCmd(Type type, bool show, bool delay)
	{
		// 如果命令系统已经销毁了,则不能再创建命令
		if (mDestroy)
		{
			return null;
		}
		var cmd = mClassPool.newClass(type, false) as Command;
		cmd.setShowDebugInfo(show);
		cmd.setDelayCommand(delay);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在子线程中创建命令
	public Command newCmdThread(Type type, bool show, bool delay)
	{
		// 如果命令系统已经销毁了,则不能再创建命令
		if (mDestroy)
		{
			return null;
		}
		var cmd = mClassPoolThread.newClass(type, out _) as Command;
		cmd.setShowDebugInfo(show);
		cmd.setDelayCommand(delay);
		cmd.setThreadCommand(true);
		return cmd;
	}
	public void interruptCommand(List<long> assignIDList, bool showError = true)
	{
		int count = assignIDList.Count;
		for(int i = 0; i < count; ++i)
		{
			interruptCommand(assignIDList[i], showError);
		}
	}
	// 中断命令
	public bool interruptCommand(long assignID, bool showError = true)
	{
		// 如果命令系统已经销毁了,则不能再中断命令
		if(mDestroy)
		{
			return true;
		}
		if (assignID < 0)
		{
			if(showError)
			{
				logError("assignID invalid! : " + assignID);
			}
			return false;
		}

		syncCommandBuffer();
		int count = mCommandBufferProcess.Count;
		for(int i = 0; i < count; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (cmd.getAssignID() == assignID)
			{
				MyStringBuilder builder = STRING("CMD : interrupt command ", LToS(assignID), " : ");
				cmd.showDebugInfo(builder);
				builder.Append(", receiver : ", cmd.getReceiver().getName());
				log(END_STRING(builder), LOG_LEVEL.HIGH);
				mCommandBufferProcess.Remove(cmd);
				// 销毁回收命令
				destroyCmd(cmd);
				return true;
			}
		}

		bool success = false;
		// 在即将执行的列表中查找,不能删除列表元素，只能将接收者设置为空来阻止命令执行,如果正在执行该命令,则没有效果
		int executeCount = mExecuteList.Count;
		for(int i = 0; i < executeCount; ++i)
		{
			Command cmd = mExecuteList[i];
			if (cmd.getAssignID() == assignID)
			{
				cmd.setReceiver(null);
				success = true;
				break;
			}
		}
		if(!success && showError)
		{
			logError("not find cmd with assignID! " + assignID);
		}
		return success;
	}
	// 在子线程中发送一个指定类型的命令
	public void pushCommandThread(Type type, CommandReceiver cmdReceiver, bool show = true)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		Command cmd = newCmdThread(type, show, false);
		pushCommand(cmd, cmdReceiver);
	}
	// 在主线程中发送一个指定类型的命令
	public void pushCommand(Type type, CommandReceiver cmdReceiver, bool show = true)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		Command cmd = newCmd(type, show, false);
		pushCommand(cmd, cmdReceiver);
	}
	// 在当前线程中执行命令
	public new void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		// 如果已经确定在运行时不会出现以下情况的错误,则在打包后就可以不需要再检测了
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (cmd == null)
		{
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY));
			return;
		}
		if (cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? Typeof(cmd).ToString() : EMPTY));
			return;
		}
		if (cmd.isDestroy())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushCommand cmd type : "
				+ Typeof(cmd) + "cmd id : " + cmd.getAssignID());
			return;
		}
		if (cmd.isDelayCommand())
		{
			MyStringBuilder builder = STRING("cmd is a delay cmd! can not use pushCommand!", LToS(cmd.getAssignID()), ", ");
			cmd.showDebugInfo(builder);
			logError(END_STRING(builder));
			return;
		}
#endif
		cmd.setReceiver(cmdReceiver);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (cmd.isShowDebugInfo())
		{
			MyStringBuilder builder = STRING("CMD : ", LToS(cmd.getAssignID()), ", ");
			cmd.showDebugInfo(builder);
			builder.Append(", receiver : ", cmdReceiver.getName());
			log(END_STRING(builder), LOG_LEVEL.NORMAL);
		}
#endif
		cmd.invokeStartCallBack();
		cmd.setState(EXECUTE_STATE.EXECUTING);
		cmd.execute();
		cmd.setState(EXECUTE_STATE.EXECUTED);
		cmd.invokeEndCallBack();
		// 销毁回收命令
		destroyCmd(cmd);
	}
	// 在子线程中发送一个指定类型的命令,并且会延迟到主线程执行
	public Command pushDelayCommandThread(Type type, CommandReceiver cmdReceiver, float delayExecute, bool show, IDelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return null;
		}
		Command cmd = newCmdThread(type, show, true);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	// 在主线程中发送一个指定类型的命令,并且在主线程中延迟执行
	public Command pushDelayCommand(Type type, CommandReceiver cmdReceiver, float delayExecute, bool show, IDelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return null;
		}
		Command cmd = newCmd(type, show, true);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	// 延迟执行命令,会延迟到主线程执行
	public new void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, IDelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		// 如果已经确定在运行时不会出现以下情况的错误,则在打包后就可以不需要再检测了
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (cmd == null)
		{
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY));
			return;
		}
		if(cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? Typeof(cmd).ToString() : EMPTY));
			return;
		}
		if (cmd.isDestroy())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushDelayCommand cmd type : "
				+ Typeof(cmd) + "cmd id : " + cmd.getAssignID());
			return;
		}
		if (!cmd.isDelayCommand())
		{
			MyStringBuilder builder = STRING("cmd is not a delay command, Command : ", LToS(cmd.getAssignID()), ", ");
			cmd.showDebugInfo(builder);
			logError(END_STRING(builder));
			return;
		}
#endif
		clampMin(ref delayExecute);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (cmd.isShowDebugInfo())
		{
			MyStringBuilder builder = STRING("CMD : delay cmd : ", LToS(cmd.getAssignID()), ", ", FToS(delayExecute), ", info : ");
			cmd.showDebugInfo(builder);
			builder.Append(", receiver : ", cmdReceiver.getName());
			log(END_STRING(builder), LOG_LEVEL.NORMAL);
		}
#endif
		cmd.setDelayTime(delayExecute);
		cmd.setReceiver(cmdReceiver);
		mBufferLock.waitForUnlock();
		mCommandBufferInput.Add(cmd);
		mBufferLock.unlock();
		watcher?.addDelayCmd(cmd);
	}
	public virtual void notifyReceiverDestroied(CommandReceiver receiver)
	{
		if (mDestroy)
		{
			return;
		}
		// 先同步命令列表
		syncCommandBuffer();
		int processCount = mCommandBufferProcess.Count;
		for (int i = 0; i < processCount; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (cmd.getReceiver() == receiver)
			{
				destroyCmd(cmd);
				mCommandBufferProcess.RemoveAt(i);
				--i;
				--processCount;
			}
		}
		// 执行列表中
		int count = mExecuteList.Count;
		for(int i = 0; i < count; ++i)
		{
			Command cmd = mExecuteList[i];
			// 已执行或正在执行的命令不作判断,该列表无法删除元素,只能将接收者设置为null
			if (cmd.getReceiver() == receiver && cmd.getState() == EXECUTE_STATE.NOT_EXECUTE)
			{
				cmd.setReceiver(null);
			}
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------------------
	protected void syncCommandBuffer()
	{
		mBufferLock.waitForUnlock();
		mCommandBufferProcess.AddRange(mCommandBufferInput);
		mCommandBufferInput.Clear();
		mBufferLock.unlock();
	}
	protected void destroyCmd(Command cmd)
	{
		if(cmd == null)
		{
			return;
		}
		if(cmd.isThreadCommand())
		{
			mClassPoolThread?.destroyClass(cmd);
		}
		else
		{
			mClassPool?.destroyClass(cmd);
		}
	}
}