using UnityEngine;
using System;
using UnityEngine.Profiling;

public class UnityProfiler
{
	static public void BeginSample(string name)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.BeginSample(name);
#endif
	}
	static public void EndSample()
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.EndSample();
#endif
	}
}