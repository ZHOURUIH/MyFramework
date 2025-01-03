using System;

// 场景注册信息
public class SceneRegisteInfo
{
	public string mName;					// 场景名
	public string mScenePath;				// 场景路径
	public Type mSceneType;					// 场景逻辑类的类型
	public SceneScriptCallback mCallback;	// 用于给场景脚本对象赋值
}