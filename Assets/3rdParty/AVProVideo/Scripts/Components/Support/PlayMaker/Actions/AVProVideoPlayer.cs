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
	[Tooltip("Closes Video of a MediaPlayer.")]
	public class AVProVideoPlayerCloseVideo : AVProVideoActionBase
    {
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

        public override void Reset()
        {
			gameObject = null;

        }

        public override void OnEnter()
        {
			if (this.UpdateCache (Fsm.GetOwnerDefaultTarget (gameObject)))
			{
				ExecuteAction ();
			}

          	Finish(); 
        }

		void ExecuteAction()
		{
			this.mediaPlayer.CloseVideo ();
		}
    }

	[ActionCategory("AVProVideo")]
	[Tooltip("Disable subtitles of a MediaPlayer.")]
	public class AVProVideoPlayerDisableSubtitles : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;

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
			this.mediaPlayer.DisableSubtitles();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Enable subtitles of a MediaPlayer.")]
	public class AVProVideoPlayerEnableSubtitles : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The file location")]
		[ObjectType(typeof(MediaPlayer.FileLocation))]
		public FsmEnum fileLocation;

		[RequiredField]
		[Tooltip("The file path, depending on the file Location")]
		public FsmString filePath;

		[ActionSection("Result")]

		[Tooltip("true if subtitle were enabled")]
		public FsmBool success;

		[Tooltip("event sent if subtitle enabling succeded")]
		public FsmEvent successEvent;

		[Tooltip("event sent if subtitle enabling failed")]
		public FsmEvent failureEvent;

		public override void Reset()
		{
			gameObject = null;
			fileLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
			filePath = null;
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
			bool ok = this.mediaPlayer.EnableSubtitles((MediaPlayer.FileLocation)fileLocation.Value, filePath.Value);

			success.Value = ok;

			Fsm.Event(ok ? successEvent : failureEvent);

		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Open a video at a location in a MediaPlayer.")]
	public class AVProVideoPlayerOpenVideoLocation : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The file location")]
		[ObjectType(typeof(MediaPlayer.FileLocation))]
		public FsmEnum fileLocation;

		[RequiredField]
		[Tooltip("The file path, depending on the file Location")]
		public FsmString filePath;

		[Tooltip("Auto play when video is loaded")]
		public FsmBool autoPlay;

		[ActionSection("Result")]

		[Tooltip("true if video is loading successfully")]
		public FsmBool success;

		[Tooltip("event sent if video is loading successfully")]
		public FsmEvent successEvent;

		[Tooltip("event sent if video loading failed")]
		public FsmEvent failureEvent;

		public override void Reset()
		{
			gameObject = null;
			fileLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
			filePath = null;
			autoPlay = true;
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
			bool ok = this.mediaPlayer.OpenVideoFromFile((MediaPlayer.FileLocation)fileLocation.Value, filePath.Value, autoPlay.Value);

			success.Value = ok;

			Fsm.Event(ok ? successEvent : failureEvent);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Pauses playback of a MediaPlayer.")]
	public class AVProVideoPlayerPause : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
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
			this.mediaPlayer.Pause();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Starts playback of a MediaPlayer.")]
	public class AVProVideoPlayerPlay : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
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
			this.mediaPlayer.Play();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Rewinds the video of a MediaPlayer.")]
	public class AVProVideoPlayerRewind : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The pause value when calling rewind. leave to none for default")]
		public FsmBool pause;

		public override void Reset()
		{
			gameObject = null;
			pause = new FsmBool() { UseVariable = true };
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
			this.mediaPlayer.Rewind(pause.Value);
		}
	}
	[ActionCategory("AVProVideo")]
	[Tooltip("Stops playback of a MediaPlayer.")]
	public class AVProVideoPlayerStop : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
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
			this.mediaPlayer.Stop();
		}
	}
}
#endif