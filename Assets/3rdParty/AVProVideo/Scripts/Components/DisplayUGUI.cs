#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5 || UNITY_5_4_OR_NEWER
	#define UNITY_FEATURE_UGUI
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
	#define REAL_ANDROID
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#elif UNITY_IOS || UNITY_ANDROID
	#if UNITY_5_5_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4)
		#define UNITY_PLATFORM_SUPPORTS_LINEAR
	#endif
#endif

#if (!UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN) && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID)
	#define UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
#endif

// Some older versions of Unity don't set the _TexelSize variable from uGUI so we need to set this manually
#if ((!UNITY_5_4_OR_NEWER && !UNITY_5) || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3_0 || UNITY_5_3_1 || UNITY_5_3_2 || UNITY_5_3_3)
	#define UNITY_UGUI_NOSET_TEXELSIZE
#endif

#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0)
	#define UNITY_HELPATTRIB
#endif

using System.Collections.Generic;
#if UNITY_FEATURE_UGUI
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Displays the video from MediaPlayer component using uGUI
	/// </summary>
	[ExecuteInEditMode]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	[AddComponentMenu("AVPro Video/Display uGUI", 200)]
	public class DisplayUGUI : UnityEngine.UI.MaskableGraphic
	{
		[SerializeField]
		public MediaPlayer _mediaPlayer;

		[SerializeField]
		public Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

		[SerializeField]
		public bool _setNativeSize = false;

		[SerializeField]
		public ScaleMode _scaleMode = ScaleMode.ScaleToFit;

		[SerializeField]
		public bool _noDefaultDisplay = true;

		[SerializeField]
		public bool _displayInEditor = true;

		[SerializeField]
		public Texture _defaultTexture;

		private int _lastWidth;
		private int _lastHeight;
		private bool _flipY;
		private Texture _lastTexture;
		private static Shader _shaderStereoPacking;
		private static Shader _shaderAlphaPacking;
#if REAL_ANDROID
		private static Shader _shaderAndroidOES;
#endif
		private static int _propAlphaPack;
		private static int _propVertScale;
		private static int _propStereo;
		private static int _propApplyGamma;
		private static int _propUseYpCbCr;
		private const string PropChromaTexName = "_ChromaTex";
		private static int _propChromaTex;
		private const string PropYpCbCrTransformName = "_YpCbCrTransform";
		private static int _propYpCbCrTransform;

#if UNITY_UGUI_NOSET_TEXELSIZE
		private static int _propMainTextureTexelSize;
#endif
		private bool _userMaterial = true;
		private Material _material;

#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2_0 && !UNITY_5_2_1)
		private List<UIVertex> _vertices = new List<UIVertex>(4);
		private static List<int> QuadIndices = new List<int>(new int[] { 0, 1, 2, 2, 3, 0 });
#endif

		protected override void Awake()
		{
			if (_propAlphaPack == 0)
			{
				_propStereo = Shader.PropertyToID("Stereo");
				_propAlphaPack = Shader.PropertyToID("AlphaPack");
				_propVertScale = Shader.PropertyToID("_VertScale");
				_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
				_propUseYpCbCr = Shader.PropertyToID("_UseYpCbCr");
				_propChromaTex = Shader.PropertyToID(PropChromaTexName);
				_propUseYpCbCr = Shader.PropertyToID("_UseYpCbCr");
				_propYpCbCrTransform = Shader.PropertyToID(PropYpCbCrTransformName);
#if UNITY_UGUI_NOSET_TEXELSIZE
				_propMainTextureTexelSize = Shader.PropertyToID("_MainTex_TexelSize");
#endif
			}

#if UNITY_IOS
			bool hasMask = HasMask(gameObject);
			if (hasMask)
			{
				Debug.LogWarning("[AVProVideo] Using DisplayUGUI with a Mask necessitates disabling YpCbCr mode on the MediaPlayer. Memory consumption will increase.");
				_mediaPlayer.PlatformOptionsIOS.useYpCbCr420Textures = false;
			}
#endif

			base.Awake();
		}

		private static bool HasMask(GameObject obj)
		{
			if (obj.GetComponent<Mask>() != null)
				return true;
			if (obj.transform.parent != null)
				return HasMask(obj.transform.parent.gameObject);
			return false;
		}

		private static Shader EnsureShader(Shader shader, string name)
		{
			if (shader == null)
			{
				shader = Shader.Find(name);
				if (shader == null)
				{
					Debug.LogWarning("[AVProVideo] Missing shader " + name);
				}
			}

			return shader;
		}

		private static Shader EnsureAlphaPackingShader()
		{
			_shaderAlphaPacking = EnsureShader(_shaderAlphaPacking, "AVProVideo/UI/Transparent Packed");
			return _shaderAlphaPacking;
		}

		private static Shader EnsureStereoPackingShader()
		{
			_shaderStereoPacking = EnsureShader(_shaderStereoPacking, "AVProVideo/UI/Stereo");
			return _shaderStereoPacking;
		}

#if REAL_ANDROID
		private Shader EnsureAndroidOESShader()
		{
			_shaderAndroidOES = EnsureShader(_shaderAndroidOES, "AVProVideo/UI/AndroidOES");
			return _shaderAndroidOES;
		}
#endif

		protected override void Start()
		{
			_userMaterial = (this.m_Material != null);
			if (_userMaterial) {
				_material = new Material(this.material);
				this.material = _material;
			}

			base.Start();
		}

		
		protected override void OnDestroy()
		{
			// Destroy existing material
			if (_material != null)
			{
				this.material = null;

#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}
			base.OnDestroy();
		}

		private Shader GetRequiredShader()
		{
			Shader result = null;
			
			switch (_mediaPlayer.m_StereoPacking)
			{
				case StereoPacking.None:
					break;
				case StereoPacking.LeftRight:
				case StereoPacking.TopBottom:
					result = EnsureStereoPackingShader();
					break;
			}

			switch (_mediaPlayer.m_AlphaPacking)
			{
				case AlphaPacking.None:
					break;
				case AlphaPacking.LeftRight:
				case AlphaPacking.TopBottom:
					result = EnsureAlphaPackingShader();
					break;
			}

#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (result == null && _mediaPlayer.Info != null)
			{
				if (QualitySettings.activeColorSpace == ColorSpace.Linear && !_mediaPlayer.Info.PlayerSupportsLinearColorSpace())
				{
					result = EnsureAlphaPackingShader();
				}
			}
#endif
			if (result == null && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTextureCount() == 2)
			{
				result = EnsureAlphaPackingShader();
			}

#if REAL_ANDROID
			if (_mediaPlayer.PlatformOptionsAndroid.useFastOesPath)
			{
				result = EnsureAndroidOESShader();
			}
#endif
			return result;
		}

		/// <summary>
		/// Returns the texture used to draw this Graphic.
		/// </summary>
		public override Texture mainTexture
		{
			get
			{
				Texture result = Texture2D.whiteTexture;
				if (HasValidTexture())
				{
					Texture resamplerTex = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[0];
					result = _mediaPlayer.m_Resample ? resamplerTex : _mediaPlayer.TextureProducer.GetTexture();
				}
				else
				{
					if (_noDefaultDisplay)
					{
						result = null;
					}
					else if (_defaultTexture != null)
					{
						result = _defaultTexture;
					}

#if UNITY_EDITOR
					if (result == null && _displayInEditor)
					{
						result = Resources.Load<Texture2D>("AVProVideoIcon");
					}
#endif
				}
				return result;
			}
		}

		public bool HasValidTexture()
		{
			return (_mediaPlayer != null && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTexture() != null);
		}

		private void UpdateInternalMaterial()
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
						this.material = null;
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

				this.material = _material;
			}
		}

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		void LateUpdate()
		{
			if (_setNativeSize)
			{
				SetNativeSize();
			}

			if (_lastTexture != mainTexture)
			{
				_lastTexture = mainTexture;
				SetVerticesDirty();
				SetMaterialDirty();
			}

			if (HasValidTexture())
			{
				if (mainTexture != null)
				{
					if (mainTexture.width != _lastWidth || mainTexture.height != _lastHeight)
					{
						_lastWidth = mainTexture.width;
						_lastHeight = mainTexture.height;
						SetVerticesDirty();
						SetMaterialDirty();
					}
				}
			}

			if (!_userMaterial && Application.isPlaying)
			{
				UpdateInternalMaterial();
			}

			if (material != null && _mediaPlayer != null)
			{
				// YpCbCr support
				if (material.HasProperty(_propUseYpCbCr) && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTextureCount() == 2)
				{
					material.EnableKeyword("USE_YPCBCR");
					material.SetMatrix(_propYpCbCrTransform, _mediaPlayer.TextureProducer.GetYpCbCrTransform());
					Texture resamplerTex = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[1];
					material.SetTexture(_propChromaTex, _mediaPlayer.m_Resample ? resamplerTex : _mediaPlayer.TextureProducer.GetTexture(1));
				}

				// Apply changes for alpha videos
				if (material.HasProperty(_propAlphaPack))
				{
					Helper.SetupAlphaPackedMaterial(material, _mediaPlayer.m_AlphaPacking);

					if (_flipY && _mediaPlayer.m_AlphaPacking != AlphaPacking.None)
					{
						material.SetFloat(_propVertScale, -1f);
					}
					else
					{
						material.SetFloat(_propVertScale, 1f);
					}

#if UNITY_UGUI_NOSET_TEXELSIZE
					if (mainTexture != null)
					{
						material.SetVector(_propMainTextureTexelSize, new Vector4(1.0f / mainTexture.width, 1.0f / mainTexture.height, mainTexture.width, mainTexture.height));
					}
#endif
				}

				// Apply changes for stereo videos
				if (material.HasProperty(_propStereo))
				{
					Helper.SetupStereoMaterial(material, _mediaPlayer.m_StereoPacking, _mediaPlayer.m_DisplayDebugStereoColorTint);
				}
#if UNITY_PLATFORM_SUPPORTS_LINEAR
				if (material.HasProperty(_propApplyGamma) && _mediaPlayer.Info != null)
				{
					Helper.SetupGammaMaterial(material, _mediaPlayer.Info.PlayerSupportsLinearColorSpace());
				}
#else
				_propApplyGamma |= 0;
#endif
			}
		}

		/// <summary>
		/// Texture to be used.
		/// </summary>
		public MediaPlayer CurrentMediaPlayer
		{
			get
			{
				return _mediaPlayer;
			}
			set
			{
				if (_mediaPlayer != value)
				{
					_mediaPlayer = value;
					//SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		/// <summary>
		/// UV rectangle used by the texture.
		/// </summary>
		public Rect uvRect
		{
			get
			{
				return m_UVRect;
			}
			set
			{
				if (m_UVRect == value)
				{
					return;
				}
				m_UVRect = value;
				SetVerticesDirty();
			}
		}

		/// <summary>
		/// Adjust the scale of the Graphic to make it pixel-perfect.
		/// </summary>
		[ContextMenu("Set Native Size")]
		public override void SetNativeSize()
		{
			Texture tex = mainTexture;
			if (tex != null)
			{
				int w = Mathf.RoundToInt(tex.width * uvRect.width);
				int h = Mathf.RoundToInt(tex.height * uvRect.height);

				if (_mediaPlayer != null)
				{
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM && !REAL_ANDROID
					if (_mediaPlayer.Info != null)
					{
						Orientation ori = Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform());
						if (ori == Orientation.Portrait || ori == Orientation.PortraitFlipped)
						{
							w = Mathf.RoundToInt(tex.height * uvRect.width);
							h = Mathf.RoundToInt(tex.width * uvRect.height);
						}
					}
#endif
					if (_mediaPlayer.m_AlphaPacking == AlphaPacking.LeftRight || _mediaPlayer.m_StereoPacking == StereoPacking.LeftRight)
					{
						w /= 2;
					}
					else if (_mediaPlayer.m_AlphaPacking == AlphaPacking.TopBottom || _mediaPlayer.m_StereoPacking == StereoPacking.TopBottom)
					{
						h /= 2;
					}
				}

				rectTransform.anchorMax = rectTransform.anchorMin;
				rectTransform.sizeDelta = new Vector2(w, h);
			}
		}

		/// <summary>
		/// Update all renderer data.
		/// </summary>
		// OnFillVBO deprecated by 5.2
		// OnPopulateMesh(Mesh mesh) deprecated by 5.2 patch 1
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2_0)
/*		protected override void OnPopulateMesh(Mesh mesh)
		{			
			List<UIVertex> verts = new List<UIVertex>();
			_OnFillVBO( verts );

			var quad = new UIVertex[4];
			for (int i = 0; i < vbo.Count; i += 4)
			{
				vbo.CopyTo(i, quad, 0, 4);
				vh.AddUIVertexQuad(quad);
			}
			vh.FillMesh( toFill );
		}*/

#if !UNITY_5_2_1
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			_OnFillVBO(_vertices);

			vh.AddUIVertexStream(_vertices, QuadIndices );
		}
#endif
#endif

#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1)
		[System.Obsolete("This method is not called from Unity 5.2 and above")]
#endif
		protected override void OnFillVBO(List<UIVertex> vbo)
		{
			_OnFillVBO(vbo);
		}

		private void _OnFillVBO(List<UIVertex> vbo)
		{
			_flipY = false;
			if (HasValidTexture())
			{
				_flipY = _mediaPlayer.TextureProducer.RequiresVerticalFlip();
			}

			Rect uvRect = m_UVRect;
			Vector4 v = GetDrawingDimensions(_scaleMode, ref uvRect);

#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			Matrix4x4 m = Matrix4x4.identity;
			if (HasValidTexture())
			{
				m = Helper.GetMatrixForOrientation(Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform()));
			}
#endif

			vbo.Clear();

			var vert = UIVertex.simpleVert;
			vert.color = color;

			vert.position = new Vector2(v.x, v.y);

			vert.uv0 = new Vector2(uvRect.xMin, uvRect.yMin);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMin, 1.0f - uvRect.yMin);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.x, v.w);
			vert.uv0 = new Vector2(uvRect.xMin, uvRect.yMax);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMin, 1.0f - uvRect.yMax);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.z, v.w);
			vert.uv0 = new Vector2(uvRect.xMax, uvRect.yMax);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMax, 1.0f - uvRect.yMax);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.z, v.y);
			vert.uv0 = new Vector2(uvRect.xMax, uvRect.yMin);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMax, 1.0f - uvRect.yMin);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);
		}

		private Vector4 GetDrawingDimensions(ScaleMode scaleMode, ref Rect uvRect)
		{
			Vector4 returnSize = Vector4.zero;

			if (mainTexture != null)
			{
				var padding = Vector4.zero;

				var textureSize = new Vector2(mainTexture.width, mainTexture.height);
				{
					// Adjust textureSize based on orientation
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM && !REAL_ANDROID
					if (HasValidTexture())
					{
						Matrix4x4 m = Helper.GetMatrixForOrientation(Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform()));
						textureSize = m.MultiplyVector(textureSize);
						textureSize.x = Mathf.Abs(textureSize.x);
						textureSize.y = Mathf.Abs(textureSize.y);
					}
#endif
					// Adjust textureSize based on alpha packing
					if (_mediaPlayer != null)
					{
						if (_mediaPlayer.m_AlphaPacking == AlphaPacking.LeftRight || _mediaPlayer.m_StereoPacking == StereoPacking.LeftRight)
						{
							textureSize.x /= 2f;
						}
						else if (_mediaPlayer.m_AlphaPacking == AlphaPacking.TopBottom || _mediaPlayer.m_StereoPacking == StereoPacking.TopBottom)
						{
							textureSize.y /= 2f;
						}
					}
				}
				
				Rect r = GetPixelAdjustedRect();

				// Fit the above textureSize into rectangle r
				int spriteW = Mathf.RoundToInt( textureSize.x );
				int spriteH = Mathf.RoundToInt( textureSize.y );
				
				var size = new Vector4( padding.x / spriteW,
										padding.y / spriteH,
										(spriteW - padding.z) / spriteW,
										(spriteH - padding.w) / spriteH );


				{
					if (textureSize.sqrMagnitude > 0.0f)
					{
						if (scaleMode == ScaleMode.ScaleToFit)
						{
							float spriteRatio = textureSize.x / textureSize.y;
							float rectRatio = r.width / r.height;

							if (spriteRatio > rectRatio)
							{
								float oldHeight = r.height;
								r.height = r.width * (1.0f / spriteRatio);
								r.y += (oldHeight - r.height) * rectTransform.pivot.y;
							}
							else
							{
								float oldWidth = r.width;
								r.width = r.height * spriteRatio;
								r.x += (oldWidth - r.width) * rectTransform.pivot.x;
							}
						}
						else if (scaleMode == ScaleMode.ScaleAndCrop)
						{
							float aspectRatio = textureSize.x / textureSize.y;
							float screenRatio = r.width / r.height;
							if (screenRatio > aspectRatio)
							{
								float adjust = aspectRatio / screenRatio;
								uvRect = new Rect(uvRect.xMin, (uvRect.yMin * adjust) + (1f - adjust) * 0.5f, uvRect.width, adjust * uvRect.height);
							}
							else
							{
								float adjust = screenRatio / aspectRatio;
								uvRect = new Rect(uvRect.xMin * adjust + (0.5f - adjust * 0.5f), uvRect.yMin, adjust * uvRect.width, uvRect.height);
							}
						}
					}
				}
				
				returnSize = new Vector4( r.x + r.width * size.x,
										  r.y + r.height * size.y,
										  r.x + r.width * size.z,
										  r.y + r.height * size.w  );

			}

			return returnSize;
		}	
	}
}

#endif