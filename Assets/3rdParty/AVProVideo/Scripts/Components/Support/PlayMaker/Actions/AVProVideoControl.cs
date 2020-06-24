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
	[Tooltip("Get the CanPlay value of a MediaPlayer media controller.")]
	public class AVProVideoControlCanPlay : AVProVideoActionBase
    {
		public AVProVideoActionHeader headerImage;
		
		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")] 
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the CanPlay.")]
		public FsmBool canPlay;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is looping")]
		public FsmEvent canPlayEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not looping")]
		public FsmEvent canNotPlayEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _canPlay = -1;

        public override void Reset()
        {
			gameObject = null;
			canPlay = null;
			canPlayEvent = null;
			canNotPlayEvent = null;
			missingControlEvent = null;
			everyframe = false;
        }

        public override void OnEnter()
        {
			if (this.UpdateCache (Fsm.GetOwnerDefaultTarget (gameObject)))
			{
				ExecuteAction ();
			}

			if (!everyframe)
			{
				Finish (); 
			}
        }
		public override void OnUpdate()
		{
			if (this.UpdateCache (Fsm.GetOwnerDefaultTarget (gameObject)))
			{
				ExecuteAction ();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event (missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.CanPlay ())
			{
				canPlay.Value = true;
				if (_canPlay!=1)
				{
					Fsm.Event (canPlayEvent);
				}
				_canPlay = 1;
			} else
			{
				canPlay.Value = false;
				if (_canPlay != 0)
				{
					Fsm.Event (canNotPlayEvent);
				}
				_canPlay = 0;
			}
		}
    }

	[ActionCategory("AVProVideo")]
	[Tooltip("Close the video of a MediaPlayer.")]
	public class AVProVideoControlCloseVideo : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer media controller.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;
			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.CloseVideo();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the Buffered Time Range of a MediaPlayer media controller.")]
	public class AVProVideoControlGetBufferedTimeRange : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The Buffered index ( from AVProControlGetBufferedTimeRangeCount )")]
		public FsmInt index;

		[ActionSection("Result")]

		[UIHint(UIHint.Variable)]
		[Tooltip("The start time of that buffer index")]
		public FsmFloat startTime;

		[UIHint(UIHint.Variable)]
		[Tooltip("The end time of that buffer index")]
		public FsmFloat endtime;

		[UIHint(UIHint.Variable)]
		[Tooltip("buffer flag")]
		public FsmBool isBuffered;

		[Tooltip("Event Sent if the result of this buffer range is true")]
		public FsmEvent isBufferedEvent;

		[Tooltip("Event Sent if the result of this buffer range is false")]
		public FsmEvent isNotBufferedEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		float _startTime;
		float _endTime;
		bool _result;


		public override void Reset()
		{
			gameObject = null;

			index = null;

			startTime = null;
			endtime = null;
			isBuffered = null;
			isBufferedEvent = null;
			isNotBufferedEvent = null;

			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			_result = this.mediaPlayer.Control.GetBufferedTimeRange(index.Value, ref _startTime, ref _endTime);
			startTime.Value = _startTime;
			endtime.Value = _endTime;

			if (_result)
			{
				isBuffered.Value = true;
				Fsm.Event(isBufferedEvent);
			}
			else
			{
				isBuffered.Value = false;
				Fsm.Event(isNotBufferedEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the Buffered Time Range progress of a MediaPlayer media controller.")]
	public class AVProVideoControlGetBufferedTimeRangeCount : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The Buffered Time Range count")]
		public FsmInt bufferedTimeRange;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			bufferedTimeRange = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}
		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			bufferedTimeRange.Value = this.mediaPlayer.Control.GetBufferedTimeRangeCount();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the buffering progress of a MediaPlayer media controller.")]
	public class AVProVideoControlGetBufferingProgress : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The buffering progress")]
		public FsmFloat bufferingProgress;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			bufferingProgress = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}
		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			bufferingProgress.Value = this.mediaPlayer.Control.GetBufferingProgress();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the current audio track id of a MediaPlayer media controller.")]
	public class AVProVideoControlGetCurrentAudioTrack : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current audio track Id")]
		public FsmInt trackId;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			trackId = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}
		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			trackId.Value = this.mediaPlayer.Control.GetCurrentAudioTrack();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the current playback time in ms of a MediaPlayer media controller.")]
	public class AVProVideoControlGetCurrentTime : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current time in ms")]
		public FsmFloat currentTime;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			currentTime = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			currentTime.Value = this.mediaPlayer.Control.GetCurrentTimeMs();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the current video track id of a MediaPlayer media controller.")]
	public class AVProVideoControlGetCurrentVideoTrack : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The current video track Id")]
		public FsmInt trackId;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			trackId = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			trackId.Value = this.mediaPlayer.Control.GetCurrentVideoTrack();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the last error of a MediaPlayer controller.")]
	public class AVProVideoControlGetLastError : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The last error code as int")]
		public FsmInt lastErrorAsInt;

		[UIHint(UIHint.Variable)]
		[Tooltip("The last error code")]
		[ObjectType(typeof(ErrorCode))]
		public FsmEnum lastError;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			lastError = ErrorCode.None;
			lastErrorAsInt = null;

			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}
		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}
			lastError.Value = this.mediaPlayer.Control.GetLastError();
			lastErrorAsInt.Value = (int)this.mediaPlayer.Control.GetLastError();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the playback rate of a MediaPlayer media controller.")]
	public class AVProVideoControlGetPlayBackRate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The playback rate")]
		public FsmFloat playbackRate;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			playbackRate = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			playbackRate.Value = this.mediaPlayer.Control.GetPlaybackRate();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Gets the volume value of a MediaPlayer media controller.")]
	public class AVProVideoControlGetVolume : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the volume.")]
		public FsmFloat volume;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			volume = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			volume.Value = this.mediaPlayer.Control.GetVolume();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the hasMetada value of a MediaPlayer media controller.")]
	public class AVProVideoControlHasMetaData : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Flag wether Media has metadata or not.")]
		public FsmBool hasMetaData;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media has metaData")]
		public FsmEvent hasMetadataEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media does not have metaData")]
		public FsmEvent hasNotMetadataEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _hasMetaData = -1;

		public override void Reset()
		{
			gameObject = null;
			hasMetaData = null;
			hasMetadataEvent = null;
			hasNotMetadataEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.HasMetaData())
			{
				hasMetaData.Value = true;
				if (_hasMetaData != 1)
				{
					Fsm.Event(hasMetadataEvent);
				}
				_hasMetaData = 1;
			}
			else
			{
				hasMetaData.Value = false;
				if (_hasMetaData != 0)
				{
					Fsm.Event(hasNotMetadataEvent);
				}
				_hasMetaData = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsBuffering value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsBuffering : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the IsBuffering.")]
		public FsmBool isBuffering;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is buffering")]
		public FsmEvent isBufferingEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not buffering")]
		public FsmEvent isNotBufferingEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isBuffering = -1;

		public override void Reset()
		{
			gameObject = null;
			isBuffering = null;
			isBufferingEvent = null;
			isNotBufferingEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsBuffering())
			{
				isBuffering.Value = true;
				if (_isBuffering != 1)
				{
					Fsm.Event(isBufferingEvent);
				}
				_isBuffering = 1;
			}
			else
			{
				isBuffering.Value = false;
				if (_isBuffering != 0)
				{
					Fsm.Event(isNotBufferingEvent);
				}
				_isBuffering = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsFinished value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsFinished : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the IsFinished.")]
		public FsmBool isFinished;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is finished")]
		public FsmEvent isFinishedEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not finished")]
		public FsmEvent isNotFinishedEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isFinished = -1;

		public override void Reset()
		{
			gameObject = null;
			isFinished = null;
			isFinishedEvent = null;
			isNotFinishedEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsFinished())
			{
				isFinished.Value = true;
				if (_isFinished != 1)
				{
					Fsm.Event(isFinishedEvent);
				}
				_isFinished = 1;
			}
			else
			{
				isFinished.Value = false;
				if (_isFinished != 0)
				{
					Fsm.Event(isNotFinishedEvent);
				}
				_isFinished = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsLooping value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsLooping : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the looping.")]
		public FsmBool isLooping;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is looping")]
		public FsmEvent isLoopingEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not looping")]
		public FsmEvent isNotLoopingEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isLooping = -1;

		public override void Reset()
		{
			gameObject = null;
			isLooping = null;
			isLoopingEvent = null;
			isNotLoopingEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsLooping())
			{
				isLooping.Value = true;
				if (_isLooping != 1)
				{
					Fsm.Event(isLoopingEvent);
				}
				_isLooping = 1;
			}
			else
			{
				isLooping.Value = false;
				if (_isLooping != 0)
				{
					Fsm.Event(isNotLoopingEvent);
				}
				_isLooping = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsMuted value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsMuted : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the isMuted.")]
		public FsmBool isMuted;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is Muted")]
		public FsmEvent isMutedEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not Muted")]
		public FsmEvent isNotMutedEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isMuted = -1;

		public override void Reset()
		{
			gameObject = null;
			isMuted = null;
			isMutedEvent = null;
			isNotMutedEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsMuted())
			{
				isMuted.Value = true;
				if (_isMuted != 1)
				{
					Fsm.Event(isMutedEvent);
				}
				_isMuted = 1;
			}
			else
			{
				isMuted.Value = false;
				if (_isMuted != 0)
				{
					Fsm.Event(isNotMutedEvent);
				}
				_isMuted = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsPaused value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsPaused : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the isPaused.")]
		public FsmBool isPaused;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is paused")]
		public FsmEvent isPausedEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not paused")]
		public FsmEvent isNotPausedEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isPaused = -1;

		public override void Reset()
		{
			gameObject = null;
			isPaused = null;
			isPausedEvent = null;
			isNotPausedEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}
		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsPaused())
			{
				isPaused.Value = true;
				if (_isPaused != 1)
				{
					Fsm.Event(isPausedEvent);
				}
				_isPaused = 1;
			}
			else
			{
				isPaused.Value = false;
				if (_isPaused != 0)
				{
					Fsm.Event(isNotPausedEvent);
				}
				_isPaused = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsPlaying value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsPlaying : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the isPlaying.")]
		public FsmBool isPlaying;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is playing")]
		public FsmEvent isPlayingEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not playing")]
		public FsmEvent isNotPlayingEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isPlaying = -1;

		public override void Reset()
		{
			gameObject = null;
			isPlaying = null;
			isPlayingEvent = null;
			isNotPlayingEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsPlaying())
			{
				isPlaying.Value = true;
				if (_isPlaying != 1)
				{
					Fsm.Event(isPlayingEvent);
				}
				_isPlaying = 1;
			}
			else
			{
				isPlaying.Value = false;
				if (_isPlaying != 0)
				{
					Fsm.Event(isNotPlayingEvent);
				}
				_isPlaying = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Get the IsSeeking value of a MediaPlayer media controller.")]
	public class AVProVideoControlIsSeeking : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the isSeeking.")]
		public FsmBool isSeeking;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is seeking")]
		public FsmEvent isSeekingEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Event Sent if media is not seeking")]
		public FsmEvent isNotSeekingEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		// cache to check if event needs sending or not
		int _isSeeking = -1;

		public override void Reset()
		{
			gameObject = null;
			isSeeking = null;
			isSeekingEvent = null;
			isNotSeekingEvent = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.IsSeeking())
			{
				isSeeking.Value = true;
				if (_isSeeking != 1)
				{
					Fsm.Event(isSeekingEvent);
				}
				_isSeeking = 1;
			}
			else
			{
				isSeeking.Value = false;
				if (_isSeeking != 0)
				{
					Fsm.Event(isNotSeekingEvent);
				}
				_isSeeking = 0;
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Sets the audio mute value of a MediaPlayer media controller.")]
	public class AVProVideoControlMuteAudio : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The value of the audio mute.")]
		public FsmBool mute;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			mute = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.MuteAudio(mute.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Pauses playback of a MediaPlayer.")]
	public class AVProVideoControlPause : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer media controller.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;
			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.Pause();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Starts playback of a MediaPlayer media controller.")]
	public class AVProVideoControlPlay : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event Sent if playback started")]
		public FsmEvent successEvent;

		[Tooltip("Event Sent if MediaPlayer can not play")]
		public FsmEvent canNotPlayEvent;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;

			canNotPlayEvent = null;
			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (this.mediaPlayer.Control.CanPlay())
			{
				this.mediaPlayer.Control.Play();
			}
			else
			{
				Fsm.Event(canNotPlayEvent);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Rewinds a MediaPlayer media controller.")]
	public class AVProVideoControlRewind : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;
			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.Rewind();
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Seeks on a MediaPlayer media controller.")]
	public class AVProVideoControlSeek : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The value of seek in ms.")]
		public FsmFloat seek;

		[Tooltip("If true will seek with a faster system")]
		public FsmBool seekFast;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;

			seek = null;
			seekFast = null;

			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			if (seekFast.Value)
			{
				this.mediaPlayer.Control.SeekFast(seek.Value);
			}
			else
			{
				this.mediaPlayer.Control.Seek(seek.Value);
			}
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Sets the audio track of a MediaPlayer media controller.")]
	public class AVProVideoControlSetAudioTrack : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The audio track")]
		public FsmInt audioTrack;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			audioTrack = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetAudioTrack(audioTrack.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Set the looping value of a MediaPlayer media controller.")]
	public class AVProVideoControlSetLooping : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The value of the looping.")]
		public FsmBool isLooping;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			isLooping = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetLooping(isLooping.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Sets the playback rate of a MediaPlayer media controller.")]
	public class AVProVideoControlSetPlayBackRate : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The playback rate")]
		public FsmFloat playbackRate;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			playbackRate = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetPlaybackRate(playbackRate.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Set the texture properties of a MediaPlayer media controller.")]
	public class AVProVideoControlSetTextureProperties : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The filter mode")]
		[ObjectType(typeof(FilterMode))]
		public FsmEnum filterMode;

		[Tooltip("The textureWrapMode mode")]
		[ObjectType(typeof(TextureWrapMode))]
		public FsmEnum textureWrapMode;

		[Tooltip("The anisoLevel Value")]
		public FsmInt anisoLevel;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			filterMode = FilterMode.Bilinear;
			textureWrapMode = TextureWrapMode.Clamp;
			anisoLevel = 1;

			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetTextureProperties((FilterMode)filterMode.Value, (TextureWrapMode)textureWrapMode.Value, anisoLevel.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Sets the video track of a MediaPlayer media controller.")]
	public class AVProVideoControlSetVideoTrack : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The video track")]
		public FsmInt videoTrack;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;


		public override void Reset()
		{
			gameObject = null;
			videoTrack = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetVideoTrack(videoTrack.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Set the volume value of a MediaPlayer media controller.")]
	public class AVProVideoControlSetVolume : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The value of the volume.")]
		public FsmFloat volume;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		[Tooltip("Execute action everyframe.")]
		public bool everyframe;

		public override void Reset()
		{
			gameObject = null;
			volume = null;
			missingControlEvent = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}

			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this.UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject)))
			{
				ExecuteAction();
			}
		}

		void ExecuteAction()
		{
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.SetVolume(volume.Value);
		}
	}

	[ActionCategory("AVProVideo")]
	[Tooltip("Stops playback of a MediaPlayer media controller.")]
	public class AVProVideoControlStop : AVProVideoActionBase
	{
		public AVProVideoActionHeader headerImage;

		[RequiredField]
		[CheckForComponent(typeof(MediaPlayer))]
		[Tooltip("The GameObject with a MediaPlayer component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event Sent if MediaPlayer media control is missing (null)")]
		public FsmEvent missingControlEvent;

		public override void Reset()
		{
			gameObject = null;
			missingControlEvent = null;
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
			if (this.mediaPlayer.Control == null)
			{
				Fsm.Event(missingControlEvent);
				return;
			}

			this.mediaPlayer.Control.Stop();
		}
	}
}
#endif