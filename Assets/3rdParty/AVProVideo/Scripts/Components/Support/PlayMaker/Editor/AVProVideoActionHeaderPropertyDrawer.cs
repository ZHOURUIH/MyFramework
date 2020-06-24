//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

#if PLAYMAKER

using UnityEngine;
using HutongGames.PlayMakerEditor;

namespace RenderHeads.Media.AVProVideo.PlayMaker.Actions
{
    /// <summary>
    /// Draws the AVPro Video logo above each PlayMaker action
    /// </summary>
    [PropertyDrawer(typeof(AVProVideoActionHeader))]
    public class AVProVideoActionHeaderPropertyDrawer : PropertyDrawer 
    {
        private Rect _rect;

        public override object OnGUI(GUIContent label, object obj, bool isSceneObject, params object[] attributes)
        {
            // always keep this enabled to avoid transparency artifact ( unless someone tells me how to style up GUIStyle for disable state)
            bool _enabled = GUI.enabled;
            GUI.enabled = true;

            _rect = GUILayoutUtility.GetLastRect();
            GUIDrawRect(_rect, Color.black);
        
            _rect.Set(_rect.x,_rect.y+1,_rect.width,_rect.height-2);
            if (HeaderTexture != null)
            {
                GUI.DrawTexture(_rect, HeaderTexture, ScaleMode.ScaleToFit);
            }

            GUI.enabled = _enabled;

            return null;
        }

        private static Texture2D _headerTexture = null;
        internal static Texture2D HeaderTexture
        {
            get
            {
                if (_headerTexture == null)
                    _headerTexture = Resources.Load<Texture2D>("AVProVideoIcon");
                if (_headerTexture != null)
                    _headerTexture.hideFlags = HideFlags.DontSaveInEditor;
                return _headerTexture;
            }
        }

        private static Texture2D _staticRectTexture;
        private static GUIStyle _staticRectStyle;

        // Note that this function is only meant to be called from OnGUI() functions.
        public static void GUIDrawRect(Rect position, Color color)
        {
            if (_staticRectTexture == null)
            {
                _staticRectTexture = new Texture2D(1, 1);
            }

            if (_staticRectStyle == null)
            {
                _staticRectStyle = new GUIStyle();
            }

            _staticRectTexture.SetPixel(0, 0, color);
            _staticRectTexture.Apply();

            _staticRectStyle.normal.background = _staticRectTexture;
        
            GUI.Box(position, GUIContent.none, _staticRectStyle);
        }
    }
}

#endif