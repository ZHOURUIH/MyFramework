using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static FrameDefine;
using static FrameBaseUtility;

// 仅在编辑器模式下用于获取指定ID的曲线
public static class EditorCurveFactory
{
	private static Dictionary<int, MyCurve> mStaticCurveList = new();
	private static Dictionary<int, MyCurve> mUnityCurveList = new();
	private static Dictionary<int, MyCurve> mCurveList = new();
	private static string[] mStaticNames;
	private static int[] mStaticIDs;
	private static string[] mNames;
	private static int[] mIDs;
	static EditorCurveFactory()
	{
		// 加载内置公式曲线
		KeyFrameManager.loadAllCalculatedCurve(mStaticCurveList);
		buildStaticNames();
		reload();
	}
	public static void reload()
	{
		// 加载Unity编辑曲线
		loadUnityCurves();
		buildFinalNames();
		mCurveList.setRange(mStaticCurveList);
		mCurveList.addRange(mUnityCurveList);
	}
	public static MyCurve getCurve(int id)
	{
		return mCurveList.get(id);
	}
	public static AnimationCurve getPreviewCurve(int id)
	{
		MyCurve curve = getCurve(id);
		if (curve == null)
		{
			return new AnimationCurve();
		}
		if (curve is UnityCurve unityCurve)
		{
			return unityCurve.getAnimationCurve();
		}
		AnimationCurve preview = new();
		for (int i = 0; i <= 50; ++i)
		{
			float t = i / 50.0f;
			preview.AddKey(t, curve.evaluate(t));
		}
		return preview;
	}
	public static string[] getNames()
	{
		return mNames;
	}
	public static int[] getIDs()
	{
		return mIDs;
	}
	//------------------------------------------------------------------------
	private static void loadUnityCurves()
	{
		mUnityCurveList.Clear();
		GameObject keyframeGo = loadAssetAtPath<GameObject>(P_GAME_RESOURCES_PATH + KEY_FRAME_FILE);
		if (keyframeGo == null)
		{
			return;
		}
		GameKeyframe keyframe = keyframeGo.GetComponent<GameKeyframe>();
		foreach (CurveInfo curveInfo in keyframe.mCurveList)
		{
			mUnityCurveList[curveInfo.mID] = new UnityCurve(curveInfo.mCurve);
		}
	}
	private static void buildFinalNames()
	{
		List<string> names = new(mStaticNames);
		List<int> ids = new(mStaticIDs);
		foreach (var item in mUnityCurveList)
		{
			names.add(item.Key.ToString());
			ids.add(item.Key);
		}
		mNames = names.ToArray();
		mIDs = ids.ToArray();
	}
	private static void buildStaticNames()
	{
		if (mStaticNames != null)
		{
			return;
		}
		List<string> names = new();
		List<int> ids = new();
		foreach (FieldInfo field in typeof(KEY_CURVE).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (names.addIf(field.Name, field.FieldType == typeof(int) && field.Name != "NONE" && field.Name != "MAX_BUILDIN_CURVE"))
			{
				ids.Add((int)field.GetValue(null));
			}
		}
		mStaticNames = names.ToArray();
		mStaticIDs = ids.ToArray();
	}
}