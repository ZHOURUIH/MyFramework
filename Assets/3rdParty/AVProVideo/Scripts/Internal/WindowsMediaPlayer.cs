#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_WSA

#if UNITY_5 || UNITY_5_4_OR_NEWER
	#if !UNITY_5_0 && !UNITY_5_1
		#define AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
	#endif
	#if !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4_0 && !UNITY_5_4_1
		#define AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
	#endif
#endif
#if UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_1 || UNITY_WSA_10
	#define AVPROVIDEO_MARSHAL_RETURN_BOOL
#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

#if NETFX_CORE
using Windows.Storage.Streams;
#endif

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Windows desktop, Windows phone and UWP implementation of BaseMediaPlayer
	/// </summary>
	public /*sealed*/ partial class WindowsMediaPlayer : BaseMediaPlayer
	{
		private bool			_forceAudioResample = true;
		private bool			_useUnityAudio = false;
		private string			_audioDeviceOutputName = string.Empty;
		private List<string>	_preferredFilters = new List<string>();
		private Audio360ChannelMode _audioChannelMode = Audio360ChannelMode.TBE_8_2;

		private bool		_isPlaying = false;
		private bool		_isPaused = false;
		private bool		_audioMuted = false;
		private float		_volume = 1.0f;
		private float		_balance = 0.0f;
		private bool		_bLoop = false;
		private bool		_canPlay = false;
		private bool		_hasMetaData = false;
		private int			_width = 0;
		private int			_height = 0;
		private float		_frameRate = 0f;
		private bool		_hasAudio = false;
		private bool		_hasVideo = false;
		private bool		_isTextureTopDown = true;
		private System.IntPtr _nativeTexture = System.IntPtr.Zero;
		private Texture2D	_texture;
		private System.IntPtr _instance = System.IntPtr.Zero;
		private float		_displayRateTimer;
		private int			_lastFrameCount;
		private float		_displayRate = 1f;
		private Windows.VideoApi	_videoApi = Windows.VideoApi.MediaFoundation;
		private bool		_useHardwareDecoding = true;
		private bool		_useTextureMips = false;
		private bool		_hintAlphaChannel = false;
		private bool		_useLowLatency = false;
		private int			_queueSetAudioTrackIndex = -1;
		private bool		_supportsLinearColorSpace = true;

		private int			_bufferedTimeRangeCount = 0;
		private float[]		_bufferedTimeRanges = new float[0];

		private static bool _isInitialised = false;
		private static string _version = "Plug-in not yet initialised";


#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
		private static System.IntPtr _nativeFunction_UpdateAllTextures;
		private static System.IntPtr _nativeFunction_FreeTextures;
		private static System.IntPtr _nativeFunction_ExtractFrame;
#endif
#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		private int _textureQuality = QualitySettings.masterTextureLimit;
#endif

		public static bool InitialisePlatform()
		{
			if (!_isInitialised)
			{
				try
				{
					if (!Native.Init(QualitySettings.activeColorSpace == ColorSpace.Linear, true))
					{
						Debug.LogError("[AVProVideo] Failing to initialise platform");
					}
					else
					{
						_isInitialised = true;
						_version = GetPluginVersion();
#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
						_nativeFunction_UpdateAllTextures = Native.GetRenderEventFunc_UpdateAllTextures();
						_nativeFunction_FreeTextures = Native.GetRenderEventFunc_FreeTextures();
						_nativeFunction_ExtractFrame = Native.GetRenderEventFunc_WaitForNewFrame();
#endif
					}
				}
				catch (System.DllNotFoundException e)
				{
					Debug.LogError("[AVProVideo] Failed to load DLL. " + e.Message);
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#if !UNITY_5 && !UNITY_5_4_OR_NEWER
					Debug.LogError("[AVProVideo] You may need to copy the Audio360 DLL into the root folder of your project (the folder above Assets)");
#endif
#endif
				}
			}

			return _isInitialised;
		}

		public static void DeinitPlatform()
		{
			Native.Deinit();
			_isInitialised = false;
		}

		public override int GetNumAudioChannels()
		{
			return Native.GetAudioChannelCount(_instance);
		}

		public WindowsMediaPlayer(Windows.VideoApi videoApi, bool useHardwareDecoding, bool useTextureMips, bool hintAlphaChannel, bool useLowLatency, string audioDeviceOutputName, bool useUnityAudio, bool forceResample, List<string> preferredFilters)
		{
			SetOptions(videoApi, useHardwareDecoding:useHardwareDecoding, useTextureMips:useTextureMips, hintAlphaChannel:hintAlphaChannel, useLowLatency:useLowLatency, audioDeviceOutputName:audioDeviceOutputName, useUnityAudio:useUnityAudio, forceResample:forceResample, preferredFilters:preferredFilters);
		}

		public void SetOptions(Windows.VideoApi videoApi, bool useHardwareDecoding, bool useTextureMips, bool hintAlphaChannel, bool useLowLatency, string audioDeviceOutputName, bool useUnityAudio, bool forceResample, List<string> preferredFilters)
		{
			_videoApi = videoApi;
			_useHardwareDecoding = useHardwareDecoding;
			_useTextureMips = useTextureMips;
			_hintAlphaChannel = hintAlphaChannel;
			_useLowLatency = useLowLatency;
			_audioDeviceOutputName = audioDeviceOutputName;
			if (!string.IsNullOrEmpty(_audioDeviceOutputName))
			{
				_audioDeviceOutputName = _audioDeviceOutputName.Trim();
			}
			_useUnityAudio = useUnityAudio;
			_forceAudioResample = forceResample;
			_preferredFilters = preferredFilters;
			if (_preferredFilters != null)
			{
				for (int i = 0; i < _preferredFilters.Count; ++i)
				{
					if (!string.IsNullOrEmpty(_preferredFilters[i]))
					{
						_preferredFilters[i] = _preferredFilters[i].Trim();
					}
				}
			}
		}

		public override string GetVersion()
		{
			return _version;
		}

		private static int GetUnityAudioSampleRate()
		{
			int result = 0;

			// For standalone builds (not in the editor):
			// In Unity 4.6, 5.0, 5.1 when audio is disabled there is no indication from the API.
			// But in 5.2.0 and above, it logs an error when trying to call
			// AudioSettings.GetDSPBufferSize() or AudioSettings.outputSampleRate
			// So to prevent the error, check if AudioSettings.GetConfiguration().sampleRate == 0
			
#if UNITY_5_4_OR_NEWER || UNITY_5_2 || UNITY_5_3
			result = (AudioSettings.GetConfiguration().sampleRate == 0) ? 0 : AudioSettings.outputSampleRate;
#else
			result = AudioSettings.outputSampleRate;
#endif
			return result;
		}

		public override bool OpenVideoFromFile(string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0, int forceFileFormat = 0)
		{
			CloseVideo();

			uint filterCount = 0U;
			IntPtr[] filters = null;

			if (_preferredFilters != null && _preferredFilters.Count > 0)
			{
				filterCount = (uint)_preferredFilters.Count;
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.OpenSource(_instance, path, (int)_videoApi, _useHardwareDecoding, _useTextureMips, _hintAlphaChannel, _useLowLatency, _audioDeviceOutputName, _useUnityAudio, _forceAudioResample, GetUnityAudioSampleRate(), filters, filterCount, (int)_audioChannelMode, sourceSamplerate, sourceChannels);

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				DisplayLoadFailureSuggestion(path);
				return false;
			}

			Native.SetUnityAudioEnabled(_instance, _useUnityAudio);

			return true;
		}

		public override bool OpenVideoFromBuffer(byte[] buffer)
		{
			CloseVideo();

			IntPtr[] filters;
			if (_preferredFilters.Count == 0)
			{
				filters = null;
			}
			else
			{
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.OpenSourceFromBuffer(_instance, buffer, (ulong)buffer.Length, (int)_videoApi, _useHardwareDecoding, _useTextureMips, _hintAlphaChannel, _useLowLatency, _audioDeviceOutputName, _useUnityAudio, filters, (uint)_preferredFilters.Count);

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				return false;
			}

			Native.SetUnityAudioEnabled(_instance, _useUnityAudio);

			return true;
		}

		public override bool StartOpenVideoFromBuffer(ulong length)
		{
			CloseVideo();

			_instance = Native.StartOpenSourceFromBuffer(_instance, (int)_videoApi, length);

			return _instance != IntPtr.Zero;
		}

		public override bool AddChunkToVideoBuffer(byte[] chunk, ulong offset, ulong length)
		{
			return Native.AddChunkToSourceBuffer(_instance, chunk, offset, length);
		}

		public override bool EndOpenVideoFromBuffer()
		{
			IntPtr[] filters;
			if (_preferredFilters.Count == 0)
			{
				filters = null;
			}
			else
			{
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.EndOpenSourceFromBuffer(_instance, _useHardwareDecoding, _useTextureMips, _hintAlphaChannel, _useLowLatency, _audioDeviceOutputName, _useUnityAudio, filters, (uint)_preferredFilters.Count);

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				return false;
			}

			Native.SetUnityAudioEnabled(_instance, _useUnityAudio);

			return true;
		}

#if NETFX_CORE
		public override bool OpenVideoFromFile(IRandomAccessStream ras, string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0)
		{
			CloseVideo();

			_instance = Native.OpenSourceFromStream(_instance, ras, path, (int)_videoApi, _useHardwareDecoding, _useTextureMips, _hintAlphaChannel, _useLowLatency, _audioDeviceOutputName, _useUnityAudio, _forceAudioResample, GetUnityAudioSampleRate(), sourceSamplerate, sourceChannels);

			if (_instance == System.IntPtr.Zero)
			{
				DisplayLoadFailureSuggestion(path);
				return false;
			}

			Native.SetUnityAudioEnabled(_instance, _useUnityAudio);

			return true;
		}
#endif

		private void DisplayLoadFailureSuggestion(string path)
		{
			bool usingDirectShow = (_videoApi == Windows.VideoApi.DirectShow) || SystemInfo.operatingSystem.Contains("Windows 7") || SystemInfo.operatingSystem.Contains("Windows Vista") || SystemInfo.operatingSystem.Contains("Windows XP");
			if (usingDirectShow && path.Contains(".mp4"))
			{
				Debug.LogWarning("[AVProVideo] The native Windows DirectShow H.264 decoder doesn't support videos with resolution above 1920x1080. You may need to reduce your video resolution, switch to another codec (such as DivX or Hap), or install 3rd party DirectShow codec (eg LAV Filters).  This shouldn't be a problem for Windows 8 and above as it has a native limitation of 3840x2160.");
			}
		}

        public override void CloseVideo()
        {
			_width = 0;
			_height = 0;
			_frameRate = 0f;
			_hasAudio = _hasVideo = false;
			_hasMetaData = false;
			_canPlay = false;
			_isPaused = false;
			_isPlaying = false;
			_bLoop = false;
			_audioMuted = false;
			_volume = 1f;
			_balance = 0f;
			_lastFrameCount = 0;
			_displayRate = 0f;
			_displayRateTimer = 0f;
			_queueSetAudioTrackIndex = -1;
			_supportsLinearColorSpace = true;
			_nativeTexture = System.IntPtr.Zero;

			if (_texture != null)
			{
				Texture2D.Destroy(_texture);
				_texture = null;
			}
			if (_instance != System.IntPtr.Zero)
			{
                Native.CloseSource(_instance);
				_instance = System.IntPtr.Zero;
            }

			// Issue thread event to free the texture on the GPU
			IssueRenderThreadEvent(Native.RenderThreadEvent.FreeTextures);

			base.CloseVideo();
        }

        public override void SetLooping(bool looping)
		{
			_bLoop = looping;
			Native.SetLooping(_instance, looping);
		}

		public override bool IsLooping()
		{
			return _bLoop;
		}

		public override bool HasMetaData()
		{
			return _hasMetaData;
		}

		public override bool HasAudio()
		{
			return _hasAudio;
		}

		public override bool HasVideo()
		{
			return _hasVideo;
		}

		public override bool CanPlay()
		{
			return _canPlay;
		}

		public override void Play()
		{
			_isPlaying = true;
			_isPaused = false;
			Native.Play(_instance);
		}

		public override void Pause()
		{
			_isPlaying = false;
			_isPaused = true;
			Native.Pause(_instance);
		}

		public override void Stop()
		{
			_isPlaying = false;
			_isPaused = false;
			Native.Pause(_instance);
		}

		public override bool IsSeeking()
		{
			return Native.IsSeeking(_instance);
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
			return Native.IsFinished(_instance);
		}

		public override bool IsBuffering()
		{
			return Native.IsBuffering(_instance);
		}

		public override float GetDurationMs()
		{
			return Native.GetDuration(_instance) * 1000f;
		}

		public override int GetVideoWidth()
		{
			return _width;
		}
			
		public override int GetVideoHeight()
		{
			return _height;
		}

		public override float GetVideoFrameRate()
		{
			return _frameRate;
		}

		public override float GetVideoDisplayRate()
		{
			return _displayRate;
		}

		public override Texture GetTexture( int index )
		{
			Texture result = null;
			if (Native.GetTextureFrameCount(_instance) > 0)
			{
				result = _texture;
			}
			return result;
		}

		public override int GetTextureFrameCount()
		{
			return Native.GetTextureFrameCount(_instance);
		}

		public override long GetTextureTimeStamp()
		{
			return Native.GetTextureTimeStamp(_instance);
		}

		public override bool RequiresVerticalFlip()
		{
			return _isTextureTopDown;
		}

		public override void Seek(float timeMs)
		{
			Native.SetCurrentTime(_instance, timeMs / 1000f, false);
		}

		public override void SeekFast(float timeMs)
		{
			Native.SetCurrentTime(_instance, timeMs / 1000f, true);
		}

		public override float GetCurrentTimeMs()
		{
			return Native.GetCurrentTime(_instance) * 1000f;
		}

		public override void SetPlaybackRate(float rate)
		{
			Native.SetPlaybackRate(_instance, rate);
		}

		public override float GetPlaybackRate()
		{
			return Native.GetPlaybackRate(_instance);
		}

		public override float GetBufferingProgress()
		{
			return Native.GetBufferingProgress(_instance);
		}

		public override int GetBufferedTimeRangeCount()
		{
			return _bufferedTimeRangeCount;
		}

		public override bool GetBufferedTimeRange(int index, ref float startTimeMs, ref float endTimeMs)
		{
			bool result = false;
			if (index >= 0 && index < _bufferedTimeRangeCount)
			{
				//Debug.Assert(_bufferedTimeRanges.Length > (index * 2 + 1));
				result = true;
				startTimeMs = 1000f * _bufferedTimeRanges[index * 2 + 0];
				endTimeMs = 1000f * _bufferedTimeRanges[index * 2 + 1];
			}
			return result;
		}

		public override void MuteAudio(bool bMuted)
		{
			_audioMuted = bMuted;
			Native.SetMuted(_instance, _audioMuted);
		}

		public override bool IsMuted()
		{
			return _audioMuted;
		}

		public override void SetVolume(float volume)
		{
			_volume = volume;
			Native.SetVolume(_instance, volume);
		}

		public override float GetVolume()
		{
			return _volume;
		}

		public override void SetBalance(float balance)
		{
			_balance = balance;
			Native.SetBalance(_instance, balance);
		}

		public override float GetBalance()
		{
			return _balance;
		}

		public override int GetAudioTrackCount()
		{
			return Native.GetAudioTrackCount(_instance);
		}

		public override int GetCurrentAudioTrack()
		{
			return Native.GetAudioTrack(_instance);
		}

		public override void SetAudioTrack( int index )
		{
			_queueSetAudioTrackIndex = index;
		}

		public override int GetVideoTrackCount()
		{
			int result = 0;
			if (HasVideo())
			{
				result = 1;
			}
			return result;
		}

		public override bool IsPlaybackStalled()
		{
			bool result = Native.IsPlaybackStalled(_instance);
			if (!result)
			{
				result = base.IsPlaybackStalled();
			}
			return result;
		}

		public override string GetCurrentAudioTrackId()
		{
			// TODO
			return string.Empty;
		}

		public override int GetCurrentAudioTrackBitrate()
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
			return string.Empty;
		}

		public override int GetCurrentVideoTrackBitrate()
		{
			// TODO
			return 0;
		}

		public override bool WaitForNextFrame(Camera dummyCamera, int previousFrameCount)
		{
			// Mark as extracting
			Native.StartExtractFrame(_instance);

			// Queue up render thread event to wait for the new frame
			IssueRenderThreadEvent(Native.RenderThreadEvent.WaitForNewFrame);

			// Force render thread to run
			dummyCamera.Render();

			// Wait for the frame to change
			Native.WaitForExtract(_instance);

			// Return whether the frame changed
			return (previousFrameCount != Native.GetTextureFrameCount(_instance));
		}

		public override void SetAudioChannelMode(Audio360ChannelMode channelMode)
		{
			_audioChannelMode = channelMode;
			Native.SetAudioChannelMode(_instance, (int)channelMode);
		}

		public override void SetAudioHeadRotation(Quaternion q)
		{
			Native.SetHeadOrientation(_instance, q.x, q.y, q.z, q.w);
		}

		public override void ResetAudioHeadRotation()
		{
			Native.SetHeadOrientation(_instance, Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
		}

		public override void SetAudioFocusEnabled(bool enabled)
		{
			Native.SetAudioFocusEnabled(_instance, enabled);
		}

		public override void SetAudioFocusProperties(float offFocusLevel, float widthDegrees)
		{
			Native.SetAudioFocusProps(_instance, offFocusLevel, widthDegrees);
		}

		public override void SetAudioFocusRotation(Quaternion q)
		{
			Native.SetAudioFocusRotation(_instance, q.x, q.y, q.z, q.w);
		}

		public override void ResetAudioFocus()
		{
			Native.SetAudioFocusEnabled(_instance, false);
			Native.SetAudioFocusProps(_instance, 0f, 90f);
			Native.SetAudioFocusRotation(_instance, 0f, 0f, 0f, 1f);
		}

		//public override void SetAudioDeviceName(string name)
		//{
		//}

		public override void Update()
		{
			Native.Update(_instance);
			_lastError = (ErrorCode)Native.GetLastErrorCode(_instance);

			if (_queueSetAudioTrackIndex >= 0 && _hasAudio)
			{
				// We have to queue the setting of the audio track, as doing it from the UI can result in a crash (for some unknown reason)
				Native.SetAudioTrack(_instance, _queueSetAudioTrackIndex);
				_queueSetAudioTrackIndex = -1;
			}

			// Update network buffering
			{
				_bufferedTimeRangeCount = Native.GetBufferedRanges(_instance, _bufferedTimeRanges, _bufferedTimeRanges.Length / 2);
				if (_bufferedTimeRangeCount > (_bufferedTimeRanges.Length / 2))
				{
					_bufferedTimeRanges = new float[_bufferedTimeRangeCount * 2];
					_bufferedTimeRangeCount = Native.GetBufferedRanges(_instance, _bufferedTimeRanges, _bufferedTimeRanges.Length / 2);
				}
			}

			UpdateSubtitles();

			if (!_canPlay)
			{
				if (!_hasMetaData)
				{
					if (Native.HasMetaData(_instance))
					{
						if (Native.HasVideo(_instance))
						{
							_width = Native.GetWidth(_instance);
							_height = Native.GetHeight(_instance);
							_frameRate = Native.GetFrameRate(_instance);

							// Sometimes the dimensions aren't available yet, in which case fail and poll them again next loop
							if (_width > 0 && _height > 0)
							{
								_hasVideo = true;

								// Note: If the Unity editor Build platform isn't set to Windows then maxTextureSize will not be correct
								if (Mathf.Max(_width, _height) > SystemInfo.maxTextureSize

								// If we're running in the editor it may be emulating another platform
								// in which case maxTextureSize won't be correct, so ignore it.
								#if UNITY_EDITOR
								&& !SystemInfo.graphicsDeviceName.ToLower().Contains("emulated")
								#endif
								)
								{
									Debug.LogError(string.Format("[AVProVideo] Video dimensions ({0}x{1}) larger than maxTextureSize ({2} for current build target)", _width, _height, SystemInfo.maxTextureSize));
									_width = _height = 0;
									_hasVideo = false;
								}
							}

							if (_hasVideo)
							{
								if (Native.HasAudio(_instance))
								{
									_hasAudio = true;
								}
							}
						}
						else
						{
							if (Native.HasAudio(_instance))
							{
								_hasAudio = true;
							}
						}

						if (_hasVideo || _hasAudio)
						{
							_hasMetaData = true;
						}

						_playerDescription = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPlayerDescription(_instance));
						_supportsLinearColorSpace = !_playerDescription.Contains("MF-MediaEngine-Hardware");
						Helper.LogInfo("Using playback path: " + _playerDescription + " (" + _width + "x" + _height + "@" + GetVideoFrameRate().ToString("F2") + ")");
						if (_hasVideo)
						{
							OnTextureSizeChanged();
						}
					}
				}
				if (_hasMetaData)
				{
					_canPlay = Native.CanPlay(_instance);
				}
			}

#if UNITY_WSA
			// NOTE: I think this issue has been resolved now as of version 1.5.24.  
			// The issue was caused by functions returning booleans incorrectly (4 bytes vs 1)
			// and as been resolved by specificying the return type during marshalling..
			// Still we'll keep this code here until after more testing.

			// WSA has an issue where it can load the audio track first and the video track later
			// Here we try to handle this case and get the video track information when it arrives
			if (_hasAudio && !_hasVideo)
			{
				_width = Native.GetWidth(_instance);
				_height = Native.GetHeight(_instance);
				_frameRate = Native.GetFrameRate(_instance);

				if (_width > 0 && _height > 0)
				{
					_hasVideo = true;
					OnTextureSizeChanged();
				}
			}
#endif

			if (_hasVideo)
			{
				System.IntPtr newPtr = Native.GetTexturePointer(_instance);

				// Check for texture recreation (due to device loss or change in texture size)
				if (_texture != null && _nativeTexture != System.IntPtr.Zero && _nativeTexture != newPtr)
				{
					_width = Native.GetWidth(_instance);
					_height = Native.GetHeight(_instance);

					if (newPtr == System.IntPtr.Zero || (_width != _texture.width || _height != _texture.height))
					{
						if (_width != _texture.width || _height != _texture.height)
						{
							Helper.LogInfo("Texture size changed: " + _width + " X " + _height);
							OnTextureSizeChanged();
						}

						_nativeTexture = System.IntPtr.Zero;
						Texture2D.Destroy(_texture);
						_texture = null;
					}
					else if (_nativeTexture != newPtr)
					{
						_texture.UpdateExternalTexture(newPtr);
						_nativeTexture = newPtr;
					}
				}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
				// In Unity 5.4.2 and above the video texture turns black when changing the TextureQuality in the Quality Settings
				// The code below gets around this issue.  A bug report has been sent to Unity.  So far we have tested and replicated the
				// "bug" in Windows only, but a user has reported it in Android too.  
				// Texture.GetNativeTexturePtr() must sync with the rendering thread, so this is a large performance hit!
				if(_textureQuality != QualitySettings.masterTextureLimit)
				{
					if (_texture != null && _nativeTexture != System.IntPtr.Zero && _texture.GetNativeTexturePtr() == System.IntPtr.Zero)
					{
						//Debug.Log("RECREATING");
						_texture.UpdateExternalTexture(_nativeTexture);
					}

					_textureQuality = QualitySettings.masterTextureLimit;
				}
				
#endif

				// Check if a new texture has to be created
				if (_texture == null && _width > 0 && _height > 0 && newPtr != System.IntPtr.Zero)
				{
					_isTextureTopDown = Native.IsTextureTopDown(_instance);
					_texture = Texture2D.CreateExternalTexture(_width, _height, TextureFormat.RGBA32, _useTextureMips, false, newPtr);
					if (_texture != null)
					{
						_texture.name = "AVProVideo";
						_nativeTexture = newPtr;
						ApplyTextureProperties(_texture);
					}
					else
					{
						Debug.LogError("[AVProVideo] Failed to create texture");
					}
				}
			}
		}

		public override long GetLastExtendedErrorCode()
		{
			return Native.GetLastExtendedErrorCode(_instance);
		}

		private void OnTextureSizeChanged()
		{
			// Warning for DirectShow Microsoft H.264 decoder which has a limit of 1920x1080 and can fail silently and return video dimensions clamped at 720x480
			if ((_width == 720 || _height == 480) && _playerDescription.Contains("DirectShow"))
			{
				Debug.LogWarning("[AVProVideo] If video fails to play then it may be due to the resolution being higher than 1920x1080 which is the limitation of the Microsoft DirectShow H.264 decoder.\nTo resolve this you can either use Windows 8 or above (and disable 'Force DirectShow' option), resize your video, use a different codec (such as Hap or DivX), or install a 3rd party H.264 decoder such as LAV Filters.");
			}
			// Warning when using software decoder with high resolution videos
			else if ((_width > 1920 || _height > 1080) && _playerDescription.Contains("MF-MediaEngine-Software"))
			{
				//Debug.LogWarning("[AVProVideo] Using software video decoder.  For best performance consider adding the -force-d3d11-no-singlethreaded command-line switch to enable GPU decoding.");
			}
		}

		private void UpdateDisplayFrameRate()
		{
			_displayRateTimer += Time.deltaTime;
			if (_displayRateTimer >= 0.5f)
			{
				int frameCount = Native.GetTextureFrameCount(_instance);
				_displayRate = (float)(frameCount - _lastFrameCount) / _displayRateTimer;
				_displayRateTimer -= 0.5f;
				if (_displayRateTimer >= 0.5f)
					_displayRateTimer = 0f;
				_lastFrameCount = frameCount;
			}
		}

		public override void Render()
		{
			UpdateDisplayFrameRate();

			IssueRenderThreadEvent(Native.RenderThreadEvent.UpdateAllTextures);
		}

		public override void Dispose()
		{
			CloseVideo();
		}

		public override void GrabAudio(float[] buffer, int floatCount, int channelCount)
		{
            Native.GrabAudio(_instance, buffer, floatCount, channelCount);
        }

		public override bool PlayerSupportsLinearColorSpace()
		{
			return _supportsLinearColorSpace;
		}

		//private static int _lastUpdateAllTexturesFrame = -1;

		private static void IssueRenderThreadEvent(Native.RenderThreadEvent renderEvent)
		{
			/*if (renderEvent == Native.RenderThreadEvent.UpdateAllTextures)
			{
				// We only want to update all textures once per frame
				if (_lastUpdateAllTexturesFrame == Time.frameCount)
					return;

				_lastUpdateAllTexturesFrame = Time.frameCount;
			}*/

#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
			if (renderEvent == Native.RenderThreadEvent.UpdateAllTextures)
			{
				GL.IssuePluginEvent(_nativeFunction_UpdateAllTextures, 0);
			}
			else if (renderEvent == Native.RenderThreadEvent.FreeTextures)
			{
				GL.IssuePluginEvent(_nativeFunction_FreeTextures, 0);
			}
			else if (renderEvent == Native.RenderThreadEvent.WaitForNewFrame)
			{
				GL.IssuePluginEvent(_nativeFunction_ExtractFrame, 0);
			}
#else
			GL.IssuePluginEvent(Native.PluginID | (int)renderEvent);
#endif
		}

		private static string GetPluginVersion()
		{
			return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPluginVersion());
		}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		public override void OnEnable()
		{
			base.OnEnable();

			if (_texture != null && _nativeTexture != System.IntPtr.Zero && _texture.GetNativeTexturePtr() == System.IntPtr.Zero)
			{
				_texture.UpdateExternalTexture(_nativeTexture);
			}
			_textureQuality = QualitySettings.masterTextureLimit;
		}
#endif

		private struct Native
		{
			public const int PluginID = 0xFA60000;

			public enum RenderThreadEvent
			{
				UpdateAllTextures,
				FreeTextures,
				WaitForNewFrame,
			}

			// Global

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool Init(bool linearColorSpace, bool isD3D11NoSingleThreaded);

			[DllImport("AVProVideo")]
			public static extern void Deinit();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetPluginVersion();

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsTrialVersion();

			// Open and Close

			[DllImport("AVProVideo")]
			public static extern System.IntPtr OpenSource(System.IntPtr instance, [MarshalAs(UnmanagedType.LPWStr)]string path, int videoApiIndex, bool useHardwareDecoding, 
				bool generateTextureMips, bool hintAlphaChannel, bool useLowLatency, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName,
				 bool useUnityAudio, bool forceResample, int sampleRate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters,
				 int audioChannelMode, uint sourceSampleRate, uint sourceChannels);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr OpenSourceFromBuffer(System.IntPtr instance, byte[] buffer, ulong bufferLength, int videoApiIndex, bool useHardwareDecoding,
				bool generateTextureMips, bool hintAlphaChannel, bool useLowLatency, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName, 
				bool useUnityAudio, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr StartOpenSourceFromBuffer(System.IntPtr instance, int videoApiIndex, ulong bufferLength);

			[DllImport("AVProVideo")]
			public static extern bool AddChunkToSourceBuffer(System.IntPtr instance, byte[] buffer, ulong offset, ulong chunkLength);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr EndOpenSourceFromBuffer(System.IntPtr instance, bool useHardwareDecoding, bool generateTextureMips, bool hintAlphaChannel, 
				bool useLowLatency, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName,	bool useUnityAudio, 
				[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters);

#if NETFX_CORE
			[DllImport("AVProVideo")]
			public static extern System.IntPtr OpenSourceFromStream(System.IntPtr instance, IRandomAccessStream ras, 
			[MarshalAs(UnmanagedType.LPWStr)]string path, int videoApiIndex, bool useHardwareDecoding, bool generateTextureMips, 
			bool hintAlphaChannel, bool useLowLatency, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName, bool useUnityAudio, bool forceResample, 
			int sampleRate, uint sourceSampleRate, uint sourceChannels);
#endif

			[DllImport("AVProVideo")]
			public static extern void CloseSource(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetPlayerDescription(System.IntPtr instance);

			// Errors

			[DllImport("AVProVideo")]
			public static extern int GetLastErrorCode(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern long GetLastExtendedErrorCode(System.IntPtr instance);

			// Controls

			[DllImport("AVProVideo")]
			public static extern void Play(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void Pause(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetMuted(System.IntPtr instance, bool muted);

			[DllImport("AVProVideo")]
			public static extern void SetVolume(System.IntPtr instance, float volume);

			[DllImport("AVProVideo")]
			public static extern void SetBalance(System.IntPtr instance, float volume);

			[DllImport("AVProVideo")]
			public static extern void SetLooping(System.IntPtr instance, bool looping);

			// Properties

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasVideo(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasAudio(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetWidth(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetHeight(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern float GetFrameRate(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern float GetDuration(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetAudioTrackCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsPlaybackStalled(System.IntPtr instance);

			// State

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasMetaData(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool CanPlay(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsSeeking(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsFinished(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsBuffering(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern float GetCurrentTime(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetCurrentTime(System.IntPtr instance, float time, bool fast);

			[DllImport("AVProVideo")]
			public static extern float GetPlaybackRate(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetPlaybackRate(System.IntPtr instance, float rate);

			[DllImport("AVProVideo")]
			public static extern int GetAudioTrack(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetAudioTrack(System.IntPtr instance, int index);

			[DllImport("AVProVideo")]
			public static extern float GetBufferingProgress(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetBufferedRanges(System.IntPtr instance, float[] timeArray, int arrayCount);

			[DllImport("AVProVideo")]
			public static extern void StartExtractFrame(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void WaitForExtract(System.IntPtr instance);

			// Update and Rendering

			[DllImport("AVProVideo")]
			public static extern void Update(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetTexturePointer(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsTextureTopDown(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetTextureFrameCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern long GetTextureTimeStamp(System.IntPtr instance);

#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_UpdateAllTextures();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_FreeTextures();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_WaitForNewFrame();
#endif

			// Audio

			[DllImport("AVProVideo")]
			public static extern void SetUnityAudioEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void GrabAudio(System.IntPtr instance, float[] buffer, int floatCount, int channelCount);

			[DllImport("AVProVideo")]
			public static extern int GetAudioChannelCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int SetAudioChannelMode(System.IntPtr instance, int channelMode);

			[DllImport("AVProVideo")]
			public static extern void SetHeadOrientation(System.IntPtr instance, float x, float y, float z, float w);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusProps(System.IntPtr instance, float offFocusLevel, float widthDegrees);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusRotation(System.IntPtr instance, float x, float y, float z, float w);
		}
	}
}
#endif