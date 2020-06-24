//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RenderHeads.Media.AVProVideo
{
	[System.Serializable]
	public class StreamParserEvent : UnityEngine.Events.UnityEvent<StreamParser, StreamParserEvent.EventType>
	{
		public enum EventType
		{
			Success,
			Failed
		}
	}

	/// <summary>
	/// Utility class for parsing adaptive media streams, such as HLS
	/// A URL is specified, loaded and then the hierarchy of the stream elements are exposed
	/// </summary>
	public class StreamParser : MonoBehaviour			// TODO: make this not a component
	{
		public enum StreamType { HLS }

		public string _url;
		public StreamType _streamType;
		public bool _autoLoad = true;

		private Stream _parser;
		private bool _loaded = false;
		private List<Stream> _substreams;
		private List<Stream.Chunk> _chunks;
		private StreamParserEvent _events;

		public StreamParserEvent Events
		{
			get
			{
				if (_events == null)
				{
					_events = new StreamParserEvent();
				}

				return _events;
			}
		}

		private void LoadFile()
		{
			try
			{
				switch(_streamType)
				{
					case StreamType.HLS:
						_parser = new HLSStream(_url);
						break;
					default:
						_parser = new HLSStream(_url);
						break;
				}

				_substreams = _parser.GetAllStreams();
				_chunks = _parser.GetAllChunks();

				_loaded = true;

				Debug.Log("[AVProVideo] Stream parser completed parsing stream file " + _url);
				if (_events != null)
				{
					_events.Invoke(this, StreamParserEvent.EventType.Success);
				}
			}
			catch (Exception e)
			{
				_loaded = false;

				Debug.LogError("[AVProVideo] Parser unable to read stream " + e.Message);

				if (_events != null)
				{
					_events.Invoke(this, StreamParserEvent.EventType.Failed);
				}
			}
		}

		public bool Loaded
		{
			get { return _loaded; }
		}

		public Stream Root
		{
			get { return _loaded ? _parser : null; }
		}

		public List<Stream> SubStreams
		{
			get { return _loaded ? _substreams : null; }
		}

		public List<Stream.Chunk> Chunks
		{
			get { return _loaded ? _chunks : null; }
		}

		public void ParseStream()
		{
#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_WSA
			LoadFile();
#else
			Thread loadThread = new Thread(new ThreadStart(LoadFile));
			loadThread.Start();
#endif
		}

		void Start()
		{
			if(_autoLoad)
			{
				ParseStream();
			}
		}
	}
}
