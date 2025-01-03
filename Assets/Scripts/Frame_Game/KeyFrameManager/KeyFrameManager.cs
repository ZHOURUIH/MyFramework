using System;
using System.Collections.Generic;

// 关键帧曲线管理器
public class KeyFrameManager : FrameSystem
{
	protected Dictionary<int, MyCurve> mCurveList = new();  // 关键帧曲线列表
	protected bool mLoaded;                                 // 资源是否加载完毕
	public KeyFrameManager()
	{
		mCreateObject = true;
		loadAllCaculatedCurve();
	}
	// 加载所有KeyFrame下的关键帧,Game部分的不需要曲线
	public override void initAsync(Action callback)
	{
		mLoaded = false;
		callback?.Invoke();
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void createCurve<T>(int curveID) where T : MyCurve, new()
	{
		mCurveList.Add(curveID, new T());
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