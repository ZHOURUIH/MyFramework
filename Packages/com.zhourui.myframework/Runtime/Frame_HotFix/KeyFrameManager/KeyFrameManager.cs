using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameDefine;

// 关键帧曲线管理器
public class KeyFrameManager : FrameSystem
{
	protected Dictionary<int, MyCurve> mCurveList = new();	// 关键帧曲线列表
	protected bool mLoaded;									// 资源是否加载完毕
	public KeyFrameManager()
	{
		mCreateObject = true;
		loadAllCalculatedCurve(mCurveList);
	}
	public override void initAsync(Action callback)
	{
		mLoaded = false;

		// 在编辑器中编辑的曲线
		mResourceManager.loadGameResourceAsync<GameObject>(KEY_FRAME_FILE, (asset) =>
		{
			if (asset == null)
			{
				callback?.Invoke();
				return;
			}
			// 删除所有ID大于100的,也就是通过加载资源获得的曲线
			int deleteCount = 0;
			Span<int> deleteKeys = stackalloc int[mCurveList.Count];
			foreach (var item in mCurveList)
			{
				if (item.Key > KEY_CURVE.MAX_BUILDIN_CURVE)
				{
					deleteKeys[deleteCount++] = item.Key;
				}
			}
			for (int i = 0; i < deleteCount; ++i)
			{
				mCurveList.Remove(deleteKeys[i]);
			}

			// 查找关键帧曲线,加入列表中
			if (!asset.getResource().TryGetComponent<GameKeyframe>(out var gameKeyframe))
			{
				logError("object in KeyFrame folder must has GameKeyframe Component!");
				callback?.Invoke();
				return;
			}
			foreach (CurveInfo curveInfo in gameKeyframe.mCurveList)
			{
				if (curveInfo.mID <= KEY_CURVE.MAX_BUILDIN_CURVE)
				{
					logError("加载的曲线ID不能低于" + KEY_CURVE.MAX_BUILDIN_CURVE);
				}
				mCurveList.Add(curveInfo.mID, new UnityCurve(curveInfo.mCurve));
			}
			mResourceManager.unload(ref asset);
			mLoaded = true;
			callback?.Invoke();
		});
	}
	public MyCurve getKeyFrame(int id)
	{
		if (id == 0)
		{
			return null;
		}
		return mCurveList.get(id);
	}
	public override void destroy()
	{
		mCurveList.Clear();
		base.destroy();
	}
	public bool isLoadDone() { return mLoaded; }
	// 创建所有通过公式计算的曲线,因为其他地方也要用,所以写成静态公共的
	public static void loadAllCalculatedCurve(Dictionary<int, MyCurve> curveList)
	{
		createCurve<CurveZeroOne>(curveList, KEY_CURVE.ZERO_ONE);
		createCurve<CurveZeroOneZero>(curveList, KEY_CURVE.ZERO_ONE_ZERO);
		createCurve<CurveOneZero>(curveList, KEY_CURVE.ONE_ZERO);
		createCurve<CurveOneZeroOne>(curveList, KEY_CURVE.ONE_ZERO_ONE);
		createCurve<CurveBackIn>(curveList, KEY_CURVE.BACK_IN);
		createCurve<CurveBackInOut>(curveList, KEY_CURVE.BACK_IN_OUT);
		createCurve<CurveBackOut>(curveList, KEY_CURVE.BACK_OUT);
		createCurve<CurveBounceIn>(curveList, KEY_CURVE.BOUNCE_IN);
		createCurve<CurveBounceInOut>(curveList, KEY_CURVE.BOUNCE_IN_OUT);
		createCurve<CurveBounceOut>(curveList, KEY_CURVE.BOUNCE_OUT);
		createCurve<CurveCircleIn>(curveList, KEY_CURVE.CIRCLE_IN);
		createCurve<CurveCircleInOut>(curveList, KEY_CURVE.CIRCLE_IN_OUT);
		createCurve<CurveCircleOut>(curveList, KEY_CURVE.CIRCLE_OUT);
		createCurve<CurveCubicIn>(curveList, KEY_CURVE.CUBIC_IN);
		createCurve<CurveCubicInOut>(curveList, KEY_CURVE.CUBIC_IN_OUT);
		createCurve<CurveCubicOut>(curveList, KEY_CURVE.CUBIC_OUT);
		createCurve<CurveElasticIn>(curveList, KEY_CURVE.ELASTIC_IN);
		createCurve<CurveElasticInOut>(curveList, KEY_CURVE.ELASTIC_IN_OUT);
		createCurve<CurveElasticOut>(curveList, KEY_CURVE.ELASTIC_OUT);
		createCurve<CurveExpoIn>(curveList, KEY_CURVE.EXPO_IN);
		createCurve<CurveExpoInOut>(curveList, KEY_CURVE.EXPO_IN_OUT);
		createCurve<CurveExpoOut>(curveList, KEY_CURVE.EXPO_OUT);
		createCurve<CurveQuadIn>(curveList, KEY_CURVE.QUAD_IN);
		createCurve<CurveQuadInOut>(curveList, KEY_CURVE.QUAD_IN_OUT);
		createCurve<CurveQuadOut>(curveList, KEY_CURVE.QUAD_OUT);
		createCurve<CurveQuartIn>(curveList, KEY_CURVE.QUART_IN);
		createCurve<CurveQuartInOut>(curveList, KEY_CURVE.QUART_IN_OUT);
		createCurve<CurveQuartOut>(curveList, KEY_CURVE.QUART_OUT);
		createCurve<CurveQuintIn>(curveList, KEY_CURVE.QUINT_IN);
		createCurve<CurveQuintInOut>(curveList, KEY_CURVE.QUINT_IN_OUT);
		createCurve<CurveQuintOut>(curveList, KEY_CURVE.QUINT_OUT);
		createCurve<CurveSineIn>(curveList, KEY_CURVE.SINE_IN);
		createCurve<CurveSineInOut>(curveList, KEY_CURVE.SINE_IN_OUT);
		createCurve<CurveSineOut>(curveList, KEY_CURVE.SINE_OUT);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void createCurve<T>(Dictionary<int, MyCurve> curveList, int curveID) where T : MyCurve, new()
	{
		curveList.Add(curveID, new T());
	}
}