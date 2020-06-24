#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0)
	#define UNITY_HELPATTRIB
#endif

using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This is an experimental feature and only works in Windows currently
	/// Audio is grabbed from the MediaPlayer and rendered via Unity
	/// This allows audio to have 3D spatial control, effects applied and to be spatialised for VR
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("AVPro Video/Audio Output", 400)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class AudioOutput : MonoBehaviour
	{
		public enum AudioOutputMode
		{
			Single,
			Multiple
		}

		public AudioOutputMode _audioOutputMode = AudioOutputMode.Multiple;

		[SerializeField]
		private MediaPlayer _mediaPlayer;

		private AudioSource _audioSource;

		[HideInInspector]
		public int _channelMask = -1;

		void Awake()
		{
			_audioSource = this.GetComponent<AudioSource>();
		}

		void Start()
		{
			ChangeMediaPlayer(_mediaPlayer);
#if (!UNITY_5 && !UNITY_5_4_OR_NEWER)
			Debug.LogWarning("[AVProVideo] AudioOutput component requires Unity 5.x or above", this);
#endif
		}

		void OnDestroy()
		{
			ChangeMediaPlayer(null);
		}

		void Update()
		{
			if (_mediaPlayer != null && _mediaPlayer.Control != null && _mediaPlayer.Control.IsPlaying())
			{
				ApplyAudioSettings(_mediaPlayer, _audioSource);
			}
		}

		public void ChangeMediaPlayer(MediaPlayer newPlayer)
		{
			// When changing the media player, handle event subscriptions
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
				_mediaPlayer = null;
			}

			_mediaPlayer = newPlayer;
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Closing:
					_audioSource.Stop();
					break;
				case MediaPlayerEvent.EventType.Started:
					ApplyAudioSettings(_mediaPlayer, _audioSource);
					_audioSource.Play();
					break;
			}
		}

		private static void ApplyAudioSettings(MediaPlayer player, AudioSource audioSource)
		{
			// Apply volume and mute from the MediaPlayer to the AudioSource
			if (player != null && player.Control != null)
			{
				float volume = player.Control.GetVolume();
				bool isMuted = player.Control.IsMuted();
				float rate = player.Control.GetPlaybackRate();
				audioSource.volume = volume;
				audioSource.mute = isMuted;
				audioSource.pitch = rate;
			}
		}

#if (UNITY_5 || UNITY_5_4_OR_NEWER)
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA_10_0 || UNITY_WINRT_8_1
		void OnAudioFilterRead(float[] data, int channels)
		{
			AudioOutputManager.Instance.RequestAudio(this, _mediaPlayer, data, _channelMask, channels, _audioOutputMode);
		}
#endif
#endif
	}
}