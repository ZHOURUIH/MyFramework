using UnityEngine;

// Mesh相关无状态公共规则。
// 保存多个渲染子系统共同使用的常量和简单辅助逻辑，避免复制同一规则。

public static class FastUIMeshUtility
{
	public const int MAX_PARTIAL_UPLOAD_RANGE = 16;
	public static readonly Bounds HUGE_BOUNDS = new(Vector3.zero, new Vector3(100000.0f, 100000.0f, 10000.0f));
}
