#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[DisallowMultipleComponent]
// UGUI生成器,根据预设生成窗口脚本的成员变量代码
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