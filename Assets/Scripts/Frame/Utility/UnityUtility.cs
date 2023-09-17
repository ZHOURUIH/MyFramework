using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static CSharpUtility;
using static StringUtility;
using static MathUtility;
using static TimeUtility;
using static FrameDefine;

// 与Unity相关的工具函数
public class UnityUtility
{
	protected static HashSet<OnLog> mOnLog = new HashSet<OnLog>();	// 用于可以让其他地方监听日志打印
	protected static bool mShowMessageBox = true;					// 是否显示报错提示框,用来判断提示框显示次数
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.FORCE;			// 当前的日志过滤等级
	protected static PointerEventData mEventData;                   // 缓存一个对象,避免每次都重新new一个
	protected static Vector2 mHardwareScreenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);	// 显示器宽高
	protected static Vector2 mScreenSize = new Vector2(Screen.width, Screen.height);				// 窗口宽高
	protected static Vector2 mHalfScreenSize = new Vector2(Screen.width >> 1, Screen.height >> 1);  // 窗口宽高的一半
	protected static float mScreenAspect = mScreenSize.x / mScreenSize.y;							// 屏幕宽高比
	protected static Vector2 mScreenScale = new Vector2(mScreenSize.x * (1.0f / STANDARD_WIDTH), 
														mScreenSize.y * (1.0f / STANDARD_HEIGHT));	// 当前分辨率相对于标准分辨率的缩放
	public static void addLogCallback(OnLog callback) { mOnLog.Add(callback); }
	public static void removeLogCallback(OnLog callback) { mOnLog.Remove(callback); }
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		logForce("log level: " + mLogLevel);
	}
	public static LOG_LEVEL getLogLevel() { return mLogLevel; }
	public static void logException(Exception e, string info = null)
	{
		if (isEmpty(info))
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
			messageBox(info, true);
#if UNITY_EDITOR
			EditorApplication.isPaused = true;
#endif
			// 运行一次只显示一次提示框,避免在循环中报错时一直弹窗
			mShowMessageBox = false;
		}
		// 此处不能使用MyStringBuilder拼接的字符串,因为可能会造成无限递归而堆栈溢出
		string time = getTimeNoBuilder(TIME_DISPLAY.HMSM);
		string trackStr = new StackTrace().ToString();
		UnityEngine.Debug.LogError(time + ": error: " + info + "\nstack: " + trackStr);
		foreach(var item in mOnLog)
		{
			item?.Invoke(time, "error: " + info + ", stack: " + trackStr, LOG_LEVEL.FORCE, true);
		}
	}
	public static void logForceNoLock(string info)
	{
		logNoLock(info, null, LOG_LEVEL.FORCE, null);
	}
	public static void logForce(string info)
	{
		log(info, null, LOG_LEVEL.FORCE, null);
	}
	public static void logForce(string info, UnityEngine.Object obj)
	{
		log(info, null, LOG_LEVEL.FORCE, obj);
	}
	public static void logForce(string info, string color)
	{
		log(info, color, LOG_LEVEL.FORCE, null);
	}
	public static void logForce(string info, Color32 color)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.FORCE, null);
	}
	public static void log(string info)
	{
		log(info, null, LOG_LEVEL.NORMAL, null);
	}
	public static void log(string info, LOG_LEVEL level)
	{
		log(info, null, level, null);
	}
	public static void log(string info, string color)
	{
		log(info, color, LOG_LEVEL.NORMAL, null);
	}
	public static void log(string info, Color32 color)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.NORMAL, null);
	}
	public static void log(string info, string color, LOG_LEVEL level, UnityEngine.Object obj)
	{
		if ((int)level < (int)mLogLevel)
		{
			return;
		}
		string time;
		if (isMainThread())
		{
			time = getNowTime(TIME_DISPLAY.HMSM);
		}
		else
		{
			time = getTimeThread(TIME_DISPLAY.HMSM);
		}
		if(!isEmpty(color))
		{
			info = colorStringNoBuilder(color, info);
		}
		UnityEngine.Debug.Log(time + ": " + info, obj);
		foreach (var item in mOnLog)
		{
			item?.Invoke(time, info, level, false);
		}
	}
	public static void logNoLock(string info, string color, LOG_LEVEL level, UnityEngine.Object obj)
	{
		if ((int)level < (int)mLogLevel)
		{
			return;
		}
		string time = getTimeNoLock(TIME_DISPLAY.HMSM);
		if (!isEmpty(color))
		{
			info = colorStringNoBuilder(color, info);
		}
		UnityEngine.Debug.Log(time + ": " + info, obj);
	}
	public static void logWarning(string info)
	{
		string time;
		if (isMainThread())
		{
			time = getNowTime(TIME_DISPLAY.HMSM);
		}
		else
		{
			time = getTimeThread(TIME_DISPLAY.HMSM);
		}
		UnityEngine.Debug.LogWarning(time + ": " + info);
		foreach (var item in mOnLog)
		{
			item?.Invoke(time, info, LOG_LEVEL.FORCE, false);
		}
	}
	public static void messageBox(string info, bool errorOrInfo)
	{
		string title = errorOrInfo ? "错误" : "提示";
		// 在编辑器中显示对话框
#if UNITY_EDITOR
		EditorUtility.DisplayDialog(title, info, "确认");
#endif
	}
	public static void setScreenSize(Vector2 size, bool fullScreen)
	{
		setScreenSize(new Vector2Int((int)size.x, (int)size.y), fullScreen);
	}
	public static void setScreenSize(Vector2Int size, bool fullScreen)
	{
		mScreenSize = size;
		mHalfScreenSize = new Vector2(size.x >> 1, size.y >> 1);
		mScreenAspect = mScreenSize.x / mScreenSize.y;   // 屏幕宽高比
		mScreenScale = new Vector2(mScreenSize.x * (1.0f / STANDARD_WIDTH), mScreenSize.y * (1.0f / STANDARD_HEIGHT));   // 当前分辨率相对于标准分辨率的缩放
		Screen.SetResolution(size.x, size.y, fullScreen);

		// UGUI
		GameObject uguiRootObj = getGameObject(UGUI_ROOT);
		var uguiRectTransform = uguiRootObj.GetComponent<RectTransform>();
		uguiRectTransform.offsetMin = -mScreenSize * 0.5f;
		uguiRectTransform.offsetMax = mScreenSize * 0.5f;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		GameCamera camera = FrameBase.mCameraManager.getUICamera();
		if (camera != null)
		{
			FT.MOVE(camera, new Vector3(0.0f, 0.0f, -mScreenSize.y * 0.5f / tan(camera.getFOVY(true) * 0.5f)));
		}
		GameCamera blurCamera = FrameBase.mCameraManager.getUIBlurCamera();
		if (blurCamera != null)
		{
			FT.MOVE(blurCamera, new Vector3(0.0f, 0.0f, -mScreenSize.y * 0.5f / tan(blurCamera.getFOVY(true) * 0.5f)));
		}
	}
	public static void destroyGameObject(UnityEngine.Object obj, bool immediately = false, bool allowDestroyAssets = false)
	{
		destroyGameObject(ref obj, immediately, allowDestroyAssets);
	}
	public static void destroyGameObject<T>(ref T obj, bool immediately = false, bool allowDestroyAssets = false) where T : UnityEngine.Object
	{
		if (obj == null)
		{
			return;
		}
		if (immediately)
		{
			GameObject.DestroyImmediate(obj, allowDestroyAssets);
		}
		else
		{
			GameObject.Destroy(obj);
		}
		obj = null;
	}
	public static List<GameObject> getGameObjectWithTag(GameObject parent, string tag)
	{
		List<GameObject> objList = new List<GameObject>();
		getGameObjectWithTag(parent, tag, objList);
		return objList;
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
	public static GameObject getGameObject(string name, bool errorIfNull = false, bool recursive = true)
	{
		return getGameObject(name, null, errorIfNull, recursive);
	}
	public static GameObject getGameObject(string name, GameObject parent, bool errorIfNull = false, bool recursive = true)
	{
		if (isEmpty(name))
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
			logError("can not find " + name + ", parent:" + (parent != null ? parent.name : EMPTY));
		}
		return go;
	}
	public static GameObject cloneObject(GameObject oriObj, string name)
	{
		GameObject obj = UnityEngine.Object.Instantiate(oriObj);
		obj.name = name;
		return obj;
	}
	public static GameObject createGameObject(string name, GameObject parent = null)
	{
		GameObject obj = new GameObject(name);
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
		GameObject obj = UnityEngine.Object.Instantiate(prefab);
		setNormalProperty(obj, parent, name);
		obj.SetActive(active);
		findShaders(obj);
		return obj;
	}
	public static void findMaterialShader(Material material)
	{
		// 在编辑器中从AssetBundle加载如果不重新查找材质,则会出现材质丢失的错误,但是真机上不查找却没有任何影响
		// 目前暂未查明具体原因,所以为了保证两端都显示正常,只在编辑器下才会重新查找材质
#if UNITY_EDITOR
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
#endif
	}
	public static void findShaders(GameObject go)
	{
		// 普通渲染器
		var renderers = go.GetComponentsInChildren<Renderer>(true);
		int rendererCount = renderers.Length;
		for (int i = 0; i < rendererCount; ++i)
		{
			Material[] rendererMaterials = renderers[i].materials;
			int count = rendererMaterials.Length;
			for (int j = 0; j < count; ++j)
			{
				findMaterialShader(rendererMaterials[j]);
			}
		}
		// 可能会用到材质的组件
		var projectors = go.GetComponentsInChildren<Projector>(true);
		int projectorCount = projectors.Length;
		for (int i = 0; i < projectorCount; ++i)
		{
			findMaterialShader(projectors[i].material);
		}
	}
	public static void findUGUIShaders(GameObject go)
	{
		var renderers = go.GetComponentsInChildren<Graphic>(true);
		int rendererCount = renderers.Length;
		for (int i = 0; i < rendererCount; ++i)
		{
			findMaterialShader(renderers[i].material);
		}
	}
	public static void raycastUGUI(Vector2 screenPosition, List<RaycastResult> results)
	{
		if (mEventData == null)
		{
			mEventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
		}
		// 将点击位置的屏幕坐标赋值给点击事件
		mEventData.position = new Vector2(screenPosition.x, screenPosition.y);
		// 向点击处发射射线
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(mEventData, results);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent)
	{
		setNormalProperty(obj, parent, null, Vector3.one, Vector3.zero, Vector3.zero);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name)
	{
		setNormalProperty(obj, parent, name, Vector3.one, Vector3.zero, Vector3.zero);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name, Vector3 pos)
	{
		setNormalProperty(obj, parent, name, Vector3.one, Vector3.zero, pos);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, Vector3 pos)
	{
		setNormalProperty(obj, parent, null, Vector3.one, Vector3.zero, pos);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name, Vector3 scale, Vector3 rot, Vector3 pos)
	{
		Transform objTrans = obj.transform;
		Transform parentTrans = parent?.transform;
		if (objTrans.parent != parentTrans)
		{
			objTrans.SetParent(parentTrans);
		}
		objTrans.localPosition = pos;
		objTrans.localEulerAngles = rot;
		objTrans.localScale = scale;
		if (!isEmpty(name))
		{
			objTrans.name = name;
		}
	}
#if UNITY_EDITOR || UNITY_STANDALONE
	public static Ray getMainCameraMouseRay()
	{
		return getCameraRay(FrameUtility.getMousePosition(), FrameUtility.getMainCamera().getCamera());
	}
#endif
	public static Ray getMainCameraRay(Vector3 screenPos)
	{
		return getCameraRay(screenPos, FrameUtility.getMainCamera().getCamera());
	}
	// screenPos是以屏幕左下角为原点的坐标
	public static Ray getCameraRay(Vector3 screenPos, Camera camera)
	{
		// 不再使用camera.ScreenPointToRay计算射线,因为在摄像机坐标值比较大,比如超过10000时,计算结果会产生比较大的误差
		// 屏幕坐标转换为相对坐标,以左下角为原点,左上角y为1,右下角x为1
		Vector2 relativeScreenPos = devideVector2(screenPos, getScreenSize());
		if (camera.orthographic)
		{
			// 在近裁剪面上的投射点
			Vector2 clipSize = new Vector2(camera.orthographicSize * 2.0f * camera.aspect, camera.orthographicSize * 2.0f);
			Vector3 nearClipPoint = replaceZ(multiVector2(relativeScreenPos, clipSize) - clipSize * 0.5f, camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			Vector3 farClipPoint = replaceZ(multiVector2(relativeScreenPos, clipSize) - clipSize * 0.5f, camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new Ray(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
		}
		else
		{
			// 在近裁剪面上的投射点
			float nearClipHeight = tan(toRadian(camera.fieldOfView * 0.5f)) * camera.nearClipPlane * 2.0f;
			Vector2 nearClipSize = new Vector2(nearClipHeight * camera.aspect, nearClipHeight);
			Vector3 nearClipPoint = replaceZ(multiVector2(relativeScreenPos, nearClipSize) - nearClipSize * 0.5f, camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			float farClipHeight = tan(toRadian(camera.fieldOfView * 0.5f)) * camera.farClipPlane * 2.0f;
			Vector2 farClipSize = new Vector2(farClipHeight * camera.aspect, farClipHeight);
			Vector3 farClipPoint = replaceZ(multiVector2(relativeScreenPos, farClipSize) - farClipSize * 0.5f, camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new Ray(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
		}
	}
	// screenPos是以屏幕左下角为原点的坐标
	public static Ray getUIRay(Vector3 screenPos)
	{
		return getCameraRay(screenPos, FrameUtility.getUICamera());
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
	public static Vector3 worldToScreen(Vector3 worldPos, bool screenCenterAsZero = true)
	{
		return worldToScreen(worldPos, FrameUtility.getMainCamera().getCamera(), screenCenterAsZero);
	}
	public static Vector3 worldUIToScreen(Vector3 worldPos)
	{
		return worldToScreen(worldPos, FrameUtility.getUICamera());
	}
	public static bool isGameObjectInScreen(Vector3 worldPos)
	{
		Vector3 screenPos = worldToScreen(worldPos, false);
		return screenPos.z >= 0.0f && inRange((Vector2)screenPos, Vector2.zero, getRootSize());
	}
	public static bool isPointInWindow(Vector2 screenPos, myUGUIObject window)
	{
		Camera camera = FrameUtility.getUICamera();
		Vector2 cameraSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = multiVector2(devideVector2(screenPos, cameraSize), rootSize) - rootSize * 0.5f;

		myUGUIObject root = FrameBase.mLayoutManager.getUIRoot();
		Vector2 parentWorldPosition = devideVector3(window.getWorldPosition(), root.getScale());
		Vector2 windowPos = devideVector2(screenPos - parentWorldPosition, window.getWorldScale());
		Vector2 halfWindowSize = window.getWindowSize() * 0.5f;
		return inRange(windowPos, -halfWindowSize, halfWindowSize);
	}
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示以window的左下角为原点
	public static Vector2 screenPosToWindow(Vector2 screenPos, myUIObject window, bool windowCenterAsZero = true)
	{
		Camera camera = FrameUtility.getUICamera();
		Vector2 cameraSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = multiVector2(devideVector2(screenPos, cameraSize), rootSize) - rootSize * 0.5f;
		Vector2 windowPos = screenPos;
		if (window != null)
		{
			myUGUIObject root = FrameBase.mLayoutManager.getUIRoot();
			Vector2 parentWorldPosition = devideVector3(window.getWorldPosition(), root.getScale());
			windowPos = devideVector2(screenPos - parentWorldPosition, window.getWorldScale());
			if (!windowCenterAsZero)
			{
				windowPos += window.getWindowSize() * 0.5f;
			}
		}
		else
		{
			if (!windowCenterAsZero)
			{
				windowPos += rootSize * 0.5f;
			}
		}
		return windowPos;
	}
	// 判断点是否在摄像机背面
	public static bool atCameraBack(Vector3 position, GameCamera camera)
	{
		return dot(normalize(position - camera.getPosition()), camera.getForward()) <= 0;
	}
	public static bool atCameraBack(Vector3 position)
	{
		return atCameraBack(position, FrameUtility.getMainCamera());
	}
	public static void setGameObjectLayer(GameObject obj, string layerName)
	{
		if (obj == null)
		{
			return;
		}
		int layer = LayerMask.NameToLayer(layerName);
		if (!inRangeFixed(layer, 1, 32))
		{
			return;
		}
		setGameObjectLayer(obj, layer);
	}
	public static void setGameObjectLayer(GameObject obj, int layer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = layer;
		var childTransformList = obj.transform.GetComponentsInChildren<Transform>(true);
		int count = childTransformList.Length;
		for (int i = 0; i < count; ++i)
		{
			childTransformList[i].gameObject.layer = layer;
		}
	}
	public static void setParticleSortOrder(GameObject obj, int sortOrder)
	{
		var renderers = obj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; ++i)
		{
			renderers[i].sortingOrder = sortOrder;
		}
	}
	public static void setParticleSortLayerID(GameObject obj, int layerID)
	{
		var renderers = obj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; ++i)
		{
			renderers[i].sortingLayerID = layerID;
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
		T com = parent.GetComponent<T>();
		if (com != null)
		{
			return com;
		}
		return getComponentInParent<T>(parent.gameObject);
	}
	public static int nameToLayerInt(string name)
	{
		int layer = LayerMask.NameToLayer(name);
		clamp(ref layer, 1, 32);
		return layer;
	}
	public static int nameToLayerPhysics(string name)
	{
		return 1 << nameToLayerInt(name);
	}
	public static void activeChilds(GameObject go, bool active = true)
	{
		if (go == null)
		{
			return;
		}
		Transform transform = go.transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			transform.GetChild(i).gameObject.SetActive(active);
		}
	}
	public static Sprite texture2DToSprite(Texture2D tex)
	{
		if (tex == null)
		{
			return null;
		}
		return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
	// 通过WWW加载本地资源时,需要确保路径的前缀正确
	public static void checkDownloadPath(ref string path)
	{
#if UNITY_EDITOR
		// 本地加载需要添加file:///前缀
		if (!startWith(path, "file:///"))
		{
			path = "file:///" + path;
		}
#else
		// 非编辑器模式下
#if UNITY_STANDALONE_WIN
		// windows本地加载需要添加file:///前缀
		if (!startWith(path, "file:///"))
		{
			path = "file:///" + path;
		}
#elif UNITY_IOS
		// ios本地加载需要添加file://前缀
		if (!startWith(path, "file://"))
		{
			path = "file://" + path;
		}
#elif UNITY_STANDALONE_OSX
		// macos本地加载需要添加file://前缀
		if (!startWith(path, "file://"))
		{
			path = "file://" + path;
		}
#elif UNITY_STANDALONE_LINUX
		// linux本地加载需要添加file://前缀
		if (!startWith(path, "file://"))
		{
			path = "file://" + path;
		}
#elif UNITY_ANDROID
		// android本地加载需要添加file://前缀
		if (!startWith(path, "file://"))
		{
			path = "file://" + path;
		}
#endif
#endif
	}
	// 计算的是旋转和缩放以后的包围盒的大小的一半, 如果填了parent,则会将尺寸转成parent坐标系中的值
	public static Vector3 getHalfBoxSize(BoxCollider collider, GameObject parent)
	{
		Vector3 worldBoxHalfSize = localToWorldDirection(collider.transform, collider.size) * 0.5f;
		worldBoxHalfSize.x = abs(worldBoxHalfSize.x);
		worldBoxHalfSize.y = abs(worldBoxHalfSize.y);
		worldBoxHalfSize.z = abs(worldBoxHalfSize.z);
		if (parent != null)
		{
			worldBoxHalfSize = worldToLocalDirection(parent.transform, worldBoxHalfSize);
		}
		return worldBoxHalfSize;
	}
	public static Vector3 localToWorld(Transform transform, Vector3 local)
	{
		return transform.localToWorldMatrix.MultiplyPoint(local);
	}
	public static Vector3 worldToLocal(Transform transform, Vector3 world)
	{
		return transform.worldToLocalMatrix.MultiplyPoint(world);
	}
	public static Vector3 localToWorldDirection(Transform transform, Vector3 local)
	{
		return transform.localToWorldMatrix.MultiplyVector(local);
	}
	public static Vector3 worldToLocalDirection(Transform transform, Vector3 world)
	{
		return transform.worldToLocalMatrix.MultiplyVector(world);
	}
	// 计算碰撞盒在parent坐标系中的最大点和最小点
	public static void getMinMaxCorner(BoxCollider box, out Vector3 min, out Vector3 max, GameObject parent, int precision = 4)
	{
		Vector3 halfSize = getHalfBoxSize(box, parent);
		Vector3 worldBoxCenter = worldToLocal(parent.transform, localToWorld(box.transform, box.center));
		Vector3 corner0 = worldBoxCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
		Vector3 corner1 = worldBoxCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
		Vector3 corner2 = worldBoxCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
		Vector3 corner3 = worldBoxCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
		Vector3 corner4 = worldBoxCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
		Vector3 corner5 = worldBoxCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
		Vector3 corner6 = worldBoxCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z);
		Vector3 corner7 = worldBoxCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
		min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		getMinMaxVector3(corner0, ref min, ref max);
		getMinMaxVector3(corner1, ref min, ref max);
		getMinMaxVector3(corner2, ref min, ref max);
		getMinMaxVector3(corner3, ref min, ref max);
		getMinMaxVector3(corner4, ref min, ref max);
		getMinMaxVector3(corner5, ref min, ref max);
		getMinMaxVector3(corner6, ref min, ref max);
		getMinMaxVector3(corner7, ref min, ref max);
		checkFloat(ref min, precision);
		checkFloat(ref max, precision);
	}
	// 两个碰撞盒相交的条件是box0.min小于box1.max,并且box0.max大于box1.min
	public static bool overlapBox(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		return isVector3Less(min0, max1) && isVector3Greater(max0, min1) ||
			   isVector3Less(min1, max0) && isVector3Greater(max1, min0);
	}
	public static int overlapAllBox(BoxCollider collider, Collider[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 colliderWorldPos = localToWorld(transform, collider.center);
		int hitCount = Physics.OverlapBoxNonAlloc(colliderWorldPos, collider.size * 0.5f, results, transform.localRotation, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllBox(BoxCollider2D collider, Collider2D[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
		int hitCount = Physics2D.OverlapBoxNonAlloc(colliderWorldPos, collider.size, transform.localEulerAngles.z, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllSphere(SphereCollider collider, Collider[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 colliderWorldPos = localToWorld(transform, collider.center);
		int hitCount = Physics.OverlapSphereNonAlloc(colliderWorldPos, collider.radius, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllSphere(CircleCollider2D collider, Collider2D[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
		int hitCount = Physics2D.OverlapCircleNonAlloc(colliderWorldPos, collider.radius, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllCapsule(CapsuleCollider collider, Collider[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 point0 = collider.center + new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		Vector3 point1 = collider.center - new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		point0 = localToWorld(transform, point0);
		point1 = localToWorld(transform, point1);
		int hitCount = Physics.OverlapCapsuleNonAlloc(point0, point1, collider.radius, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllCapsule(CharacterController collider, Collider[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 point0 = collider.center + new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		Vector3 point1 = collider.center - new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		point0 = localToWorld(transform, point0);
		point1 = localToWorld(transform, point1);
		int hitCount = Physics.OverlapCapsuleNonAlloc(point0, point1, collider.radius, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static int overlapAllCapsule(CapsuleCollider2D collider, Collider2D[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		float eulerZ = transform.localEulerAngles.z;
		int hitCount = Physics2D.OverlapCapsuleNonAlloc(transform.position, collider.size, collider.direction, eulerZ, results, layer);
		return removeClassElement(results, hitCount, collider);
	}
	public static bool overlapBoxIgnoreY(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		min0.y = 0.0f;
		max0.y = 1.0f;
		min1.y = 0.0f;
		max1.y = 1.0f;
		return isVector3Less(min0, max1) && isVector3Greater(max0, min1) ||
			   isVector3Less(min1, max0) && isVector3Greater(max1, min0);
	}
	public static bool overlapBoxIgnoreZ(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		min0.z = 0.0f;
		max0.z = 1.0f;
		min1.z = 0.0f;
		max1.z = 1.0f;
		return isVector3Less(min0, max1) && isVector3Greater(max0, min1) ||
			   isVector3Less(min1, max0) && isVector3Greater(max1, min0);
	}
	public static int overlapCollider(Collider collider, Collider[] results, int layer = -1)
	{
		if (collider == null)
		{
			return 0;
		}
		int maxCount = results.Length;
		for (int i = 0; i < maxCount; ++i)
		{
			results[i] = null;
		}
		if (collider is BoxCollider)
		{
			return overlapAllBox(collider as BoxCollider, results, layer);
		}
		else if (collider is SphereCollider)
		{
			return overlapAllSphere(collider as SphereCollider, results, layer);
		}
		else if (collider is CapsuleCollider)
		{
			return overlapAllCapsule(collider as CapsuleCollider, results, layer);
		}
		return 0;
	}
	public static int overlapCollider(Collider2D collider, Collider2D[] results, int layer = -1)
	{
		if (collider == null)
		{
			return 0;
		}
		int maxCount = results.Length;
		for (int i = 0; i < maxCount; ++i)
		{
			results[i] = null;
		}
		int hitCount = 0;
		if (collider is BoxCollider2D)
		{
			hitCount = overlapAllBox(collider as BoxCollider2D, results, layer);
		}
		else if (collider is CircleCollider2D)
		{
			hitCount = overlapAllSphere(collider as CircleCollider2D, results, layer);
		}
		else if (collider is CapsuleCollider2D)
		{
			hitCount = overlapAllCapsule(collider as CapsuleCollider2D, results, layer);
		}
		return hitCount;
	}
	public static bool raycast(Ray ray, out Collider result, out Vector3 point, int layer = -1)
	{
		return raycast(ray, out result, out point, 10000.0f, layer);
	}
	public static bool raycast(Ray ray, out Collider result, out Vector3 point, float maxDistance, int layer = -1)
	{
		result = null;
		point = Vector3.zero;
		if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layer))
		{
			return false;
		}
		result = hitInfo.collider;
		point = hitInfo.point;
		return true;
	}
	public static int raycastAll(Ray ray, RaycastHit[] result, int layer = -1)
	{
		return raycastAll(ray, result, 10000.0f, layer);
	}
	public static int raycastAll(Ray ray, RaycastHit[] result, float maxDistance, int layer = -1)
	{
		if (result == null)
		{
			return 0;
		}
		clampMin(ref maxDistance);
		return Physics.RaycastNonAlloc(ray, result, maxDistance, layer);
	}
	public static bool getRaycastPoint(Collider collider, Ray ray, ref Vector3 intersectPoint)
	{
		return getRaycastPoint(collider, ray, ref intersectPoint, 10000.0f);
	}
	public static bool getRaycastPoint(Collider collider, Ray ray, ref Vector3 intersectPoint, float maxDistance)
	{
		clampMin(ref maxDistance);
		if (collider.Raycast(ray, out RaycastHit hit, maxDistance))
		{
			intersectPoint = hit.point;
			return true;
		}
		return false;
	}
	public static void playAllParticle(GameObject go, bool reactive = false)
	{
		if (go == null)
		{
			return;
		}
		if (reactive)
		{
			go.SetActive(false);
			go.SetActive(true);
		}
		var particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		int count = particles.Length;
		for (int i = 0; i < count; ++i)
		{
			particles[i].Play();
		}
	}
	public static void stopAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		var particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		int count = particles.Length;
		for (int i = 0; i < count; ++i)
		{
			particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
	public static void restartAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		var particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		int count = particles.Length;
		for (int i = 0; i < count; ++i)
		{
			ParticleSystem particle = particles[i];
			particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			particle.Play();
		}
	}
	public static void pauseAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		var particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		int count = particles.Length;
		for (int i = 0; i < count; ++i)
		{
			particles[i].Pause();
		}
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
	public static Vector3 generateLocalPosition(Transform transform, Vector3 worldPosition)
	{
		Transform parent = transform.parent;
		Vector3 localPosition = worldPosition - generateWorldPosition(parent);
		// 还原缩放
		localPosition = devideVector3(localPosition, generateWorldScale(parent));
		// 还原旋转
		return rotateVector3(localPosition, Quaternion.Inverse(generateWorldRotation(parent)));
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
	public static float getAnimationLength(Animator animator, string name)
	{
		if (animator == null || animator.runtimeAnimatorController == null)
		{
			return 0.0f;
		}
		var clips = animator.runtimeAnimatorController.animationClips;
		int count = clips.Length;
		for (int i = 0; i < count; ++i)
		{
			AnimationClip clip = clips[i];
			if (clip.name == name)
			{
				return clip.length;
			}
		}
		return 0.0f;
	}
	public static void applyAnchor(GameObject obj, bool force, GameLayout layout = null)
	{
		var scaleAnchor = obj.GetComponent<ScaleAnchor>();
		var paddingAnchor = obj.GetComponent<PaddingAnchor>();
		if (paddingAnchor != null || (scaleAnchor != null && scaleAnchor.mRemoveUGUIAnchor))
		{
			// 去除UGUI自带的锚点,避免计算错误
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.one * 0.5f;
				rectTransform.anchorMax = Vector2.one * 0.5f;
			}
		}

		// 先更新自己
		scaleAnchor?.updateRect(force);
		paddingAnchor?.updateRect(force);
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
	public static Vector2 getHardwareScreenSize() { return mHardwareScreenSize; }
	public static Vector2 getScreenSize() { return mScreenSize; }
	public static Vector2 getHalfScreenSize() { return mHalfScreenSize; }
	public static float getScreenAspect() { return mScreenAspect; }
	public static Vector2 getRootSize()
	{
		return FrameUtility.getUGUIRoot().getWindowSize();
	}
	public static Vector2 getScreenScale()
	{
		return new Vector2(mScreenSize.x * (1.0f / STANDARD_WIDTH),
						   mScreenSize.y * (1.0f / STANDARD_HEIGHT));
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		return new Vector2(rootSize.x * (1.0f / STANDARD_WIDTH), 
						   rootSize.y * (1.0f / STANDARD_HEIGHT));
	}
	public static Vector2 adjustScreenScale(ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		return adjustScreenScale(getScreenScale(), aspectBase);
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
	public static Material findMaterial(Renderer render)
	{
		if (render == null)
		{
			return null;
		}
#if UNITY_EDITOR
		return render.material;
#else
		return render.sharedMaterial;
#endif
	}
	public static string getEnumLabel<T>(T value) where T : Enum
	{
		return getEnumLabel(Typeof(value), value.ToString());
	}
	public static string getEnumLabel(Type type, string name)
	{
		var list = type.GetMember(name);
		if (list == null || list.Length == 0)
		{
			return name;
		}
		var attris = list[0].GetCustomAttributes(false);
		foreach (var item in attris)
		{
			if (item.GetType() == typeof(LabelAttribute))
			{
				return (item as LabelAttribute).getLabel();
			}
		}
		return name;
	}
	public static string getEnumToolTip<T>(T value) where T : Enum
	{
		return getEnumToolTip(Typeof(value), value.ToString());
	}
	public static string getEnumToolTip(Type type, string name)
	{
		var list = type.GetMember(name);
		if (list == null || list.Length == 0)
		{
			return name;
		}
		var attris = list[0].GetCustomAttributes(false);
		foreach (var item in attris)
		{
			if (item.GetType() == typeof(TooltipAttribute))
			{
				return (item as TooltipAttribute).tooltip;
			}
		}
		return name;
	}
	public static string getGameObjectPath(GameObject go)
	{
		if (go == null)
		{
			return EMPTY;
		}
		Transform transform = go.transform;
		string path = go.name;
		while(true)
		{
			Transform parentTrans = transform?.parent;
			if (parentTrans == null)
			{
				break;
			}
			path = parentTrans.name + "/" + path;
			transform = transform.parent;
		}
		return path;
	}
	public static int getContentLength(myUGUIText textWindow, string str)
	{
		Text textComponent = textWindow.getTextComponent();
		var textGenerator = textComponent.cachedTextGeneratorForLayout;
		var settings = textComponent.GetGenerationSettings(Vector2.zero);
		return ceil(textGenerator.GetPreferredWidth(str, settings) / textComponent.pixelsPerUnit);
	}
	public static int getLastError()
	{
		return Kernel32.GetLastError();
	}
	public static bool isMobile()
	{
		return isAndroid() || isIOS();
	}
	public static bool isRealMobile()
	{
		return !isEditor() && (isAndroid() || isIOS());
	}
	public static bool isEditor()
	{
#if UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
	public static bool isWindows()
	{
#if UNITY_STANDALONE_WIN
		return true;
#else
		return false;
#endif
	}
	public static bool isAndroid()
	{
#if UNITY_ANDROID
		return true;
#else
		return false;
#endif
	}
	public static bool isIOS()
	{
#if UNITY_IOS
		return true;
#else
		return false;
#endif
	}
	public static bool isMacOS()
	{
#if UNITY_STANDALONE_OSX
		return true;
#else
		return false;
#endif
	}
	public static bool isLinux()
	{
#if UNITY_STANDALONE_LINUX
		return true;
#else
		return false;
#endif
	}
	public static bool isDevelopment()
	{
#if DEVELOPMENT_BUILD
		return true;
#else
		return false;
#endif
	}
}