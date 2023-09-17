#if USE_ILRUNTIME
using UnityEngine;
using System;
using System.Collections.Generic;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using ILRuntime.Runtime.Enviorment;
using RenderHeads.Media.AVProVideo;
using LitJson;
using ILRuntime.Runtime.Intepreter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 用于实现ILR加载完毕以后的一些初始化操作
public abstract class ILRLaunchFrame
{
	public void onILRuntimeInitialized(ILRAppDomain appDomain)
	{
		crossAdapter(appDomain);
		valueTypeBind(appDomain);
		// 跨域调用的委托
		registeAllDelegate(appDomain);
		clrBind(appDomain);
	}
	public abstract void valueTypeBind(ILRAppDomain appDomain);
	public virtual void collectCrossInheritClass(HashSet<Type> classList)
	{
		// 收集所有需要生成适配器的类
		classList.Add(typeof(IDisposable));
		classList.Add(typeof(ClassObject));
		classList.Add(typeof(GameScene));
		classList.Add(typeof(LayoutScript));
		classList.Add(typeof(SceneProcedure));
		classList.Add(typeof(Character));
		classList.Add(typeof(CharacterData));
		classList.Add(typeof(GameComponent));
		classList.Add(typeof(StateParam));
		classList.Add(typeof(CharacterState));
		classList.Add(typeof(StateGroup));
		classList.Add(typeof(Command));
		classList.Add(typeof(PooledWindowUGUI));
		classList.Add(typeof(PooledWindowUI));
		classList.Add(typeof(PooledWindow));
		classList.Add(typeof(SceneInstance));
		classList.Add(typeof(FrameSystem));
		classList.Add(typeof(Transformable));
		classList.Add(typeof(NetPacket));
		classList.Add(typeof(GameEvent));
		classList.Add(typeof(WindowObject));
		classList.Add(typeof(WindowObjectUI));
		classList.Add(typeof(WindowObjectUGUI));
		classList.Add(typeof(MonoBehaviour));
		classList.Add(typeof(NetConnectTCP));
		classList.Add(typeof(DelayCmdWatcher));
		classList.Add(typeof(ExcelData));
		classList.Add(typeof(ExcelTable));
		classList.Add(typeof(NetPacketFrame));
		classList.Add(typeof(RedPoint));
		classList.Add(typeof(RedPointCount));
		classList.Add(typeof(SerializableBit));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void clrBind(ILRAppDomain appDomain);
	protected abstract void crossAdapter(ILRAppDomain appDomain);
	protected virtual void registeAllDelegate(ILRAppDomain appDomain)
	{
		DelegateManager delegateManager = appDomain.DelegateManager;
		delegateManager.RegisterFunctionDelegate<ILTypeInstance, ILTypeInstance, int>();
		delegateManager.RegisterFunctionDelegate<int, int, int>();
		delegateManager.RegisterMethodDelegate<PointerEventData, GameObject>();
		delegateManager.RegisterMethodDelegate<AnimControl, int, bool>();
		delegateManager.RegisterMethodDelegate<AnimControl, bool, bool>();
		delegateManager.RegisterMethodDelegate<AssetBundleInfo, object>();
		delegateManager.RegisterMethodDelegate<ComponentOwner>();
		delegateManager.RegisterMethodDelegate<ComponentOwner, TouchPoint, BOOL>();
		delegateManager.RegisterMethodDelegate<Character, object>();
		delegateManager.RegisterMethodDelegate<ComponentTrackTarget, bool>();
		delegateManager.RegisterMethodDelegate<ComponentKeyFrame, bool>();
		delegateManager.RegisterMethodDelegate<ComponentLerp, bool>();
		delegateManager.RegisterMethodDelegate<Command>();
		delegateManager.RegisterMethodDelegate<Dictionary<string, GameObject>, object>();
		delegateManager.RegisterMethodDelegate<ErrorCode>();
		delegateManager.RegisterMethodDelegate<GameLayout>();
		delegateManager.RegisterMethodDelegate<GameObject, object>();
		delegateManager.RegisterMethodDelegate<GameEffect, object>();
		delegateManager.RegisterMethodDelegate<GameEvent>();
		delegateManager.RegisterMethodDelegate<IUIAnimation, bool>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector3>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector3, bool>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector3, BOOL>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector3, int>();
		delegateManager.RegisterMethodDelegate<IScrollItem, int>();
		delegateManager.RegisterMethodDelegate<LayoutScript, bool>();
		delegateManager.RegisterMethodDelegate<myUIObject, object>();
		delegateManager.RegisterMethodDelegate<UnityEngine.Object, UnityEngine.Object[], byte[], object, string>();
		delegateManager.RegisterMethodDelegate<CharacterState, bool, string>();
		delegateManager.RegisterMethodDelegate<NetConnectTCP>();
		delegateManager.RegisterMethodDelegate<Texture, string>();
		delegateManager.RegisterMethodDelegate<Texture2D, object>();
		delegateManager.RegisterMethodDelegate<UGUIAtlas, object>();
		delegateManager.RegisterMethodDelegate<Vector3>();
		delegateManager.RegisterMethodDelegate<Vector3, int>();
		delegateManager.RegisterMethodDelegate<Vector3, Vector3, float, int>();
		delegateManager.RegisterMethodDelegate<BOOL>();
		delegateManager.RegisterMethodDelegate<BOOL, object>();
		delegateManager.RegisterMethodDelegate<string>();
		delegateManager.RegisterMethodDelegate<string, bool>();
		delegateManager.RegisterMethodDelegate<string, long>();
		delegateManager.RegisterMethodDelegate<string, long, long>();
		delegateManager.RegisterMethodDelegate<string, object>();
		delegateManager.RegisterMethodDelegate<string, string, LOG_LEVEL, bool>();
		delegateManager.RegisterMethodDelegate<float>();
		delegateManager.RegisterMethodDelegate<float, bool>();
		delegateManager.RegisterMethodDelegate<bool>();
		delegateManager.RegisterMethodDelegate<UGUICheckbox, bool>();

		delegateManager.RegisterDelegateConvertor<UnityAction>((act) =>
		{
			return new UnityAction(() => { ((Action)act)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnCheck>((act) =>
		{
			return new OnCheck((checkbox, check) => { ((Action<UGUICheckbox, bool>)act)(checkbox, check); });
		});
		delegateManager.RegisterDelegateConvertor<Comparison<int>>((act) =>
		{
			return new Comparison<int>((x, y) => { return ((Func<int, int, int>)act)(x, y); });
		});
		delegateManager.RegisterDelegateConvertor<Comparison<ILTypeInstance>>((action) =>
		{
			return new Comparison<ILTypeInstance>((x, y) => { return ((Func<ILTypeInstance, ILTypeInstance, int>)action)(x, y); });
		});
		delegateManager.RegisterDelegateConvertor<OnLog>((action) =>
		{
			return new OnLog((time, info, level, isError) => { ((Action<string, string, LOG_LEVEL, bool>)action)(time, info, level, isError); });
		});
		delegateManager.RegisterDelegateConvertor<OnHttpWebRequestCallback>((action) =>
		{
			return new OnHttpWebRequestCallback((data, userData) => { ((Action<JsonData, object>)action)(data, userData); });
		});
		delegateManager.RegisterDelegateConvertor<TextureAnimCallback>((action) =>
		{
			return new TextureAnimCallback((window, isBreak) => { ((Action<IUIAnimation, bool>)action)(window, isBreak); });
		});
		delegateManager.RegisterDelegateConvertor<KeyFrameCallback>((action) =>
		{
			return new KeyFrameCallback((com, isBreak) => { ((Action<ComponentKeyFrame, bool>)action)(com, isBreak); });
		});
		delegateManager.RegisterDelegateConvertor<LerpCallback>((action) =>
		{
			return new LerpCallback((com, breakLerp) => { ((Action<ComponentLerp, bool>)action)(com, breakLerp); });
		});
		delegateManager.RegisterDelegateConvertor<CommandCallback>((action) =>
		{
			return new CommandCallback((cmd) => { ((Action<Command>)action)(cmd); });
		});
		delegateManager.RegisterDelegateConvertor<AssetBundleLoadCallback>((action) =>
		{
			return new AssetBundleLoadCallback((assetBundle, userData) => { ((Action<AssetBundleInfo, object>)action)(assetBundle, userData); });
		});
		delegateManager.RegisterDelegateConvertor<AssetLoadDoneCallback>((action) =>
		{
			return new AssetLoadDoneCallback((asset, assets, bytes, userData, loadPath) =>
			{
				((Action<UnityEngine.Object, UnityEngine.Object[], byte[], object, string>)action)(asset, assets, bytes, userData, loadPath);
			});
		});
		delegateManager.RegisterDelegateConvertor<SceneLoadCallback>((action) =>
		{
			return new SceneLoadCallback((progress, done) => { ((Action<float, bool>)action)(progress, done); });
		});
		delegateManager.RegisterDelegateConvertor<SceneActiveCallback>((action) =>
		{
			return new SceneActiveCallback(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<LayoutAsyncDone>((action) =>
		{
			return new LayoutAsyncDone((layout) => { ((Action<GameLayout>)action)(layout); });
		});
		delegateManager.RegisterDelegateConvertor<VideoCallback>((action) =>
		{
			return new VideoCallback((videoName, isBreak) => { ((Action<string, bool>)action)(videoName, isBreak); });
		});
		delegateManager.RegisterDelegateConvertor<VideoErrorCallback>((action) =>
		{
			return new VideoErrorCallback((errorCode) => { ((Action<ErrorCode>)action)(errorCode); });
		});
		delegateManager.RegisterDelegateConvertor<TrackCallback>((action) =>
		{
			return new TrackCallback((com, breakTrack) => { ((Action<ComponentTrackTarget, bool>)action)(com, breakTrack); });
		});
		delegateManager.RegisterDelegateConvertor<OnReceiveDrag>((action) =>
		{
			return new OnReceiveDrag((dragObj, mousePos, continueEvent) => { ((Action<IMouseEventCollect, Vector3, BOOL>)action)(dragObj, mousePos, continueEvent); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragHover>((action) =>
		{
			return new OnDragHover((dragObj, mousePos, hover) => { ((Action<IMouseEventCollect, Vector3, bool>)action)(dragObj, mousePos, hover); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseEnter>((action) =>
		{
			return new OnMouseEnter((obj, mousePos, touchID) => { ((Action<IMouseEventCollect, Vector3, int>)action)(obj, mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseLeave>((action) =>
		{
			return new OnMouseLeave((obj, mousePos, touchID) => { ((Action<IMouseEventCollect, Vector3, int>)action)(obj, mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseDown>((action) =>
		{
			return new OnMouseDown((mousePos, touchID) => { ((Action<Vector3, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseUp>((action) =>
		{
			return new OnMouseUp((mousePos, touchID) => { ((Action<Vector3, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseMove>((action) =>
		{
			return new OnMouseMove((mousePos, moveDelta, moveTime, touchID) => 
			{
				((Action<Vector3, Vector3, float, int>)action)(mousePos, moveDelta, moveTime, touchID); 
			});
		});
		delegateManager.RegisterDelegateConvertor<OnMouseStay>((action) =>
		{
			return new OnMouseStay((mousePos, touchID) => { ((Action<Vector3, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnScreenMouseUp>((action) =>
		{
			return new OnScreenMouseUp((obj, mousePos, touchID) => { ((Action<IMouseEventCollect, Vector3, int>)action)(obj, mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnLongPress>((action) =>
		{
			return new OnLongPress(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnLongPressing>((action) =>
		{
			return new OnLongPressing((progress) => { ((Action<float>)action)(progress); });
		});
		delegateManager.RegisterDelegateConvertor<HeadDownloadCallback>((action) =>
		{
			return new HeadDownloadCallback((head, openID) => { ((Action<Texture, string>)action)(head, openID); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragViewCallback>((action) =>
		{
			return new OnDragViewCallback(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragViewStartCallback>((action) =>
		{
			return new OnDragViewStartCallback((BOOL allowDrag) => { ((Action<BOOL>)action)(allowDrag); });
		});
		delegateManager.RegisterDelegateConvertor<MyThreadCallback>((action) =>
		{
			return new MyThreadCallback((ref bool allowDrag) => { ((Action<bool>)action)(allowDrag); });
		});
		delegateManager.RegisterDelegateConvertor<OnPlayingCallback>((action) =>
		{
			return new OnPlayingCallback((control, frame, isPlaying) => { ((Action<AnimControl, int, bool>)action)(control, frame, isPlaying); });
		});
		delegateManager.RegisterDelegateConvertor<OnPlayEndCallback>((action) =>
		{
			return new OnPlayEndCallback((control, callback, isBreak) => { ((Action<AnimControl, bool, bool>)action)(control, callback, isBreak); });
		});
		delegateManager.RegisterDelegateConvertor<OnDraging>((action) =>
		{
			return new OnDraging(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnScrollItem>((action) =>
		{
			return new OnScrollItem((item, index) => { ((Action<IScrollItem, int>)action)(item, index); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragCallback>((action) =>
		{
			return new OnDragCallback((dragObj) => { ((Action<ComponentOwner>)action)(dragObj); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragStartCallback>((action) =>
		{
			return new OnDragStartCallback((ComponentOwner dragObj, TouchPoint touchPoint, BOOL allowDrag) => 
			{
				((Action<ComponentOwner, TouchPoint, BOOL>)action)(dragObj, touchPoint, allowDrag); 
			});
		});
		delegateManager.RegisterDelegateConvertor<StartDownloadCallback>((action) =>
		{
			return new StartDownloadCallback((fileName, totalSize) => { ((Action<string, long>)action)(fileName, totalSize); });
		});
		delegateManager.RegisterDelegateConvertor<DownloadingCallback>((action) =>
		{
			return new DownloadingCallback((fileName, fileSize, downloadedSize) => 
			{
				((Action<string, long, long>)action)(fileName, fileSize, downloadedSize); 
			});
		});
		delegateManager.RegisterDelegateConvertor<ObjectPreClickCallback>((action) =>
		{
			return new ObjectPreClickCallback((obj, userData) => { ((Action<myUIObject, object>)action)(obj, userData); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectClickCallback>((action) =>
		{
			return new ObjectClickCallback((obj, mousePos) => { ((Action<IMouseEventCollect, Vector3>)action)(obj, mousePos); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectDoubleClickCallback>((action) =>
		{
			return new ObjectDoubleClickCallback((obj, mousePos) => { ((Action<IMouseEventCollect, Vector3>)action)(obj, mousePos); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectHoverCallback>((action) =>
		{
			return new ObjectHoverCallback((obj, mousePos, hover) => { ((Action<IMouseEventCollect, Vector3, bool>)action)(obj, mousePos, hover); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectPressCallback>((action) =>
		{
			return new ObjectPressCallback((obj, mousePos, press) => { ((Action<IMouseEventCollect, Vector3, bool>)action)(obj, mousePos, press); });
		});
		delegateManager.RegisterDelegateConvertor<SliderCallback>((action) =>
		{
			return new SliderCallback(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<LayoutScriptCallback>((action) =>
		{
			return new LayoutScriptCallback((script, create) => { ((Action<LayoutScript, bool>)action)(script, create); });
		});
		delegateManager.RegisterDelegateConvertor<OnCharacterLoaded>((action) =>
		{
			return new OnCharacterLoaded((character, userData) => { ((Action<Character, object>)action)(character, userData); });
		});
		delegateManager.RegisterDelegateConvertor<CreateObjectCallback>((action) =>
		{
			return new CreateObjectCallback((go, userData) => { ((Action<GameObject, object>)action)(go, userData); });
		});
		delegateManager.RegisterDelegateConvertor<CreateObjectGroupCallback>((action) =>
		{
			return new CreateObjectGroupCallback((go, userData) => { ((Action<Dictionary<string, GameObject>, object>)action)(go, userData); });
		});
		delegateManager.RegisterDelegateConvertor<OnEffectLoadedCallback>((action) =>
		{
			return new OnEffectLoadedCallback((effect, userData) => { ((Action<GameEffect, object>)action)(effect, userData); });
		});
		delegateManager.RegisterDelegateConvertor<AtlasLoadDone>((action) =>
		{
			return new AtlasLoadDone((atlas, userData) => { ((Action<UGUIAtlasPtr, object>)action)(atlas, userData); });
		});
		delegateManager.RegisterDelegateConvertor<OnInputField>((action) =>
		{
			return new OnInputField((str) => { ((Action<string>)action)(str); });
		});
		delegateManager.RegisterDelegateConvertor<OnStateLeave>((action) =>
		{
			return new OnStateLeave((state, isBreak, param) => { ((Action<CharacterState, bool, string>)action)(state, isBreak, param); });
		});
		delegateManager.RegisterDelegateConvertor<EventCallback>((action) =>
		{
			return new EventCallback((param) => { ((Action<GameEvent>)action)(param); });
		});
		delegateManager.RegisterDelegateConvertor<OnEffectDestroy>((action) =>
		{
			return new OnEffectDestroy((effect, userData) => { ((Action<GameEffect, object>)action)(effect, userData); });
		});
		delegateManager.RegisterDelegateConvertor<ConnectCallback>((action) =>
		{
			return new ConnectCallback((client) => { ((Action<NetConnectTCP>)action)(client); });
		});
		delegateManager.RegisterDelegateConvertor<OnKeyCurrentDown>((action) =>
		{
			return new OnKeyCurrentDown(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnLog>((action) =>
		{
			return new OnLog((time, info, level, isError) => { ((Action<string, string, LOG_LEVEL, bool>)action)(time, info, level, isError); });
		});
	}
}
#endif