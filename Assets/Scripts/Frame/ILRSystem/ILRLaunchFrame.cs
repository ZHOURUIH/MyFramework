#if USE_ILRUNTIME
using UnityEngine;
using System;
using System.Collections.Generic;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Enviorment;
using RenderHeads.Media.AVProVideo;
using LitJson;
using ILRuntime.Runtime.Intepreter;

// 用于实现ILR加载完毕以后的一些初始化操作
public class ILRLaunchFrame : FrameBase
{
	public static void OnILRuntimeInitialized(ILRAppDomain appDomain)
	{
		// 跨域继承适配器
		CrossAdapterRegister.registeCrossAdaptor(appDomain);
		// 值类型绑定
		registeValueTypeBinder(appDomain);
		// 跨域调用的委托
		registeAllDelegate(appDomain);
		CLRBindings.Initialize(appDomain);
		ILRFrameUtility.start();
		mGameFramework.hotFixInited();
	}
	public static void registeValueTypeBinder(ILRAppDomain appDomain)
	{
		appDomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2Int), new Vector2IntBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3Int), new Vector3IntBinder());
		ILRLaunch.registeValueTypeBinder(appDomain);
	}
	public static void collectCrossInheritClass(HashSet<Type> classList)
	{
		// 收集所有需要生成适配器的类
		classList.Add(typeof(FrameBase));
		classList.Add(typeof(ClassObject));
		classList.Add(typeof(GameScene));
		classList.Add(typeof(LayoutScript));
		classList.Add(typeof(SceneProcedure));
		classList.Add(typeof(Character));
		classList.Add(typeof(CharacterBaseData));
		classList.Add(typeof(GameComponent));
		classList.Add(typeof(StateParam));
		classList.Add(typeof(PlayerState));
		classList.Add(typeof(StateGroup));
		classList.Add(typeof(Command));
		classList.Add(typeof(SQLiteTable));
		classList.Add(typeof(SQLiteData));
		classList.Add(typeof(PooledWindow));
		classList.Add(typeof(SceneInstance));
		classList.Add(typeof(FrameSystem));
		classList.Add(typeof(Transformable));
		classList.Add(typeof(GameBase));
		classList.Add(typeof(SocketPacket));
		classList.Add(typeof(GameEvent));
		classList.Add(typeof(ConfigBase));
		classList.Add(typeof(WindowItem));
		classList.Add(typeof(MonoBehaviour));
		classList.Add(typeof(SocketConnectClient));
		classList.Add(typeof(IDelayCmdWatcher));
		ILRLaunch.collectCrossInheritClass(classList);
	}
	//-------------------------------------------------------------------------------------------------
	protected static void registeAllDelegate(ILRAppDomain appDomain)
	{
		DelegateManager delegateManager = appDomain.DelegateManager;
		delegateManager.RegisterFunctionDelegate<ILTypeInstance, ILTypeInstance, int>();
		delegateManager.RegisterFunctionDelegate<int, int, int>();
		delegateManager.RegisterMethodDelegate<AnimControl, int, bool>();
		delegateManager.RegisterMethodDelegate<AnimControl, bool, bool>();
		delegateManager.RegisterMethodDelegate<AssetBundleInfo, object>();
		delegateManager.RegisterMethodDelegate<ComponentOwner>();
		delegateManager.RegisterMethodDelegate<ComponentOwner, BOOL>();
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
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, int>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, bool>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, BOOL>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector2>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector2, int>();
		delegateManager.RegisterMethodDelegate<IScrollItem, int>();
		delegateManager.RegisterMethodDelegate<LayoutScript, bool>();
		delegateManager.RegisterMethodDelegate<myUIObject, object>();
		delegateManager.RegisterMethodDelegate<UnityEngine.Object, UnityEngine.Object[], byte[], object, string>();
		delegateManager.RegisterMethodDelegate<PlayerState, bool, string>();
		delegateManager.RegisterMethodDelegate<SocketConnectClient>();
		delegateManager.RegisterMethodDelegate<Texture, string>();
		delegateManager.RegisterMethodDelegate<Texture2D, object>();
		delegateManager.RegisterMethodDelegate<UGUIAtlas, object>();
		delegateManager.RegisterMethodDelegate<Vector3, Vector3, float, int>();
		delegateManager.RegisterMethodDelegate<Vector2>();
		delegateManager.RegisterMethodDelegate<Vector2, int>();
		delegateManager.RegisterMethodDelegate<Vector2, Vector2>();
		delegateManager.RegisterMethodDelegate<Vector2, Vector2, Vector2, Vector2>();
		delegateManager.RegisterMethodDelegate<BOOL>();
		delegateManager.RegisterMethodDelegate<BOOL, object>();
		delegateManager.RegisterMethodDelegate<string>();
		delegateManager.RegisterMethodDelegate<string, bool>();
		delegateManager.RegisterMethodDelegate<string, long>();
		delegateManager.RegisterMethodDelegate<string, long, long>();
		delegateManager.RegisterMethodDelegate<float>();
		delegateManager.RegisterMethodDelegate<float, bool>();
		delegateManager.RegisterMethodDelegate<bool>();

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
			return new OnReceiveDrag((dragObj, continueEvent) => { ((Action<IMouseEventCollect, BOOL>)action)(dragObj, continueEvent); });
		});
		delegateManager.RegisterDelegateConvertor<OnDragHover>((action) =>
		{
			return new OnDragHover((dragObj, hover) => { ((Action<IMouseEventCollect, bool>)action)(dragObj, hover); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseEnter>((action) =>
		{
			return new OnMouseEnter((obj, touchID) => { ((Action<IMouseEventCollect, int>)action)(obj, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseLeave>((action) =>
		{
			return new OnMouseLeave((obj, touchID) => { ((Action<IMouseEventCollect, int>)action)(obj, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseDown>((action) =>
		{
			return new OnMouseDown((mousePos, touchID) => { ((Action<Vector2, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseUp>((action) =>
		{
			return new OnMouseUp((mousePos, touchID) => { ((Action<Vector2, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseMove>((action) =>
		{
			return new OnMouseMove((mousePos, moveDelta, moveTime, touchID) => { ((Action<Vector3, Vector3, float>)action)(mousePos, moveDelta, moveTime); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseStay>((action) =>
		{
			return new OnMouseStay((mousePos, touchID) => { ((Action<Vector2, int>)action)(mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnScreenMouseUp>((action) =>
		{
			return new OnScreenMouseUp((obj, mousePos, touchID) => { ((Action<IMouseEventCollect, Vector2, int>)action)(obj, mousePos, touchID); });
		});
		delegateManager.RegisterDelegateConvertor<OnLongPress>((action) =>
		{
			return new OnLongPress(() => { ((Action)action)(); });
		});
		delegateManager.RegisterDelegateConvertor<OnLongPressing>((action) =>
		{
			return new OnLongPressing((progress) => { ((Action<float>)action)(progress); });
		});
		delegateManager.RegisterDelegateConvertor<OnMultiTouchStart>((action) =>
		{
			return new OnMultiTouchStart((touch0, touch1) => { ((Action<Vector2, Vector2>)action)(touch0, touch1); });
		});
		delegateManager.RegisterDelegateConvertor<OnMultiTouchMove>((action) =>
		{
			return new OnMultiTouchMove((touch0, lastPosition0, touch1, lastPosition1) => { ((Action<Vector2, Vector2, Vector2, Vector2>)action)(touch0, lastPosition0, touch1, lastPosition1); });
		});
		delegateManager.RegisterDelegateConvertor<OnMultiTouchEnd>((action) =>
		{
			return new OnMultiTouchEnd(() => { ((Action)action)(); });
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
			return new MyThreadCallback((BOOL allowDrag) => { ((Action<BOOL>)action)(allowDrag); });
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
			return new OnDragStartCallback((ComponentOwner dragObj, BOOL allowDrag) => { ((Action<ComponentOwner, BOOL>)action)(dragObj, allowDrag); });
		});
		delegateManager.RegisterDelegateConvertor<StartDownloadCallback>((action) =>
		{
			return new StartDownloadCallback((fileName, totalSize) => { ((Action<string, long>)action)(fileName, totalSize); });
		});
		delegateManager.RegisterDelegateConvertor<DownloadingCallback>((action) =>
		{
			return new DownloadingCallback((fileName, fileSize, downloadedSize) => { ((Action<string, long, long>)action)(fileName, fileSize, downloadedSize); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectPreClickCallback>((action) =>
		{
			return new ObjectPreClickCallback((obj, userData) => { ((Action<myUIObject, object>)action)(obj, userData); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectClickCallback>((action) =>
		{
			return new ObjectClickCallback((obj) => { ((Action<IMouseEventCollect>)action)(obj); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectDoubleClickCallback>((action) =>
		{
			return new ObjectDoubleClickCallback((obj) => { ((Action<IMouseEventCollect>)action)(obj); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectHoverCallback>((action) =>
		{
			return new ObjectHoverCallback((obj, hover) => { ((Action<IMouseEventCollect, bool>)action)(obj, hover); });
		});
		delegateManager.RegisterDelegateConvertor<ObjectPressCallback>((action) =>
		{
			return new ObjectPressCallback((obj, press) => { ((Action<IMouseEventCollect, bool>)action)(obj, press); });
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
			return new AtlasLoadDone((atlas, userData) => { ((Action<UGUIAtlas, object>)action)(atlas, userData); });
		});
		delegateManager.RegisterDelegateConvertor<OnInputField>((action) =>
		{
			return new OnInputField((str) => { ((Action<string>)action)(str); });
		});
		delegateManager.RegisterDelegateConvertor<OnStateLeave>((action) =>
		{
			return new OnStateLeave((state, isBreak, param) => { ((Action<PlayerState, bool, string>)action)(state, isBreak, param); });
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
			return new ConnectCallback((client) => { ((Action<SocketConnectClient>)action)(client); });
		});
		delegateManager.RegisterDelegateConvertor<OnKeyCurrentDown>((action) =>
		{
			return new OnKeyCurrentDown(() => { ((Action)action)(); });
		});

		ILRLaunch.registeAllDelegate(appDomain);
	}
}
#endif