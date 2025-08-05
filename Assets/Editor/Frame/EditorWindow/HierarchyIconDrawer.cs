using UnityEditor;
using UnityEngine;
using static UGUIGeneratorUtility;

[InitializeOnLoad]
public class HierarchyIconDrawer
{
	protected static readonly Texture2D greenIcon;
	protected static readonly Texture2D redIcon;
	static HierarchyIconDrawer()
	{
		// 使用Unity内置图标资源
		greenIcon = EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image as Texture2D;
		redIcon = EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo").image as Texture2D;
		EditorApplication.hierarchyWindowItemOnGUI += drawHierarchyIcon;
	}
	protected static void drawHierarchyIcon(int instanceID, Rect rect)
	{
		var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
		if (go == null || !needDrawIcon(go))
		{
			return;
		}

		Rect iconRect = new(rect.x + rect.width - 24, rect.y, 18, 18);
		// 显示图标附加文字提示
		GUI.DrawTexture(iconRect, getIcon(go));
		// 鼠标悬停显示提示（可选）
		if (iconRect.Contains(Event.current.mousePosition))
		{
			EditorGUI.LabelField(new Rect(iconRect.x - 50, iconRect.y - 18, 120, 16), getTip(go));
		}
	}
	protected static bool needDrawIcon(GameObject go)
	{
		return go.GetComponent<UGUISubGenerator>() != null;
	}
	protected static Texture2D getIcon(GameObject go)
	{
		bool hasScript = ClassTypeCaches.hasClass(getClassNameFromGameObject(go));
		return hasScript ? greenIcon : redIcon;
	}
	protected static string getTip(GameObject go)
	{
		bool hasScript = ClassTypeCaches.hasClass(getClassNameFromGameObject(go));
		return hasScript ? "Script Exists" : "Missing Script";
	}
}