using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBase;
using static FrameUtility;
using static StringUtility;
using static FrameDefine;

// 关键帧曲线管理器
public class KeyFrameManager : FrameSystem
{
	protected Dictionary<int, MyCurve> mCurveList;			// 关键帧曲线列表
	protected AssetLoadDoneCallback mKeyframeLoadCallback;	// 用于避免GC的委托
	protected bool mAutoLoad;		// 是否在资源可访问时自动加载所有关键帧
	protected bool mLoaded;			// 资源是否加载完毕
	public KeyFrameManager()
	{
		mCurveList = new Dictionary<int, MyCurve>();
		mKeyframeLoadCallback = onKeyFrameLoaded;
		mCreateObject = true;
		mAutoLoad = true;

		loadAllCaculatedCurve();
	}
	public override void resourceAvailable()
	{
		if(!mAutoLoad)
		{
			return;
		}
		loadAll(false);
	}
	public void setAutoLoad(bool autoLoad) { mAutoLoad = autoLoad; }
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

		// 在编辑器中编辑的曲线
		if (async)
		{
			mResourceManager.loadResourceAsync<GameObject>(KEY_FRAME_FILE, mKeyframeLoadCallback);
		}
		else
		{
			GameObject prefab = mResourceManager.loadResource<GameObject>(KEY_FRAME_FILE);
			mKeyframeLoadCallback(prefab, null, null, null, KEY_FRAME_FILE);
		}
	}
	public override void destroy()
	{
		mCurveList.Clear();
		base.destroy();
	}
	public bool isLoadDone() { return mLoaded; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void createCurve<T>(int curveID) where T : MyCurve, new()
	{
		mCurveList.Add(curveID, new T());
	}
	protected void onKeyFrameLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAsset, byte[] bytes, object userData, string loadPath)
	{
		// 删除所有ID大于100的,也就是通过加载资源获得的曲线
		using (new ListScope<int>(out var deleteKeys))
		{
			foreach (var item in mCurveList)
			{
				if (item.Key > KEY_CURVE.MAX_BUILDIN_CURVE)
				{
					deleteKeys.Add(item.Key);
				}
			}
			foreach (var item in deleteKeys)
			{
				mCurveList.Remove(item);
			}
		}

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
			var unityCurve = new UnityCurve();
			unityCurve.setCurve(curveInfo.mCurve);
			mCurveList[curveInfo.mID] = unityCurve;
		}
		destroyGameObject(keyFrameObject);
		mResourceManager.unloadPath(R_KEY_FRAME_PATH);
		mLoaded = true;
	}
	// 创建所有通过公式计算的曲线
	protected void loadAllCaculatedCurve()
	{
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
	}
}