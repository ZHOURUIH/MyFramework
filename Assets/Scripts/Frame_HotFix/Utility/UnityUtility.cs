using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
#if USE_SPINE
using Spine.Unity;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if USE_URP
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
#endif
using UObject = UnityEngine.Object;
using UDebug = UnityEngine.Debug;
using static FrameBaseUtility;
using static CSharpUtility;
using static StringUtility;
using static BinaryUtility;
using static MathUtility;
using static TimeUtility;
using static FileUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameUtility;
using static FrameBaseHotFix;

// 与Unity相关的工具函数
public class UnityUtility
{
	protected static bool mShowMessageBox = true;					// 是否显示报错提示框,用来判断提示框显示次数
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.FORCE;			// 当前的日志过滤等级
	protected static PointerEventData mEventData;                   // 缓存一个对象,避免每次都重新new一个
	protected static Vector2Int mHardwareScreenSize = new(Screen.currentResolution.width, Screen.currentResolution.height);	// 显示器宽高
	protected static Vector2Int mScreenSize = new(Screen.width, Screen.height);					// 窗口宽高
	protected static Vector2Int mHalfScreenSize = new(Screen.width >> 1, Screen.height >> 1);	// 窗口宽高的一半
	protected static float mScreenAspect = mScreenSize.x / (float)mScreenSize.y;				// 屏幕宽高比
	protected static Vector2 mScreenScale = new(mScreenSize.x * (1.0f / STANDARD_WIDTH), 
												mScreenSize.y * (1.0f / STANDARD_HEIGHT));		// 当前分辨率相对于标准分辨率的缩放
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
	public static void log(string info, UObject obj)
	{
		log(info, null, LOG_LEVEL.FORCE, obj);
	}
	public static void log(string info, string color)
	{
		log(info, color, LOG_LEVEL.FORCE, null);
	}
	public static void log(string info, Color32 color)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.FORCE, null);
	}
	public static void log(string info, Color32 color, UObject obj)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.FORCE, obj);
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
		if (!color.isEmpty())
		{
			info = colorStringNoBuilder(color, info);
		}
		// isPlayging是unity的接口,只能在主线程使用
		if (isMainThread() && isPlaying())
		{
			UDebug.Log(getNowTime(TIME_DISPLAY.HMSM) + ": " + info, obj);
		}
		else
		{
			UDebug.Log(info, obj);
		}
	}
	public static void logNoLock(string info, string color, LOG_LEVEL level, UObject obj)
	{
		if ((int)level < (int)mLogLevel)
		{
			return;
		}
		if (!color.isEmpty())
		{
			info = colorStringNoBuilder(color, info);
		}
		if (isMainThread() && isPlaying())
		{
			UDebug.Log(getTimeNoLock(TIME_DISPLAY.HMSM) + ": " + info, obj);
		}
		else
		{
			UDebug.Log(info, obj);
		}
	}
	public static void logWarning(string info)
	{
		if (isMainThread() && isPlaying())
		{
			UDebug.LogWarning(info);
		}
		else
		{
			UDebug.LogWarning(getNowTime(TIME_DISPLAY.HMSM) + ": " + info);
		}
	}
	public static void setScreenSize(Vector2 size, bool fullScreen)
	{
		mScreenSize.x = (int)size.x;
		mScreenSize.y = (int)size.y;
		mHalfScreenSize = new(mScreenSize.x >> 1, mScreenSize.y >> 1);
		mScreenAspect = divide(mScreenSize.x, mScreenSize.y);   // 屏幕宽高比
		mScreenScale = new(mScreenSize.x * (1.0f / STANDARD_WIDTH), mScreenSize.y * (1.0f / STANDARD_HEIGHT));   // 当前分辨率相对于标准分辨率的缩放
		setScreenSizeBase(mScreenSize, fullScreen);
		GameCamera camera = mCameraManager.getUICamera();
		if (camera != null)
		{
			FT.MOVE(camera, new(0.0f, 0.0f, -divide(mScreenSize.y * 0.5f, tan(camera.getFOVY(true) * 0.5f))));
		}
		GameCamera blurCamera = mCameraManager.getUIBlurCamera();
		if (blurCamera != null)
		{
			FT.MOVE(blurCamera, new(0.0f, 0.0f, -divide(mScreenSize.y * 0.5f, tan(blurCamera.getFOVY(true) * 0.5f))));
		}
	}
	public static List<GameObject> getGameObjectWithTag(GameObject parent, string tag)
	{
		List<GameObject> objList = new();
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
	public static GameObject findOrCreateRootGameObject(string name)
	{
		GameObject obj = getRootGameObject(name, false);
		if (obj == null)
		{
			obj = createGameObject(name);
		}
		return obj;
	}
	public static GameObject findOrCreateGameObject(string name, GameObject parent, bool recursive = true)
	{
		GameObject obj = getGameObject(name, parent, false, recursive);
		if (obj == null)
		{
			obj = createGameObject(name, parent);
		}
		return obj;
	}
	// 查找所有名字为name的GameObject
	public static void getAllGameObject(List<GameObject> list, string name, GameObject parent, bool recursive = true)
	{
		if (name.isEmpty())
		{
			return;
		}
		if (parent == null)
		{
			logError("parent不能为空,查找无父节点的GameObject请使用getRootGameObject");
			return;
		}
		// 第一级子节点中查找
		Transform parentTrans = parent.transform;
		int childCount = parentTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			GameObject child = parentTrans.GetChild(i).gameObject;
			if (child.name == name)
			{
				list.Add(child);
			}
		}
		// 递归查找
		if (recursive)
		{
			for (int i = 0; i < childCount; ++i)
			{
				getAllGameObject(list, name, parentTrans.GetChild(i).gameObject, true);
			}
		}
	}
	public static GameObject cloneObject(GameObject oriObj, string name)
	{
		GameObject obj = UObject.Instantiate(oriObj);
		obj.name = name;
		return obj;
	}
	public static void cloneObjectAsync(GameObject oriObj, string name, GameObjectCallback callback)
	{
		GameEntry.getInstance().StartCoroutine(instantiateCoroutine(oriObj, name, callback));
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
	public static void findUGUIShaders(GameObject go)
	{
		using var a = new ListScope<Graphic>(out var graphics);
		go.GetComponentsInChildren(true, graphics);
		foreach (Graphic graphic in graphics)
		{
			findMaterialShader(graphic.material);
		}
	}
	public static void raycastUGUI(Vector2 screenPosition, List<RaycastResult> results)
	{
		mEventData ??= new(UnityEngine.EventSystems.EventSystem.current);
		// 将点击位置的屏幕坐标赋值给点击事件
		mEventData.position = new(screenPosition.x, screenPosition.y);
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
	public static Ray getMainCameraMouseRay()
	{
		if (isEditor() || isStandalone())
		{
			return getCameraRay(getMousePosition(), getMainCamera().getCamera());
		}
		return new Ray();
	}
	public static Ray getMainCameraScreenCenterRay()
	{
		return getCameraRay((Vector2)getHalfScreenSize(), getMainCamera().getCamera());
	}
	public static Ray getMainCameraRay(Vector3 screenPos)
	{
		return getCameraRay(screenPos, getMainCamera().getCamera());
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
	// screenPos是以屏幕左下角为原点的坐标
	public static Ray getUIRay(Vector3 screenPos)
	{
		return getCameraRay(screenPos, getUICamera());
	}
	// screenCenterAsZero为false表示返回的坐标是以屏幕左下角为原点的坐标
	// screenCenterAsZero为true表示返回的坐标是以屏幕中心为原点的坐标
	public static Vector3 worldToScreen(Vector3 worldPos, Camera camera, bool screenCenterAsZero = true)
	{
		Vector3 screenPosition = camera.WorldToScreenPoint(worldPos);
		if (screenCenterAsZero)
		{
			screenPosition -= getHalfScreenSize().toVec3();
		}
		screenPosition.z = 0.0f;
		return screenPosition;
	}
	public static Vector3 worldToScreen(Vector3 worldPos, bool screenCenterAsZero = true)
	{
		return worldToScreen(worldPos, getMainCamera().getCamera(), screenCenterAsZero);
	}
	public static Vector3 worldUIToScreen(Vector3 worldPos, bool screenCenterAsZero = true)
	{
		return worldToScreen(worldPos, getUICamera(), screenCenterAsZero);
	}
	public static bool isGameObjectInScreen(Vector3 worldPos)
	{
		Vector3 screenPos = worldToScreen(worldPos, false);
		return screenPos.z >= 0.0f && inRange((Vector2)screenPos, Vector2.zero, getRootSize());
	}
	public static bool isPointInWindow(Vector2 screenPos, myUGUIObject window)
	{
		Camera camera = getUICamera();
		Vector2 cameraSize = new(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = multiVector2(divideVector2(screenPos, cameraSize), rootSize) - rootSize * 0.5f;

		Vector2 parentWorldPosition = divideVector3(window.getWorldPosition(), mLayoutManager.getUIRoot().getScale());
		Vector2 windowPos = divideVector2(screenPos - parentWorldPosition, window.getWorldScale());
		Vector2 halfWindowSize = window.getWindowSize() * 0.5f;
		return inRange(windowPos, -halfWindowSize, halfWindowSize);
	}
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示以window的左下角为原点
	public static Vector2 screenPosToWindow(Vector2 screenPos, myUIObject window, bool windowCenterAsZero = true)
	{
		Camera camera = getUICamera();
		Vector2 cameraSize = new(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = multiVector2(divideVector2(screenPos, cameraSize), rootSize) - rootSize * 0.5f;
		Vector2 windowPos = screenPos;
		if (window != null)
		{
			Vector2 parentWorldPosition = divideVector3(window.getWorldPosition(), mLayoutManager.getUIRoot().getScale());
			windowPos = divideVector2(screenPos - parentWorldPosition, window.getWorldScale());
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
		return atCameraBack(position, getMainCamera());
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
	public static void setParticleSortOrder(GameObject obj, int sortOrder)
	{
		using var a = new ListScope<Renderer>(out var renderers);
		obj.GetComponentsInChildren(true, renderers);
		foreach (Renderer renderer in renderers)
		{
			renderer.sortingOrder = sortOrder;
		}
	}
	public static void setParticleSortLayerID(GameObject obj, int layerID)
	{
		using var a = new ListScope<Renderer>(out var renderers);
		obj.GetComponentsInChildren(true, renderers);
		foreach (Renderer renderer in renderers)
		{
			renderer.sortingLayerID = layerID;
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
		return Sprite.Create(tex, new(0.0f, 0.0f, tex.width, tex.height), new(0.5f, 0.5f));
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
				path = path.ensurePrefix("file:///");
			}
			else if (isIOS())
			{
				path = path.ensurePrefix("file://");
			}
			else if (isMacOS())
			{
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
	public static Vector3 localToWorldDirection(Transform transform, Vector3 local)
	{
		if (transform == null)
		{
			return Vector3.forward;
		}
		return transform.localToWorldMatrix.MultiplyVector(local);
	}
	public static Vector3 worldToLocalDirection(Transform transform, Vector3 world)
	{
		if (transform == null)
		{
			return Vector3.forward;
		}
		return transform.worldToLocalMatrix.MultiplyVector(world);
	}
	// 计算碰撞盒在parent坐标系中的最大点和最小点
	public static void getMinMaxCorner(BoxCollider box, out Vector3 min, out Vector3 max, GameObject parent, int precision = 4)
	{
		Vector3 halfSize = getHalfBoxSize(box, parent);
		Vector3 worldBoxCenter = worldToLocal(parent.transform, localToWorld(box.transform, box.center));
		min = new(float.MaxValue, float.MaxValue, float.MaxValue);
		max = new(float.MinValue, float.MinValue, float.MinValue);
		getMinMaxVector3(worldBoxCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z), ref min, ref max);
		getMinMaxVector3(worldBoxCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z), ref min, ref max);
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
	public static bool isPointInBoxCollider(BoxCollider collider, Vector3 worldPos)
	{
		if (collider == null)
		{
			return false;
		}
		Vector3 delta = worldToLocal(collider.transform, worldPos) - collider.center;
		return abs(delta.x) <= collider.size.x * 0.5 && abs(delta.y) <= collider.size.y * 0.5f;
	}
	public static int overlapCollider(Collider collider, Collider[] results, int layer = -1)
	{
		if (collider == null)
		{
			return 0;
		}
		memset(results, null);
		if (collider is BoxCollider box)
		{
			return overlapAllBox(box, results, layer);
		}
		else if (collider is SphereCollider sphere)
		{
			return overlapAllSphere(sphere, results, layer);
		}
		else if (collider is CapsuleCollider capsule)
		{
			return overlapAllCapsule(capsule, results, layer);
		}
		else
		{
			logError("不支持的碰撞体类型:" + collider.GetType());
		}
		return 0;
	}
	public static int overlapCollider(Collider2D collider, Collider2D[] results, int layer = -1)
	{
		if (collider == null)
		{
			return 0;
		}
		memset(results, null);
		int hitCount = 0;
		if (collider is BoxCollider2D box2D)
		{
			hitCount = overlapAllBox(box2D, results, layer);
		}
		else if (collider is CircleCollider2D circle2D)
		{
			hitCount = overlapAllSphere(circle2D, results, layer);
		}
		else if (collider is CapsuleCollider2D capsule2D)
		{
			hitCount = overlapAllCapsule(capsule2D, results, layer);
		}
		else
		{
			logError("不支持的碰撞体类型:" + collider.GetType());
		}
		return hitCount;
	}
	// 判断两个碰撞体是否相交
	public static bool isOverlap(Collider collider0, Collider collider1)
	{
		return Physics.ComputePenetration(collider0, Vector3.zero, Quaternion.identity, collider1, Vector3.zero, Quaternion.identity, out _, out _);
	}
	public static bool raycast(Ray ray, out Collider result, out Vector3 point, int layer = -1)
	{
		return raycast(ray, out result, out point, 20000.0f, layer);
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
		return raycastAll(ray, result, 20000.0f, layer);
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
	public static bool raycast(Ray ray, Collider collider)
	{
		if (collider == null)
		{
			return false;
		}
		return collider.Raycast(ray, out _, 20000.0f);
	}
	public static bool raycast(Ray ray, out RaycastHit hitInfo)
	{
		return Physics.Raycast(ray, out hitInfo, 20000.0f, -1);
	}
	public static bool getRaycastPoint(Collider collider, Ray ray, ref Vector3 intersectPoint)
	{
		return getRaycastPoint(collider, ray, ref intersectPoint, 20000.0f);
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
		using var a = new ListScope<ParticleSystem>(out var particles);
		go.GetComponentsInChildren(true, particles);
		foreach (ParticleSystem particle in particles)
		{
			particle.Play(false);
		}
	}
	public static void stopAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		using var a = new ListScope<ParticleSystem>(out var particles);
		go.GetComponentsInChildren(true, particles);
		foreach (ParticleSystem particle in particles)
		{
			particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
	public static void restartAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		using var a = new ListScope<ParticleSystem>(out var particles);
		go.GetComponentsInChildren(true, particles);
		foreach (ParticleSystem particle in particles)
		{
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
		using var a = new ListScope<ParticleSystem>(out var particles);
		go.GetComponentsInChildren(true, particles);
		foreach (ParticleSystem particle in particles)
		{
			particle.Pause();
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
		localPosition = divideVector3(localPosition, generateWorldScale(parent));
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
		foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
		{
			if (clip.name == name)
			{
				return clip.length;
			}
		}
		return 0.0f;
	}
	public static void applyAnchor(GameObject obj, bool force, GameLayout layout = null)
	{
		obj.TryGetComponent<ScaleAnchor>(out var scaleAnchor);
		obj.TryGetComponent<ScaleAnchor3D>(out var scaleAnchor3D);
		obj.TryGetComponent<PaddingAnchor>(out var paddingAnchor);
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
		if (scaleAnchor3D != null)
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
	public static bool multiSpriteToSpritePNG(Texture2D tex2D, string outputPath)
	{
		if (!isEditor())
		{
			return false;
		}
#if UNITY_EDITOR
		bool backupReadable = tex2D.isReadable;
		string texPath = getAssetPath(tex2D);
		bool modified = false;
		var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
		TextureImporterCompression backupCompress = importer.textureCompression;
		if (!tex2D.isReadable)
		{
			importer.isReadable = true;
			modified = true;
		}
		if (tex2D.format != TextureFormat.RGBA32 && tex2D.format != TextureFormat.RGB24)
		{
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			modified = true;
		}
		if (modified)
		{
			importer.SaveAndReimport();
		}
		validPath(ref outputPath);
		foreach (UObject obj in loadAllAssetsAtPath(texPath))
		{
			if (obj is not Sprite sprite)
			{
				continue;
			}
			Texture2D output = new((int)sprite.rect.width, (int)sprite.rect.height);
			Rect r = sprite.textureRect;
			output.SetPixels(sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height));
			output.Apply();
			output.name = sprite.name;
			byte[] bytes = output.EncodeToPNG();
			writeFile(outputPath + sprite.name + ".png", bytes, bytes.Length);
		}
		if (modified)
		{
			importer.isReadable = backupReadable;
			importer.textureCompression = backupCompress;
			importer.SaveAndReimport();
		}
#endif
		return true;
	}
	public static Vector2 getGameViewSize()
	{
		if (isEditor())
		{
			Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
			MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			return (Vector2)GetSizeOfMainGameView.Invoke(null, null);
		}
		else
		{
			logError("getGameViewSize can only call in editor!");
			return Vector2.zero;
		}
	}
	public static Vector2Int getHardwareScreenSize() { return mHardwareScreenSize; }
	public static Vector2Int getScreenSize() { return mScreenSize; }
	public static Vector2Int getHalfScreenSize() { return mHalfScreenSize; }
	public static float getScreenAspect() { return mScreenAspect; }
	public static Vector2 getRootSize() { return getUGUIRoot().getWindowSize(); }
	// 获取屏幕独立的缩放值
	public static Vector2 getScreenScale() { return FrameBaseUtility.getScreenScale(mScreenSize); }
	// 根据一定规则,获取屏幕的缩放
	public static Vector2 getScreenScale(ASPECT_BASE aspectBase) { return adjustScreenScale(getScreenScale(), aspectBase); }
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
	// 根据屏幕适配的x方向的缩放,来调整originValue的值
	public static float applyScreenScaleX(float originValue, ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		return originValue * getScreenScale(aspectBase).x;
	}
	// 根据屏幕适配的y方向的缩放,来调整originValue的值
	public static float applyScreenScaleY(float originValue, ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		return originValue * getScreenScale(aspectBase).y;
	}
	public static Material findMaterial(Renderer render)
	{
		if (render == null)
		{
			return null;
		}
		if (isEditor())
		{
			return render.material;
		}
		else
		{
			return render.sharedMaterial;
		}
	}
	public static string getEnumLabel<T>(T value) where T : Enum
	{
		return getEnumLabel(value.GetType(), value.ToString());
	}
	public static string getEnumLabel(Type type, string name)
	{
		foreach (object item in (type.GetMember(name).get(0)?.GetCustomAttributes(false)).safe())
		{
			if (item.GetType() == typeof(EnumLabelAttribute))
			{
				return (item as EnumLabelAttribute).getLabel();
			}
		}
		return name;
	}
	public static string getEnumToolTip<T>(T value) where T : Enum
	{
		return getEnumToolTip(value.GetType(), value.ToString());
	}
	public static string getEnumToolTip(Type type, string name)
	{
		foreach (object item in (type.GetMember(name).get(0)?.GetCustomAttributes(false))?.safe())
		{
			if (item.GetType() == typeof(TooltipAttribute))
			{
				return (item as TooltipAttribute).tooltip;
			}
		}
		return name;
	}
	public static GameObject getGameObjectInParent(GameObject go, string parentName)
	{
		if (go == null)
		{
			return null;
		}
		if (go.name == parentName)
		{
			return go;
		}
		if (go.transform.parent == null)
		{
			return null;
		}
		if (go.transform.parent.name == parentName)
		{
			return go.transform.parent.gameObject;
		}
		return getTopParent(go.transform.parent.gameObject);
	}
	public static GameObject getTopParent(GameObject go)
	{
		if (go.transform.parent == null)
		{
			return go;
		}
		return getTopParent(go.transform.parent.gameObject);
	}
	public static string getGameObjectPath(GameObject go)
	{
		if (go == null)
		{
			return EMPTY;
		}
		Transform transform = go.transform;
		string path = go.name;
		while (true)
		{
			Transform parentTrans = transform != null ? transform.parent : null;
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
		TextGenerator textGenerator = textComponent.cachedTextGeneratorForLayout;
		TextGenerationSettings settings = textComponent.GetGenerationSettings(Vector2.zero);
		return ceil(divide(textGenerator.GetPreferredWidth(str, settings), textComponent.pixelsPerUnit));
	}
#if USE_SPINE
	public static void playSpineAnimation(SkeletonAnimation comSkeleton, string anim, bool loop, bool force = false)
	{
		if (comSkeleton == null || comSkeleton.Skeleton == null)
		{
			return;
		}
		if (comSkeleton.Skeleton.Data.FindAnimation(anim) == null)
		{
			logWarning("动画不存在:" + anim);
			return;
		}
		// 避免重复播放循环动作
		Spine.AnimationState animState = comSkeleton.AnimationState;
		Spine.Animation curAnim = animState.GetCurrent(0)?.Animation;
		if (!force && loop && animState.GetCurrent(0) != null && animState.GetCurrent(0).Loop && curAnim != null && curAnim.Name == anim)
		{
			return;
		}
		comSkeleton.Skeleton.SetToSetupPose();
		animState.ClearTracks();
		animState.SetAnimation(0, anim, loop);
		comSkeleton.Update(0);
		comSkeleton.LateUpdate();
	}
	public static void stopSpineAnimation(SkeletonAnimation comSkeleton)
	{
		comSkeleton.AnimationState.ClearTracks();
	}
#endif
#if USE_URP
	public static void setRenderScale(float scale)
	{
		// 获取当前活动的URP资产
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
		if (urpAsset != null)
		{
			urpAsset.renderScale = scale;
		}
	}
	public static float getRenderScale()
	{
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
		if (urpAsset == null)
		{
			return 0.0f;
		}
		return urpAsset.renderScale;
	}
#endif
	public static int getLastError()
	{
		return Kernel32.GetLastError();
	}
	public static int prefsGetInt(string key, int defaultValue = 0)
	{
#if UNITY_EDITOR
		return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
#elif BYTE_DANCE
		return TTSDK.TT.PlayerPrefs.GetInt(key, defaultValue);
#elif WEIXINMINIGAME
		return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
#else
		return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
#endif
	}
	public static void prefsSetInt(string key, int value)
	{
#if UNITY_EDITOR
		UnityEngine.PlayerPrefs.SetInt(key, value);
		UnityEngine.PlayerPrefs.Save();
#elif BYTE_DANCE
		TTSDK.TT.PlayerPrefs.SetInt(key, value);
		TTSDK.TT.PlayerPrefs.Save();
#elif WEIXINMINIGAME
		UnityEngine.PlayerPrefs.SetInt(key, value);
		UnityEngine.PlayerPrefs.Save();
#else
		UnityEngine.PlayerPrefs.SetInt(key, value);
		UnityEngine.PlayerPrefs.Save();
#endif
	}
	public static string prefsGetString(string key)
	{
#if UNITY_EDITOR
		return UnityEngine.PlayerPrefs.GetString(key);
#elif BYTE_DANCE
		return TTSDK.TT.PlayerPrefs.GetString(key);
#elif WEIXINMINIGAME
		return UnityEngine.PlayerPrefs.GetString(key);
#else
		return UnityEngine.PlayerPrefs.GetString(key);
#endif
	}
	public static void prefsSetString(string key, string value)
	{
#if UNITY_EDITOR
		UnityEngine.PlayerPrefs.SetString(key, value);
		UnityEngine.PlayerPrefs.Save();
#elif BYTE_DANCE
		TTSDK.TT.PlayerPrefs.SetString(key, value);
		TTSDK.TT.PlayerPrefs.Save();
#elif WEIXINMINIGAME
		UnityEngine.PlayerPrefs.SetString(key, value);
		UnityEngine.PlayerPrefs.Save();
#else
		UnityEngine.PlayerPrefs.SetString(key, value);
		UnityEngine.PlayerPrefs.Save();
#endif
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static IEnumerator instantiateCoroutine(GameObject origin, string name, GameObjectCallback callback)
	{
		var ret = UObject.InstantiateAsync(origin);
		yield return ret;
		GameObject go = ret.Result.get(0);
		if (go != null)
		{
			go.name = name;
		}
		callback?.Invoke(go);
	}
}