using System.Collections.Generic;
using UnityEngine;
using static StringUtility;

// ���ڽ�������־��ӡ�������Ļ��
public class ConsoleToScreen : MonoBehaviour
{
    protected string mLogStr = "";					// ������ʾ������
	protected GUIStyle mStyle = new();		        // ��ʾ���ֵķ�����
	protected const int MAX_LINE = 5;				// ��������
    protected const int MAX_LINE_LENGTH = 100;		// ÿ�������ַ���
    protected const int FONT_SIZE = 13;				// �����С
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
