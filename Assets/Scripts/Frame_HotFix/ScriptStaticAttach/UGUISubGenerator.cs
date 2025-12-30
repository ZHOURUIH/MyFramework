#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public class UGUISubGenerator : UGUIGeneratorBase
{
	public string mParentType;			// 基类的类型
	public bool mAutoType = true;		// 是否根据GameObject的名字自动设置类型名
	public string mCustomClassName;     // 如果不是自动设置类型名字,则需要自己输入类名
	public bool mOnlyForMarkType;       // 是否仅用于标记,而不是用于生成代码
#if UNITY_EDITOR
	private void OnValidate()
	{
		// 必须是 Canvas
		if (GetComponent<Canvas>() != null)
		{
			Debug.LogError("UGUISubGenerator 不能挂在 Canvas 上", this);
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