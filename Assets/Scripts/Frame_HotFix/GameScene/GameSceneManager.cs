﻿using System;
using System.Collections.Generic;
using static CSharpUtility;

// 逻辑场景管理器
public class GameSceneManager : FrameSystem
{
	protected List<GameScene> mLastSceneList = new();		// 上一个场景的列表,用于在update中延迟销毁上一个场景
	protected GameScene mCurScene;							// 当前场景
	public GameSceneManager()
	{
		mCreateObject = true;
	}
	public GameScene getCurScene(){ return mCurScene; }
	public bool enterScene<T>(Type startProcedure) where T : GameScene
	{
		return enterScene(typeof(T), startProcedure);
	}
	public bool enterScene(Type type, Type startProcedure)
	{
		// 再次进入当前的场景,只是从初始流程开始执行,并不会重新执行进入场景的操作
		if (mCurScene != null && mCurScene.GetType() == type)
		{
			mCurScene.setTempStartProcedure(startProcedure);
			mCurScene.enterStartProcedure();
		}
		else
		{
			var pScene = createInstance<GameScene>(type);
			pScene.setName(type.ToString());
			// 如果有上一个场景,则先销毁上一个场景,只是暂时保存下上个场景的指针,然后在更新中将场景销毁
			if (mCurScene != null)
			{
				mLastSceneList.Add(mCurScene);
				mCurScene.exit();
				mCurScene = null;
			}
			mCurScene = pScene;
			mCurScene.setTempStartProcedure(startProcedure);
			mCurScene.init();
		}
		return true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 如果上一个场景不为空,则将上一个场景销毁
		foreach (GameScene scene in mLastSceneList)
		{
			scene.willDestroy();
			scene.destroy();
		}
		mLastSceneList.Clear();
		mCurScene?.update(elapsedTime);
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		mCurScene?.lateUpdate(elapsedTime);
	}
	public override void destroy()
	{
		foreach (GameScene scene in mLastSceneList)
		{
			scene.destroy();
		}
		mLastSceneList.Clear();
		mCurScene?.exit();
		mCurScene?.destroy();
		mCurScene = null;
		base.destroy();
	}
	public override void willDestroy()
	{
		base.willDestroy();
		foreach (GameScene scene in mLastSceneList)
		{
			scene.willDestroy();
		}
		mCurScene?.willDestroy();
	}
}