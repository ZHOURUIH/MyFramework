using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Hide the SDK's introductory notice in Play Mode without changing any Mock module.
[InitializeOnLoad]
public static class TTSDKEditorNotice
{
    private static Type mMockUIType;
    private static FieldInfo mBannerTimer;
    private static UnityEngine.Object mHiddenNotice;

    static TTSDKEditorNotice()
    {
        EditorApplication.update += hideNotice;
    }

    private static void hideNotice()
    {
#if BYTE_DANCE
        if (!EditorApplication.isPlaying)
        {
            mHiddenNotice = null;
            return;
        }
        if (mHiddenNotice != null)
        {
            return;
        }
        mMockUIType ??= Type.GetType("TTSDK.MockUIUtil, ttsdk", false);
        mBannerTimer ??= mMockUIType?.GetField("m_ShowBannerTipsTime", BindingFlags.Instance | BindingFlags.NonPublic);
        if (mBannerTimer == null || mBannerTimer.FieldType != typeof(float))
        {
            return;
        }
        foreach (var notice in UnityEngine.Object.FindObjectsOfType(mMockUIType, true))
        {
            mBannerTimer.SetValue(notice, 0f);
            mHiddenNotice = notice;
        }
#endif
    }
}
