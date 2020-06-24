using System.Collections.Generic;
using UnityEngine;
using System;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// A singleton to handle mulitple instances of the AudioOutput component
	/// </summary>
	public class AudioOutputManager
	{
		private static AudioOutputManager _instance = null;

		public static AudioOutputManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new AudioOutputManager();
				}

				return _instance;
			}
		}

		private Dictionary<MediaPlayer, HashSet<AudioOutput>> _accessTrackers;
		private Dictionary<MediaPlayer, float[]> _pcmData;

		private AudioOutputManager()
		{
			_accessTrackers = new Dictionary<MediaPlayer, HashSet<AudioOutput>>();
			_pcmData = new Dictionary<MediaPlayer, float[]>();
		}

		public void RequestAudio(AudioOutput _outputComponent, MediaPlayer mediaPlayer, float[] data, int channelMask, int totalChannels, AudioOutput.AudioOutputMode audioOutputMode)
		{
			if (mediaPlayer == null || mediaPlayer.Control == null || !mediaPlayer.Control.IsPlaying())
			{
				return;
			}

			int channels = mediaPlayer.Control.GetNumAudioChannels();
			if(channels <= 0)
			{
				return;
			}

			//total samples requested should be multiple of channels
#if (UNITY_5 && !UNITY_5_0) || UNITY_5_4_OR_NEWER
			Debug.Assert(data.Length % totalChannels == 0);
#endif

			if (!_accessTrackers.ContainsKey(mediaPlayer))
			{
				_accessTrackers[mediaPlayer] = new HashSet<AudioOutput>();
			}

			//requests data if it hasn't been requested yet for the current cycle
			if (_accessTrackers[mediaPlayer].Contains(_outputComponent) || _accessTrackers[mediaPlayer].Count == 0 || _pcmData[mediaPlayer] == null)
			{
				_accessTrackers[mediaPlayer].Clear();

				int actualDataRequired = data.Length / totalChannels * channels;
				_pcmData[mediaPlayer] = new float[actualDataRequired];

				GrabAudio(mediaPlayer, _pcmData[mediaPlayer], channels);

				_accessTrackers[mediaPlayer].Add(_outputComponent);
			}

			//calculate how many samples and what channels are needed and then copy over the data
			int samples = Math.Min(data.Length / totalChannels, _pcmData[mediaPlayer].Length / channels);
			int storedPos = 0;
			int requestedPos = 0;

			//multiple mode, copies over audio from desired channels into the same channels on the audiosource
			if (audioOutputMode == AudioOutput.AudioOutputMode.Multiple)
			{
				int lesserChannels = Math.Min(channels, totalChannels);

				for (int i = 0; i < samples; ++i)
				{
					for (int j = 0; j < lesserChannels; ++j)
					{
						if ((1 << j & channelMask) > 0)
						{
							data[requestedPos + j] = _pcmData[mediaPlayer][storedPos + j];
						}
					}

					storedPos += channels;
					requestedPos += totalChannels;
				}
			}

			//Mono mode, copies over single channel to all output channels
			else if(audioOutputMode == AudioOutput.AudioOutputMode.Single)
			{
				int desiredChannel = 0;

				for (int i = 0; i < 8; ++i)
				{
					if ((channelMask & (1 << i)) > 0)
					{
						desiredChannel = i;
						break;
					}
				}

				if(desiredChannel < channels)
				{
					for (int i = 0; i < samples; ++i)
					{
						for (int j = 0; j < totalChannels; ++j)
						{
							data[requestedPos + j] = _pcmData[mediaPlayer][storedPos + desiredChannel];
						}

						storedPos += channels;
						requestedPos += totalChannels;
					}
				}
			}
		}

		private void GrabAudio(MediaPlayer player, float[] data, int channels)
		{
			player.Control.GrabAudio(data, data.Length, channels);
		}
	}
}

