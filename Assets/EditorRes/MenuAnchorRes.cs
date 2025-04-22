using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static FrameBaseUtility;
using static FrameBaseDefine;

public class MenuAnchorRes
{
	public const string mAutoAnchorMenuName = "GameAnchor/";
	public const string mScaleAnchorMenuName = "ScaleAnchor/";
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchorKeepAspectRes &3")]
	public static void addScaleAnchorKeepAspectRes()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addScaleAnchorRes(go, true);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchorRes")]
	public static void addScaleAnchorRes()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addScaleAnchorRes(go, false);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "RemoveAnchorRes &4")]
	public static void removeScaleAnchorRes()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			removeScaleAnchorRes(go);
		}
	}
	//-------------------------------------------------------------------------------------------------------------------
	protected static void addScaleAnchorRes(GameObject obj, bool keepAspect)
	{
		// 先设置自己的Anchor
		if (obj.TryGetComponent<RectTransform>(out _))
		{
			getOrAddComponent<ResScaleAnchor>(obj).mKeepAspect = keepAspect;
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addScaleAnchorRes(obj.transform.GetChild(i).gameObject, keepAspect);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void removeScaleAnchorRes(GameObject obj)
	{
		// 先销毁自己的Anchor
		destroyComponent<ResScaleAnchor>(obj);
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removeScaleAnchorRes(obj.transform.GetChild(i).gameObject);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static bool checkEditable()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return false;
		}
		PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
		if (prefabStage == null)
		{
			return false;
		}
		if (!prefabStage.assetPath.StartsWith(P_RESOURCES_PATH))
		{
			Debug.LogError("编辑的是不是Resources的Prefab,请使用其他方式添加");
			return false;
		}
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				Debug.LogError("所选择的物体必须在同一个父节点下");
				return false;
			}
		}
		return true;
	}
}