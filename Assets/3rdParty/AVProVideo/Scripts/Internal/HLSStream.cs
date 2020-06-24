//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

#if !UNITY_WSA_10_0 && !UNITY_WINRT_8_1 && !UNITY_WSA && !UNITY_WEBPLAYER
	#define SUPPORT_SSL
#endif

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if SUPPORT_SSL
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
#endif

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Utility class to parses an HLS(m3u8) adaptive stream from a URL to 
	/// allow easy inspection and navitation of the stream data
	/// </summary>
	public class HLSStream : Stream
	{
		private const string BANDWITH_NAME = "BANDWIDTH=";
		private const string RESOLUTION_NAME = "RESOLUTION=";
		private const string CHUNK_TAG = "#EXTINF";
		private const string STREAM_TAG = "#EXT-X-STREAM-INF";

		private List<Stream> _streams;
		private List<Chunk> _chunks;
		private string _streamURL;
		private int _width;
		private int _height;
		private int _bandwidth;

		public override int Width
		{
			get { return _width; }
		}

		public override int Height
		{
			get { return _height; }
		}

		public override int Bandwidth
		{
			get { return _bandwidth; }
		}

		public override string URL
		{
			get { return _streamURL; }
		}

		public override List<Chunk> GetAllChunks()
		{
			List<Chunk> chunks = new List<Chunk>();

			for(int i = 0; i < _streams.Count; ++i)
			{
				var streamChunks = _streams[i].GetAllChunks();
				chunks.AddRange(streamChunks);
			}

			chunks.AddRange(_chunks);

			return chunks;
		}

		public override List<Chunk> GetChunks()
		{
			return _chunks;
		}

		public override List<Stream> GetAllStreams()
		{
			List<Stream> streams = new List<Stream>();
			for(int i = 0; i < _streams.Count; ++i)
			{
				var childrenStreams = _streams[i].GetAllStreams();
				streams.AddRange(childrenStreams);
			}

			streams.AddRange(_streams);

			return streams;
		}

		public override List<Stream> GetStreams()
		{
			return _streams;
		}

		private bool ExtractStreamInfo(string line, ref int width, ref int height, ref int bandwidth)
		{
			if (line.StartsWith(STREAM_TAG))
			{
				int bandwidthPos = line.IndexOf(BANDWITH_NAME);
				if (bandwidthPos >= 0)
				{
					int endPos = line.IndexOf(',', bandwidthPos + BANDWITH_NAME.Length);
					if (endPos < 0)
					{
						endPos = line.Length;
					}

					if (endPos >= 0 && endPos - BANDWITH_NAME.Length > bandwidthPos)
					{
						int length = endPos - bandwidthPos - BANDWITH_NAME.Length;
						string bandwidthString = line.Substring(bandwidthPos + BANDWITH_NAME.Length, length);
						if (!int.TryParse(bandwidthString, out bandwidth))
						{
							bandwidth = 0;
						}
					}
				}
				else
				{
					bandwidth = 0;
				}

				int resolutionPos = line.IndexOf(RESOLUTION_NAME);
				if (resolutionPos >= 0)
				{
					int endPos = line.IndexOf(',', resolutionPos + RESOLUTION_NAME.Length);
					if (endPos < 0)
					{
						endPos = line.Length;
					}

					if (endPos >= 0 && endPos - RESOLUTION_NAME.Length > resolutionPos)
					{
						int length = endPos - resolutionPos - RESOLUTION_NAME.Length;
						string resolutionString = line.Substring(resolutionPos + RESOLUTION_NAME.Length, length);
						int xPos = resolutionString.IndexOf('x');

						if (xPos < 0 || !int.TryParse(resolutionString.Substring(0, xPos), out width) ||
							!int.TryParse(resolutionString.Substring(xPos + 1, resolutionString.Length - (xPos + 1)), out height))
						{
							width = height = 0;
						}
					}
				}
				else
				{
					width = height = 0;
				}

				return true;
			}

			return false;
		}

		private static bool IsChunk(string line)
		{
			return line.StartsWith(CHUNK_TAG);
		}

		private void ParseFile(string[] text, string path)
		{
			bool nextIsChunk = false;
			bool nextIsStream = false;
			int width = 0, height = 0, bitrate = 0;

			for (int i = 0; i < text.Length; ++i)
			{
				if (ExtractStreamInfo(text[i], ref width, ref height, ref bitrate))
				{
					nextIsStream = true;
					nextIsChunk = false;
				}
				else if (IsChunk(text[i]))
				{
					nextIsChunk = true;
					nextIsStream = false;
				}
				else if (nextIsChunk)
				{
					Chunk chunk;
					chunk.name = path + text[i];
					_chunks.Add(chunk);

					nextIsChunk = false;
					nextIsStream = false;
				}
				else if (nextIsStream)
				{
					try
					{
						string fullpath = text[i].IndexOf("://") < 0 ? path + text[i] : text[i];
						HLSStream stream = new HLSStream(fullpath, width, height, bitrate);
						_streams.Add(stream);
					}
					catch (Exception e)
					{
						Debug.LogError("[AVProVideo]HLSParser cannot parse stream " + path + text[i] + ", " + e.Message);
					}

					nextIsChunk = false;
					nextIsStream = false;
				}
				else
				{
					nextIsChunk = false;
					nextIsStream = false;
				}
			}
		}

		public HLSStream(string filename, int width = 0, int height = 0, int bandwidth = 0)
		{
			_streams = new List<Stream>();
			_chunks = new List<Chunk>();

			_width = width;
			_height = height;
			_bandwidth = bandwidth;
			_streamURL = filename;

			try
			{
				string[] fileLines = null;

				if (filename.ToLower().StartsWith("http://") || filename.ToLower().StartsWith("https://"))
				{
#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_WSA

#if UNITY_2017_2_OR_NEWER
					UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filename);
					AsyncOperation request = www.SendWebRequest();
#else
					WWW www = new WWW(filename);
#endif

					int watchdog = 0;
					while (!www.isDone && watchdog < 5000)
					{
#if NETFX_CORE
						System.Threading.Tasks.Task.Delay(1).Wait();
#else
						System.Threading.Thread.Sleep(1);
#endif
						watchdog++;
					}
					if (www.isDone && watchdog < 5000)
					{
#if UNITY_2017_2_OR_NEWER
						if (!request.isDone)
						{
							Debug.LogError("[AVProVideo] Failed to download subtitles: " + www.error);
						}
						else
						{
							fileLines = ((UnityEngine.Networking.DownloadHandler)www.downloadHandler).text.Split('\n');
						}
#else
						fileLines = www.text.Split('\n');
#endif
						
					}
					www.Dispose();
#else

#if SUPPORT_SSL
					if (filename.ToLower().StartsWith("https://"))
					{
						ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
					}
#endif
					using (System.Net.WebClient webClient = new System.Net.WebClient())
					{
						string fileString = webClient.DownloadString(filename);
						fileLines = fileString.Split('\n');
					}
#endif
				}
				else
				{
					fileLines = File.ReadAllLines(filename);
				}	

				int lastDash = filename.LastIndexOf('/');
				if(lastDash < 0)
				{
					lastDash = filename.LastIndexOf('\\');
				}

				string path = _streamURL.Substring(0, lastDash + 1);

				ParseFile(fileLines, path);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

#if SUPPORT_SSL
		private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain,
			// look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
					{
						continue;
					}
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
					// Note: change flags to X509VerificationFlags.AllFlags to skip all security checks
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
					bool chainIsValid = chain.Build((X509Certificate2)certificate);
					if (!chainIsValid)
					{
						isOk = false;
						break;
					}
				}
			}
			return isOk;
		}
#endif
	}
}

