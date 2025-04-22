using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class PrefabNodeLocator
{
	// 在 Prefab 编辑模式下通过名称定位节点
	public static void FocusNodeInPrefabMode(string targetName)
	{
		// 1. 检查是否处于 Prefab 编辑模式
		PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
		if (prefabStage == null)
		{
			Debug.LogWarning("当前未处于 Prefab 编辑模式！");
			return;
		}
		// 2. 获取 Prefab 根对象
		GameObject prefabRoot = prefabStage.prefabContentsRoot;
		if (prefabRoot == null)
		{
			Debug.LogWarning("无法获取 Prefab 根对象！");
			return;
		}
		// 3. 递归查找目标节点
		Transform target = FindChildByName(prefabRoot.transform, targetName);
		if (target == null)
		{
			Debug.LogWarning($"未找到名为 {targetName} 的节点！");
			return;
		}
		// 4. 强制展开父级并定位
		ExpandParentsRecursive(target);
		Selection.activeGameObject = target.gameObject;
		EditorGUIUtility.PingObject(target.gameObject);
	}
	// 递归查找子节点（支持嵌套）
	private static Transform FindChildByName(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		foreach (Transform child in parent)
		{
			Transform result = FindChildByName(child, name);
			if (result != null)
			{
				return result;
			}
		}
		return null;
	}
	// 递归展开父级节点
	private static void ExpandParentsRecursive(Transform target)
	{
		if (target.parent == null)
			return;
		ExpandParentsRecursive(target.parent);
		SetExpanded(target.parent.gameObject, true);
	}
	// 通过反射强制展开节点
	private static void SetExpanded(GameObject go, bool expand)
	{
		var sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
		var window = EditorWindow.GetWindow(sceneHierarchyWindowType);
		var setExpandedMethod = sceneHierarchyWindowType.GetMethod("SetExpanded");
		setExpandedMethod?.Invoke(window, new object[] { go.GetInstanceID(), expand });
	}
}