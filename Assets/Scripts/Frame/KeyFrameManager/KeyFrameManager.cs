using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyFrameManager : FrameSystem
{
	protected Dictionary<int, MyCurve> mCurveList;
	protected AssetLoadDoneCallback mKeyframeLoadCallback;
	protected bool mLoaded;
	public KeyFrameManager()
	{
		mCurveList = new Dictionary<int, MyCurve>();
		mKeyframeLoadCallback = onKeyFrameLoaded;
		mCreateObject = true;
	}
	public MyCurve getKeyFrame(int id)
	{
		if (id == 0)
		{
			return null;
		}
		mCurveList.TryGetValue(id, out MyCurve curve);
		return curve;
	}
	// 加载所有KeyFrame下的关键帧
	public void loadAll(bool async)
	{
		mLoaded = false;

		// 通过公式计算的曲线
		createCurve<CurveZeroOne>(KEY_CURVE.ZERO_ONE);
		createCurve<CurveZeroOneZero>(KEY_CURVE.ZERO_ONE_ZERO);
		createCurve<CurveOneZero>(KEY_CURVE.ONE_ZERO);
		createCurve<CurveOneZeroOne>(KEY_CURVE.ONE_ZERO_ONE);
		createCurve<CurveBackIn>(KEY_CURVE.BACK_IN);
		createCurve<CurveBackInOut>(KEY_CURVE.BACK_IN_OUT);
		createCurve<CurveBackOut>(KEY_CURVE.BACK_OUT);
		createCurve<CurveBounceIn>(KEY_CURVE.BOUNCE_IN);
		createCurve<CurveBounceInOut>(KEY_CURVE.BOUNCE_IN_OUT);
		createCurve<CurveBounceOut>(KEY_CURVE.BOUNCE_OUT);
		createCurve<CurveCircleIn>(KEY_CURVE.CIRCLE_IN);
		createCurve<CurveCircleInOut>(KEY_CURVE.CIRCLE_IN_OUT);
		createCurve<CurveCircleOut>(KEY_CURVE.CIRCLE_OUT);
		createCurve<CurveCubicIn>(KEY_CURVE.CUBIC_IN);
		createCurve<CurveCubicInOut>(KEY_CURVE.CUBIC_IN_OUT);
		createCurve<CurveCubicOut>(KEY_CURVE.CUBIC_OUT);
		createCurve<CurveElasticIn>(KEY_CURVE.ELASTIC_IN);
		createCurve<CurveElasticInOut>(KEY_CURVE.ELASTIC_IN_OUT);
		createCurve<CurveElasticOut>(KEY_CURVE.ELASTIC_OUT);
		createCurve<CurveExpoIn>(KEY_CURVE.EXPO_IN);
		createCurve<CurveExpoInOut>(KEY_CURVE.EXPO_IN_OUT);
		createCurve<CurveExpoOut>(KEY_CURVE.EXPO_OUT);
		createCurve<CurveQuadIn>(KEY_CURVE.QUAD_IN);
		createCurve<CurveQuadInOut>(KEY_CURVE.QUAD_IN_OUT);
		createCurve<CurveQuadOut>(KEY_CURVE.QUAD_OUT);
		createCurve<CurveQuartIn>(KEY_CURVE.QUART_IN);
		createCurve<CurveQuartInOut>(KEY_CURVE.QUART_IN_OUT);
		createCurve<CurveQuartOut>(KEY_CURVE.QUART_OUT);
		createCurve<CurveQuintIn>(KEY_CURVE.QUINT_IN);
		createCurve<CurveQuintInOut>(KEY_CURVE.QUINT_IN_OUT);
		createCurve<CurveQuintOut>(KEY_CURVE.QUINT_OUT);
		createCurve<CurveSineIn>(KEY_CURVE.SINE_IN);
		createCurve<CurveSineInOut>(KEY_CURVE.SINE_IN_OUT);
		createCurve<CurveSineOut>(KEY_CURVE.SINE_OUT);

		// 在编辑器中编辑的曲线
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
	//--------------------------------------------------------------------------------------------------------------------------------------
	protected void createCurve<T>(int curveID) where T : MyCurve, new()
	{
		mCurveList.Add(curveID, new T());
	}
	protected void onKeyFrameLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAsset, byte[] bytes, object userData, string loadPath)
	{
		// 删除所有ID小于100的,也就是通过加载资源获得的曲线
		LIST(out List<int> deleteKeys);
		foreach (var item in mCurveList)
		{
			if (item.Key < (int)KEY_CURVE.BUILDIN_CURVE)
			{
				deleteKeys.Add(item.Key);
			}
		}
		foreach (var item in deleteKeys)
		{
			mCurveList.Remove(item);
		}
		UN_LIST(deleteKeys);

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
			mCurveList[curveInfo.mID] = new UnityCurve(curveInfo.mCurve);
		}
		destroyGameObject(keyFrameObject);
		mResourceManager.unloadPath(FrameDefine.R_KEY_FRAME_PATH);
		mLoaded = true;
	}
}
