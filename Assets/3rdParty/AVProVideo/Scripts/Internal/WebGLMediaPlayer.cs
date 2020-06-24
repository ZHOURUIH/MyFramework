#if UNITY_WEBGL
using UnityEngine;
using System.Runtime.InteropServices;
using System;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// WebGL implementation of BaseMediaPlayer
	/// </summary>
    public sealed class WebGLMediaPlayer : BaseMediaPlayer
    {
		//private enum AVPPlayerStatus
		//{
		//    Unknown,
		//    ReadyToPlay,
		//    Playing,
		//    Finished,
		//    Seeking,
		//    Failed
		//}

		[DllImport("__Internal")]
		private static extern bool AVPPlayerInsertVideoElement(string path, int[] idValues, int externalLibrary);

        [DllImport("__Internal")]
        private static extern int AVPPlayerWidth(int player);

        [DllImport("__Internal")]
        private static extern int AVPPlayerHeight(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetLastError(int player);

        [DllImport("__Internal")]
        private static extern int AVPPlayerAudioTrackCount(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerSetAudioTrack(int player, int index);

        [DllImport("__Internal")]
        private static extern void AVPPlayerClose(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerReady(int player);

        [DllImport("__Internal")]
        private static extern void AVPPlayerSetLooping(int player, bool loop);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsLooping(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsSeeking(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsPlaying(int player); 

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsPaused(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsFinished(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsBuffering(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsPlaybackStalled(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerPlay(int player);

        [DllImport("__Internal")]
        private static extern void AVPPlayerPause(int player);

        [DllImport("__Internal")]
        private static extern void AVPPlayerSeekToTime(int player, float timeMS, bool fast);

        [DllImport("__Internal")]
        private static extern float AVPPlayerGetCurrentTime(int player);

		[DllImport("__Internal")]
        private static extern float AVPPlayerGetPlaybackRate(int player);

        [DllImport("__Internal")]
        private static extern void AVPPlayerSetPlaybackRate(int player, float rate);

        [DllImport("__Internal")]
        private static extern void AVPPlayerSetMuted(int player, bool muted);

        [DllImport("__Internal")]
        private static extern float AVPPlayerGetDuration(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerIsMuted(int player);

        [DllImport("__Internal")]
        private static extern float AVPPlayerGetVolume(int player);

        [DllImport("__Internal")]
        private static extern void  AVPPlayerSetVolume(int player, float volume);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerHasVideo(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerHasAudio(int player);

		// Need jslib
		[DllImport("__Internal")]
        private static extern void AVPPlayerFetchVideoTexture(int player, IntPtr texture, bool init);

        [DllImport("__Internal")]
        private static extern int AVPPlayerGetDecodedFrameCount(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerSupportedDecodedFrameCount(int player);

        [DllImport("__Internal")]
        private static extern bool AVPPlayerHasMetadata(int player);

        [DllImport("__Internal")]
        private static extern int AVPPlayerUpdatePlayerIndex(int id);       

		[DllImport("__Internal")]
        private static extern int AVPPlayerGetNumBufferedTimeRanges(int id);    

		[DllImport("__Internal")]
        private static extern float AVPPlayerGetTimeRangeStart(int id, int timeRangeIndex);
		[DllImport("__Internal")]
		private static extern float AVPPlayerGetTimeRangeEnd(int id, int timeRangeIndex);

        private WebGL.ExternalLibrary _externalLibrary = WebGL.ExternalLibrary.None;
		private int _playerIndex = -1;
        private int _playerID = -1;
        private RenderTexture _texture = null;
        private int _width = 0;
        private int _height = 0;
		private int _audioTrackCount = 0;
		private int _audioTrackIndex = 0;
        private bool _useTextureMips = false;
		private System.IntPtr _cachedTextureNativePtr = System.IntPtr.Zero;

		private int _lastFrameCount = 0;
		private float _displayRateTimer = 0f;
		private float _displayRate = 0f;

        private static bool _isWebGL1 = false;

		public static void InitialisePlatform()
        {
            _isWebGL1 = (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
        }

		public WebGLMediaPlayer(WebGL.ExternalLibrary externalLibrary, bool useTextureMips)
		{
			SetOptions(externalLibrary, useTextureMips);
		}

        public void SetOptions(WebGL.ExternalLibrary externalLibrary, bool useTextureMips)
        {
            _externalLibrary = externalLibrary;
            _useTextureMips = useTextureMips;
        }

        public override string GetVersion()
        {
			return "1.9.17";
		}

        public override bool OpenVideoFromFile(string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0, int forceFileFormat = 0)
        {
            bool result = false;

            if (path.StartsWith("http://") || 
				path.StartsWith("https://") ||
				path.StartsWith("file://") ||
                path.StartsWith("blob:") ||
				path.StartsWith("chrome-extension://"))
            {
				int[] idValues = new int[2];
				idValues[0] = -1;
				AVPPlayerInsertVideoElement(path, idValues, (int)_externalLibrary);
				{
					int playerIndex = idValues[0];
					_playerID = idValues[1];

					if (playerIndex > -1)
					{
						_playerIndex = playerIndex;
						result = true;
					}				
				}
			}
			else
			{
				Debug.LogError("[AVProVideo] Unknown URL protocol");
			}

			return result;
		}

		public override void CloseVideo()
		{
			if (_playerIndex != -1)
			{
				Pause();

				_width = 0;
				_height = 0;
				_audioTrackCount = 0;
				_audioTrackIndex = 0;

				AVPPlayerClose(_playerIndex);

				if (_texture != null)
				{
					// Have to update with zero to release Metal textures!
					//_texture.UpdateExternalTexture(0);
					_cachedTextureNativePtr = System.IntPtr.Zero;
					RenderTexture.Destroy(_texture);
					_texture = null;
				}

				_playerIndex = -1;
				_playerID = -1;

                base.CloseVideo();
			}
        }

        public override bool IsLooping()
        {
            //Debug.Assert(_player != -1, "no player IsLooping");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsLooping(_playerIndex);
            }

            return result;
        }

        public override void SetLooping(bool looping)
        {
            Debug.Assert(_playerIndex != -1, "no player SetLooping");

            AVPPlayerSetLooping(_playerIndex, looping);
        }

        public override bool HasAudio()
        {
            //Debug.Assert(_player != -1, "no player HasAudio");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerHasAudio(_playerIndex);
            }

            return result;
        }

        public override bool HasVideo()
        {
            //Debug.Assert(_player != -1, "no player HasVideo");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerHasVideo(_playerIndex);
            }

            return result;
        }

        public override bool HasMetaData()
        {
            //Debug.Assert(_player != -1, "no player HasMetaData");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerHasMetadata(_playerIndex);
            }

            return result;
        }

        public override bool CanPlay()
        {
            //Debug.Assert(_player != -1, "no player CanPlay");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerReady(_playerIndex);
            }

            return result;
        }

        public override void Play()
        {
            Debug.Assert(_playerIndex != -1, "no player Play");

            if (!AVPPlayerPlay(_playerIndex))
			{
				Debug.LogWarning("[AVProVideo] Browser permission prevented video playback");
			}
        }

        public override void Pause()
        {
            Debug.Assert(_playerIndex != -1, "no player Pause");

            AVPPlayerPause(_playerIndex);
        }

        public override void Stop()
        {
            Debug.Assert(_playerIndex != -1, "no player Stop");

            AVPPlayerPause(_playerIndex);
        }

        public override void Seek(float ms)
        {
            Debug.Assert(_playerIndex != -1, "no player Seek");
            AVPPlayerSeekToTime(_playerIndex, ms * 0.001f, false);
        }

        public override void SeekFast(float ms)
        {
            Debug.Assert(_playerIndex != -1, "no player SeekFast");

            AVPPlayerSeekToTime(_playerIndex, ms * 0.001f, true);
        }

        public override float GetCurrentTimeMs()
        {
            //Debug.Assert(_player != -1, "no player GetCurrentTimeMs");
            float result = 0.0f;

            if (_playerIndex != -1)
            {
                result = (AVPPlayerGetCurrentTime(_playerIndex) * 1000.0f);
                //Debug.Log("CurrentTime C#: " + result);
            }

            return result;
        }

        public override void SetPlaybackRate(float rate)
        {
            Debug.Assert(_playerIndex != -1, "no player SetPlaybackRate");

			// No HTML implementations allow negative rate yet
			rate = Mathf.Clamp(rate, 0.25f, 8f);

            AVPPlayerSetPlaybackRate(_playerIndex, rate);
        }

        public override float GetPlaybackRate()
        {
            //Debug.Assert(_player != -1, "no player GetPlaybackRate");
            float result = 0.0f;

            if (_playerIndex != -1)
            {
                result = AVPPlayerGetPlaybackRate(_playerIndex);
            }

            return result;
        }

        public override float GetDurationMs()
        {
            //Debug.Assert(_player != -1, "no player GetDurationMs");
            float result = 0.0f;

            if (_playerIndex != -1)
            {
                result = (AVPPlayerGetDuration(_playerIndex) * 1000.0f);
            }

            return result;
        }

        public override int GetVideoWidth()
        {
			if (_width == 0)
			{
				_width = AVPPlayerWidth(_playerIndex);
			}
			return _width;
        }

        public override int GetVideoHeight()
        {
			if (_height == 0)
			{
				_height = AVPPlayerHeight(_playerIndex);
			}
			return _height;
        }

		public override float GetVideoFrameRate()
        {
			// There is no way in HTML5 yet to get the frame rate of the video
            return 0f;
        }

        public override float GetVideoDisplayRate()
        {
			return _displayRate;
        }

        public override bool IsSeeking()
        {
            //Debug.Assert(_player != -1, "no player IsSeeking");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsSeeking(_playerIndex);
            }

            return result;
        }

        public override bool IsPlaying()
        {
            //Debug.Assert(_player != -1, "no player IsPlaying");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsPlaying(_playerIndex);
            }

            return result;
        }

        public override bool IsPaused()
        {
            //Debug.Assert(_player != -1, "no player IsPaused");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsPaused(_playerIndex);
            }

            return result;
        }

        public override bool IsFinished()
        {
            //Debug.Assert(_player != -1, "no player IsFinished");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsFinished(_playerIndex);
            }

            return result;
        }

        public override bool IsBuffering()
        {
            //Debug.Assert(_player != -1, "no player IsBuffering");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsBuffering(_playerIndex);
            }

            return result;
        }

        public override Texture GetTexture( int index )
        {
            return _texture;
        }

        public override int GetTextureFrameCount()
        {
            //Debug.Assert(_player != -1, "no player GetTextureFrameCount");
            int result = 0;

            if (_playerIndex != -1)
            {
                result = AVPPlayerGetDecodedFrameCount(_playerIndex);
            }

            return result;
        }

        public override bool SupportsTextureFrameCount()
        {
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerSupportedDecodedFrameCount(_playerIndex);
            }

            return result;
        }        

        public override bool RequiresVerticalFlip()
        {
			return true;
        }

        public override bool IsMuted()
        {
            //Debug.Assert(_player != -1, "no player IsMuted");
            bool result = false;

            if (_playerIndex != -1)
            {
                result = AVPPlayerIsMuted(_playerIndex);
            }

            return result;
        }

        public override void MuteAudio(bool bMute)
        {
            Debug.Assert(_playerIndex != -1, "no player MuteAudio");

            AVPPlayerSetMuted(_playerIndex, bMute);
        }

        public override void SetVolume(float volume)
        {
            Debug.Assert(_playerIndex != -1, "no player SetVolume");

            AVPPlayerSetVolume(_playerIndex, volume);
        }

        public override float GetVolume()
        {
            //Debug.Assert(_player != -1, "no player GetVolume");
            float result = 0.0f;

            if (_playerIndex != -1)
            {
                result = AVPPlayerGetVolume(_playerIndex);
            }

            return result;
        }

        public override void Render()
        {
            
        }

		private void UpdateLastErrorCode()
		{
			var code = AVPPlayerGetLastError(_playerIndex);

			switch(code){
				case 0:
					_lastError = ErrorCode.None;
					break;
				case 1:
					_lastError = ErrorCode.LoadFailed;
					break;
				case 2:
					_lastError = ErrorCode.LoadFailed;
					break;
				case 3:
					_lastError = ErrorCode.DecodeFailed;
					break;
				case 4:
					_lastError = ErrorCode.LoadFailed;
					break;
				default:
					break;
			}
		}

        public override void Update()
        {
            if(_playerID > -1) // CheckPlayer's index and update it
            {
                _playerIndex = AVPPlayerUpdatePlayerIndex(_playerID);
            }

            if(_playerIndex > -1)
            {
				UpdateSubtitles();

				UpdateLastErrorCode();

				if (AVPPlayerReady(_playerIndex))
                {
					if (AVPPlayerHasVideo(_playerIndex))
					{
						_width = AVPPlayerWidth(_playerIndex);
						_height = AVPPlayerHeight(_playerIndex);

						if (_texture == null && _width > 0 && _height > 0)
						{
							_texture = new RenderTexture(_width, _height, 0, RenderTextureFormat.Default);
                            #if UNITY_5_6_OR_NEWER
                            _texture.autoGenerateMips = false;
                            #endif
                            _texture.useMipMap = false;
                            if (_useTextureMips && (!_isWebGL1 || (Mathf.IsPowerOfTwo(_width) && Mathf.IsPowerOfTwo(_height))))
                            {
                                // Mip generation only supported in WebGL 2.0, or WebGL 1.0 when using power-of-two textures
                                _texture.useMipMap = true;
                            }
                            _texture.Create();
							_cachedTextureNativePtr = _texture.GetNativeTexturePtr();
							ApplyTextureProperties(_texture);

                            // Textures in WebGL 2.0 don't require texImage2D as they are already recreated with texStorage2D
							AVPPlayerFetchVideoTexture(_playerIndex, _cachedTextureNativePtr, _isWebGL1?true:false);
						}

						if (_texture != null && (_texture.width != _width || _texture.height != _height))
						{
                            RenderTexture.Destroy(_texture);
							_texture = new RenderTexture(_width, _height, 0, RenderTextureFormat.Default);
                            #if UNITY_5_6_OR_NEWER
                            _texture.autoGenerateMips = false;
                            #endif
                            _texture.useMipMap = false;
                            if (_useTextureMips && (!_isWebGL1 || (Mathf.IsPowerOfTwo(_width) && Mathf.IsPowerOfTwo(_height))))
                            {
                                // Mip generation only supported in WebGL 2.0, or WebGL 1.0 when using power-of-two textures
                                _texture.useMipMap = true;
                            }
                            _texture.Create();
							_cachedTextureNativePtr = _texture.GetNativeTexturePtr();
                            ApplyTextureProperties(_texture);

                            // Textures in WebGL 2.0 don't require texImage2D as they are already recreated with texStorage2D
                            AVPPlayerFetchVideoTexture(_playerIndex, _cachedTextureNativePtr, _isWebGL1?true:false);
                            #if UNITY_5_6_OR_NEWER
                            if (_texture.useMipMap)
                            {
                                _texture.GenerateMips();
                            }
                            #endif
						}

						if (_cachedTextureNativePtr != System.IntPtr.Zero)
						{
							// TODO: only update the texture when the frame count changes
							// (actually this will break the update for certain browsers such as edge and possibly safari - Sunrise)
							AVPPlayerFetchVideoTexture(_playerIndex, _cachedTextureNativePtr, false);
                            #if UNITY_5_6_OR_NEWER
                            if (_texture.useMipMap)
                            {
                                _texture.GenerateMips();
                            }
                            #endif
						}

						UpdateDisplayFrameRate();
					}

					if (AVPPlayerHasAudio(_playerIndex))
					{
						_audioTrackCount = Mathf.Max(1, AVPPlayerAudioTrackCount(_playerIndex));
					}
				}
			} 
        }

		private void UpdateDisplayFrameRate()
		{
			_displayRateTimer += Time.deltaTime;
			if (_displayRateTimer >= 0.5f)
			{
				int frameCount = AVPPlayerGetDecodedFrameCount(_playerIndex);
				int frames = (frameCount - _lastFrameCount);
				if (frames > 0)
				{
					_displayRate = (float)frames / _displayRateTimer;
				}
				else
				{
					_displayRate = 0f;
				}
				_displayRateTimer = 0f;
				_lastFrameCount = frameCount;
			}
		}

		public override void Dispose()
        {
            CloseVideo();
        }

		public override int GetAudioTrackCount()
		{
			return _audioTrackCount;
		}

		public override int GetCurrentAudioTrack()
		{
			return _audioTrackIndex;
		}

		public override void SetAudioTrack(int index)
		{
			if (_playerIndex > -1)
			{
				if (index >= 0 && index < _audioTrackCount)
				{
					if (index != _audioTrackIndex)
					{
						AVPPlayerSetAudioTrack(_playerIndex, index);
						_audioTrackIndex = index;
					}
				}
			}
		}

		public override bool IsPlaybackStalled()
		{
            bool result = false;
			if (_playerIndex > -1)
			{
                result = AVPPlayerIsPlaybackStalled(_playerIndex);
            }
            return result;
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

		public override int GetVideoTrackCount()
		{
			// TODO
			return 0;
		}

		public override int GetCurrentVideoTrack()
		{
			// TODO
			return 0;
		}

		public override void SetVideoTrack( int index )
		{
			// TODO
		}

		public override string GetCurrentVideoTrackId()
		{
			// TODO
			return "";
		}

		public override int GetCurrentVideoTrackBitrate()
		{
			// TODO
			return 0;
		}

		public override float GetBufferingProgress()
		{
			//TODO
			return 0f;
		}

		public override int GetBufferedTimeRangeCount()
		{
			return AVPPlayerGetNumBufferedTimeRanges(_playerIndex);
		}

		public override bool GetBufferedTimeRange(int index, ref float startTimeMs, ref float endTimeMs)
		{
			startTimeMs = AVPPlayerGetTimeRangeStart(_playerIndex, index) * 1000.0f;
			endTimeMs = AVPPlayerGetTimeRangeEnd(_playerIndex, index) * 1000.0f;

			return true;
		}
	}
}
#endif