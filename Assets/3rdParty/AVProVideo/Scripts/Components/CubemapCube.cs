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
	/// Builds a cube mesh for displaying a 360 degree "Cubemap 3x2 facebook layout" texture in VR
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	//[ExecuteInEditMode]
	[AddComponentMenu("AVPro Video/Cubemap Cube (VR)", 400)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class CubemapCube : MonoBehaviour
	{
		public enum Layout
		{
			FacebookTransform32,	// Layout for Facebooks FFMPEG Transform plugin with 3:2 layout
			Facebook360Capture,		// Layout for Facebooks 360-Capture-SDK
		}

		private Mesh _mesh;
        protected MeshRenderer _renderer;

        [SerializeField]
        protected Material _material = null;

        [SerializeField]
        private MediaPlayer _mediaPlayer = null;

		// This value comes from the facebook transform ffmpeg filter and is used to prevent seams appearing along the edges due to bilinear filtering
		[SerializeField]
		private float expansion_coeff = 1.01f;

		[SerializeField]
		private Layout _layout = Layout.FacebookTransform32;

		private Texture _texture;
		private bool _verticalFlip;
		private int _textureWidth;
		private int _textureHeight;
		private static int _propApplyGamma;

		private static int _propUseYpCbCr;
		private const string PropChromaTexName = "_ChromaTex";
		private static int _propChromaTex;
		private const string PropYpCbCrTransformName = "_YpCbCrTransform";
		private static int _propYpCbCrTransform;

		public MediaPlayer Player
		{
			set { _mediaPlayer = value; }
			get { return _mediaPlayer; }
		}


		void Awake()
		{
			if (_propApplyGamma == 0)
			{
				_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
			}
			if (_propUseYpCbCr == 0)
				_propUseYpCbCr = Shader.PropertyToID("_UseYpCbCr");
			if (_propChromaTex == 0)
				_propChromaTex = Shader.PropertyToID(PropChromaTexName);
			if (_propYpCbCrTransform == 0)
				_propYpCbCrTransform = Shader.PropertyToID(PropYpCbCrTransformName);
		}

		void Start()
		{
			if (_mesh == null)
			{
				_mesh = new Mesh();
				_mesh.MarkDynamic();
				MeshFilter filter = this.GetComponent<MeshFilter>();
				if (filter != null)
				{
					filter.mesh = _mesh;
				}
				_renderer = this.GetComponent<MeshRenderer>();
				if (_renderer != null)
				{
					_renderer.material = _material;
				}
				BuildMesh();
			}
		}

		void OnDestroy()
		{
			if (_mesh != null)
			{
				MeshFilter filter = this.GetComponent<MeshFilter>();
				if (filter != null)
				{
					filter.mesh = null;
				}

#if UNITY_EDITOR
				Mesh.DestroyImmediate(_mesh);
#else
				Mesh.Destroy(_mesh);
#endif
				_mesh = null;
			}

			if (_renderer != null)
			{
				_renderer.material = null;
				_renderer = null;
			}
		}

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		void LateUpdate()
		{
			if (Application.isPlaying)
			{
				Texture texture = null;
				bool requiresVerticalFlip = false;
				if (_mediaPlayer != null && _mediaPlayer.Control != null)
				{
					if (_mediaPlayer.TextureProducer != null)
					{
						Texture resamplerTex = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[0];
						texture = _mediaPlayer.m_Resample ? resamplerTex : _mediaPlayer.TextureProducer.GetTexture();
						requiresVerticalFlip = _mediaPlayer.TextureProducer.RequiresVerticalFlip();

						// Detect changes that we need to apply to the material/mesh
						if (_texture != texture || 
							_verticalFlip != requiresVerticalFlip ||
							(texture != null && (_textureWidth != texture.width || _textureHeight != texture.height))
							)
						{
							_texture = texture;
							if (texture != null)
							{
								UpdateMeshUV(texture.width, texture.height, requiresVerticalFlip);
							}
						}

#if UNITY_PLATFORM_SUPPORTS_LINEAR
						// Apply gamma
						if (_renderer.material.HasProperty(_propApplyGamma) && _mediaPlayer.Info != null)
						{
							Helper.SetupGammaMaterial(_renderer.material, _mediaPlayer.Info.PlayerSupportsLinearColorSpace());
						}
#endif
						if (_renderer.material.HasProperty(_propUseYpCbCr) && _mediaPlayer.TextureProducer.GetTextureCount() == 2)
						{
							_renderer.material.EnableKeyword("USE_YPCBCR");
							Texture resamplerTexYCRCB = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[1];
							_renderer.material.SetTexture(_propChromaTex, _mediaPlayer.m_Resample ? resamplerTexYCRCB : _mediaPlayer.TextureProducer.GetTexture(1));
							_renderer.material.SetMatrix(_propYpCbCrTransform, _mediaPlayer.TextureProducer.GetYpCbCrTransform());
						}
					}

					_renderer.material.mainTexture = _texture;
				}
				else
				{
					_renderer.material.mainTexture = null;
				}
			}
		}	

		private void BuildMesh()
		{
			Vector3 offset = new Vector3(-0.5f, -0.5f, -0.5f);
			Vector3[] v = new Vector3[]
			{
				// Left
				new Vector3(0f,-1f,0f) - offset,
				new Vector3(0f,0f,0f) - offset,
				new Vector3(0f,0f,-1f) - offset,
				new Vector3(0f,-1f,-1f) - offset,
				// Front
				new Vector3(0f,0f,0f) - offset,
				new Vector3(-1f,0f,0f) - offset,
				new Vector3(-1f,0f,-1f) - offset,
				new Vector3(0f,0f,-1f) - offset,
				// Right
				new Vector3(-1f,0f,0f) - offset,
				new Vector3(-1f,-1f,0f) - offset,
				new Vector3(-1f,-1f,-1f) - offset,
				new Vector3(-1f,0f,-1f) - offset,
				// Back
				new Vector3(-1f,-1f,0f) - offset,
				new Vector3(0f,-1f,0f) - offset,
				new Vector3(0f,-1f,-1f) - offset,
				new Vector3(-1f,-1f,-1f) - offset,
				// Bottom
				new Vector3(0f,-1f,-1f) - offset,
				new Vector3(0f,0f,-1f) - offset,
				new Vector3(-1f,0f,-1f) - offset,
				new Vector3(-1f,-1f,-1f) - offset,
				// Top
				new Vector3(-1f,-1f,0f) - offset,
				new Vector3(-1f,0f,0f) - offset,
				new Vector3(0f,0f,0f) - offset,
				new Vector3(0f,-1f,0f) - offset,
			};

			Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
			for (int i = 0; i < v.Length; i++)
			{
				v[i] = rot.MultiplyPoint(v[i]);
			}

			_mesh.vertices = v;

			_mesh.triangles = new int[]
			{
				0,1,2,
				0,2,3,
				4,5,6,
				4,6,7,
				8,9,10,
				8,10,11,
				12,13,14,
				12,14,15,
				16,17,18,
				16,18,19,
				20,21,22,
				20,22,23,
			};

			_mesh.normals = new Vector3[]
			{
				// Left
				new Vector3(-1f,0f,0f),
				new Vector3(-1f,0f,0f),
				new Vector3(-1f,0f,0f),
				new Vector3(-1f,0f,0f),
				// Front
				new Vector3(0f,-1f,0f),
				new Vector3(0f,-1f,0f),
				new Vector3(0f,-1f,0f),
				new Vector3(0f,-1f,0f),
				// Right
				new Vector3(1f,0f,0f),
				new Vector3(1f,0f,0f),
				new Vector3(1f,0f,0f),
				new Vector3(1f,0f,0f),
				// Back
				new Vector3(0f,1f,0f),
				new Vector3(0f,1f,0f),
				new Vector3(0f,1f,0f),
				new Vector3(0f,1f,0f),
				// Bottom
				new Vector3(0f,0f,1f),
				new Vector3(0f,0f,1f),
				new Vector3(0f,0f,1f),
				new Vector3(0f,0f,1f),
				// Top
				new Vector3(0f,0f,-1f),
				new Vector3(0f,0f,-1f),
				new Vector3(0f,0f,-1f),
				new Vector3(0f,0f,-1f)
			};

			UpdateMeshUV(512, 512, false);
		}

		private void UpdateMeshUV(int textureWidth, int textureHeight, bool flipY)
		{
			_textureWidth = textureWidth;
			_textureHeight = textureHeight;
			_verticalFlip = flipY;

			float texWidth = textureWidth;
			float texHeight = textureHeight;

			float blockWidth = texWidth / 3f;
			float pixelOffset = Mathf.Floor(((expansion_coeff * blockWidth) - blockWidth) / 2f);

			float wO = pixelOffset / texWidth;
			float hO = pixelOffset / texHeight;

			const float third = 1f / 3f;
			const float half = 0.5f;

			Vector2[] uv = null;
			if (_layout == Layout.Facebook360Capture)
			{
				uv = new Vector2[]
				{
					//front (texture middle top) correct left
					new Vector2(third+wO, half-hO),
					new Vector2((third*2f)-wO, half-hO),
					new Vector2((third*2f)-wO, 0f+hO),
					new Vector2(third+wO, 0f+hO),
				
					//left (texture middle bottom) correct front
					new Vector2(third+wO,1f-hO),
					new Vector2((third*2f)-wO, 1f-hO),
					new Vector2((third*2f)-wO, half+hO),
					new Vector2(third+wO, half+hO),

					//bottom (texture left top) correct right
					new Vector2(0f+wO, half-hO),
					new Vector2(third-wO, half-hO),
					new Vector2(third-wO, 0f+hO),
					new Vector2(0f+wO, 0f+hO),

					//top (texture right top) correct rear
					new Vector2((third*2f)+wO, 1f-hO),
					new Vector2(1f-wO, 1f-hO),
					new Vector2(1f-wO, half+hO),
					new Vector2((third*2f)+wO, half+hO),

					//back (texture right bottom) correct ground
					new Vector2((third*2f)+wO, 0f+hO),
					new Vector2((third*2f)+wO, half-hO),
					new Vector2(1f-wO, half-hO),
					new Vector2(1f-wO, 0f+hO),

					//right (texture left bottom) correct sky
					new Vector2(third-wO, 1f-hO),
					new Vector2(third-wO, half+hO),
					new Vector2(0f+wO, half+hO),
					new Vector2(0f+wO, 1f-hO),
				};
			}
			else if (_layout == Layout.FacebookTransform32)
			{
				uv = new Vector2[]
				{
					//left
					new Vector2(third+wO,1f-hO),
					new Vector2((third*2f)-wO, 1f-hO),
					new Vector2((third*2f)-wO, half+hO),
					new Vector2(third+wO, half+hO),

					//front
					new Vector2(third+wO, half-hO),
					new Vector2((third*2f)-wO, half-hO),
					new Vector2((third*2f)-wO, 0f+hO),
					new Vector2(third+wO, 0f+hO),

					//right
					new Vector2(0f+wO, 1f-hO),
					new Vector2(third-wO, 1f-hO),
					new Vector2(third-wO, half+hO),
					new Vector2(0f+wO, half+hO),

					//back
					new Vector2((third*2f)+wO, half-hO),
					new Vector2(1f-wO, half-hO),
					new Vector2(1f-wO, 0f+hO),
					new Vector2((third*2f)+wO, 0f+hO),

					//bottom
					new Vector2(0f+wO, 0f+hO),
					new Vector2(0f+wO, half-hO),
					new Vector2(third-wO, half-hO),
					new Vector2(third-wO, 0f+hO),

					//top
					new Vector2(1f-wO, 1f-hO),
					new Vector2(1f-wO, half+hO),
					new Vector2((third*2f)+wO, half+hO),
					new Vector2((third*2f)+wO, 1f-hO)
				};
			}
			
			if (flipY)
			{
				for (int i = 0; i < uv.Length; i++)
				{
					uv[i].y = 1f - uv[i].y;
				}
			}

			_mesh.uv = uv;
			_mesh.UploadMeshData(false);
		}
	}
}
