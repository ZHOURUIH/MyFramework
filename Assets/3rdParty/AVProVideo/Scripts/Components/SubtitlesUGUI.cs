#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0)
	#define UNITY_HELPATTRIB
#endif

using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Update a standard uGUI Text element with subtitle text as it plays from the MediaPlayer
	/// </summary>
	[AddComponentMenu("AVPro Video/Subtitles uGUI", 201)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class SubtitlesUGUI : MonoBehaviour
	{
		[SerializeField]
		private MediaPlayer _mediaPlayer = null;

		[SerializeField]
		private Text _text = null;

		void Start()
		{
			ChangeMediaPlayer(_mediaPlayer);
		}

		void OnDestroy()
		{
			ChangeMediaPlayer(null);
		}

		public void ChangeMediaPlayer(MediaPlayer newPlayer)
		{
			// When changing the media player, handle event subscriptions
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
				_mediaPlayer = null;
			}

			if (newPlayer != null)
			{
				newPlayer.Events.AddListener(OnMediaPlayerEvent);
				_mediaPlayer = newPlayer;
			}
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.SubtitleChange:
					{
						string text = _mediaPlayer.Subtitles.GetSubtitleText();

						// Change RichText for Unity uGUI Text
						text = text.Replace("<font color=", "<color=");
						text = text.Replace("</font>", "</color>");
						text = text.Replace("<u>", string.Empty);
						text = text.Replace("</u>", string.Empty);

						_text.text = text;
					}
					break;
			}
		}
	}
}