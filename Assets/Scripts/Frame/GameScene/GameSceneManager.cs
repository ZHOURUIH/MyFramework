using UnityEngine;
using System;
using System.Collections.Generic;

public class GameSceneManager : FrameSystem
{
	protected List<GameScene> mLastSceneList;
	protected GameScene mCurScene;
	public GameSceneManager()
	{
		mLastSceneList = new List<GameScene>();
		mCreateObject = true;
	}
	public new GameScene getCurScene(){ return mCurScene; }
	public new bool enterScene(Type type, Type startProcedure, string intent)
	{
		// 再次进入当前的场景,只是从初始流程开始执行,并不会重新执行进入场景的操作
		if (mCurScene != null && Typeof(mCurScene) == type)
		{
			mCurScene.setTempStartProcedure(startProcedure, intent);
			mCurScene.enterStartProcedure();
		}
		else
		{
			GameScene pScene = createInstance<GameScene>(type);
			pScene.setName(type.ToString());
			// 如果有上一个场景,则先销毁上一个场景,只是暂时保存下上个场景的指针,然后在更新中将场景销毁
			if (mCurScene != null)
			{
				mLastSceneList.Add(mCurScene);
				mCurScene.exit();
				mCurScene = null;
			}
			mCurScene = pScene;
			mCurScene.setTempStartProcedure(startProcedure, intent);
			mCurScene.init();
		}
		return true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 如果上一个场景不为空,则将上一个场景销毁
		int count = mLastSceneList.Count;
		for(int i = 0; i < count; ++i)
		{
			mLastSceneList[i].destroy();
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
		int count = mLastSceneList.Count;
		for(int i = 0; i < count; ++i)
		{
			mLastSceneList[i].destroy();
		}
		mLastSceneList.Clear();
		mCurScene?.destroy();
		mCurScene = null;
		base.destroy();
	}
}