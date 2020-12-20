using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyFrameManager : FrameSystem
{
	protected Dictionary<int, AnimationCurve> mCurveList;
	protected AssetLoadDoneCallback mKeyframeLoadCallback;
	protected bool mLoaded;
	public KeyFrameManager()
	{
		mCurveList = new Dictionary<int, AnimationCurve>();
		mKeyframeLoadCallback = onKeyFrameLoaded;
		mCreateObject = true;
	}
	public AnimationCurve getKeyFrame(int id)
	{
		if (id == 0)
		{
			return null;
		}
		mCurveList.TryGetValue(id, out AnimationCurve curve);
		return curve;
	}
	// 加载所有KeyFrame下的关键帧
	public void loadAll(bool async)
	{
		mLoaded = false;
		if (async)
		{
			mResourceManager.loadResourceAsync<GameObject>(FrameDefine.KEY_FRAME_FILE, mKeyframeLoadCallback);
		}
		else
		{
			GameObject prefab = mResourceManager.loadResource<GameObject>(FrameDefine.KEY_FRAME_FILE);
			mKeyframeLoadCallback(prefab, null, null, null, FrameDefine.KEY_FRAME_FILE);
		}
	}
	public override void destroy()
	{
		mCurveList.Clear();
		base.destroy();
	}
	public bool isLoadDone() { return mLoaded; }
	//------------------------------------------------------------------------------------------------------------------
	protected void onKeyFrameLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAsset, byte[] bytes, object userData, string loadPath)
	{
		GameObject keyFrameObject = instantiatePrefab(mObject, asset as GameObject, getFileName(asset.name), true);
		// 查找关键帧曲线,加入列表中
		var gameKeyframe = keyFrameObject.GetComponent<GameKeyframe>();
		if (gameKeyframe == null)
		{
			logError("object in KeyFrame folder must has GameKeyframe Component!");
			return;
		}
		int count = gameKeyframe.mCurveList.Count;
		for(int i = 0; i < count; ++i)
		{
			var curveInfo = gameKeyframe.mCurveList[i];
			mCurveList[curveInfo.mID] = curveInfo.mCurve;
		}
		destroyGameObject(keyFrameObject);
		mResourceManager.unloadPath(FrameDefine.R_KEY_FRAME_PATH);
		mLoaded = true;
	}
}
