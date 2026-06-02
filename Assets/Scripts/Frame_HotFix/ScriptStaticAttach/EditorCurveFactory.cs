using System.Collections.Generic;

// 仅在编辑器模式下用于获取指定ID的曲线
public static class EditorCurveFactory
{
	private static Dictionary<int, MyCurve> mCurveList;
	public static MyCurve getCurve(int id)
	{
		if (mCurveList == null)
		{
			mCurveList = new();
			KeyFrameManager.loadAllCalculatedCurve(mCurveList);
		}
		return mCurveList.get(id);
	}
}