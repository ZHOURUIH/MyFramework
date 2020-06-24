#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#elif UNITY_IOS || UNITY_ANDROID
	#if UNITY_5_5_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4)
		#define UNITY_PLATFORM_SUPPORTS_LINEAR
	#endif
#endif

using UnityEngine;
using System.Collections.Generic;

#if NETFX_CORE
using Windows.Storage.Streams;
#endif

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Base class for all platform specific MediaPlayers
	/// </summary>
	public abstract class BaseMediaPlayer : IMediaPlayer, IMediaControl, IMediaInfo, IMediaProducer, IMediaSubtitles, System.IDisposable
	{
		public abstract string		GetVersion();

		public abstract bool		OpenVideoFromFile(string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0, int forceFileFormat = 0);

#if NETFX_CORE
		public virtual bool			OpenVideoFromFile(IRandomAccessStream ras, string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0){return false;}
#endif

		public virtual bool			OpenVideoFromBuffer(byte[] buffer) { return false; }
		public virtual bool			StartOpenVideoFromBuffer(ulong length) { return false; }
		public virtual bool			AddChunkToVideoBuffer(byte[] chunk, ulong offset, ulong length) { return false; }
		public virtual bool			EndOpenVideoFromBuffer() { return false; }

		public virtual void			CloseVideo()
		{
			_stallDetectionTimer = 0f;
			_stallDetectionFrame = 0;
			_lastError = ErrorCode.None;
		}

		public abstract void		SetLooping(bool bLooping);
		public abstract bool		IsLooping();

		public abstract bool		HasMetaData();
		public abstract bool		CanPlay();
		public abstract void		Play();
		public abstract void		Pause();
		public abstract void		Stop();
		public virtual void			Rewind() { SeekFast(0.0f);  }

		public abstract void		Seek(float timeMs);
		public abstract void		SeekFast(float timeMs);
		public virtual void			SeekWithTolerance(float timeMs, float beforeMs, float afterMs) { Seek(timeMs); }
		public abstract float		GetCurrentTimeMs();
		public virtual double		GetCurrentDateTimeSecondsSince1970() { return 0.0; }
		public virtual TimeRange[]	GetSeekableTimeRanges() { return _seekableTimeRanges; }

		public abstract float		GetPlaybackRate();
		public abstract void		SetPlaybackRate(float rate);

		public abstract float		GetDurationMs();
		public abstract int			GetVideoWidth();
		public abstract int			GetVideoHeight();
		public virtual  Rect		GetCropRect() { return new Rect(0f, 0f, 0f, 0f); }
		public abstract float		GetVideoDisplayRate();
		public abstract bool		HasAudio();
		public abstract bool		HasVideo();

		public abstract bool		IsSeeking();
		public abstract bool		IsPlaying();
		public abstract bool		IsPaused();
		public abstract bool		IsFinished();
		public abstract bool		IsBuffering();
		public virtual bool			WaitForNextFrame(Camera dummyCamera, int previousFrameCount) { return false; }

		public virtual void			SetPlayWithoutBuffering(bool playWithoutBuffering) { }

		public virtual void			SetKeyServerURL(string url) { }
		public virtual void			SetKeyServerAuthToken(string token) { }
		public virtual void			SetDecryptionKeyBase64(string key) { }
		public virtual void			SetDecryptionKey(byte[] key) { }

		public virtual int			GetTextureCount() { return 1; }
		public abstract Texture		GetTexture(int index = 0);
		public abstract int			GetTextureFrameCount();
		public virtual bool			SupportsTextureFrameCount() { return true; }
		public virtual long			GetTextureTimeStamp() { return long.MinValue; }
		public abstract bool		RequiresVerticalFlip();
		public virtual float[]		GetTextureTransform() { return new float[] { 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f }; }
		public virtual Matrix4x4	GetYpCbCrTransform() { return Matrix4x4.identity; }

		public abstract void		MuteAudio(bool bMuted);
		public abstract bool		IsMuted();
		public abstract void		SetVolume(float volume);
		public virtual void			SetBalance(float balance) { }
		public abstract float		GetVolume();
		public virtual float		GetBalance() { return 0f; }

		public abstract int			GetAudioTrackCount();
		public virtual string		GetAudioTrackId(int index) { return index.ToString(); }
		public abstract int			GetCurrentAudioTrack();
		public abstract void		SetAudioTrack(int index);
		public abstract string		GetCurrentAudioTrackId();
		public abstract int			GetCurrentAudioTrackBitrate();
		public virtual int			GetNumAudioChannels() { return -1; }
		public virtual void			SetAudioHeadRotation(Quaternion q) { }
		public virtual void			ResetAudioHeadRotation() { }
		public virtual void			SetAudioChannelMode(Audio360ChannelMode channelMode) { }
		public virtual void			SetAudioFocusEnabled(bool enabled) { }
		public virtual void			SetAudioFocusProperties(float offFocusLevel, float widthDegrees) { }
		public virtual void			SetAudioFocusRotation(Quaternion q) { }
		public virtual void			ResetAudioFocus() { }

		public abstract int			GetVideoTrackCount();
		public virtual string		GetVideoTrackId(int index) { return index.ToString(); }
		public abstract int			GetCurrentVideoTrack();
		public abstract void		SetVideoTrack(int index);
		public abstract string		GetCurrentVideoTrackId();
		public abstract int			GetCurrentVideoTrackBitrate();

		public abstract float		GetVideoFrameRate();

		public virtual long			GetEstimatedTotalBandwidthUsed() { return -1; }

		public abstract float		GetBufferingProgress();

		public abstract void		Update();
		public abstract void		Render();
		public abstract void		Dispose();

		public ErrorCode GetLastError()
		{
			return _lastError;
		}

		public virtual long GetLastExtendedErrorCode()
		{
			return 0;
		}

		public string GetPlayerDescription()
		{
			return _playerDescription;
		}

		public virtual bool PlayerSupportsLinearColorSpace()
		{
#if UNITY_PLATFORM_SUPPORTS_LINEAR
			return true;
#else
			return false;
#endif
		}

		public virtual int		GetBufferedTimeRangeCount() { return 0; }
		public virtual bool		GetBufferedTimeRange(int index, ref float startTimeMs, ref float endTimeMs) { return false; }

		protected string _playerDescription = string.Empty;
		protected ErrorCode _lastError = ErrorCode.None;
		protected FilterMode _defaultTextureFilterMode = FilterMode.Bilinear;
		protected TextureWrapMode _defaultTextureWrapMode = TextureWrapMode.Clamp;
		protected int _defaultTextureAnisoLevel = 1;

		protected TimeRange[] _seekableTimeRanges = new TimeRange[0];

		public void SetTextureProperties(FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, int anisoLevel = 0)
		{
			_defaultTextureFilterMode = filterMode;
			_defaultTextureWrapMode = wrapMode;
			_defaultTextureAnisoLevel = anisoLevel;
			for (int i = 0; i < GetTextureCount(); ++i)
			{
				ApplyTextureProperties(GetTexture(i));
			}
		}

		protected virtual void ApplyTextureProperties(Texture texture)
		{
			if (texture != null)
			{
				texture.filterMode = _defaultTextureFilterMode;
				texture.wrapMode = _defaultTextureWrapMode;
				texture.anisoLevel = _defaultTextureAnisoLevel;
			}
		}

		public virtual void GrabAudio(float[] buffer, int floatCount, int channelCount)
		{

		}

		protected bool IsExpectingNewVideoFrame()
		{
			if (HasVideo())
			{
				// If we're playing then we expect a new frame
				if (!IsFinished() && (!IsPaused() || IsPlaying()))
				{
					// NOTE: if a new frame isn't available then we could either be seeking or stalled
					return true;
				}
			}
			return false;
		}

		public virtual bool IsPlaybackStalled()
		{
			const float StallDetectionDuration = 0.75f;

			// Manually detect stalled video if the platform doesn't have native support to detect it
			if (SupportsTextureFrameCount() && IsExpectingNewVideoFrame())
			{
				int frameCount = GetTextureFrameCount();
				if (frameCount != _stallDetectionFrame)
				{
					_stallDetectionTimer = 0f;
					_stallDetectionFrame = frameCount;
				}
				else
				{
					_stallDetectionTimer += Time.deltaTime;
				}
				return (_stallDetectionTimer > StallDetectionDuration);
			}
			else
			{
				_stallDetectionTimer = 0f;
			}
			return false;
		}

		private float _stallDetectionTimer;
		private int _stallDetectionFrame;

		protected List<Subtitle> _subtitles;
		protected Subtitle _currentSubtitle;

		public bool LoadSubtitlesSRT(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				// Disable subtitles
				_subtitles = null;
				_currentSubtitle = null;
			}
			else
			{
				_subtitles = Helper.LoadSubtitlesSRT(data);
				_currentSubtitle = null;
			}
			return (_subtitles != null);
		}

		public virtual void UpdateSubtitles()
		{
			if (_subtitles != null)
			{
				float time = GetCurrentTimeMs();

				// TODO: implement a more effecient subtitle index searcher
				int searchIndex = 0;
				if (_currentSubtitle != null)
				{
					if (!_currentSubtitle.IsTime(time))
					{
						if (time > _currentSubtitle.timeEndMs)
						{
							searchIndex = _currentSubtitle.index + 1;
						}
						_currentSubtitle = null;
					}
				}

				if (_currentSubtitle == null)
				{
					for (int i = searchIndex; i < _subtitles.Count; i++)
					{
						if (_subtitles[i].IsTime(time))
						{
							_currentSubtitle = _subtitles[i];
							break;
						}
					}
				}
			}
		}

		public virtual int GetSubtitleIndex()
		{
			int result = -1;
			if (_currentSubtitle != null)
			{
				result = _currentSubtitle.index;
			}
			return result;
		}

		public virtual string GetSubtitleText()
		{
			string result = string.Empty;
			if (_currentSubtitle != null)
			{
				result = _currentSubtitle.text;
			}
			return result;
		}

		public virtual void OnEnable()
		{
		}
	}
}