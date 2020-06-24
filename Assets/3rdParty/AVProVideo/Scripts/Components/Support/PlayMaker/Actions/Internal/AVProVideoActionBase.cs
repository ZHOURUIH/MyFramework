//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

#if PLAYMAKER

using UnityEngine;

using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

using RenderHeads.Media.AVProVideo;

namespace RenderHeads.Media.AVProVideo.PlayMaker.Actions
{
	public class AVProVideoActionHeader
	{

	}

	/// <summary>
	/// AVProVideo action base.
	/// Gets and Caches MediaPlayer for perfs and code reuse
	/// </summary>
	public abstract class AVProVideoActionBase : FsmStateAction
	{
		protected GameObject cachedGameObject;

		protected MediaPlayer mediaPlayer;

		protected bool UpdateCache(GameObject go)
		{
			if (go == null)
			{
				return false;
			}

			if (mediaPlayer == null || cachedGameObject != go)
			{
				mediaPlayer = go.GetComponent<MediaPlayer>();
				cachedGameObject = go;
			}

			return mediaPlayer != null;
		}
	}
}
#endif