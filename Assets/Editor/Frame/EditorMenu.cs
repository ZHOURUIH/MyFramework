using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

public class EditorMenu : EditorCommonUtility
{
	[MenuItem("快捷操作/启动游戏 _F5", false, 0)]
	public static void StartGame()
	{
		if(!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.P_RESOURCES_SCENE_PATH + "start.unity", OpenSceneMode.Single);
			EditorApplication.isPlaying = true;
		}
		else
		{
			EditorApplication.isPlaying = false;
		}
	}
	[MenuItem("快捷操作/暂停游戏 _F6", false, 0)]
	public static void PauseGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}
	}
	[MenuItem("快捷操作/单帧执行 _F7", false, 0)]
	public static void StepGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.Step();
		}
	}
	[MenuItem("快捷操作/打开初始场景 _F9")]
	public static void JumpGameSceme()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.P_RESOURCES_SCENE_PATH + "start.unity");
		}
	}
	[MenuItem("快捷操作/调整物体坐标为整数 _F1")]
	public static void adjustUIRectToInt()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			return;
		}
		EditorUtility.SetDirty(go);
		roundRectTransformToInt(go.GetComponent<RectTransform>());
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void roundRectTransformToInt(RectTransform rectTransform)
	{
		if (rectTransform == null)
		{
			return;
		}
		rectTransform.localPosition = round(rectTransform.localPosition);
		WidgetUtility.setRectSize(rectTransform, round(WidgetUtility.getRectSize(rectTransform)), false);
		int childCount = rectTransform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			roundRectTransformToInt(rectTransform.GetChild(i) as RectTransform);
		}
	}
}