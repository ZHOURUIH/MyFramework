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

// 与Unity相关的工具函数
public class UnityUtility : CSharpUtility
{
	protected static OnLog mOnLog;
	protected static bool mShowMessageBox = true;
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.FORCE;
	protected static PointerEventData mEventData;		// 缓存一个对象,避免每次都重新new一个
	public static new void initUtility() { }
	public static void setLogCallback(OnLog callback) { mOnLog = callback; }
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		logForce("log level: " + mLogLevel);
	}
	public static LOG_LEVEL getLogLevel() { return mLogLevel; }
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
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		// FrameBase.mLocalLog?.log(time + ": error: " + info + ", stack: " + trackStr);
#endif
		UnityEngine.Debug.LogError(time + ": error: " + info + ", stack: " + trackStr);
		mOnLog?.Invoke(time, ": error: " + info + ", stack: " + trackStr, LOG_LEVEL.FORCE, true);
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
			time = getTime(TIME_DISPLAY.HMSM);
		}
		else
		{
			time = getTimeThread(TIME_DISPLAY.HMSM);
		}
		if(!isEmpty(color))
		{
			info = colorStringNoBuilder(color, info);
		}
		string fullInfo = time + ": " + info;
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		// FrameBase.mLocalLog?.log(fullInfo);
#endif
		UnityEngine.Debug.Log(fullInfo, obj);
		mOnLog?.Invoke(time, info, level, false);
	}
	public static void logWarning(string info)
	{
		string time;
		if (isMainThread())
		{
			time = getTime(TIME_DISPLAY.HMSM);
		}
		else
		{
			time = getTimeThread(TIME_DISPLAY.HMSM);
		}
		string fullInfo = time + ": " + info;
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		// FrameBase.mLocalLog?.log(fullInfo);
#endif
		UnityEngine.Debug.LogWarning(fullInfo);
		mOnLog?.Invoke(time, info, LOG_LEVEL.FORCE, false);
	}
	public static void messageBox(string info, bool errorOrInfo)
	{
		string title = errorOrInfo ? "错误" : "提示";
		// 在编辑器中显示对话框
#if UNITY_EDITOR
		EditorUtility.DisplayDialog(title, info, "确认");
#endif
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
	public static Ray getMainCameraMouseRay()
	{
		return getCameraRay(FrameUtility.getMousePosition(), FrameUtility.getMainCamera().getCamera());
	}
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
			screenPosition -= (Vector3)(getScreenSize() * 0.5f);
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
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示以window的左下角为原点
	public static Vector2 screenPosToWindow(Vector2 screenPos, myUIObject window, bool screenCenterAsZero = true)
	{
		Camera camera = FrameUtility.getUICamera();
		Vector2 cameraSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize();
		screenPos = multiVector2(devideVector2(screenPos, cameraSize), rootSize);
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos -= rootSize * 0.5f;
		Vector2 windowPos = screenPos;
		if (window != null)
		{
			myUIObject root = FrameBase.mLayoutManager.getUIRoot();
			Vector2 parentWorldPosition = devideVector3(window.getWorldPosition(), root.getScale());
			windowPos = devideVector2(screenPos - parentWorldPosition, window.getWorldScale());
			if (!screenCenterAsZero)
			{
				windowPos += window.getWindowSize() * 0.5f;
			}
		}
		else
		{
			if (!screenCenterAsZero)
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
		if (!inRange(layer, 1, 32))
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
	// 获得指定屏幕坐标下的可交互UI,比如勾选了RaycastTarget的Image或Text等,Button,InputField等
	public static void checkUGUIInteractable(Vector2 screenPosition, List<GameObject> clickList)
	{
		if (clickList == null)
		{
			return;
		}
		if (mEventData == null)
		{
			mEventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
		}
		// 将点击位置的屏幕坐标赋值给点击事件
		mEventData.position = new Vector2(screenPosition.x, screenPosition.y);
		FrameUtility.LIST(out List<RaycastResult> results);
		// 向点击处发射射线
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(mEventData, results);
		// 如果点击到了非透明的图片或者文字,则不可穿透射线
		int count = results.Count;
		for (int i = 0; i < count; ++i)
		{
			GameObject go = results[i].gameObject;
			var graphic = go.GetComponent<Graphic>();
			if (graphic != null && graphic.raycastTarget)
			{
				clickList.Add(go);
				continue;
			}
			var selectable = go.GetComponent<Selectable>();
			if(selectable != null && selectable.interactable)
			{
				clickList.Add(go);
				continue;
			}
		}
		FrameUtility.UN_LIST(results);
	}
	// 获得指定屏幕坐标下的可见UI,可见UI是指已激活且透明度不为0
	public static GameObject getPointerOnUI(Vector2 screenPosition)
	{
		if (mEventData == null)
		{
			mEventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
		}
		// 将点击位置的屏幕坐标赋值给点击事件
		mEventData.position = new Vector2(screenPosition.x, screenPosition.y);
		FrameUtility.LIST(out List<RaycastResult> results);
		// 向点击处发射射线
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(mEventData, results);
		// 如果点击到了非透明的图片或者文字,则不可穿透射线
		GameObject hoverObject = null;
		int count = results.Count;
		for (int i = 0; i < count; ++i)
		{
			GameObject go = results[i].gameObject;
			var image = go.GetComponent<Image>();
			if (image != null)
			{
				if (image.raycastTarget && !isFloatZero(image.color.a))
				{
					hoverObject = go;
					break;
				}
				continue;
			}
			var text = go.GetComponent<Text>();
			if (text != null)
			{
				if (text.raycastTarget && !isFloatZero(text.color.a))
				{
					hoverObject = go;
					break;
				}
				continue;
			}
		}
		FrameUtility.UN_LIST(results);
		return hoverObject;
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
	public static void setUGUIChildAlpha(GameObject go, float alpha)
	{
		var graphic = go.GetComponent<Graphic>();
		if (graphic != null)
		{
			Color color = graphic.color;
			color.a = alpha;
			graphic.color = color;
		}
		Transform transform = go.transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			GameObject child = transform.GetChild(i).gameObject;
			setUGUIChildAlpha(child, alpha);
		}
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
	public static Vector2 getScreenSize()
	{
		return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
	}
	public static Vector2 getRootSize()
	{
		Canvas uguiRoot = FrameUtility.getUGUIRootComponent();
		if (uguiRoot == null)
		{
			return Vector2.zero;
		}
		Rect rect = uguiRoot.gameObject.GetComponent<RectTransform>().rect;
		return new Vector2(rect.height * FrameUtility.getUICamera().aspect, rect.height);
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		return new Vector2(rootSize.x * (1.0f / FrameDefineExtension.STANDARD_WIDTH), 
							rootSize.y * (1.0f / FrameDefineExtension.STANDARD_HEIGHT));
	}
	public static Vector2 adjustScreenScale(ASPECT_BASE aspectBase = ASPECT_BASE.AUTO)
	{
		return adjustScreenScale(getScreenScale(getRootSize()), aspectBase);
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
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, CORNER startCorner = CORNER.LEFT_TOP)
	{
		autoGridFixedRootHeight(root, gridSize, Vector2.zero, startCorner);
	}
	// 保持父节点的高度,从指定角开始纵向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, Vector2 interval, CORNER startCorner = CORNER.LEFT_TOP)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		FrameUtility.LIST(out List<RectTransform> childList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (childRect.gameObject.activeSelf)
			{
				childList.Add(childRect);
			}
		}

		// 计算父节点大小
		Vector2 rootSize = root.getWindowSize();
		Vector3 beforeRealPosition = root.getPositionNoPivot();
		Vector3 beforeRootLeftTop = new Vector3(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
		Vector3 beforeRootLeftBottom = new Vector3(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
		Vector3 beforeRootRightTop = new Vector3(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
		Vector3 beforeRootRightBottom = new Vector3(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
		int rowCount = 1;
		if (rootSize.y > gridSize.y)
		{
			rowCount = (int)((rootSize.y - gridSize.y) / (interval.y + gridSize.y)) + 1;
		}
		int activeChildCount = childList.Count;
		// 固定父节点高度时只能纵向排列
		int columnCount = activeChildCount / rowCount + clampMax(activeChildCount % rowCount, 1);
		rootSize.x = columnCount * gridSize.x + (columnCount - 1) * interval.x;
		root.setWindowSize(rootSize);

		// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标,并且调整父节点的坐标
		Vector2 startPos = Vector2.zero;
		int horizontalSign = 0;
		int verticalSign = 0;
		Vector3 curRealPosition = root.getPositionNoPivot();
		if (startCorner == CORNER.LEFT_TOP)
		{
			startPos = new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowTop());
			horizontalSign = 1;
			verticalSign = -1;
			// 保持左上角的坐标与改变大小之前的左上角坐标一致
			Vector3 curRootLeftTop = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftTop, curRootLeftTop))
			{
				root.setPosition(root.getPosition() + beforeRootLeftTop - curRootLeftTop);
			}
		}
		else if (startCorner == CORNER.LEFT_BOTTOM)
		{
			startPos = new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowBottom());
			horizontalSign = 1;
			verticalSign = 1;
			// 保持左下角的坐标与改变大小之前的左下角坐标一致
			Vector3 curRootLeftBottom = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftBottom, curRootLeftBottom))
			{
				root.setPosition(root.getPosition() + beforeRootLeftBottom - curRootLeftBottom);
			}
		}
		else if (startCorner == CORNER.RIGHT_TOP)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowTop());
			horizontalSign = -1;
			verticalSign = -1;
			// 保持右上角的坐标与改变大小之前的右上角坐标一致
			Vector3 curRootRightTop = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootRightTop, curRootRightTop))
			{
				root.setPosition(root.getPosition() + beforeRootRightTop - curRootRightTop);
			}
		}
		else if (startCorner == CORNER.RIGHT_BOTTOM)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowBottom());
			horizontalSign = -1;
			verticalSign = 1;
			// 保持右下角的坐标与改变大小之前的右下角坐标一致
			Vector3 curRootRightBottom = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootRightBottom, curRootRightBottom))
			{
				root.setPosition(root.getPosition() + beforeRootRightBottom - curRootRightBottom);
			}
		}

		// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
		// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
		for (int i = 0; i < activeChildCount; ++i)
		{
			RectTransform child = childList[i];
			int indexX = i / rowCount;
			int indexY = i % rowCount;
			Vector2 pos = new Vector2((indexX * gridSize.x + indexX * interval.x) * horizontalSign, 
										(indexY * gridSize.y + indexY * interval.y) * verticalSign);
			child.localPosition = startPos + pos;
			WidgetUtility.setRectSize(child, gridSize);
		}
		FrameUtility.UN_LIST(childList);
	}
	public static void autoGridFixedRootWidth(myUGUIObject root, Vector2 gridSize, CORNER startCorner = CORNER.LEFT_TOP)
	{
		autoGridFixedRootWidth(root, gridSize, Vector2.zero, startCorner);
	}
	// 保持父节点的宽度,从指定角开始横向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public static void autoGridFixedRootWidth(myUGUIObject root, Vector2 gridSize, Vector2 interval, CORNER startCorner = CORNER.LEFT_TOP)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		FrameUtility.LIST(out List<RectTransform> childList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (childRect.gameObject.activeSelf)
			{
				childList.Add(childRect);
			}
		}

		// 计算父节点大小
		Vector2 rootSize = root.getWindowSize();
		Vector3 beforeRealPos = root.getPositionNoPivot();
		Vector3 beforeRootLeftTop = new Vector3(beforeRealPos.x - rootSize.x * 0.5f, beforeRealPos.y + rootSize.y * 0.5f);
		Vector3 beforeRootLeftBottom = new Vector3(beforeRealPos.x - rootSize.x * 0.5f, beforeRealPos.y - rootSize.y * 0.5f);
		Vector3 beforeRootRightTop = new Vector3(beforeRealPos.x + rootSize.x * 0.5f, beforeRealPos.y + rootSize.y * 0.5f);
		Vector3 beforeRootRightBottom = new Vector3(beforeRealPos.x + rootSize.x * 0.5f, beforeRealPos.y - rootSize.y * 0.5f);
		int columnCount = 1;
		if (rootSize.x > gridSize.x)
		{
			columnCount = (int)((rootSize.x - gridSize.x) / (interval.x + gridSize.x)) + 1;
		}
		int activeChildCount = childList.Count;
		int rowCount = ceil(activeChildCount / (float)columnCount);
		rootSize.y = rowCount * gridSize.y + (rowCount - 1) * interval.y;
		root.setWindowSize(rootSize);

		// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标,并且调整父节点的坐标
		Vector2 startPos = Vector2.zero;
		int horizontalSign = 0;
		int verticalSign = 0;
		Vector3 curRealPosition = root.getPositionNoPivot();
		if (startCorner == CORNER.LEFT_TOP)
		{
			startPos = new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowTop());
			horizontalSign = 1;
			verticalSign = -1;
			// 保持左上角的坐标与改变大小之前的左上角坐标一致
			Vector3 curRootLeftTop = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftTop, curRootLeftTop))
			{
				root.setPosition(root.getPosition() + beforeRootLeftTop - curRootLeftTop);
			}
		}
		else if (startCorner == CORNER.LEFT_BOTTOM)
		{
			startPos = new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowBottom());
			horizontalSign = 1;
			verticalSign = 1;
			// 保持左下角的坐标与改变大小之前的左下角坐标一致
			Vector3 curRootLeftBottom = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftBottom, curRootLeftBottom))
			{
				root.setPosition(root.getPosition() + beforeRootLeftBottom - curRootLeftBottom);
			}
		}
		else if (startCorner == CORNER.RIGHT_TOP)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowTop());
			horizontalSign = -1;
			verticalSign = -1;
			// 保持右上角的坐标与改变大小之前的右上角坐标一致
			Vector3 curRootRightTop = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootRightTop, curRootRightTop))
			{
				root.setPosition(root.getPosition() + beforeRootRightTop - curRootRightTop);
			}
		}
		else if (startCorner == CORNER.RIGHT_BOTTOM)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowBottom());
			horizontalSign = -1;
			verticalSign = 1;
			// 保持右下角的坐标与改变大小之前的右下角坐标一致
			Vector3 curRootRightBottom = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootRightBottom, curRootRightBottom))
			{
				root.setPosition(root.getPosition() + beforeRootRightBottom - curRootRightBottom);
			}
		}

		// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
		// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
		for (int i = 0; i < activeChildCount; ++i)
		{
			RectTransform child = childList[i];
			int indexX = i % columnCount;
			int indexY = i / columnCount;
			Vector2 pos = new Vector2((indexX * gridSize.x + indexX * interval.x) * horizontalSign,
										(indexY * gridSize.y + indexY * interval.y) * verticalSign);
			child.localPosition = startPos + pos;
			WidgetUtility.setRectSize(child, gridSize);
		}
		FrameUtility.UN_LIST(childList);
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小
	public static void autoGridVertical(myUGUIObject root, float interval = 0.0f, float minHeight = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		FrameUtility.LIST(out List<RectTransform> childList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (childRect.gameObject.activeSelf)
			{
				childList.Add(childRect);
			}
		}

		float beforeRootTopY = root.getPositionNoPivot().y + root.getWindowSize().y * 0.5f;
		// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
		Vector2 rootSize = root.getWindowSize();
		float height = 0.0f;
		for (int i = 0; i < childList.Count; ++i)
		{
			height += childList[i].rect.height;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1)
			{
				height += interval;
			}
		}
		rootSize.y = clampMin(height, minHeight);
		root.setWindowSize(rootSize);

		// 改变完父节点的大小后需要保持父节点上边界的y坐标不变
		float curRootTopY = root.getPositionNoPivot().y + root.getWindowSize().y * 0.5f;
		if (!isFloatEqual(curRootTopY, beforeRootTopY))
		{
			Vector3 rootPos = root.getPosition();
			root.setPosition(new Vector3(rootPos.x, rootPos.y + beforeRootTopY - curRootTopY, rootPos.z));
		}

		// 计算子节点坐标
		float currentTop = root.getWindowTop();
		for (int i = 0; i < childList.Count; ++i)
		{
			RectTransform childRect = childList[i];
			float curHeight = childRect.rect.height;
			childRect.localPosition = new Vector3(childRect.localPosition.x, currentTop - curHeight * 0.5f);
			currentTop -= curHeight;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1)
			{
				currentTop -= interval;
			}
		}
		FrameUtility.UN_LIST(childList);
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小
	public static void autoGridHorizontal(myUGUIObject root, float interval = 0.0f, float minWidth = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		FrameUtility.LIST(out List<RectTransform> childList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (childRect.gameObject.activeSelf)
			{
				childList.Add(childRect);
			}
		}

		float beforeRootLeftX = root.getPositionNoPivot().x - root.getWindowSize().x * 0.5f;
		// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
		Vector2 rootSize = root.getWindowSize();
		float width = 0.0f;
		for (int i = 0; i < childList.Count; ++i)
		{
			width += childList[i].rect.width;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1)
			{
				width += interval;
			}
		}
		rootSize.x = clampMin(width, minWidth);
		root.setWindowSize(rootSize);

		// 改变完父节点的大小后需要保持父节点左边界的x坐标不变
		float curRootLeftX = root.getPositionNoPivot().x - root.getWindowSize().x * 0.5f;
		if (!isFloatEqual(curRootLeftX, beforeRootLeftX))
		{
			Vector3 rootPos = root.getPosition();
			root.setPosition(new Vector3(rootPos.x + beforeRootLeftX - curRootLeftX, rootPos.y, rootPos.z));
		}

		// 计算子节点坐标
		float currentLeft = root.getWindowLeft();
		for (int i = 0; i < childList.Count; ++i)
		{
			RectTransform childRect = childList[i];
			float curWidth = childRect.rect.width;
			childRect.localPosition = new Vector3(currentLeft + curWidth * 0.5f, childRect.localPosition.y);
			currentLeft += curWidth;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1)
			{
				currentLeft += interval;
			}
		}
		FrameUtility.UN_LIST(childList);
	}
}