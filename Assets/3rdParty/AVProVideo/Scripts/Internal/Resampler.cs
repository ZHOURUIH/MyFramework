using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Utility class to resample MediaPlayer video frames to allow for smoother playback
	/// Keeps a buffer of frames with timestamps and presents them using its own clock
	/// </summary>
	public class Resampler
	{
		private class TimestampedRenderTexture
		{
			public RenderTexture texture = null;
			public long timestamp = 0;
			public bool used = false;
		}

		public enum ResampleMode
		{
			POINT, LINEAR
		}

		private List<TimestampedRenderTexture[]> _buffer = new List<TimestampedRenderTexture[]>();
		private MediaPlayer _mediaPlayer;
		private RenderTexture[] _outputTexture = null;

		private int _start = 0;
		private int _end = 0;
		private int _bufferSize = 0;

		private long _baseTimestamp = 0;
		private float _elapsedTimeSinceBase = 0f;

		private Material _blendMat;

		private ResampleMode _resampleMode;
		private string _name = "";

		private long _lastTimeStamp = -1;

		private int _droppedFrames = 0;

		private long _lastDisplayedTimestamp = 0;
		private int _frameDisplayedTimer = 0;
		private long _currentDisplayedTimestamp = 0;

		public int DroppedFrames
		{
			get { return _droppedFrames; }
		}

		public int FrameDisplayedTimer
		{
			get { return _frameDisplayedTimer; }
		}

		public long BaseTimestamp
		{
			get { return _baseTimestamp; }
			set { _baseTimestamp = value; }
		}

		public float ElapsedTimeSinceBase
		{
			get { return _elapsedTimeSinceBase; }
			set { _elapsedTimeSinceBase = value; }
		}

		public float LastT
		{
			get; private set;
		}

		public long TextureTimeStamp
		{
			get; private set;
		}

		private const string ShaderPropT = "_t";
		private const string ShaderPropAftertex = "_AfterTex";
		private int _propAfterTex;
		private int _propT;
		private float _videoFrameRate;

		public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.MetaDataReady:
					_videoFrameRate = mp.Info.GetVideoFrameRate();
					_elapsedTimeSinceBase = 0f;
					if (_videoFrameRate > 0f)
					{
						_elapsedTimeSinceBase = _bufferSize / _videoFrameRate;
					}
					break;
				case MediaPlayerEvent.EventType.Closing:
					Reset();
					break;
				default:
					break;
			}
		}

		public Resampler(MediaPlayer player, string name, int bufferSize = 2, ResampleMode resampleMode = ResampleMode.LINEAR)
		{
			_bufferSize = Mathf.Max(2, bufferSize);

			player.Events.AddListener(OnVideoEvent);

			_mediaPlayer = player;

			Shader blendShader = Shader.Find("AVProVideo/BlendFrames");
			if (blendShader != null)
			{
				_blendMat = new Material(blendShader);
				_propT = Shader.PropertyToID(ShaderPropT);
				_propAfterTex = Shader.PropertyToID(ShaderPropAftertex);
			}
			else
			{
				Debug.LogError("[AVProVideo] Failed to find BlendFrames shader");
			}

			_resampleMode = resampleMode;
			_name = name;

			Debug.Log("[AVProVideo] Resampler " + _name + " started");
		}

		public Texture[] OutputTexture
		{
			get { return _outputTexture; }
		}

		public void Reset()
		{
			_lastTimeStamp = -1;
			_baseTimestamp = 0;
			InvalidateBuffer();
		}

		public void Release()
		{
			ReleaseRenderTextures();
			if (_blendMat != null)
			{
				Material.Destroy(_blendMat);
			}
		}

		private void ReleaseRenderTextures()
		{
			for (int i = 0; i < _buffer.Count; ++i)
			{
				for (int j = 0; j < _buffer[i].Length; ++j)
				{
					if (_buffer[i][j].texture != null)
					{
						RenderTexture.ReleaseTemporary(_buffer[i][j].texture);
						_buffer[i][j].texture = null;
					}
				}

				if (_outputTexture != null && _outputTexture[i] != null)
				{
					RenderTexture.ReleaseTemporary(_outputTexture[i]);
				}
			}

			_outputTexture = null;
		}

		private void ConstructRenderTextures()
		{
			ReleaseRenderTextures();
			_buffer.Clear();

			_outputTexture = new RenderTexture[_mediaPlayer.TextureProducer.GetTextureCount()];

			for (int i = 0; i < _mediaPlayer.TextureProducer.GetTextureCount(); ++i)
			{
				Texture tex = _mediaPlayer.TextureProducer.GetTexture(i);
				_buffer.Add(new TimestampedRenderTexture[_bufferSize]);
				for (int j = 0; j < _bufferSize; ++j)
				{
					_buffer[i][j] = new TimestampedRenderTexture();
				}

				for (int j = 0; j < _buffer[i].Length; ++j)
				{
					_buffer[i][j].texture = RenderTexture.GetTemporary(tex.width, tex.height, 0);
					_buffer[i][j].timestamp = 0;
					_buffer[i][j].used = false;
				}

				_outputTexture[i] = RenderTexture.GetTemporary(tex.width, tex.height, 0);
				_outputTexture[i].filterMode = tex.filterMode;
				_outputTexture[i].wrapMode = tex.wrapMode;
				_outputTexture[i].anisoLevel = tex.anisoLevel;
				// TODO: set up the mips level too?
			}
		}

		private bool CheckRenderTexturesValid()
		{
			for (int i = 0; i < _mediaPlayer.TextureProducer.GetTextureCount(); ++i)
			{
				Texture tex = _mediaPlayer.TextureProducer.GetTexture(i);
				for (int j = 0; j < _buffer.Count; ++j)
				{
					if (_buffer[i][j].texture == null || _buffer[i][j].texture.width != tex.width || _buffer[i][j].texture.height != tex.height)
					{
						return false;
					}
				}

				if (_outputTexture == null || _outputTexture[i] == null || _outputTexture[i].width != tex.width || _outputTexture[i].height != tex.height)
				{
					return false;
				}

			}

			return true;
		}

		//finds closest frame that occurs before given index
		private int FindBeforeFrameIndex(int frameIdx)
		{
			if (frameIdx >= _buffer.Count)
			{
				return -1;
			}

			int foundFrame = -1;
			float smallestDif = float.MaxValue;
			int closest = -1;
			float smallestElapsed = float.MaxValue;

			for (int i = 0; i < _buffer[frameIdx].Length; ++i)
			{
				if (_buffer[frameIdx][i].used)
				{
					float elapsed = (_buffer[frameIdx][i].timestamp - _baseTimestamp) / 10000000f;

					//keep track of closest after frame, just in case no before frame was found
					if (elapsed < smallestElapsed)
					{
						closest = i;
						smallestElapsed = elapsed;
					}

					float dif = _elapsedTimeSinceBase - elapsed;

					if (dif >= 0 && dif < smallestDif)
					{
						smallestDif = dif;
						foundFrame = i;
					}
				}
			}

			if (foundFrame < 0)
			{
				if (closest < 0)
				{
					return -1;
				}

				return closest;
			}

			return foundFrame;
		}

		private int FindClosestFrame(int frameIdx)
		{
			if (frameIdx >= _buffer.Count)
			{
				return -1;
			}

			int foundPos = -1;
			float smallestDif = float.MaxValue;

			for (int i = 0; i < _buffer[frameIdx].Length; ++i)
			{
				if (_buffer[frameIdx][i].used)
				{
					float elapsed = (_buffer[frameIdx][i].timestamp - _baseTimestamp) / 10000000f;
					float dif = Mathf.Abs(_elapsedTimeSinceBase - elapsed);
					if (dif < smallestDif)
					{
						foundPos = i;
						smallestDif = dif;
					}
				}
			}

			return foundPos;
		}

		//point update selects closest frame and uses that as output
		private void PointUpdate()
		{
			for (int i = 0; i < _buffer.Count; ++i)
			{
				int frameIndex = FindClosestFrame(i);
				if (frameIndex < 0)
				{
					continue;
				}

				_outputTexture[i].DiscardContents();
				Graphics.Blit(_buffer[i][frameIndex].texture, _outputTexture[i]);
				TextureTimeStamp = _currentDisplayedTimestamp = _buffer[i][frameIndex].timestamp;
			}

		}

		//Updates currently displayed frame
		private void SampleFrame(int frameIdx, int bufferIdx)
		{
			_outputTexture[bufferIdx].DiscardContents();
			Graphics.Blit(_buffer[bufferIdx][frameIdx].texture, _outputTexture[bufferIdx]);
			TextureTimeStamp = _currentDisplayedTimestamp = _buffer[bufferIdx][frameIdx].timestamp;
		}

		//Same as sample frame, but does a lerp of the two given frames and outputs that image instead
		private void SampleFrames(int bufferIdx, int frameIdx1, int frameIdx2, float t)
		{
			_blendMat.SetFloat(_propT, t);
			_blendMat.SetTexture(_propAfterTex, _buffer[bufferIdx][frameIdx2].texture);
			_outputTexture[bufferIdx].DiscardContents();
			Graphics.Blit(_buffer[bufferIdx][frameIdx1].texture, _outputTexture[bufferIdx], _blendMat);
			TextureTimeStamp = (long)Mathf.Lerp(_buffer[bufferIdx][frameIdx1].timestamp, _buffer[bufferIdx][frameIdx2].timestamp, t);
			_currentDisplayedTimestamp = _buffer[bufferIdx][frameIdx1].timestamp;
		}

		private void LinearUpdate()
		{
			for (int i = 0; i < _buffer.Count; ++i)
			{
				//find closest frame
				int frameIndex = FindBeforeFrameIndex(i);

				//no valid frame, this should never ever happen actually...
				if (frameIndex < 0)
				{
					continue;
				}

				//resample or just use last frame and set current elapsed time to that frame
				float frameElapsed = (_buffer[i][frameIndex].timestamp - _baseTimestamp) / 10000000f;
				if (frameElapsed > _elapsedTimeSinceBase)
				{
					SampleFrame(frameIndex, i);
					LastT = -1f;
				}
				else
				{
					int next = (frameIndex + 1) % _buffer[i].Length;
					float nextElapsed = (_buffer[i][next].timestamp - _baseTimestamp) / 10000000f;

					//no larger frame, move elapsed time back a bit since we cant predict the future
					if (nextElapsed < frameElapsed)
					{
						SampleFrame(frameIndex, i);
						LastT = 2f;
					}
					//have a before and after frame, interpolate
					else
					{

						float range = nextElapsed - frameElapsed;
						float t = (_elapsedTimeSinceBase - frameElapsed) / range;
						SampleFrames(i, frameIndex, next, t);
						LastT = t;
					}
				}
			}
		}

		private void InvalidateBuffer()
		{
			_elapsedTimeSinceBase = (_bufferSize / 2) / _videoFrameRate;

			for (int i = 0; i < _buffer.Count; ++i)
			{
				for (int j = 0; j < _buffer[i].Length; ++j)
				{
					_buffer[i][j].used = false;
				}
			}

			_start = _end = 0;
		}

		private float GuessFrameRate()
		{
			int fpsCount = 0;
			long fps = 0;
			
			for (int k = 0; k < _buffer[0].Length; k++)
			{
				if (_buffer[0][k].used)
				{
					// Find the pair with the smallest difference
					long smallestDiff = long.MaxValue;
					for (int j = k + 1; j < _buffer[0].Length; j++)
					{
						if (_buffer[0][j].used)
						{
							long diff = System.Math.Abs(_buffer[0][k].timestamp - _buffer[0][j].timestamp);
							if (diff < smallestDiff)
							{
								smallestDiff = diff;
							}
						}
					}

					if (smallestDiff != long.MaxValue)
					{
						fps += smallestDiff;
						fpsCount++;
					}
				}
			}
			if (fpsCount > 1)
			{
				fps /= fpsCount;
			}
			return 10000000f / (float)fps;
		}

		public void Update()
		{
			if (_mediaPlayer.TextureProducer == null)
			{
				return;
			}

			//recreate textures if invalid
			if (_mediaPlayer.TextureProducer == null || _mediaPlayer.TextureProducer.GetTexture() == null)
			{
				return;
			}

			if (!CheckRenderTexturesValid())
			{
				ConstructRenderTextures();
			}

			long currentTimestamp = _mediaPlayer.TextureProducer.GetTextureTimeStamp();

			//if frame has been updated, do a calculation to estimate dropped frames
			if (currentTimestamp != _lastTimeStamp)
			{
				float dif = Mathf.Abs(currentTimestamp - _lastTimeStamp);
				float frameLength = (10000000f / _videoFrameRate);
				if (dif > frameLength * 1.1f && dif < frameLength * 3.1f)
				{
					_droppedFrames += (int)((dif - frameLength) / frameLength + 0.5);
				}
				_lastTimeStamp = currentTimestamp;
			}

			//Adding texture to buffer logic
			long timestamp = _mediaPlayer.TextureProducer.GetTextureTimeStamp();
			bool insertNewFrame = !_mediaPlayer.Control.IsSeeking();
			//if buffer is not empty, we need to check if we need to reject the new frame
			if (_start != _end || _buffer[0][_end].used)
			{
				int lastFrame = (_end + _buffer[0].Length - 1) % _buffer[0].Length;
				//frame is not new and thus we do not need to store it
				if (timestamp == _buffer[0][lastFrame].timestamp)
				{
					insertNewFrame = false;
				}
			}

			bool bufferWasNotFull = (_start != _end) || (!_buffer[0][_end].used);

			if (insertNewFrame)
			{
				//buffer empty, reset base timestamp to current
				if (_start == _end && !_buffer[0][_end].used)
				{
					_baseTimestamp = timestamp;
				}

				//update buffer counters, if buffer is full, we get rid of the earliest frame by incrementing the start counter
				if (_end == _start && _buffer[0][_end].used)
				{
					_start = (_start + 1) % _buffer[0].Length;
				}

				for (int i = 0; i < _mediaPlayer.TextureProducer.GetTextureCount(); ++i)
				{
					Texture currentTexture = _mediaPlayer.TextureProducer.GetTexture(i);

					//store frame info
					_buffer[i][_end].texture.DiscardContents();
					Graphics.Blit(currentTexture, _buffer[i][_end].texture);
					_buffer[i][_end].timestamp = timestamp;
					_buffer[i][_end].used = true;
				}

				_end = (_end + 1) % _buffer[0].Length;
			}

			bool bufferNotFull = (_start != _end) || (!_buffer[0][_end].used);

			if (bufferNotFull)
			{
				for (int i = 0; i < _buffer.Count; ++i)
				{
					_outputTexture[i].DiscardContents();
					Graphics.Blit(_buffer[i][_start].texture, _outputTexture[i]);
					_currentDisplayedTimestamp = _buffer[i][_start].timestamp;
				}
			}
			else
			{
				// If we don't have a valid frame rate and the buffer is now full, guess the frame rate by looking at the buffered timestamps
				if (bufferWasNotFull && _videoFrameRate <= 0f)
				{
					_videoFrameRate = GuessFrameRate();
					_elapsedTimeSinceBase = (_bufferSize / 2) / _videoFrameRate;
				}
			}

			if (_mediaPlayer.Control.IsPaused())
			{
				InvalidateBuffer();
			}

			//we always wait until buffer is full before display things, just assign first frame in buffer to output so that the user can see something
			if (bufferNotFull)
			{
				return;
			}

			if (_mediaPlayer.Control.IsPlaying() && !_mediaPlayer.Control.IsFinished())
			{
				//correct elapsed time if too far out
				long ts = _buffer[0][(_start + _bufferSize / 2) % _bufferSize].timestamp - _baseTimestamp;
				double dif = Mathf.Abs(((float)((double)_elapsedTimeSinceBase * 10000000) - ts));
				double threshold = (_buffer[0].Length / 2) / _videoFrameRate * 10000000;

				if (dif > threshold)
				{
					_elapsedTimeSinceBase = ts / 10000000f;
				}

				if (_resampleMode == ResampleMode.POINT)
				{
					PointUpdate();
				}
				else if (_resampleMode == ResampleMode.LINEAR)
				{
					LinearUpdate();
				}

				_elapsedTimeSinceBase += Time.unscaledDeltaTime;
			}
		}

		public void UpdateTimestamp()
		{
			if (_lastDisplayedTimestamp != _currentDisplayedTimestamp)
			{
				_lastDisplayedTimestamp = _currentDisplayedTimestamp;
				_frameDisplayedTimer = 0;
			}
			_frameDisplayedTimer++;
		}
	}
}

