using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	[System.Serializable]
	public class MediaPlaylist
	{
		[System.Serializable]
		public class MediaItem
		{
			/*public enum SourceType
			{
				AVProMediaPlayer,
				Texture2D,
			}

			[SerializeField]
			public SourceType sourceType;*/

			[SerializeField]
			public MediaPlayer.FileLocation fileLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;

			[SerializeField]
			public string filePath;

			/*[SerializeField]
			public Texture2D texture;

			[SerializeField]
			public float textureDuration;*/

			[SerializeField]
			public bool loop = false;

			[SerializeField]
			public PlaylistMediaPlayer.StartMode startMode = PlaylistMediaPlayer.StartMode.Immediate;

			[SerializeField]
			public PlaylistMediaPlayer.ProgressMode progressMode = PlaylistMediaPlayer.ProgressMode.OnFinish;

			[SerializeField]
			public float progressTimeSeconds = 0.5f;

			[SerializeField]
			public bool autoPlay = true;

			[SerializeField]
			public StereoPacking stereoPacking = StereoPacking.None;

			[SerializeField]
			public AlphaPacking alphaPacking = AlphaPacking.None;

			[SerializeField]
			public bool isOverrideTransition = false;

			[SerializeField]
			public PlaylistMediaPlayer.Transition overrideTransition = PlaylistMediaPlayer.Transition.None;

			[SerializeField]
			public float overrideTransitionDuration = 1f;

			[SerializeField]
			public PlaylistMediaPlayer.Easing overrideTransitionEasing;
		}

		[SerializeField]
		private List<MediaItem> _items = new List<MediaItem>(8);

		public List<MediaItem> Items { get { return _items; } }

		public bool HasItemAt(int index)
		{
			return (index >= 0 && index < _items.Count);
		}
	}

	/// <summary>
	/// This is a BETA component
	/// </summary>
	[AddComponentMenu("AVPro Video/Playlist Media Player (BETA)", -100)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class PlaylistMediaPlayer : MediaPlayer, IMediaProducer
	{
		public enum Transition
		{
			None,
			Fade,
			Black,
			White,
			Transparent,
			Horiz,
			Vert,
			Diag,
			MirrorH,
			MirrorV,
			MirrorD,
			ScrollV,
			ScrollH,
			Circle,
			Diamond,
			Blinds,
			Arrows,
			SlideH,
			SlideV,
			Zoom,
			RectV,
			Random,
		}

		public enum PlaylistLoopMode
		{
			None,
			Loop,
		}

		public enum StartMode
		{
			Immediate,
			//AfterSeconds,
			Manual,
		}

		public enum ProgressMode
		{
			OnFinish,
			BeforeFinish,
			//AfterTime,
			Manual,
		}
			
		[SerializeField]
		private MediaPlayer _playerA = null;
		
		[SerializeField]
		private MediaPlayer _playerB = null;

		[SerializeField]
		private bool _playlistAutoProgress = true;

		[SerializeField]
		private PlaylistLoopMode _playlistLoopMode = PlaylistLoopMode.None;

		[SerializeField]
		private MediaPlaylist _playlist = new MediaPlaylist();

		[SerializeField]
		[Tooltip("Pause the previously playing video. This is useful for systems that will struggle to play 2 videos at once")]
		private bool _pausePreviousOnTransition = true;

		[SerializeField]
		private Transition _nextTransition = Transition.None;

		[SerializeField]
		private float _transitionDuration = 1f;

		[SerializeField]
		private Easing _transitionEasing = null;

		private static int _propFromTex;
		private static int _propT;

		private int _playlistIndex = 0;
		private MediaPlayer _nextPlayer;
		private Shader _shader;
		private Material _material;
		private Transition _currentTransition = Transition.None;
		private string _currentTransitionName = "LERP_NONE";
		private float _currentTransitionDuration = 1f;
		private Easing.Preset _currentTransitionEasing;
		private float _transitionTimer;
		private System.Func<float, float> _easeFunc;
		private RenderTexture _rt;
		private MediaPlaylist.MediaItem _currentItem;
		private MediaPlaylist.MediaItem _nextItem;

		public MediaPlayer CurrentPlayer
		{
			get
			{
				if (NextPlayer == _playerA)
				{
					return _playerB;
				}
				return _playerA;
			}
		}

		public MediaPlayer NextPlayer
		{
			get
			{
				return _nextPlayer;
			}
		}

		public MediaPlaylist Playlist { get { return _playlist; } }

		public int PlaylistIndex { get { return _playlistIndex; } }

		public MediaPlaylist.MediaItem PlaylistItem { get { if (_playlist.HasItemAt(_playlistIndex)) return _playlist.Items[_playlistIndex]; return null; } }

		public PlaylistLoopMode LoopMode { get { return _playlistLoopMode; } set { _playlistLoopMode = value; } }

		public bool AutoProgress { get { return _playlistAutoProgress; } set { _playlistAutoProgress = value; } }

		public override IMediaInfo Info
		{
			get { if (CurrentPlayer != null) return CurrentPlayer.Info; return null; }
		}

		public override IMediaControl Control
		{
			get { if (CurrentPlayer != null) return CurrentPlayer.Control; return null; }
		}

		public override IMediaProducer TextureProducer
		{
			get
			{
				if (CurrentPlayer != null)
				{
					if (IsTransitioning())
					{
						return this;
					}
					/*if (_currentItem != null && _currentItem.sourceType == MediaPlaylist.MediaItem.SourceType.Texture2D && _currentItem.texture != null)
					{
						return this;
					}*/
					return CurrentPlayer.TextureProducer;
				}
				return null; 
			}
		}

		private void SwapPlayers()
		{
			// Pause the previously playing video
			// This is useful for systems that will struggle to play 2 videos at once
			if (_pausePreviousOnTransition)
			{
				CurrentPlayer.Pause();
			}

			// Tell listeners that the playlist item has changed
			Events.Invoke(this, MediaPlayerEvent.EventType.PlaylistItemChanged, ErrorCode.None);

			// Start the transition
			if (_currentTransition != Transition.None)
			{
				// Create a new transition texture if required
				Texture currentTexture = GetCurrentTexture();
				Texture nextTexture = GetNextTexture();
				if (currentTexture != null && nextTexture != null)
				{
					int maxWidth = Mathf.Max(nextTexture.width, currentTexture.width);
					int maxHeight = Mathf.Max(nextTexture.height, currentTexture.height);
					if (_rt != null)
					{
						if (_rt.width != maxWidth || _rt.height != maxHeight)
						{
							RenderTexture.ReleaseTemporary(_rt = null);
						}
					}

					if (_rt == null)
					{
						_rt = RenderTexture.GetTemporary(maxWidth, maxHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
						Graphics.Blit(currentTexture, _rt);
					}

					_material.SetTexture(_propFromTex, currentTexture);

					_easeFunc = Easing.GetFunction(_currentTransitionEasing);
					_transitionTimer = 0f;
				}
				else
				{
					_transitionTimer = _currentTransitionDuration;
				}
			}

			// Swap the videos
			if (NextPlayer == _playerA)
			{
				_nextPlayer = _playerB;
			}
			else
			{
				_nextPlayer = _playerA;
			}

			// Swap the items
			_currentItem = _nextItem;
			_nextItem = null;
		}

		private Texture GetCurrentTexture()
		{
			/*if (_currentItem != null && _currentItem.sourceType == MediaPlaylist.MediaItem.SourceType.Texture2D && _currentItem.texture != null)
			{
				return _currentItem.texture;
			}*/
			if (CurrentPlayer != null && CurrentPlayer.TextureProducer != null)
			{
				return CurrentPlayer.TextureProducer.GetTexture();
			}
			return null;
		}

		private Texture GetNextTexture()
		{
			/*if (_nextItem != null && _nextItem.sourceType == MediaPlaylist.MediaItem.SourceType.Texture2D && _nextItem.texture != null)
			{
				return _nextItem.texture;
			}*/
			if (_nextPlayer != null && _nextPlayer.TextureProducer != null)
			{
				return _nextPlayer.TextureProducer.GetTexture();
			}
			return null;
		}

		private void Awake()
		{
			_nextPlayer = _playerA;
			_shader = Shader.Find("AVProVideo/Helper/Transition");
			_material = new Material(_shader);
			_propFromTex = Shader.PropertyToID("_FromTex");
			_propT = Shader.PropertyToID("_Fade");
			_easeFunc = Easing.GetFunction(_transitionEasing.preset);
		}

		protected override void OnDestroy()
		{
			if (_rt != null)
			{
				RenderTexture.ReleaseTemporary(_rt);
				_rt = null;
			}
			if (_material != null)
			{
				Material.Destroy(_material);
				_material = null;
			}
			base.OnDestroy();
		}

		private void Start()
		{
			if (CurrentPlayer)
			{
				CurrentPlayer.Events.AddListener(OnVideoEvent);

				if (NextPlayer)
				{
					NextPlayer.Events.AddListener(OnVideoEvent);
				}
			}

			JumpToItem(0);
		}

		public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			if (mp == CurrentPlayer)
			{
				Events.Invoke(mp, et, errorCode);
			}

			switch (et)
			{
				case MediaPlayerEvent.EventType.FirstFrameReady:
					if (mp == NextPlayer)
					{
						SwapPlayers();
						Events.Invoke(mp, et, errorCode);
					}
					break;
				case MediaPlayerEvent.EventType.FinishedPlaying:
					if (_playlistAutoProgress && mp == CurrentPlayer && _currentItem.progressMode == ProgressMode.OnFinish)
					{
						NextItem();
					}
					break;
			}
		}

		public bool PrevItem()
		{
			return JumpToItem(_playlistIndex - 1);
		}

		public bool NextItem()
		{
			bool result = JumpToItem(_playlistIndex + 1);
			if (!result)
			{
				Events.Invoke(this, MediaPlayerEvent.EventType.PlaylistFinished, ErrorCode.None);
			}
			return result;
		}

		public bool CanJumpToItem(int index)
		{
			if (_playlistLoopMode == PlaylistLoopMode.Loop)
			{
				if (_playlist.Items.Count > 0)
				{
					index %= _playlist.Items.Count;
					if (index < 0)
					{
						index += _playlist.Items.Count;
					}					
				}
			}
			return _playlist.HasItemAt(index);
		}

		public bool JumpToItem(int index)
		{
			if (_playlistLoopMode == PlaylistLoopMode.Loop)
			{
				if (_playlist.Items.Count > 0)
				{
					index %= _playlist.Items.Count;
					if (index < 0)
					{
						index += _playlist.Items.Count;
					}
				}
			}
			if (_playlist.HasItemAt(index))
			{
				_playlistIndex = index;
				_nextItem = _playlist.Items[_playlistIndex];
				OpenVideoFile(_nextItem);
				return true;
			}
			return false;
		}

		public void OpenVideoFile(MediaPlaylist.MediaItem mediaItem)
 		{
			bool isMediaAlreadyLoaded = false;
			if (NextPlayer.m_VideoPath == mediaItem.filePath && NextPlayer.m_VideoLocation == mediaItem.fileLocation)
			{
				isMediaAlreadyLoaded = true;
			}

			if (mediaItem.isOverrideTransition)
			{
				SetTransition(mediaItem.overrideTransition, mediaItem.overrideTransitionDuration, mediaItem.overrideTransitionEasing.preset);
			}
			else
			{
				SetTransition(_nextTransition, _transitionDuration, _transitionEasing.preset);
			}

			this.m_Loop = NextPlayer.m_Loop = mediaItem.loop;
			this.m_AutoStart = NextPlayer.m_AutoStart = mediaItem.autoPlay;
			this.m_VideoLocation = NextPlayer.m_VideoLocation = mediaItem.fileLocation;
			this.m_VideoPath = NextPlayer.m_VideoPath = mediaItem.filePath;
			this.m_StereoPacking = NextPlayer.m_StereoPacking = mediaItem.stereoPacking;
			this.m_AlphaPacking = NextPlayer.m_AlphaPacking = mediaItem.alphaPacking;

			if (isMediaAlreadyLoaded)
			{
				NextPlayer.Rewind(false);
				if (_nextItem.startMode == StartMode.Immediate)
				{
					NextPlayer.Play();
				}
				// TODO: We probably want to wait until the new frame arrives before swapping after a Rewind()
				SwapPlayers();
			}
			else
			{
				if (string.IsNullOrEmpty(NextPlayer.m_VideoPath))
				{
					NextPlayer.CloseVideo();
				}
				else
				{
					//NextPlayer.m_AutoStart = false;
					NextPlayer.OpenVideoFromFile(NextPlayer.m_VideoLocation, NextPlayer.m_VideoPath, _nextItem.startMode == StartMode.Immediate);
				}
			}
		}

		private bool IsTransitioning()
		{
			if (_rt != null && _transitionTimer < _currentTransitionDuration && _currentTransition != Transition.None)
			{
				return true;
			}
			return false;
		}

		private void SetTransition(Transition transition, float duration, Easing.Preset easing)
		{
			if (transition == Transition.Random)
			{
				transition = (Transition)Random.Range(0, (int)Transition.Random);
			}

			if (transition != _currentTransition)
			{
				// Disable the previous transition
				if (!string.IsNullOrEmpty(_currentTransitionName))
				{
					_material.DisableKeyword(_currentTransitionName);
				}

				// Enable the next transition
				_currentTransition = transition;
				_currentTransitionName = GetTransitionName(transition);
				_material.EnableKeyword(_currentTransitionName);
			}

			_currentTransitionDuration = duration;
			_currentTransitionEasing = easing;
		}

		protected override void Update()
		{
			if (IsTransitioning())
			{
				_transitionTimer += Time.deltaTime;
				float t = _easeFunc(Mathf.Clamp01(_transitionTimer / _currentTransitionDuration));

				// Fade the audio volume
				NextPlayer.Control.SetVolume(1f - t);
				CurrentPlayer.Control.SetVolume(t);

				// TODO: support going from mono to stereo
				// TODO: support videos of different aspect ratios by rendering with scaling to fit
				// This can be done by blitting twice, once for each eye
				// If the stereo mode is different for playera/b then both should be set to stereo during the transition
				// if (CurrentPlayer.m_StereoPacking == StereoPacking.TopBottom)....
				_material.SetFloat(_propT, t);
				_rt.DiscardContents();
				Graphics.Blit(GetCurrentTexture(), _rt, _material);

				// After the transition is complete, pause the previous video
				if (!_pausePreviousOnTransition && !IsTransitioning())
				{
					if (NextPlayer != null && NextPlayer.Control.IsPlaying())
					{
						NextPlayer.Pause();
					}
				}
			}
			else
			{
				if (_playlistAutoProgress && _nextItem == null && _currentItem != null && _currentItem.progressMode == ProgressMode.BeforeFinish && Control != null && Control.GetCurrentTimeMs() >= (Info.GetDurationMs() - (_currentItem.progressTimeSeconds * 1000f)))
				{
					this.NextItem();
				}
			}

			base.Update();
		}

		public Texture GetTexture(int index = 0)
		{
			// TODO: support iOS YCbCr by supporting multiple textures
			/*if (!IsTransitioning())
			{
				if (_currentItem != null && _currentItem.sourceType == MediaPlaylist.MediaItem.SourceType.Texture2D && _currentItem.texture != null)
				{
					return _currentItem.texture;
				}
			}*/
			return _rt;
		}

		public int GetTextureCount()
		{
			return CurrentPlayer.TextureProducer.GetTextureCount();
		}

		public int GetTextureFrameCount()
		{
			return CurrentPlayer.TextureProducer.GetTextureFrameCount();
		}

		public bool SupportsTextureFrameCount()
		{
			return CurrentPlayer.TextureProducer.SupportsTextureFrameCount();
		}

		public long GetTextureTimeStamp()
		{
			return CurrentPlayer.TextureProducer.GetTextureTimeStamp();
		}

		public bool RequiresVerticalFlip()
		{
			return CurrentPlayer.TextureProducer.RequiresVerticalFlip();
		}

		public Matrix4x4 GetYpCbCrTransform()
		{
			return CurrentPlayer.TextureProducer.GetYpCbCrTransform();
		}

		private static string GetTransitionName(Transition transition)
		{
			switch (transition)
			{
				case Transition.None:		return "LERP_NONE";
				case Transition.Fade: 		return "LERP_FADE";
				case Transition.Black:		return "LERP_BLACK";
				case Transition.White:		return "LERP_WHITE";
				case Transition.Transparent:return "LERP_TRANSP";
				case Transition.Horiz:		return "LERP_HORIZ";
				case Transition.Vert:		return "LERP_VERT";
				case Transition.Diag:		return "LERP_DIAG";
				case Transition.MirrorH:	return "LERP_HORIZ_MIRROR";
				case Transition.MirrorV:	return "LERP_VERT_MIRROR";
				case Transition.MirrorD:	return "LERP_DIAG_MIRROR";
				case Transition.ScrollV:	return "LERP_SCROLL_VERT";
				case Transition.ScrollH:	return "LERP_SCROLL_HORIZ";
				case Transition.Circle:		return "LERP_CIRCLE";
				case Transition.Diamond:	return "LERP_DIAMOND";
				case Transition.Blinds:		return "LERP_BLINDS";
				case Transition.Arrows:		return "LERP_ARROW";
				case Transition.SlideH:		return "LERP_SLIDE_HORIZ";
				case Transition.SlideV:		return "LERP_SLIDE_VERT";
				case Transition.Zoom:		return "LERP_ZOOM_FADE";
				case Transition.RectV:		return "LERP_RECTS_VERT";
			}
			return string.Empty;
		}

#region Easing

		/// <summary>
		/// Easing functions
		/// </summary>
		[System.Serializable]
		public class Easing
		{
			public Preset preset = Preset.Linear;

			public enum Preset
			{
				Step,
				Linear,
				InQuad,
				OutQuad,
				InOutQuad,
				InCubic,
				OutCubic,
				InOutCubic,
				InQuint,
				OutQuint,
				InOutQuint,
				InQuart,
				OutQuart,
				InOutQuart,
				InExpo,
				OutExpo,
				InOutExpo,
				Random,
				RandomNotStep,
			}

			public static System.Func<float, float> GetFunction(Preset preset)
			{
				System.Func<float, float> result = null;
				switch (preset)
				{
					case Preset.Step:
						result = Step;
						break;
					case Preset.Linear:
						result = Linear;
						break;
					case Preset.InQuad:
						result = InQuad;
						break;
					case Preset.OutQuad:
						result = OutQuad;
						break;
					case Preset.InOutQuad:
						result = InOutQuad;
						break;
					case Preset.InCubic:
						result = InCubic;
						break;
					case Preset.OutCubic:
						result = OutCubic;
						break;
					case Preset.InOutCubic:
						result = InOutCubic;
						break;
					case Preset.InQuint:
						result = InQuint;
						break;
					case Preset.OutQuint:
						result = OutQuint;
						break;
					case Preset.InOutQuint:
						result = InOutQuint;
						break;
					case Preset.InQuart:
						result = InQuart;
						break;
					case Preset.OutQuart:
						result = OutQuart;
						break;
					case Preset.InOutQuart:
						result = InOutQuart;
						break;
					case Preset.InExpo:
						result = InExpo;
						break;
					case Preset.OutExpo:
						result = OutExpo;
						break;
					case Preset.InOutExpo:
						result = InOutExpo;
						break;
					case Preset.Random:
						result = GetFunction((Preset)Random.Range(0, (int)Preset.Random));
						break;
					case Preset.RandomNotStep:
						result = GetFunction((Preset)Random.Range((int)Preset.Step+1, (int)Preset.Random));
						break;
				}
				return result;
			}

			public static float PowerEaseIn(float t, float power)
			{
				return Mathf.Pow(t, power);
			}

			public static float PowerEaseOut(float t, float power)
			{
				return 1f - Mathf.Abs(Mathf.Pow(t - 1f, power));
			}

			public static float PowerEaseInOut(float t, float power)
			{
				float result;
				if (t < 0.5f)
				{
					result = PowerEaseIn(t * 2f, power) / 2f;
				}
				else
				{
					result = PowerEaseOut(t * 2f - 1f, power) / 2f + 0.5f;
				}
				return result;
			}

			public static float Step(float t)
			{
				float result = 0f;
				if (t >= 0.5f)
				{
					result = 1f;
				}
				return result;
			}

			public static float Linear(float t)
			{
				return t;
			}

			public static float InQuad(float t)
			{
				return PowerEaseIn(t, 2f);
			}

			public static float OutQuad(float t)
			{
				return PowerEaseOut(t, 2f);
				//return t * (2f - t);
			}

			public static float InOutQuad(float t)
			{
				return PowerEaseInOut(t, 2f);
				//return t < 0.5 ? (2f * t * t) : (-1f + (4f - 2f * t) * t);
			}

			public static float InCubic(float t)
			{
				return PowerEaseIn(t, 3f);
				//return t * t * t;
			}

			public static float OutCubic(float t)
			{
				return PowerEaseOut(t, 3f);
				//return (--t) * t * t + 1f;
			}

			public static float InOutCubic(float t)
			{
				return PowerEaseInOut(t, 3f);
				//return t < .5f ? (4f * t * t * t) : ((t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f);
			}

			public static float InQuart(float t)
			{
				return PowerEaseIn(t, 4f);
				//return t * t * t * t;
			}

			public static float OutQuart(float t)
			{
				return PowerEaseOut(t, 4f);
				//return 1f - (--t) * t * t * t;
			}

			public static float InOutQuart(float t)
			{
				return PowerEaseInOut(t, 4f);
				//return t < 0.5f ? (8f * t * t * t * t) : (1f - 8f * (--t) * t * t * t);
			}

			public static float InQuint(float t)
			{
				return PowerEaseIn(t, 5f);
				//return t * t * t * t * t;
			}

			public static float OutQuint(float t)
			{
				return PowerEaseOut(t, 5f);
				//return 1f + (--t) * t * t * t * t;
			}

			public static float InOutQuint(float t)
			{
				return PowerEaseInOut(t, 5f);
				//return t < 0.5f ? (16f * t * t * t * t * t) : (1f + 16f * (--t) * t * t * t * t);
			}

			public static float InExpo(float t)
			{
				float result = 0f;
				if (t != 0f)
				{
					result = Mathf.Pow(2f, 10f * (t - 1f));
				}
				return result;
			}

			public static float OutExpo(float t)
			{
				float result = 1f;
				if (t != 1f)
				{
					result = -Mathf.Pow(2f, -10f * t) + 1f;
				}
				return result;
			}

			public static float InOutExpo(float t)
			{
				float result = 0f;
				if (t > 0f)
				{
					result = 1f;
					if (t < 1f)
					{
						t *= 2f;
						if (t < 1f)
						{
							result = 0.5f * Mathf.Pow(2f, 10f * (t - 1f));
						}
						else
						{
							t--;
							result = 0.5f * (-Mathf.Pow(2f, -10f * t) + 2f);
						}
					}
				}
				return result;
			}
		}
#endregion Easing

	}
}