#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#elif UNITY_IOS || UNITY_ANDROID
	#if UNITY_5_5_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4)
		#define UNITY_PLATFORM_SUPPORTS_LINEAR
	#endif
#endif
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
	/// Sets up a mesh to display the video from a MediaPlayer
	/// </summary>
	[AddComponentMenu("AVPro Video/Apply To Mesh", 300)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class ApplyToMesh : MonoBehaviour 
	{
		// TODO: add specific material / material index to target in the mesh if there are multiple materials

		[Header("Media Source")]

		[SerializeField]
		private MediaPlayer _media = null;

		public MediaPlayer Player
		{
			get { return _media; }
			set { ChangeMediaPlayer(value); }
		}

		[Tooltip("Default texture to display when the video texture is preparing")]
		[SerializeField]
		private Texture2D _defaultTexture = null;

		public Texture2D DefaultTexture
		{
			get { return _defaultTexture; }
			set { if (_defaultTexture != value) { _defaultTexture = value; _isDirty = true; } }
		}

		[Space(8f)]
		[Header("Renderer Target")]

		[SerializeField]
		private Renderer _mesh = null;

		public Renderer MeshRenderer
		{
			get { return _mesh; }
			set { if (_mesh != value) { _mesh = value; _isDirty = true; } }
		}

		[SerializeField]
		private string _texturePropertyName = "_MainTex";

		public string TexturePropertyName
		{
			get { return _texturePropertyName; }
			set
			{
				if (_texturePropertyName != value)
				{
					_texturePropertyName = value;
#if UNITY_5_6_OR_NEWER
					_propTexture = Shader.PropertyToID(_texturePropertyName);
#endif
					_isDirty = true;
				}
			}
		}

		[SerializeField]
		private Vector2 _offset = Vector2.zero;

		public Vector2 Offset
		{
			get { return _offset; }
			set { if (_offset != value) { _offset = value; _isDirty = true; } }
		}

		[SerializeField]
		private Vector2 _scale = Vector2.one;

		public Vector2 Scale
		{
			get { return _scale; }
			set { if (_scale != value) { _scale = value; _isDirty = true; } }
		}

		private bool _isDirty = false;
		private Texture _lastTextureApplied;
#if UNITY_5_6_OR_NEWER
		private int _propTexture;
#endif

		private static int _propStereo;
		private static int _propAlphaPack;
		private static int _propApplyGamma;
		private static int _propLayout;

		private const string PropChromaTexName = "_ChromaTex";
		private static  int _propChromaTex;
		private const string PropYpCbCrTransformName = "_YpCbCrTransform";
		private static int _propYpCbCrTransform;
		private const string PropUseYpCbCrName = "_UseYpCbCr";
		private static int _propUseYpCbCr;

		private void Awake()
		{
			if (_propStereo == 0)
			{
				_propStereo = Shader.PropertyToID("Stereo");
			}
			if (_propAlphaPack == 0)
			{
				_propAlphaPack = Shader.PropertyToID("AlphaPack");
			}
			if (_propApplyGamma == 0)
			{
				_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
			}
			if (_propLayout == 0)
			{
				_propLayout = Shader.PropertyToID("Layout");
			}
			if (_propChromaTex == 0)
			{
				_propChromaTex = Shader.PropertyToID(PropChromaTexName);
			}
			if (_propYpCbCrTransform == 0)
			{
				_propYpCbCrTransform = Shader.PropertyToID(PropYpCbCrTransformName);
			}
			if (_propUseYpCbCr == 0)
			{
				_propUseYpCbCr = Shader.PropertyToID(PropUseYpCbCrName);
			}

			if (_media != null)
			{
				_media.Events.AddListener(OnMediaPlayerEvent);
			}
		}

		public void ForceUpdate()
		{
			_isDirty = true;
			LateUpdate();
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.FirstFrameReady:
				case MediaPlayerEvent.EventType.PropertiesChanged:
					ForceUpdate();
					break;
			}
		}

		private void ChangeMediaPlayer(MediaPlayer player)
		{
			if (_media != player)
			{
				if (_media != null)
				{
					_media.Events.RemoveListener(OnMediaPlayerEvent);
				}
				_media = player;
				if (_media != null)
				{
					_media.Events.AddListener(OnMediaPlayerEvent);
				}
				_isDirty = true; 
			}
		}

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		private void LateUpdate()
		{
			bool applied = false;

			// Try to apply texture from media
			if (_media != null && _media.TextureProducer != null)
			{
				Texture resamplerTex = _media.FrameResampler == null || _media.FrameResampler.OutputTexture == null ? null : _media.FrameResampler.OutputTexture[0];
				Texture texture = _media.m_Resample ? resamplerTex : _media.TextureProducer.GetTexture(0);
				if (texture != null)
				{
					// Check for changing texture
					if (texture != _lastTextureApplied)
					{
						_isDirty = true;
					}

					if (_isDirty)
					{
						int planeCount = _media.m_Resample ? 1 : _media.TextureProducer.GetTextureCount();
						for (int plane = 0; plane < planeCount; plane++)
						{
							Texture resamplerTexPlane = _media.FrameResampler == null || _media.FrameResampler.OutputTexture == null ? null : _media.FrameResampler.OutputTexture[plane];
							texture = _media.m_Resample ? resamplerTexPlane : _media.TextureProducer.GetTexture(plane);
							if (texture != null)
							{
								ApplyMapping(texture, _media.TextureProducer.RequiresVerticalFlip(), plane);
							}
						}
					}
					applied = true;
				}
			}

			// If the media didn't apply a texture, then try to apply the default texture
			if (!applied)
			{
				if (_defaultTexture != _lastTextureApplied)
				{
					_isDirty = true;
				}
				if (_isDirty)
				{
					ApplyMapping(_defaultTexture, false);
				}
			}
		}
		
		private void ApplyMapping(Texture texture, bool requiresYFlip, int plane = 0)
		{
			if (_mesh != null)
			{
				_isDirty = false;

				Material[] meshMaterials = _mesh.materials;
				if (meshMaterials != null)
				{
					for (int i = 0; i < meshMaterials.Length; i++)
					{
						Material mat = meshMaterials[i];
						if (mat != null)
						{
							if (plane == 0)
							{
#if UNITY_5_6_OR_NEWER
								mat.SetTexture(_propTexture, texture);
#else
								mat.SetTexture(_texturePropertyName, texture);
#endif

								_lastTextureApplied = texture;

								if (texture != null)
								{
#if UNITY_5_6_OR_NEWER
									if (requiresYFlip)
									{
										mat.SetTextureScale(_propTexture, new Vector2(_scale.x, -_scale.y));
										mat.SetTextureOffset(_propTexture, Vector2.up + _offset);
									}
									else
									{
										mat.SetTextureScale(_propTexture, _scale);
										mat.SetTextureOffset(_propTexture, _offset);
									}
#else
									if (requiresYFlip)
									{
										mat.SetTextureScale(_texturePropertyName, new Vector2(_scale.x, -_scale.y));
										mat.SetTextureOffset(_texturePropertyName, Vector2.up + _offset);
									}
									else
									{
										mat.SetTextureScale(_texturePropertyName, _scale);
										mat.SetTextureOffset(_texturePropertyName, _offset);
									}
#endif
								}
							}
							else if (plane == 1)
							{
								if (mat.HasProperty(_propUseYpCbCr) && mat.HasProperty(_propChromaTex))
								{
									mat.EnableKeyword("USE_YPCBCR");
									mat.SetTexture(_propChromaTex, texture);
									mat.SetMatrix(_propYpCbCrTransform, _media.TextureProducer.GetYpCbCrTransform());
#if UNITY_5_6_OR_NEWER
									if (requiresYFlip)
									{
										mat.SetTextureScale(_propChromaTex, new Vector2(_scale.x, -_scale.y));
										mat.SetTextureOffset(_propChromaTex, Vector2.up + _offset);
									}
									else
									{
										mat.SetTextureScale(_propChromaTex, _scale);
										mat.SetTextureOffset(_propChromaTex, _offset);
									}
#else
									if (requiresYFlip)
									{
										mat.SetTextureScale(PropChromaTexName, new Vector2(_scale.x, -_scale.y));
										mat.SetTextureOffset(PropChromaTexName, Vector2.up + _offset);
									}
									else
									{
										mat.SetTextureScale(PropChromaTexName, _scale);
										mat.SetTextureOffset(PropChromaTexName, _offset);
									}
#endif
								}
							}

							if (_media != null)
							{
								// Apply changes for layout
								if (mat.HasProperty(_propLayout))
								{
									Helper.SetupLayoutMaterial(mat, _media.VideoLayoutMapping);
								}
								// Apply changes for stereo videos
								if (mat.HasProperty(_propStereo))
								{
									Helper.SetupStereoMaterial(mat, _media.m_StereoPacking, _media.m_DisplayDebugStereoColorTint);
								}
								// Apply changes for alpha videos
								if (mat.HasProperty(_propAlphaPack))
								{
									Helper.SetupAlphaPackedMaterial(mat, _media.m_AlphaPacking);
								}
#if UNITY_PLATFORM_SUPPORTS_LINEAR
								// Apply gamma
								if (mat.HasProperty(_propApplyGamma))
								{
									if (texture == _defaultTexture || _media.Info == null)
									{
										Helper.SetupGammaMaterial(mat, true);
									}
									else
									{
										Helper.SetupGammaMaterial(mat, _media.Info.PlayerSupportsLinearColorSpace());
									}
								}
#else
								_propApplyGamma |= 0;
#endif
							}
						}
					}
				}
			}
		}

		private void OnEnable()
		{
			if (_mesh == null)
			{
				_mesh = this.GetComponent<MeshRenderer>();
				if (_mesh == null)
				{
					Debug.LogWarning("[AVProVideo] No mesh renderer set or found in gameobject");
				}
			}

#if UNITY_5_6_OR_NEWER
			_propTexture = Shader.PropertyToID(_texturePropertyName);
#endif

			_isDirty = true;
			if (_mesh != null)
			{
				LateUpdate();
			}
		}
		
		private void OnDisable()
		{
			ApplyMapping(_defaultTexture, false);
		}
	}
}