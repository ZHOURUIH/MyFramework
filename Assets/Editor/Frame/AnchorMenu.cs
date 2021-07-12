using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AnchorMenu
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
		if (go.transform.parent == null || go.transform.parent.name != FrameDefine.UGUI_ROOT)
		{
			Debug.LogError("选中的节点必须是" + FrameDefine.UGUI_ROOT + "的一级子节点");
			return;
		}
		if (go.GetComponent<Canvas>() == null)
		{
			Debug.LogError("选中的节点必须拥有Canvas组件");
			return;
		}

		Vector2 gameViewSize = UnityUtility.getGameViewSize();
		string info = "是否要预览" + go.name + "在" + gameViewSize.x + "x" + gameViewSize.y + "下的显示?";
		if (!EditorUtility.DisplayDialog("预览", info, "确定", "取消"))
		{
			return;
		}
		// 设置摄像机的Z坐标为视图高的一半,设置画布根节点为屏幕大小
		GameObject uguiRootObj = UnityUtility.getGameObject(FrameDefine.UGUI_ROOT);
		RectTransform transform = uguiRootObj.GetComponent<RectTransform>();
		transform.offsetMin = new Vector2(-gameViewSize.x * 0.5f, -gameViewSize.y * 0.5f);
		transform.offsetMax = new Vector2(gameViewSize.x * 0.5f, gameViewSize.y * 0.5f);
		transform.anchorMax = Vector2.zero;
		transform.anchorMin = Vector2.zero;
		GameObject camera = UnityUtility.getGameObject(FrameDefine.UI_CAMERA, uguiRootObj, true);
		camera.transform.localPosition = new Vector3(0.0f, 0.0f, -gameViewSize.y * 0.5f);
		UnityUtility.applyAnchor(go, true);
	}
	[MenuItem(mAutoAnchorMenuName + "End PreviewAnchor %.")]
	public static void endPreviewLayoutAnchor()
	{
		// 恢复摄像机设置
		GameObject uguiRootObj = UnityUtility.getGameObject(FrameDefine.UGUI_ROOT);
		RectTransform transform = uguiRootObj.GetComponent<RectTransform>();
		transform.offsetMin = new Vector2(-FrameDefineExtension.STANDARD_WIDTH * 0.5f, -FrameDefineExtension.STANDARD_HEIGHT * 0.5f);
		transform.offsetMax = new Vector2(FrameDefineExtension.STANDARD_WIDTH * 0.5f, FrameDefineExtension.STANDARD_HEIGHT * 0.5f);
		transform.anchorMax = Vector2.zero;
		transform.anchorMin = Vector2.zero;
		GameObject uguiCamera = UnityUtility.getGameObject(FrameDefine.UI_CAMERA, uguiRootObj, true);
		uguiCamera.transform.localPosition = new Vector3(0.0f, 0.0f, -FrameDefineExtension.STANDARD_HEIGHT * 0.5f);
	}
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "AddAnchor")]
	public static void addPaddingAnchor()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			addPaddingAnchor(Selection.gameObjects[i]);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mPaddingAnchorMenuName + "RemoveAnchor")]
	public static void removePaddingAnchor()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			removePaddingAnchor(Selection.gameObjects[i]);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchorKeepAspect &1")]
	public static void addScaleAnchorKeepAspect()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			addScaleAnchor(Selection.gameObjects[i], true);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "SetRaycastTargetFalse &3")]
	public static void setRaycastTargetFalse()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			setSeleteRaycastTarget(Selection.gameObjects[i], false);
		}
	}
	public static void setSeleteRaycastTarget(GameObject obj, bool target)
	{
		// 先设置自己的Anchor
		if (obj.GetComponent<Graphic>() != null)
		{
			obj.GetComponent<Graphic>().raycastTarget = target;
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			setSeleteRaycastTarget(obj.transform.GetChild(i).gameObject, target);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "AddAnchor")]
	public static void addScaleAnchor()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			addScaleAnchor(Selection.gameObjects[i], false);
		}
	}
	[MenuItem(mAutoAnchorMenuName + mScaleAnchorMenuName + "RemoveAnchor &2")]
	public static void removeScaleAnchor()
	{
		if (Selection.gameObjects.Length <= 0)
		{
			return;
		}
		// 所选择的物体必须在同一个父节点下
		Transform parent = Selection.transforms[0].parent;
		int count = Selection.gameObjects.Length;
		for (int i = 1; i < count; ++i)
		{
			if (parent != Selection.transforms[i].parent)
			{
				UnityUtility.logError("objects must have the same parent!");
				return;
			}
		}
		for (int i = 0; i < count; ++i)
		{
			removeScaleAnchor(Selection.gameObjects[i]);
		}
	}
	//-------------------------------------------------------------------------------------------------------------------
	public static void addPaddingAnchor(GameObject obj)
	{
		// 先设置自己的Anchor
		if (obj.GetComponent<PaddingAnchor>() == null)
		{
			obj.AddComponent<PaddingAnchor>().setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
		}
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addPaddingAnchor(obj.transform.GetChild(i).gameObject);
		}
	}
	public static void removePaddingAnchor(GameObject obj)
	{
		// 先销毁自己的Anchor
		if (obj.GetComponent<PaddingAnchor>() != null)
		{
			UnityUtility.destroyGameObject(obj.GetComponent<PaddingAnchor>(), true);
		}
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removePaddingAnchor(obj.transform.GetChild(i).gameObject);
		}
	}
	public static void addScaleAnchor(GameObject obj, bool keepAspect)
	{
		// 先设置自己的Anchor
		if (obj.GetComponent<ScaleAnchor>() == null)
		{
			obj.AddComponent<ScaleAnchor>();
		}
		obj.GetComponent<ScaleAnchor>().mKeepAspect = keepAspect;
		// 再设置子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			addScaleAnchor(obj.transform.GetChild(i).gameObject, keepAspect);
		}
	}
	public static void removeScaleAnchor(GameObject obj)
	{
		// 先销毁自己的Anchor
		if (obj.GetComponent<ScaleAnchor>() != null)
		{
			UnityUtility.destroyGameObject(obj.GetComponent<ScaleAnchor>(), true);
		}
		// 再销毁子节点的Anchor
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			removeScaleAnchor(obj.transform.GetChild(i).gameObject);
		}
	}
}