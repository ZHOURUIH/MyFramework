using System;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This media player fakes video playback for platforms that aren't supported
	/// </summary>
	public sealed class NullMediaPlayer : BaseMediaPlayer
	{
		private bool		_isPlaying = false;
		private bool		_isPaused = false;
		private float		_currentTime = 0.0f;
//		private bool		_audioMuted = false;
		private float		_volume = 0.0f;
		private float		_playbackRate = 1.0f;
		private bool		_bLoop;

		private int			_Width = 256;
		private int			_height = 256;
		private Texture2D	_texture;
		private Texture2D	_texture_AVPro;
		private Texture2D	_texture_AVPro1;
		private float		_fakeFlipTime;
		private int			_frameCount;

		private const float FrameRate = 10f;

		public override string GetVersion()
		{
			return "0.0.0";
		}

		public override bool OpenVideoFromFile(string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0, int forceFileFormat = 0)
		{
			_texture_AVPro = (Texture2D)Resources.Load("AVPro");
			_texture_AVPro1 = (Texture2D)Resources.Load("AVPro1");

			if( _texture_AVPro )
			{
				_Width = _texture_AVPro.width;
				_height = _texture_AVPro.height;
			}
			
			_texture = _texture_AVPro;

			_fakeFlipTime = 0.0f;
			_frameCount = 0;

			return true;
		}

        public override void CloseVideo()
        {
			_frameCount = 0;
			Resources.UnloadAsset(_texture_AVPro);
			Resources.UnloadAsset(_texture_AVPro1);

			base.CloseVideo();
        }

        public override void SetLooping( bool bLooping )
		{
			_bLoop = bLooping;
		}

		public override bool IsLooping()
		{
			return _bLoop;
		}

		public override bool HasMetaData()
		{
			return true;
		}

		public override bool CanPlay()
		{
			return true;
		}

		public override bool HasAudio()
		{
			return false;
		}

		public override bool HasVideo()
		{
			return false;
		}

		public override void Play()
		{
			_isPlaying = true;
			_isPaused = false;
			_fakeFlipTime = 0.0f;
		}

		public override void Pause()
		{
			_isPlaying = false;
			_isPaused = true;
		}

		public override void Stop()
		{
			_isPlaying = false;
			_isPaused = false;
		}

		public override bool IsSeeking()
		{
			return false;
		}
		public override bool IsPlaying()
		{
			return _isPlaying;
		}
		public override bool IsPaused()
		{
			return _isPaused;
		}
		public override bool IsFinished()
		{
			return _isPlaying && (_currentTime >= GetDurationMs());
		}

		public override bool IsBuffering()
		{
			return false;
		}

		public override float GetDurationMs()
		{
			return 10.0f * 1000.0f;
		}

		public override int GetVideoWidth()
		{
			return _Width;
		}
			
		public override int GetVideoHeight()
		{
			return _height;
		}

		public override float GetVideoDisplayRate()
		{
			return FrameRate;
		}

		public override Texture GetTexture( int index )
		{
//			return _texture ? _texture : Texture2D.whiteTexture;
			return _texture;
		}

		public override int GetTextureFrameCount()
		{
			return _frameCount;
		}

		public override bool RequiresVerticalFlip()
		{
			return false;
		}

		public override void Seek(float timeMs)
		{
			_currentTime = timeMs;
		}

		public override void SeekFast(float timeMs)
		{
			_currentTime = timeMs;
		}

		public override void SeekWithTolerance(float timeMs, float beforeMs, float afterMs)
		{
			_currentTime = timeMs;
		}

		public override float GetCurrentTimeMs()
		{
			return _currentTime;
		}

		public override void SetPlaybackRate(float rate)
		{
			_playbackRate = rate;
		}

		public override float GetPlaybackRate()
		{
			return _playbackRate;
		}

		public override float GetBufferingProgress()
		{
			return 0f;
		}

		public override void MuteAudio(bool bMuted)
		{
//			_audioMuted = bMuted;
		}

		public override bool IsMuted()
		{
			return true;
		}

		public override void SetVolume(float volume)
		{
			_volume = volume;
		}

		public override float GetVolume()
		{
			return _volume;
		}

		public override int GetAudioTrackCount()
		{
			return 0;
		}

		public override int GetCurrentAudioTrack()
		{
			return 0;
		}

		public override void SetAudioTrack( int index )
		{
		}

		public override int GetVideoTrackCount()
		{
			return 0;
		}

		public override int GetCurrentVideoTrack()
		{
			return 0;
		}

		public override string GetCurrentAudioTrackId()
		{
			// TODO
			return "";
		}

		public override int GetCurrentAudioTrackBitrate()
		{
			// TODO
			return 0;
		}
		public override void SetVideoTrack( int index )
		{
		}

		public override string GetCurrentVideoTrackId()
		{
			return "";
		}

		public override int GetCurrentVideoTrackBitrate()
		{
			return 0;
		}
		
		public override float GetVideoFrameRate()
		{
			return 0.0f;
		}

		public override void Update()
		{
			UpdateSubtitles();

			if (_isPlaying)
			{
				_currentTime += Time.deltaTime * 1000.0f;
				if (_currentTime >= GetDurationMs())
				{
					_currentTime = GetDurationMs();
					if( _bLoop )
					{
						Rewind();
					}
				}

				// 

				_fakeFlipTime += Time.deltaTime;
				if( _fakeFlipTime >= (1.0 / FrameRate))
				{
					_fakeFlipTime = 0.0f;
					_texture = ( _texture == _texture_AVPro ) ? _texture_AVPro1 : _texture_AVPro;
					_frameCount++;
				}
			}
		}

		public override void Render()
		{
		}

		public override void Dispose()
		{
		}
	}
}