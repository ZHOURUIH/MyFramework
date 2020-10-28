#if USE_ILRUNTIME
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using HotFix;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Enviorment;
using RenderHeads.Media.AVProVideo;

// 用于实现ILR加载完毕以后的一些初始化操作
public static class ILRLaunch
{
	public static void OnILRuntimeInitialized(ILRAppDomain appDomain)
	{
		CLRBindings.Initialize(appDomain);
		// 跨域继承适配器
		registeCrossAdaptor(appDomain);
		// 跨域调用的委托
		registeAllDelegate(appDomain);
		ILRUtility.callStatic("GameILR", "startILR");
	}
	public static void registeCrossAdaptor(ILRAppDomain appDomain)
	{
		appDomain.RegisterCrossBindingAdaptor(new FrameBaseAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameSceneAdapter());
		appDomain.RegisterCrossBindingAdaptor(new LayoutScriptAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SceneProcedureAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CharacterAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CharacterBaseDataAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameComponentAdapter());
		appDomain.RegisterCrossBindingAdaptor(new StateParamAdapter());
		appDomain.RegisterCrossBindingAdaptor(new PlayerStateAdapter());
		appDomain.RegisterCrossBindingAdaptor(new StateGroupAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CommandAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SQLiteTableAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SQLiteDataAdapter());
		appDomain.RegisterCrossBindingAdaptor(new PooledWindowAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SceneInstanceAdapter());
		appDomain.RegisterCrossBindingAdaptor(new FrameSystemAdapter());
		appDomain.RegisterCrossBindingAdaptor(new TransformableAdapter());

		// 值类型绑定
		appDomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2Int), new Vector2IntBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3Int), new Vector3IntBinder());
	}
	public static void registeAllDelegate(ILRAppDomain appDomain)
	{
		DelegateManager delegateManager = appDomain.DelegateManager;
		delegateManager.RegisterMethodDelegate<LayoutScript, bool>();
		delegateManager.RegisterMethodDelegate<IUIAnimation, bool>();
		delegateManager.RegisterMethodDelegate<ComponentKeyFrameBase, bool>();
		delegateManager.RegisterMethodDelegate<Command>();
		delegateManager.RegisterMethodDelegate<AssetBundleInfo, object>();
		delegateManager.RegisterMethodDelegate<ComponentLerp, bool>();
		delegateManager.RegisterMethodDelegate<UnityEngine.Object, UnityEngine.Object[], byte[], object[], string>();
		delegateManager.RegisterMethodDelegate<float>();
		delegateManager.RegisterMethodDelegate<float, bool>();
		delegateManager.RegisterMethodDelegate<GameLayout>();
		delegateManager.RegisterMethodDelegate<ErrorCode>();
		delegateManager.RegisterMethodDelegate<ComponentTrackTargetBase, bool>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, bool>();
		delegateManager.RegisterMethodDelegate<IMouseEventCollect, Vector2>();
		delegateManager.RegisterMethodDelegate<Vector3, Vector3, float>();
		delegateManager.RegisterMethodDelegate<Vector2>();
		delegateManager.RegisterMethodDelegate<Vector2, Vector2>();
		delegateManager.RegisterMethodDelegate<Vector2, Vector2, Vector2, Vector2>();
		delegateManager.RegisterMethodDelegate<NGUILine>();
		delegateManager.RegisterMethodDelegate<Texture, string>();
		delegateManager.RegisterMethodDelegate<bool>();
		delegateManager.RegisterMethodDelegate<AnimControl, int, bool>();
		delegateManager.RegisterMethodDelegate<AnimControl, bool, bool>();
		delegateManager.RegisterMethodDelegate<IScrollItem, int>();
		delegateManager.RegisterMethodDelegate<ComponentOwner>();
		delegateManager.RegisterMethodDelegate<ComponentOwner, bool>();
		delegateManager.RegisterMethodDelegate<string>();
		delegateManager.RegisterMethodDelegate<string, bool>();
		delegateManager.RegisterMethodDelegate<string, long>();
		delegateManager.RegisterMethodDelegate<string, long, long>();
		delegateManager.RegisterMethodDelegate<myUIObject, object>();
		delegateManager.RegisterMethodDelegate<Character, object>();
		delegateManager.RegisterMethodDelegate<GameObject, object>();
		delegateManager.RegisterMethodDelegate<Dictionary<string, GameObject>, object>();
		delegateManager.RegisterMethodDelegate<GameEffect, object>();
		delegateManager.RegisterMethodDelegate<Texture2D, object>();
		delegateManager.RegisterMethodDelegate<PlayerState, bool, string>();

		delegateManager.RegisterDelegateConvertor<TextureAnimCallBack>((action) =>
		{
			return new TextureAnimCallBack((window, isBreak) => { ((Action<IUIAnimation, bool>)action)(window, isBreak); });
		});
		delegateManager.RegisterDelegateConvertor<KeyFrameCallback>((action) =>
		{
			return new KeyFrameCallback((component, breakTremling) => { ((Action<ComponentKeyFrameBase, bool>)action)(component, breakTremling); });
		});
		delegateManager.RegisterDelegateConvertor<LerpCallback>((action) =>
		{
			return new LerpCallback((component, breakLerp) => { ((Action<ComponentLerp, bool>)action)(component, breakLerp); });
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
			return new AssetLoadDoneCallback((asset, assets, bytes, userData, loadPath) => { ((Action<UnityEngine.Object, UnityEngine.Object[], byte[], object[], string>)action)(asset, assets, bytes, userData, loadPath); });
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
			return new TrackCallback((component, breakTrack) => { ((Action<ComponentTrackTargetBase, bool>)action)(component, breakTrack); });
		});
		delegateManager.RegisterDelegateConvertor<OnReceiveDragCallback>((action) =>
		{
			return new OnReceiveDragCallback((IMouseEventCollect dragObj, ref bool continueEvent) =>
			{
				((Action<IMouseEventCollect, bool>)action)(dragObj, continueEvent);
			});
		});
		delegateManager.RegisterDelegateConvertor<OnDragHoverCallback>((action) =>
		{
			return new OnDragHoverCallback((dragObj, hover) => { ((Action<IMouseEventCollect, bool>)action)(dragObj, hover); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseEnter>((action) =>
		{
			return new OnMouseEnter((obj) => { ((Action<IMouseEventCollect>)action)(obj); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseLeave>((action) =>
		{
			return new OnMouseLeave((obj) => { ((Action<IMouseEventCollect>)action)(obj); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseDown>((action) =>
		{
			return new OnMouseDown((mousePos) => { ((Action<Vector2>)action)(mousePos); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseUp>((action) =>
		{
			return new OnMouseUp((mousePos) => { ((Action<Vector2>)action)(mousePos); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseMove>((action) =>
		{
			return new OnMouseMove((ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime) => { ((Action<Vector3, Vector3, float>)action)(mousePos, moveDelta, moveTime); });
		});
		delegateManager.RegisterDelegateConvertor<OnMouseStay>((action) =>
		{
			return new OnMouseStay((mousePos) => { ((Action<Vector2>)action)(mousePos); });
		});
		delegateManager.RegisterDelegateConvertor<OnScreenMouseUp>((action) =>
		{
			return new OnScreenMouseUp((obj, mousePos) => { ((Action<IMouseEventCollect, Vector2>)action)(obj, mousePos); });
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
		delegateManager.RegisterDelegateConvertor<OnNGUILineGenerated>((action) =>
		{
			return new OnNGUILineGenerated((line) => { ((Action<NGUILine>)action)(line); });
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
			return new OnDragViewStartCallback((ref bool allowDrag) => { ((Action<bool>)action)(allowDrag); });
		});
		delegateManager.RegisterDelegateConvertor<MyThreadCallback>((action) =>
		{
			return new MyThreadCallback((ref bool allowDrag) => { ((Action<bool>)action)(allowDrag); });
		});
		delegateManager.RegisterDelegateConvertor<onPlayingCallback>((action) =>
		{
			return new onPlayingCallback((control, frame, isPlaying) => { ((Action<AnimControl, int, bool>)action)(control, frame, isPlaying); });
		});
		delegateManager.RegisterDelegateConvertor<onPlayEndCallback>((action) =>
		{
			return new onPlayEndCallback((control, callback, isBreak) => { ((Action<AnimControl, bool, bool>)action)(control, callback, isBreak); });
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
			return new OnDragStartCallback((ComponentOwner dragObj, ref bool allowDrag) => { ((Action<ComponentOwner, bool>)action)(dragObj, allowDrag); });
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
	}
	public static void collectCrossInheritClass(List<Type> classList)
	{
		// 收集所有需要生成适配器的类
		classList.Add(typeof(FrameBase));
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
	}
}
#endif