using UnityEngine;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using LitJson;
using System.Net.Sockets;
#if USE_ILRUNTIME
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
#endif

// 游戏委托定义-------------------------------------------------------------------------------------------------------------
public delegate void VoidCallback();
public delegate void OnAndroidLog(string info);
public delegate void OnAndroidError(string info);
public delegate void OnLog(string time, string info, LOG_LEVEL level, bool isError);
public delegate void RecordCallback(short[] data, int dataCount);
public delegate void OnHttpWebRequestCallback(JsonData data, object userData);
public delegate void TextureAnimCallback(IUIAnimation window, bool isBreak);
public delegate void KeyFrameCallback(ComponentKeyFrame com, bool isBreak);
public delegate void LerpCallback(ComponentLerp com, bool breakLerp);
public delegate void CommandCallback(Command cmd);
public delegate void AssetBundleLoadCallback(AssetBundleInfo assetBundle, object userData);
public delegate void AssetLoadDoneCallback(UnityEngine.Object asset, UnityEngine.Object[] assets, byte[] bytes, object userData, string loadPath);
public delegate void SceneLoadCallback(float progress, bool done);
public delegate void SceneActiveCallback();
public delegate void LayoutAsyncDone(GameLayout layout);
public delegate void VideoCallback(string videoName, bool isBreak);
public delegate void VideoErrorCallback(ErrorCode errorCode);
public delegate void TrackCallback(ComponentTrackTarget com, bool breakTrack);
public delegate void OnReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent);
public delegate void OnDragHover(IMouseEventCollect dragObj, Vector3 mousePos, bool hover);
public delegate void OnMouseEnter(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void OnMouseLeave(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void OnMouseDown(Vector3 mousePos, int touchID);
public delegate void OnMouseUp(Vector3 mousePos, int touchID);
public delegate void OnMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID);
public delegate void OnMouseStay(Vector3 mousePos, int touchID);
public delegate void OnScreenMouseUp(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void OnLongPress();
public delegate void OnLongPressing(float progress);
public delegate void HeadDownloadCallback(Texture head, string openID);
public delegate void OnDragViewCallback();
public delegate void OnDragViewStartCallback(BOOL allowDrag);
public delegate void MyThreadCallback(ref bool run);    // run表示是否继续运行该线程,可在运行时修改
public delegate void OnPlayingCallback(AnimControl control, int frame, bool isPlaying); // isPlaying表示是否是在播放过程中触发的该回调
public delegate void OnPlayEndCallback(AnimControl control, bool callback, bool isBreak);
public delegate void OnDraging();
public delegate void OnScrollItem(IScrollItem item, int index);
public delegate void OnDragCallback(ComponentOwner dragObj);
public delegate void OnDragStartCallback(ComponentOwner dragObj, TouchPoint touchPoint, BOOL allowDrag);
public delegate void StartDownloadCallback(string fileName, long totalSize);
public delegate void DownloadingCallback(string fileName, long fileSize, long downloadedSize);
public delegate void ObjectPreClickCallback(myUIObject obj, object userData);
public delegate void ObjectClickCallback(IMouseEventCollect obj, Vector3 mousePos);
public delegate void ObjectDoubleClickCallback(IMouseEventCollect obj, Vector3 mousePos);
public delegate void ObjectHoverCallback(IMouseEventCollect obj, Vector3 mousePos, bool hover);
public delegate void ObjectPressCallback(IMouseEventCollect obj, Vector3 mousePos, bool press);
public delegate void SliderCallback();
public delegate void LayoutScriptCallback(LayoutScript script, bool create);
public delegate void SceneScriptCallback(SceneInstance instance, bool create);
public delegate void OnCharacterLoaded(Character character, object userData);
public delegate void CreateObjectCallback(GameObject go, object userData);
public delegate void CreateObjectGroupCallback(Dictionary<string, GameObject> go, object userData);
public delegate void OnEffectLoadedCallback(GameEffect effect, object userData);
public delegate void AtlasLoadDone(UGUIAtlasPtr atlas, object userData);
public delegate void OnInputField(string str);
public delegate void OnStateLeave(CharacterState state, bool isBreak, string param);
public delegate void EventCallback(GameEvent param);
public delegate void OnEffectDestroy(GameEffect effect, object userData);
public delegate void ConnectCallback(NetConnectTCP client);
public delegate void OnKeyCurrentDown();
public delegate myUGUIImage CreateImage();
public delegate myUGUIImageAnim CreateImageAnim();
public delegate void DestroyImage(myUGUIImage image);
public delegate void DestroyImageAnim(myUGUIImageAnim imageAnim);
public delegate void OnCheck(UGUICheckbox checkbox, bool check);
public delegate void OnDestroyWindow(myUIObject window);
public delegate void EncryptPacket(byte[] data, int offset, int length, byte param);
public delegate void DecryptPacket(byte[] data, int offset, int length, byte param);
public delegate void OnHotKeyCallback(KeyCode key);
public delegate void NetStateCallback(NET_STATE state, NET_STATE lastState);
#if USE_ILRUNTIME
public delegate void OnHotFixLoaded(ILRAppDomain appDomain);
#endif