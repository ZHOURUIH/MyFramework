#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	public class PostProcessBuild
	{
		[PostProcessBuild]
 		public static void OnPostProcessBuild(BuildTarget target, string path)
 		{
			bool x86 = false;
#if UNITY_2017_3_OR_NEWER
			// 64-bit only from here on out, woo hoo!!! \o/
#else
			x86 = target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXUniversal;
#endif
			if (x86)
			{
				string message = "AVPro Video doesn't support target StandaloneOSXIntel (32-bit), please use StandaloneOSXIntel64 (64-bit) or remove this PostProcessBuild script";
				Debug.LogError(message);
				EditorUtility.DisplayDialog("AVPro Video", message, "Ok");
			}
		}
	}
}
#endif