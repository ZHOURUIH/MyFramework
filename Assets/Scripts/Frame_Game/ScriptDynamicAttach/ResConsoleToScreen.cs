using System.Collections.Generic;
using UnityEngine;
using static StringUtility;

// ���ڽ�������־��ӡ�������Ļ��
public class ResConsoleToScreen : MonoBehaviour
{
    protected List<string> mLines = new();		    // ��ʾ����־
    protected string mLogStr = "";					// ������ʾ������
	protected bool mHasErrorShown;					// �Ƿ��Ѿ���ʾ��������־��,ֻ��ʾ��һ��������־,����Ĳ�����ʾ
	protected GUIStyle mStyle = new();		        // ��ʾ���ֵķ�����
	protected const int MAX_LINE = 5;				// ��������
    protected const int MAX_LINE_LENGTH = 100;		// ÿ�������ַ���
    protected const int FONT_SIZE = 13;				// �����С
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLog(string logString, string stackTrace, LogType type)
    {
		if (type != LogType.Error || mHasErrorShown)
		{
			return;
		}
		mHasErrorShown = true;

		mLines.Clear();
		foreach (string line in splitLine(logString))
        {
            if (line.Length <= MAX_LINE_LENGTH)
            {
                mLines.Add(line);
                continue;
            }
            int lineCount = line.Length / MAX_LINE_LENGTH + 1;
            for (int i = 0; i < lineCount; ++i)
            {
                if ((i + 1) * MAX_LINE_LENGTH <= line.Length)
                {
                    mLines.Add(line.substr(i * MAX_LINE_LENGTH, MAX_LINE_LENGTH));
                }
                else
                {
                    mLines.Add(line.substr(i * MAX_LINE_LENGTH, line.Length - i * MAX_LINE_LENGTH));
                }
				if (mLines.Count >= MAX_LINE)
				{
					break;
				}
            }
        }
        mLogStr = string.Join("\n", mLines);
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