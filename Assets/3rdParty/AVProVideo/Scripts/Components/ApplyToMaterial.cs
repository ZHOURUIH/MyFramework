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
	/// Sets up a material to display the video from a MediaPlayer
	/// </summary>
	[AddComponentMenu("AVPro Video/Apply To Material", 300)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class ApplyToMaterial : MonoBehaviour
	{
		public Vector2 _offset = Vector2.zero;
		public Vector2 _scale = Vector2.one;
		public Material _material;
		public string _texturePropertyName;
		public MediaPlayer _media;
		public Texture2D _defaultTexture;

		private Texture _originalTexture;
		private Vector2 _originalScale = Vector2.one;
		private Vector2 _originalOffset = Vector2.zero;

		private static int _propStereo;
		private static int _propAlphaPack;
		private static int _propApplyGamma;
		private static int _propLayout;

		private const string PropChromaTexName = "_ChromaTex";
		private static int _propChromaTex;

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
		}

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		private void LateUpdate()
		{
			bool applied = false;

			if (_media != null && _media.TextureProducer != null)
			{
				int planeCount = _media.m_Resample ? 1 : _media.TextureProducer.GetTextureCount();
				for (int plane = 0; plane < planeCount; ++plane)
				{
					Texture resamplerTex = _media.FrameResampler == null || _media.FrameResampler.OutputTexture == null ? null : _media.FrameResampler.OutputTexture[plane];
					Texture texture = _media.m_Resample ? resamplerTex : _media.TextureProducer.GetTexture(plane);
					if (texture != null)
					{
						ApplyMapping(texture, _media.TextureProducer.RequiresVerticalFlip(), plane);
						applied = true;
					}
				}
			}

			if (!applied)
			{
				if (_material != null && _material.HasProperty(_propUseYpCbCr))
				{
					_material.DisableKeyword("USE_YPCBCR");
				}
				ApplyMapping(_defaultTexture, false);
			}
		}

		private void ApplyMapping(Texture texture, bool requiresYFlip, int plane = 0)
		{
			if (_material != null)
			{
				if (plane == 0)
				{
					if (string.IsNullOrEmpty(_texturePropertyName))
					{
						_material.mainTexture = texture;

						if (texture != null)
						{
							if (requiresYFlip)
							{
								_material.mainTextureScale = new Vector2(_scale.x, -_scale.y);
								_material.mainTextureOffset = Vector2.up + _offset;
							}
							else
							{
								_material.mainTextureScale = _scale;
								_material.mainTextureOffset = _offset;
							}
						}
					}
					else
					{
						_material.SetTexture(_texturePropertyName, texture);

						if (texture != null)
						{
							if (requiresYFlip)
							{
								_material.SetTextureScale(_texturePropertyName, new Vector2(_scale.x, -_scale.y));
								_material.SetTextureOffset(_texturePropertyName, Vector2.up + _offset);
							}
							else
							{
								_material.SetTextureScale(_texturePropertyName, _scale);
								_material.SetTextureOffset(_texturePropertyName, _offset);
							}
						}
					}
				}
				else if (plane == 1)
				{
					if (_material.HasProperty(_propUseYpCbCr))
					{
						_material.EnableKeyword("USE_YPCBCR");
					}
					if (_material.HasProperty(_propChromaTex))
					{
						_material.SetTexture(_propChromaTex, texture);
						_material.SetMatrix(_propYpCbCrTransform, _media.TextureProducer.GetYpCbCrTransform());
						if (texture != null)
						{
							#if UNITY_5_6_OR_NEWER
							if (requiresYFlip)
							{
								_material.SetTextureScale(_propChromaTex, new Vector2(_scale.x, -_scale.y));
								_material.SetTextureOffset(_propChromaTex, Vector2.up + _offset);
							}
							else
							{
								_material.SetTextureScale(_propChromaTex, _scale);
								_material.SetTextureOffset(_propChromaTex, _offset);
							}
							#else
							if (requiresYFlip)
							{
								_material.SetTextureScale(PropChromaTexName, new Vector2(_scale.x, -_scale.y));
								_material.SetTextureOffset(PropChromaTexName, Vector2.up + _offset);
							}
							else
							{
								_material.SetTextureScale(PropChromaTexName, _scale);
								_material.SetTextureOffset(PropChromaTexName, _offset);
							}
							#endif
						}
					}
				}

				if (_media != null)
				{
					// Apply changes for layout
					if (_material.HasProperty(_propLayout))
					{
						Helper.SetupLayoutMaterial(_material, _media.VideoLayoutMapping);
					}	
					// Apply changes for stereo videos
					if (_material.HasProperty(_propStereo))
					{
						Helper.SetupStereoMaterial(_material, _media.m_StereoPacking, _media.m_DisplayDebugStereoColorTint);
					}
					// Apply changes for alpha videos
					if (_material.HasProperty(_propAlphaPack))
					{
						Helper.SetupAlphaPackedMaterial(_material, _media.m_AlphaPacking);
					}
#if UNITY_PLATFORM_SUPPORTS_LINEAR
					// Apply gamma
					if (_material.HasProperty(_propApplyGamma) && _media.Info != null)
					{
						Helper.SetupGammaMaterial(_material, _media.Info.PlayerSupportsLinearColorSpace());
					}
#else
					_propApplyGamma |= 0;
#endif
				}
			}
		}

		private void Start()
		{
			SaveProperties();
			LateUpdate();
		}

		private void OnEnable()
		{
			SaveProperties();
			LateUpdate();
		}

		private void OnDisable()
		{
			RestoreProperties();
		}

		private void SaveProperties()
		{
			if (_material != null)
			{
				if (string.IsNullOrEmpty(_texturePropertyName))
				{
					_originalTexture = _material.mainTexture;
					_originalScale = _material.mainTextureScale;
					_originalOffset = _material.mainTextureOffset;
				}
				else
				{
					_originalTexture = _material.GetTexture(_texturePropertyName);
					_originalScale = _material.GetTextureScale(_texturePropertyName);
					_originalOffset = _material.GetTextureOffset(_texturePropertyName);
				}
			}
		}

		private void RestoreProperties()
		{
			if (_material != null)
			{
				if (string.IsNullOrEmpty(_texturePropertyName))
				{
					_material.mainTexture = _originalTexture;
					_material.mainTextureScale = _originalScale;
					_material.mainTextureOffset = _originalOffset;
				}
				else
				{
					_material.SetTexture(_texturePropertyName, _originalTexture);
					_material.SetTextureScale(_texturePropertyName, _originalScale);
					_material.SetTextureOffset(_texturePropertyName, _originalOffset);
				}
			}
		}
	}
}