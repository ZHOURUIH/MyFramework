using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FastGUICreationMenu
{
	private const string MENU_ROOT = "GameObject/UI/FastGUI/";
	[MenuItem(MENU_ROOT + "Canvas", false, 2030)]
	private static void createCanvas(MenuCommand command)
	{
		GameObject gameObject = createRectObject("FastCanvas", null, new Vector2(1920.0f, 1080.0f), false);
		Undo.AddComponent<FastCanvas>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Image", false, 2031)]
	private static void createImage(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("Image", parent, new Vector2(100.0f, 100.0f), true);
		Undo.AddComponent<FastImage>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Raw Image", false, 2032)]
	private static void createRawImage(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("RawImage", parent, new Vector2(100.0f, 100.0f), true);
		Undo.AddComponent<FastRawImage>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Text", false, 2033)]
	private static void createText(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("Text", parent, new Vector2(240.0f, 60.0f), true);
		Undo.AddComponent<FastText>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Input Field", false, 2034)]
	private static void createInputField(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("InputField", parent, new Vector2(240.0f, 50.0f), true);
		FastInputField input = Undo.AddComponent<FastInputField>(gameObject);
		Undo.RegisterFullObjectHierarchyUndo(gameObject, "Create FastInputField Hierarchy");
		input.getTextComponent();
		input.getPlaceholderComponent();
		input.getViewportClip();
		input.getSelectionGraphic();
		input.getCaretGraphic();
		EditorUtility.SetDirty(input);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Visibility Group", false, 2040)]
	private static void createVisibilityRoot(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("Visibility Group", parent, new Vector2(100.0f, 100.0f), true);
		Undo.AddComponent<FastUIVisibility>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "Rect Mask 2D", false, 2041)]
	private static void createRectMask(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("Rect Mask 2D", parent, new Vector2(300.0f, 300.0f), true);
		Undo.AddComponent<FastRectMask2D>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	[MenuItem(MENU_ROOT + "SOA Render Group", false, 2042)]
	private static void createSOAGroup(MenuCommand command)
	{
		GameObject parent = resolveUIParent(command);
		GameObject gameObject = createRectObject("SOA Render Group", parent, new Vector2(100.0f, 100.0f), true);
		Undo.AddComponent<FastSOARenderGroup>(gameObject);
		Selection.activeGameObject = gameObject;
	}
	private static GameObject resolveUIParent(MenuCommand command)
	{
		GameObject selected = command.context as GameObject ?? Selection.activeGameObject;
		if (selected != null && selected.GetComponentInParent<FastCanvas>(true) != null)
		{
			return selected;
		}
		Scene scene = selected != null ? selected.scene : SceneManager.GetActiveScene();
		FastCanvas[] canvases = Object.FindObjectsByType<FastCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < canvases.Length; ++i)
		{
			if (canvases[i] != null && canvases[i].gameObject.scene == scene)
			{
				return canvases[i].gameObject;
			}
		}
		return createCanvasRoot();
	}
	private static GameObject createCanvasRoot()
	{
		GameObject canvasObject = createRectObject("FastCanvas", null, new Vector2(1920.0f, 1080.0f), false);
		Undo.AddComponent<FastCanvas>(canvasObject);
		return canvasObject;
	}
	private static GameObject createRectObject(string name, GameObject parent, Vector2 size, bool alignToParent)
	{
		GameObject gameObject = new(name, typeof(RectTransform));
		Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
		if (parent != null)
		{
			GameObjectUtility.SetParentAndAlign(gameObject, parent);
			gameObject.layer = parent.layer;
		}
		else
		{
			int uiLayer = LayerMask.NameToLayer("UI");
			if (uiLayer >= 0)
			{
				gameObject.layer = uiLayer;
			}
		}
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		if (alignToParent)
		{
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
		rectTransform.sizeDelta = size;
		rectTransform.localScale = Vector3.one;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.anchoredPosition3D = Vector3.zero;
		return gameObject;
	}
}
