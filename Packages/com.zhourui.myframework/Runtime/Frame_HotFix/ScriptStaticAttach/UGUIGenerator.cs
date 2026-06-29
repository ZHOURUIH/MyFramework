#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public class UGUIGenerator : UGUIGeneratorBase
{
	public string mParentType;			// 基类的类型
	public bool mIsPersistent;          // 是否为常驻界面
#if UNITY_EDITOR
	private void OnValidate()
	{
		// 必须是 Canvas
		if (GetComponent<Canvas>() == null)
		{
			Debug.LogError("UGUIGenerator 必须挂在 Canvas 上", this);
			// 延迟销毁
			EditorApplication.delayCall += () =>
			{
				if (this != null)
				{
					DestroyImmediate(this);
				}
			};
			return;
		}
	}
#endif
}