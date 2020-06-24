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
	/// This script is needed to send the camera position to the stereo shader so that
	/// it can determine which eye it is rendering.  In the future this script won't 
	/// be needed once we have support for single-pass stereo rendering.
	/// </summary>
	[AddComponentMenu("AVPro Video/Update Stereo Material", 400)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	public class UpdateStereoMaterial : MonoBehaviour
	{
		[Header("Stereo camera")]
		public Camera _camera;

		[Header("Rendering elements")]
		public MeshRenderer _renderer;
		public UnityEngine.UI.Graphic _uGuiComponent;
		public Material _material;

		[SerializeField]
		private StereoEye _forceEyeMode;

		private static int _cameraPositionId;
		private static int _viewMatrixId;
		private StereoEye _setForceEyeMode = StereoEye.Both;
		public StereoEye ForceEyeMode { get { return _forceEyeMode; } set { _forceEyeMode = value; } }

		private Camera _foundCamera;

		void Awake()
		{
			if (_cameraPositionId == 0)
			{
				_cameraPositionId = Shader.PropertyToID("_cameraPosition");
			}
			if (_viewMatrixId == 0)
			{
				_viewMatrixId = Shader.PropertyToID("_ViewMatrix");
			}
			if (_camera == null)
			{
				Debug.LogWarning("[AVProVideo] No camera set for UpdateStereoMaterial component. If you are rendering in stereo then it is recommended to set this.");
			}
		}

		private void SetupMaterial(Material m, Camera camera)
		{
			m.SetVector(_cameraPositionId, camera.transform.position);
			m.SetMatrix(_viewMatrixId, camera.worldToCameraMatrix.transpose);
			if (_forceEyeMode != _setForceEyeMode)
			{
				Helper.SetupStereoEyeModeMaterial(m, _forceEyeMode);
				_setForceEyeMode = _forceEyeMode;
			}
		}

		// We do a LateUpdate() to allow for any changes in the camera position that may have happened in Update()
		private void LateUpdate()
		{
			if (_camera != null && _foundCamera != _camera)
			{
				_foundCamera = _camera;
			}
			if (_foundCamera == null)
			{
				_foundCamera = Camera.main;
				if (_foundCamera == null)
				{
					Debug.LogWarning("[AVPro Video] Cannot find main camera for UpdateStereoMaterial, this can lead to eyes flickering");
					if (Camera.allCameras.Length > 0)
					{
						_foundCamera = Camera.allCameras[0];
						Debug.LogWarning("[AVPro Video] UpdateStereoMaterial using camera " + _foundCamera.name);
					}
				}
			}

			if (_renderer == null && _material == null)
			{
				_renderer = this.gameObject.GetComponent<MeshRenderer>();
			}

			if (_foundCamera != null)
			{
				if (_renderer != null)
				{
					SetupMaterial(_renderer.material, _foundCamera);
				}
				if (_material != null)
				{
					SetupMaterial(_material, _foundCamera);
				}
				if (_uGuiComponent != null)
				{
					SetupMaterial(_uGuiComponent.material, _foundCamera);
				}
			}
		}
	}
}