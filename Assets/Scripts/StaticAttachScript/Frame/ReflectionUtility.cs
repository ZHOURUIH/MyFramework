using System;
using System.Collections.Generic;
using UnityEngine;

// 用于静态挂接到物体上的脚本使用的,动态挂接的脚本可以直接调用需要的函数
public class ReflectionUtility
{
	public static string UTILITY_STRING = "StringUtility";
	public static string UTILITY_UNITY = "UnityUtility";
	public static string UTILITY_MATH = "MathUtility";
	public static string UTILITY_WIDGET = "WidgetUtility";
#if MAKE_CS_DLL
	public static bool getStaticBool(string className, string paramName)
	{
		Type type = getTypeInGame(className);
		if (type != null)
		{
			FieldInfo fieldInfo = type.GetField(paramName, BindingFlags.Public | BindingFlags.Static);
			if (fieldInfo != null)
			{
				return (bool)fieldInfo.GetValue(null);
			}
			else
			{
				UnityEngine.Debug.LogError("there is no param : " + paramName + " in class : " + className);
			}
		}
		else
		{
			UnityEngine.Debug.LogError("there is no className : " + className);
		}
		return false;
	}
	public static int getStaticInt(string className, string paramName)
	{
		Type type = getTypeInGame(className);
		if (type != null)
		{
			FieldInfo fieldInfo = type.GetField(paramName, BindingFlags.Public | BindingFlags.Static);
			if (fieldInfo != null)
			{
				return (int)fieldInfo.GetValue(null);
			}
			else
			{
				UnityEngine.Debug.LogError("there is no param : " + paramName + " in class : " + className);
			}
		}
		else
		{
			UnityEngine.Debug.LogError("there is no className : " + className);
		}
		return 0;
	}
	public static object callStatic(string className, string methodName, object[] param)
	{
		Type type = getTypeInGame(className);
		if (type != null)
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
			if (method != null)
			{
				return method.Invoke(null, param);
			}
			else
			{
				UnityEngine.Debug.LogError("there is no method : " + methodName + " in class : " + className);
			}
		}
		else
		{
			UnityEngine.Debug.LogError("there is no className : " + className);
		}
		return null;
	}
	public static void invokeGameMethod(Type type, string methodName, BindingFlags flag, object instance)
	{
		if (type != null)
		{
			MethodInfo method = type.GetMethod(methodName, flag);
			if (method != null)
			{
				method.Invoke(instance, null);
			}
			else
			{
				UnityEngine.Debug.LogError("there is no method : " + methodName + " in class : " + type.ToString());
			}
		}
	}
	public static Type getTypeInGame(string typeName)
	{
		Type type = null;
		if (GameEntryPoint.mGameAssembly != null)
		{
			type = GameEntryPoint.mGameAssembly.GetType(typeName);
		}
		return type;
	}
	public static int getEnumValue(string enumType, string enumName)
	{
		Type logLevelType = getTypeInGame(enumType);
		FieldInfo[] enumInfos = logLevelType.GetFields();
		foreach (var item in enumInfos)
		{
			if (item.Name == enumName)
			{
				return (int)item.GetValue(null);
			}
		}
		return 0;
	}
#endif
	public static int getStandardWidth()
	{
#if MAKE_CS_DLL
		return getStaticInt("GameDefine", "STANDARD_WIDTH");
#else
		return GameDefine.STANDARD_WIDTH;
#endif
	}
	public static int getStandardHeight()
	{
#if MAKE_CS_DLL
		return getStaticInt("GameDefine", "STANDARD_HEIGHT");
#else
		return GameDefine.STANDARD_HEIGHT;
#endif
	}
	public static string getNGUIRootName()
	{
#if MAKE_CS_DLL
		return getStaticString("CommonDefine", "NGUI_ROOT");
#else
		return CommonDefine.NGUI_ROOT;
#endif
	}
	public static string getUGUIRootName()
	{
#if MAKE_CS_DLL
		return getStaticString("CommonDefine", "UGUI_ROOT");
#else
		return CommonDefine.UGUI_ROOT;
#endif
	}
	public static string getUICameraName()
	{
#if MAKE_CS_DLL
		return getStaticString("CommonDefine", "UI_CAMERA");
#else
		return CommonDefine.UI_CAMERA;
#endif
	}
	// StringUtility
	//---------------------------------------------------------------------------------------------------------
	public static string vector3ToString(Vector3 value, int precision = 4)
	{
#if MAKE_CS_DLL
		return (string)callStatic(UTILITY_STRING, "vector3ToString", new object[] { value, precision });
#else
		return StringUtility.vector3ToString(value, precision);
#endif
	}
	// MathUtility
	//---------------------------------------------------------------------------------------------------------
	public static int ceil(float value)
	{
#if MAKE_CS_DLL
		return (int)callStatic(UTILITY_MATH, "ceil", new object[] { value});
#else
		return MathUtility.ceil(value);
#endif
	}
	public static float getLength(Vector3 vec)
	{
#if MAKE_CS_DLL
		return (float)callStatic(UTILITY_MATH, "getLength", new object[] { vec });
#else
		return MathUtility.getLength(vec);
#endif
	}
	public static bool isVectorZero(Vector3 vec, float precision = 0.0001f)
	{
#if MAKE_CS_DLL
		return (float)callStatic(UTILITY_MATH, "isVectorZero", new object[] { vec, precision });
#else
		return MathUtility.isVectorZero(vec, precision);
#endif
	}
	public static Vector3 multiVector3(Vector3 v1, Vector3 v2)
	{
#if MAKE_CS_DLL
		return (Vector3)callStatic(UTILITY_MATH, "multiVector3", new object[] { v1, v2 });
#else
		return MathUtility.multiVector3(v1, v2);
#endif
	}
	public static Vector3 round(Vector3 value)
	{
#if MAKE_CS_DLL
		return (Vector3)callStatic(UTILITY_MATH, "round", new object[] { value });
#else
		return MathUtility.round(value);
#endif
	}
	// UnityUtility
	//---------------------------------------------------------------------------------------------------------------------------------------
	public static void logError(string info, bool isMainThread = true)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_UNITY, "logError", new object[] { info, isMainThread });
#else
		UnityUtility.logError(info, isMainThread);
#endif
	}
	public static void logWarning(string info)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_UNITY, "logWarning", new object[] { info });
#else
		UnityUtility.logWarning(info);
#endif
	}
	public static void logInfo(string info, int level)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_UNITY, "logInfo", new object[] { info, level });
#else
		UnityUtility.logInfo(info, (LOG_LEVEL)level);
#endif
	}
#if USE_NGUI
	public static UIRoot getNGUIRootComponent()
	{
#if MAKE_CS_DLL
		return (UIRoot)callStatic(UTILITY_UNITY, "getNGUIRootComponent", null);
#else
		return UnityUtility.getNGUIRootComponent();
#endif
	}
#endif
	public static Canvas getUGUIRootComponent()
	{
#if MAKE_CS_DLL
		return (UIRoot)callStatic(UTILITY_UNITY, "getUGUIRootComponent", null);
#else
		return UnityUtility.getUGUIRootComponent();
#endif
	}
	public static Camera getNGUICamera()
	{
#if MAKE_CS_DLL
		return (Camera)callStatic(UTILITY_UNITY, "getNGUICamera", null);
#else
		return UnityUtility.getUICamera(true);
#endif
	}
	public static Camera getUGUICamera()
	{
#if MAKE_CS_DLL
		return (Camera)callStatic(UTILITY_UNITY, "getUICamera", false);
#else
		return UnityUtility.getUICamera(false);
#endif
	}
	public static GameObject getGameObject(GameObject parent, string name, bool errorIfNull = false)
	{
#if MAKE_CS_DLL
		return (GameObject)callStatic(UTILITY_UNITY, "getGameObject", new object[] { parent, name, errorIfNull });
#else
		return UnityUtility.getGameObject(parent, name, errorIfNull);
#endif
	}
	public static Vector2 getGameViewSize()
	{
#if MAKE_CS_DLL
		return (Vector2)callStatic(UTILITY_UNITY, "getGameViewSize", new object[]{});
#else
		return UnityUtility.getGameViewSize();
#endif
	}
	public static void destroyGameObject(UnityEngine.Object obj, bool immediately = false, bool allowDestroyAssets = false)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_UNITY, "destroyGameObject", new object[] { obj, immediately, allowDestroyAssets });
#else
		UnityUtility.destroyGameObject(obj, immediately, allowDestroyAssets);
#endif
	}
	public static void applyAnchor(GameObject go, bool force, GameLayout layout = null)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_UNITY, "applyAnchor", new object[] { go, force, layout });
#else
		UnityUtility.applyAnchor(go, force, layout);
#endif
	}
	public static Vector2 getRootSize(bool ngui, bool useGameViewSize = false)
	{
#if MAKE_CS_DLL
		return (Vector2)callStatic(UTILITY_UNITY, "getRootSize", new object[] { ngui, useGameViewSize });
#else
		return UnityUtility.getRootSize(ngui, useGameViewSize);
#endif
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
#if MAKE_CS_DLL
		return (Vector2)callStatic(UTILITY_UNITY, "getScreenScale", new object[] { rootSize });
#else
		return UnityUtility.getScreenScale(rootSize);
#endif
	}
	public static Vector3 adjustScreenScale(Vector2 screenScale, ASPECT_BASE aspectBase)
	{
#if MAKE_CS_DLL
		return (Vector3)callStatic(UTILITY_UNITY, "adjustScreenScale", new object[] { screenScale, aspectBase });
#else
		return UnityUtility.adjustScreenScale(screenScale, aspectBase);
#endif
	}
	public static Vector3 localToWorld(Transform transform, Vector3 local)
	{
#if MAKE_CS_DLL
		return (Vector3)callStatic(UTILITY_UNITY, "localToWorld", new object[] { transform, local });
#else
		return UnityUtility.localToWorld(transform, local);
#endif
	}
	public static Vector3 worldToLocal(Transform transform, Vector3 world)
	{
#if MAKE_CS_DLL
		return (Vector3)callStatic(UTILITY_UNITY, "worldToLocal", new object[] { transform, world });
#else
		return UnityUtility.worldToLocal(transform, world);
#endif
	}
	// WidgetUtility
	//----------------------------------------------------------------------------------------------------------------------------------------
	public static bool isNGUI(GameObject go)
	{
#if MAKE_CS_DLL
		return (bool)callStatic(UTILITY_WIDGET, "isNGUI", new object[] { go });
#else
		return WidgetUtility.isNGUI(go);
#endif
	}
	// 父节点在父节点坐标系下的各条边
	public static void getParentSides(GameObject parent, Vector3[] sides)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_WIDGET, "getParentSides", new object[] { parent, sides});
#else
		WidgetUtility.getParentSides(parent, sides);
#endif
	}
#if USE_NGUI
	public static void setNGUIWidgetSize(UIWidget widget, Vector2 size)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_WIDGET, "setNGUIWidgetSize", new object[] { widget, size });
#else
		WidgetUtility.setNGUIWidgetSize(widget, size);
#endif
	}
	public static Vector2 getNGUIRectSize(UIRect rect)
	{
#if MAKE_CS_DLL
		return (Vector2)callStatic(UTILITY_WIDGET, "getNGUIRectSize", new object[] { rect });
#else
		return WidgetUtility.getNGUIRectSize(rect);
#endif
	}
	public static UIRect findNGUIParentRect(GameObject obj)
	{
#if MAKE_CS_DLL
		return (UIRect)callStatic(UTILITY_WIDGET, "findNGUIParentRect", new object[] { obj });
#else
		return WidgetUtility.findNGUIParentRect(obj);
#endif
	}
#endif
	public static void setUGUIRectSize(RectTransform rectTransform, Vector2 size, bool adjustFont)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_WIDGET, "setUGUIRectSize", new object[] { rectTransform, size, adjustFont });
#else
		WidgetUtility.setUGUIRectSize(rectTransform, size, adjustFont);
#endif
	}
	public static Vector2 getUGUIRectSize(RectTransform rect)
	{
#if MAKE_CS_DLL
		return (Vector2)callStatic(UTILITY_WIDGET, "getUGUIRectSize", new object[] { rect });
#else
		return WidgetUtility.getUGUIRectSize(rect);
#endif
	}
	public static void cornerToSide(Vector3[] corners, Vector3[] sides)
	{
#if MAKE_CS_DLL
		callStatic(UTILITY_WIDGET, "cornerToSide", new object[] { corners, sides });
#else
		WidgetUtility.cornerToSide(corners, sides);
#endif
	}
#if MAKE_CS_DLL
	public static void checkUnityDefine()
	{
		Type type = GameEntryPoint.mGameAssembly.GetType("UnityDefineCheck");
#if UNITY_EDITOR
		check(type, "_UNITY_EDITOR", true);
#else
		check(type, "_UNITY_EDITOR", false);
#endif

#if UNITY_ANDROID
		check(type, "_UNITY_ANDROID", true);
#else
		check(type, "_UNITY_ANDROID", false);
#endif

#if UNITY_STANDALONE_WIN
		check(type, "_UNITY_STANDALONE_WIN", true);
#else
		check(type, "_UNITY_STANDALONE_WIN", false);
#endif

#if UNITY_STANDALONE_LINUX
		check(type, "_UNITY_STANDALONE_LINUX", true);
#else
		check(type, "_UNITY_STANDALONE_LINUX", false);
#endif

#if UNITY_IOS
		check(type, "_UNITY_IOS", true);
#else
		check(type, "_UNITY_IOS", false);
#endif

#if UNITY_2018
		check(type, "_UNITY_2018", true);
#else
		check(type, "_UNITY_2018", false);
#endif
	}
	//--------------------------------------------------------------------------------------------------
	protected static void check(Type type, string name, bool curValue)
	{
		FieldInfo info = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
		if ((bool)info.GetValue(null) != curValue)
		{
			UnityEngine.Debug.LogError(name + " define error!");
		}
	}
#endif
}