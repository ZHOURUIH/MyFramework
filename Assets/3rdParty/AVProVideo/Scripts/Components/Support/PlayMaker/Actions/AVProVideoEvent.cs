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
	[Tooltip("Listen to the Closing Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventClosing: AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a Closing event")]
		public FsmEvent closingEvent;

        public override void Reset()
        {
			gameObject = null;
			closingEvent = null;
        }

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
			case MediaPlayerEvent.EventType.Closing:
				Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget (gameObject);
				Fsm.Event (closingEvent);
				Finish(); 
				break;
			
			}
		}

        public override void OnEnter()
        {
			if (this.UpdateCache (Fsm.GetOwnerDefaultTarget (gameObject)))
			{
				this.mediaPlayer.Events.AddListener (OnMediaPlayerEvent);
			} else
			{
				Finish(); 
			}
        }

		public override void OnExit()
		{
			if (this.mediaPlayer!=null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
    }

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the Error Event of a MediaPlayer and sends an event. Error Code is passed in the event data as an int. 100 = loadFailed 200 = decodeFailed")]
	public class AVProVideoEventError : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a Error event")]
		public FsmEvent errorEvent;

		public override void Reset()
		{
			gameObject = null;
			errorEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Error:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.EventData.IntData = (int)errorCode;
					Fsm.Event(errorEvent);
					Finish();
					break;
			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the FinishedPlaying Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventFinishedPlaying : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a FinishedPlaying event")]
		public FsmEvent finishedPlayingEvent;

		public override void Reset()
		{
			gameObject = null;
			finishedPlayingEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.FinishedPlaying:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(finishedPlayingEvent);
					Finish();
					break;
			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the FirstFrameReady Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventFirstFrameReady : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;
	
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a FirstFrameReady event")]
		public FsmEvent firstFrameReadyEvent;

		public override void Reset()
		{
			gameObject = null;
			firstFrameReadyEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.FirstFrameReady:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(firstFrameReadyEvent);
					Finish();
					break;

			}
		}

		public override void OnEnter()
		{

			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the MetaDataReady Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventMetaDataReady : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a metaDataReady event")]
		public FsmEvent metaDataReadyEvent;

		public override void Reset()
		{
			gameObject = null;
			metaDataReadyEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.MetaDataReady:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(metaDataReadyEvent);
					Finish();
					break;

			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the ReadytoPlay Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventReadyToPlay : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a ReadyToPlay event")]
		public FsmEvent readyToPlayEvent;

		public override void Reset()
		{
			gameObject = null;
			readyToPlayEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.ReadyToPlay:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(readyToPlayEvent);
					Finish();
					break;

			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the Stalled Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventStalled : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a Stalled event")]
		public FsmEvent stalledEvent;

		public override void Reset()
		{
			gameObject = null;
			stalledEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Stalled:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(stalledEvent);
					Finish();
					break;

			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the ReadytoPlay Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventStarted : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a Started event")]
		public FsmEvent startedEvent;

		public override void Reset()
		{
			gameObject = null;
			startedEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Started:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(startedEvent);
					Finish();
					break;
			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the SubtitleChange Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventSubtitleChange : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a SubtitleChange event")]
		public FsmEvent subtitleChangeEvent;

		public override void Reset()
		{
			gameObject = null;
			subtitleChangeEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.SubtitleChange:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(subtitleChangeEvent);
					Finish();
					break;
			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Listen to the Unstalled Event of a MediaPlayer and sends an event.")]
	public class AVProVideoEventUnstalled : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event fired when MediaPlayer sends a Unstalled event")]
		public FsmEvent unstalledEvent;

		public override void Reset()
		{
			gameObject = null;
			unstalledEvent = null;
		}

		// Callback function to handle events
		public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Unstalled:
					Fsm.EventData.GameObjectData = Fsm.GetOwnerDefaultTarget(gameObject);
					Fsm.Event(unstalledEvent);
					Finish();
					break;
			}
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				this.mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
			else
			{
				Finish();
			}
		}

		public override void OnExit()
		{
			if (this.mediaPlayer != null)
			{
				this.mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
			}
		}
	}
}
#endif