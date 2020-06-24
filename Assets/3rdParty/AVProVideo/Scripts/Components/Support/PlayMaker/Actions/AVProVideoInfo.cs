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
	[Tooltip("Gets Whether this MediaPlayer instance supports linear color space from media info a MediaPlayer. If it doesn't then a correction may have to be made in the shader")]
	public class AVProVideoInfoDoesSupportsLinearColorSpace : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")] 
		[UIHint(UIHint.Variable)]
		[Tooltip("True if player supports linear color space")]
		public FsmBool doesSupportsLCS;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if player supports linear color space")]
		public FsmEvent doesSupportsLCSEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if player does not supports linear color space")]
		public FsmEvent doesNotSupportsLCSEvent;

		[Tooltip("Event Sent if MediaPlayer media media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

        public override void Reset()
        {
			gameObject = null;
			doesSupportsLCS = null;
			doesSupportsLCSEvent = null;
			doesNotSupportsLCSEvent = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event (missingMediaInfoEvent);
				return;
			}

			if (this.mediaPlayer.Info.PlayerSupportsLinearColorSpace())
			{
				doesSupportsLCS.Value = true;
				Fsm.Event (doesSupportsLCSEvent);
				
			} else
			{
				doesSupportsLCS.Value = false;
				Fsm.Event (doesNotSupportsLCSEvent);
			}
		}
    }

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the number of audio tracks from media info a MediaPlayer.")]
	public class AVProVideoInfoGetAudioTrackCount : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The number of audio tracks")]
		public FsmInt trackCount;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackCount = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackCount.Value = this.mediaPlayer.Info.GetAudioTrackCount();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the current audio bitrate from media info a MediaPlayer.")]
	public class AVProVideoInfoGetCurrentAudioTrackBitrate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current audio bitrate")]
		public FsmInt trackBitrate;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackBitrate = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackBitrate.Value = this.mediaPlayer.Info.GetCurrentAudioTrackBitrate();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the current audio track identification from media info a MediaPlayer.")]
	public class AVProVideoInfoGetCurrentAudioTrackId : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current audio track identification")]
		public FsmString trackId;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackId = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackId.Value = this.mediaPlayer.Info.GetCurrentAudioTrackId();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the current video bitrate from media info a MediaPlayer.")]
	public class AVProVideoInfoGetCurrentVideoTrackBitrate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current video bitrate")]
		public FsmInt trackBitrate;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackBitrate = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackBitrate.Value = this.mediaPlayer.Info.GetCurrentVideoTrackBitrate();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the current video track identification from media info a MediaPlayer.")]
	public class AVProVideoInfoGetCurrentVideoTrackId : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current video track identification")]
		public FsmString trackId;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackId = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackId.Value = this.mediaPlayer.Info.GetCurrentVideoTrackId();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets video duration from media info a MediaPlayer.")]
	public class AVProVideoInfoGetDurationMs : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the video duration in ms.")]
		public FsmFloat duration;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			duration = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			duration.Value = this.mediaPlayer.Info.GetDurationMs();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the description of which playback path is used internally from media info a MediaPlayer.")]
	public class AVProVideoInfoAVProInfoGetPlayerDescription : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The description of the playback")]
		public FsmString description;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			description = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			description.Value = this.mediaPlayer.Info.GetPlayerDescription();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the current achieved display rate in frames per second from media info a MediaPlayer.")]
	public class AVProVideoInfoGetVideoDisplayRate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The achieved framerate of the video")]
		public FsmFloat framerate;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			framerate = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			framerate.Value = this.mediaPlayer.Info.GetVideoDisplayRate();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets video frame rate from media info a MediaPlayer.")]
	public class AVProVideoInfoGetVideoFrameRate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The framerate of the video")]
		public FsmFloat framerate;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			framerate = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			framerate.Value = this.mediaPlayer.Info.GetVideoFrameRate();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets video size ( width and height ) from media info a MediaPlayer.")]
	public class AVProVideoInfoGetVideoSize : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The width of the video")]
		public FsmFloat width;

		[UIHint(UIHint.Variable)]
		[Tooltip("The height of the video")]
		public FsmFloat height;

		[UIHint(UIHint.Variable)]
		[Tooltip("The width and height of the video")]
		public FsmVector2 size;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			width = null;
			height = null;
			size = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			if (!width.IsNone) width.Value = this.mediaPlayer.Info.GetVideoWidth();
			if (!height.IsNone) height.Value = this.mediaPlayer.Info.GetVideoHeight();

			if (!size.IsNone) size.Value = new Vector2(this.mediaPlayer.Info.GetVideoWidth(), this.mediaPlayer.Info.GetVideoHeight());
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the number of video tracks from media info a MediaPlayer.")]
	public class AVProVideoInfoGetVideoTrackCount : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The number of video tracks")]
		public FsmInt trackCount;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			trackCount = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			trackCount.Value = this.mediaPlayer.Info.GetVideoTrackCount();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets hasAudio from media info a MediaPlayer.")]
	public class AVProVideoInfoHasAudio : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the hasAudio.")]
		public FsmBool hasAudio;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media has audio")]
		public FsmEvent hasAudioEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media doesn't have audio")]
		public FsmEvent hasNoAudioEvent;

		[Tooltip("Event Sent if MediaPlayer media media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			hasAudio = null;
			hasAudioEvent = null;
			hasNoAudioEvent = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			if (this.mediaPlayer.Info.HasAudio())
			{
				hasAudio.Value = true;
				Fsm.Event(hasNoAudioEvent);
			}
			else
			{
				hasAudio.Value = false;
				Fsm.Event(hasNoAudioEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets hasVideo from media info a MediaPlayer.")]
	public class AVProVideoInfoHasVideo : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the hasVideo.")]
		public FsmBool hasVideo;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media has video")]
		public FsmEvent hasVideoEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media doesn't have video")]
		public FsmEvent hasNoVideoEvent;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			hasVideo = null;
			hasVideoEvent = null;
			hasNoVideoEvent = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			if (this.mediaPlayer.Info.HasVideo())
			{
				hasVideo.Value = true;
				Fsm.Event(hasNoVideoEvent);
			}
			else
			{
				hasVideo.Value = false;
				Fsm.Event(hasNoVideoEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Checks if the playback is in a stalled state from media info a MediaPlayer.")]
	public class AVProVideoInfoIsPlaybackStalled : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("True if the playback is in a stalled state")]
		public FsmBool isStalled;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if  the playback is in a stalled state")]
		public FsmEvent isStalledEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if  the playback is not in a stalled state")]
		public FsmEvent isNotStalledEvent;

		[Tooltip("Event Sent if MediaPlayer media info is missing (null)")]
		public FsmEvent missingMediaInfoEvent;

		public override void Reset()
		{
			gameObject = null;
			isStalled = null;
			isStalledEvent = null;
			isNotStalledEvent = null;
			missingMediaInfoEvent = null;
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
			if (this.mediaPlayer.Info == null)
			{
				Fsm.Event(missingMediaInfoEvent);
				return;
			}

			if (this.mediaPlayer.Info.IsPlaybackStalled())
			{
				isStalled.Value = true;
				Fsm.Event(isStalledEvent);
			}
			else
			{
				isStalled.Value = false;
				Fsm.Event(isNotStalledEvent);
			}
		}
	}
}
#endif