using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
#if USE_SPINE
using Spine.Unity;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if USE_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.Rendering;
using TMPro;
using UObject = UnityEngine.Object;
using UDebug = UnityEngine.Debug;
using static FrameBaseUtility;
using static StringUtility;
using static BinaryUtility;
using static MathUtility;
using static TimeUtility;
using static FileUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameUtility;
using static FrameBaseHotFix;
using UEventSystem = UnityEngine.EventSystems.EventSystem;

// 与Unity相关的工具函数
public class UnityUtility
{
	protected static Collider[] mColliderOverlapResults = new Collider[32];
	protected static RaycastHit[] mColliderCastResults = new RaycastHit[32];
	protected static bool mShowMessageBox = true;                   // 是否显示报错提示框,用来判断提示框显示次数
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.FORCE;         // 当前的日志过滤等级
	protected static PointerEventData mEventData;                   // 缓存一个对象,避免每次都重新new一个
	protected static Vector2Int mHardwareScreenSize = new(Screen.currentResolution.width, Screen.currentResolution.height); // 显示器宽高
	protected static Vector2Int mScreenSize = new(Screen.width, Screen.height);                 // 窗口宽高
	protected static Vector2Int mHalfScreenSize = new(Screen.width >> 1, Screen.height >> 1);   // 窗口宽高的一半
	protected static float mScreenAspect = mScreenSize.x / (float)mScreenSize.y;                // 屏幕宽高比
	protected static Vector2 mScreenScale = new(mScreenSize.x / FrameSettings.getUISize().x,
												mScreenSize.y / FrameSettings.getUISize().y);      // 当前分辨率相对于标准分辨率的缩放
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		log("log level: " + mLogLevel);
	}
	public static LOG_LEVEL getLogLevel() { return mLogLevel; }
	[HideInCallstack]
	public static void logException(Exception e, string info = null)
	{
		if (e == null)
		{
			UDebug.LogError(info.isEmpty() ? "异常对象为空" : info);
			return;
		}
		string originInfo = info;
		if (info == null)
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
		if (isEditor())
		{
			info += ",编辑器中双击下一条日志可跳转到抛异常的具体代码位置";
		}
		logError(info);
		if (isEditor())
		{
			makeExceptionStack(e, originInfo);
		}
	}
	[HideInCallstack]
	public static void logError(string info)
	{
		if (isMainThread() && mShowMessageBox && Application.isPlaying)
		{
			displayDialog("错误", info, "确认");
			setPause(true);
			// 运行一次只显示一次提示框,避免在循环中报错时一直弹窗
			mShowMessageBox = false;
		}
		// 此处不能使用MyStringBuilder拼接的字符串,因为可能会造成无限递归而堆栈溢出
		info = getTimeNoBuilder(TIME_DISPLAY.HMSM) + ": error: " + info + "\nstack: " + new StackTrace().ToString();
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
			// 这里需要手动触发bugly的上报,因为没有调用unity的LogError,无法自动捕获到错误
			BuglyForwarder.reportError(info, "", LogType.Error);
		}
		else
		{
			UDebug.LogError(info);
		}
	}
	[HideInCallstack]
	public static void logNoLock(string info)
	{
		logNoLock(info, null, LOG_LEVEL.FORCE, null);
	}
	[HideInCallstack]
	public static void log(string info)
	{
		log(info, null, LOG_LEVEL.FORCE, null);
	}
	[HideInCallstack]
	public static void log(string info, UObject obj)
	{
		log(info, null, LOG_LEVEL.FORCE, obj);
	}
	[HideInCallstack]
	public static void log(string info, string color)
	{
		log(info, color, LOG_LEVEL.FORCE, null);
	}
	[HideInCallstack]
	public static void log(string info, Color32 color)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.FORCE, null);
	}
	[HideInCallstack]
	public static void log(string info, Color32 color, UObject obj)
	{
		log(info, colorToRGBString(color), LOG_LEVEL.FORCE, obj);
	}
	[HideInCallstack]
	public static void log(string info, LOG_LEVEL level)
	{
		log(info, null, level, null);
	}
	[HideInCallstack]
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
		// isPlaying是unity的接口,只能在主线程使用
		if (isMainThread() && isPlaying())
		{
			info = getNowTime(TIME_DISPLAY.HMSM) + ": " + info;
		}
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
		}
		else
		{
			UDebug.Log(info, obj);
		}
	}
	[HideInCallstack]
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
			info = getTimeNoLock(TIME_DISPLAY.HMSM) + ": " + info;
		}
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
		}
		else
		{
			UDebug.Log(info, obj);
		}
	}
	[HideInCallstack]
	public static void logWarning(string info)
	{
		if (isMainThread() && isPlaying())
		{
			info = getNowTime(TIME_DISPLAY.HMSM) + ": " + info;
		}
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
		}
		else
		{
			UDebug.LogWarning(info);
		}
	}
	public static void setScreenSize(Vector2 size, bool fullScreen)
	{
		mScreenSize.x = (int)size.x;
		mScreenSize.y = (int)size.y;
		mHalfScreenSize = new(mScreenSize.x >> 1, mScreenSize.y >> 1);
		mScreenAspect = mScreenSize.x.divide(mScreenSize.y);   // 屏幕宽高比
		Vector2Int uiSize = FrameSettings.getUISize();
		mScreenScale = new(mScreenSize.x / uiSize.x, mScreenSize.y / uiSize.y);   // 当前分辨率相对于标准分辨率的缩放
		setScreenSizeBase(mScreenSize, fullScreen);
		GameCamera camera = mCameraManager.getUICamera();
		camera?.MOVE(new(0.0f, 0.0f, -(mScreenSize.y * 0.5f).divide((camera.getFOVY(true) * 0.5f).tan())));
		GameCamera blurCamera = mCameraManager.getUIBlurCamera();
		blurCamera?.MOVE(new(0.0f, 0.0f, -(mScreenSize.y * 0.5f).divide((blurCamera.getFOVY(true) * 0.5f).tan())));
	}
	public static List<GameObject> findGameObjectWithTag(GameObject parent, string tag)
	{
		List<GameObject> objList = new();
		findGameObjectWithTag(parent, tag, objList);
		return objList;
	}
	public static void findGameObjectWithTag(GameObject parent, string tag, List<GameObject> objList)
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
			objList.addIf(child.gameObject, child.CompareTag(tag));
			// 递归查找子节点
			findGameObjectWithTag(child.gameObject, tag, objList);
		}
	}
	public static GameObject findOrCreateRootGameObject(string name)
	{
		GameObject obj = findRootGameObject(name, false);
		if (obj == null)
		{
			obj = createGameObject(name);
		}
		return obj;
	}
	public static GameObject findOrCreateGameObject(string name, GameObject parent, bool recursive = true)
	{
		GameObject obj = findGameObject(name, parent, false, recursive);
		if (obj == null)
		{
			obj = createGameObject(name, parent);
		}
		return obj;
	}
	// 查找所有名字为name的GameObject
	public static void findAllGameObject(List<GameObject> list, string name, GameObject parent, bool recursive = true)
	{
		if (name.isEmpty())
		{
			return;
		}
		if (parent == null)
		{
			logError("parent不能为空");
			return;
		}
		// 第一级子节点中查找
		Transform parentTrans = parent.transform;
		int childCount = parentTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			GameObject child = parentTrans.GetChild(i).gameObject;
			list.addIf(child, child.name == name);
		}
		// 递归查找
		if (recursive)
		{
			for (int i = 0; i < childCount; ++i)
			{
				findAllGameObject(list, name, parentTrans.GetChild(i).gameObject, true);
			}
		}
	}
	public static GameObject cloneObject(GameObject oriObj, string name)
	{
		GameObject obj = UObject.Instantiate(oriObj);
		obj.name = name;
		return obj;
	}
	public static GameObject cloneObject(GameObject oriObj, GameObject parent, string name)
	{
		return cloneObject(oriObj, parent.transform, name);
	}
	public static GameObject cloneObject(GameObject oriObj, Transform parent, string name)
	{
		GameObject obj = UObject.Instantiate(oriObj);
		obj.name = name;
		obj.transform.SetParent(parent.transform, false);
		return obj;
	}
	public static void cloneObjectAsync(GameObject oriObj, string name, GameObjectCallback callback)
	{
		GameEntryBase.startCoroutine(instantiateCoroutine(oriObj, name, callback));
	}
	public static GameObject createGameObject(string name, GameObject parent = null)
	{
		GameObject obj = new(name);
		setNormalProperty(obj, parent);
		return obj;
	}
	// 一般不会直接调用该函数,要创建物体时需要使用ObjectPool来创建和回收
	// parent为实例化后挂接的父节点
	// prefabName为预设名,带GameResources下相对路径
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
		try
		{
			callback?.Invoke(obj);
		}
		catch (Exception e)
		{
			logException(e);
		}
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
		// findMaterialShader本身只在编辑器中有实际作用。
		// 非编辑器下直接返回,避免无意义地遍历Renderer和材质。
		if (!isEditor() || go == null)
		{
			return;
		}

		// Renderer.material/materials的getter会让Renderer获得独立材质实例。
		// 这里的目的只是重新绑定Shader,不能因此破坏原本共享的Material引用。
		using var a = new ListScope<Renderer>(out var renderers);
		using var b = new ListScope<Material>(out var materials);
		go.GetComponentsInChildren(true, renderers);
		foreach (Renderer renderer in renderers)
		{
			materials.Clear();
			renderer.GetSharedMaterials(materials);
			foreach (Material item in materials)
			{
				findMaterialShader(item);
			}
		}
		// 可能会用到材质的组件
		using var c = new ListScope<Projector>(out var projectors);
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
		mEventData ??= new(UEventSystem.current);
		// 将点击位置的屏幕坐标赋值给点击事件
		mEventData.position = new(screenPosition.x, screenPosition.y);
		// 向点击处发射射线
		UEventSystem.current.RaycastAll(mEventData, results);
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
		if (!objTrans.localPosition.isEqual(pos))
		{
			objTrans.localPosition = pos;
		}
		if (!objTrans.localEulerAngles.isEqual(rot))
		{
			objTrans.localEulerAngles = rot;
		}
		if (!objTrans.localScale.isEqual(scale))
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
		Vector2 relativeScreenPos = ((Vector2)screenPos).divide(getScreenSize());
		if (camera.orthographic)
		{
			// 在近裁剪面上的投射点
			Vector2 clipSize = new(camera.orthographicSize * 2.0f * camera.aspect, camera.orthographicSize * 2.0f);
			Vector3 nearClipPoint = ((Vector3)(relativeScreenPos.multi(clipSize) - clipSize * 0.5f)).replaceZ(camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			Vector3 farClipPoint = ((Vector3)(relativeScreenPos.multi(clipSize) - clipSize * 0.5f)).replaceZ(camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new(nearClipWorldPoint, (farClipWorldPoint - nearClipWorldPoint).normalize());
		}
		else
		{
			// 在近裁剪面上的投射点
			float nearClipHeight = (camera.fieldOfView * 0.5f).toRadian().tan() * camera.nearClipPlane * 2.0f;
			Vector2 nearClipSize = new(nearClipHeight * camera.aspect, nearClipHeight);
			Vector3 nearClipPoint = ((Vector3)(relativeScreenPos.multi(nearClipSize) - nearClipSize * 0.5f)).replaceZ(camera.nearClipPlane);
			Vector3 nearClipWorldPoint = localToWorld(camera.transform, nearClipPoint);
			// 在远裁剪面上的投射点
			float farClipHeight = (camera.fieldOfView * 0.5f).toRadian().tan() * camera.farClipPlane * 2.0f;
			Vector2 farClipSize = new(farClipHeight * camera.aspect, farClipHeight);
			Vector3 farClipPoint = ((Vector3)(relativeScreenPos.multi(farClipSize) - farClipSize * 0.5f)).replaceZ(camera.farClipPlane);
			Vector3 farClipWorldPoint = localToWorld(camera.transform, farClipPoint);
			return new(nearClipWorldPoint, (farClipWorldPoint - nearClipWorldPoint).normalize());
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
	// screenCenterAsZero为false表示输入的坐标是以屏幕左下角为原点的坐标
	// screenCenterAsZero为true表示输入的坐标是以屏幕中心为原点的坐标
	public static Vector3 screenToWorld(Vector3 screenPos, Camera camera, bool screenCenterAsZero = true)
	{
		if (screenCenterAsZero)
		{
			screenPos += getHalfScreenSize().toVec3();
			screenPos.z = 0.0f;
		}
		Vector3 worldPosition = camera.ScreenToWorldPoint(screenPos);
		worldPosition.z = 0.0f;
		return worldPosition;
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
		return screenPos.z >= 0.0f && ((Vector2)screenPos).inRange(Vector2.zero, getRootSize());
	}
	public static bool isPointInWindow(Vector2 screenPos, myUGUIObject window)
	{
		Camera camera = getUICamera();
		Vector2 cameraSize = new(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = screenPos.divide(cameraSize).multi(rootSize) - rootSize * 0.5f;

		Vector2 parentWorldPosition = window.getWorldPosition().divide(mLayoutManager.getUIRoot().getScale());
		Vector2 windowPos = (screenPos - parentWorldPosition).divide(window.getWorldScale());
		Vector2 halfWindowSize = window.getSize() * 0.5f;
		return windowPos.inRange(-halfWindowSize, halfWindowSize);
	}
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示以window的左下角为原点
	public static Vector2 screenPosToWindow(Vector2 screenPos, myUGUIObject window, bool windowCenterAsZero = true)
	{
		Camera camera = getUICamera();
		Vector2 cameraSize = new(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos = screenPos.divide(cameraSize).multi(rootSize) - rootSize * 0.5f;
		Vector2 windowPos = screenPos;
		if (window != null)
		{
			Vector2 parentWorldPosition = window.getWorldPosition().divide(mLayoutManager.getUIRoot().getScale());
			windowPos = (screenPos - parentWorldPosition).divide(window.getWorldScale());
			if (!windowCenterAsZero)
			{
				windowPos += window.getSize() * 0.5f;
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
	// 判断child是否为parent的递归子节点
	public static bool isTransformChild(Transform parent, Transform child)
	{
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			Transform curTrans = parent.GetChild(i);
			if (curTrans == child || isTransformChild(curTrans, child))
			{
				return true;
			}
		}
		return false;
	}
	// 判断点是否在摄像机背面
	public static bool atCameraBack(Vector3 position, GameCamera camera)
	{
		return (position - camera.getPosition()).normalize().dot(camera.getForward()) <= 0;
	}
	public static bool atCameraBack(Vector3 position)
	{
		return atCameraBack(position, getMainCamera());
	}
#if USE_URP
	public static void setRenderType(Camera camera, CameraRenderType renderType)
	{
		if (!camera.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
		{
			cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
		}
		if (renderType == CameraRenderType.Overlay)
		{
			cameraData.cameraStack?.Clear();
		}
		cameraData.renderType = renderType;
	}
#endif
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
		return LayerMask.NameToLayer(name).clamp(1, 32);
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
	// 计算的是旋转和缩放以后的包围盒的大小的一半, 如果填了parent,则会将尺寸转成parent坐标系中的值
	public static Vector3 getHalfBoxSize(BoxCollider collider, GameObject parent)
	{
		Vector3 worldBoxHalfSize = (localToWorldDirection(collider.transform, collider.size) * 0.5f).abs();
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
		Vector3 worldBoxCenter;
		if (parent != null)
		{
			worldBoxCenter = worldToLocal(parent.transform, localToWorld(box.transform, box.center));
		}
		else
		{
			worldBoxCenter = localToWorld(box.transform, box.center);
		}
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
		min = min.checkFloat(precision);
		max = max.checkFloat(precision);
	}
	// 两个碰撞盒相交的条件是box0.min小于box1.max,并且box0.max大于box1.min
	public static bool overlapBox(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		return min0.isLess(max1) && max0.isGreater(min1) ||
			   min1.isLess(max0) && max1.isGreater(min0);
	}
	public static int overlapAllCapsule(CharacterController collider, Collider[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 point0 = collider.center + new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		Vector3 point1 = collider.center - new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		point0 = localToWorld(transform, point0);
		point1 = localToWorld(transform, point1);
		int hitCount = Physics.OverlapCapsuleNonAlloc(point0, point1, collider.radius, results, layer);
		return results.removeValue(hitCount, collider);
	}
	public static bool overlapBoxIgnoreY(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		min0.y = 0.0f;
		max0.y = 1.0f;
		min1.y = 0.0f;
		max1.y = 1.0f;
		return min0.isLess(max1) && max0.isGreater(min1) ||
			   min1.isLess(max0) && max1.isGreater(min0);
	}
	public static bool overlapBoxIgnoreZ(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		min0.z = 0.0f;
		max0.z = 1.0f;
		min1.z = 0.0f;
		max1.z = 1.0f;
		return min0.isLess(max1) && max0.isGreater(min1) ||
			   min1.isLess(max0) && max1.isGreater(min0);
	}
	public static bool isPointInBoxCollider(BoxCollider collider, Vector3 worldPos)
	{
		if (collider == null)
		{
			return false;
		}
		Vector3 delta = worldToLocal(collider.transform, worldPos) - collider.center;
		return delta.x.abs() <= collider.size.x * 0.5 && delta.y.abs() <= collider.size.y * 0.5f;
	}
	// 检测指定碰撞体从上一帧位置移动到当前位置时，与哪些碰撞体发生重叠。
	// 会检测上一帧位置、移动过程中扫过的区域以及当前帧位置，避免高速移动时穿过较薄碰撞体而漏检。
	// 支持BoxCollider、SphereCollider和CapsuleCollider。
	// lastWorldPos为碰撞体Transform上一帧的世界坐标，默认认为移动期间旋转和缩放没有变化。
	// results用于接收检测到的碰撞体，返回值表示results中的有效元素数量。
	// layer为参与检测的LayerMask，默认-1表示检测所有层。
	// 检测结果会排除collider自身，并对上一帧重叠、扫掠命中和当前帧重叠的结果进行去重。
	public static int overlapCollider(Collider collider, Vector3 lastWorldPos, Collider[] results, int layer = -1)
	{
		if (collider == null || results.isEmpty())
		{
			return 0;
		}

		int availableCount = results.Length.getGreaterPow2();
		if (mColliderCastResults.Length < availableCount)
		{
			mColliderCastResults = new RaycastHit[availableCount];
		}
		if (mColliderOverlapResults.Length < availableCount)
		{
			mColliderOverlapResults = new Collider[availableCount];
		}

		// 先检测当前帧位置的重叠
		int resultCount = overlapCollider(collider, results, layer);
		Transform transform = collider.transform;
		Vector3 moveDelta = transform.position - lastWorldPos;
		float moveDistance = moveDelta.magnitude;
		if (moveDistance <= 0.0001f)
		{
			return resultCount;
		}

		Vector3 moveDirection = moveDelta / moveDistance;
		int startOverlapCount;
		int castCount;
		mColliderOverlapResults.setAllValue(null);
		QueryTriggerInteraction interaction = QueryTriggerInteraction.UseGlobal;
		if (collider is BoxCollider box)
		{
			Vector3 scale = transform.lossyScale;
			Vector3 halfExtents = box.size.multi(scale.abs()) * 0.5f;
			Vector3 currentCenter = transform.TransformPoint(box.center);
			Vector3 lastCenter = currentCenter - moveDelta;
			// 检测上一帧位置，避免目标在扫掠起点已经重叠时被Cast忽略
			startOverlapCount = Physics.OverlapBoxNonAlloc(lastCenter, halfExtents, mColliderOverlapResults, transform.rotation, layer, interaction);
			castCount = Physics.BoxCastNonAlloc(lastCenter, halfExtents, moveDirection, mColliderCastResults, transform.rotation, moveDistance, layer, interaction);
		}
		else if (collider is SphereCollider sphere)
		{
			Vector3 scale = transform.lossyScale;
			float radius = sphere.radius * getMax(scale.x.abs(), scale.y.abs(), scale.z.abs());
			Vector3 currentCenter = transform.TransformPoint(sphere.center);
			Vector3 lastCenter = currentCenter - moveDelta;
			startOverlapCount = Physics.OverlapSphereNonAlloc(lastCenter, radius, mColliderOverlapResults, layer, interaction);
			castCount = Physics.SphereCastNonAlloc(lastCenter, radius, moveDirection, mColliderCastResults, moveDistance, layer, interaction);
		}
		else if (collider is CapsuleCollider capsule)
		{
			getCapsuleWorldInfo(capsule, out Vector3 currentPoint0, out Vector3 currentPoint1, out float radius);
			Vector3 lastPoint0 = currentPoint0 - moveDelta;
			Vector3 lastPoint1 = currentPoint1 - moveDelta;
			startOverlapCount = Physics.OverlapCapsuleNonAlloc(lastPoint0, lastPoint1, radius, mColliderOverlapResults, layer, interaction);
			castCount = Physics.CapsuleCastNonAlloc(lastPoint0, lastPoint1, radius, moveDirection, mColliderCastResults, moveDistance, layer, interaction);
		}
		else
		{
			logError("不支持的碰撞体类型:" + collider.GetType());
			return resultCount;
		}

		// 合并上一帧位置的重叠结果
		for (int i = 0; i < startOverlapCount; ++i)
		{
			resultCount = addOverlapColliderResult(collider, mColliderOverlapResults[i], results, resultCount);
			if (resultCount >= results.Length)
			{
				return resultCount;
			}
		}

		// 合并移动路径中的扫掠结果
		for (int i = 0; i < castCount; ++i)
		{
			resultCount = addOverlapColliderResult(collider, mColliderCastResults[i].collider, results, resultCount);
			if (resultCount >= results.Length)
			{
				return resultCount;
			}
		}
		return resultCount;
	}
	// 检测指定碰撞体在当前位置与哪些碰撞体发生重叠。
	// 支持BoxCollider、SphereCollider和CapsuleCollider。
	// results用于接收检测到的碰撞体，返回值表示results中的有效元素数量。
	// layer为参与检测的LayerMask，默认-1表示检测所有层。
	// 检测结果会排除collider自身。
	public static int overlapCollider(Collider collider, Collider[] results, int layer = -1)
	{
		if (collider == null)
		{
			return 0;
		}
		results.setAllValue(null);
		Transform transform = collider.transform;
		if (collider is BoxCollider box)
		{
			Vector3 colliderWorldPos = localToWorld(transform, box.center);
			int hitCount = Physics.OverlapBoxNonAlloc(colliderWorldPos, box.size * 0.5f, results, transform.localRotation, layer);
			return results.removeValue(hitCount, collider);
		}
		else if (collider is SphereCollider sphere)
		{
			Vector3 colliderWorldPos = localToWorld(transform, sphere.center);
			int hitCount = Physics.OverlapSphereNonAlloc(colliderWorldPos, sphere.radius, results, layer);
			return results.removeValue(hitCount, collider);
		}
		else if (collider is CapsuleCollider capsule)
		{
			getCapsuleWorldInfo(capsule, out Vector3 point0, out Vector3 point1, out float radius);
			int hitCount = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layer, QueryTriggerInteraction.UseGlobal);
			return results.removeValue(hitCount, collider);
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
		results.setAllValue(null);
		Transform transform = collider.transform;
		int hitCount = 0;
		if (collider is BoxCollider2D box2D)
		{
			Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
			hitCount = Physics2D.OverlapBoxNonAlloc(colliderWorldPos, box2D.size, transform.localEulerAngles.z, results, layer);
			return results.removeValue(hitCount, collider);
		}
		else if (collider is CircleCollider2D circle2D)
		{
			Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
			hitCount = Physics2D.OverlapCircleNonAlloc(colliderWorldPos, circle2D.radius, results, layer);
			return results.removeValue(hitCount, collider);
		}
		else if (collider is CapsuleCollider2D capsule2D)
		{
			float eulerZ = transform.localEulerAngles.z;
			hitCount = Physics2D.OverlapCapsuleNonAlloc(transform.position, capsule2D.size, capsule2D.direction, eulerZ, results, layer);
			return results.removeValue(hitCount, collider);
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
		return Physics.RaycastNonAlloc(ray, result, maxDistance.clampMin(), layer);
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
		if (collider.Raycast(ray, out RaycastHit hit, maxDistance.clampMin()))
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
		return generateWorldScale(transform.parent).multi(transform.localScale);
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
		localPosition = localPosition.rotate(generateWorldRotation(transform.parent));
		localPosition = localPosition.multi(generateWorldScale(transform.parent));
		return localPosition + generateWorldPosition(transform.parent);
	}
	public static Vector3 generateLocalPosition(Transform transform, Vector3 worldPosition)
	{
		Transform parent = transform.parent;
		Vector3 localPosition = worldPosition - generateWorldPosition(parent);
		// 还原缩放
		localPosition = localPosition.divide(generateWorldScale(parent));
		// 还原旋转
		return localPosition.rotate(Quaternion.Inverse(generateWorldRotation(parent)));
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
	public static Vector2Int getGameViewSize()
	{
		if (isEditor())
		{
			Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
			MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			Vector2 value = (Vector2)GetSizeOfMainGameView.Invoke(null, null);
			return new Vector2Int((int)value.x, (int)value.y);
		}
		else
		{
			logError("getGameViewSize can only call in editor!");
			return Vector2Int.zero;
		}
	}
	public static Vector2Int getHardwareScreenSize() { return mHardwareScreenSize; }
	public static Vector2Int getScreenSize() { return mScreenSize; }
	public static Vector2Int getHalfScreenSize() { return mHalfScreenSize; }
	public static float getScreenAspect() { return mScreenAspect; }
	public static Vector2 getRootSize() { return getUGUIRoot().getSize(); }
	// 获取屏幕独立的缩放值
	public static Vector2 getScreenScale() { return FrameBaseUtility.getScreenScale(mScreenSize); }
	// 获取常用情况下的自动缩放比例
	public static float getScreenScaleAuto() { return generateScreenScaleByAspectBase(getScreenScale(), ASPECT_BASE.AUTO).x; }
	// 根据一定规则,获取屏幕的缩放
	public static Vector2 getScreenScale(ASPECT_BASE aspectBase) { return generateScreenScaleByAspectBase(getScreenScale(), aspectBase); }
	public static Vector2 generateScreenScaleByAspectBase(Vector2 screenScale, ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		Vector2 newScale = screenScale;
		if (aspectBase == ASPECT_BASE.USE_HEIGHT_SCALE)
		{
			newScale.x = screenScale.y;
			newScale.y = newScale.x;
		}
		else if (aspectBase == ASPECT_BASE.USE_WIDTH_SCALE)
		{
			newScale.x = screenScale.x;
			newScale.y = newScale.x;
		}
		else if (aspectBase == ASPECT_BASE.AUTO)
		{
			newScale.x = getMin(screenScale.x, screenScale.y);
			newScale.y = newScale.x;
		}
		else if (aspectBase == ASPECT_BASE.INVERSE_AUTO)
		{
			newScale.x = getMax(screenScale.x, screenScale.y);
			newScale.y = newScale.x;
		}
		return newScale;
	}
	// 根据屏幕适配的缩放,来调整originValue的值
	public static float adjustByScreenScaleAuto(float originValue)
	{
		return originValue * getScreenScaleAuto();
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
	// 在go的所有层级的父节点中查找名叫parentName的父节点
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
	public static int getContentLength(Text textComponent, string str)
	{
		TextGenerator textGenerator = textComponent.cachedTextGeneratorForLayout;
		TextGenerationSettings settings = textComponent.GetGenerationSettings(Vector2.zero);
		return textGenerator.GetPreferredWidth(str, settings).divide(textComponent.pixelsPerUnit).ceil();
	}
	public static int getContentLength(TextMeshProUGUI textComponent, string str)
	{
		return (int)textComponent.GetPreferredValues(str, float.PositiveInfinity, 0).x;
	}
	public static int getGameObjectID(UObject go)
	{
		if (go == null)
		{
			return 0;
		}
#if UNITY_6000_4_OR_NEWER
		return (int)EntityId.ToULong(go.GetEntityId());
#else
		return go.GetInstanceID();
#endif
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
#if UNITY_6000_0_OR_NEWER
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
#else
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
#endif
		if (urpAsset != null)
		{
			urpAsset.renderScale = scale;
		}
	}
	public static float getRenderScale()
	{
#if UNITY_6000_0_OR_NEWER
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
#else
		var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
#endif
		if (urpAsset == null)
		{
			return 1.0f;
		}
		return urpAsset.renderScale;
	}
#endif
	public static Texture2D captureCamera(Camera camera, int width, string name)
	{
		if (camera == null)
		{
			return null;
		}
		int height = (width / camera.aspect).round().clampMin(1);
		RenderTexture oldActive = RenderTexture.active;
		RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
		Texture2D preview = new(width, height, TextureFormat.RGB24, false);
		try
		{
#if USE_URP
			UniversalRenderPipeline.SingleCameraRequest request = new();
			if (!RenderPipeline.SupportsRenderRequest(camera, request))
			{
				destroyUnityObject(preview);
				preview = null;
				return null;
			}
			request.destination = renderTexture;
			RenderPipeline.SubmitRenderRequest(camera, request);
#else
			RenderTexture oldTarget = camera.targetTexture;
			try
			{
				camera.targetTexture = renderTexture;
				camera.Render();
			}
			finally
			{
				camera.targetTexture = oldTarget;
			}
#endif
			RenderTexture.active = renderTexture;
			preview.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0, false);
			preview.Apply(false, false);
			preview.name = name;
		}
		finally
		{
			RenderTexture.active = oldActive;
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		return preview;
	}
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	public static int getLastError()
	{
		return Kernel32.GetLastError();
	}
#endif
	//------------------------------------------------------------------------------------------------------------------------------
	protected static IEnumerator instantiateCoroutine(GameObject origin, string name, GameObjectCallback callback)
	{
#if UNITY_6000_0_OR_NEWER
		var ret = UObject.InstantiateAsync(origin);
		yield return ret;
		GameObject go = ret.Result.get(0);
#else
		GameObject go = UObject.Instantiate(origin);
		yield return null;
#endif
		if (go != null)
		{
			go.name = name;
		}
		try
		{
			callback?.Invoke(go);
		}
		catch (Exception e)
		{
			logException(e, "实例化游戏对象异常");
		}
	}
	// 获取CapsuleCollider当前状态下的世界坐标参数
	protected static void getCapsuleWorldInfo(CapsuleCollider collider, out Vector3 point0, out Vector3 point1, out float radius)
	{
		Transform transform = collider.transform;
		Vector3 scale = transform.lossyScale;
		float scaleX = scale.x.abs();
		float scaleY = scale.y.abs();
		float scaleZ = scale.z.abs();
		Vector3 localAxis;
		float heightScale;
		float radiusScale;
		if (collider.direction == 0)
		{
			// X轴胶囊
			localAxis = Vector3.right;
			heightScale = scaleX;
			radiusScale = getMax(scaleY, scaleZ);
		}
		else if (collider.direction == 1)
		{
			// Y轴胶囊
			localAxis = Vector3.up;
			heightScale = scaleY;
			radiusScale = getMax(scaleX, scaleZ);
		}
		else
		{
			// Z轴胶囊
			localAxis = Vector3.forward;
			heightScale = scaleZ;
			radiusScale = getMax(scaleX, scaleY);
		}

		Vector3 center = transform.TransformPoint(collider.center);
		Vector3 worldAxis = transform.TransformDirection(localAxis).normalized;
		radius = collider.radius * radiusScale;
		// CapsuleCollider.height包含两端半球的直径
		float height = getMax(collider.height * heightScale, radius * 2.0f);
		// Physics.OverlapCapsule和CapsuleCast需要传入两个半球的球心
		float halfLineLength = height * 0.5f - radius;
		point0 = center + worldAxis * halfLineLength;
		point1 = center - worldAxis * halfLineLength;
	}
	protected static int addOverlapColliderResult(Collider sourceCollider, Collider targetCollider, Collider[] results, int resultCount)
	{
		if (targetCollider == null || targetCollider == sourceCollider || resultCount >= results.Length || results.contains(targetCollider))
		{
			return resultCount;
		}
		results[resultCount++] = targetCollider;
		return resultCount;
	}
	// 在Unity编辑器中根据异常调用栈创建一条可定位的错误日志。
	// 日志会显示异常类型、错误信息和完整调用栈，双击日志时会直接跳转到异常实际发生的代码位置，
	// 而不是跳转到捕获异常或调用logException的位置。
	// 此功能依赖UnityEditor内部接口，仅在UNITY_EDITOR环境下生效；
	// 无法获取有效文件和行号或内部接口不可用时，不会创建可定位日志。
	protected static void makeExceptionStack(Exception e, string info = null)
	{
#if UNITY_EDITOR
		try
		{
			// 优先查找最深层内部异常,因为真正的错误可能被外层异常包装
			Exception targetException = e;
			while (targetException.InnerException != null)
			{
				targetException = targetException.InnerException;
			}
			StackFrame targetFrame = null;
			Exception frameException = targetException;
			while (frameException != null && targetFrame == null)
			{
				foreach (StackFrame frame in new StackTrace(frameException, true).GetFrames().safe())
				{
					if (!frame.GetFileName().isEmpty() && frame.GetFileLineNumber() > 0)
					{
						targetFrame = frame;
						break;
					}
				}
				// 内部异常中没有可定位的调试信息时,再尝试最外层异常
				if (ReferenceEquals(frameException, e))
				{
					break;
				}
				frameException = e;
			}
			if (targetFrame == null)
			{
				return;
			}
			string filePath = targetFrame.GetFileName();
			int line = targetFrame.GetFileLineNumber();
			int column = targetFrame.GetFileColumnNumber();
			if (column <= 0)
			{
				column = 1;
			}
			Assembly editorAssembly = typeof(UnityEditor.EditorApplication).Assembly;
			Type logEntriesType = editorAssembly.GetType("UnityEditor.LogEntries");
			Type logEntryType = editorAssembly.GetType("UnityEditor.LogEntry");
			Type consoleWindowType = editorAssembly.GetType("UnityEditor.ConsoleWindow");
			if (logEntriesType == null || logEntryType == null || consoleWindowType == null)
			{
				return;
			}
			const int logIdentifier = 0x4D4C4558;
			const string callbackRegisterKey = "UnityUtility.ExceptionLogDoubleClickCallback";
			BindingFlags instanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags staticFieldFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			bool callbackRegistered = AppDomain.CurrentDomain.GetData(callbackRegisterKey) is bool registered && registered;
			if (!callbackRegistered)
			{
				EventInfo callbackEvent = consoleWindowType.GetEvent("entryWithManagedCallbackDoubleClicked", staticFieldFlags);
				FieldInfo callbackField = null;
				Type callbackType = null;
				if (callbackEvent != null)
				{
					callbackType = callbackEvent.EventHandlerType;
				}
				else
				{
					callbackField = consoleWindowType.GetField("entryWithManagedCallbackDoubleClicked", staticFieldFlags);
					callbackType = callbackField?.FieldType;
				}
				if (callbackType != null)
				{
					ParameterInfo[] callbackParameters = callbackType.GetMethod("Invoke")?.GetParameters();
					if (callbackParameters.count() == 1)
					{
						Type callbackEntryType = callbackParameters[0].ParameterType;
						FieldInfo identifierField = callbackEntryType.GetField("identifier", instanceFieldFlags);
						FieldInfo fileField = callbackEntryType.GetField("file", instanceFieldFlags);
						FieldInfo lineField = callbackEntryType.GetField("line", instanceFieldFlags);
						FieldInfo columnField = callbackEntryType.GetField("column", instanceFieldFlags);
						MethodInfo openFileMethod = logEntriesType.GetMethod("OpenFileOnSpecificLineAndColumn", staticFieldFlags);
						if (identifierField != null && fileField != null && lineField != null && columnField != null && openFileMethod != null)
						{
							DynamicMethod callbackMethod = new("openExceptionFile", typeof(void), new Type[] { callbackEntryType }, typeof(UnityUtility).Module, true);
							ILGenerator il = callbackMethod.GetILGenerator();
							Label returnLabel = il.DefineLabel();
							// 只处理UnityUtility创建的异常日志,不影响其他系统的双击回调
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, identifierField);
							il.Emit(OpCodes.Ldc_I4, logIdentifier);
							il.Emit(OpCodes.Bne_Un_S, returnLabel);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, fileField);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, lineField);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, columnField);
							il.Emit(OpCodes.Call, openFileMethod);
							il.MarkLabel(returnLabel);
							il.Emit(OpCodes.Ret);
							Delegate callback = callbackMethod.CreateDelegate(callbackType);
							if (callbackEvent != null)
							{
								callbackEvent.GetAddMethod(true)?.Invoke(null, new object[] { callback });
							}
							else if (callbackField != null)
							{
								Delegate oldCallback = callbackField.GetValue(null) as Delegate;
								callbackField.SetValue(null, Delegate.Combine(oldCallback, callback));
							}
							AppDomain.CurrentDomain.SetData(callbackRegisterKey, true);
						}
					}
				}
			}
			object logEntry = Activator.CreateInstance(logEntryType, true);
			string logMessage = makeUnityExceptionMessage(e, info, out int callstackStartUTF16, out int callstackStartUTF8);
			logEntryType.GetField("message", instanceFieldFlags)?.SetValue(logEntry, logMessage);
			logEntryType.GetField("file", instanceFieldFlags)?.SetValue(logEntry, filePath);
			logEntryType.GetField("line", instanceFieldFlags)?.SetValue(logEntry, line);
			logEntryType.GetField("column", instanceFieldFlags)?.SetValue(logEntry, column);
			logEntryType.GetField("identifier", instanceFieldFlags)?.SetValue(logEntry, logIdentifier);
			// 告诉Unity Console从哪个字符开始属于调用栈
			logEntryType.GetField("callstackTextStartUTF16", instanceFieldFlags)?.SetValue(logEntry, callstackStartUTF16);
			logEntryType.GetField("callstackTextStartUTF8", instanceFieldFlags)?.SetValue(logEntry, callstackStartUTF8);
			// ScriptingException | DontExtractStacktrace
			logEntryType.GetField("mode", instanceFieldFlags)?.SetValue(logEntry, (1 << 17) | (1 << 18));
			MethodInfo addMessageMethod = logEntriesType.GetMethod("AddMessageWithDoubleClickCallback", staticFieldFlags);
			addMessageMethod?.Invoke(null, new object[] { logEntry });
		}
		catch (Exception editorException)
		{
			UDebug.LogError("创建可定位异常日志失败:" + editorException);
		}
#endif
	}
	// 将异常转换为Unity Console可显示和点击的日志格式。
	// 每一层包含源码文件和行号的调用栈都会被转换为可点击链接。
	protected static string makeUnityExceptionMessage(Exception e, string info, out int callstackStartUTF16, out int callstackStartUTF8)
	{
		string exceptionText = e.ToString().Replace("\r\n", "\n").Replace('\r', '\n');

		// Mono异常调用栈格式:
		// at Test.execute() [0x00000] in E:\Project\Test.cs:10
		//
		// 转换为Unity Console的超链接格式:
		// at Test.execute() [0x00000] (at <link="href='E:\Project\Test.cs' line='10'">E:\Project\Test.cs:10</link>)

		string hyperlinkColor = getUnityConsoleHyperlinkColor();
		// 将Mono格式的调用栈转换为Unity Console超链接格式
		exceptionText = Regex.Replace
		(
			exceptionText,
			@"\s+in\s+(.+):(?:line\s+)?(\d+)\s*$",
			match =>
			{
				string filePath = match.Groups[1].Value.Trim();
				string lineString = match.Groups[2].Value;
				string linkFilePath = escapeUnityConsoleLink(filePath);
				string linkText = escapeUnityConsoleLink(filePath + ":" + lineString);
				return " (at <color=" + hyperlinkColor + "><link=\"href='" + linkFilePath + "' line='" + lineString + "'\">" + linkText + "</link></color>)";
			},
			RegexOptions.Multiline
		);

		// 兼容已经是(at 文件:行号),但还没有添加link标签的调用栈
		exceptionText = Regex.Replace
		(
			exceptionText,
			@"\(at\s+(?!<color|<link)(.+):(\d+)\)",
			match =>
			{
				string filePath = match.Groups[1].Value.Trim();
				string lineString = match.Groups[2].Value;
				string linkFilePath = escapeUnityConsoleLink(filePath);
				string linkText = escapeUnityConsoleLink(filePath + ":" + lineString);
				return "(at <color=" + hyperlinkColor + "><link=\"href='" + linkFilePath + "' line='" + lineString + "'\">" + linkText + "</link></color>)";
			}
		);

		string logMessage;
		if (info.isEmpty())
		{
			logMessage = exceptionText;
		}
		else
		{
			logMessage = info + "\n" + exceptionText;
		}

		// 调用栈超链接已经手动生成,不再让Unity重复解析调用栈
		callstackStartUTF16 = logMessage.Length;
		callstackStartUTF8 = Encoding.UTF8.GetByteCount(logMessage);
		return logMessage;
	}
	// 转义Unity Console超链接中的特殊字符。
	protected static string escapeUnityConsoleLink(string value)
	{
		if (value == null)
		{
			return "";
		}
		return value.Replace("&", "&amp;")
					.Replace("\"", "&quot;")
					.Replace("'", "&apos;")
					.Replace("<", "&lt;")
					.Replace(">", "&gt;");
	}
	// 获取与Unity Console一致的堆栈超链接颜色。
	protected static string getUnityConsoleHyperlinkColor()
	{
#if UNITY_EDITOR
		try
		{
			MethodInfo method = typeof(UnityEditor.EditorGUIUtility).GetMethod("GetHyperlinkColorForSkin", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method?.Invoke(null, null) is string color && !color.isEmpty())
			{
				return color;
			}
		}
		catch { }
		return UnityEditor.EditorGUIUtility.isProSkin ? "#40a0ff" : "#0000FF";
#else
		return "#0000FF";
#endif
	}
}