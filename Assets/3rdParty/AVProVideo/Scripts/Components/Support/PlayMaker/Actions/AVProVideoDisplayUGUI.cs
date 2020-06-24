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
	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the MediaPlayer of a DisplayUGUI Component.")]
	public class AVProVideoDisplayUGUIGetMediaPlayer : ComponentAction<DisplayUGUI>
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(DisplayUGUI))]
		[Tooltip("The GameObject with a DisplayUGUI component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The MediaPlayer GameObject.")]
		[UIHint(UIHint.Variable)]
		public FsmGameObject mediaPlayerGameObject;

		[Tooltip("Event Sent if no MediaPlayer is referenced on the DisplayUGUI component")]
		public FsmEvent missingMediaPlayerEvent;


		public override void Reset()
		{
			gameObject = null;
			mediaPlayerGameObject = null;
			missingMediaPlayerEvent = null;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			Finish();
		}

		void ExecuteAction()
		{
			mediaPlayerGameObject.Value = this.cachedComponent._mediaPlayer ? this.cachedComponent._mediaPlayer.gameObject : null;

			if (missingMediaPlayerEvent != null && this.cachedComponent._mediaPlayer == null)
			{
				Fsm.Event(missingMediaPlayerEvent);
			}
		}
	}
}

#endif