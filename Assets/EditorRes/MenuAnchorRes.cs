using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityUtility;
using static FrameDefine;

public class MenuAnchorRes
{
	public const string mAutoAnchorMenuName = "GameAnchor/";
	public const string mPaddingAnchorMenuName = "PaddingAnchor/";
	public const string mScaleAnchorMenuName = "ScaleAnchor/";
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "AddAnchorRes")]
	public static void addPaddingAnchorRes()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addPaddingAnchorRes(go);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "RemoveAnchorRes")]
	public static void removePaddingAnchorRes()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			removePaddingAnchorRes(go);
		}
	}
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
	protected static void addPaddingAnchorRes(GameObject obj)
	{
		// 先设置自己的Anchor
		if (obj.TryGetComponent<RectTransform>(out _) && !obj.TryGetComponent<ResPaddingAnchor>(out _))
		{
			obj.AddComponent<ResPaddingAnchor>().setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addPaddingAnchorRes(obj.transform.GetChild(i).gameObject);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void removePaddingAnchorRes(GameObject obj)
	{
		// 先销毁自己的Anchor
		if (obj.TryGetComponent<ResPaddingAnchor>(out var padding))
		{
			destroyUnityObject(padding, true);
		}
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removePaddingAnchorRes(obj.transform.GetChild(i).gameObject);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void addScaleAnchorRes(GameObject obj, bool keepAspect)
	{
		// 先设置自己的Anchor
		if (obj.TryGetComponent<RectTransform>(out _))
		{
			if (!obj.TryGetComponent<ResScaleAnchor>(out var anchor))
			{
				anchor = obj.AddComponent<ResScaleAnchor>();
			}
			anchor.mKeepAspect = keepAspect;
		}
		else
		{
			if (!obj.TryGetComponent<ResScaleAnchor3D>(out var _))
			{
				obj.AddComponent<ResScaleAnchor3D>();
			}
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
		if (obj.TryGetComponent<ResScaleAnchor>(out var anchor))
		{
			destroyUnityObject(anchor, true);
		}
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