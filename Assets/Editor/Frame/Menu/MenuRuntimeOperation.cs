using UnityEditor;
using UnityEngine;
using static FrameBaseHotFix;

public class MenuRuntimeOperation
{
	[MenuItem("快捷操作/热重载表格数据 (experimental)", false, 39)]
	public static void reloadExcelTables()
	{
		Debug.LogWarning("热重载是实验性功能，不能保证所有更改都能成功应用，部分更改需要手动刷新当前游戏界面才能成功应用。因为靠id来匹配新旧数据，无法处理修改id的情况。可能会产生异常，请谨慎使用该功能！");
		mExcelManager.reloadAllAsync(() =>
		{
			Debug.Log("成功热重载表格！");
		});
	}
}
