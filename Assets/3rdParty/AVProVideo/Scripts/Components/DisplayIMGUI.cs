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
	/// Displays the video from MediaPlayer component using IMGUI
	/// </summary>
	[AddComponentMenu("AVPro Video/Display IMGUI", 200)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	[ExecuteInEditMode]
	public class DisplayIMGUI : MonoBehaviour
	{
		private const string PropChromaTexName = "_ChromaTex";
		private const string PropYpCbCrTransformName = "_YpCbCrTransform";

		public MediaPlayer	_mediaPlayer;

		public bool			_displayInEditor = true;
		public ScaleMode	_scaleMode	= ScaleMode.ScaleToFit;
		public Color		_color		= Color.white;
		public bool			_alphaBlend	= false;

		[SerializeField]
		private bool		_useDepth = false;

		public int			_depth = 0;
		public bool			_fullScreen	= true;
		[Range(0f, 1f)]
		public float		_x			= 0.0f;
		[Range(0f, 1f)]
		public float		_y			= 0.0f;
		[Range(0f, 1f)]
		public float		_width		= 1.0f;
		[Range(0f, 1f)]
		public float		_height		= 1.0f;

		private static int		_propAlphaPack;
		private static int		_propVertScale;
		private static int		_propApplyGamma;
		private static int		_propChromaTex;
		private static int		_propYpCbCrTransform;
		private static Shader	_shaderAlphaPacking;
		private Material		_material;

		void Awake()
		{
			if (_propAlphaPack == 0)
			{
				_propAlphaPack = Shader.PropertyToID("AlphaPack");
				_propVertScale = Shader.PropertyToID("_VertScale");
				_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
				_propChromaTex = Shader.PropertyToID(PropChromaTexName);
				_propYpCbCrTransform = Shader.PropertyToID(PropYpCbCrTransformName);
			}
		}

		void Start()
		{
			// Disabling this lets you skip the GUI layout phase which helps performance, but this also breaks the GUI.depth usage.
			if (!_useDepth)
			{
				this.useGUILayout = false;
			}

			if (_shaderAlphaPacking == null)
			{
				_shaderAlphaPacking = Shader.Find("AVProVideo/IMGUI/Texture Transparent");
				if (_shaderAlphaPacking == null)
				{
					Debug.LogWarning("[AVProVideo] Missing shader AVProVideo/IMGUI/Transparent Packed");
				}
			}
		}

		void OnDestroy()
		{
			// Destroy existing material
			if (_material != null)
			{
#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}
		}

		private Shader GetRequiredShader()
		{
			Shader result = null;

			switch (_mediaPlayer.m_AlphaPacking)
			{
				case AlphaPacking.None:
					break;
				case AlphaPacking.LeftRight:
				case AlphaPacking.TopBottom:
					result = _shaderAlphaPacking;
					break;
			}

#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (result == null && _mediaPlayer.Info != null)
			{
				// If the player does support generating sRGB textures then we need to use a shader to convert them for display via IMGUI
				if (QualitySettings.activeColorSpace == ColorSpace.Linear && _mediaPlayer.Info.PlayerSupportsLinearColorSpace())
				{
					result = _shaderAlphaPacking;
				}
			}
#endif
			if (result == null && _mediaPlayer.TextureProducer != null)
			{
				if (_mediaPlayer.TextureProducer.GetTextureCount() == 2)
				{
					result = _shaderAlphaPacking;
				}
			}
			return result;
		}

		void Update()
		{
			if (_mediaPlayer != null)
			{
				// Get required shader
				Shader currentShader = null;
				if (_material != null)
				{
					currentShader = _material.shader;
				}
				Shader nextShader = GetRequiredShader();

				// If the shader requirement has changed
				if (currentShader != nextShader)
				{
					// Destroy existing material
					if (_material != null)
					{
#if UNITY_EDITOR
						Material.DestroyImmediate(_material);
#else
						Material.Destroy(_material);
#endif
						_material = null;
					}

					// Create new material
					if (nextShader != null)
					{
						_material = new Material(nextShader);
					}
				}

				// Apply material changes
				if (_material != null)
				{
					if (_material.HasProperty(_propAlphaPack))
					{
						Helper.SetupAlphaPackedMaterial(_material, _mediaPlayer.m_AlphaPacking);
					}
#if UNITY_PLATFORM_SUPPORTS_LINEAR
					// Apply gamma
					if (_material.HasProperty(_propApplyGamma) && _mediaPlayer.Info != null)
					{
						Helper.SetupGammaMaterial(_material, _mediaPlayer.Info.PlayerSupportsLinearColorSpace());
					}
#else
					_propApplyGamma |= 0;
#endif
				}

			}
		}

		void OnGUI()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && _displayInEditor)
			{
				GUI.depth = _depth;
				GUI.color = _color;
				Rect rect = GetRect();
				Texture2D icon = Resources.Load<Texture2D>("AVProVideoIcon");
				Rect uv = rect;
				uv.x /= Screen.width;
				uv.width /= Screen.width;
				uv.y /= Screen.height;
				uv.height /= Screen.height;
				uv.width *= 16f;
				uv.height *= 16f;
				uv.x += 0.5f;
				uv.y += 0.5f;
				GUI.DrawTextureWithTexCoords(rect, icon, uv);
				return;
			}
#endif

			if (_mediaPlayer == null)
			{
				return;
			}

			bool requiresVerticalFlip = false;
			Texture texture = null;

			if (_displayInEditor)
			{
#if UNITY_EDITOR
				texture = Texture2D.whiteTexture;
#endif
			}

			if (_mediaPlayer.Info != null && !_mediaPlayer.Info.HasVideo())
			{
				texture = null;
			}

			if (_mediaPlayer.TextureProducer != null)
			{
				if (_mediaPlayer.m_Resample)
				{
					if (_mediaPlayer.FrameResampler.OutputTexture != null && _mediaPlayer.FrameResampler.OutputTexture[0] != null)
					{
						texture = _mediaPlayer.FrameResampler.OutputTexture[0];
						requiresVerticalFlip = _mediaPlayer.TextureProducer.RequiresVerticalFlip();
					}
				}
				else
				{
					if (_mediaPlayer.TextureProducer.GetTexture() != null)
					{
						texture = _mediaPlayer.TextureProducer.GetTexture();
						requiresVerticalFlip = _mediaPlayer.TextureProducer.RequiresVerticalFlip();
					}
				}

				if (_mediaPlayer.TextureProducer.GetTextureCount() == 2 && _material != null)
				{
					Texture resamplerTex = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[1];
					Texture chroma = _mediaPlayer.m_Resample ? resamplerTex : _mediaPlayer.TextureProducer.GetTexture(1);
					_material.SetTexture(_propChromaTex, chroma);
					_material.SetMatrix(_propYpCbCrTransform, _mediaPlayer.TextureProducer.GetYpCbCrTransform());
					_material.EnableKeyword("USE_YPCBCR");
				}
			}

			if (texture != null)
			{
				if (!_alphaBlend || _color.a > 0f)
				{
					GUI.depth = _depth;
					GUI.color = _color;

					Rect rect = GetRect();

					if (_material != null)
					{
						if (requiresVerticalFlip)
						{
							_material.SetFloat(_propVertScale, -1f);
						}
						else
						{
							_material.SetFloat(_propVertScale, 1f);
						}
						Helper.DrawTexture(rect, texture, _scaleMode, _mediaPlayer.m_AlphaPacking, _material);
					}
					else
					{
						if (requiresVerticalFlip)
						{
							GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0f, rect.y + (rect.height / 2f)));
						}
						GUI.DrawTexture(rect, texture, _scaleMode, _alphaBlend);
					}
				}
			}
		}

		public Rect GetRect()
		{
			Rect rect;
			if (_fullScreen)
			{
				rect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
			}
			else
			{
				rect = new Rect(_x * (Screen.width - 1), _y * (Screen.height - 1), _width * Screen.width, _height * Screen.height);
			}

			return rect;
		}
	}
}
