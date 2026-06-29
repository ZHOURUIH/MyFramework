using System;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;
using static FrameBaseUtility;

// 与Unity相关的工具函数
public class UnityUtility
{
	// 一般不会直接调用该函数,要创建物体时需要使用ObjectPool来创建和回收
	// parent为实例化后挂接的父节点
	// prefabName为预设名,带Resources下相对路径
	// name为实例化后的名字
	// 其他三个是实例化后本地的变换
	public static GameObject instantiatePrefab(GameObject parent, GameObject prefab, string name, bool active)
	{
		GameObject obj = UObject.Instantiate(prefab);
		Transform objTrans = obj.transform;
		Transform parentTrans = parent != null ? parent.transform : null;
		if (objTrans.parent != parentTrans)
		{
			objTrans.SetParent(parentTrans);
		}
		objTrans.localPosition = Vector3.zero;
		objTrans.localEulerAngles = Vector3.zero;
		objTrans.localScale = Vector3.one;
		if (!name.isEmpty())
		{
			objTrans.name = name;
		}
		if (obj.activeSelf != active)
		{
			obj.SetActive(active);
		}
		return obj;
	}
	// 通过WWW加载本地资源时,需要确保路径的前缀正确
	public static void checkDownloadPath(ref string path)
	{
		if (isEditor())
		{
			// 本地加载需要添加file:///前缀
			path = path.ensurePrefix("file:///");
		}
		// 非编辑器模式下
		else
		{
			if (isWindows())
			{
				// windows本地加载需要添加file:///前缀
				path = path.ensurePrefix("file:///");
			}
			else if (isIOS())
			{
				// ios本地加载需要添加file://前缀
				path = path.ensurePrefix("file://");
			}
			else if (isMacOS())
			{
				// macos本地加载需要添加file://前缀
				path = path.ensurePrefix("file://");
			}
			else if (isLinux())
			{
				// linux本地加载需要添加file://前缀
				path = path.ensurePrefix("file://");
			}
			else if (isAndroid())
			{
				// android本地加载需要添加jar:file://前缀
				path = path.ensurePrefix("jar:file://");
			}
		}
	}
	public static void applyAnchor(GameObject obj, bool force, GameLayout layout = null)
	{
		obj.TryGetComponent<ResScaleAnchor>(out var scaleAnchor);
		obj.TryGetComponent<ResPaddingAnchor>(out var paddingAnchor);
		if (scaleAnchor != null || paddingAnchor != null)
		{
			// 去除UGUI自带的锚点,避免计算错误
			if (obj.TryGetComponent<RectTransform>(out var rectTransform))
			{
				rectTransform.anchorMin = Vector2.one * 0.5f;
				rectTransform.anchorMax = Vector2.one * 0.5f;
			}
		}

		// 先更新自己
		if (scaleAnchor != null)
		{
			scaleAnchor.updateRect(force);
		}
		if (paddingAnchor != null)
		{
			paddingAnchor.updateRect(force);
		}

		// 然后更新所有子节点
		Transform curTrans = obj.transform;
		int childCount = curTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			applyAnchor(curTrans.GetChild(i).gameObject, force, layout);
		}
	}
	public static Vector3 adjustScreenScale(Vector2 screenScale, ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		Vector3 newScale = screenScale;
		if (aspectBase != ASPECT_BASE.NONE)
		{
			if (aspectBase == ASPECT_BASE.USE_HEIGHT_SCALE)
			{
				newScale.x = screenScale.y;
				newScale.y = screenScale.y;
			}
			else if (aspectBase == ASPECT_BASE.USE_WIDTH_SCALE)
			{
				newScale.x = screenScale.x;
				newScale.y = screenScale.x;
			}
			else if (aspectBase == ASPECT_BASE.AUTO)
			{
				newScale.x = Mathf.Min(screenScale.x, screenScale.y);
				newScale.y = Mathf.Min(screenScale.x, screenScale.y);
			}
			else if (aspectBase == ASPECT_BASE.INVERSE_AUTO)
			{
				newScale.x = Mathf.Max(screenScale.x, screenScale.y);
				newScale.y = Mathf.Max(screenScale.x, screenScale.y);
			}
		}
		// Z轴按照Y轴的缩放值来缩放
		newScale.z = newScale.y;
		return newScale;
	}
	public static MethodInfo getMethodRecursive(Type type, string methodName, BindingFlags flags)
	{
		while (type != null)
		{
			MethodInfo method = type.GetMethod(methodName, flags);
			if (method != null)
			{
				return method;
			}
			type = type.BaseType;
		}
		return null;
	}
	public static Vector3 localToWorld(Transform transform, Vector3 local)
	{
		if (transform == null)
		{
			return Vector3.zero;
		}
		return transform.localToWorldMatrix.MultiplyPoint(local);
	}
	public static Vector3 worldToLocal(Transform transform, Vector3 world)
	{
		if (transform == null)
		{
			return Vector3.zero;
		}
		return transform.worldToLocalMatrix.MultiplyPoint(world);
	}
}