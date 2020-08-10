using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public delegate void onLog(string time, string info, LOG_LEVEL level, bool isError);
public class UnityUtility : FileUtility
{
	private static LOG_LEVEL mLogLevel = LOG_LEVEL.LL_NORMAL;
	protected static bool mShowMessageBox = true;
	protected static int mIDMaker;
	public static onLog mOnLog;
	public static void setLogLevel(LOG_LEVEL level)
	{
		mLogLevel = level;
		logInfo("log level: " + mLogLevel, LOG_LEVEL.LL_FORCE);
	}
	public static LOG_LEVEL getLogLevel()
	{
		return mLogLevel;
	}
	public static void logError(string info, bool isMainThread = true)
	{
		if (isMainThread && mShowMessageBox)
		{
			messageBox(info, true);
#if UNITY_EDITOR
			EditorApplication.isPaused = true;
#endif
			// 运行一次只显示一次提示框,避免在循环中报错时一直弹窗
			mShowMessageBox = false;
		}
		string time = getTime(TIME_DISPLAY.TD_HMSM_0);
		string trackStr = new StackTrace().ToString();
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(time + ": error: " + info + ", stack: " + trackStr);
#endif
		UnityEngine.Debug.LogError(time + ": error: " + info + ", stack: " + trackStr);
		mOnLog?.Invoke(time, "error: " + info + ", stack: " + trackStr, LOG_LEVEL.LL_FORCE, true);
	}
	public static void logInfo(string info, LOG_LEVEL level = LOG_LEVEL.LL_NORMAL)
	{
		if ((int)level > (int)mLogLevel)
		{
			return;
		}
		string time = getTime(TIME_DISPLAY.TD_HMSM_0);
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(getTime(TIME_DISPLAY.TD_HMSM_0) + ": " + info);
#endif
		UnityEngine.Debug.Log(time + ": " + info);
		mOnLog?.Invoke(time, info, level, false);
	}
	public static void logWarning(string info, LOG_LEVEL level = LOG_LEVEL.LL_NORMAL)
	{
		if ((int)level > (int)mLogLevel)
		{
			return;
		}
		string time = getTime(TIME_DISPLAY.TD_HMSM_0);
#if !UNITY_EDITOR
		// 打包后使用LocalLog打印日志
		FrameBase.mLocalLog?.log(getTime(TIME_DISPLAY.TD_HMSM_0) + ": " + info);
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
		if(display == TIME_DISPLAY.TD_HMSM_0)
		{
			return hour + ":" + min + ":" + second;
		}
		else if (display == TIME_DISPLAY.TD_HMS_2)
		{
			return intToString(hour, 2) + ":" + intToString(min, 2) + ":" + intToString(second, 2);
		}
		else if(display == TIME_DISPLAY.TD_DHMS_ZH)
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
		return "";
	}
	public static string getTime(DateTime time, TIME_DISPLAY display)
	{
		if(display == TIME_DISPLAY.TD_HMSM_0)
		{
			return time.Hour + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
		}
		else if(display == TIME_DISPLAY.TD_HMS_2)
		{
			return intToString(time.Hour, 2) + ":" + intToString(time.Minute, 2) + ":" + intToString(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.TD_DHMS_ZH)
		{
			return time.Hour + "时" + time.Minute + "分" + time.Second + "秒";
		}
		return "";
	}
	//将时间转化成时间戳
	public static long getTimeStamp(DateTime dateTime)
	{
		DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
		return (long)(dateTime - startTime).TotalSeconds; // 相差秒数
	}
	//将时间戳转化成时间
	public static DateTime timeStampToDateTime(long unixTimeStamp)
	{
		DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
		return startTime.AddSeconds(unixTimeStamp);
	}
	public static void copyTextToClipbord(string str)
	{
#if UNITY_STANDALONE_WIN
		Clipboard.SetText(str);
#else
		logError("clipboard can only used in windows");
#endif
	}
	public static string getTextFromClipboard()
	{
#if UNITY_STANDALONE_WIN
		return Clipboard.GetText();
#else
		logError("clipboard can only used in windows");
		return null;
#endif
	}
	public static void messageBox(string info, bool errorOrInfo)
	{
		string title = errorOrInfo ? "错误" : "提示";
		// 在编辑器中显示对话框
#if UNITY_EDITOR
		EditorUtility.DisplayDialog(title, info, "确认");
#elif UNITY_STANDALONE_WIN
		// 游戏运行过程中显示窗口提示框
		System.Windows.Forms.MessageBox.Show(info, title, MessageBoxButtons.OK, errorOrInfo ? MessageBoxIcon.Error : MessageBoxIcon.Information);
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
	public static GameObject getGameObject(GameObject parent, string name, bool errorIfNull = false, bool recursive = true)
	{
		if (isEmpty(name))
		{
			return null;
		}
		GameObject go = null;
		if (parent == null)
		{
			go = GameObject.Find(name);
		}
		else
		{
			Transform trans = parent.transform.Find(name);
			if (trans != null)
			{
				go = trans.gameObject;
			}
			// 如果父节点的第一级子节点中找不到,就递归查找
			else
			{
				if(recursive)
				{
					int childCount = parent.transform.childCount;
					for (int i = 0; i < childCount; ++i)
					{
						go = getGameObject(parent.transform.GetChild(i).gameObject, name, false, recursive);
						if (go != null)
						{
							break;
						}
					}
				}
			}
		}
		if(go == null && errorIfNull)
		{
			string file = getCurSourceFileName(2);
			int line = getLineNum(2);
			logError("can not find " + name + ". file : " + file + ", line : " + line);
		}
		return go;
	}
	public static GameObject cloneObject(GameObject oriObj, string name)
	{
		GameObject obj = GameObject.Instantiate(oriObj);
		obj.name = name;
		return obj;
	}
	public static GameObject createGameObject(string name, GameObject parent = null)
	{
		GameObject obj = new GameObject(name);
		if(parent != null)
		{
			obj.GetComponent<Transform>().SetParent(parent.GetComponent<Transform>());
		}
		return obj;
	}
	// 根据预设名实例化
	public static GameObject instantiatePrefab(GameObject parent, string prefabName, string name, Vector3 scale, Vector3 rot, Vector3 pos, bool active)
	{
		if (isEmpty(prefabName))
		{
			return null;
		}
		GameObject prefab = FrameBase.mResourceManager.loadResource<GameObject>(prefabName, true);
		if (prefab == null)
		{
			logError("can not find prefab : " + prefabName);
			return null;
		}
		GameObject obj = instantiatePrefab(parent, prefab, name, scale, rot, pos, active);
		FrameBase.mResourceManager.unload(ref prefab);
		return obj;
	}
	// prefabName为Resource下的相对路径
	public static GameObject instantiatePrefab(GameObject parent, string prefabName, bool active)
	{
		if (isEmpty(prefabName))
		{
			return null;
		}
		string name = getFileName(prefabName);
		return instantiatePrefab(parent, prefabName, name, Vector3.one, Vector3.zero, Vector3.zero, active);
	}
	// 根据预设对象实例化
	public static GameObject instantiatePrefab(GameObject parent, GameObject prefab, bool active)
	{
		string name = getFileName(prefab.name);
		return instantiatePrefab(parent, prefab, name, Vector3.one, Vector3.zero, Vector3.zero, active);
	}
	public static GameObject instantiatePrefab(GameObject parent, GameObject prefab, string name, bool active)
	{
		return instantiatePrefab(parent, prefab, name, Vector3.one, Vector3.zero, Vector3.zero, active);
	}
	public static GameObject instantiatePrefab(GameObject parent, string prefabName, string name, bool active)
	{
		if (isEmpty(prefabName))
		{
			return null;
		}
		return instantiatePrefab(parent, prefabName, name, Vector3.one, Vector3.zero, Vector3.zero, active);
	}
	// parent为实例化后挂接的父节点
	// prefabName为预设名,带Resources下相对路径
	// name为实例化后的名字
	// 其他三个是实例化后本地的变换
	public static GameObject instantiatePrefab(GameObject parent, GameObject prefab, string name, Vector3 scale, Vector3 rot, Vector3 pos, bool active)
	{
		GameObject obj = GameObject.Instantiate(prefab);
		setNormalProperty(obj, parent, name, scale, rot, pos);
		obj.SetActive(active);
		findShaders(obj);
		return obj;
	}
	public static void findMaterialShader(Material material)
	{
		// 在编辑器中从AssetBundle加载如果不重新查找材质,则会出现材质丢失的错误,但是真机上不查找却没有任何影响
		// 目前暂未查明具体原因,所以为了保证两端都显示正常,只在编辑器下才会重新查找材质
#if UNITY_EDITOR
		if(material == null)
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
		for(int i = 0; i < projectorCount; ++i)
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
		if(parent != null)
		{
			obj.transform.SetParent(parent.transform);
		}
		obj.transform.localPosition = pos;
		obj.transform.localEulerAngles = rot;
		obj.transform.localScale = scale;
		obj.transform.name = name;
	}
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		return Activator.CreateInstance(classType, param) as T;
	}
	public static T deepCopy<T>(T obj) where T : class
	{
		//如果是字符串或值类型则直接返回
		if (obj == null || obj is string || obj.GetType().IsValueType)
		{
			return obj;
		}
		object retval = createInstance<object>(obj.GetType());
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		foreach (FieldInfo field in fields)
		{
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
	public static void getUIRay(ref Vector3 screenPos, out Ray ray, bool ngui = true)
	{
		getCameraRay(ref screenPos, out ray, getUICamera(ngui));
	}
	// halfScreenOffset为false返回的坐标是以屏幕左下角为原点的坐标
	// halfScreenOffset为true表示返回的坐标是以屏幕中心为原点的坐标
	public static Vector3 worldToScreenPos(Vector3 worldPos, Camera camera, bool halfScreenOffset = true)
	{
		Vector3 screenPosition = camera.WorldToScreenPoint(worldPos);
		if(halfScreenOffset)
		{
			screenPosition -= (Vector3)(getScreenSize() * 0.5f);
		}
		screenPosition.z = 0.0f;
		return screenPosition;
	}
	public static Vector3 worldToScreenPos(Vector3 worldPos, bool halfScreenOffset = true)
	{
		return worldToScreenPos(worldPos, FrameBase.mCameraManager.getMainCamera().getCamera(), halfScreenOffset);
	}
	public static Vector3 worldUIToScreenPos(Vector3 worldPos, bool ngui)
	{
		return worldToScreenPos(worldPos, getUICamera(ngui));
	}
	public static bool whetherGameObjectInScreen(Vector3 worldPos, bool isNGUI)
	{
		Vector3 screenPos = worldToScreenPos(worldPos, false);
		Vector2 rootSize = getRootSize(isNGUI);
		return screenPos.z >= 0.0f && isInRange((Vector2)screenPos, Vector2.zero, rootSize);
	}
	// screenCenterAsZero为true表示返回的坐标是以window的中心为原点,false表示已window的左下角为原点
	public static Vector2 screenPosToWindowPos(Vector2 screenPos, txUIObject window, bool screenCenterAsZero = true, bool isNGUI = true)
	{
		Camera camera = getUICamera(isNGUI);
		Vector2 cameraSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
		Vector2 rootSize = getRootSize(isNGUI);
		screenPos = multiVector2(devideVector2(screenPos, cameraSize), rootSize);
		// 将坐标转换到以屏幕中心为原点的坐标
		screenPos -= rootSize * 0.5f;
		Vector2 windowPos = screenPos;
		if (window != null)
		{
			txUIObject root = FrameBase.mLayoutManager.getUIRoot(isNGUI);
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
		if (!isInRange(layer, 1, 32))
		{
			return;
		}
		setGameObjectLayer(obj, layer);
	}
	public static void setGameObjectLayer(GameObject obj, int layer)
	{
		obj.layer = layer;
		Transform[] childTransformList = obj.transform.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in childTransformList)
		{
			t.gameObject.layer = layer;
		}
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
		if(go != null)
		{
			Transform transform = go.transform;
			int childCount = transform.childCount;
			for(int i = 0; i < childCount; ++i)
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
	public static int makeID() { return ++mIDMaker; }
	public static void notifyIDUsed(int id)
	{
		mIDMaker = getMax(mIDMaker, id);
	}
	public static Sprite texture2DToSprite(Texture2D tex)
	{
		if(tex == null)
		{
			return null;
		}
		return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
#if USE_NGUI
	// name为Resource下相对路径,不带后缀名
	public static UIAtlas loadNGUIAtlas(string name)
	{
		GameObject go = FrameBase.mResourceManager.loadResource<GameObject>(name, true);
		if (go != null)
		{
			return go.GetComponent<UIAtlas>();
		}
		return null;
	}
#endif
	public static void checkDownloadPath(ref string path, bool localPath)
	{
		if(!localPath)
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
#if USE_NGUI
	public static UIRoot getNGUIRootComponent()
	{
		if(FrameBase.mLayoutManager != null && FrameBase.mLayoutManager.getRootObject(true) != null)
		{
			return FrameBase.mLayoutManager.getNGUIRootComponent();
		}
		return null;
	}
#endif
	public static Canvas getUGUIRootComponent()
	{
		if (FrameBase.mLayoutManager != null && FrameBase.mLayoutManager.getRootObject(false) != null)
		{
			return FrameBase.mLayoutManager.getUGUIRootComponent();
		}
		return null;
	}
	public static Camera getUICamera(bool ngui)
	{
		if(FrameBase.mCameraManager != null && FrameBase.mCameraManager.getUICamera(ngui) != null)
		{
			return FrameBase.mCameraManager.getUICamera(ngui).getCamera();
		}
		return null;
	}
	// bottomHeight表示输入框下边框的y坐标
	public static void adjustByVirtualKeyboard(bool isNGUI, float bottomY, bool reset = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		int keyboardHeight = AndroidPluginManager.getKeyboardHeight();
		if (keyboardHeight > 0 && !reset)
		{
			// 200是软键盘上面的自带的输入框的高度
			keyboardHeight += 200;
			float cameraOffset = bottomY + getScreenSize().y * 0.5f - keyboardHeight;
			FrameBase.mCameraManager.getUICamera(isNGUI).setCameraPositionOffset(new Vector3(0.0f, cameraOffset, 0.0f));
		}
		else
		{
			FrameBase.mCameraManager.getUICamera(isNGUI).setCameraPositionOffset(Vector3.zero);
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
		if(parent != null)
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
	public static Collider[] overlapAllBox(BoxCollider collider, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 colliderWorldPos = localToWorld(transform, collider.center);
		return Physics.OverlapBox(colliderWorldPos, collider.size * 0.5f, transform.localRotation, layer);
	}
	public static Collider2D[] overlapAllBox(BoxCollider2D collider, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
		return Physics2D.OverlapBoxAll(colliderWorldPos, collider.size, -transform.localEulerAngles.z, layer);
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
	public static Collider2D[] overlapAllCollider(Collider2D collider, int layer = -1)
	{
		Collider2D[] hitColliders = null;
		if (collider is BoxCollider2D)
		{
			hitColliders = overlapAllBox(collider as BoxCollider2D, layer);
		}
		else if (collider is CircleCollider2D)
		{
			hitColliders = overlapAllSphere(collider as CircleCollider2D, layer);
		}
		else if (collider is CapsuleCollider2D)
		{
			hitColliders = overlapAllCapsule(collider as CapsuleCollider2D, layer);
		}
		return hitColliders;
	}
	public static Collider[] overlapCollider(Collider collider, int layer = -1)
	{
		Collider[] hitColliders = null;
		if (collider is BoxCollider)
		{
			hitColliders = overlapAllBox(collider as BoxCollider, layer);
		}
		else if (collider is SphereCollider)
		{
			hitColliders = overlapAllSphere(collider as SphereCollider, layer);
		}
		else if (collider is CapsuleCollider)
		{
			hitColliders = overlapAllCapsule(collider as CapsuleCollider, layer);
		}
		return hitColliders;
	}
	public static Collider[] overlapAllSphere(SphereCollider collider, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 colliderWorldPos = localToWorld(transform, collider.center);
		return Physics.OverlapSphere(colliderWorldPos, collider.radius, layer);
	}
	public static Collider2D[] overlapAllSphere(CircleCollider2D collider, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector2 colliderWorldPos = localToWorld(transform, collider.offset);
		return Physics2D.OverlapCircleAll(colliderWorldPos, collider.radius, layer);
	}
	public static Collider[] overlapAllCapsule(CapsuleCollider collider, int layer = -1)
	{
		Transform transform = collider.transform;
		Vector3 point0 = collider.center + new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		Vector3 point1 = collider.center - new Vector3(0.0f, collider.height * 0.5f, 0.0f);
		point0 = localToWorld(transform, point0);
		point1 = localToWorld(transform, point1);
		return Physics.OverlapCapsule(point0, point1, collider.radius, layer);
	}
	public static Collider2D[] overlapAllCapsule(CapsuleCollider2D collider, int layer = -1)
	{
		Transform transform = collider.transform;
		return Physics2D.OverlapCapsuleAll(transform.position, collider.size, collider.direction, -transform.localEulerAngles.z, layer);
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
		if(go == null)
		{
			return;
		}
		if(reactive)
		{
			go.SetActive(false);
			go.SetActive(true);
		}
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		foreach (var item in particles)
		{
			item.Play();
		}
	}
	public static void stopAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		foreach (var item in particles)
		{
			item.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
	public static void restartAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		foreach (var item in particles)
		{
			item.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			item.Play();
		}
	}
	public static void pauseAllParticle(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		ParticleSystem[] particles = go.transform.GetComponentsInChildren<ParticleSystem>();
		foreach (var item in particles)
		{
			item.Pause();
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
		if(transform.parent == null)
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
	public static void setUIChildAlpha(GameObject go, float alpha)
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
			setUIChildAlpha(child, alpha);
		}
	}
	public static float getAnimationLength(Animator animator, string name)
	{
		foreach (var item in animator.runtimeAnimatorController.animationClips)
		{
			if (item.name == name)
			{
				return item.length;
			}
		}
		return 0.0f;
	}
	public static void applyAnchor(GameObject obj, bool force, GameLayout layout = null)
	{
		// 先更新自己
		obj.GetComponent<ScaleAnchor>()?.updateRect(force);
		obj.GetComponent<PaddingAnchor>()?.updateRect(force);
		if(layout != null)
		{
			txUIObject uiObj = layout.getUIObject(obj);
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
		PropertyInfo prop = gameView.GetType().GetProperty("currentGameViewSize", BindingFlags.NonPublic | BindingFlags.Instance);
		object gvsize = prop.GetValue(gameView, new object[0] { });
		Type gvSizeType = gvsize.GetType();
		int height = (int)gvSizeType.GetProperty("height", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { });
		int width = (int)gvSizeType.GetProperty("width", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { });
		return new Vector2(width, height);
#else
		logError("getGameViewSize can only call in editor!");
		return Vector2.zero;
#endif
	}
	public static Vector2 getRootSize(bool ngui, bool useGameViewSize = false)
	{
		Vector2 rootSize = Vector2.zero;
		if (useGameViewSize)
		{
			Vector2 gameViewSize = getGameViewSize();
			Camera camera = null;
			if (ngui)
			{
				camera = getGameObject(null, CommonDefine.NGUI_ROOT + "/" + CommonDefine.UI_CAMERA, true).GetComponent<Camera>();
			}
			else
			{
				camera = getGameObject(null, CommonDefine.UGUI_ROOT + "/" + CommonDefine.UI_CAMERA, true).GetComponent<Camera>();
			}
			rootSize = new Vector2(gameViewSize.y * camera.aspect, gameViewSize.y);
		}
		else
		{
			if (ngui)
			{
#if USE_NGUI
				UIRoot NGUIRoot = getNGUIRootComponent();
				if (NGUIRoot != null)
				{
					rootSize = new Vector2(NGUIRoot.activeHeight * getUICamera(true).aspect, NGUIRoot.activeHeight);
				}
#endif
			}
			else
			{
				Canvas UGUIRoot = getUGUIRootComponent();
				if (UGUIRoot != null)
				{
					Rect rect = UGUIRoot.gameObject.GetComponent<RectTransform>().rect;
					rootSize = new Vector2(rect.height * getUICamera(false).aspect, rect.height);
				}
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
	public static Vector2 adjustScreenScale(bool ngui, ASPECT_BASE aspectBase = ASPECT_BASE.AB_AUTO)
	{
		return adjustScreenScale(getScreenScale(getRootSize(ngui)), aspectBase);
	}
	public static Vector3 adjustScreenScale(Vector2 screenScale, ASPECT_BASE aspectBase = ASPECT_BASE.AB_AUTO)
	{
		Vector3 newScale = screenScale;
		if (aspectBase != ASPECT_BASE.AB_NONE)
		{
			if (aspectBase == ASPECT_BASE.AB_USE_HEIGHT_SCALE)
			{
				newScale.x = screenScale.y;
				newScale.y = screenScale.y;
			}
			else if (aspectBase == ASPECT_BASE.AB_USE_WIDTH_SCALE)
			{
				newScale.x = screenScale.x;
				newScale.y = screenScale.x;
			}
			else if (aspectBase == ASPECT_BASE.AB_AUTO)
			{
				newScale.x = getMin(screenScale.x, screenScale.y);
				newScale.y = getMin(screenScale.x, screenScale.y);
			}
			else if (aspectBase == ASPECT_BASE.AB_INVERSE_AUTO)
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
		if(render == null)
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
	public static void autoGrid(txUGUIObject root, float interval = 0.0f)
	{
		Vector2 rootSize = root.getWindowSize();
		float currentTop = rootSize.y * 0.5f;
		Transform transform = root.getTransform();
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
			float curHeight = childRect.rect.height;
			childRect.localPosition = new Vector3(0.0f, currentTop - curHeight * 0.5f);
			currentTop -= curHeight + interval;
		}
	}
}