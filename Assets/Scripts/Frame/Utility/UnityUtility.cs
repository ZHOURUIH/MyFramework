using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
#if USE_ILRUNTIME
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using ILRuntime.CLR.TypeSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public delegate void onLog(string time, string info, LOG_LEVEL level, bool isError);
public class UnityUtility : FileUtility
{
	protected static onLog mOnLog;
	protected static uint mIDMaker;
	protected static int mMainThreadID;
	protected static bool mShowMessageBox = true;
	protected static LOG_LEVEL mLogLevel = LOG_LEVEL.NORMAL;
	public static void setMainThreadID(int mainThreadID) { mMainThreadID = mainThreadID; }
	public static bool isMainThread() { return Thread.CurrentThread.ManagedThreadId == mMainThreadID; }
	public static void setLogCallback(onLog callback) { mOnLog = callback; }
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		log("log level: " + mLogLevel, LOG_LEVEL.FORCE);
	}
	public static LOG_LEVEL getLogLevel()
	{
		return mLogLevel;
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
		string time = getTime(TIME_DISPLAY.HMSM);
		string trackStr = new StackTrace().ToString();
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(time + ": error: " + info + ", stack: " + trackStr);
#endif
		UnityEngine.Debug.LogError(time + ": error: " + info + ", stack: " + trackStr);
		mOnLog?.Invoke(time, "error: " + info + ", stack: " + trackStr, LOG_LEVEL.FORCE, true);
	}
	public static void log(string info, LOG_LEVEL level = LOG_LEVEL.NORMAL)
	{
		if ((int)level > (int)mLogLevel)
		{
			return;
		}
		string time = getTime(TIME_DISPLAY.HMSM);
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(getTime(TIME_DISPLAY.HMSM) + ": " + info);
#endif
		UnityEngine.Debug.Log(time + ": " + info);
		mOnLog?.Invoke(time, info, level, false);
	}
	public static void logWarning(string info, LOG_LEVEL level = LOG_LEVEL.NORMAL)
	{
		if ((int)level > (int)mLogLevel)
		{
			return;
		}
		string time = getTime(TIME_DISPLAY.HMSM);
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(getTime(TIME_DISPLAY.HMSM) + ": " + info);
#endif
		UnityEngine.Debug.LogWarning(time + ": " + info);
		mOnLog?.Invoke(time, info, level, false);
	}
	// 获取从1970年1月1日到现在所经过的毫秒数
	public static long timeGetTime()
	{
		TimeSpan timeForm19700101 = DateTime.Now - new DateTime(1970, 1, 1);
		return (long)timeForm19700101.TotalMilliseconds;
	}
	public static string getTime(TIME_DISPLAY display)
	{
		return getTime(DateTime.Now, display);
	}
	public static string getTime(int timeSecond, TIME_DISPLAY display)
	{
		int min = timeSecond / 60;
		int second = timeSecond % 60;
		int hour = min / 60;
		if (display == TIME_DISPLAY.HMSM)
		{
			return hour + ":" + min + ":" + second;
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return intToString(hour, 2) + ":" + intToString(min, 2) + ":" + intToString(second, 2);
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			int totalMin = timeSecond / 60;
			int totalHour = totalMin / 60;
			int totalDay = totalHour / 24;
			int curHour = totalHour % 24;
			int curMin = totalMin % 60;
			int curSecond = timeSecond % 60;
			// 大于等于1天
			if (totalDay > 0)
			{
				return totalDay + "天" + curHour + "时" + curMin + "分" + curSecond + "秒";
			}
			// 小于1天,并且大于等于1小时
			else if (totalHour > 0)
			{
				return totalHour + "时" + curMin + "分" + curSecond + "秒";
			}
			// 小于1小时,并且大于等于1分钟
			else if (totalMin > 0)
			{
				return totalMin + "分" + curSecond + "秒";
			}
			return timeSecond + "秒";
		}
		return EMPTY;
	}
	public static string getTime(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return time.Hour + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return intToString(time.Hour, 2) + ":" + intToString(time.Minute, 2) + ":" + intToString(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return time.Hour + "时" + time.Minute + "分" + time.Second + "秒";
		}
		return EMPTY;
	}
	// 将时间转化成时间戳,dateTime是本地时间
	public static long getTimeStamp(DateTime dateTime)
	{
		return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
	}
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampToDateTime(long unixTimeStamp)
	{
		return new DateTime(1970, 1, 1).AddSeconds(unixTimeStamp);
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
			string file = getCurSourceFileName(2);
			int line = getLineNum(2);
			logError("can not find " + name + ". file : " + file + ", line : " + line);
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
		Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
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
		Projector[] projectors = go.GetComponentsInChildren<Projector>(true);
		int projectorCount = projectors.Length;
		for (int i = 0; i < projectorCount; ++i)
		{
			findMaterialShader(projectors[i].material);
		}
	}
	public static void findUGUIShaders(GameObject go)
	{
		Graphic[] renderers = go.GetComponentsInChildren<Graphic>(true);
		int rendererCount = renderers.Length;
		for (int i = 0; i < rendererCount; ++i)
		{
			findMaterialShader(renderers[i].material);
		}
	}
	public static void setNormalProperty(GameObject obj, GameObject parent)
	{
		setNormalProperty(obj, parent, obj.name, Vector3.one, Vector3.zero, Vector3.zero);
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
		setNormalProperty(obj, parent, obj.name, Vector3.one, Vector3.zero, pos);
	}
	public static void setNormalProperty(GameObject obj, GameObject parent, string name, Vector3 scale, Vector3 rot, Vector3 pos)
	{
		if (parent != null)
		{
			obj.transform.SetParent(parent.transform);
		}
		else
		{
			obj.transform.SetParent(null);
		}
		obj.transform.localPosition = pos;
		obj.transform.localEulerAngles = rot;
		obj.transform.localScale = scale;
		obj.transform.name = name;
	}
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		T obj;
		try
		{
			obj = Activator.CreateInstance(classType, param) as T;
		}
		catch (Exception e)
		{
#if USE_ILRUNTIME
			obj = FrameBase.mILRSystem.getAppDomain().Instantiate<T>(classType.Name, param);
#else
			logError("create instance error! " + e.Message + ", inner error:" + e.InnerException?.Message);
			obj = null;
#endif
		}
		return obj;
	}
	public static T deepCopy<T>(T obj) where T : class
	{
		//如果是字符串或值类型则直接返回
		if (obj == null || obj is string || Typeof(obj).IsValueType)
		{
			return obj;
		}
		object retval = createInstance<object>(Typeof(obj));
		FieldInfo[] fields = Typeof(obj).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		int count = fields.Length;
		for (int i = 0; i < count; ++i)
		{
			FieldInfo field = fields[i];
			field.SetValue(retval, deepCopy(field.GetValue(obj)));
		}
		return (T)retval;
	}
	public static void getCameraRay(ref Vector3 screenPos, out Ray ray, Camera camera)
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
			ray = new Ray(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
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
			ray = new Ray(nearClipWorldPoint, normalize(farClipWorldPoint - nearClipWorldPoint));
		}
	}
	// 使用输出参数的方式避免多次赋值
	public static void getUIRay(ref Vector3 screenPos, out Ray ray)
	{
		getCameraRay(ref screenPos, out ray, getUICamera());
	}
	// screenCenterAsZero为false表示返回的坐标是以屏幕左下角为原点的坐标
	// screenCenterAsZero为true表示返回的坐标是以屏幕中心为原点的坐标
	public static Vector3 worldToScreenPos(Vector3 worldPos, Camera camera, bool screenCenterAsZero = true)
	{
		Vector3 screenPosition = camera.WorldToScreenPoint(worldPos);
		if (screenCenterAsZero)
		{
			screenPosition -= (Vector3)(getScreenSize() * 0.5f);
		}
		screenPosition.z = 0.0f;
		return screenPosition;
	}
	public static Vector3 worldToScreenPos(Vector3 worldPos, bool screenCenterAsZero = true)
	{
		return worldToScreenPos(worldPos, FrameBase.mCameraManager.getMainCamera().getCamera(), screenCenterAsZero);
	}
	public static Vector3 worldUIToScreenPos(Vector3 worldPos)
	{
		return worldToScreenPos(worldPos, getUICamera());
	}
	public static bool isGameObjectInScreen(Vector3 worldPos)
	{
		Vector3 screenPos = worldToScreenPos(worldPos, false);
		return screenPos.z >= 0.0f && inRange((Vector2)screenPos, Vector2.zero, getRootSize());
	}
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示以window的左下角为原点
	public static Vector2 screenPosToWindowPos(Vector2 screenPos, myUIObject window, bool screenCenterAsZero = true)
	{
		Camera camera = getUICamera();
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
	public static void setGameObjectLayer(GameObject obj, string layerName)
	{
		int layer = LayerMask.NameToLayer(layerName);
		if (!inRange(layer, 1, 32))
		{
			return;
		}
		setGameObjectLayer(obj, layer);
	}
	public static void setGameObjectLayer(GameObject obj, int layer)
	{
		obj.layer = layer;
		Transform[] childTransformList = obj.transform.GetComponentsInChildren<Transform>(true);
		int count = childTransformList.Length;
		for (int i = 0; i < count; ++i)
		{
			childTransformList[i].gameObject.layer = layer;
		}
	}
	public static void setParticleSortOrder(GameObject obj, int sortOrder)
	{
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; ++i)
		{
			renderers[i].sortingOrder = sortOrder;
		}
	}
	public static void setParticleSortLayerID(GameObject obj, int layerID)
	{
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
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
		T component = parent.GetComponent<T>();
		if (component != null)
		{
			return component;
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
	public static void activeObject(GameObject go, bool active = true)
	{
		go?.SetActive(active);
	}
	public static void activeChilds(GameObject go, bool active = true)
	{
		if (go != null)
		{
			Transform transform = go.transform;
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				transform.GetChild(i).gameObject.SetActive(active);
			}
		}
	}
	// preFrameCount为1表示返回调用getLineNum的行号
	public static int getLineNum(int preFrameCount = 1)
	{
		StackTrace st = new StackTrace(preFrameCount, true);
		return st.GetFrame(0).GetFileLineNumber();
	}
	// preFrameCount为1表示返回调用getCurSourceFileName的文件名
	public static string getCurSourceFileName(int preFrameCount = 1)
	{
		StackTrace st = new StackTrace(preFrameCount, true);
		return st.GetFrame(0).GetFileName();
	}
	public static uint makeID()
	{
		if (mIDMaker >= 0xFFFFFFFF)
		{
			logError("ID已超过最大值");
		}
		return ++mIDMaker;
	}
	public static void notifyIDUsed(uint id)
	{
		mIDMaker = getMax(mIDMaker, id);
	}
	public static Sprite texture2DToSprite(Texture2D tex)
	{
		if (tex == null)
		{
			return null;
		}
		return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
	public static void checkDownloadPath(ref string path, bool localPath)
	{
		if (!localPath)
		{
			return;
		}
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
#endif
#endif
	}
	public static Canvas getUGUIRootComponent()
	{
		if (FrameBase.mLayoutManager?.getRootObject() != null)
		{
			return FrameBase.mLayoutManager.getUGUIRootComponent();
		}
		return null;
	}
	public static Camera getUICamera()
	{
		if (FrameBase.mCameraManager?.getUICamera() != null)
		{
			return FrameBase.mCameraManager.getUICamera().getCamera();
		}
		return null;
	}
	// bottomHeight表示输入框下边框的y坐标
	public static void adjustByVirtualKeyboard(float bottomY, bool reset = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		int keyboardHeight = AndroidPluginManager.getKeyboardHeight();
		if (keyboardHeight > 0 && !reset)
		{
			// 200是软键盘上面的自带的输入框的高度
			keyboardHeight += 200;
			float cameraOffset = bottomY + getScreenSize().y * 0.5f - keyboardHeight;
			FrameBase.mCameraManager.getUICamera(guiType).setCameraPositionOffset(new Vector3(0.0f, cameraOffset, 0.0f));
		}
		else
		{
			FrameBase.mCameraManager.getUICamera(guiType).setCameraPositionOffset(Vector3.zero);
		}
#else
		logError("call only on android phone!");
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
		min = new Vector3(9999.0f, 9999.0f, 9999.0f);
		max = new Vector3(-9999.0f, -9999.0f, -9999.0f);
		getMinMaxVector3(ref corner0, ref min, ref max);
		getMinMaxVector3(ref corner1, ref min, ref max);
		getMinMaxVector3(ref corner2, ref min, ref max);
		getMinMaxVector3(ref corner3, ref min, ref max);
		getMinMaxVector3(ref corner4, ref min, ref max);
		getMinMaxVector3(ref corner5, ref min, ref max);
		getMinMaxVector3(ref corner6, ref min, ref max);
		getMinMaxVector3(ref corner7, ref min, ref max);
		checkFloat(ref min, precision);
		checkFloat(ref max, precision);
	}
	// 两个碰撞盒相交的条件是box0.min小于box1.max,并且box0.max大于box1.min
	public static bool overlapBox(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		return isVector3Less(ref min0, ref max1) && isVector3Greater(ref max0, ref min1) ||
			   isVector3Less(ref min1, ref max0) && isVector3Greater(ref max1, ref min0);
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
	public static int overlapAllCapsule(CapsuleCollider2D collider, Collider2D[] results, int layer = -1)
	{
		Transform transform = collider.transform;
		int hitCount = Physics2D.OverlapCapsuleNonAlloc(transform.position, collider.size, collider.direction, transform.localEulerAngles.z, results, layer);
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
		return isVector3Less(ref min0, ref max1) && isVector3Greater(ref max0, ref min1) ||
			   isVector3Less(ref min1, ref max0) && isVector3Greater(ref max1, ref min0);
	}
	public static bool overlapBoxIgnoreZ(BoxCollider box0, BoxCollider box1, GameObject parent, int precision = 4)
	{
		getMinMaxCorner(box0, out Vector3 min0, out Vector3 max0, parent, precision);
		getMinMaxCorner(box1, out Vector3 min1, out Vector3 max1, parent, precision);
		min0.z = 0.0f;
		max0.z = 1.0f;
		min1.z = 0.0f;
		max1.z = 1.0f;
		return isVector3Less(ref min0, ref max1) && isVector3Greater(ref max0, ref min1) ||
			   isVector3Less(ref min1, ref max0) && isVector3Greater(ref max1, ref min0);
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
	public static bool getRaycastPoint(Collider collider, ref Ray ray, ref Vector3 intersectPoint)
	{
		if (collider.Raycast(ray, out RaycastHit hit, 10000.0f))
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
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
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
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
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
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
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
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
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
		Vector3 parentScale = generateWorldScale(transform.parent);
		return multiVector3(parentScale, transform.localScale);
	}
	public static Quaternion generateWorldRotation(Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.localRotation;
		}
		Quaternion parentRotation = generateWorldRotation(transform.parent);
		return parentRotation * transform.localRotation;
	}
	public static Vector3 generateWorldPosition(Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.localPosition;
		}
		Vector3 parentWorldPosition = generateWorldPosition(transform.parent);
		Vector3 localPosition = transform.localPosition;
		Quaternion parentRotation = generateWorldRotation(transform.parent);
		localPosition = rotateVector3(localPosition, parentRotation);
		Vector3 parentScale = generateWorldScale(transform.parent);
		localPosition = multiVector3(localPosition, parentScale);
		return localPosition + parentWorldPosition;
	}
	public static Vector3 generateLocalPosition(Transform transform, Vector3 worldPosition)
	{
		Transform parent = transform.parent;
		Vector3 parentWorldPosition = generateWorldPosition(parent);
		Quaternion parentWorldRotation = generateWorldRotation(parent);
		Vector3 parentWorldScale = generateWorldScale(parent);
		Vector3 localPosition = worldPosition - parentWorldPosition;
		// 还原缩放
		localPosition = devideVector3(localPosition, parentWorldScale);
		// 还原旋转
		localPosition = rotateVector3(localPosition, Quaternion.Inverse(parentWorldRotation));
		return localPosition;
	}
	public static void setUGUIChildAlpha(GameObject go, float alpha)
	{
		Graphic graphic = go.GetComponent<Graphic>();
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
		// 去除UGUI自带的锚点,避免计算错误
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			rectTransform.anchorMin = Vector2.one * 0.5f;
			rectTransform.anchorMax = Vector2.one * 0.5f;
		}
		// 先更新自己
		obj.GetComponent<ScaleAnchor>()?.updateRect(force);
		obj.GetComponent<PaddingAnchor>()?.updateRect(force);
		if (layout != null)
		{
			myUIObject uiObj = layout.getUIObject(obj);
			uiObj?.notifyAnchorApply();
		}
		// 然后更新所有子节点
		Transform curTrans = obj.transform;
		int childCount = curTrans.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			applyAnchor(curTrans.GetChild(i).gameObject, force, layout);
		}
	}
	public static Vector2 getScreenSize()
	{
		return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
	}
	public static Vector2 getGameViewSize()
	{
#if UNITY_EDITOR
		Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
		MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
		object Res = GetMainGameView.Invoke(null, null);
		var gameView = (EditorWindow)Res;
		PropertyInfo prop = Typeof(gameView).GetProperty("currentGameViewSize", BindingFlags.NonPublic | BindingFlags.Instance);
		object gvsize = prop.GetValue(gameView, new object[0] { });
		Type gvSizeType = Typeof(gvsize);
		int height = (int)gvSizeType.GetProperty("height", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { });
		int width = (int)gvSizeType.GetProperty("width", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { });
		return new Vector2(width, height);
#else
		logError("getGameViewSize can only call in editor!");
		return Vector2.zero;
#endif
	}
	public static Vector2 getRootSize(bool useGameViewSize = false)
	{
		Vector2 rootSize = Vector2.zero;
		if (useGameViewSize)
		{
			Vector2 gameViewSize = getGameViewSize();
			Camera camera = getGameObject(FrameDefine.UGUI_ROOT + "/" + FrameDefine.UI_CAMERA, true).GetComponent<Camera>();
			rootSize = new Vector2(gameViewSize.y * camera.aspect, gameViewSize.y);
		}
		else
		{
			Canvas UGUIRoot = getUGUIRootComponent();
			if (UGUIRoot != null)
			{
				Rect rect = UGUIRoot.gameObject.GetComponent<RectTransform>().rect;
				rootSize = new Vector2(rect.height * getUICamera().aspect, rect.height);
			}
		}
		return rootSize;
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		Vector2 scale = Vector2.one;
		scale.x = rootSize.x * (1.0f / GameDefine.STANDARD_WIDTH);
		scale.y = rootSize.y * (1.0f / GameDefine.STANDARD_HEIGHT);
		return scale;
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
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列
	public static void autoGridVertical(myUGUIObject root, float interval = 0.0f, bool resizeRootSize = true, float minHeight = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		int childCount = transform.childCount;
		Vector2 rootSize = root.getWindowSize();
		// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
		if (resizeRootSize)
		{
			float height = 0.0f;
			for (int i = 0; i < childCount; ++i)
			{
				RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (!childRect.gameObject.activeSelf)
				{
					continue;
				}
				height += childRect.rect.height;
				// 最后一个子节点后不再添加间隔
				if (i != childCount - 1)
				{
					height += interval;
				}
			}
			rootSize.y = clampMin(height, minHeight);
			root.setWindowSize(rootSize);
		}
		float currentTop = rootSize.y * (1.0f - transform.pivot.y);
		for (int i = 0; i < childCount; ++i)
		{
			RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (!childRect.gameObject.activeSelf)
			{
				continue;
			}
			float curHeight = childRect.rect.height;
			childRect.localPosition = new Vector3(childRect.localPosition.x, currentTop - curHeight * 0.5f);
			currentTop -= curHeight;
			// 最后一个子节点后不再添加间隔
			if (i != childCount - 1)
			{
				currentTop -= interval;
			}
		}
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列
	public static void autoGridHorizontal(myUGUIObject root, float interval = 0.0f, bool resizeRootSize = true, float minWidth = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		int childCount = transform.childCount;
		Vector2 rootSize = root.getWindowSize();
		// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
		if (resizeRootSize)
		{
			float width = 0.0f;
			for (int i = 0; i < childCount; ++i)
			{
				RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (!childRect.gameObject.activeSelf)
				{
					continue;
				}
				width += childRect.rect.width;
				// 最后一个子节点后不再添加间隔
				if (i != childCount - 1)
				{
					width += interval;
				}
			}
			rootSize.x = clampMin(width, minWidth);
			root.setWindowSize(rootSize);
		}
		float currentLeft = rootSize.x * transform.pivot.x;
		for (int i = 0; i < childCount; ++i)
		{
			RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
			if (!childRect.gameObject.activeSelf)
			{
				continue;
			}
			float curWidth = childRect.rect.width;
			childRect.localPosition = new Vector3(currentLeft + curWidth * 0.5f, childRect.localPosition.y);
			currentLeft += curWidth;
			// 最后一个子节点后不再添加间隔
			if (i != childCount - 1)
			{
				currentLeft += interval;
			}
		}
	}
	// 移除数组中的第index个元素,validElementCount是数组中有效的元素个数
	public static void removeElement<T>(T[] array, int validElementCount, int index)
	{
		if (index < 0 || index >= validElementCount)
		{
			return;
		}
		int moveCount = validElementCount - index - 1;
		for (int i = 0; i < moveCount; ++i)
		{
			array[index + i] = array[index + i + 1];
		}
	}
	// 移除数组中的所有value,T为引用类型
	public static int removeClassElement<T>(T[] array, int validElementCount, T value) where T : class
	{
		// 从后往前遍历删除
		for (int i = validElementCount - 1; i >= 0; --i)
		{
			if (array[i] == value)
			{
				removeElement(array, validElementCount, i);
				--validElementCount;
			}
		}
		return validElementCount;
	}
	// 移除数组中的所有value,T为继承自IEquatable的值类型
	public static int removeValueElement<T>(T[] array, int validElementCount, T value) where T : IEquatable<T>
	{
		// 从后往前遍历删除
		for (int i = validElementCount - 1; i >= 0; --i)
		{
			if (array[i].Equals(value))
			{
				removeElement(array, validElementCount, i);
				--validElementCount;
			}
		}
		return validElementCount;
	}
	public static bool arrayContains<T>(T[] array, int arrayLen, T value) where T : class
	{
		for(int i = 0; i < arrayLen; ++i)
		{
			if(array[i] == value)
			{
				return true;
			}
		}
		return false;
	}
	// 获取类型,因为ILR的原因,如果是热更工程中的类型,直接使用typeof获取的是错误的类型
	// 所以需要使用此函数获取真实的类型,要获取真实类型必须要有一个实例
	// 为了方便调用,所以写在UnityUtility中
	public static Type Typeof<T>()
	{
		Type type = typeof(T);
#if USE_ILRUNTIME
		if (typeof(CrossBindingAdaptorType).IsAssignableFrom(type))
		{
			logError("无法获取热更工程中的类型");
			return null;
		}
		if(typeof(ILTypeInstance).IsAssignableFrom(type))
		{
			logError("无法获取热更工程中的类型");
			return null;
		}
#endif
		return type;
	}
	public static Type Typeof(object obj)
	{
#if USE_ILRUNTIME
		return obj?.GetActualType();
#else
		return obj?.GetType();
#endif
	}
}