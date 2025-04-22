using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement; 
using static FrameDefine;
using static FrameBaseDefine;
using static UnityUtility;
using static FrameBaseUtility;

public class MenuAnchor
{
	public const string mAutoAnchorMenuName = "GameAnchor/";
	public const string mPaddingAnchorMenuName = "PaddingAnchor/";
	public const string mScaleAnchorMenuName = "ScaleAnchor/";
	[MenuItem(mAutoAnchorMenuName + "Start PreviewAnchor %,")]
	public static void startPreviewLayoutAnchor()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要选中场景结构面板中的节点");
			return;
		}
		if (go.transform.parent == null || go.transform.parent.name != UGUI_ROOT)
		{
			Debug.LogError("选中的节点必须是" + UGUI_ROOT + "的一级子节点");
			return;
		}
		if (!go.TryGetComponent<Canvas>(out _))
		{
			Debug.LogError("选中的节点必须拥有Canvas组件");
			return;
		}

		Vector2 gameViewSize = getGameViewSize();
		string info = "是否要预览" + go.name + "在" + gameViewSize.x + "x" + gameViewSize.y + "下的显示?";
		if (!EditorUtility.DisplayDialog("预览", info, "确定", "取消"))
		{
			return;
		}
		// 设置摄像机的Z坐标为视图高的一半,设置画布根节点为屏幕大小
		GameObject uguiRootObj = getRootGameObject(UGUI_ROOT);
		uguiRootObj.TryGetComponent<RectTransform>(out var transform);
		transform.offsetMin = new(-gameViewSize.x * 0.5f, -gameViewSize.y * 0.5f);
		transform.offsetMax = new(gameViewSize.x * 0.5f, gameViewSize.y * 0.5f);
		transform.anchorMax = Vector2.zero;
		transform.anchorMin = Vector2.zero;
		GameObject camera = getGameObject(UI_CAMERA, uguiRootObj, true);
		camera.transform.localPosition = new(0.0f, 0.0f, -gameViewSize.y * 0.5f);
		applyAnchor(go, true);
	}
	[MenuItem(mAutoAnchorMenuName + "End PreviewAnchor %.")]
	public static void endPreviewLayoutAnchor()
	{
		// 恢复摄像机设置
		GameObject uguiRootObj = getRootGameObject(UGUI_ROOT);
		uguiRootObj.TryGetComponent<RectTransform>(out var transform);
		transform.offsetMin = new(-STANDARD_WIDTH >> 1, -STANDARD_HEIGHT >> 1);
		transform.offsetMax = new(STANDARD_WIDTH >> 1, STANDARD_HEIGHT >> 1);
		transform.anchorMax = Vector2.zero;
		transform.anchorMin = Vector2.zero;
		GameObject uguiCamera = getGameObject(UI_CAMERA, uguiRootObj, true);
		uguiCamera.transform.localPosition = new(0.0f, 0.0f, -STANDARD_HEIGHT >> 1);
	}
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "AddAnchor")]
	public static void addPaddingAnchor()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addPaddingAnchor(go);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "RemoveAnchor")]
	public static void removePaddingAnchor()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			removePaddingAnchor(go);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchorKeepAspect &1")]
	public static void addScaleAnchorKeepAspect()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addScaleAnchor(go, true);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchor")]
	public static void addScaleAnchor()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			addScaleAnchor(go, false);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "RemoveAnchor &2")]
	public static void removeScaleAnchor()
	{
		if (!checkEditable())
		{
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			removeScaleAnchor(go);
		}
	}
	//-------------------------------------------------------------------------------------------------------------------
	protected static void addPaddingAnchor(GameObject obj)
	{
		// 先设置自己的Anchor
		if (obj.TryGetComponent<RectTransform>(out _) && getOrAddComponent(obj, out PaddingAnchor com))
		{
			com.setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addPaddingAnchor(obj.transform.GetChild(i).gameObject);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void removePaddingAnchor(GameObject obj)
	{
		// 先销毁自己的Anchor
		destroyComponent<PaddingAnchor>(obj);
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removePaddingAnchor(obj.transform.GetChild(i).gameObject);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void addScaleAnchor(GameObject obj, bool keepAspect)
	{
		// 先设置自己的Anchor
		if (obj.TryGetComponent<RectTransform>(out _))
		{
			getOrAddComponent<ScaleAnchor>(obj).mKeepAspect = keepAspect;
		}
		else
		{
			getOrAddComponent<ScaleAnchor3D>(obj);
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addScaleAnchor(obj.transform.GetChild(i).gameObject, keepAspect);
		}
		EditorUtility.SetDirty(obj);
	}
	protected static void removeScaleAnchor(GameObject obj)
	{
		// 先销毁自己的Anchor
		destroyComponent<ScaleAnchor>(obj);
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removeScaleAnchor(obj.transform.GetChild(i).gameObject);
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
		if (prefabStage.assetPath.StartsWith(P_RESOURCES_PATH))
		{
			Debug.LogError("编辑的是Resources的Prefab,请使用其他方式添加");
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