//#define AVPROVIDEO_DISABLE_DEBUG_GUI			// INTERNAL TESTING
//#define AVPROVIDEO_DEBUG_DISPLAY_EVENTS		// DEV FEATURE: show event logs in the gui display
//#define AVPROVIDEO_DEBUG_FRAMESYNC			// INTERNAL TESTING

using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Uses IMGUI to display a UI to show information about the MediaPlayer
	/// </summary>
	[AddComponentMenu("AVPro Video/Debug Overlay", -99)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class DebugOverlay : MonoBehaviour
	{
		[SerializeField]
		private MediaPlayer _mediaPlayer = null;

#pragma warning disable 414
		[SerializeField]
		private int _guiDepth = -1000;
	
		[SerializeField]
		private float _displaySize = 1f;

		private int _debugOverlayCount;
#pragma warning restore 414

		[SerializeField]
		private bool _displayControls = true;

		public bool DisplayControls
		{
			get { return _displayControls; }
			set { _displayControls = value; }
		}

		public MediaPlayer CurrentMediaPlayer
		{
			get
			{
				return _mediaPlayer;
			}
			set
			{
				if (_mediaPlayer != value)
				{
#if AVPROVIDEO_DEBUG_DISPLAY_EVENTS
					if (_mediaPlayer != null)
					{
						_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
					}
#endif
					_mediaPlayer = value;
				}
			}
		}		

		private const int s_GuiStartWidth = 10;
		private const int s_GuiWidth = 180;
#if AVPROVIDEO_DISABLE_DEBUG_GUI && !UNITY_EDITOR
#else
		private int m_GuiPositionX = s_GuiStartWidth;
#endif

		private void SetGuiPositionFromVideoIndex(int index)
		{
#if AVPROVIDEO_DISABLE_DEBUG_GUI && !UNITY_EDITOR
#else
			m_GuiPositionX = Mathf.FloorToInt((s_GuiStartWidth * _displaySize) + (s_GuiWidth * index * _displaySize));
#endif
		}

#if AVPROVIDEO_DEBUG_FRAMESYNC
		private int _lastFrameCount = 0;
		private int _sameFrameCount = 1;

		public int SameFrameCount
		{
			get { return _sameFrameCount; }
		}

		private void UpdateFrameSyncDebugging()
		{
			int frameCount = TextureProducer.GetTextureFrameCount();
			if (frameCount == _lastFrameCount)
			{
				_sameFrameCount++;
			}
			else
			{
				_sameFrameCount = 1;
			}
			_lastFrameCount = frameCount;
		}
#endif

#if AVPROVIDEO_DEBUG_DISPLAY_EVENTS
		private Queue<string> _eventLog = new Queue<string>(8);
		private float _eventTimer = 1f;

		private void AddEvent(MediaPlayerEvent.EventType et)
		{
			Helper.LogInfo("[MediaPlayer] Event: " + et.ToString(), this);
			_eventLog.Enqueue(et.ToString());
			if (_eventLog.Count > 5)
			{
				_eventLog.Dequeue();
				_eventTimer = 1f;
			}
		}

		private void UpdateEventLogs()
		{
			if (_eventLog != null && _eventLog.Count > 0)
			{
				_eventTimer -= Time.deltaTime;
				if (_eventTimer < 0f)
				{
					_eventLog.Dequeue();
					_eventTimer = 1f;
				}
			}
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			AddEvent(et);
		}
#endif

		private void Update()
		{
			_debugOverlayCount = 0;
#if AVPROVIDEO_DISABLE_DEBUG_GUI
			// Stub code so that the variables are used and don't produce a warning
			_guiDepth = -1000;
			_displaySize = 1f;
			_debugOverlayCount = 0;
#endif
		}

#if AVPROVIDEO_DISABLE_DEBUG_GUI && !UNITY_EDITOR
#else
		void OnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				SetGuiPositionFromVideoIndex(_debugOverlayCount++);
			}

			if (_mediaPlayer != null && _mediaPlayer.Info != null)
			{
				IMediaInfo info = _mediaPlayer.Info;
				IMediaControl control = _mediaPlayer.Control;
				IMediaProducer textureProducer = _mediaPlayer.TextureProducer;

				GUI.depth = _guiDepth;
				GUI.matrix = Matrix4x4.TRS(new Vector3(m_GuiPositionX, 10f, 0f), Quaternion.identity, new Vector3(_displaySize, _displaySize, 1.0f));

				GUILayout.BeginVertical("box", GUILayout.MaxWidth(s_GuiWidth));
				GUILayout.Label(System.IO.Path.GetFileName(_mediaPlayer.m_VideoPath));
				GUILayout.Label("Dimensions: " + info.GetVideoWidth() + "x" + info.GetVideoHeight() + "@" + info.GetVideoFrameRate().ToString("F2"));
				GUILayout.Label("Time: " + (control.GetCurrentTimeMs() * 0.001f).ToString("F1") + "s / " + (info.GetDurationMs() * 0.001f).ToString("F1") + "s");
				GUILayout.Label("Rate: " + info.GetVideoDisplayRate().ToString("F2") + "Hz");

				if (_mediaPlayer.m_Resample && _mediaPlayer.FrameResampler != null)
				{
					Resampler resampler = _mediaPlayer.FrameResampler;
					GUILayout.BeginVertical();
					GUILayout.Label("Resampler Info:");
					GUILayout.Label("Resampler timestamp: " + resampler.TextureTimeStamp);
					GUILayout.Label("Resampler frames dropped: " + resampler.DroppedFrames);
					GUILayout.Label("Resampler frame displayed timer: " + resampler.FrameDisplayedTimer);
					GUILayout.EndVertical();
				}

				if (textureProducer != null && textureProducer.GetTexture() != null)
				{
#if REAL_ANDROID
					// In OES mode we can't display the texture without using a special shader, so just don't display it
					if (!_optionsAndroid.useFastOesPath)
#endif
					{
						// Show texture without and with alpha blending
						GUILayout.BeginHorizontal();
						Rect r1 = GUILayoutUtility.GetRect(32f, 32f);
						GUILayout.Space(8f);
						Rect r2 = GUILayoutUtility.GetRect(32f, 32f);
						Matrix4x4 prevMatrix = GUI.matrix;
						if (textureProducer.RequiresVerticalFlip())
						{
							GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, r1.y + (r1.height / 2f)));
						}
						GUI.DrawTexture(r1, textureProducer.GetTexture(), ScaleMode.ScaleToFit, false);
						GUI.DrawTexture(r2, textureProducer.GetTexture(), ScaleMode.ScaleToFit, true);
						GUI.matrix = prevMatrix;
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}
				}

				if (_displayControls)
				{
					GUILayout.BeginHorizontal();
					if (control.IsPaused())
					{
						if (GUILayout.Button("Play", GUILayout.Width(50)))
						{
							control.Play();
						}
					}
					else
					{
						if (GUILayout.Button("Pause", GUILayout.Width(50)))
						{
							control.Pause();
						}
					}

					float duration = info.GetDurationMs();
					float time = control.GetCurrentTimeMs();
					float newTime = GUILayout.HorizontalSlider(time, 0f, duration);
					if (newTime != time)
					{
						control.Seek(newTime);
					}
					GUILayout.EndHorizontal();
				}

#if AVPROVIDEO_DEBUG_DISPLAY_EVENTS
				// Dirty code to hack in an event monitor
				if (Event.current.type == EventType.Repaint)
				{
					_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
					_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
					UpdateEventLogs();
				}

				if (_eventLog != null && _eventLog.Count > 0)
				{
					GUILayout.Label("Recent Events: ");
					GUILayout.BeginVertical("box");
					int eventIndex = 0;
					foreach (string eventString in _eventLog)
					{
						GUI.color = Color.white;
						if (eventIndex == 0)
						{
							GUI.color = new Color(1f, 1f, 1f, _eventTimer);
						}
						GUILayout.Label(eventString);
						eventIndex++;
					}
					GUILayout.EndVertical();
					GUI.color = Color.white;
				}
#endif
				GUILayout.EndVertical();
			}
		}
#endif
	}
}