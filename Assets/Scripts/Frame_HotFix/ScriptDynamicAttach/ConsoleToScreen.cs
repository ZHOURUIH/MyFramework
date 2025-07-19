using System.Collections.Generic;
using UnityEngine;
using static StringUtility;

// 用于将错误日志打印到真机屏幕上
public class ConsoleToScreen : MonoBehaviour
{
    protected string mLogStr = "";					// 最终显示的文字
	protected GUIStyle mStyle = new();		        // 显示文字的风格对象
	protected const int MAX_LINE = 5;				// 最大的行数
    protected const int MAX_LINE_LENGTH = 100;		// 每行最大的字符数
    protected const int FONT_SIZE = 13;				// 字体大小
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLog(string logString, string stackTrace, LogType type)
    {
		if (type != LogType.Error || !mLogStr.isEmpty())
		{
			return;
		}

		List<string> lines = new();
		foreach (string line in splitLine(logString))
        {
            if (line.Length <= MAX_LINE_LENGTH)
            {
				lines.Add(line);
                continue;
            }
            int lineCount = line.Length / MAX_LINE_LENGTH + 1;
            for (int i = 0; i < lineCount; ++i)
            {
                if ((i + 1) * MAX_LINE_LENGTH <= line.Length)
                {
					lines.Add(line.substr(i * MAX_LINE_LENGTH, MAX_LINE_LENGTH));
                }
                else
                {
					lines.Add(line.substr(i * MAX_LINE_LENGTH, line.Length - i * MAX_LINE_LENGTH));
                }
				if (lines.Count >= MAX_LINE)
				{
					break;
				}
            }
        }
        mLogStr = string.Join("\n", lines);
	}
	protected void OnEnable() { Application.logMessageReceived += onLog; }
	protected void OnDisable() { Application.logMessageReceived -= onLog; }
	protected void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new(Screen.width / 1500.0f, Screen.height / 800.0f, 1.0f));
		mStyle.fontSize = FONT_SIZE;
		mStyle.normal.textColor = Color.red;
		GUI.Label(new(10, 10, 800, 370), mLogStr, mStyle);
    }
}
