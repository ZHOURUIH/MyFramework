using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.EventSystems;
using UObject = UnityEngine.Object;
using UDebug = UnityEngine.Debug;
using static CSharpUtility;
using static StringUtility;
using static MathUtility;
using static TimeUtility;
using static FrameDefine;
using static FrameDefineBase;
using static FrameUtility;
using static FrameBase;
using static FrameEditorUtility;

// 与Unity相关的工具函数
public class UnityUtility
{
	protected static bool mShowMessageBox = true;					// 是否显示报错提示框,用来判断提示框显示次数
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.FORCE;			// 当前的日志过滤等级
	protected static PointerEventData mEventData;                   // 缓存一个对象,避免每次都重新new一个
	protected static Vector2 mHardwareScreenSize = new(Screen.currentResolution.width, Screen.currentResolution.height);	// 显示器宽高
	protected static Vector2 mScreenSize = new(Screen.width, Screen.height);				// 窗口宽高
	protected static Vector2 mHalfScreenSize = new(Screen.width >> 1, Screen.height >> 1);  // 窗口宽高的一半
	protected static float mScreenAspect = mScreenSize.x / mScreenSize.y;					// 屏幕宽高比
	protected static Vector2 mScreenScale = new(mScreenSize.x * (1.0f / STANDARD_WIDTH), 
												mScreenSize.y * (1.0f / STANDARD_HEIGHT));	// 当前分辨率相对于标准分辨率的缩放
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		log("log level: " + mLogLevel);
	}
	public static LOG_LEVEL getLogLevel() { return mLogLevel; }
	public static void logException(Exception e, string info = null)
	{
		if (info.isEmpty())
		{
			info = "";
		}
		else
		{
			info += ", ";
		}
		info += e.Message + ", stack:" + e.StackTrace;
		if (e.InnerException != null)
		{
			info += "\ninner exception:" + e.InnerException.Message + ", stack:" + e.InnerException.StackTrace;
		}
		logError(info);
	}
	public static void logError(string info)
	{
		if (isMainThread() && mShowMessageBox)
		{
			// 在编辑器中显示对话框
			displayDialog("错误", info, "确认");
			setPause(true);
			// 运行一次只显示一次提示框,避免在循环中报错时一直弹窗
			mShowMessageBox = false;
		}
		// 此处不能使用MyStringBuilder拼接的字符串,因为可能会造成无限递归而堆栈溢出
		UDebug.LogError(getTimeNoBuilder(TIME_DISPLAY.HMSM) + ": error: " + info + "\nstack: " + new StackTrace().ToString());
	}
	public static void logNoLock(string info)
	{
		logNoLock(info, null, LOG_LEVEL.FORCE, null);
	}
	public static void log(string info)
	{
		log(info, null, LOG_LEVEL.FORCE, null);
	}
	public static void log(string info, LOG_LEVEL level)
	{
		log(info, null, level, null);
	}
	public static void log(string info, string color, LOG_LEVEL level, UObject obj)
	{
		if ((int)level < (int)mLogLevel)
		{
			return;
		}
		if(!color.isEmpty())
		{
			info = colorStringNoBuilder(color, info);
		}
		UDebug.Log(getNowTime(TIME_DISPLAY.HMSM) + ": " + info, obj);
	}
	public static void logNoLock(string info, string color, LOG_LEVEL level, UObject obj)
	{
		if ((int)level < (int)mLogLevel)
		{
			return;
		}
		string time = getTimeNoLock(TIME_DISPLAY.HMSM);
		if (!color.isEmpty())
		{
			info = colorStringNoBuilder(color, info);
		}
		UDebug.Log(time + ": " + info, obj);
	}
	public static void logWarning(string info)
	{
		UDebug.LogWarning(getNowTime(TIME_DISPLAY.HMSM) + ": " + info);
	}
	public static void setScreenSize(Vector2Int size, bool fullScreen)
	{
		mScreenSize = size;
		mHalfScreenSize = new(size.x >> 1, size.y >> 1);
		mScreenAspect = divide(mScreenSize.x, mScreenSize.y);   // 屏幕宽高比
		mScreenScale = new(mScreenSize.x * (1.0f / STANDARD_WIDTH), mScreenSize.y * (1.0f / STANDARD_HEIGHT));   // 当前分辨率相对于标准分辨率的缩放
		Screen.SetResolution(size.x, size.y, fullScreen);

		// UGUI
		GameObject uguiRootObj = getGameObject(UGUI_ROOT);
		uguiRootObj.TryGetComponent<RectTransform>(out var uguiRectTransform);
		uguiRectTransform.offsetMin = -mScreenSize * 0.5f;
		uguiRectTransform.offsetMax = mScreenSize * 0.5f;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		GameCamera camera = mCameraManager.getUICamera();
		if (camera != null)
		{
			FT.MOVE(camera, new(0.0f, 0.0f, -divide(mScreenSize.y * 0.5f, tan(camera.getFOVY(true) * 0.5f))));
		}
	}
	public static void destroyUnityObject(UObject obj, bool immediately = false, bool allowDestroyAssets = false)
	{
		destroyUnityObject(ref obj, immediately, allowDestroyAssets);
	}
	public static void destroyUnityObject<T>(ref T obj, bool immediately = false, bool allowDestroyAssets = false) where T : UObject
	{
		if (obj == null)
		{
			return;
		}
		if (immediately)
		{
			UObject.DestroyImmediate(obj, allowDestroyAssets);
		}
		else
		{
			UObject.Destroy(obj);
		}
		obj = null;
	}
	public static void getGameObjectWithTag(GameObject parent, string tag, List<GameObject> objList)
	{
		// 如果父节点为空,则不再查找,不支持全局查找,因为这样容易出错
		if (parent == null)
		{
			return;
		}
		Transform parentTrans = parent.transform;
		int childCount = parentTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			Transform child = parentTrans.GetChild(i);
			if (child.CompareTag(tag))
			{
				objList.Add(child.gameObject);
			}
			// 递归查找子节点
			getGameObjectWithTag(child.gameObject, tag, objList);
		}
	}
	public static GameObject getGameObject(string name, GameObject parent = null, bool errorIfNull = false, bool recursive = true)
	{
		if (name.isEmpty())
		{
			return null;
		}
		GameObject go = null;
		do
		{
			if (parent == null)
			{
				go = GameObject.Find(name);
				break;
			}
			Transform trans = parent.transform.Find(name);
			if (trans != null)
			{
				go = trans.gameObject;
				break;
			}
			// 如果父节点的第一级子节点中找不到,就递归查找
			if (recursive)
			{
				int childCount = parent.transform.childCount;
				for (int i = 0; i < childCount; ++i)
				{
					GameObject thisParent = parent.transform.GetChild(i).gameObject;
					go = getGameObject(name, thisParent, false, recursive);
					if (go != null)
					{
						break;
					}
				}
			}
		} while (false);

		if (go == null && errorIfNull)
		{
			if (parent == null)
			{
				logError("找不到物体,请确认物体存在且是已激活状态,parent为空时无法查找到未激活的物体:" + name );
			}
			else
			{
				logError("找不到物体,请确认是否存在:" + name + ", parent:" + (parent != null ? parent.name : EMPTY));
			}
		}
		return go;
	}
	public static GameObject createGameObject(string name, GameObject parent = null)
	{
		GameObject obj = new(name);
		setNormalProperty(obj, parent);
		return obj;
	}
	// 一般不会直接调用该函数,要创建物体时需要使用ObjectPool来创建和回收
	// parent为实例化后挂接的父节点
	// prefabName为预设名,带Resources下相对路径
	// name为实例化后的名字
	// 其他三个是实例化后本地的变换
	public static GameObject instantiatePrefab(GameObject parent, GameObject prefab, string name, bool active)
	{
		GameObject obj = UObject.Instantiate(prefab);
		setNormalProperty(obj, parent, name);
		if (obj.activeSelf != active)
		{
			obj.SetActive(active);
		}
		findShaders(obj);
		return obj;
	}
#if UNITY_6000_0_OR_NEWER
	public static async void instantiatePrefabAsync(GameObject prefab, string name, bool active, GameObjectCallback callback)
	{
		AsyncInstantiateOperation<GameObject> op = UObject.InstantiateAsync(prefab);
		await op;
		GameObject obj = op.Result[0];
		setNormalProperty(obj, null, name);
		if (obj.activeSelf != active)
		{
			obj.SetActive(active);
		}
		findShaders(obj);
		callback?.Invoke(obj);
	}
#endif
	public static void findMaterialShader(Material material)
	{
		// 在编辑器中从AssetBundle加载如果不重新查找材质,则会出现材质丢失的错误,但是真机上不查找却没有任何影响
		// 目前暂未查明具体原因,所以为了保证两端都显示正常,只在编辑器下才会重新查找材质
		// 可能是shader不匹配导致的,编辑器中需要PC的shader,而AssetBundle中只有移动端的shader
		if (isEditor())
		{
			if (material == null)
			{
				return;
			}
			string shaderName = material.shader.name;
			Shader shader = Shader.Find(shaderName);
			if (shader == null)
			{
				logError("找不到shader:" + shaderName);
				return;
			}
			if (!shader.isSupported)
			{
				logError("不支持shader:" + shaderName);
			}
			material.shader = shader;
		}
	}
	public static void findShaders(GameObject go)
	{
		// 普通渲染器
		using var a = new ListScope<Renderer>(out var renderers);
		go.GetComponentsInChildren(true, renderers);
		foreach (Renderer renderer in renderers)
		{
			foreach (Material item in renderer.materials)
			{
				findMaterialShader(item);
			}
		}
		// 可能会用到材质的组件
		using var b = new ListScope<Projector>(out var projectors);
		go.GetComponentsInChildren(true, projectors);
		foreach (Projector projector in projectors)
		{
			findMaterialShader(projector.material);
		}
	}
	public static void setNormalProperty(GameObject obj, GameObject parent)
	{
		setNormalProperty(obj, parent, null, Vector3.one, Vector3.zero, Vector3.zero);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name)
	{
		setNormalProperty(obj, parent, name, Vector3.one, Vector3.zero, Vector3.zero);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name, Vector3 scale, Vector3 rot, Vector3 pos)
	{
		Transform objTrans = obj.transform;
		Transform parentTrans = parent != null ? parent.transform : null;
		if (objTrans.parent != parentTrans)
		{
			objTrans.SetParent(parentTrans);
		}
		if (!isVectorEqual(objTrans.localPosition, pos))
		{
			objTrans.localPosition = pos;
		}
		if (!isVectorEqual(objTrans.localEulerAngles, rot))
		{
			objTrans.localEulerAngles = rot;
		}
		if (!isVectorEqual(objTrans.localScale, scale))
		{
			objTrans.localScale = scale;
		}
		if (!name.isEmpty())
		{
			objTrans.name = name;
		}
	}
	// screenPos是以屏幕左下角为原点的坐标
	public static Ray getCameraRay(Vector3 screenPos, Camera camera)
	{
		// 不再使用camera.ScreenPointToRay计算射线,因为在摄像机坐标值比较大,比如超过10000时,计算结果会产生比较大的误差
		// 屏幕坐标转换为相对坐标,以左下角为原点,左上角y为1,右下角x为1
		Vector2 relativeScreenPos = divideVector2(screenPos, getScreenSize());
		if (camera.orthographic)
		{
			// 在近裁剪面上的投射点
			Vector2 clipSize = new(camera.orthographicSize * 2.0f * camera.aspect, camera.orthographicSize * 2.0f);
			Vector3 nearClipPoint = replaceZ(multiVector2(relativeScreenPos, clipSize) - clipSize * 0.5f, camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			Vector3 farClipPoint = replaceZ(multiVector2(relativeScreenPos, clipSize) - clipSize * 0.5f, camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
		}
		else
		{
			// 在近裁剪面上的投射点
			float nearClipHeight = tan(toRadian(camera.fieldOfView * 0.5f)) * camera.nearClipPlane * 2.0f;
			Vector2 nearClipSize = new(nearClipHeight * camera.aspect, nearClipHeight);
			Vector3 nearClipPoint = replaceZ(multiVector2(relativeScreenPos, nearClipSize) - nearClipSize * 0.5f, camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			float farClipHeight = tan(toRadian(camera.fieldOfView * 0.5f)) * camera.farClipPlane * 2.0f;
			Vector2 farClipSize = new(farClipHeight * camera.aspect, farClipHeight);
			Vector3 farClipPoint = replaceZ(multiVector2(relativeScreenPos, farClipSize) - farClipSize * 0.5f, camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
		}
	}
	// screenCenterAsZero为false表示返回的坐标是以屏幕左下角为原点的坐标
	// screenCenterAsZero为true表示返回的坐标是以屏幕中心为原点的坐标
	public static Vector3 worldToScreen(Vector3 worldPos, Camera camera, bool screenCenterAsZero = true)
	{
		Vector3 screenPosition = camera.WorldToScreenPoint(worldPos);
		if (screenCenterAsZero)
		{
			screenPosition -= (Vector3)(getHalfScreenSize());
		}
		screenPosition.z = 0.0f;
		return screenPosition;
	}
	public static void setGameObjectLayer(GameObject obj, int layer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = layer;
		using var a = new ListScope<Transform>(out var childTransformList);
		obj.transform.GetComponentsInChildren(true, childTransformList);
		foreach (Transform trans in childTransformList)
		{
			trans.gameObject.layer = layer;
		}
	}
	public static T getComponentInParent<T>(GameObject obj) where T : Component
	{
		if (obj == null)
		{
			return null;
		}
		Transform transform = obj.transform;
		Transform parent = transform.parent;
		if (parent == null)
		{
			return null;
		}
		if (parent.TryGetComponent(out T com))
		{
			return com;
		}
		return getComponentInParent<T>(parent.gameObject);
	}
	// 通过WWW加载本地资源时,需要确保路径的前缀正确
	public static void checkDownloadPath(ref string path)
	{
		if (isEditor())
		{
			// 本地加载需要添加file:///前缀
			if (!startWith(path, "file:///"))
			{
				path = "file:///" + path;
			}
		}
		// 非编辑器模式下
		else
		{
			if (isWindows())
			{
				// windows本地加载需要添加file:///前缀
				if (!startWith(path, "file:///"))
				{
					path = "file:///" + path;
				}
			}
			else if (isIOS())
			{
				// ios本地加载需要添加file://前缀
				if (!startWith(path, "file://"))
				{
					path = "file://" + path;
				}
			}
			else if (isMacOS())
			{
				// macos本地加载需要添加file://前缀
				if (!startWith(path, "file://"))
				{
					path = "file://" + path;
				}
			}
			else if (isLinux())
			{
				// linux本地加载需要添加file://前缀
				if (!startWith(path, "file://"))
				{
					path = "file://" + path;
				}
			}
			else if (isAndroid())
			{
				// android本地加载需要添加jar:file://前缀
				if (!startWith(path, "jar:file://"))
				{
					path = "jar:file://" + path;
				}
			}
		}
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
	public static Vector3 generateWorldScale(Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.localScale;
		}
		return multiVector3(generateWorldScale(transform.parent), transform.localScale);
	}
	public static Quaternion generateWorldRotation(Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.localRotation;
		}
		return generateWorldRotation(transform.parent) * transform.localRotation;
	}
	public static Vector3 generateWorldPosition(Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.localPosition;
		}
		Vector3 localPosition = transform.localPosition;
		localPosition = rotateVector3(localPosition, generateWorldRotation(transform.parent));
		localPosition = multiVector3(localPosition, generateWorldScale(transform.parent));
		return localPosition + generateWorldPosition(transform.parent);
	}
	public static string getTransformPath(Transform transform)
	{
		if (transform == null)
		{
			return EMPTY;
		}
		if (transform.parent == null)
		{
			return transform.name;
		}
		return getTransformPath(transform.parent) + "/" + transform.name;
	}
	public static void applyAnchor(GameObject obj, bool force, GameLayout layout = null)
	{
		obj.TryGetComponent<ResScaleAnchor>(out var scaleAnchor);
		obj.TryGetComponent<ResScaleAnchor3D>(out var scaleAnchor3D);
		obj.TryGetComponent<ResPaddingAnchor>(out var paddingAnchor);
		if (paddingAnchor != null || (scaleAnchor != null && scaleAnchor.mRemoveUGUIAnchor))
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
		if(scaleAnchor3D != null)
		{
			scaleAnchor3D.updateRect(force);
		}
		if (paddingAnchor != null)
		{
			paddingAnchor.updateRect(force);
		}
		layout?.getUIObject(obj)?.notifyAnchorApply();

		// 然后更新所有子节点
		Transform curTrans = obj.transform;
		int childCount = curTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			applyAnchor(curTrans.GetChild(i).gameObject, force, layout);
		}
	}
	public static Vector2 getGameViewSize()
	{
#if UNITY_EDITOR
		Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
		MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
		return (Vector2)GetSizeOfMainGameView.Invoke(null, null);
#else
		logError("getGameViewSize can only call in editor!");
		return Vector2.zero;
#endif
	}
	public static Vector2 getScreenSize() { return mScreenSize; }
	public static Vector2 getHalfScreenSize() { return mHalfScreenSize; }
	public static Vector2 getRootSize() { return getUGUIRoot().getWindowSize(); }
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		return new(rootSize.x * (1.0f / STANDARD_WIDTH), rootSize.y * (1.0f / STANDARD_HEIGHT));
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
				newScale.x = getMin(screenScale.x, screenScale.y);
				newScale.y = getMin(screenScale.x, screenScale.y);
			}
			else if (aspectBase == ASPECT_BASE.INVERSE_AUTO)
			{
				newScale.x = getMax(screenScale.x, screenScale.y);
				newScale.y = getMax(screenScale.x, screenScale.y);
			}
		}
		// Z轴按照Y轴的缩放值来缩放
		newScale.z = newScale.y;
		return newScale;
	}
	public static GameObject getTopParent(GameObject go)
	{
		if (go.transform.parent == null)
		{
			return go;
		}
		return getTopParent(go.transform.parent.gameObject);
	}
}