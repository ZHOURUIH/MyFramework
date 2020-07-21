using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CommandPool : GameBase
{
	protected Dictionary<Type, List<Command>> mInusedList;
	protected Dictionary<Type, List<Command>> mUnusedList;
	protected ThreadLock mInuseLock;
	protected ThreadLock mUnuseLock;
	protected ThreadLock mNewCmdLock;	// 只需要添加创建命令的锁就可以,只要不分配出重复的命令,回收命令时就不会发生冲突
	protected int mNewCount;
	protected static int mIDSeed = 0;
	protected static int mAssignIDSeed = 0;
	public CommandPool()
	{
		mInusedList = new Dictionary<Type, List<Command>>();
		mUnusedList = new Dictionary<Type, List<Command>>();
		mInuseLock = new ThreadLock();
		mUnuseLock = new ThreadLock();
		mNewCmdLock = new ThreadLock();
	}
	public void init(){}
	public void destroy()
	{
		mInusedList.Clear();
		mUnusedList.Clear();
		mInusedList = null;
		mUnusedList = null;
	}
	public Command newCmd(Type type, bool show = true, bool delay = false)
	{
		mNewCmdLock.waitForUnlock();
		// 首先从未使用的列表中获取,获取不到再重新创建一个
		Command cmd = null;
		if (mUnusedList.ContainsKey(type))
		{
			if (mUnusedList[type].Count > 0)
			{
				cmd = mUnusedList[type][0];
				// 从未使用列表中移除
				removeUnuse(cmd);
			}
		}
		// 没有找到可以用的,则创建一个
		if (cmd == null)
		{
			cmd = createInstance<Command>(type);
			cmd.setID(mIDSeed++);
			cmd.init();
			cmd.setType(type);
			++mNewCount;
		}
		// 设置为可用命令
		cmd.setValid(true);
		if (delay)
		{
			cmd.setAssignID(mAssignIDSeed++);
		}
		else
		{
			cmd.setAssignID(-1);
		}
		cmd.setShowDebugInfo(show);
		cmd.setDelayCommand(delay);
		// 加入已使用列表
		addInuse(cmd);
		mNewCmdLock.unlock();
		return cmd;
	}
	public T newCmd<T>(bool show = true, bool delay = false) where T : Command, new()
	{
		return newCmd(typeof(T), show, delay) as T;
	}
	public void destroyCmd(Command cmd) 
	{
		// 销毁命令时,初始化命令数据,并设置为不可用命令
		cmd.init();
		cmd.setValid(false);
		addUnuse(cmd);
		removeInuse(cmd);
	}
	//------------------------------------------------------------------------------------------------------------------
	protected void addInuse(Command cmd)
	{
		mInuseLock.waitForUnlock();
		// 添加到使用列表中
		Type t = cmd.getType();
		if (!mInusedList.ContainsKey(t))
		{
			mInusedList.Add(t, new List<Command>());
		}
		mInusedList[t].Add(cmd);
		mInuseLock.unlock();
	}
	protected void addUnuse(Command cmd)
	{
		mUnuseLock.waitForUnlock();
		// 添加到未使用列表中
		Type t = cmd.getType();
		if (!mUnusedList.ContainsKey(t))
		{
			mUnusedList.Add(t, new List<Command>());
		}
		mUnusedList[t].Add(cmd);
		mUnuseLock.unlock();
	}
	protected void removeInuse(Command cmd)
	{
		mInuseLock.waitForUnlock();
		Type t = cmd.getType();
		if (mInusedList.ContainsKey(t))
		{
			mInusedList[t].Remove(cmd);
		}
		mInuseLock.unlock();
	}
	protected void removeUnuse(Command cmd)
	{
		mUnuseLock.waitForUnlock();
		Type t = cmd.getType();
		if (mUnusedList.ContainsKey(t))
		{
			mUnusedList[t].Remove(cmd);
		}
		mUnuseLock.unlock();
	}
}
