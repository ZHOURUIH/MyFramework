using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using static EditorCommonUtility;
using static MathUtility;
using static WidgetUtility;
using static FrameBaseUtility;
using UObject = UnityEngine.Object;

public class MenuGameObject
{
	public const string mMenuName = "GameObject/";
	[MenuItem(mMenuName + "将缩放转换为位置和大小 &R", false, 35)]
	public static void applyScaleToTransform()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		Vector3 scale = go.transform.localScale;
		foreach (RectTransform item in go.transform.GetComponentsInChildren<RectTransform>(true))
		{
			if (item != go.transform)
			{
				item.localPosition = round(multiVector3(item.localPosition, scale));
			}
			setRectSize(item, round(multiVector2(item.rect.size, scale)));
		}
		foreach (Text item in go.transform.GetComponentsInChildren<Text>(true))
		{
			item.fontSize = (int)(item.fontSize * getMin(scale.x, scale.y));
		}
		foreach (TextMeshProUGUI item in go.transform.GetComponentsInChildren<TextMeshProUGUI>(true))
		{
			item.fontSize = (int)(item.fontSize * getMin(scale.x, scale.y));
		}
		go.transform.localScale = Vector3.one;
		EditorUtility.SetDirty(go);
	}
	[MenuItem(mMenuName + "修改UI位置大小与父节点一致 &T", false, 35)]
	public static void setPosSizeSameAsParent()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		var thisTrans = go.transform as RectTransform;
		var parent = go.transform.parent as RectTransform;
		if (thisTrans == null || parent == null)
		{
			Debug.LogError("当前节点和父节点都需要是RectTransform");
			return;
		}
		thisTrans.localPosition = Vector3.zero;
		setRectSize(thisTrans, parent.rect.size);
		EditorUtility.SetDirty(go);
	}
	[MenuItem(mMenuName + "将Image替换为SpriteRenderer &Q", false, 36)]
	public static void imageToSpriteRenderer()
	{
		if (Selection.gameObjects == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			Sprite sprite = null;
			Material material = null;
			if (go.TryGetComponent<Image>(out var image))
			{
				sprite = image.sprite;
				material = image.material;
				UObject.DestroyImmediate(image);
			}
			destroyComponent<CanvasRenderer>(go);
			if (getOrAddComponent(go, out SpriteRenderer spriteRenderer))
			{
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = material;
			}
			EditorUtility.SetDirty(go);
		}
	}
	[MenuItem(mMenuName + "调整物体坐标为整数 _F1", false, 30)]
	public static void adjustUIRectToInt()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			return;
		}
		EditorUtility.SetDirty(go);
		go.TryGetComponent<Transform>(out var trans);
		roundTransformToInt(trans);
	}
	[MenuItem(mMenuName + "调整节点位置和大小使其完全包含所有子节点的范围 %E", false, 31)]
	public static void adjustParentRect()
	{
		if (Selection.activeGameObject == null)
		{
			return;
		}
		var selection = Selection.activeGameObject.transform as RectTransform;
		if (selection == null)
		{
			Debug.LogError("节点必须有RectTransform组件");
			return;
		}
		if (selection.childCount == 0)
		{
			Debug.LogError("选择的节点没有子节点");
			return;
		}
		adjustRectTransformToContainsAllChildRect(selection);
	}
	[MenuItem(mMenuName + "将SpriteRenderer替换为Image", false, 31)]
	public static void toImage()
	{
		if (Selection.activeGameObject == null)
		{
			return;
		}
		spriteRendererToImage(Selection.activeGameObject);
	}
}