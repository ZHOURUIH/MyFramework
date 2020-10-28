using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyFrameManager : FrameSystem
{
	protected Dictionary<string, Curve> mCurveList;
	protected int mLoadedCount;	// 已加载的关键帧数量
	public KeyFrameManager()
	{
		mCurveList = new Dictionary<string, Curve>();
		mLoadedCount = 0;
		mCreateObject = true;
	}
	public AnimationCurve getKeyFrame(string name)
	{
		if (isEmpty(name))
		{
			return null;
		}
		name = name.ToLower();
		if (mCurveList.ContainsKey(name))
		{
			return mCurveList[name].mCurve;
		}
		return null;
	}
	// 加载所有KeyFrame下的关键帧
	public void loadAll(bool async)
	{
		mLoadedCount = 0;
		string path = FrameDefine.R_KEY_FRAME_PATH;
		List<string> fileList = mListPool.newList(out fileList);
		mResourceManager.getFileList(path, fileList);
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			// 去除KeyFrame/前缀
			string keyFrameName = getFileNameNoSuffix(fileList[i], true).ToLower();
			if(mCurveList.ContainsKey(keyFrameName))
			{
				destroyGameObject(ref mCurveList[keyFrameName].mObject);
				mCurveList[keyFrameName] = null;
			}
			else
			{
				mCurveList.Add(keyFrameName, null);
			}
		}
		// 分为两次遍历,是为了方便检查是否已经加载完毕
		for (int i = 0; i < fileCount; ++i)
		{
			string fileNameNoSuffix = fileList[i];
			if (async)
			{
				mResourceManager.loadResourceAsync<GameObject>(fileNameNoSuffix, onKeyFrameLoaded, null, true);
			}
			else
			{
				GameObject prefab = mResourceManager.loadResource<GameObject>(fileNameNoSuffix, true);
				onKeyFrameLoaded(prefab, null, null, null, fileNameNoSuffix);
			}
		}
		mListPool.destroyList(fileList);
	}
	public override void destroy()
	{
		// 将实例化出的所有物体销毁
		foreach(var item in mCurveList)
		{
			destroyGameObject(ref item.Value.mObject);
		}
		mCurveList.Clear();
		mLoadedCount = 0;
		base.destroy();
	}
	public bool isLoadDone()
	{
		return mLoadedCount == mCurveList.Count;
	}
	public float getLoadedPercent()
	{
		return (float)mLoadedCount / mCurveList.Count;
	}
	//------------------------------------------------------------------------------------------------------------------
	protected void onKeyFrameLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAsset, byte[] bytes, object userData, string loadPath)
	{
		GameObject keyFrameObject = instantiatePrefab(mObject, asset as GameObject, true);
		// 查找关键帧曲线,加入列表中
		GameKeyframe gameKeyframe = keyFrameObject.GetComponent<GameKeyframe>();
		if (gameKeyframe == null)
		{
			logError("object in KeyFrame folder must has GameKeyframe Component!");
			return;
		}
		AnimationCurve animCurve = gameKeyframe.mCurve;
		if (animCurve != null)
		{
			Curve curve = new Curve();
			curve.mObject = keyFrameObject;
			curve.mCurve = animCurve;
			mCurveList[asset.name.ToLower()] = curve;
			++mLoadedCount;
			// 加载完所有关键帧后,卸载所有关键帧预设
			if(isLoadDone())
			{
				mResourceManager.unloadPath(FrameDefine.R_KEY_FRAME_PATH);
			}
		}
		else
		{
			logError("object in KeyFrame folder must has AnimationCurve!");
		}
	}
}
