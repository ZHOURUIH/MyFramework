using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameILR : GB
{
	public static void startILR()
	{
		// 启动之前需要先进行注册
		registeAll();

		CommandGameSceneManagerEnter cmd = newILRCmd(out cmd);
		cmd.mSceneType = typeof(MainScene);
		pushCommand(cmd, mGameSceneManager);
	}
	public static void registeAll()
	{
		LayoutRegisterILR.registeAllLayout();
	}
}