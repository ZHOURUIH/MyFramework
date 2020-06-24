// AVProVideo
// (C) 2017-2018 RenderHeads Ltd
//
// AVProVideo iOS plugin bootstrap.
//
// Unity removed UnityRegisterRenderingPlugin with Unity5.5 and replaced
// it with UnityRegisterRenderingPluginV5 (first available in Unity5.4).
// We patch through to UnityRegisterRenderingPlugin for older versions
// of Unity.
//
// Unity removed UnityEndCurrentMTLCommandEncoder with Unity2018 so we
// provide a stub here for the plugin to link against.

#if !defined(UNITY_5_4_0) || UNITY_VERSION < 540

extern void AVPPluginUnityRegisterRenderingPlugin();

void UnityRegisterRenderingPluginV5(void (*unused)(void *), void (*unused2)())
{
	AVPPluginUnityRegisterRenderingPlugin();
}

#endif

#if UNITY_VERSION >= UNITY_2018_1_0

void UnityEndCurrentMTLCommandEncoder()
{
	// Empty
}

#endif
