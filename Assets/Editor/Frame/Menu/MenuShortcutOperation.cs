using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static WidgetUtility;
using static MathUtility;
using static EditorCommonUtility;
using static FileUtility;
using static FrameDefine;

public class MenuShortcutOperation
{
	[MenuItem("快捷操作/启动游戏 _F5", false, 0)]
	public static void startGame()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE, OpenSceneMode.Single);
		}
		EditorApplication.isPlaying = !EditorApplication.isPlaying;
	}
	[MenuItem("快捷操作/暂停游戏 _F6", false, 1)]
	public static void pauseGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}
	}
	[MenuItem("快捷操作/单帧执行 _F7", false, 2)]
	public static void stepGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.Step();
		}
	}
	[MenuItem("快捷操作/打开初始场景 _F9", false, 3)]
	public static void jumpGameSceme()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE);
		}
	}
	[MenuItem("快捷操作/调整物体坐标为整数 _F1", false, 30)]
	public static void adjustUIRectToInt()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			return;
		}
		EditorUtility.SetDirty(go);
		roundTransformToInt(go.GetComponent<Transform>());
	}
	[MenuItem("快捷操作/调整父节点使其完全包含所有子节点的范围", false, 31)]
	public static void adjustParentRect()
	{
		var selection = Selection.activeGameObject.transform as RectTransform;
		if (selection == null)
		{
			Debug.LogError("节点必须有RectTransform组件");
			return;
		}

		// 获得父节点的四个边界
		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;
		int childCount = selection.childCount;
		if (childCount == 0)
		{
			Debug.LogError("选择的节点没有子节点");
			return;
		}
		var childWorldPositionList = new Dictionary<RectTransform, Vector3>();
		for (int i = 0; i < childCount; ++i)
		{
			var child = selection.GetChild(i) as RectTransform;
			if (child == null)
			{
				Debug.LogError("子节点必须有RectTransform组件");
				return;
			}
			Vector2 size = getRectSize(child);
			Vector3 pos = child.position;
			left = getMin(pos.x - size.x * 0.5f, left);
			right = getMax(pos.x + size.x * 0.5f, right);
			top = getMax(pos.y + size.y * 0.5f, top);
			bottom = getMin(pos.y - size.y * 0.5f, bottom);
			childWorldPositionList.Add(child, pos);
		}

		// 设置父节点新的位置和大小,重新设置所有子节点的世界坐标
		selection.position = new Vector3(ceil((right + left) * 0.5f), ceil((top + bottom) * 0.5f));
		setRectSize(selection, new Vector2(ceil(right - left), ceil(top - bottom)));
		foreach (var item in childWorldPositionList)
		{
			item.Key.position = item.Value;
		}
	}
	[MenuItem("快捷操作/删除所有空文件夹", false, 32)]
	public static void deleteAllEmptyFolder()
	{
		deleteEmptyFolder(F_GAME_RESOURCES_PATH);
	}
	[MenuItem("快捷操作/修复MeshCollider的模型引用", false, 33)]
	public static void fixMeshCollider()
	{
		GameObject[] selectObj = Selection.gameObjects;
		if (selectObj == null || selectObj.Length == 0)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}

		bool modified = false;
		foreach (var item in selectObj)
		{
			modified |= fixMeshOfMeshCollider(item);
		}
		if (modified)
		{
			EditorUtility.SetDirty(selectObj[0]);
		}
	}
	[MenuItem("快捷操作/生成选中对象的角色控制器", false, 34)]
	public static void createGameObjectCollider()
	{
		GameObject[] objects = Selection.gameObjects;
		if (objects == null)
		{
			EditorUtility.DisplayDialog("错误", "请先Project中选中需要设置的文件", "确定");
			return;
		}

		// 对选中的所有对象遍历生成角色控制器
		foreach (var item in objects)
		{
			var renderers = item.GetComponentsInChildren<Renderer>();
			if (renderers == null)
			{
				continue;
			}
			Vector3 center = Vector3.zero;
			foreach (var render in renderers)
			{
				center += render.bounds.center;
			}
			if (renderers.Length > 0)
			{
				center /= renderers.Length;
			}
			// 计算包围盒
			Bounds bounds = new Bounds(center, Vector3.zero);
			foreach (var render in renderers)
			{
				bounds.min = getMinVector3(bounds.min, render.bounds.min);
				bounds.max = getMaxVector3(bounds.max, render.bounds.max);
			}

			// 创建角色控制器并设置参数
			var controller = item.GetComponent<CharacterController>();
			if (controller == null)
			{
				controller = item.AddComponent<CharacterController>();
			}

			// 生成的高度不能小于半径的2倍,且不能大于4
			controller.height = bounds.size.y;
			controller.radius = clamp(bounds.size.x * 0.5f, 0.001f, controller.height * 0.5f);
			controller.center = new Vector3(0.0f, bounds.center.y - bounds.size.y * 0.5f + controller.height * 0.5f, 0.0f);
			controller.slopeLimit = 80.0f;
			controller.stepOffset = clampMax(0.3f, controller.height + controller.radius * 2.0f);
			controller.enabled = true;
			controller.skinWidth = 0.01f;
		}
	}
	[MenuItem("快捷操作/将缩放转换为位置和大小", false, 35)]
	public static void applyScaleToTransform()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		Vector3 scale = go.transform.localScale;
		var transforms = go.transform.GetComponentsInChildren<RectTransform>(true);
		foreach (var item in transforms)
		{
			item.localPosition = multiVector3(item.localPosition, scale);
			setRectSize(item, multiVector2(item.rect.size, scale));
		}
		go.transform.localScale = Vector3.one;
	}
	[MenuItem("快捷操作/将Image替换为SpriteRenderer &Q", false, 36)]
	public static void imageToSpriteRenderer()
	{
		GameObject[] objects = Selection.gameObjects;
		if (objects == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		for (int i = 0; i < objects.Length; ++i)
		{
			GameObject go = objects[i];
			Sprite sprite = null;
			Material material = null;
			var image = go.GetComponent<Image>();
			if (image != null)
			{
				sprite = image.sprite;
				material = image.material;
				GameObject.DestroyImmediate(image);
			}
			var canvasRenderer = go.GetComponent<CanvasRenderer>();
			if (canvasRenderer != null)
			{
				GameObject.DestroyImmediate(canvasRenderer);
			}
			var spriteRenderer = go.GetComponent<SpriteRenderer>();
			if (spriteRenderer == null)
			{
				spriteRenderer = go.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = material;
			}
		}
	}
	[MenuItem("快捷操作/将字体设置为宋体 &W", false, 36)]
	public static void setFontToSimsun()
	{
		GameObject[] objects = Selection.gameObjects;
		if (objects == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		for (int i = 0; i < objects.Length; ++i)
		{
			var text = objects[i].GetComponent<Text>();
			if (text == null)
			{
				continue;
			}
			text.font = AssetDatabase.LoadAssetAtPath<Font>(P_GAME_RESOURCES_PATH + "Font/simsunFull.ttf");
			EditorUtility.SetDirty(objects[i]);
		}
	}
}