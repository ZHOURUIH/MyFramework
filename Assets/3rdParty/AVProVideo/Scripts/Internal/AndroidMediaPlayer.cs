#define DLL_METHODS

#if UNITY_ANDROID
#if UNITY_5 || UNITY_5_4_OR_NEWER
	#if !UNITY_5_0 && !UNITY_5_1
		#define AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
	#endif
	#if !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4_0 && !UNITY_5_4_1
		#define AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
	#endif
#endif

using UnityEngine;
using System;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Android implementation of BaseMediaPlayer
	/// </summary>
	// TODO: seal this class
	public class AndroidMediaPlayer : BaseMediaPlayer
	{
        protected static AndroidJavaObject	s_ActivityContext	= null;
		protected static AndroidJavaObject  s_Interface			= null;
        protected static bool				s_bInitialised		= false;

		private static string				s_Version = "Plug-in not yet initialised";

#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
		private static System.IntPtr 		_nativeFunction_RenderEvent = System.IntPtr.Zero;
#endif
		protected AndroidJavaObject			m_Video;
		private Texture2D					m_Texture;
        private int                         m_TextureHandle;
		private bool						m_UseFastOesPath;

		private float						m_DurationMs		= 0.0f;
		private int							m_Width				= 0;
		private int							m_Height			= 0;

		protected int 						m_iPlayerIndex		= -1;

		private Android.VideoApi			m_API;
		private bool						m_HeadRotationEnabled = false;
		private bool						m_FocusEnabled = false;
		private System.IntPtr 				m_Method_Update;
		private System.IntPtr 				m_Method_SetHeadRotation;
		private System.IntPtr				m_Method_GetCurrentTimeMs;
		private System.IntPtr				m_Method_GetSourceVideoFrameRate;
		private System.IntPtr				m_Method_IsPlaying;
		private System.IntPtr				m_Method_IsPaused;
		private System.IntPtr				m_Method_IsFinished;
		private System.IntPtr				m_Method_IsSeeking;
		private System.IntPtr				m_Method_IsBuffering;
		private System.IntPtr				m_Method_IsLooping;
		private System.IntPtr				m_Method_HasVideo;
		private System.IntPtr				m_Method_HasAudio;
		private System.IntPtr				m_Method_SetFocusProps;
		private System.IntPtr				m_Method_SetFocusEnabled;
		private System.IntPtr				m_Method_SetFocusRotation;
		private jvalue[]					m_Value0 = new jvalue[0];
		private jvalue[]					m_Value1 = new jvalue[1];
		private jvalue[]					m_Value2 = new jvalue[2];
		private jvalue[]					m_Value4 = new jvalue[4];

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		private int _textureQuality = QualitySettings.masterTextureLimit;
#endif
		public static bool InitialisePlatform()
		{
#if UNITY_5_4_OR_NEWER
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
			{
				Debug.LogError("[AVProVideo] Vulkan graphics API is not supported");
				return false;
			}
#endif

			// Get the activity context
			if (s_ActivityContext == null)
            {
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                if (activityClass != null)
                {
                    s_ActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
				}
			}

			if (!s_bInitialised)
			{
				s_Interface = new AndroidJavaObject("com.RenderHeads.AVProVideo.AVProMobileVideo");
				if (s_Interface != null)
				{
					s_Version = s_Interface.Call<string>("GetPluginVersion");
					s_Interface.Call("SetContext", s_ActivityContext);

					// Calling this native function cause the .SO library to become loaded
					// This is important for Unity < 5.2.0 where GL.IssuePluginEvent works differently
					#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
					_nativeFunction_RenderEvent = Native.GetRenderEventFunc();
					#else
					Native.GetRenderEventFunc();
					#endif

					s_bInitialised = true;
				}
			}

			return s_bInitialised;
		}

		public static void DeinitPlatform()
		{
			if (s_bInitialised)
			{
				if (s_Interface != null)
				{
					s_Interface.CallStatic("Deinitialise");
					s_Interface = null;
				}
				s_ActivityContext = null;
				s_bInitialised = false;
			}
		}

		private static void IssuePluginEvent(Native.AVPPluginEvent type, int param)
		{
			// Build eventId from the type and param.
			int eventId = 0x5d5ac000 | ((int)type << 8);

			switch (type)
			{
				case Native.AVPPluginEvent.PlayerSetup:
				case Native.AVPPluginEvent.PlayerUpdate:
				case Native.AVPPluginEvent.PlayerDestroy:
				case Native.AVPPluginEvent.ExtractFrame:
					{
						eventId |= param & 0xff;
					}
					break;
			}

#if AVPROVIDEO_ISSUEPLUGINEVENT_UNITY52
			GL.IssuePluginEvent(_nativeFunction_RenderEvent, eventId);
#else
			GL.IssuePluginEvent(eventId);
#endif
		}

		private System.IntPtr GetMethod(string methodName, string signature)
		{
#if UNITY_5 || UNITY_5_4_OR_NEWER
			Debug.Assert(m_Video != null);
#endif
			 System.IntPtr result = AndroidJNIHelper.GetMethodID(m_Video.GetRawClass(), methodName, signature, false);

#if UNITY_5 || UNITY_5_4_OR_NEWER
			Debug.Assert(result != System.IntPtr.Zero);
#endif
			if (result == System.IntPtr.Zero)
			{
				Debug.LogError("[AVProVideo] Unable to get method " + methodName + " " + signature);
				throw new System.Exception("[AVProVideo] Unable to get method " + methodName + " " + signature);
			}

			 return result;
		}

		public AndroidMediaPlayer(bool useFastOesPath, bool showPosterFrame, Android.VideoApi api, bool enable360Audio, Audio360ChannelMode channelMode, bool preferSoftware)
		{
#if UNITY_5 || UNITY_5_4_OR_NEWER
			Debug.Assert(s_Interface != null);
			Debug.Assert(s_bInitialised);
#endif
			m_API = api;
			// Create a java-size video class up front
			m_Video = s_Interface.Call<AndroidJavaObject>("CreatePlayer", (int)m_API, useFastOesPath, enable360Audio, (int)channelMode, preferSoftware);

			if (m_Video != null)
            {
				m_Method_Update = GetMethod("Update", "()V");
				m_Method_SetHeadRotation = GetMethod("SetHeadRotation", "(FFFF)V");
				m_Method_SetFocusProps = GetMethod("SetFocusProps", "(FF)V");
				m_Method_SetFocusEnabled = GetMethod("SetFocusEnabled", "(Z)V");
				m_Method_SetFocusRotation = GetMethod("SetFocusRotation", "(FFFF)V");
				m_Method_GetCurrentTimeMs = GetMethod("GetCurrentTimeMs", "()J");
				m_Method_GetSourceVideoFrameRate = GetMethod("GetSourceVideoFrameRate", "()F");
				m_Method_IsPlaying = GetMethod("IsPlaying", "()Z");
				m_Method_IsPaused = GetMethod("IsPaused", "()Z");
				m_Method_IsFinished = GetMethod("IsFinished", "()Z");
				m_Method_IsSeeking = GetMethod("IsSeeking", "()Z");
				m_Method_IsBuffering = GetMethod("IsBuffering", "()Z");
				m_Method_IsLooping = GetMethod("IsLooping", "()Z");
				m_Method_HasVideo = GetMethod("HasVideo", "()Z");
				m_Method_HasAudio = GetMethod("HasAudio", "()Z");

				m_iPlayerIndex = m_Video.Call<int>("GetPlayerIndex");
				Helper.LogInfo("Creating player " + m_iPlayerIndex);
				//Debug.Log( "AVPro: useFastOesPath: " + useFastOesPath );
				SetOptions(useFastOesPath, showPosterFrame);

				// Initialise renderer, on the render thread
				AndroidMediaPlayer.IssuePluginEvent(Native.AVPPluginEvent.PlayerSetup, m_iPlayerIndex);
            }
			else
			{
				Debug.LogError("[AVProVideo] Failed to create player instance");
			}
        }

		public void SetOptions(bool useFastOesPath, bool showPosterFrame)
		{
			m_UseFastOesPath = useFastOesPath;
			if (m_Video != null)
			{
				// Show poster frame is only needed when using the MediaPlayer API
				showPosterFrame = (m_API == Android.VideoApi.MediaPlayer) ? showPosterFrame:false;

				m_Video.Call("SetPlayerOptions", m_UseFastOesPath, showPosterFrame);
			}
		}

		public override long GetEstimatedTotalBandwidthUsed()
		{
			long result = -1;
			if (s_Interface != null)
			{
				result = m_Video.Call<long>("GetEstimatedBandwidthUsed");
			}
			return result;
		}


		public override string GetVersion()
		{
			return s_Version;
		}

		public override bool OpenVideoFromFile(string path, long offset, string httpHeaderJson, uint sourceSamplerate = 0, uint sourceChannels = 0, int forceFileFormat = 0)
		{
			bool bReturn = false;

			if( m_Video != null )
			{
#if UNITY_5 || UNITY_5_4_OR_NEWER
				Debug.Assert(m_Width == 0 && m_Height == 0 && m_DurationMs == 0.0f);
#endif

				bReturn = m_Video.Call<bool>("OpenVideoFromFile", path, offset, httpHeaderJson, forceFileFormat);
			}
			else
			{
				Debug.LogError("[AVProVideo] m_Video is null!");
			}

			return bReturn;
		}

		public override TimeRange[] GetSeekableTimeRanges()
		{
			float[] rangeArray = m_Video.Call<float[]>("GetSeekableTimeRange");

			TimeRange[] result = new TimeRange[1];
			result[0].startTime = rangeArray[0];
			result[0].duration = rangeArray[1] - rangeArray[0];

			return result;
		}

		public override void CloseVideo()
        {
			if (m_Texture != null)
            {
                Texture2D.Destroy(m_Texture);
                m_Texture = null;
            }
            m_TextureHandle = 0;

            m_DurationMs = 0.0f;
            m_Width = 0;
            m_Height = 0;

			if (m_Video != null)
			{
				m_Video.Call("CloseVideo");
			}

			base.CloseVideo();
		}

        public override void SetLooping( bool bLooping )
		{
			if( m_Video != null )
			{
				m_Video.Call("SetLooping", bLooping);
			}
		}

		public override bool IsLooping()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_IsLooping != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsLooping, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsLooping");
				}
			}
			return result;
		}

		public override bool HasVideo()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_HasVideo != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_HasVideo, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("HasVideo");
				}
			}
			return result;
		}

		public override bool HasAudio()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_HasAudio != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_HasAudio, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("HasAudio");
				}
			}
			return result;
		}

		public override bool HasMetaData()
		{
			bool result = false;
			if( m_DurationMs > 0.0f )
			{
				result = true;

				if( HasVideo() )
				{
					result = ( m_Width > 0 && m_Height > 0 );
				}
			}
			return result;
		}

		public override bool CanPlay()
		{
			bool result = false;
#if DLL_METHODS
			result = Native._CanPlay( m_iPlayerIndex );
#else
			if (m_Video != null)
			{
				result = m_Video.Call<bool>("CanPlay");
			}
#endif
			return result;
		}

		public override void Play()
		{
			if (m_Video != null)
			{
				m_Video.Call("Play");
			}
		}

		public override void Pause()
		{
			if (m_Video != null)
			{
				m_Video.Call("Pause");
			}
		}

		public override void Stop()
		{
			if (m_Video != null)
			{
				// On Android we never need to actually Stop the playback, pausing is fine
				m_Video.Call("Pause");
			}
		}

		public override void Seek(float timeMs)
		{
			if (m_Video != null)
			{
				m_Video.Call("Seek", Mathf.FloorToInt(timeMs));
			}
		}

		public override void SeekFast(float timeMs)
		{
			if (m_Video != null)
			{
				m_Video.Call("SeekFast", Mathf.FloorToInt(timeMs));
			}
		}

		public override float GetCurrentTimeMs()
		{
			float result = 0.0f;
			if (m_Video != null)
			{
				if (m_Method_GetCurrentTimeMs != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallLongMethod(m_Video.GetRawObject(), m_Method_GetCurrentTimeMs, m_Value0);
				}
				else
				{
					result = (float)m_Video.Call<long>("GetCurrentTimeMs");
				}
			}
			return result;
		}

		public override void SetPlaybackRate(float rate)
		{
			if (m_Video != null)
			{
				m_Video.Call("SetPlaybackRate", rate);
			}
		}

		public override float GetPlaybackRate()
		{
			float result = 0.0f;
			if (m_Video != null)
			{
				result = m_Video.Call<float>("GetPlaybackRate");
			}
			return result;
		}

		public override void SetAudioHeadRotation(Quaternion q)
		{
			if (m_Video != null)
			{
				if (!m_HeadRotationEnabled)
				{
					m_Video.Call("SetPositionTrackingEnabled", true);
					m_HeadRotationEnabled = true;
				}

				if (m_Method_SetHeadRotation != System.IntPtr.Zero)
				{
					m_Value4[0].f = q.x;
					m_Value4[1].f = q.y;
					m_Value4[2].f = q.z;
					m_Value4[3].f = q.w;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetHeadRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetHeadRotation", q.x, q.y, q.z, q.w);
				}
			}
		}

		public override void ResetAudioHeadRotation()
		{
			if(m_Video != null && m_HeadRotationEnabled)
			{
				m_Video.Call("SetPositionTrackingEnabled", false);
				m_HeadRotationEnabled = false;
			}
		}

		public override void SetAudioFocusEnabled(bool enabled)
		{
			if (m_Video != null && enabled != m_FocusEnabled)
			{
				if (m_Method_SetFocusEnabled != System.IntPtr.Zero)
				{
					m_Value1[0].z = enabled;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusEnabled, m_Value1);
				}
				else
				{
					m_Video.Call("SetFocusEnabled", enabled);
				}
				m_FocusEnabled = enabled;
			}
		}

		public override void SetAudioFocusProperties(float offFocusLevel, float widthDegrees)
		{
			if(m_Video != null && m_FocusEnabled)
			{
				if (m_Method_SetFocusProps != System.IntPtr.Zero)
				{
					m_Value2[0].f = offFocusLevel;
					m_Value2[1].f = widthDegrees;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusProps, m_Value2);
				}
				else
				{
					m_Video.Call("SetFocusProps", offFocusLevel, widthDegrees);
				}
			}
		}

		public override void SetAudioFocusRotation(Quaternion q)
		{
			if (m_Video != null && m_FocusEnabled)
			{
				if (m_Method_SetFocusRotation != System.IntPtr.Zero)
				{
					m_Value4[0].f = q.x;
					m_Value4[1].f = q.y;
					m_Value4[2].f = q.z;
					m_Value4[3].f = q.w;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetFocusRotation", q.x, q.y, q.z, q.w);
				}
			}
		}

		public override void ResetAudioFocus()
		{
			if (m_Video != null)
			{

				if (m_Method_SetFocusProps != System.IntPtr.Zero &&
					m_Method_SetFocusEnabled != System.IntPtr.Zero &&
					m_Method_SetFocusRotation != System.IntPtr.Zero)
				{
					m_Value2[0].f = 0f;
					m_Value2[1].f = 90f;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusProps, m_Value2);
					m_Value1[0].z = false;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusEnabled, m_Value1);
					m_Value4[0].f = 0f;
					m_Value4[1].f = 0f;
					m_Value4[2].f = 0f;
					m_Value4[3].f = 1f;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetFocusProps", 0f, 90f);
					m_Video.Call("SetFocusEnabled", false);
					m_Video.Call("SetFocusRotation", 0f, 0f, 0f, 1f);
				}
			}
		}

		public override float GetDurationMs()
		{
			return m_DurationMs;
		}

		public override int GetVideoWidth()
		{
			return m_Width;
		}
			
		public override int GetVideoHeight()
		{
			return m_Height;
		}

		public override float GetVideoFrameRate()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				if (m_Method_GetSourceVideoFrameRate != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallFloatMethod(m_Video.GetRawObject(), m_Method_GetSourceVideoFrameRate, m_Value0);
				}
				else
				{
					result = m_Video.Call<float>("GetSourceVideoFrameRate");
				}
			}
			return result;
		}

		public override float GetBufferingProgress()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				result = m_Video.Call<float>("GetBufferingProgressPercent") * 0.01f;
			}
			return result;
		}

		public override float GetVideoDisplayRate()
		{
			float result = 0.0f;
#if DLL_METHODS
			result = Native._GetVideoDisplayRate( m_iPlayerIndex );
#else
			if (m_Video != null)
			{
				result = m_Video.Call<float>("GetDisplayRate");
			}
#endif
			return result;
		}

		public override bool IsSeeking()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsSeeking != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsSeeking, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsSeeking");
				}
			}
			return result;
		}

		public override bool IsPlaying()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsPlaying != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsPlaying, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsPlaying");
				}
			}
			return result;
		}

		public override bool IsPaused()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsPaused != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsPaused, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsPaused");
				}
			}
			return result;
		}

		public override bool IsFinished()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsFinished != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsFinished, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsFinished");
				}
			}
			return result;
		}

		public override bool IsBuffering()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsBuffering != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsBuffering, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsBuffering");
				}
			}
			return result;
		}

		public override Texture GetTexture( int index )
		{
			Texture result = null;
			if (GetTextureFrameCount() > 0)
			{
				result = m_Texture;
			}
			return result;
		}

		public override int GetTextureFrameCount()
		{
			int result = 0;
			if( m_Texture != null )
			{
#if DLL_METHODS
				result = Native._GetFrameCount( m_iPlayerIndex );
#else
				if (m_Video != null)
				{
					result = m_Video.Call<int>("GetFrameCount");
				}
#endif
			}
			return result;
		}

		public override bool RequiresVerticalFlip()
		{
			return false;
		}

		public override void MuteAudio(bool bMuted)
		{
			if (m_Video != null)
			{
				m_Video.Call("MuteAudio", bMuted);
			}
		}

		public override bool IsMuted()
		{
			bool result = false;
			if( m_Video != null )
			{
				result = m_Video.Call<bool>("IsMuted");
			}
			return result;
		}

		public override void SetVolume(float volume)
		{
			if (m_Video != null)
			{
				m_Video.Call("SetVolume", volume);
			}
		}

		public override float GetVolume()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				result = m_Video.Call<float>("GetVolume");
			}
			return result;
		}

		public override void SetBalance(float balance)
		{
			if( m_Video != null )
			{
				m_Video.Call("SetAudioPan", balance);
			}
		}

		public override float GetBalance()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				result = m_Video.Call<float>("GetAudioPan");
			}
			return result;
		}

		public override int GetAudioTrackCount()
		{
			int result = 0;
			if( m_Video != null )
			{
				result = m_Video.Call<int>("GetNumberAudioTracks");
			}
			return result;
		}

		public override int GetCurrentAudioTrack()
		{
			int result = 0;
			if( m_Video != null )
			{
				result = m_Video.Call<int>("GetCurrentAudioTrackIndex");
			}
			return result;
		}

		public override void SetAudioTrack( int index )
		{
			if( m_Video != null )
			{
				m_Video.Call("SetAudioTrack", index);
			}
		}

		public override string GetCurrentAudioTrackId()
		{
			/*string id = "";
			if( m_Video != null )
			{
				id = m_Video.Call<string>("GetCurrentAudioTrackIndex");
			}
			return id;*/

			return GetCurrentAudioTrack().ToString();
		}

		public override int GetCurrentAudioTrackBitrate()
		{
			int result = 0;
			/*if( m_Video != null )
			{
				result = m_Video.Call<int>("GetCurrentAudioTrackIndex");
			}*/
			return result;
		}

		public override int GetVideoTrackCount()
		{
			int result = 0;
			if( m_Video != null )
			{
				if (HasVideo())
				{
					result = 1;
				}
				//result = m_Video.Call<int>("GetNumberVideoTracks");
			}
			return result;
		}

		public override int GetCurrentVideoTrack()
		{
			int result = 0;
			/*if( m_Video != null )
			{
				result = m_Video.Call<int>("GetCurrentVideoTrackIndex");
			}*/
			return result;
		}

		public override void SetVideoTrack( int index )
		{
			/*if( m_Video != null )
			{
				m_Video.Call("SetVideoTrack", index);
			}*/
		}

		public override string GetCurrentVideoTrackId()
		{
			string id = "";
			/*if( m_Video != null )
			{
				id = m_Video.Call<string>("GetCurrentVideoTrackId");
			}*/
			return id;
		}

		public override int GetCurrentVideoTrackBitrate()
		{
			int bitrate = 0;
			/*if( m_Video != null )
			{
				bitrate = m_Video.Call<int>("GetCurrentVideoTrackBitrate");
			}*/
			return bitrate;
		}

		public override bool WaitForNextFrame(Camera dummyCamera, int previousFrameCount)
		{
			// Mark as extracting
			bool isMultiThreaded = m_Video.Call<bool>("StartExtractFrame");

			// In single threaded Android this method won't work, so just return
			if (isMultiThreaded)
			{
				// Queue up render thread event to wait for the new frame
				IssuePluginEvent(Native.AVPPluginEvent.ExtractFrame, m_iPlayerIndex);

				// Force render thread to run
				dummyCamera.Render();

				// Wait for the frame to change
				m_Video.Call("WaitForExtract");

				// Return whether the frame changed
				return (previousFrameCount != GetTextureFrameCount());
			}
			return false;	
		}

		public override long GetTextureTimeStamp()
        {
            long timeStamp = long.MinValue;
            if (m_Video != null)
            {
                timeStamp = m_Video.Call<long>("GetTextureTimeStamp");
            }
            return timeStamp;
        }

        public override void Render()
		{
			if (m_Video != null)
			{
				if (m_UseFastOesPath)
				{
					// This is needed for at least Unity 5.5.0, otherwise it just renders black in OES mode
					GL.InvalidateState();
				}
				AndroidMediaPlayer.IssuePluginEvent( Native.AVPPluginEvent.PlayerUpdate, m_iPlayerIndex );
				if (m_UseFastOesPath)
				{
					GL.InvalidateState();
				}

				// Check if we can create the texture
                // Scan for a change in resolution
				int newWidth = -1;
				int newHeight = -1;
                if (m_Texture != null)
                {
#if DLL_METHODS
                    newWidth = Native._GetWidth( m_iPlayerIndex );
                    newHeight = Native._GetHeight( m_iPlayerIndex );
#else
                    newWidth = m_Video.Call<int>("GetWidth");
                    newHeight = m_Video.Call<int>("GetHeight");
#endif
                    if (newWidth != m_Width || newHeight != m_Height)
                    {
                        m_Texture = null;
                        m_TextureHandle = 0;
                    }
                }
#if DLL_METHODS
                int textureHandle = Native._GetTextureHandle( m_iPlayerIndex );
#else
                int textureHandle = m_Video.Call<int>("GetTextureHandle");
#endif
				if (textureHandle != 0 && textureHandle != m_TextureHandle )
				{
					// Already got? (from above)
					if( newWidth == -1 || newHeight == -1 )
                    {
#if DLL_METHODS
						newWidth = Native._GetWidth( m_iPlayerIndex );
						newHeight = Native._GetHeight( m_iPlayerIndex );
#else
						newWidth = m_Video.Call<int>("GetWidth");
						newHeight = m_Video.Call<int>("GetHeight");
#endif
					}

					if (Mathf.Max(newWidth, newHeight) > SystemInfo.maxTextureSize)
					{
						m_Width = newWidth;
						m_Height = newHeight;
	                    m_TextureHandle = textureHandle;
						Debug.LogError("[AVProVideo] Video dimensions larger than maxTextureSize");
					}
					else if( newWidth > 0 && newHeight > 0 )
					{
						m_Width = newWidth;
						m_Height = newHeight;
	                    m_TextureHandle = textureHandle;
						
						switch(m_API)
						{
							case Android.VideoApi.MediaPlayer:
								_playerDescription = "MediaPlayer";
								break;
							case Android.VideoApi.ExoPlayer:
								_playerDescription = "ExoPlayer";
								break;
							default:
								_playerDescription = "UnknownPlayer";
								break;
						}

						if (m_UseFastOesPath)
						{
							_playerDescription += " OES";
						}
						else
						{
							_playerDescription += " NonOES";
						}

						Helper.LogInfo("Using playback path: " + _playerDescription + " (" + m_Width + "x" + m_Height + "@" + GetVideoFrameRate().ToString("F2") + ")");

						// NOTE: From Unity 5.4.x when using OES textures, an error "OPENGL NATIVE PLUG-IN ERROR: GL_INVALID_OPERATION: Operation illegal in current state" will be logged.
						// We assume this is because we're passing in TextureFormat.RGBA32 which isn't the true texture format.  This error should be safe to ignore.
						m_Texture = Texture2D.CreateExternalTexture(m_Width, m_Height, TextureFormat.RGBA32, false, false, new System.IntPtr(textureHandle));
						if (m_Texture != null)
						{
							ApplyTextureProperties(m_Texture);
						}
						Helper.LogInfo("Texture ID: " + textureHandle);
					}
				}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
				// In Unity 5.4.2 and above the video texture turns black when changing the TextureQuality in the Quality Settings
				// The code below gets around this issue.  A bug report has been sent to Unity.  So far we have tested and replicated the
				// "bug" in Windows only, but a user has reported it in Android too.  
				// Texture.GetNativeTexturePtr() must sync with the rendering thread, so this is a large performance hit!
				if (_textureQuality != QualitySettings.masterTextureLimit)
				{
					if (m_Texture != null && textureHandle > 0 && m_Texture.GetNativeTexturePtr() == System.IntPtr.Zero)
					{
						//Debug.Log("RECREATING");
						m_Texture.UpdateExternalTexture(new System.IntPtr(textureHandle));
					}

					_textureQuality = QualitySettings.masterTextureLimit;
				}
#endif
			}
		}

		protected override void ApplyTextureProperties(Texture texture)
		{
			// NOTE: According to OES_EGL_image_external: For external textures, the default min filter is GL_LINEAR and the default S and T wrap modes are GL_CLAMP_TO_EDGE
			// See https://www.khronos.org/registry/gles/extensions/OES/OES_EGL_image_external_essl3.txt
			// But there is a new extension that allows some wrap modes:
			// https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_EGL_image_external_wrap_modes.txt
			// So really we need to detect whether these extensions exist when m_UseFastOesPath is true
			//if (!m_UseFastOesPath)
			{
				base.ApplyTextureProperties(texture);
			}
		}

		public override void OnEnable()
		{
			base.OnEnable();

#if DLL_METHODS
			int textureHandle = Native._GetTextureHandle(m_iPlayerIndex);
#else
            int textureHandle = m_Video.Call<int>("GetTextureHandle");
#endif

			if (m_Texture != null && textureHandle > 0 && m_Texture.GetNativeTexturePtr() == System.IntPtr.Zero)
			{
				//Debug.Log("RECREATING");
				m_Texture.UpdateExternalTexture(new System.IntPtr(textureHandle));
			}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
			_textureQuality = QualitySettings.masterTextureLimit;
#endif
		}

		public override double GetCurrentDateTimeSecondsSince1970()
		{
            double result = 0.0;
			if (m_Video != null)
            {
				result = m_Video.Call<double>("GetCurrentAbsoluteTimestamp");
			}
			return result;
		}

		public override void Update()
		{
			if (m_Video != null)
			{
				if (m_Method_Update != System.IntPtr.Zero)
				{
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_Update, m_Value0);
				}
				else
				{
					m_Video.Call("Update");
				}
				
				//				_lastError = (ErrorCode)( m_Video.Call<int>("GetLastErrorCode") );
				_lastError = (ErrorCode)( Native._GetLastErrorCode( m_iPlayerIndex) );
			}

			UpdateSubtitles();

			if(Mathf.Approximately(m_DurationMs, 0f))
			{
#if DLL_METHODS
				m_DurationMs = (float)( Native._GetDuration( m_iPlayerIndex ) );
#else
				m_DurationMs = (float)(m_Video.Call<long>("GetDurationMs"));
#endif
//				if( m_DurationMs > 0.0f ) { Helper.LogInfo("Duration: " + m_DurationMs); }
			}
		}

		public override bool PlayerSupportsLinearColorSpace()
		{
			return false;
		}

		public override float[] GetTextureTransform()
		{
			float[] transform = null;
			if (m_Video != null)
			{
				transform = m_Video.Call<float[]>("GetTextureTransform");
				/*if (transform != null)
				{
					Debug.Log("xform: " + transform[0] + " " + transform[1] + " " + transform[2] + " " + transform[3] + " " + transform[4] + " " + transform[5]);
				}*/
			}
			return transform;
		}

		public override void Dispose()
		{
			//Debug.LogError("DISPOSE");

			if (m_Video != null)
			{
				m_Video.Call("SetDeinitialiseFlagged");

				m_Video.Dispose();
				m_Video = null;
			}

			if (s_Interface != null)
			{
				s_Interface.Call("DestroyPlayer", m_iPlayerIndex);
			}

			if (m_Texture != null)
			{
				Texture2D.Destroy(m_Texture);
				m_Texture = null;
			}

			// Deinitialise player (replaces call directly as GL textures are involved)
			AndroidMediaPlayer.IssuePluginEvent( Native.AVPPluginEvent.PlayerDestroy, m_iPlayerIndex );
		}

		private struct Native
		{
			[DllImport ("AVProLocal")]
			public static extern IntPtr GetRenderEventFunc();

			[DllImport ("AVProLocal")]
			public static extern int _GetWidth( int iPlayerIndex );

			[DllImport ("AVProLocal")]
			public static extern int _GetHeight( int iPlayerIndex );
			
			[DllImport ("AVProLocal")]
			public static extern int _GetTextureHandle( int iPlayerIndex );

			[DllImport ("AVProLocal")]
			public static extern long _GetDuration( int iPlayerIndex );

			[DllImport ("AVProLocal")]
			public static extern int _GetLastErrorCode( int iPlayerIndex );

			[DllImport ("AVProLocal")]
			public static extern int _GetFrameCount( int iPlayerIndex );
		
			[DllImport ("AVProLocal")]
			public static extern float _GetVideoDisplayRate( int iPlayerIndex );

			[DllImport ("AVProLocal")]
			public static extern bool _CanPlay( int iPlayerIndex );
			
			public enum AVPPluginEvent
			{
				Nop,
				PlayerSetup,
				PlayerUpdate,
				PlayerDestroy,
				ExtractFrame,
			}
		}
	}
}
#endif